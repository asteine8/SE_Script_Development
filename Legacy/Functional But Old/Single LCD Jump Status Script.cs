
IMyBlockGroup JumpDriveGroup;
List<IMyJumpDrive> JumpDrives = new List<IMyJumpDrive>();

IMyTextPanel LCD1;

float[] StoredPower;

int counter = 0;


public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks

}

void Main(string arg) {
    if (counter == 5) {
        CheckAndLogJumpDriveStatus();
        counter = 0;
    }
    else {
        counter++;
    }
}

void CheckAndLogJumpDriveStatus() {
    LCD1 = GridTerminalSystem.GetBlockWithName("Jump Status LCD 1") as IMyTextPanel;

    JumpDriveGroup = GridTerminalSystem.GetBlockGroupWithName("Jump Drives");
    JumpDriveGroup.GetBlocksOfType(JumpDrives);

    StoredPower = new float[JumpDrives.Count];

    string output1 = "  Jump Drives Status\n\n";

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
        string DriveLine = "Drive " + (i+1).ToString() + " [";

        int charge;

        for (charge = 0; charge < 15; charge ++) {
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
        Echo(StoredPower[i].ToString());
        
        output1 += DriveLine + "]\n";

        
    }
    LCD1.WritePublicText(output1);
}