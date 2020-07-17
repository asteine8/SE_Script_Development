IMyMotorSuspension SusA;
IMyTextPanel LCD;

public Program() {
    SusA = GridTerminalSystem.GetBlockWithName("Wheel Suspension") as IMyMotorSuspension;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    List<Sandbox.ModAPI.Interfaces.ITerminalProperty> propertyList = new List<Sandbox.ModAPI.Interfaces.ITerminalProperty>();
    List<Sandbox.ModAPI.Interfaces.ITerminalAction> actionList = new List<Sandbox.ModAPI.Interfaces.ITerminalAction>();


    SusA.GetProperties(propertyList);
    SusA.GetActions(actionList);

    string output = "";

    for (int i = 0; i < actionList.Count; i++) {
        output += "(Action) -- " + actionList[i].Id + "\n";
    }
    for (int i = 0; i < propertyList.Count; i++) {
        output += "(Property) -- " + propertyList[i].Id + " | Type: " + propertyList[i].TypeName + "\n";
    }
    LCD.WriteText(output);
}

void Main(string arg) {

}