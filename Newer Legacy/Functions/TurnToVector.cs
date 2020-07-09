void GyroTurn6(Vector3D TARGET, double GAIN, List<IMyGyro> Gyros, IMyRemoteControl REF_RC, double ROLLANGLE,double MAXANGULARVELOCITY) {
    //Ensures Autopilot Not Functional
    REF_RC.SetAutoPilotEnabled(false);
    Echo("Running Gyro Control Program");

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

    for (int i = 0; i < Gyros.Count; i++) {
        var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(Gyros[i].WorldMatrix.GetOrientation()));  //Converts To Gyro Local

        //Applies To Scenario
        Gyros[i].Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y) * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z) * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].GyroOverride = true;
    }
}