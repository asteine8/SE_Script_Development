

/*

1) Moves the welder block to the start position
2) Enables welders and moves back only when the welders are no-longer welding
3) Disables welder when projector is finished
4) Displays progress on LCD

 */

string LCDName = "Welder Progress LCD";
string ProjectorName = "Projector";
string WelderGroupName = "Welders";

float pistonSpeed = 0.5f; // m/s

// No Touchy under here ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

IMyTextPanel LCD;
IMyProjector ConstructionProjector;

List<IMyShipWelder> Welders = new List<IMyShipWelder>();
IMyBlockGroup WelderGroup;
List<IMyPistonBase> Pistons = new List<IMyPistonBase>(); // Expected to be the only pistons on the grid

// string red = ""; 
string red = "\uE035";
// string yellow = ""; 
string yellow = "\uE038";
// string green = "";
string green = "\uE036";
// string blue = "";
string blue = "\uE037";


int systemState = 0;
/*
States:
0 - Idle
1 - System initiated, move to start position
2 - System welding
3 - Build completed, moving back to rest position
 */
int percentWelded = 0;
public Program() {
    // Runtime.UpdateFrequency = 0; // Start at 0 update speed
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    LCD = GridTerminalSystem.GetBlockWithName(LCDName) as IMyTextPanel;
    ConstructionProjector = GridTerminalSystem.GetBlockWithName(ProjectorName) as IMyProjector;
}

void Main(string arg) {
    if (arg.Equals("Start")) {
        Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks (0.6Hz)
    }
    else if (arg.Equals("Pause")) {
        Runtime.UpdateFrequency = 0; // Stop autoupdates
    }
    percentWelded += 4;
    DisplaySystemStatus();
}

void DisplaySystemStatus() {
    string output = "";
    output += "++Welder System Status++\n\n";

    switch (systemState) {
        case 0:
            output += " Idle \n";
            break;
        case 1:
            output += " System initializing \n";
            break;
        case 2:
            output += " System welding \n";
            break;
        case 3:
            output += " Build completed \n";
            break;
    }

    int totalBlocks = ConstructionProjector.TotalBlocks;
    int blocksBuilt = totalBlocks - ConstructionProjector.RemainingBlocks;
    // int percentWelded = Convert.ToInt32(((double)blocksBuilt / (double)totalBlocks) * (double)100);


    output += " Completed: " + blocksBuilt.ToString() + "/" + totalBlocks.ToString() + "\n";
    output += " [";
    for (int i = 1; i < 21; i++) {
        if ((i * 5) <= percentWelded) {
            output += green;
        }
        else {
            output += red;
        }
    }
    output += "]\n";

    output += percentWelded.ToString();
    LCD.WritePublicText(output);
}