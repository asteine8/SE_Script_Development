



// Raycast Config
int NUM_RUNS_PER_RAYCAST_TRACK = 3; // Run raycast tracking every x main method calls
double MAX_RAYCAST_RANGE = 3500; // Maximum distance a camera can raycast (meters)

double RAYCAST_NET_SEPERATION = 5; // meters
double MAX_CASTS_IN_NET = 10; // Maximum casts in detection scan net

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

// Raycast Internal Variables
int raycastTrackerCount = 0;
int camIndex = 0;
double timeSinceLastRaycast = 0; // In seconds
MyDetectedEntityInfo LastRaycastInfo;

// Raycasting Required Blocks
IMyRemoteControl ReferenceRemote;
List<IMyCameraBlock> RaycastingCameras = new List<IMyCameraBlock>();

// Debug
IMyBeacon Beacon;

public Program() {
    // Get blocks (Need error checking)
    ReferenceRemote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
    GridTerminalSystem.GetBlocksOfType(RaycastingCameras);

    // Get a beacon for debugging
    List<IMyBeacon> AllBeacons = new List<IMyBeacon>();
    GridTerminalSystem.GetBlocksOfType(AllBeacons);
    Beacon = AllBeacons[0];
    
    SetCameraRaycastingState(RaycastingCameras, true); // Enable camera raycasting

    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 0.6Hz
}

void Main(string arg) {
    timeSinceLastRaycast += Runtime.TimeSinceLastRun.TotalSeconds;
    raycastTrackerCount++; // Increment counter

    // Debug via beacon as console
    Beacon.CustomName = ("Running: " + LastRaycastInfo.IsEmpty().ToString() + "\n" + raycastTrackerCount.ToString());

    if (!LastRaycastInfo.IsEmpty() && raycastTrackerCount == NUM_RUNS_PER_RAYCAST_TRACK) { // Make sure that we have data
        MyDetectedEntityInfo RaycastInfo = TrackTarget(ReferenceRemote, LastRaycastInfo, RaycastingCameras, timeSinceLastRaycast);

        if (RaycastInfo.EntityId == LastRaycastInfo.EntityId) { // Check that we are tracking the same entity
            LastRaycastInfo = RaycastInfo;
        }

        timeSinceLastRaycast = 0; // Reset time since last track
        raycastTrackerCount = 0; // Reset counter
    }

    if (arg.ToLower() == "scan") {
        LastRaycastInfo = RaycastScanForTarget(ReferenceRemote, RaycastingCameras, MAX_RAYCAST_RANGE, 1, 1);
        timeSinceLastRaycast = 0; // Reset time since last track
        raycastTrackerCount = 0;
    }
}

/**
 * Casts a raycast scan in the current direction of the reference remote to the target Distance in a square grid with
 * netSeperation meters seperation
 */
MyDetectedEntityInfo RaycastScanForTarget(IMyRemoteControl REF_RC, List<IMyCameraBlock> Cameras, double targetDistance, double netSeperation, double maxCasts) {
    // Placeholder (Because actually doing what this function says its going to do is hard and I value my sanity)
    MyDetectedEntityInfo RaycastResult = Cameras[camIndex++].Raycast(targetDistance);
    if (camIndex == Cameras.Count) camIndex = 0;
    return RaycastResult;
}

// Raycast forwards by target distance from a raycasting camera
MyDetectedEntityInfo RaycastForward(IMyRemoteControl REF_RC, List<IMyCameraBlock> Cameras, double targetDistace) {
    MyDetectedEntityInfo RaycastResult = Cameras[camIndex++].Raycast(targetDistance);
    if (camIndex == Cameras.Count) camIndex = 0;
    return RaycastResult;
}

MyDetectedEntityInfo TrackTarget(IMyRemoteControl REF_RC, MyDetectedEntityInfo LastHit, List<IMyCameraBlock> Cameras, double timeSinceLastTrack) {
    Vector3D CurrentTargetPosition = LastHit.Position + (new Vector3D(LastHit.Velocity)) * timeSinceLastTrack;
    Vector3D CurrentPosition = REF_RC.GetPosition();

    // Calculate Distance of raycast
    double overshoot = 10; // How far to shoot the raycast past the predicted location
    double raycastDistance = (CurrentTargetPosition - CurrentPosition).Length() + overshoot;
    if (raycastDistance > MAX_RAYCAST_RANGE) raycastDistance = MAX_RAYCAST_RANGE;

    // Calculate final raycast target location
    CurrentTargetPosition = Vector3D.Normalize(CurrentTargetPosition-CurrentPosition) * raycastDistance + CurrentPosition;

    // Raycast to target (Returns an empty struct if it hits nothing)
    MyDetectedEntityInfo RaycastResult = Cameras[camIndex++].Raycast(CurrentTargetPosition);
    if (camIndex == Cameras.Count) camIndex = 0;
    return RaycastResult;
}

void SetCameraRaycastingState(List<IMyCameraBlock> Cameras, bool state) {
    for (int i = 0; i < Cameras.Count; i++) {
        Cameras[i].EnableRaycast = state;
    }
}