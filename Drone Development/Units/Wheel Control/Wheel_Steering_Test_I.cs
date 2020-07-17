List<IMyMotorSuspension> Wheels = new List<IMyMotorSuspension>();
IMyTextPanel LCD;

public Program() {
    GridTerminalSystem.GetBlocksOfType(Wheels);
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    string output = "";

    foreach (IMyMotorSuspension Wheel in Wheels) {
        Wheel.SetValue("Steer override", 0f);
        Wheel.SetValue("Propulsion override", 0f);

        output += Wheel.CustomName + ": " + Wheel.BlockDefinition.SubtypeId + "\n";
    }

    LCD.WriteText(output);
}

void Main(string arg) {
    
}

