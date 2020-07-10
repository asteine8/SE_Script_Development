List<IMyLargeTurretBase> Turrets = new List<IMyLargeTurretBase>();
IMyTextPanel LCD;

public Program() {
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
    GridTerminalSystem.GetBlocksOfType(Turrets);

    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Set update frequency to every 10 ticks
}

public void Main(string arg) {
    for (int i = 0; i < Turrets.Count; i++) {
        if (Turrets[i].HasTarget) {
            MyDetectedEntityInfo information = Turrets[i].GetTargetedEntity();

            string stuff = information.Position.ToString("0.00");

            stuff += "\n" + information.Relationship.ToString();
            stuff += "\n" + information.Name.ToString();
            stuff += "\n" + information.Type.ToString();
            stuff += "\n" + information.BoundingBox.ToString();
            LCD.WriteText(stuff);
        }
    }
}