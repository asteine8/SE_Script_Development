
/*
States:


 */
int programState = 0;
int missileNumber = 0;

MyDetectedEntityInfo raycastInfo;
Vector3D TargetLocation;
// Constants

const double raycastRange = 4000;

//Lists for groups of blocks
List<IMyTerminalBlock> missileSystems = new List<IMyTerminalBlock>(); 
List<IMyTerminalBlock> launchThrusters = new List<IMyTerminalBlock>();
List<IMyTerminalBlock> forwardThrusters = new List<IMyTerminalBlock>();

// Individual blocks on primary grid
IMyTextPanel lcd;
IMyCameraBlock camera;

// Individual blocks on missile
IMyRemoteControl remote;


public void main(string argument) {
    // argument stores the missile number to launch

    switch(programState) {
        case 0: // Initialize the program
            lcd = GridTerminalSystem.GetBlockWithName("Missile LCD") as IMyTextPanel;
            camera = GridTerminalSystem.GetBlockWithName("Missile Camera") as IMyCameraBlock;

            missileNumber = Int32.Parse(argument); // Get missile number from run argument

            camera.EnableRaycast = true;

            if (camera.CanScan(raycastRange)) { // Check to ensure that the camera is capable of scanning at specified range
                raycastInfo = camera.Raycast(raycastRange,0,0);
                
                if (raycastInfo.HitPosition.HasValue) { // Check to make sure we hit something with the raycast
                    TargetLocation = raycastInfo.HitPosition.Value;
                    printToConsole("Target found at " + TargetLocation.ToString("0.0"));
                    AssignBlocks();
                    return;
                }
                else {
                    printToConsole("No Target at given location");
                    return;
                }
            }
            else {
                printToConsole("Can not Scan at specified range")
                return;
            }


            programStated ++;
            break;
    }
    

}

void printToConsole(string str) {
    Echo(str);
    lcd.WritePublicText(str);
}

void AssignBlocks() {

}