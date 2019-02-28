
/*
Notes:

-Remote.WorldMatrix returns the base grid orientation transformed by the relative block orientation

 */

IMyRemoteControl Remote;
IMyTextPanel LCD;

List<IMyCameraBlock> Cameras = new List<IMyCameraBlock>();

VRageMath.Quaternion TargetOrientation;
VRageMath.Quaternion CurrentOrientation;

bool OrientToTarget = true;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update every 10 Physics ticks (6Hz)

    Remote = GridTerminalSystem.GetBlockWithName("ForwardRemote") as IMyRemoteControl;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    GridTerminalSystem.GetBlocksOfType(Cameras);

}

void Main(string arg) {


    if (arg.Equals("ResetTarget")) {
        // Reset Target Orientation to current
        TargetOrientation = VRageMath.Quaternion.CreateFromRotationMatrix(Remote.WorldMatrix);
    }
    else if (arg.Equals("ToggleRotation")) {
        // Toggle OrientToTarget
        OrientToTarget = (OrientToTarget == true) ? false : true;
    }
    
    if (OrientToTarget) {
        // Rotate to Target Orientation
    }

    // Test Code for block orientation
    string output = "";

    Vector3 axis;
    float angle;

    VRageMath.Quaternion TransformationQuaternion;

    // LCD.Orientation.GetQuaternion(out TransformationQuaternion);
    // VRageMath.MyBlockOrientation ForwardOrientation = VRageMath.MyBlockOrientation.Identity;
    
    // ForwardOrientation.GetQuaternion(out TransformationQuaternion);

    TransformationQuaternion = VRageMath.Base6Directions.GetOrientation(Remote.Orientation.Forward, Remote.Orientation.Up);
    TransformationQuaternion.GetAxisAngle(out axis, out angle);

    output += "TransAxis: " + axis.ToString("0.00") + "\n";
    output += "TransAngle" + (angle/(float)Math.PI * 180f).ToString("0.00") + "\n\n";

    // CurrentOrientation = VRageMath.Quaternion.CreateFromRotationMatrix(Me.WorldMatrix) * TransformationQuaternion;
    CurrentOrientation = VRageMath.Quaternion.CreateFromRotationMatrix(Remote.WorldMatrix);
    CurrentOrientation.GetAxisAngle(out axis, out angle);

    output += "CAxis: " + axis.ToString("0.00") + "\n";
    output += "CAngle" + (angle/(float)Math.PI * 180f).ToString("0.00") + "\n";

    LCD.WritePublicText(output);
}