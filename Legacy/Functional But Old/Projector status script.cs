

IMyProjector Projector;
IMyTextPanel LCD;

int count = 0;

int SecondsPerUpdate = 5;

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks (0.6Hz)

    Projector = GridTerminalSystem.GetBlockWithName("Projector") as IMyProjector;
    LCD = GridTerminalSystem.GetBlockWithName("Projector Status LCD") as IMyTextPanel;
}

void Main(string arg) {
    count ++;
    if (count == SecondsPerUpdate) {
        LCD.WritePublicText(Projector.DetailedInfo);
        count = 0;
    }
}