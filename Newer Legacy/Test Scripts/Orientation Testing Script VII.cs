IMyTextPanel LCD;
IMyRemoteControl Remote;

VRageMath.Quaternion CurrentRotation;
VRageMath.Quaternion TargetRotation;

// Get Gyro Groups

IMyBlockGroup Gyros;
List<IMyGyro> Gyroscopes = new List<IMyGyro>();

public struct TaitBryanAngleGroup { // Struct for Tait Bryan Angles
    public float Pitch, Yaw, Roll;

    public TaitBryanAngleGroup(float yaw, float pitch, float roll) {
        Pitch = pitch;
        Yaw = yaw;
        Roll = roll;
    }
    public TaitBryanAngleGroup(double yaw, double pitch, double roll) {
        Pitch = (float)pitch;
        Yaw = (float)yaw;
        Roll = (float)roll;
    }
}

TaitBryanAngleGroup CurrentAngles;
TaitBryanAngleGroup TargetAngles;
TaitBryanAngleGroup DeltaAngles;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Set update frequency to 1x
    TargetRotation = new VRageMath.Quaternion(0,0,0,1); // Set Target to world forward
}

void Main(string argument) {
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
    Remote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;

    Gyros = GridTerminalSystem.GetBlockGroupWithName("Gyros") as IMyBlockGroup;
    Gyros.GetBlocksOfType(Gyroscopes); // Get list of gyroscope IMyGyro's

    CurrentRotation = VRageMath.Quaternion.CreateFromRotationMatrix(Me.WorldMatrix);

    CurrentAngles = new TaitBryanAngleGroup();
    TargetAngles = new TaitBryanAngleGroup();
    DeltaAngles = new TaitBryanAngleGroup();

    QuatToTaitBryan(TargetRotation, out TargetAngles.Yaw, out TargetAngles.Pitch, out TargetAngles.Roll);
    QuatToTaitBryan(CurrentRotation, out CurrentAngles.Yaw, out CurrentAngles.Pitch, out CurrentAngles.Roll);
    
    DeltaAngles.Yaw = TargetAngles.Yaw - CurrentAngles.Yaw;
    DeltaAngles.Pitch = TargetAngles.Pitch - CurrentAngles.Pitch;
    DeltaAngles.Roll = TargetAngles.Roll - CurrentAngles.Roll;

    string output = "";
    output += "DYaw" + DeltaAngles.Yaw.ToString("0.00");
    output += "\nDPitch" + DeltaAngles.Pitch.ToString("0.00");
    output += "\nDRoll" + DeltaAngles.Roll.ToString("0.00");

    LCD.WritePublicText(output);
}

void QuatToTaitBryan(VRageMath.Quaternion Quat, out float Yaw, out float Pitch, out float Roll) {
    // Convert a quaternion to Tait Bryan Angles

    float ar = 2 * (Quat.GetComponent(0)*Quat.GetComponent(1) + Quat.GetComponent(2)*Quat.GetComponent(3));
    float br = Quat.GetComponent(3)*Quat.GetComponent(3) - Quat.GetComponent(2)*Quat.GetComponent(2) - Quat.GetComponent(1)*Quat.GetComponent(1) + Quat.GetComponent(0)*Quat.GetComponent(0);
    Roll = (float)Math.Atan2(ar, br);

    Pitch = (float)Math.Asin(2 * (Quat.GetComponent(0)*Quat.GetComponent(2) - Quat.GetComponent(3)*Quat.GetComponent(1)));

    float ay = 2 * (Quat.GetComponent(0)*Quat.GetComponent(3) + Quat.GetComponent(1)*Quat.GetComponent(2));
    float by = Quat.GetComponent(3)*Quat.GetComponent(3) + Quat.GetComponent(2)*Quat.GetComponent(2) - Quat.GetComponent(1)*Quat.GetComponent(1) - Quat.GetComponent(0)*Quat.GetComponent(0);

    Yaw = (float)Math.Atan2(ay, by);
}