
IMyRemoteControl Remote;
IMyTextPanel LCD;

List<IMyGyro> Gyros = new List<IMyGyro>();

VRageMath.Quaternion TargetOrientation;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10;

    Remote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    TargetOrientation = VRageMath.Quaternion.CreateFromRotationMatrix(Remote.WorldMatrix);
}

void Main(string Arg) {
    if (Arg.Equals("Reset")) {
        TargetOrientation = VRageMath.Quaternion.CreateFromRotationMatrix(Remote.WorldMatrix);
        Echo("Changed Orientation to:\n" + TargetOrientation.ToString("0.00"));
    }
    RotateToQuaternion(TargetOrientation);

}

void RotateToQuaternion(VRageMath.Quaternion TargetQuat) {
    string output = "";

    VRageMath.Quaternion CurrentOrientation = VRageMath.Quaternion.CreateFromRotationMatrix(Remote.WorldMatrix);

    // Get difference between Target and Current Rotation
    VRageMath.Quaternion QuatDiff = CurrentOrientation * VRageMath.Quaternion.Inverse(TargetQuat);


    output += EulerAngles.ToString("0.00");


    LCD.WritePublicText(output);

}