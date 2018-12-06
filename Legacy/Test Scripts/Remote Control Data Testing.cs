
IMyRemoteControl Remote;

IMyTextPanel LCD;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Start out with updates every 100 ticks
}

void Main(string arg) {
    Remote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    string output = "";

    Vector3D PlanetPos;
    if (Remote.TryGetPlanetPosition(out PlanetPos)) {
        output += "Pos: " + PlanetPos.ToString("0.00") + "\n";
    }
    else {
        output += "Pos: No Panet Detected\n";
    }

    

}

