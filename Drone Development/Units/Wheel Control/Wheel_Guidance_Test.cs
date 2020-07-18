MyWheelControlSystem WheelControlSystem;
IMyTextPanel LCD;

class PIDecayControler {
    public float PrevIError;
    public float iterationTime = 0.167F; // Seconds

    public float pTerm;
    public float iTerm;

    public float Decay_Term;

    public float pComponent;
    public float iComponent;

    public PIDecayControler(float p, float i, float decay) {
        pTerm = p;
        iTerm = i;
        Decay_Term = decay;

        PrevIError = 0F;
    }

    public float CalcResponse(float currentError) {
        // Proportional Component:
        pComponent = currentError * pTerm;

        // Integral Component:
        float iError = PrevIError + (currentError * iterationTime);
        PrevIError = Decay_Term * iError;
        iComponent = iError * iTerm;

        return pComponent + iComponent;
    }
}

class WheelSuspension {
    public float propulsionInversion; // +/- 1

    public float steeringInversion; // +/- 1
    public float maxSteeringAngle; // radians

    public IMyMotorSuspension SuspBlock;
    
    public WheelSuspension(IMyMotorSuspension s) {
        propulsionInversion = 1F;
        if (!s.BlockDefinition.SubtypeId.Contains("mirrored")) {
            propulsionInversion = -1F;
        }
        else if (s.InvertPropulsion) {
            propulsionInversion = -1F;
        }

        maxSteeringAngle = s.MaxSteerAngle;
        steeringInversion = 1F;
        
        SuspBlock = s;
    }

    public WheelSuspension(IMyMotorSuspension s, Vector3I Grid_Center, Vector3I Grid_Forward_Direction) {
        propulsionInversion = 1F;
        if (!s.BlockDefinition.SubtypeId.Contains("mirrored")) {
            propulsionInversion = -1F;
        }
        else if (s.InvertPropulsion) {
            propulsionInversion = -1F;
        }

        maxSteeringAngle = s.MaxSteerAngle;

        // Check if wheel is on front or back (inverts if on the back)
        steeringInversion = HelperFunctions.pointInFrontOfReference(s.Position, Grid_Center, Grid_Forward_Direction) ? 1F : -1F;
        
        SuspBlock = s;
    }

    public void SetPropulsionOverride(float percent) {
        SuspBlock.SetValue("Propulsion override", percent * propulsionInversion);
    }

    public void SetSteeringOverride(float percent) {
        SuspBlock.SetValue("Steer override", percent * steeringInversion);
    }

    public void SetSteeringOverrideRadians(float radians) {
        SuspBlock.SetValue("Steer override", radians * steeringInversion / maxSteeringAngle);
    }

    public void SetParkingBrake(bool brakeOn) {
        SuspBlock.Brake = brakeOn;
    }
}

class MyWheelControlSystem {
    public List<WheelSuspension> WheelSuspensions;
    public IMyShipController SystemController;

    private Vector3I ReferenceLocation; // Center of Grid bounding box
    private Vector3I ReferenceDirection; // Direction of ship controller

    private Vector3D ReferenceForward;
    private Vector3D ReferenceRight;

    public bool onPlanet;
    public Vector3D planetPosition;

    public float Location_Tolerance = 5F; // Min distance before not worrying about propulsion anymore

    public MyWheelControlSystem(List<IMyMotorSuspension> WheelSusps, IMyShipController Controller) {
        // Get Reference Locations
        ReferenceLocation = (Controller.CubeGrid.Min - Controller.CubeGrid.Max) / 2;
        ReferenceDirection = Base6Directions.GetIntVector(Controller.Orientation.Forward);

        // Register Wheel Suspensions
        WheelSuspensions = new List<WheelSuspension>();
        RegisterSusps(WheelSusps);

        // Register current planet
        onPlanet = Controller.TryGetPlanetPosition(out planetPosition);

        SystemController = Controller;
    }

    public void RegisterSusps(List<IMyMotorSuspension> WheelSusps) {
        foreach(IMyMotorSuspension WheelSusp in WheelSusps) {
            if (WheelSusp.IsFunctional)
                WheelSuspensions.Add(new WheelSuspension(WheelSusp, ReferenceLocation, ReferenceDirection));
        }
    }
    
    public void SetSystemSteering(float percent) {
        foreach(WheelSuspension WheelSusp in WheelSuspensions) {
            WheelSusp.SetSteeringOverride(percent);
        }
    }

    public void SetSystemPropulsion(float percent) {
        foreach(WheelSuspension WheelSusp in WheelSuspensions) {
            WheelSusp.SetPropulsionOverride(percent);
        }
    }

    public void SetParkingBrake(bool brakeOn) {
        SystemController.HandBrake = brakeOn;
    }

    // Returns true if already at point within tolerance
    public string MoveToLocation(Vector3D location) {
        // Get Reference Directions
        ReferenceForward = SystemController.WorldMatrix.Forward;
        ReferenceRight = SystemController.WorldMatrix.Right;

        Vector3D targetLocation;
        Vector3D currentLocation = SystemController.GetPosition();

        // If on a planet, get closest point to line between target location and planet center
        if (onPlanet) {
            targetLocation = HelperFunctions.getClosestPointOnLine(location, location - planetPosition, currentLocation);
        }
        else { // Otherwise, just go there I guess
            targetLocation = location;
        }

        Vector3D vectorToTarget = targetLocation - currentLocation;

        // Do Propulsion (invert if behind)
        float distanceToTarget = (float)vectorToTarget.Length();
        if (distanceToTarget < Location_Tolerance) {
            distanceToTarget = 0; // Set distance to 0 if in threshold
            SetParkingBrake(true);
        }
        else {
            SetParkingBrake(false);
        }


        if (HelperFunctions.pointInFrontOfReference(targetLocation, currentLocation, ReferenceForward)) {
            distanceToTarget *= -1;
        }

        // Do Steering (invert if to the left)
        float angleToTarget = (float)Math.Acos( Vector3D.Dot(ReferenceForward, vectorToTarget) / (ReferenceForward.Length() * vectorToTarget.Length()));
        if (!HelperFunctions.pointInFrontOfReference(targetLocation, currentLocation, ReferenceRight)) {
            angleToTarget *= -1;
        }

        string output = "";
        output += "angle: " + angleToTarget.ToString("0.00") + "\n";
        output += "distance: " + distanceToTarget.ToString("0.00") + "\n";
        output += "target: " + targetLocation.ToString("0") + "\n";
        output += "V to target: " + vectorToTarget.ToString("0") + "\n";
        output += "Ref Forward: " + ReferenceForward.ToString("0.00") + "\n";

        // Test
        SetSystemSteering(angleToTarget);
        SetSystemPropulsion(distanceToTarget/100);
        // SetSystemSteering(0);
        // SetSystemPropulsion(0);

        return output;
    }
}

public Program() {

    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Set update frequency to every tick

    List<IMyMotorSuspension> s = new List<IMyMotorSuspension>();
    GridTerminalSystem.GetBlocksOfType(s);

    IMyShipController remote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyShipController;
    WheelControlSystem = new MyWheelControlSystem(s, remote);


}

void Main(string arg) {
    string output = WheelControlSystem.MoveToLocation(new Vector3D(-11260.34, -5334.46, 58922.87));

    (GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel).WriteText(output);
}

#region Helper Functions

class HelperFunctions {
    // Returns true if point is infront of the reference location with the forward reference. Uses a dot product technique
    public static bool pointInFrontOfReference(Vector3I point, Vector3I reference_location, Vector3I reference_forward) {
        return HelperFunctions.pointInFrontOfReference(new Vector3D(point), new Vector3D(reference_location), new Vector3D(reference_forward));
    }

    public static bool pointInFrontOfReference(Vector3D point, Vector3D reference_location, Vector3D reference_forward) {
        Vector3D vectorToReference = reference_location - point;

        double angleToReferenceForward = Math.Acos(Vector3D.Dot(vectorToReference, reference_forward) / (vectorToReference.Length() * reference_forward.Length()) );
        return angleToReferenceForward < (Math.PI/2);
    }

    public static Vector3D getClosestPointOnLine(Vector3D pointOnLine, Vector3D lineDirection, Vector3D currentPoint) {
        return pointOnLine + lineDirection * Vector3D.Dot(currentPoint - pointOnLine, lineDirection) / (lineDirection.Length()*lineDirection.Length());
    }
}

#endregion