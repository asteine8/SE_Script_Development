
// User Defined Variables

double MiningBoxScalar = 0.9; // Percentage of how big the ships mining footprint is calculated from max


// Block Definitions

List<IMyShipDrill> Drills = new List<IMyShipDrill>();
List<IMyGyro> Gyros = new List<IMyGyro>();
List<IMyThrust> Thrusters = new List<IMyThrust>();
List<IMyConveyorSorter> Sorters = new List<IMyConveyorSorter>();
List<IMyBeacon> Beacons = new List<IMyBeacon>();


IMyRemoteControl Remote;
IMyProgrammableBlock ProgBlock;
IMyBeacon Beacon;


// Internal Variables

/*
    States:
        0: Idle, do nothing
        1: Initialize Program, calculate pathing and automaticly move to state 2
        2: Move to starting position
        3: Move along path
        4: Back up to path start // May be not required
        5: Switch Paths
        6: Digging complete
        -1: Pause Digging. Set velocity to 0 and stay at current point. Maintain orientation and switch off drills
 */
int ProgramState = 0;

Vector2D MiningBox = new Vector2D();
List<LineD> MiningPaths = new List<LineD>();

List<double> MaxDirectionalThrust = new List<double>(); // [Forward, Backward, Left, Right, Up, Down]

// Ship Variables

Vector3D TargetPosition;
Vector3D TargetVelocity;
Quaternion TargetOrientation;


public Program() {

    // Initialize Blocks on Grid
    GridTerminalSystem.GetBlocksOfType(Drills);
    GridTerminalSystem.GetBlocksOfType(Gyros);
    GridTerminalSystem.GetBlocksOfType(Thrusters);
    GridTerminalSystem.GetBlocksOfType(Sorters);
    GridTerminalSystem.GetBlocksOfType(Beacons);

    Remote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
    ProgBlock = Me;
    Beacon = Beacons[0];

    Echo("Initializied");
}

void Main(string argument) {
    // User Input Block
    if (argument.ToLower() == "reset") ProgramState = 6;
    else if (argument.ToLower() == "dig") ProgramState = 1;
    
    // User Input End

    switch (ProgramState) {
        case 0: // Idle, do nothing
            if (argument.ToLower() == "dig") {
                Beacon.CustomData = "Initializing Dig";
                Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz since we're doing something now
                ProgramState = 1;
            }
            break;
        case 1: // Initialize Program, calculate pathing and automaticly move to state 2
            Beacon.CustomData = "Initializing Dig";
            Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz since we're doing something now

            MiningBox = GetShipMiningBox(Remote); // Get bounding box to calculate pathing
            MiningPaths = CalculateMiningPaths(Remote, MiningBox);
            TargetOrientation = GetPathOrientation(Remote, MiningPaths);

            MaxDirectionalThrust = GetMaxDirectionalThrust(Remote, Thrusters);

            for (int i = 0; i < Gyros.Count; i++) { // Initialize Gyro Overrides
                Gyros[i].GyroOverride = true;
            }
            ProgramState = 2; // We have our paths and are good to go. Move to starting point
            break;
        case 2: // Move To Starting Position
            TurnToQuaternion(TargetOrientation);
            break;
        case 6: // Digging Complete. Reset things, release control, and goto idle
            for (int i = 0; i < Gyros.Count; i++) { // Turn Off Gyro Overrides
                Gyros[i].GyroOverride = true;
            }
            break;
    }
}

// Calculation Functions

Vector2D GetShipMiningBox(IMyRemoteControl ReferenceRemote) {
    IMyCubeGrid Grid = ReferenceRemote.CubeGrid;
    Vector3D BoundingBox = Grid.GridIntegerToWorld(Grid.Max) - Grid.GridIntegerToWorld(Grid.Min);

    double XDimension = Math.Abs(Vector3D.Dot(Vector3D.Normalize(ReferenceRemote.WorldMatrix.Right), BoundingBox)); // Project onto reference vectors
    double YDimension = Math.Abs(Vector3D.Dot(Vector3D.Normalize(ReferenceRemote.WorldMatrix.Up), BoundingBox));
    Vector2D MiningBox = new Vector2D(Math.Round(XDimension, 2), Math.Round(YDimension, 2));
    MiningBox *= MiningBoxScalar; // Mining box at 90%

    return MiningBox;
}

List<LineD> CalculateMiningPaths(IMyRemoteControl ReferenceRemote, Vector2D shipMiningBox) {
    // Parse dimensions From Programmable Block custom data 

    List<LineD> Paths = new List<LineD>();

    List<Vector3D> UserPoints = new List<Vector3D>();

    // GPS:Mining point 1:43841.17:225650.13:5801813.62: <= Line Point 1
    // GPS:Mining point 2:43846.98:225648.98:5801817.96: <= Line Point 2
    // GPS:Mining Point 3:43840.69:225631.93:5801812.36: <= Expands outwards from line
    // GPS:Mining End:43856.92:225629.92:5801790.96: <= Distance from plane determiness direction and depth of hole

    string[] configLines = Me.CustomData.Trim().Split('\n');

    for (int i = 0; i < 4; i++) { // Get Pathing Config Data
        string[] dataElements = configLines[i].Split(':');
        UserPoints.Add(new Vector3D(Double.Parse(dataElements[2]), Double.Parse(dataElements[3]), Double.Parse(dataElements[4])));
        // Echo(UserPoints[i].ToString());
    }

    Vector3D PlaneNormal = Vector3D.Normalize(Vector3D.Cross(UserPoints[1] - UserPoints[0], UserPoints[2] - UserPoints[0])); // Normal vector to plane
    Vector3D BaseVector = UserPoints[1] - UserPoints[0]; // Base Vector for rectangle -> point 1 to 2
    double boxWidth = (UserPoints[1] - UserPoints[0]).Length();
    Vector3D heightDir = Vector3D.Normalize(Vector3D.Cross(BaseVector, PlaneNormal)); // Normal vector on place but perpendicular to the base

    double boxHeight = Vector3D.Dot(UserPoints[2] - UserPoints[0], heightDir);
    double boxDepth = Vector3D.Dot(UserPoints[3] - UserPoints[0], PlaneNormal);

    Vector3D BasePathingVector = PlaneNormal * boxDepth; // Vector from Plane to destination 
    
    // Check if BasePathingVector is in same direction of mining end point
    double angle = Math.Acos(Vector3D.Dot(UserPoints[3]-UserPoints[0], BasePathingVector) / ((UserPoints[3]-UserPoints[0]).Length() * boxDepth));
    if (angle > Math.PI) { // Vector is pointing in the wrong direction, flip it...
        BasePathingVector *= -1;
    }

    // Determine rotation of mining footprint
    Vector3D GravityVector = Vector3D.Normalize(ReferenceRemote.GetNaturalGravity());
    double gravityToNormAngle = (ReferenceRemote.GetNaturalGravity().Length() < 0.02) ? 0 : Math.Acos(Vector3D.Dot(GravityVector,PlaneNormal));

    Vector2D PlanarFootprintRelativeToGrav; // Aligned with mining box

    if (ReferenceRemote.GetNaturalGravity().Length() < 0.02 || gravityToNormAngle < 0.1745) { // No gravity or insignificant angle, no extra math required
        PlanarFootprintRelativeToGrav = new Vector2D(boxWidth, boxHeight);
    }
    else { // There is a significant angle between the gravity and plane vectors. We should determine which orientation of the mining and planar footprints to use
        // Assign X values for planar footprint to vector with least slope relative to the gravity vector
        double widthToGravAngle = Math.Acos(Vector3D.Dot(BaseVector,GravityVector)/BaseVector.Length());
        double heightToGravAngle = Math.Acos(Vector3D.Dot(heightDir, GravityVector));
        if (Math.Abs(widthToGravAngle-Math.PI) < Math.Abs(heightToGravAngle-Math.PI)) { // Width (Base Vector) is on bottom
            PlanarFootprintRelativeToGrav = new Vector2D(boxWidth, boxHeight);
        }
        else { // Height Vector is on top
            PlanarFootprintRelativeToGrav = new Vector2D(boxHeight, boxWidth);
        }
    }

    // Generate paths using points 0-1 as x direction and heightDir as y direction

    int NumXSteps = Convert.ToInt32(Math.Ceiling(Math.Abs(PlanarFootprintRelativeToGrav.X/shipMiningBox.X))); // Round up
    int NumYSteps = Convert.ToInt32(Math.Ceiling(Math.Abs(PlanarFootprintRelativeToGrav.Y/shipMiningBox.Y)));

    double xStepSize = (PlanarFootprintRelativeToGrav.X - 2*shipMiningBox.X) / (double)NumXSteps;
    double yStepSize = (PlanarFootprintRelativeToGrav.Y - 2*shipMiningBox.Y) / (double)NumYSteps;

    Echo(NumXSteps.ToString() + "|" + NumYSteps.ToString());

    for (int y = 1; y <= NumYSteps; y++) { // [1,NumYSteps]
        for (int x = 1; x <= NumXSteps; x++) { // [1,NumXSteps]
            Vector3D StartPoint = UserPoints[0];
            LineD path = new LineD();
            
            // Add scaled up unit vectors
            StartPoint += (xStepSize*x)*Vector3D.Normalize(BaseVector);
            StartPoint += (yStepSize*y)*Vector3D.Normalize(heightDir);

            path.From = StartPoint;
            path.To = StartPoint + BasePathingVector;

            Paths.Add(path);
        }
    }

    return Paths;
}

List<double> GetMaxDirectionalThrust(IMyRemoteControl ReferenceRemote, List<IMyThrust> Thrusters) {
    List<double> DirectionalThrust = new List<double>(); // [Forward, Backward, Left, Right, Up, Down]

    Base6Directions.Direction RefForward = ReferenceRemote.Orientation.Forward;
    Base6Directions.Direction RefUp = ReferenceRemote.Orientation.Up;
    Base6Directions.Direction RefLeft = ReferenceRemote.Orientation.Left;

    for (int i = 0; i < Thrusters.Count; i++) {
        Base6Directions.Direction thusterOrientation = Thrusters[i].Orientation.Forward;
        switch (thusterOrientation) {
            case Base6Directions.GetOppositeDirection(RefForward): // Forward Thrust
                DirectionalThrust[0] += Thrusters[i].MaxEffectiveThrust;
                break;
            case RefForward: // Backward Thrust
                DirectionalThrust[1] += Thrusters[i].MaxEffectiveThrust;
                break;
            case Base6Directions.GetOppositeDirection(RefLeft): // Left Thrust
                DirectionalThrust[2] += Thrusters[i].MaxEffectiveThrust;
                break;
            case RefLeft: // Right Thrust
                DirectionalThrust[3] += Thrusters[i].MaxEffectiveThrust;
                break;
            case Base6Directions.GetOppositeDirection(RefUp): // Up Thrust
                DirectionalThrust[4] += Thrusters[i].MaxEffectiveThrust;
                break;
            case RefUp: // Down Thrust
                DirectionalThrust[5] += Thrusters[i].MaxEffectiveThrust;
                break;
        }
    }
    return DirectionalThrust;
}

Quaternion GetPathOrientation(IMyRemoteControl ReferenceRemote, List<LineD> Paths) {
    Vector3D GravityVector = ReferenceRemote.GetNaturalGravity();
    Vector3D Forward = Vector3D.Normalize(Paths[0].To - Paths[0].From);
    Vector3D Right = Vector3D.Normalize(Vector3D.Cross(GravityVector, Forward));
    Vector3D Up = Vector3D.Normalize(Vector3D.Cross(Right, Forward));
    return Quaternion.CreateFromForwardUp(new Vector3(Forward), new Vector3(Up));
}

Vector3 QuaternionToYawPitchRoll(Quaternion quat) {
    double q0 = (double)quat.W;
    double q1 = (double)quat.X;
    double q2 = (double)quat.Y;
    double q3 = (double)quat.Z;

    double Roll = Math.Atan2(2*(q0*q1+q2*q3), 1 - 2*(q1*q1+q2*q2));
    double Pitch = Math.Asin(2*(q0*q2-q3*q1));
    double Yaw = Math.Atan2(2*(q0*q3+q1*q2), 1 - 2*(q2*q2+q3*q3));

    return new Vector3((float)Yaw, (float)Pitch, (float)Roll);
}

// Ship Control Functions

void TurnToQuaternion(Quaternion TargetOrientation, List<IMyGyro> Gyros, IMyRemoteControl REF_RC, double GAIN, double RollGain, double MAXANGULARVELOCITY) {
    //Ensures Autopilot Not Functional
    REF_RC.SetAutoPilotEnabled(false);

    //Detect Forward, Up & Pos
    Vector3D ShipForward = REF_RC.WorldMatrix.Forward;
    Vector3D ShipUp = REF_RC.WorldMatrix.Up;
    Vector3D ShipPos = REF_RC.GetPosition();
    Quaternion Quat_Two = Quaternion.CreateFromForwardUp(ShipForward, ShipUp);
    var InvQuat = Quaternion.Inverse(Quat_Two);

    double ROLLANGLE = QuaternionToYawPitchRoll(Quaternion.Inverse(Quat_Two) * TargetOrientation).X; // Get roll angle to target quaternion

    //Create And Use Inverse Quatinion                   
    Vector3D DirectionVector = Vector3D.Transform(Vector3D.Forward, TargetOrientation); // Modified to use quaternion orientation
    Vector3D RCReferenceFrameVector = Vector3D.Transform(DirectionVector, InvQuat); //Target Vector In Terms Of RC Block

    //Convert To Local Azimuth And Elevation
    double ShipForwardAzimuth = 0; double ShipForwardElevation = 0;
    Vector3D.GetAzimuthAndElevation(RCReferenceFrameVector, out ShipForwardAzimuth, out ShipForwardElevation);

    for (int i = 0; i < Gyros.Count; i++) {
        //Does Some Rotations To Provide For any Gyro-Orientation
        var RC_Matrix = REF_RC.WorldMatrix.GetOrientation();
        var Vector = Vector3.Transform((new Vector3D(ShipForwardElevation, ShipForwardAzimuth, ROLLANGLE)), RC_Matrix); //Converts To World
        var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(Gyros[i].WorldMatrix.GetOrientation()));  //Converts To Gyro Local

        // Applies To Scenario
        Gyros[i].Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y) * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z) * RollGain), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        // Gyros[i].GyroOverride = true;
    }
}

void SetDrillState(List<IMyShipDrill> drills, bool state) {
    string stateStr = (state) ? "OnOff_On" : "OnOff_Off";
    for (int i = 0; i < drills.Count; i++) {
        drills[i].GetActionWithName(stateStr).Apply(drills[i]);
    }
}