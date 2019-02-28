// Gyroscope Control Script
// Rain42 - 12/3/18

/*
This script adds functionality to gyroscopes, allowing one to align their grid
with gravity, their current orientation, and even other grids. In addition, once
aligned, the orientation of the ship can be rotated in 90 degree intervals.

Set Up Instructions:
    1: Add a remote control. Name it whatever the RemoteControlName is (See user defined variables)
    2: Add a forward facing camera. Name it whatever the RaycastCameraName is
    3: Add this script to a programmable block on the same grid
    4: To add functionality to your toolbar, use the run action of the programmable block with one of the below arguments

Accepted Arguments (Case Insensitive):
    "Current": Maintain current heading
    "Level": Keep azumith the same but align elevation to 0 and roll to 0 with gravity
    "GravDown": Keep current roll, but constrain forward vector to gravity
    "GravUp": Keep current roll, but contrain forward vector to inverse gravity
    "Save": Save current orientation to programmable block custom data (Quaternion format)
    "Load": Load orientation from programmable block custom data (Quaternion format)
    "AlignToGrid": Aligns to grid hit by raycast

    Note: Modulation Commands are relative to the current orientation of the remote control
    "RollRight": Roll right by 90 degrees
    "RollLeft": Roll left by 90 degrees
    "RotateLeft": Rotates left by 90 degrees
    "RotateRight": Rotates Right by 90 degrees
    "RotateUp": Rotates Up by 90 degrees
    "RotateDown": Rotates Down by 90 degrees

    "On": Turn on gyro control
    "Off": Turn off gyro control
*/

// +++User Defined variables+++
double RaycastRange = 500; // How far to raycast when attempting alignment (Try to keep it low)
double NoGravThreshold = 0.02; // Minimum gravity to align to

// Rotation modifiers
double rotationGain = 2; // Increases azuith/elevation rotation rates
double rollGain = 0.5; // Increases roll rotation rates
double maxAngularVelocity = 20; // 30 is max. Decrease if turning too fast is an issue
int rotationUpdatePeriod = 1; // Waits rotationUpdatePeriod cycles (in 0.6Hz) before updating gyros

// Block Names
string RemoteControlName = "Remote Control";
string RaycastCameraName = "Alignment Camera";

// =========================================================================================================================
//                                         Actual Program Stuff, don't go under here
// =========================================================================================================================

List<IMyGyro> Gyros = new List<IMyGyro>();
int gyroUpdateCounter = 0;

IMyRemoteControl Remote;

IMyCameraBlock AlignmentCam;
bool AlignmentAvailible = false;

QuaternionD TargetOrientation;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.None; // Don't Autoupdate at start

    GridTerminalSystem.GetBlocksOfType(Gyros); // Get all gyros
    Remote = GridTerminalSystem.GetBlockWithName(RemoteControlName) as IMyRemoteControl;

    try {
        AlignmentCam = GridTerminalSystem.GetBlockWithName(RaycastCameraName) as IMyCameraBlock;
        AlignmentCam.EnableRaycast = true;
        AlignmentAvailible = true; // Only goes true if camera exists
    } catch(Exception e) {
        Echo("No Alignment Camera Found");
    }
    Echo(AlignmentAvailible.ToString());

    SetGyroOverrides(Gyros, false); // Disable gyro overrides
    TargetOrientation = GetCurrentOrientation(Remote);
}

void Main(string arg) {
    arg = arg.Trim().ToLower(); // process argument string

    switch (arg) {
        // Program State Commands
        case "on":
            Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz
            gyroUpdateCounter = 0; // Reset gyro counter
            SetGyroOverrides(Gyros, true); // Enable gyro overrides
            break;
        case "off":
            Runtime.UpdateFrequency = UpdateFrequency.None; // Don't Autoupdate
            SetGyroOverrides(Gyros, false); // Disable gyro overrides
            break;

        // Basic Commands
        case "current":
            TargetOrientation = GetCurrentOrientation(Remote);
            break;
        case "level":
            if (Remote.GetNaturalGravity().Length() < NoGravThreshold) {
                TargetOrientation = GetCurrentOrientation(Remote);
            }
            else {
                TargetOrientation = GetLevelOrientation(Remote);
            }
            break;
        case "gravdown":
            if (Remote.GetNaturalGravity().Length() < NoGravThreshold) {
                TargetOrientation = GetCurrentOrientation(Remote);
            } else {
                TargetOrientation = GetGravOrientation(Remote, "down");
            }
            break;
        case "gravup":
            if (Remote.GetNaturalGravity().Length() < NoGravThreshold) {
                TargetOrientation = GetCurrentOrientation(Remote);
            } else {
                TargetOrientation = GetGravOrientation(Remote, "up");
            }
            break;
        case "aligntogrid":
            if (AlignmentAvailible) { // Prevent errors with undefined block
                TargetOrientation = GetAlignmentFromRaycast(Remote,AlignmentCam,RaycastRange);
            }
            break;

        // Load and Save Commands
        case "save":
            QuaternionD OutputQuat = GetCurrentOrientation(Remote);
            string output = "";
            output += "X:" + OutputQuat.X.ToString("0.00000") + "\n";
            output += "Y:" + OutputQuat.Y.ToString("0.00000") + "\n";
            output += "Z:" + OutputQuat.Z.ToString("0.00000") + "\n";
            output += "W:" + OutputQuat.W.ToString("0.00000");
            Me.CustomData = output;
            break;
        case "load":
            try {
                QuaternionD inputQuat = QuaternionD.Identity;
                string quatStr = Me.CustomData;
                string[] lines = quatStr.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
                inputQuat.X = Double.Parse(lines[0].Trim().Substring(2));
                inputQuat.Y = Double.Parse(lines[1].Trim().Substring(2));
                inputQuat.Z = Double.Parse(lines[2].Trim().Substring(2));
                inputQuat.W = Double.Parse(lines[3].Trim().Substring(2));
                TargetOrientation = inputQuat;
            } catch (Exception e) {
                Echo("Failed Loading Orientation\n" + e.Message);
            }
            break;

        // Rotation Modification Commands
        case "rollright":
            TargetOrientation = TransformQuaternion(TargetOrientation,0,0,90);
            break;
        case "rollleft":
            TargetOrientation = TransformQuaternion(TargetOrientation,0,0,-90);
            break;
        case "rotateright":
            TargetOrientation = TransformQuaternion(TargetOrientation,90,0,0);
            break;
        case "rotateleft":
            TargetOrientation = TransformQuaternion(TargetOrientation,-90,0,0);
            break;
        case "rotateup":
            TargetOrientation = TransformQuaternion(TargetOrientation,0,90,0);
            break;
        case "rotatedown":
            TargetOrientation = TransformQuaternion(TargetOrientation,0,-90,0);
            break;
    }

    // Update gyros
    if (gyroUpdateCounter >= rotationUpdatePeriod) {
        TurnToQuaternion(TargetOrientation, Gyros, Remote, rotationGain, rollGain, maxAngularVelocity);
        gyroUpdateCounter = 0; // Reset counter
    } else gyroUpdateCounter++;
}

QuaternionD GetCurrentOrientation(IMyRemoteControl REF_REMOTE) {
    return QuaternionD.CreateFromForwardUp(REF_REMOTE.WorldMatrix.Forward, REF_REMOTE.WorldMatrix.Up);
}

QuaternionD TransformQuaternion(QuaternionD BaseQuat, double yaw, double pitch, double roll) {
    QuaternionD Transform = QuaternionD.CreateFromYawPitchRoll(-yaw*Math.PI/180,pitch*Math.PI/180,-roll*Math.PI/180);
    return BaseQuat*Transform;
}

QuaternionD GetLevelOrientation(IMyRemoteControl REF_REMOTE) {
    Vector3D GravVector = Remote.GetNaturalGravity();
    Vector3D Right = Vector3D.Cross(GravVector, Remote.WorldMatrix.Forward);
    Vector3D Forward = Vector3D.Normalize(Vector3D.Cross(Right, GravVector));
    return QuaternionD.CreateFromForwardUp(Vector3D.Normalize(Forward), Vector3D.Normalize(GravVector*-1));
}

QuaternionD GetGravOrientation(IMyRemoteControl REF_REMOTE, string updown) {
    Vector3D GravVector = Vector3D.Normalize(Remote.GetNaturalGravity());
    Vector3D Up = Vector3D.Normalize((updown == "up") ? Vector3D.Cross(Remote.WorldMatrix.Left, GravVector) : Vector3D.Cross(Remote.WorldMatrix.Right, GravVector));
    GravVector = (updown == "up") ? GravVector * -1 : GravVector;
    return QuaternionD.CreateFromForwardUp(GravVector, Up);
}

QuaternionD GetAlignmentFromRaycast(IMyRemoteControl REF_REMOTE, IMyCameraBlock Camera, double RaycastRange) {
    MyDetectedEntityInfo RaycastResult = Camera.Raycast(RaycastRange, 0f, 0f);
    if (RaycastResult.IsEmpty()) { // Didn't hit anything
        return QuaternionD.Identity;
    }
    else { // Hit something, align to it
        MatrixD TargetMatrix = RaycastResult.Orientation;
        Vector3D GravVector = Vector3D.Normalize(REF_REMOTE.GetNaturalGravity());

        double[] anglesToForward = new double[] {0,0,0,0,0,0}; // Forward,Backward,Right,Left,Down,Up
        Vector3D ShipForward = REF_REMOTE.WorldMatrix.Forward;
        anglesToForward[0] = GetAngleBetweenVectors(TargetMatrix.Forward, ShipForward);
        anglesToForward[1] = GetAngleBetweenVectors(TargetMatrix.Backward, ShipForward);
        anglesToForward[2] = GetAngleBetweenVectors(TargetMatrix.Right, ShipForward);
        anglesToForward[3] = GetAngleBetweenVectors(TargetMatrix.Left, ShipForward);
        anglesToForward[4] = GetAngleBetweenVectors(TargetMatrix.Down, ShipForward);
        anglesToForward[5] = GetAngleBetweenVectors(TargetMatrix.Up, ShipForward);
        int index = 0;
        double lowest = anglesToForward[0];
        for (int i = 1; i < anglesToForward.Length; i++) {
            if (anglesToForward[i] < lowest){
                lowest = anglesToForward[i];
                index = i;
            }
        }
        Vector3D Forward = TargetMatrix.Forward;
        switch(index) {
            case 0:
                Forward = TargetMatrix.Forward;
                break;
            case 1:
                Forward = TargetMatrix.Backward;
                break;
            case 2:
                Forward = TargetMatrix.Right;
                break;
            case 3:
                Forward = TargetMatrix.Left;
                break;
            case 4:
                Forward = TargetMatrix.Down;
                break;
            case 5:
                Forward = TargetMatrix.Up;
                break;
        }
        Forward = Vector3D.Normalize(Forward);

        if (GravVector.Length() < NoGravThreshold) { // No gravity here, don't worry about it
            return QuaternionD.CreateFromForwardUp(TargetMatrix.Forward, TargetMatrix.Up);
        }
        else { // Now worry about gravity
            Vector3D Right = Vector3D.Cross(GravVector, Forward);
            Vector3D Up = Vector3D.Normalize(Vector3D.Cross(Right, Forward));
            return QuaternionD.CreateFromForwardUp(Forward, Up);
        }
    }
}

void SetGyroOverrides(List<IMyGyro> Gyros, bool state) {
    for (int i = 0; i < Gyros.Count; i++) {
        Gyros[i].GyroOverride = state;
    }
}

/* Modified Gyro Rotation Script:
    Base code from RDav.
    Modified to use quaternions instead of a target position
*/
void TurnToQuaternion(QuaternionD TargetO, List<IMyGyro> Gyros, IMyRemoteControl REF_RC, double GAIN, double RollGain, double MAXANGULARVELOCITY) {
    //Ensures Autopilot Not Functional
    REF_RC.SetAutoPilotEnabled(false);

    Quaternion TargetOrientation = new Quaternion((float)TargetO.X,(float)TargetO.Y,(float)TargetO.Z,(float)TargetO.W);

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

double GetAngleBetweenVectors(Vector3D V1, Vector3D V2) {
    V1 = Vector3D.Normalize(V1);
    V2 = Vector3D.Normalize(V2);

    return 180*Math.Acos(Vector3D.Dot(V1,V2))/Math.PI; // Convert to degrees
}
