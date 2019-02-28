

IMyRadioAntenna Antenna; 
IMyTextPanel LCD;
public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks

    Antenna = GridTerminalSystem.GetBlockWithName("Antenna") as IMyRadioAntenna;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
}

void Main(string arg, UpdateType updateSource) {
    string output = "Argument: " + arg + "\n";
    output += updateSource.ToString() + "\n";

    // Transmit message
    output += "Message Status: " + Antenna.TransmitMessage("Hello There", MyTransmitTarget.Everyone).ToString() + "\n";

    LCD.WritePublicText(output);
}


