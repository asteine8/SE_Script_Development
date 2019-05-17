


IMyRemoteControl ReferenceRemote;
List<IMyGyro> GridGyros = new List<IMyGyro>();

Quaternion TargetO = Quaternion.Identity;

int maxCount = 0;
int count = 1;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz
    ReferenceRemote = GridTerminalSystem.GetBlockWithName("Remote") as IMyRemoteControl;

    GridTerminalSystem.GetBlocksOfType(GridGyros);
}

void Main(string argument) {

    if (argument.Equals("Reset")) {
        TargetO = GetQuaternionOrientation(ReferenceRemote);
    }

    if (count >= maxCount) {
        TurnToQuaternion(TargetO, GridGyros, ReferenceRemote, 2, 0.25, 2);
        count = 0;
    }
    else {
        count ++;
    }
}

Quaternion GetQuaternionOrientation(IMyRemoteControl REF_RC) {
    return Quaternion.CreateFromForwardUp(REF_RC.WorldMatrix.Forward, REF_RC.WorldMatrix.Up);
}

/*
Modified Gyro Rotation Script:
    Base code if from RDav.

    Modified to use quaternions instead of a target position
 */
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