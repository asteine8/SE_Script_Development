
IMyTextPanel LCD;
IMyRemoteControl Remote;

VRageMath.Quaternion CurrentRotation;

// Target Rotation
float x = 0f;
float y = 0f;
float z = 0f;
float w = 1f;

VRageMath.Quaternion TargetRotation;
VRageMath.Quaternion QuatDifference;

// Tait Bryan Angles

float Yaw;
float Pitch;
float Roll;

float rotMult = 2f;

float ZeroAngleThreshold = 0.025f;

// Get Gyro Groups

IMyBlockGroup Gyros;
List<IMyGyro> Gyroscopes = new List<IMyGyro>();

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Set update frequency to 1x
    // TargetRotation = VRageMath.Quaternion.CreateFromRotationMatrix(Me.WorldMatrix);
}

void Main (string argument) {
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
    Remote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;

    Gyros = GridTerminalSystem.GetBlockGroupWithName("Gyros") as IMyBlockGroup;
    Gyros.GetBlocksOfType(Gyroscopes); // Get list of gyroscope IMyGyro's

    TargetRotation = new VRageMath.Quaternion(x, y, z, w);

    CurrentRotation = VRageMath.Quaternion.CreateFromRotationMatrix(Me.WorldMatrix);

    Vector3 EulerOrientation = VRageMath.MyMath.QuaternionToEuler(VRageMath.Quaternion.CreateFromRotationMatrix(Me.WorldMatrix));

    Vector3 QuickEulerOrientation;
    VRageMath.Matrix.GetEulerAnglesXYZ(Me.WorldMatrix, out QuickEulerOrientation);

    QuatDifference = CurrentRotation/TargetRotation;

    QuatToTaitBryan(QuatDifference, out Yaw, out Pitch, out Roll);

    string output = "";
    // output += "X: " + QuatDifference.X.ToString("0.000");
    // output += "\nY: " + QuatDifference.Y.ToString("0.000");
    // output += "\nZ: " + QuatDifference.Z.ToString("0.000");
    // output += "\nW: " + QuatDifference.W.ToString("0.000");
    // output += "Yaw: " + Yaw.ToString();
    // output += "\nPitch: " + Pitch.ToString();
    // output += "\nRoll: " + Roll.ToString();
    // output += "\nQuatLength: " + CurrentRotation.Length().ToString("0.00");
    output += "X: " + EulerOrientation.X.ToString();
    output += "\nY: " + EulerOrientation.Y.ToString();
    output += "\nZ: " + EulerOrientation.Z.ToString();
    output += "\n" + CurrentRotation.ToString("0.00");
    
    LCD.WritePublicText(output);

    // for (int i = 0; i < Gyroscopes.Count; i++) {
    //     Gyroscopes[i].SetValueFloat("Roll", -Roll * rotMult);
    //     if (Math.Abs(Roll) < ZeroAngleThreshold) {
    //         Gyroscopes[i].SetValueFloat("Pitch", Pitch * rotMult);
    //         if (Math.Abs(Pitch) < ZeroAngleThreshold) {
    //             Gyroscopes[i].SetValueFloat("Yaw", Yaw * rotMult);
    //         }
    //         else {
    //             Gyroscopes[i].SetValueFloat("Yaw", 0);
    //         }
    //     }
    //     else {
    //         Gyroscopes[i].SetValueFloat("Pitch", 0);
    //         Gyroscopes[i].SetValueFloat("Yaw", 0);
    //     }
    //     // Gyroscopes[i].SetValueFloat("Roll", 0);
    // }
}

void QuatToTaitBryan(VRageMath.Quaternion Quat, out float Yaw, out float Pitch, out float Roll) {
    // Convert a quaternion to Tait Bryan Angles

    float ar = 2 * (Quat.X*Quat.Y + Quat.Z*Quat.W);
    float br = 1 - 2 * (Quat.Y*Quat.Y + Quat.Z*Quat.Z);
    Roll = (float)Math.Atan2(ar, br);

    Pitch = (float)Math.Asin(2 * (Quat.X*Quat.Z - Quat.W*Quat.Y));

    float ay = 2 * (Quat.X*Quat.W + Quat.Y*Quat.Z);
    float by = 1 - 2 * (Quat.Z*Quat.Z + Quat.W*Quat.W);

    Yaw = (float)Math.Atan2(ay, by);
}

// void QuatToTaitBryan(VRageMath.Quaternion Quat, out float Yaw, out float Pitch, out float Roll) {
//     // Convert a quaternion to Tait Bryan Angles

//     float ar = 2 * (Quat.X*Quat.Y + Quat.Z*Quat.W);
//     float br = Quat.W*Quat.W - Quat.Z*Quat.Z - Quat.Y*Quat.Y + Quat.X*Quat.X;
//     Roll = (float)Math.Atan2(ar, br);

//     Pitch = (float)Math.Asin(2 * (Quat.X*Quat.Z - Quat.W*Quat.Y));

//     float ay = 2 * (Quat.X*Quat.W + Quat.Y*Quat.Z);
//     float by = Quat.W*Quat.W + Quat.Z*Quat.Z - Quat.Y*Quat.Y - Quat.X*Quat.X;

//     Yaw = (float)Math.Atan2(ay, by);
// }