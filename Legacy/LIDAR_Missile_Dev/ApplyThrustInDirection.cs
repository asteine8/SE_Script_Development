
IMyTextPanel LCD;
IMyRemoteControl referenceRemote;
List<IMyThrust> thrusters = new List<IMyThrust>();
Vector3D targetPos = new Vector3D(0,0,0);

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz

    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
    referenceRemote = GridTerminalSystem.GetBlockWithName("Remote") as IMyRemoteControl;

    GridTerminalSystem.GetBlocksOfType(thrusters); // Get all thrusters on grid

    targetPos = referenceRemote.GetPosition();
    // targetPos = Vector3D.Zero;
}

void Main(string argument) {
    // if (argument.Equals("Reset")) {
    //     targetPos = referenceRemote.GetPosition();
    // }
    ThrustToTarget(referenceRemote, thrusters, targetPos, 100, 350, 10, 20);
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

