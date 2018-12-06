IMyLargeTurretBase TargetingTurret;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    Echo("");
}

void Main() {
    TargetingTurret = GridTerminalSystem.GetBlockWithName("Targeting Turret") as IMyLargeTurretBase;
    Echo("Az: " + TargetingTurret.Azimuth.ToString("0.000") + " | El: " + TargetingTurret.Elevation.ToString("0.000"));
}