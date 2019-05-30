

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//                 User Modifiable Variables
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

// Block id tags (Should be in custom data)
public const string shipControllerTag = "Ship Controller";
public const string antennaTag = "Broadcasting Antenna";
public const string lcdTag = "LCD";

// Group Frequency (What this group's communication's are encoded with)
public const string GROUP_FREQUENCY = "123"; // Lets keep this at three characters

// Update Speed (How many 10s of ticks to wait between updates)
public const int UPDATE_RATE = 2;

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//                   Program - Do not touch
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

bool isRunning = true;

IMyShipController Reference;
IMyRadioAntenna Antenna;
List<IMyTextPanel> displayLCDs = new List<IMyTextPanel>();

IMyBroadcastListener BroadcastListener;

List<long> droneIds = new List<long>();
List<Drone> Drones = new List<Drone>();

List<string> displayLines = new List<string>();

int clk = 0;

public class Drone {
    public Vector3D Offset;
    public long EntityId;
    public Drone (Vector3D offset, long id) {
        this.Offset = offset;
        this.EntityId = id;
    }
}

public Program() {
    if (UpdateBlocks()) {
        
    }
}

void Main(string arg) {
    // Create 
    if (clk == UPDATE_RATE) {
        ListenForBroadcasts(Reference);
        BroadcastInfo(Reference);
        UnicastToDrones(Reference);

        clk = 0;
    }
    else {
        clk++;
    }
}

/**
 * Updates blocks used by program
 * Returns false if the program is unrunnable due to missing blocks
 */
bool UpdateBlocks() {
    // Get reference ship controller
    List<IMyShipController> shipControllers = new List<IMyShipController>();
    GridTerminalSystem.GetBlocksOfType(shipControllers);
    if (shipControllers.Count == 0) return false;
    for (int i = 0; i < shipControllers.Count; i++) {
        if (shipControllers[i].CustomData.ToLower().Contains(shipControllerTag.ToLower())) {
            Reference = shipControllers[i];
        }
        else if (i == shipControllers.Count - 1) {
            // if on last item and no ship controllers are usable, we can't run this script
            return false;
        }
    }

    // Get lcds (Not required)
    List<IMyTextPanel> lcds = new List<IMyTextPanel>();
    displayLCDs = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(lcds);
    for (int i = 0; i < lcds.Count; i++) {
        if (lcds[i].CustomData.ToLower.Contains(lcdTag.ToLower())) {
            displayLCDs.Add(lcds[i]);
        }
    }
    
    // Get antenna for broadcasting
    List<IMyRadioAntenna> antennas = new List<IMyRadioAntenna>();
    GridTerminalSystem.GetBlocksOfType(antennas);
    if (antennas.Count == 0) return false;
    for (int i = 0; i < antennas.Count; i++) {
        if (antennas[i].CustomData.ToLower().Contains(antennaTag.ToLower())) {
            Antenna = antennas[i];
        }
        else if (i == antennas.Count - 1) {
            return false;
        }
    }
    // Attach programmable block to antenna
    Antenna.AttachedProgrammableBlock = Me.EntityId;

    // Register a broadcast listener under the current group tag
    BroadcastListener = IGC.RegisterBroadcastListener(GROUP_FREQUENCY);

    return true;
}

/**
 * Listen for broadcasts from ships in the group
 * Registers ships that are not yet registered yet
 * Unregisters ships that broadcast an unregister command
 */
void ListenForBroadcasts(IMyShipController REFERENCE) {
    if (BroadcastListener.HasPendingMessage) {
        MyIGCMessage message = BroadcastListener.AcceptMessage();

        // Check if we have this drone in registry
        if (!droneIds.Contains(message.Source)) {
            // This drone is not registered. We need to add it
            droneIds.Add(message.Source);
            
            // Get orientation of reference
            Quaternion orientation = Quaternion.CreateFromForwardUp(REFERENCE.WorldMatrix.Forward, REFERENCE.WorldMatrix.Up);

            // Get current position offset of the drone
            Vector3D positionOffset = (Vector3D)message.Data - REFERENCE.GetPosition();
            
            // Untransform to base offset by transforming by inverse orientation quaternion
            positionOffset = Vector3D.Transform(positionOffset,  Quaternion.Inverse(orientation));

            // Add the drone to the list of registered drones
            Drones.Add(new Drone(positionOffset, message.Source));
        }
        // Check for unregister command
        else if (message.Data.ToString().ToLower().Contains("unregister")) {
            UnregisterDrone(message.Source);
        }
    }
}

/**
 * Unregister drone(s) with given entity id
 */
void UnregisterDrone(long id) {
    for (int i = 0; i < Drones.Count; i++) {
        if (Drones[i].EntityId == id) {
            Drones.RemoveAt(i); // Strike the drone from the drone registry
        }
    }
    // Strike the drone from the droneId registery
    droneIds.Remove(id);
}

/**
 * Broadcasts ship orientation as a quaternion
 */
void BroadcastInfo(IMyShipController REFERENCE) {
    // Broadcast ship orientation as quaternion
    Quaternion orientation = Quaternion.CreateFromForwardUp(REFERENCE.WorldMatrix.Forward, REFERENCE.WorldMatrix.Up);
    IGC.SendBroadcastMessage<Quaternion>(GROUP_FREQUENCY, orientation);
}

/**
 * Unicast drone target position and speed to each drone
 */
void UnicastToDrones(IMyShipController REFERENCE) {
    // Get orientation of reference
    Quaternion orientation = Quaternion.CreateFromForwardUp(REFERENCE.WorldMatrix.Forward, REFERENCE.WorldMatrix.Up);

    // Get current scalar speed
    double speed = REFERENCE.GetShipSpeed();

    foreach (Drone drone in Drones) {
        // Get transformed offset
        Vector3D position = REFERENCE.GetPosition() + Vector3D.Transform(drone.Offset, orientation);
        
        // Create a 4D vector to store <Sx, Sy, Sz, Speed>
        Vector4D data = new Vector4D(position.X, position.Y, position.Z, speed);

        // Unicast position to drone with speed
        bool success = IGC.SendUnicastMessage<Vector4D>(drone.EntityId, GROUP_FREQUENCY, data);

        // Could not send message, drone must be disabled. Unregister drone
        if (!success) {
            UnregisterDrone(drone.EntityId);
        }
    }
}

/**
 * Write some basic information to display LCDs so the user knows what this script is doing
 */
void UpdateDisplay() {
    // Skip this if there are no acceptable lcds onboard
    if (displayLCDs.Count == 0) return;
}