

IMyRemoteControl ReferenceRemote;
IMyTextPanel LCD;
IMyGyro gyro;
IMyCameraBlock camera;

List<IMyGyro> Gyros = new List<IMyGyro>();

Quaternion initialOrientation;
Quaternion currentOrientation;

int count = 0;
public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz

    ReferenceRemote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
    gyro = GridTerminalSystem.GetBlockWithName("Gyro") as IMyGyro;

    camera = GridTerminalSystem.GetBlockWithName("Camera") as IMyCameraBlock;
    camera.EnableRaycast = true;

    GridTerminalSystem.GetBlocksOfType(Gyros);

    initialOrientation = GetCurrentOrientation(ReferenceRemote);
}

void Main(string arg) {
    // if (arg.Equals("Reset")) {
    //     initialOrientation = GetCurrentOrientation(ReferenceRemote);
    // }
    // currentOrientation = GetCurrentOrientation(ReferenceRemote);

    // Echo(currentOrientation.Length().ToString("0.000"));

    // Quaternion deltaOrientation = Quaternion.Conjugate(initialOrientation) * currentOrientation;

    // Echo(deltaOrientation.ToString("0.000"));
    // Echo(deltaOrientation.Length().ToString("0.00"));

    // Vector3 EulerAngles = QuaternionToYawPitchRoll(deltaOrientation);

    // string output = "";

    // output += "Y: " + EulerAngles.X.ToString("0.00") + "\n";
    // output += "P: " + EulerAngles.Y.ToString("0.00") + "\n";
    // output += "R: " + EulerAngles.Z.ToString("0.00") + "\n";

    // LCD.WritePublicText(output);
    for (int i = 0; i < Gyros.Count; i++) {
        GyroTurn6(new Vector3D(0f,0f,0f), 3, Gyros[i], ReferenceRemote, 0, 2.5);
    }
    
    if (count == 1) {
        camera.Raycast(60,0,0);
        count = 0;
    }
    else {
        count ++;
    }
    
}

Quaternion GetCurrentOrientation(IMyRemoteControl reference) {
    Quaternion remoteOrientation;
    reference.Orientation.GetQuaternion(out remoteOrientation);

    Vector3D Forward = reference.WorldMatrix.Forward;
    Vector3D Up = reference.WorldMatrix.Up;

    return Quaternion.CreateFromForwardUp(Forward, Up) * Quaternion.Inverse(remoteOrientation);
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

/*
RDav's Gyro Rotation Script:
    function will: The next generation of Gyroturn, designed to be performance optimised
    over actuating performance, it detects orientation and directly applies overrides

    Modified to not touch remote autopilot and
 */
void GyroTurn6(Vector3D TARGET, double GAIN, IMyGyro GYRO, IMyRemoteControl REF_RC, double ROLLANGLE,double MAXANGULARVELOCITY) {
    //Ensures Autopilot Not Functional
    // REF_RC.SetAutoPilotEnabled(false);

    //Detect Forward, Up & Pos
    Vector3D ShipForward = REF_RC.WorldMatrix.Forward;
    Vector3D ShipUp = REF_RC.WorldMatrix.Up;
    Vector3D ShipPos = REF_RC.GetPosition();

    //Create And Use Inverse Quatinion                   
    Quaternion Quat_Two = Quaternion.CreateFromForwardUp(ShipForward, ShipUp);
    var InvQuat = Quaternion.Inverse(Quat_Two);
    Vector3D DirectionVector = Vector3D.Normalize(TARGET - ShipPos); //RealWorld Target Vector
    Vector3D RCReferenceFrameVector = Vector3D.Transform(DirectionVector, InvQuat); //Target Vector In Terms Of RC Block

    //Convert To Local Azimuth And Elevation
    double ShipForwardAzimuth = 0; double ShipForwardElevation = 0;
    Vector3D.GetAzimuthAndElevation(RCReferenceFrameVector, out ShipForwardAzimuth, out ShipForwardElevation);

    //Does Some Rotations To Provide For any Gyro-Orientation
    var RC_Matrix = REF_RC.WorldMatrix.GetOrientation();
    var Vector = Vector3.Transform((new Vector3D(ShipForwardElevation, ShipForwardAzimuth, ROLLANGLE)), RC_Matrix); //Converts To World
    var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(GYRO.WorldMatrix.GetOrientation()));  //Converts To Gyro Local

    //Applies To Scenario
    GYRO.Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
    GYRO.Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y) * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
    GYRO.Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z) * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
    GYRO.GyroOverride = true;

    //GYRO.SetValueFloat("Pitch", (float)((TRANS_VECT.X) * GAIN));     
    //GYRO.SetValueFloat("Yaw", (float)((-TRANS_VECT.Y) * GAIN));
    //GYRO.SetValueFloat("Roll", (float)((-TRANS_VECT.Z) * GAIN));
}

void TurnToQuaternion(Quaternion TargetOrientation, IMyRemoteControl Reference, double Gain, double MaxAngularVelocity) {
    
}