
List<IMyLargeTurretBase> Turrets = new List<IMyLargeTurretBase>();
List<IMyThrust> Thrusters = new List<IMyThrust>();
List<IMyReactor> Reactors = new List<IMyReactor>();
List<IMyGyro> Gyros = new List<IMyGyro>();
List<IMyBatteryBlock> Batteries = new List<IMyBatteryBlock>();

IMyTextPanel LCD;

string LCDOutput = "";
string ShipName = " ";

int TotalTurrets, DamagedTurrets, DestroyedTurrets;
int TotalThrusters, DamagedThrusters, DestroyedThrusters;
int TotalReactors, DamagedReactors, DestroyedReactors;
int TotalGyros, DamagedGyros, DestroyedGyros;
int TotalBatteries, DamagedBatteries, DestroyedBatteries;

int i = 0; // Predeclare to prevent redeclaration

int counter = 0;
int numCycles = 5; // Number of cycles until ready to run script

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks

    GridTerminalSystem.GetBlocksOfType(Turrets);
    GridTerminalSystem.GetBlocksOfType(Thrusters);
    GridTerminalSystem.GetBlocksOfType(Reactors);
    GridTerminalSystem.GetBlocksOfType(Gyros);
    GridTerminalSystem.GetBlocksOfType(Batteries);

    TotalTurrets = Turrets.Count;
    TotalThrusters = Thrusters.Count;
    TotalReactors = Reactors.Count;
    TotalGyros = Gyros.Count;
    TotalBatteries = Batteries.Count;

    LCD = GridTerminalSystem.GetBlockWithName("Block Status LCD") as IMyTextPanel;
}

void Main(string argument) {
    if (counter < numCycles) {
        counter ++;
        return;
    }
    else {
        counter = 0;
    }
    GridTerminalSystem.GetBlocksOfType(Turrets);
    GridTerminalSystem.GetBlocksOfType(Thrusters);
    GridTerminalSystem.GetBlocksOfType(Reactors);
    GridTerminalSystem.GetBlocksOfType(Gyros);
    GridTerminalSystem.GetBlocksOfType(Batteries);

    DamagedTurrets = 0;
    DamagedThrusters = 0;
    DamagedReactors = 0;
    DamagedGyros = 0;
    DamagedBatteries = 0;

    LCDOutput = "";


    for (i = 0; i < Turrets.Count; i++) {
        if (!Turrets[i].IsFunctional) {
            DamagedTurrets ++;
        }
    }

    for (i = 0; i < Thrusters.Count; i++) {
        if (!Thrusters[i].IsFunctional) {
            DamagedThrusters ++;
        }
    }

    for (i = 0; i < Reactors.Count; i++) {
        if (!Reactors[i].IsFunctional) {
            DamagedReactors ++;
        }
    }

    for (i = 0; i < Gyros.Count; i++) {
        if (!Gyros[i].IsFunctional) {
            DamagedGyros ++;
        }
    }

    for (i = 0; i < Batteries.Count; i++) {
        if (!Batteries[i].IsFunctional) {
            DamagedBatteries ++;
        }
    }

    DestroyedTurrets = TotalTurrets - Turrets.Count;
    DestroyedThrusters = TotalThrusters - Thrusters.Count;
    DestroyedReactors = TotalReactors - Reactors.Count;
    DestroyedGyros = TotalGyros - Gyros.Count;
    DestroyedBatteries = TotalBatteries - Batteries.Count;

    LCDOutput += ShipName;
    LCDOutput += " Destroyed | Disabled\n";
    LCDOutput += "\n  Turrets: " + DestroyedTurrets.ToString() + " | " + DamagedTurrets.ToString();
    LCDOutput += "\n  Thrusters: " + DestroyedThrusters.ToString() + " | " + DamagedThrusters.ToString();
    LCDOutput += "\n  Reactors: " + DestroyedReactors.ToString() + " | " + DamagedReactors.ToString();
    LCDOutput += "\n  Gyros: " + DestroyedGyros.ToString() + " | " + DamagedGyros.ToString();
    LCDOutput += "\n  Batteries: " + DestroyedBatteries.ToString() + " | " + DamagedBatteries.ToString();

    LCD.WritePublicText(LCDOutput);
}