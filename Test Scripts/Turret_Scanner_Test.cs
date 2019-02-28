
IMyLargeTurretBase Turret;
IMyTextPanel LCD;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks

    Turret = GridTerminalSystem.GetBlockWithName("Targeting Turret") as IMyLargeTurretBase;
    Turret.Range = 1000f;

    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
}

void Main() {
    LCD.WritePublicText(Turret.GetTargetedEntity().IsEmpty().ToString());
}