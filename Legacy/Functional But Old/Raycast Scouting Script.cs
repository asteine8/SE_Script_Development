
IMyCameraBlock Camera;
IMyTextPanel TerminalLCD;

double RaycastDistance = 2500; // Max raycast range = 2.5km (Adjust as needed) (Charge speed should be 2000m/s)

//=========================================================================================================================

public Program() {
    Camera = GridTerminalSystem.GetBlockWithName("RaycastCamera") as IMyCameraBlock;
    TerminalLCD = GridTerminalSystem.GetBlockWithName("TerminalLCD") as IMyTextPanel;

    Camera.EnableRaycast = true; // Enable raycasting from camera
    Echo("Max raycast range: " + Camera.RaycastDistanceLimit.ToString("0") + "m"); // -1 means infinite
}

public void Main(string arg) {

    if (Camera.CanScan(RaycastDistance)) {

        MyDetectedEntityInfo DetectedEntityInfo  = Camera.Raycast(RaycastDistance, 0, 0); // Raycast Forwards (0 pitch + 0 yaw)

        if (!DetectedEntityInfo.IsEmpty()) {
            string output = "";

            output += "EntityId: " + DetectedEntityInfo.EntityId.ToString() + "\n";
            output += "Type: " + DetectedEntityInfo.Type.ToString() + "\n";
            output += "Relation: " + DetectedEntityInfo.Relationship.ToString() + "\n";
            output += "EntiyName: " + DetectedEntityInfo.Name + "\n\n";

            output += "HitPos: " + DetectedEntityInfo.HitPosition.Value.ToString("0") + "\n";
            output += "Size: " + DetectedEntityInfo.BoundingBox.Size.ToString("0") + "\n";

            double distance = VRageMath.Vector3D.Distance(DetectedEntityInfo.HitPosition.Value, Camera.GetPosition());
            output += "Distance: " + distance.ToString("0");

            TerminalLCD.WritePublicText(output);

        }
        else {
            TerminalLCD.WritePublicText("Raycast did not hit anything");
        }
    }
    else {
        TerminalLCD.WritePublicText("Raycast not charged\nWait " + Camera.TimeUntilScan(RaycastDistance).ToString() + " seconds");
    }
}