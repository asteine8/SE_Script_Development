
IMyTextPanel LCD;
IMyRemoteControl ReferenceRemote;

List<IMyGyro> Gyros = new List<IMyGyro>();
List<IMyThrust> Thrusters = new List<IMyThrust>();
List<IMyCameraBlock> Cameras = new List<IMyCameraBlock>();


Stack<Vector3D> waypointPath = new Stack<Vector3D>();
MyDetectedEntityInfo LastRaycastResult;

Quaternion CurrentOrientation;

double gyroGain = 2;
double thrusterGain = 15;
double maxSpeed = 125;
double thresholdForSlow = 500;
double thresholdForStop = 12;
bool pointAtTarget = true;

double MaxRaycastRange = 1000; // In meters

int obstacleRaycastCounter = 0;
int ticksPerObstacleRaycast = 2;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz
    GetBlocks();
    waypointPath.Push(ReferenceRemote.GetPosition()); // Initiate path at current position (So we don't go anywhere)
}

void Main(string arg) {
    if (arg.Equals("Reset")) { // Allow for updates to the block system (Adding more)
        GetBlocks();
    }
    if (arg.Equals("RandomMove")) {
        waypointPath.Push(ReferenceRemote.GetPosition() + new Vector3D(500,-500,500));
    }

    if (obstacleRaycastCounter >= ticksPerObstacleRaycast) { // Do raycast stuff and calculate alternate pathing if nessisary
        obstacleRaycastCounter = 0;
        LastRaycastResult = RaycastForObstacles(Cameras, ReferenceRemote, MaxRaycastRange);
        if (!LastRaycastResult.IsEmpty()) { // Do something about it because we don't want to hit anything
            Vector3D RemotePos = ReferenceRemote.GetPosition();
            // Vector3D HitPos = LastRaycastResult.HitPosition.Value;
            // Vector3D ObjectPos = LastRaycastResult.Position;

            Vector3D dodgeDir = Vector3D.Normalize(Vector3D.Cross(
                                            LastRaycastResult.HitPosition.Value - RemotePos, LastRaycastResult.Position - RemotePos));

            dodgeDir *= LastRaycastResult.BoundingBox.Size.Length(); // Make sure we put the dodging waypoint outside the bounding box (This puts it 2x box max radius from center)
            waypointPath.Push(dodgeDir + RemotePos); // Push the new waypoint into the stack
        }
    }
    else obstacleRaycastCounter++;

    double dFromTarget = ThrustToTarget(ReferenceRemote, Thrusters, waypointPath.Peek(), maxSpeed, thresholdForSlow, thresholdForStop, thrusterGain);
    if (dFromTarget < thresholdForStop && ReferenceRemote.GetShipVelocities().LinearVelocity.Length() < 0.1) {
        if (waypointPath.Count > 1) {
            Echo("Reached waypoint at:\n" + waypointPath.Pop().ToString("0"));
        }
        else { // End of pathing, assign current position to end to prevent stack errors
            waypointPath.Push(ReferenceRemote.GetPosition());
        }
        
    }

    if (pointAtTarget && dFromTarget > thresholdForStop) {
        CurrentOrientation = Quaternion.CreateFromTwoVectors(Vector3D.Forward, waypointPath.Peek() - ReferenceRemote.GetPosition());
    }

    TurnToQuaternion(CurrentOrientation, Gyros, ReferenceRemote, gyroGain, gyroGain/2, gyroGain);

    string output = "";

    output += dFromTarget.ToString("0.00") + "\n";

    LCD.WritePublicText(output);
}

void GetBlocks() {
    GridTerminalSystem.GetBlocksOfType(Gyros);
    GridTerminalSystem.GetBlocksOfType(Thrusters);
    GridTerminalSystem.GetBlocksOfType(Cameras);

    LCD = GridTerminalSystem.GetBlockWithName("Autopilot LCD") as IMyTextPanel;
    ReferenceRemote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
}

MyDetectedEntityInfo RaycastForObstacles(List<IMyCameraBlock> Cameras, IMyRemoteControl RefRemote, double raycastDistance) {
    Vector3D RefForward = RefRemote.WorldMatrix.Forward;
    float maxRaycastAngle = Cameras[0].RaycastConeLimit;


    for (int i = 0; i < Cameras.Count; i++) {

        if (Cameras[i].CanScan(raycastDistance)) {
            // We are go for raycast, initiate raycast now
            Cameras[i].EnableRaycast = true;

            Vector3D RaycastTarget = Vector3D.Normalize(RefRemote.GetShipVelocities().LinearVelocity) * raycastDistance;
            RaycastTarget += RefRemote.GetPosition();

            Echo("Raycasting");

            return Cameras[i].Raycast(RaycastTarget); // Return MyDetectedEntityInfo
        }
    }
    return new MyDetectedEntityInfo();
}

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

        //Applies To Scenario
        Gyros[i].Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y) * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z) * RollGain), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        // Gyros[i].GyroOverride = true;
    }
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

double ThrustToTarget(IMyRemoteControl RefRemote, List<IMyThrust> thrusters, Vector3D target, double maxSpeed, double thresholdForSlow, double thresholdForStop, double gain) {
    Vector3D shipVelocity = RefRemote.GetShipVelocities().LinearVelocity;
    double distanceFromTarget = (target - RefRemote.GetPosition()).Length();

    Vector3D targetVelocity = target - RefRemote.GetPosition();

    if (distanceFromTarget < thresholdForStop) {
        targetVelocity = Vector3D.Zero;
    }

    Vector3D thrustDirection = targetVelocity - shipVelocity; // Gets direction to thrust towards

    Vector3D GridRelativeVelocity;
    GridRelativeVelocity.X =  Vector3D.Dot(RefRemote.WorldMatrix.Forward, shipVelocity) / RefRemote.WorldMatrix.Forward.Length();
    GridRelativeVelocity.Y = Vector3D.Dot(RefRemote.WorldMatrix.Right, shipVelocity) / RefRemote.WorldMatrix.Right.Length();
    GridRelativeVelocity.Z = Vector3D.Dot(RefRemote.WorldMatrix.Up, shipVelocity) / RefRemote.WorldMatrix.Up.Length();

    Vector3D GridRelativeTargetVelocity;
    if (thrustDirection.Length() > 0) {
        GridRelativeTargetVelocity.X = Vector3D.Dot(RefRemote.WorldMatrix.Forward, thrustDirection) / RefRemote.WorldMatrix.Forward.Length();
        GridRelativeTargetVelocity.Y = Vector3D.Dot(RefRemote.WorldMatrix.Right, thrustDirection) / RefRemote.WorldMatrix.Right.Length();
        GridRelativeTargetVelocity.Z = Vector3D.Dot(RefRemote.WorldMatrix.Up, thrustDirection) / RefRemote.WorldMatrix.Up.Length();
        GridRelativeTargetVelocity = Vector3D.Normalize(GridRelativeTargetVelocity);
    }
    else {
        GridRelativeTargetVelocity = Vector3D.Zero;
    }

    if (distanceFromTarget < thresholdForSlow) {
        GridRelativeTargetVelocity *= maxSpeed * (distanceFromTarget/thresholdForSlow); // Apply thrust on a gradient in the slowdown threshold
    }
    else { // Outside of all thresholds, bring up to max speed
        GridRelativeTargetVelocity *= maxSpeed;
    }

    Vector3D GridRelativeAppliedThrust = (GridRelativeTargetVelocity - GridRelativeVelocity) / (maxSpeed/gain);


    // string output = "";
    // output += GridRelativeTargetVelocity.Length().ToString("0.000") + "\n";
    // output += targetVelocity.ToString("0.00") + "\n";
    // output += GridRelativeTargetVelocity.ToString("0.00") + "\n";
    // output += GridRelativeVelocity.ToString("0.0") + "\n";
    // output += distanceFromTarget.ToString("0.0");

    // LCD.WritePublicText(output);

    for (int i = 0; i < thrusters.Count; i++) {
        if (thrusters[i].Orientation.Forward == Base6Directions.GetOppositeDirection(RefRemote.Orientation.Forward)) { // Forward Thrusters (Thrust ship forwards)
            // Thrust for positive 
            if (GridRelativeAppliedThrust.X > 0) {
                thrusters[i].ThrustOverridePercentage = (float)(GridRelativeAppliedThrust.X);
            }
            else thrusters[i].ThrustOverridePercentage = 0f;
        }
        else if (thrusters[i].Orientation.Forward == RefRemote.Orientation.Forward) { // Backward Thrusters (Thrust ship backwards)
            // Thrust for negative 
            if (GridRelativeAppliedThrust.X < 0) {
                thrusters[i].ThrustOverridePercentage = (float)(-GridRelativeAppliedThrust.X);
            }
            else thrusters[i].ThrustOverridePercentage = 0f;
        }

        else if (thrusters[i].Orientation.Forward == RefRemote.Orientation.Left) { // Right Thrusters  (Thrust ship right)
            // Thrust for positive 
            if (GridRelativeAppliedThrust.Y > 0) {
                thrusters[i].ThrustOverridePercentage = (float)(GridRelativeAppliedThrust.Y);
            }
            else thrusters[i].ThrustOverridePercentage = 0f;
        }
        else if (thrusters[i].Orientation.Forward == Base6Directions.GetOppositeDirection(RefRemote.Orientation.Left)) { // Left Thrusters (Thrust ship left)
            // Thrust for negative 
            if (GridRelativeAppliedThrust.Y < 0) {
                thrusters[i].ThrustOverridePercentage = (float)(-GridRelativeAppliedThrust.Y);
            }
            else thrusters[i].ThrustOverridePercentage = 0f;
        }

        else if (thrusters[i].Orientation.Forward == Base6Directions.GetOppositeDirection(RefRemote.Orientation.Up)) { // Up Thrusters  (Thrust ship up)
            // Thrust for positive 
            if (GridRelativeAppliedThrust.Z > 0) {
                thrusters[i].ThrustOverridePercentage = (float)(GridRelativeAppliedThrust.Z);
            }
            else thrusters[i].ThrustOverridePercentage = 0f;
        }
        else if (thrusters[i].Orientation.Forward == RefRemote.Orientation.Up) { // Down Thrusters (Thrust ship down)
            // Thrust for negative 
            if (GridRelativeAppliedThrust.Z < 0) {
                thrusters[i].ThrustOverridePercentage = (float)(-GridRelativeAppliedThrust.Z);
            }
            else thrusters[i].ThrustOverridePercentage = 0f;
        }
        
    }

    return distanceFromTarget;
}