// Attack Drone Script with Raycast Object Advoidance
// Phillia Steiner 8/1/18

/*
Drone should have:
    1x Forward facing camera
    1x Remote Control
    1x Programmable Block (Duh)
    Basic Gyros and Thrusters
    Hopefully some turrets and armor (This script doesn't auto-aim forward mounted weapons)
    Maybe some treasure

Note: Drone should be in NPC faction

Just plop this script into the programming block, save, and recompile and this little killing machine will go start killing things
 */

// Script moves drone into proximity of player and uses camera raycast to do basic collision advoidance.
// Script terminates if camera, remote, or programmable block are destroyed


// Block Objects

IMyRemoteControl Remote;
IMyCameraBlock RaycastCamera;

// List Objects

List<IMyRemoteControl> Remotes = new List<IMyRemoteControl>();
List<IMyCameraBlock> Cameras = new List<IMyCameraBlock>();
List<IMyProgrammableBlock> ProgBlocks = new List<IMyProgrammableBlock>();


// Vectors

Vector3D PlayerLocation;
Vector3D TargetLocation;
Vector3D PrevTargetLocation;
Vector3D CurrentPosistion;

bool IsFunctional = true; // While true, this drone is functional

double RaycastDistance = 1; // Camera only raycasts 800 meters for preformance reasons

double clearanceMultiplier = 1.5; // Scales up clearance from obstacles

double AtLocationRange = 50; // Drone is at a location if it is this range away from the posistion (meters)

bool CanUpdateTarget = true;

double RangeFromPlayer = 5; // Drone will not correct from its current course if current destination is 25m from nearest player


public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks (0.6 Hz)
    PrevTargetLocation = new Vector3D(10,-10,19);
}

void Main(string arg) {
    // Get block lists from GridTerminalSystem
    GridTerminalSystem.GetBlocksOfType(Remotes);
    GridTerminalSystem.GetBlocksOfType(Cameras);
    GridTerminalSystem.GetBlocksOfType(ProgBlocks);

    if (Remotes.Count == 0 || Cameras.Count == 0 || ProgBlocks.Count == 0) {
        // No blocks of type found
        IsFunctional = false;
        Echo("Error 404, block not found\nShutting down primary systems\nDiverting energy to defensive systems");

        Remote.SetAutoPilotEnabled(false); // Turn off autopilot
        Remote.ClearWaypoints(); // Wipe Waypoint Memory

        Runtime.UpdateFrequency = 0; // Don't update anymore
        return; // Kill everything
    }
    else {
        // Get first index and set it as our block of choice
        Remote = Remotes[0];
        RaycastCamera = Cameras[0];
        // We don't need to use the programmable block so there is no individual object for it
    }

    if (IsFunctional) {
        // Drone is functional, run primary program

        // Update Drone and Player posistions in Vector3D
        Remote.GetNearestPlayer(out PlayerLocation);
        CurrentPosistion = Remote.GetPosition();


        RaycastCamera.EnableRaycast = true; // Enable raycasting on camera
        MyDetectedEntityInfo RaycastInfo = RaycastCamera.Raycast(RaycastDistance); // Raycast

        if (Vector3D.Distance(CurrentPosistion, TargetLocation) < AtLocationRange) {
            // We are close enough to the locking target location, enable full target update capabilities
            CanUpdateTarget = true;
        }

        if (!RaycastInfo.IsEmpty()) {
            // Raycast hit something, we need to update our target posistion or we may run into something
            CanUpdateTarget = true;
        }

        if (CanUpdateTarget) {
            if (!RaycastInfo.IsEmpty() && Vector3D.Distance(RaycastInfo.HitPosition.Value, CurrentPosistion) < Vector3D.Distance(PlayerLocation, CurrentPosistion)) {
                // Raycast hit something, and its closer to us than the player

                Echo("Raycast Hit Something");

                Vector3D ObstacleCenter = RaycastInfo.Position; // Get center Vector of obstacle
                Vector3D ObstacleSize = RaycastInfo.BoundingBox.Size; // Get dimentions of bounding box

                double ObsiticalRadius = ObstacleSize.Length() / 2;

                // Get vector pair for cross product with orgin at the obstacle's bounding box center
                Vector3D v1 = CurrentPosistion - ObstacleCenter; // Vector from obstacle center to current posistion
                Vector3D v2 = RaycastInfo.HitPosition.Value - ObstacleCenter; // Vector from obstacle center to raycast hit position
                
                Vector3D NormalVector = Vector3D.Cross(v1, v2); // Vector at normal to plane defined by v1 and v2

                // Scale up normal vector so it clears the bounding box of the object
                NormalVector = NormalVector * (ObsiticalRadius/NormalVector.Length()) * clearanceMultiplier;

                TargetLocation = ObstacleCenter + NormalVector; // Get target location from up of normal vector and target center
           
                CanUpdateTarget = false; // Don't update target until we get to the computed location as to dodge the obstacle
            }

            else {
                if (Vector3D.Distance(TargetLocation, PlayerLocation) > RangeFromPlayer) {
                    TargetLocation = PlayerLocation;
                }
            }
        }

        if (PrevTargetLocation != TargetLocation && TargetLocation != Vector3D.Zero) {
            // Don't update if the target hasn't changed

            Remote.ClearWaypoints(); // Remove all current waypoints

            Remote.AddWaypoint(TargetLocation, "Target Location"); // Enter target coord into remote control

            Remote.SetAutoPilotEnabled(true); // Enable autopilot if it isn't already

            PrevTargetLocation = TargetLocation; // Update PrevTargetLocation
        }

    }
}