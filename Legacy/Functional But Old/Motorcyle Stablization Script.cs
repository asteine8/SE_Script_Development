

IMyCameraBlock LeftRaycastCam;
IMyCameraBlock RightRaycastCam;
// IMyTextPanel LCD;

IMyBlockGroup Gyros;
List<IMyGyro> StablizationGyros = new List<IMyGyro>();


MyDetectedEntityInfo leftCast, rightCast;

double raycastRange = 1; // Only go 1m down

double leftElevation, rightElevation;

double angle;

float RollMult = 25f;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Set update frequency to 10Hz
    LeftRaycastCam = GridTerminalSystem.GetBlockWithName("LeftCam") as IMyCameraBlock;
    RightRaycastCam = GridTerminalSystem.GetBlockWithName("RightCam") as IMyCameraBlock;
    // LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    Gyros = GridTerminalSystem.GetBlockGroupWithName("Gyros");
    Gyros.GetBlocksOfType(StablizationGyros);

    LeftRaycastCam.EnableRaycast = true;
    RightRaycastCam.EnableRaycast = true;
}

void Main() {
    leftCast = LeftRaycastCam.Raycast(raycastRange);
    rightCast = RightRaycastCam.Raycast(raycastRange);

    if (!leftCast.IsEmpty() && !rightCast.IsEmpty()) { // Ground is in sight
        
        
        leftElevation = Vector3D.Distance(leftCast.HitPosition.Value, LeftRaycastCam.GetPosition());
        rightElevation = Vector3D.Distance(rightCast.HitPosition.Value, RightRaycastCam.GetPosition());

        double deltaHeight = Math.Abs(leftElevation - rightElevation);

        angle = Math.Tan(deltaHeight/6);

        angle = (leftElevation < rightElevation) ? angle * 1 : angle * -1;

        // Echo("L: " + leftElevation.ToString("0.000") + " | R: " + rightElevation.ToString("0.000"));
        
    }
    for (int i = 0; i < StablizationGyros.Count; i++) {
        StablizationGyros[i].Roll = (float)angle * RollMult;
    }

    // LCD.WritePublicText(StablizationGyros[0].Roll.ToString("0.000"));
}