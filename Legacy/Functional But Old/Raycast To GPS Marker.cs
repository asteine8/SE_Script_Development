// Raycast To GPS Marker.cs
// Rain 7/24/2018 - Updated 11/19/18 for Setup

// Script to raycast forward and save copyable gps strings to an lcd. Useful for tagging asteroids or grids that you can't get close to.

/* Setup Instructions
1: Pick forward facing camera to raycast from and name "RaycastCamera"
2: Place or pick a lcd in front or next to your cockpit to display information on this script. Name this lcd "TerminalLCD"
3: Place or pick a lcd to act as storage for collected gps coordinants. Name this lcd "WaypointLCD"
4: To add functionality to ship controller toolbars:
    -Run with no argument to just raycast
    -Run with the argument "DeleteLastEntry" as an undo button
5: Modify the variable "RaycastDistance" to increase or decrease raycast range
 */

double RaycastDistance = 3500; // Max raycast range = 3.5km (Adjust as needed) (Charge speed should be 2000m/s)

// Don't touch stuff below this line unless you know what you're doing
// =========================================================================================================================

IMyCameraBlock Camera;
IMyTextPanel TerminalLCD;
IMyTextPanel WaypointLCD;

public Program() {
    Camera = GridTerminalSystem.GetBlockWithName("RaycastCamera") as IMyCameraBlock;
    TerminalLCD = GridTerminalSystem.GetBlockWithName("TerminalLCD") as IMyTextPanel;
    WaypointLCD = GridTerminalSystem.GetBlockWithName("WaypointLCD") as IMyTextPanel;

    Camera.EnableRaycast = true; // Enable raycasting from camera
    Camera.CustomData = "1"; // Start at entity 1
    Echo("Max raycast range: " + Camera.RaycastDistanceLimit.ToString("0") + "m"); // -1 means infinite
}

public void Main(string arg) {
    if (arg == "DeleteLastEntry") {
        string gpsWaypoints = WaypointLCD.GetPublicText(); // Get string with all recorded waypoints
        string[] splitWaypoints = gpsWaypoints.Split(new string[]{"\n"}, StringSplitOptions.RemoveEmptyEntries);
        string[] MinusOneWaypointArray = new string[splitWaypoints.Length - 1];

        for (int i = 0; i < MinusOneWaypointArray.Length; i++) {
            MinusOneWaypointArray[i] = splitWaypoints[i];
        }

        WaypointLCD.WritePublicText(string.Join("\n", MinusOneWaypointArray)); // Join all lines back together

        int DetectedEntityNumber = Int32.Parse((string)Camera.CustomData); // Get DetectedEntityNumber from camera custom data

        DetectedEntityNumber -= 1; // Go back one waypoint

        TerminalLCD.WritePublicText("Waypoint " + DetectedEntityNumber.ToString() + " deleted");

        Camera.CustomData = DetectedEntityNumber.ToString(); // Save the place change

        return; // We're done here
    }

    if (Camera.CanScan(RaycastDistance)) {

        MyDetectedEntityInfo DetectedEntityInfo  = Camera.Raycast(RaycastDistance, 0, 0); // Raycast Forwards (0 pitch + 0 yaw)

        if (!DetectedEntityInfo.IsEmpty()) {

            string gpsWaypoints = WaypointLCD.GetPublicText(); // String to store all recorded waypoints

            string WaypointName = DetectedEntityInfo.Type.ToString(); // String to store name of waypoint
            // Echo(Camera.CustomData);
            int DetectedEntityNumber = Int32.Parse((string)Camera.CustomData); // Get DetectedEntityNumber from camera custom data
            WaypointName += " " + DetectedEntityNumber.ToString();

            DetectedEntityNumber ++; // Increment DetectedEntityNumber and write to camera custom data
            Camera.CustomData = DetectedEntityNumber.ToString();

            Vector3D EntityPos = DetectedEntityInfo.HitPosition.Value; // Get center of entity's bounding box as a Vector3D

            // Generate Waypoint string from detected entity position
            string WaypointString = "GPS:" + WaypointName;
            WaypointString += ":" + EntityPos.X.ToString("0.00");
            WaypointString += ":" + EntityPos.Y.ToString("0.00");
            WaypointString += ":" + EntityPos.Z.ToString("0.00") + ":";

            gpsWaypoints += WaypointString + "\n"; // Add waypoint to waypoint string and return line

            WaypointLCD.WritePublicText(gpsWaypoints); // Write all waypoints to LCD

            TerminalLCD.WritePublicText("Recorded location for " + WaypointName);
            
        }
        else {
            TerminalLCD.WritePublicText("Raycast did not hit anything");
        }
    }
    else {
        TerminalLCD.WritePublicText("Raycast not charged\nWait " + Camera.TimeUntilScan(RaycastDistance).ToString() + " seconds");
    }
}