
double MaxSpeed = 350; // Should be the max speed of the server (350m/s in this case for small grid)
double TargetSpeedOvershoot = 100; // Adjusts how much the missile will turn to adjust velocity
double OrientationWaypointDistance = 200; // Adjusts how far the remote control waypoint for orientation is from the missile
float RotationSpeedMutiplier = 5f; // Adjusts how fast the missile rotates

Vector3D LastKnownMissilePosition;
Vector3D LastKnownMissileVelocity;

Vector3D TestTarget = new Vector3D(269.18f,176.96f,89.21f);

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz
}

void Main(string Arg) {
    // SetForwardThrusterOverride(0.05f, Remotes[0]);

    ThrustToTarget(TestTarget);
}

void ThrustToTarget(Vector3D target) {
    // Controls missile orientation and 

    // Get all remote controls then get the first one
    List<IMyRemoteControl> Remotes = new List<IMyRemoteControl>();
    GridTerminalSystem.GetBlocksOfType(Remotes);
    IMyRemoteControl Remote = Remotes[0]; // Get remote with index = 0

    LastKnownMissilePosition = Remote.GetPosition();
    LastKnownMissileVelocity = Remote.GetShipVelocities().LinearVelocity;

    Vector3D TargetVelocityVector = Vector3D.Normalize(target - LastKnownMissilePosition) * (MaxSpeed + TargetSpeedOvershoot); // creates scaled vector pointing at target with set magnitude
    
    Vector3D DeltaVelocity = TargetVelocityVector - LastKnownMissileVelocity; // Gets difference between target velocity and current velocity

    // Orient Towards Orientation Waypoint
    
    Vector3D OrientationVector = Vector3D.Normalize(DeltaVelocity); // Gets unit vector for remote to target to rotate the missile
    // RotateToOrientation(Quaternion.CreateFromAxisAngle(OrientationVector, 1f), Remote); // Rotate to direction vector via axis angle (angle = 0)
    RotateToOrientation(Quaternion.Identity, Remote);

    // Override Thrusters to Max
    // SetForwardThrusterOverride(1, Remote);
}

void SetForwardThrusterOverride(float OverridePercentage, IMyRemoteControl ReferenceRemote) {
    // Overrides forward thrusters defined by blocks with their backward equal to the reference remote's forward

    List<IMyThrust> Thrusters = new List<IMyThrust>();

    GridTerminalSystem.GetBlocksOfType(Thrusters); // Get all thruster blocks on the missile

    Vector3I RemoteForwardVector = Base6Directions.GetIntVector(ReferenceRemote.Orientation.Forward);

    for (int i = 0; i < Thrusters.Count; i++) { // Iterate through all thrusters

        Vector3I ThrusterBackwardVector = Base6Directions.GetIntVector(Thrusters[i].Orientation.Forward) * -1; // Flip forward vector to compare against remote forward vector
        
        if (ThrusterBackwardVector == RemoteForwardVector) { // If forward vector is equal to backward vector, set thruster override

            Thrusters[i].ThrustOverridePercentage = OverridePercentage;

        }
    }
}

void RotateToOrientation(Quaternion TargetOrientation, IMyRemoteControl ReferenceRemote) {
    Quaternion RemoteTransQuat;
    MyBlockOrientation RemoteOrientation = ReferenceRemote.Orientation;
    RemoteOrientation.GetQuaternion(out RemoteTransQuat);

    Quaternion CurrentOrientation = Quaternion.CreateFromRotationMatrix(ReferenceRemote.WorldMatrix);
    CurrentOrientation = CurrentOrientation * RemoteTransQuat; // Rotate grid orientation quat to remote's orientation

    Quaternion DeltaOrientation = Quaternion.Inverse(CurrentOrientation) * TargetOrientation; // Get delta quaternion from current to target orientation
    Vector3 deltaEuler = QuaternionToYawPitchRoll(DeltaOrientation); // Get yaw, pitch, and roll from delta quaternion

    deltaEuler = deltaEuler * RotationSpeedMutiplier; // Apply scalar multiplication to euler transformation
    
    deltaEuler.X *= -1f;
    deltaEuler.Z *= -1f; // Some transformations to make things work

    ApplyRotationToGyros(deltaEuler.X, deltaEuler.Y, deltaEuler.Z, ReferenceRemote); // apply rotation to gyros

    // Echo(deltaEuler.ToString("0.00"));
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

float GetAngleBetweenQuaternions(Quaternion Quat1, Quaternion Quat2) {
    float angle = (float)Math.Acos((double)Quaternion.Dot(Quat1, Quat2));

    if (float.IsNaN(angle)) { // Catch Nan errors
        return 0;
    }
    else {
        return angle;
    }
}

void ApplyRotationToGyros(float yaw, float pitch, float roll, IMyRemoteControl ReferenceRemote) {
    List<IMyGyro> Gyros = new List<IMyGyro>();
    GridTerminalSystem.GetBlocksOfType(Gyros); // Get all gyros on grid

    MyBlockOrientation ReferenceOrientation = ReferenceRemote.Orientation; // Get orientations of reference
    MyBlockOrientation GyroOrientation;

    Base6Directions.Direction GyroForward;
    Base6Directions.Direction GyroUp; // Predeclare gyro variables

    Base6Directions.Direction RefForward = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Forward);
    Base6Directions.Direction RefBackward = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Backward);
    Base6Directions.Direction RefUp = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Up);
    Base6Directions.Direction RefDown = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Down);
    Base6Directions.Direction RefLeft = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Left);
    Base6Directions.Direction RefRight = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Right); // Get Reference orientation directions relative to the grid

    for (int g = 0; g < Gyros.Count; g++) {
        GyroOrientation = Gyros[g].Orientation; // Get gyro orientation

        GyroForward = GyroOrientation.TransformDirection(Base6Directions.Direction.Forward);
        GyroUp = GyroOrientation.TransformDirection(Base6Directions.Direction.Up); // Get gyro cardinal directions relative to their orientation to the grid

        float y,p,r;

        // Let's look at roll first
        if (GyroForward == RefForward) {
            r = roll; // We know this off the bat

            if (GyroUp == RefUp) {
                y = yaw;
                p = pitch;
            }
            else if (GyroUp == RefDown) {
                y = -yaw;
                p = -pitch;
            }
            else if (GyroUp == RefLeft) {
                y = pitch;
                p = -yaw;
            }
            else { // GyroUp == RefRight
                y = -pitch;
                p = yaw;
            }
        }
        else if (GyroForward == RefBackward) {
            r = -roll; // We know this off the bat

            if (GyroUp == RefUp) {
                y = yaw;
                p = -pitch;
            }
            else if (GyroUp == RefDown) {
                y = -yaw;
                p = pitch;
            }
            else if (GyroUp == RefLeft) {
                y = pitch;
                p = yaw;
            }
            else { // GyroUp == RefRight
                y = -pitch;
                p = -yaw; 
            }
        }
        else if (GyroForward == RefUp) {
            r = -yaw; // We know this off the bat

            if (GyroUp == RefBackward) {
                y = roll;
                p = pitch;
            }
            else if (GyroUp == RefForward) {
                y = -roll;
                p = -pitch;
            }
            else if (GyroUp == RefLeft) {
                y = pitch;
                p = -roll;
            }
            else { // GyroUp == RefRight
                y = -pitch;
                p = roll;
            }
        }
        else if (GyroForward == RefDown) {
            r = yaw; // We know this off the bat

            if (GyroUp == RefForward) {
                y = -roll;
                p = pitch;
            }
            else if (GyroUp == RefBackward) {
                y = roll;
                p = -pitch;
            }
            else if (GyroUp == RefLeft) {
                y = pitch;
                p = roll;
            }
            else { // GyroUp == RefRight
                y = -pitch;
                p = -roll;
            }
        }
        else if (GyroForward == RefLeft) {
            r = -pitch; // We know this off the bat

            if (GyroUp == RefUp) {
                Echo("hit");
                y = yaw;
                p = roll;
            }
            else if (GyroUp == RefDown) {
                y = -yaw;
                p = -roll;
            }
            else if (GyroUp == RefForward) {
                y = -roll;
                p = yaw;
            }
            else { // GyroUp == RefBackward
                y = roll;
                p = -yaw;
            }
        }
        else { // GyroForward == RefRight
            r = pitch; // We know this off the bat

            if (GyroUp == RefUp) {
                y = yaw;
                p = -roll;
            }
            else if (GyroUp == RefDown) {
                y = -yaw;
                p = roll;
            }
            else if (GyroUp == RefForward) {
                y = -roll;
                p = -yaw;
            }
            else { // GyroUp == RefBackward
                y = roll;
                p = yaw;
            }
        }

        Gyros[g].SetValue<float>("Yaw", y);
        Gyros[g].SetValue<float>("Pitch", p);
        Gyros[g].SetValue<float>("Roll", r);
    }
}