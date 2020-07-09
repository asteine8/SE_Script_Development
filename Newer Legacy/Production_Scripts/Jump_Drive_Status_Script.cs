// Jump Drive Status Script.cs
// Rain 8/17/2018 - Updated 11/19/18 for User

/* Set Up Instructions
1: Change LCD names that you wish to display LCD statuses on using the format below. Higher indexed lcds will recieve overflow from lower indexed lcds
2: Put script into programmable block and change variables as need be
3: Check, compile, and run script
4: Recompile when lcds or jump drives are added/removed
5: Profit
 */

// LCD name format (LCDName can be anything but the tag "<JumpDriveLCD>" must be in the name and is not case sensitive. The lcd index must follow):
// LCDName <JumpDriveLCD>x
// Where:
//      -LCDNAme is the name of the lcd
//      -<JumpDriveLCD> is the tag that designates this as a jump drive lcd
//      -x is the index of the lcd (It will be filled with status bars first)

// Variables to change as need be

int LinesPerBar = 8; // Number of lines per bar
int LinesPerLCD = 6; // Number of jump drive lines per bar
string LCDHeader = "   Jump Drive Status\n\n"; // The text to display as a title on each LCD


// Actual Program, no changing things under here unless you know what you're doing

List<IMyJumpDrive> JumpDrives = new List<IMyJumpDrive>();

List<IMyTextPanel> LCDs = new List<IMyTextPanel>();
List<IMyTextPanel> JumpDriveDisplays = new List<IMyTextPanel>();

float[] StoredPower;
int counter = 5;
int LRProgBlockAnimationState = 0;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks
    GridTerminalSystem.GetBlocksOfType(JumpDrives); // Get all jump drives


    GridTerminalSystem.GetBlocksOfType(LCDs); // Get all LCDs
    // Echo("hit");

    for (int i = 0; i < LCDs.Count; i++) {
        string LCDName = LCDs[i].CustomName + "     ";
        int tagIndex = LCDName.IndexOf("<JumpDriveLCD>");
        if (tagIndex != -1) { // Contains jump drive tag
            int driveIndex = Convert.ToInt32(LCDName.Substring(tagIndex + "<JumpDriveLCD>".Length, 1));
            while (driveIndex >= JumpDriveDisplays.Count) {
                JumpDriveDisplays.Add(null);
            }
            // Echo(JumpDriveDisplays.Count.ToString() + "|" + driveIndex.ToString());
            JumpDriveDisplays[driveIndex] = LCDs[i];
        }
    }
    for (int i = JumpDriveDisplays.Count-1; i >= 0; i--) {
        if (JumpDriveDisplays[i] == null){
            JumpDriveDisplays.RemoveAt(i);
        }
    }
    // Echo(JumpDriveDisplays.Count.ToString());
}

void Main(string arg) {
    if (counter == 5) { // Update every 5 scripts
        CheckAndLogJumpDriveStatus();
        counter = 0;
    }
    else counter++;
    string outP = "Script Running " + ((LRProgBlockAnimationState == 1) ? "/" : "\\");
    LRProgBlockAnimationState = (LRProgBlockAnimationState==0) ? 1 : 0;
    Echo(outP);
}

void CheckAndLogJumpDriveStatus() {

    StoredPower = new float[JumpDrives.Count];

    List<string> outputLines = new List<string>();

    for (int i = 0; i < JumpDrives.Count; i++) {
        string DriveInfo = JumpDrives[i].DetailedInfo;
        string[] splitInfo = DriveInfo.Split(new string[]{"\n"}, StringSplitOptions.RemoveEmptyEntries);
        string[] splitLine = splitInfo[4].Split(new string[]{" "}, StringSplitOptions.None);

        StoredPower[i] = float.Parse(splitLine[2]) / 3f; // Get percentage of stored power in jump drives

        if (splitLine[3] == "kWh") {
            StoredPower[i] /= 1000f; // Convert to MWh
        }
        else if (splitLine[3] == "Wh") {
            StoredPower[i] /= 1000000f; // Convert to MWh
        }

        string DriveLine = JumpDrives[i].CustomName + " [";

        for (int charge = 0; charge < LinesPerBar-1; charge ++) {
            if (StoredPower[i] == 1f) {
                DriveLine += "~";
            }
            else if (charge < (int)(StoredPower[i] * 15f)) {
                DriveLine += "|";
            }
            else {
                DriveLine += " ";
            }
        }
        // Echo(StoredPower[i].ToString());
        DriveLine += "]\n";
        outputLines.Add(DriveLine);
    }

    int numLCDs = (int)Math.Ceiling((float)outputLines.Count/(float)LinesPerLCD);
    numLCDs = (numLCDs > JumpDriveDisplays.Count) ? JumpDriveDisplays.Count : numLCDs;

    for (int i = 0; i < numLCDs; i++) {
        string strOut = LCDHeader;
        for (int l = 0; l < LinesPerLCD; l++) {
            strOut += ((l + (i*LinesPerLCD)) >= (outputLines.Count)) ? "" : outputLines[l + (i*LinesPerLCD)];
        }
        JumpDriveDisplays[i].WritePublicText(strOut);
        // Echo(JumpDriveDisplays[i].CustomName.ToString() + " | " + strOut);
    }
}