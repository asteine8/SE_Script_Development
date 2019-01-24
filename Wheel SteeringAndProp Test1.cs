IMyMotorSuspension SusA;
IMyTextPanel LCD;

public Program() {
    SusA = GridTerminalSystem.GetBlockWithName("Wheel Suspension 3x3 Left") as IMyMotorSuspension;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    SusA.SetValue("Steer override", 0.5f);
}

void Main(string arg) {

}

