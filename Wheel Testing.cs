IMyMotorSuspension SusA;
IMyTextPanel LCD;

public Program() {
    SusA = GridTerminalSystem.GetBlockWithName("Wheel Suspension 3x3 Left") as IMyMotorSuspension;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    // List<Sandbox.ModAPI.Interfaces.ITerminalProperty> resultList = new List<Sandbox.ModAPI.Interfaces.ITerminalProperty>();
    List<Sandbox.ModAPI.Interfaces.ITerminalAction> resultList = new List<Sandbox.ModAPI.Interfaces.ITerminalAction>();


    // SusA.GetProperties(resultList);
    SusA.GetActions(resultList);

    string output = "";

    for (int i = 0; i < resultList.Count; i++) {
        output += resultList[i].Id + "\n";
        // output += resultList[i].Id + " | Type: " + resultList[i].TypeName + "\n";
    }
    LCD.WritePublicText(output);
}

void Main(string arg) {

}