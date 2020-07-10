
IMySoundBlock speaker;
int count = 0;
public Program() {
    speaker = GridTerminalSystem.GetBlockWithName("Speaker") as IMySoundBlock;
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    speaker.Volume = 10F;
    speaker.Range = 50F;
    speaker.LoopPeriod = 30F;
}

public void Main(string arg) {
    if (count++ == 10) {
        speaker.ApplyAction("PlaySound");
        Echo("Playing Sound");

        count = 0;
    }
    if (count == 5) {
        speaker.ApplyAction("StopSound");
        Echo("Stopping Sound");
    }
}