

// Thruster Groups - Direction tells what direction thrusters thrust towards

IMyBlockGroup ForwardThrust;
IMyBlockGroup BackwardThrust;
IMyBlockGroup UpThrust;
IMyBlockGroup DownThrust;
IMyBlockGroup LeftThrust;
IMyBlockGroup RightThrust;

List<IMyThrust> ForwardThrusters = new List<IMyThrust>();
List<IMyThrust> BackwardThrusters = new List<IMyThrust>();
List<IMyThrust> UpThrusters = new List<IMyThrust>();
List<IMyThrust> DownThrusters = new List<IMyThrust>();
List<IMyThrust> LeftThrusters = new List<IMyThrust>();
List<IMyThrust> RightThrusters = new List<IMyThrust>();

// Other Block Groups

List<IMyGyro> Gyroscopes = new List<IMyGyro>();
List<IMyShipDrill> Drills = new List<IMyShipDrill>();
List<IMyCargoContainer> Cargos = new List<IMyCargoContainer>();

// Single Block Delarations

IMyCameraBlock ForwardRaycastCamera;
IMyRemoteControl Remote;
IMyShipConnector Connector;




public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update1; // Set update frequency to 1x
}

void Main (string argument) {

}