// Raycast To GPS Marker.cs
// Rain 7/24/2018 - Updated 12/2/18 for Setup

// Script to raycast forward and save copyable gps strings to an lcd. Useful for tagging asteroids
// or grids that you can't get close to.

/* Setup Instructions
1: Pick forward facing camera to raycast from and name "RaycastCamera"
2: Place or pick a lcd in front or next to your cockpit to display information on this script. Name this lcd "TerminalLCD"
3: Place or pick a lcd to act as storage for collected gps coordinants. Name this lcd "WaypointLCD"
4: To add functionality to ship controller toolbars:
    -Run with the argument "Mark" to just raycast
    -Run with the argument "DeleteLastEntry" as an undo button
5: Modify the variable "RaycastDistance" to increase or decrease raycast range
 */

/* Instructions for Use
To mark a grid, run the programmable block with the argument "Mark" while facing towards the grid. Note
that this script does not fire multiple raycasts in a search pattern around the forward facing
vector so be sure that you are actually pointed at the target.

The Terminal LCD will display the status of the program. Place it near or in front of the cockpit
so it can be easily seen and adjust the font-size so it is readable. The Waypoint LCD will store
the recorded gps coordinants. These can be directly pasted into one's gps line by line (You can't
directly edit one's gps coords via programmable block for good reason). Place it somewhere accessable.

Arguments accepted (Case insensitive):
    "Mark": Raycasts forward and stores the gps of any hit object at the hit point
    "DeleteLastEntry": Acts like an undo to the list of gps points (deletes the most recent entry)
*/

double RaycastDistance = 5000; // Max raycast range = 5km (Adjust as needed) (Charge speed should be about 1000m/s)

// Don't touch stuff below this line unless you know what you're doing
// =========================================================================================================================

IMyCameraBlock Camera;
IMyTextPanel TerminalLCD;
IMyTextPanel WaypointLCD;

string TerminalStatus = "";

public Program() {
    // Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks
    Camera = GridTerminalSystem.GetBlockWithName("RaycastCamera") as IMyCameraBlock;
    TerminalLCD = GridTerminalSystem.GetBlockWithName("TerminalLCD") as IMyTextPanel;
    WaypointLCD = GridTerminalSystem.GetBlockWithName("WaypointLCD") as IMyTextPanel;

    Camera.EnableRaycast = true; // Enable raycasting from camera
    Camera.CustomData = (Camera.CustomData.Trim() == "") ? "1" : Camera.CustomData.Trim(); // Start at entity 1 if empty
    Echo("Max raycast range: " + Camera.RaycastDistanceLimit.ToString("0") + "m"); // -1 means infinite
}

public void Main(string arg) {

    if (arg == "DeleteLastEntry") { // Delete Last Entry from Waypoint LCD
        string gpsWaypoints = WaypointLCD.GetPublicText(); // Get string with all recorded waypoints
        if (gpsWaypoints.Trim().Length == 0) {
            TerminalStatus = "No Entries to delete";
        }
        else {
            string[] splitWaypoints = gpsWaypoints.Split(new string[]{"\n"}, StringSplitOptions.RemoveEmptyEntries);
            string[] MinusOneWaypointArray = new string[splitWaypoints.Length - 1];

            for (int i = 0; i < MinusOneWaypointArray.Length; i++) {
                MinusOneWaypointArray[i] = splitWaypoints[i];
            }

            WaypointLCD.WritePublicText(string.Join("\n", MinusOneWaypointArray)); // Join all lines back together

            int DetectedEntityNumber = Int32.Parse((string)Camera.CustomData); // Get DetectedEntityNumber from camera custom data

            DetectedEntityNumber -= 1; // Go back one waypoint

            TerminalStatus = ("Waypoint " + DetectedEntityNumber.ToString() + " deleted");

            Camera.CustomData = DetectedEntityNumber.ToString(); // Save the place change
        }
    }

    if (Camera.CanScan(RaycastDistance) && arg.ToLower() == "mark") { // Raycast

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

            TerminalStatus = ("Recorded location for " + WaypointName);

        }
        else {
            TerminalStatus = ("Raycast did not hit anything");
        }
    }
    else if(arg.ToLower() == "mark") {
        TerminalStatus = ("Raycast not charged\nWait " + Camera.TimeUntilScan(RaycastDistance).ToString() + " seconds");
    }

    // Accept Stop/Start Commands for autoupdate (Obsolete)
    // if (arg.ToLower() == "start") Runtime.UpdateFrequency = UpdateFrequency.Update100; // Start updating
    // else if (arg.ToLower() == "stop") Runtime.UpdateFrequency = UpdateFrequency.None; // Stop updating

    // Update TerminalLCD with camera raycast status
    string output = "";
    // output = "Charge = " + Camera.AvailableScanRange.ToString("0") + "m\n";
    output += (Camera.AvailableScanRange >= RaycastDistance) ? "Camera Can Raycast" : "Camera Can't Raycast";
    output += "\n";
    output += TerminalStatus;

    TerminalLCD.WritePublicText(output);
}
