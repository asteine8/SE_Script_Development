MyWheelControlSystem WheelControlSystem;

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
}

class MyWheelControlSystem {
    public List<WheelSuspension> WheelSuspensions;
    public IMyShipController SystemController;

    private Vector3I ReferenceLocation; // Center of Grid bounding box
    private Vector3I ReferenceDirection; // Direction of ship controller

    public MyWheelControlSystem(List<IMyMotorSuspension> WheelSusps, IMyShipController Controller) {
        // Get Reference Locations
        ReferenceLocation = (Controller.CubeGrid.Min - Controller.CubeGrid.Max) / 2;
        ReferenceDirection = Base6Directions.GetIntVector(Controller.Orientation.Forward);

        // Register Wheel Suspensions
        WheelSuspensions = new List<WheelSuspension>();
        RegisterSusps(WheelSusps);

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
}

public Program() {
    float steeringOverride = 0.75F;
    float propulsionOverride = 0.02F;

    List<IMyMotorSuspension> s = new List<IMyMotorSuspension>();
    GridTerminalSystem.GetBlocksOfType(s);

    IMyShipController remote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyShipController;
    WheelControlSystem = new MyWheelControlSystem(s, remote);

    WheelControlSystem.SetSystemSteering(steeringOverride);
    WheelControlSystem.SetSystemPropulsion(propulsionOverride);
}

void Main(string arg) {
    
}

void RegisterBlocks() {

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
}

#endregion