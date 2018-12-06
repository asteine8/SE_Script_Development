
private double distance = 1000;
// private string str = "";
public void Main(string argument) {

    // IMyTextPanel lcd = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
    IMyCameraBlock camera = GridTerminalSystem.GetBlockWithName("Raycast Camera") as IMyCameraBlock;
    // lcd.WritePublicTitle("Hello");

    camera.EnableRaycast = true;
    if (camera.CanScan(distance)) {

        MyDetectedEntityInfo information = camera.Raycast(distance, 0, -20);

        if(information.HitPosition.HasValue) {

            string stuff = Vector3D.Distance(camera.GetPosition(), information.HitPosition.Value).ToString("0.00");

            stuff += "\n" + information.Relationship.ToString();
            stuff += "\n" + information.Name.ToString();
            stuff += "\n" + information.Type.ToString();
            stuff += "\n" + information.BoundingBox.ToString();
            Echo(stuff);
        }
        else {
            // lcd.WritePublicText("Raycast did not hit anything");
        }
    }
    else {
        // lcd.WritePublicText("Cannot Scan " + distance.ToString("0") + " meters");
    }
}