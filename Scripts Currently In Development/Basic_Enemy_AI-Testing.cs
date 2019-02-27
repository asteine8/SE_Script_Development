// Basic Enemy Ai for Ship Testing
// Rain42

/**
 * This script adds basic movement functionality with a direct attack function to nearly any ship
 * 
 * Ship Requirements (Minimum):
 *      -Remote Control
 *      -Forward Facing Camera (In the same direction as the remote)
 *      -Beacon
 *      -Programmable Block
 * 
 * Installation Instructions:
 *      1: Check to ensure that ship has all nessesary blocks
 *      2: Load code into programmable block
 *      3: Check and compile code
 *      4: Convert Ship to be owned by enemy npc faction
 *      5: Run
 */






// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// User defined variables
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

double MaxDetectionRange = 3250; // Maximum distance to start attacking
double RetreatRange = 1200;
double ProjectileVelocity = 100; // (m/s) [400=gatling,100=missile]
double Chance2BreakOff = 0.15; // Make this low but not too low
double Chance2Primary = 0.25; // Checks against this probability every time a horsefly coord has been reached

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// Actual Program Stuff
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


// Internal variables (change at your own risk)
double MaxEngageRange = 800; // Turret and bullet range (Could be extended for railgun ships)
double MinEngageRange = 200;
double DistanceToWaypointTolerance = 150; // meters
double TargetingAngleTolerance = 0.5;

double MAX_ANGULAR_VELOCITY = 0.75;
double RotationGain = 0.5;

double angleToTargetVector = 0;

List<IMyUserControllableGun> PrimaryWeapons = new List<IMyUserControllableGun>();
List<IMyCameraBlock> RaycastCameras = new List<IMyCameraBlock>();
List<IMyGyro> Gyros = new List<IMyGyro>();

List<IMyRemoteControl> Remotes = new List<IMyRemoteControl>();
IMyRemoteControl CurrentRemote;

IMyBeacon Beacon;

Vector3D LastKnownPlayerPosition;
Vector3D TargetPosition;

Random r = new Random();

int CurrentCameraIndex = 0;

bool isRunning = true;
bool hasPrimaryWeapon = true;

int AiState = 0;
/**
 * States:
 *      0: Passive State (No enemies detected inside of detection range)
 *      1: Primary Weapon Attack (Fire directly at the target)
 *      2: Horsefly tactics (Flies around the target)
 *      3: Retreat Mode (Moves outside of engagement range)
 */

public Program() {
    /*
        // Get all remotes (Will use remote at index 0)
        GridTerminalSystem.GetBlocksOfType(Remotes);
        if (Remotes.Count < 1) {
            isRunning = false;
            return;
        }
        CurrentRemote = Remotes[0];

        List<IMyBeacon> AllBeacons = new List<IMyBeacon>();
        GridTerminalSystem.GetBlocksOfType(AllBeacons);
        if (AllBeacons.Count < 1) {
            isRunning = false;
            return;
        }
        Beacon = AllBeacons[0];

        GridTerminalSystem.GetBlocksOfType(Gyros);
        if (Gyros.Count < 1) {
            isRunning = false;
            return;
        }

        // Get forward facing cameras for raycasting
        List<IMyCameraBlock> AllCameras = new List<IMyCameraBlock>();
        GridTerminalSystem.GetBlocksOfType(AllCameras);
        if (AllCameras.Count < 1) {
            isRunning = false;
            return;
        }
        for (int i = 0; i < AllCameras.Count; i++) {
            if (CurrentRemote.Orientation.Forward == AllCameras[i].Orientation.Forward) { // Pointed in the right direction
                RaycastCameras.Add(AllCameras[i]);
                AllCameras[i].EnableRaycast = true;
            }
        }
        if (RaycastCameras.Count < 1) {
            isRunning = false;
            return;
        }

        // Add weapons to primary weapon list if they are pointed in the same direction as the remote control
        List<IMyUserControllableGun> AllWeapons = new List<IMyUserControllableGun>();
        GridTerminalSystem.GetBlocksOfType(AllWeapons);
        for (int i = 0; i < AllWeapons.Count; i++) {
            if (CurrentRemote.Orientation.Forward == AllWeapons[i].Orientation.Forward) { // Pointed in the right direction
                PrimaryWeapons.Add(AllWeapons[i]);
            }
        }
    */
    if (!UpdateBlocks()) { // Cannot run
        isRunning = false;
    }

    // Setup Remote Control
    CurrentRemote.SpeedLimit = 100f; // Max
    CurrentRemote.ClearWaypoints(); // Reset
    // CurrentRemote.AddWaypoint(CurrentRemote.GetPosition()+ (new Vector3D(100,0,0)), "Initial Position");
    CurrentRemote.SetAutoPilotEnabled(false);

    CurrentRemote.FlightMode = FlightMode.OneWay; // Set flightmode to one way
    CurrentRemote.SetDockingMode(false); // Don't need to be careful
    CurrentRemote.SetCollisionAvoidance(false); // It sucks
    // Setup Beacon
    Beacon.Radius = 50000f; // Max range

    ShootWeapons(PrimaryWeapons, false); // Don't keep shooting for reasons

    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Update at 0.6Hz

    ApplyBlockStatesforAiStates(0);

}

void Main(string arg) {

    // AiState = 1;
    // ApplyBlockStatesforAiStates(1);
    if (isRunning) {
        try {
            UpdateBeacon();
            switch (AiState) {
                case 0: // Passive
                    if (CurrentRemote.GetNearestPlayer(out LastKnownPlayerPosition) == true) {
                        if (Vector3D.Distance(LastKnownPlayerPosition, CurrentRemote.GetPosition()) < MaxDetectionRange) {
                            AiState = 2; // Horsefly
                            ApplyBlockStatesforAiStates(AiState);
                        }
                    }
                    break;
                case 1: // Primary Attack
                    if (CurrentRemote.GetNearestPlayer(out LastKnownPlayerPosition) == true) {
                        if (Vector3D.Distance(LastKnownPlayerPosition, CurrentRemote.GetPosition()) > MaxDetectionRange) {
                            AiState = 0; // Go to passive, out of detection range
                            ApplyBlockStatesforAiStates(AiState);
                            ShootWeapons(PrimaryWeapons, false); // Don't keep shooting for reasons
                            break;
                        }

                        if (hasPrimaryWeapon == false) { // No primary weapon
                            AiState = 2; // Go to horsefly
                            ApplyBlockStatesforAiStates(AiState);
                            break;
                        }

                        if (Vector3D.Distance(CurrentRemote.GetPosition(),LastKnownPlayerPosition) > MaxEngageRange) {
                            AiState = 2; // Out of engagement range: go to horsefly mode
                            ApplyBlockStatesforAiStates(AiState);
                            ShootWeapons(PrimaryWeapons, false); // Don't keep shooting for reasons
                        }
                        else { // Fire mah lazor
                            CurrentRemote.SetAutoPilotEnabled(false); // Turn off the autopilot
                            if (RaycastCameras[CurrentCameraIndex].CanScan(LastKnownPlayerPosition)) {
                                MyDetectedEntityInfo raycastResult = RaycastCameras[CurrentCameraIndex].Raycast(LastKnownPlayerPosition);
                                CurrentCameraIndex = (CurrentCameraIndex == RaycastCameras.Count-1) ? 0 : CurrentCameraIndex + 1;

                                // We hit something and its an enemy!
                                if (raycastResult.IsEmpty() == false && raycastResult.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies) {
                                    Vector3D target = GetTargetWithBasicLeading(CurrentRemote, raycastResult.HitPosition.Value, new Vector3D(raycastResult.Velocity), ProjectileVelocity);
                                    angleToTargetVector = Math.Acos(Vector3D.Dot(Vector3D.Normalize(target - CurrentRemote.GetPosition()), Vector3D.Normalize(CurrentRemote.WorldMatrix.Forward)));

                                    TurnToVector(target, RotationGain, Gyros, CurrentRemote, 0, MAX_ANGULAR_VELOCITY);
                                    
                                    if (angleToTargetVector < TargetingAngleTolerance) {
                                        ShootWeapons(PrimaryWeapons, true);
                                    }
                                    else {
                                        ShootWeapons(PrimaryWeapons, false);
                                    }
                                }
                                else {
                                    TurnToVector(LastKnownPlayerPosition, RotationGain, Gyros, CurrentRemote, 0, MAX_ANGULAR_VELOCITY);
                                }
                            }
                            else {
                                TurnToVector(LastKnownPlayerPosition, RotationGain, Gyros, CurrentRemote, 0, MAX_ANGULAR_VELOCITY);
                            }
                            if (r.NextDouble() < Chance2BreakOff) { // Breakoff primary atack randomly
                                AiState = r.Next(2,4); // Either go to horsefly or retreat
                                ApplyBlockStatesforAiStates(AiState);
                                ShootWeapons(PrimaryWeapons, false); // Don't keep shooting for reasons
                            }
                        }
                        
                    }
                    else {
                        AiState = 0;
                        ApplyBlockStatesforAiStates(AiState);
                        ShootWeapons(PrimaryWeapons, false); // Don't keep shooting for reasons
                    }
                    break;
                case 2: // Horsefly
                    if (CurrentRemote.GetNearestPlayer(out LastKnownPlayerPosition) == true) {
                        if (Vector3D.Distance(LastKnownPlayerPosition, CurrentRemote.GetPosition()) > MaxDetectionRange) {
                            AiState = 0; // Passive
                            ApplyBlockStatesforAiStates(AiState);
                            break;
                        }

                        List<MyWaypointInfo> Waypoints = new List<MyWaypointInfo>();
                        CurrentRemote.GetWaypointInfo(Waypoints); // Get list of waypoints to make sure we are going to one
                        Echo(Waypoints.Count.ToString());
                        if (AtWaypoint(CurrentRemote, TargetPosition, DistanceToWaypointTolerance) || Waypoints.Count == 0 || Vector3D.Distance(TargetPosition, CurrentRemote.GetPosition()) > MaxDetectionRange) {
                            if (r.NextDouble() < Chance2Primary) {
                                AiState = r.Next(1,4); // Either go to horsefly or retreat
                                ApplyBlockStatesforAiStates(AiState);
                            }
                            else { // Stay in horsefly and assign new waypoint
                                Vector3D myPos = CurrentRemote.GetPosition();
                                Vector3D myRight = (r.Next(0,2) == 0) ? CurrentRemote.WorldMatrix.Right : CurrentRemote.WorldMatrix.Up; // Add some basic varience

                                Vector3D waypoint = Vector3D.Cross(myRight, LastKnownPlayerPosition - myPos);
                                waypoint = (Vector3D.Normalize(waypoint) * ( (r.NextDouble() * (MaxEngageRange - MinEngageRange)) + MinEngageRange)) + LastKnownPlayerPosition;

                                // Set waypoint into remote control
                                CurrentRemote.ClearWaypoints();
                                CurrentRemote.AddWaypoint(waypoint, "Target Location");
                                CurrentRemote.SetAutoPilotEnabled(true);

                                TargetPosition = waypoint;
                            }
                        }
                        if (Waypoints.Count > 0) {
                            CurrentRemote.SetAutoPilotEnabled(true);
                        }
                    }
                    else {
                        AiState = 0; // Passive
                        ApplyBlockStatesforAiStates(AiState);
                    }
                    break;
                case 3: // Retreat
                    if (CurrentRemote.GetNearestPlayer(out LastKnownPlayerPosition) == true) {
                        Vector3D MyPos = CurrentRemote.GetPosition();
                        Vector3D card = (r.Next(0,2) == 0) ? CurrentRemote.WorldMatrix.Right : CurrentRemote.WorldMatrix.Up; // Add some basic varience

                        Vector3D Rwaypoint = Vector3D.Cross(card, LastKnownPlayerPosition - MyPos);
                        Rwaypoint = (Vector3D.Normalize(Rwaypoint) * RetreatRange) + LastKnownPlayerPosition;

                        // Set waypoint into remote control
                        CurrentRemote.ClearWaypoints();
                        CurrentRemote.AddWaypoint(Rwaypoint, "Target Location");
                        CurrentRemote.SetAutoPilotEnabled(true);

                        AiState = 2; // Horsefly
                        ApplyBlockStatesforAiStates(AiState);

                        TargetPosition = Rwaypoint;
                    }
                    else {
                        AiState = 0; // Passive
                        ApplyBlockStatesforAiStates(AiState);
                    }
                    break;
            }
        }
        catch {
            if (!UpdateBlocks()) { // Cannot run
                isRunning = false;
            }
        }
    }
    else {
        Echo("Program not runnable");
    }
}

Vector3D GetTargetWithBasicLeading(IMyRemoteControl REF_REMOTE, Vector3D TargetPosition, Vector3D TargetVelocity, double ProjectileVelocity) {
    Vector3D CurrentPosition = REF_REMOTE.GetPosition();
    Vector3D CurrentVelocity = REF_REMOTE.GetShipVelocities().LinearVelocity;

    // Rough approximation (Not exact, but better than no leading at all: Should underlead)
    double timeToTarget = Vector3D.Distance(CurrentPosition, TargetPosition) / (ProjectileVelocity + ProjectOnVector(CurrentVelocity,TargetPosition-CurrentPosition) );

    return TargetPosition + (TargetVelocity * timeToTarget);
}

bool AtWaypoint(IMyRemoteControl REF_REMOTE, Vector3D waypoint, double tolerance) {
    double distance2Waypoint = Vector3D.Distance(REF_REMOTE.GetPosition(), waypoint);
    if (distance2Waypoint < tolerance) {
        return true;
    }
    else {
        return false;
    }
}

void TurnToVector(Vector3D TARGET, double GAIN, List<IMyGyro> Gyros, IMyRemoteControl REF_RC, double ROLLANGLE,double MAXANGULARVELOCITY) {
    //Ensures Autopilot Not Functional
    REF_RC.SetAutoPilotEnabled(false);
    Echo("Running Gyro Control Program");

    //Detect Forward, Up & Pos
    Vector3D ShipForward = REF_RC.WorldMatrix.Forward;
    Vector3D ShipUp = REF_RC.WorldMatrix.Up;
    Vector3D ShipPos = REF_RC.GetPosition();

    //Create And Use Inverse Quatinion                   
    Quaternion Quat_Two = Quaternion.CreateFromForwardUp(ShipForward, ShipUp);
    var InvQuat = Quaternion.Inverse(Quat_Two);
    Vector3D DirectionVector = Vector3D.Normalize(TARGET - ShipPos); //RealWorld Target Vector
    Vector3D RCReferenceFrameVector = Vector3D.Transform(DirectionVector, InvQuat); //Target Vector In Terms Of RC Block

    //Convert To Local Azimuth And Elevation
    double ShipForwardAzimuth = 0; double ShipForwardElevation = 0;
    Vector3D.GetAzimuthAndElevation(RCReferenceFrameVector, out ShipForwardAzimuth, out ShipForwardElevation);

    //Does Some Rotations To Provide For any Gyro-Orientation
    var RC_Matrix = REF_RC.WorldMatrix.GetOrientation();
    var Vector = Vector3.Transform((new Vector3D(ShipForwardElevation, ShipForwardAzimuth, ROLLANGLE)), RC_Matrix); //Converts To World

    for (int i = 0; i < Gyros.Count; i++) {
        var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(Gyros[i].WorldMatrix.GetOrientation()));  //Converts To Gyro Local

        //Applies To Scenario
        Gyros[i].Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y) * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z) * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        // Gyros[i].GyroOverride = true;
    }
}

void ApplyStateToGyroOverrides(List<IMyGyro> Gyros, bool state) {
    for (int i = 0; i < Gyros.Count; i++) {
        Gyros[i].GyroOverride = state;
    }
}

void ShootWeapons(List<IMyUserControllableGun> weapons, bool state) {
    for (int i = 0; i < weapons.Count; i++) {
        if (state) weapons[i].ApplyAction("Shoot_On");
        else weapons[i].ApplyAction("Shoot_Off");
    }
}

void ApplyBlockStatesforAiStates(int state) {
    switch(state) {
        case 0: // Passive
            CurrentRemote.SetAutoPilotEnabled(false);
            ApplyStateToGyroOverrides(Gyros, false);
            break;
        case 1: // Primary
            CurrentRemote.SetAutoPilotEnabled(false);
            ApplyStateToGyroOverrides(Gyros, true);
            break;
        case 2: // Horsefly
            // CurrentRemote.SetAutoPilotEnabled(true);
            ApplyStateToGyroOverrides(Gyros, false);
            break;
        case 3: // Retreat
            // CurrentRemote.SetAutoPilotEnabled(true);
            ApplyStateToGyroOverrides(Gyros, false);
            break;
    }
}

double ProjectOnVector(Vector3D vec, Vector3D guideVector) {
    if (Vector3D.IsZero(vec) || Vector3D.IsZero(guideVector)) {
        return 0;
    }
    return Vector3D.Dot(vec, guideVector) / guideVector.Length();
}

void UpdateBeacon() {
    string beaconText = "";
    switch (AiState) {
        case 0:
            beaconText = "Passive";
            break;
        case 1:
            beaconText = "Primary Attack";
            beaconText += "Angle2Target: " + angleToTargetVector.ToString("0.000");
            break;
        case 2:
            beaconText = "Horsefly";
            beaconText += "\n Distance2Target: " + Vector3D.Distance(CurrentRemote.GetPosition(),TargetPosition).ToString("0");
            break;
        case 3:
            beaconText = "Retreat";
            break;
    }
    Beacon.CustomName = beaconText;
}

bool UpdateBlocks() {

    int cIndex = 0;
    // Get all remotes (Will use remote at index 0)
    GridTerminalSystem.GetBlocksOfType(Remotes);
    if (Remotes.Count < 1) {
        return false;
    }
    while(cIndex < Remotes.Count) {
        if (Remotes[cIndex].IsFunctional && Remotes[cIndex].IsWorking) {
            CurrentRemote = Remotes[cIndex];
            break;
        }
        cIndex ++;
    }
    if (cIndex = Remotes.Count) {
        AllBlocksAvailible = false;
        return false;
    }


    cIndex = 0;
    List<IMyBeacon> AllBeacons = new List<IMyBeacon>();
    GridTerminalSystem.GetBlocksOfType(AllBeacons);
    if (AllBeacons.Count < 1) {
        AllBlocksAvailible = false;
        return false;
    }
    while(cIndex < AllBeacons.Count) {
        if (AllBeacons[cIndex].IsFunctional && AllBeacons[cIndex].IsWorking) {
            Beacon = AllBeacons[cIndex];
            break;
        }
        cIndex ++;
    }
    if (cIndex = AllBeacons.Count) {
        AllBlocksAvailible = false;
        return false;
    }


    GridTerminalSystem.GetBlocksOfType(Gyros);
    if (Gyros.Count < 1) {
        return false;
    }


    // Get forward facing cameras for raycasting
    List<IMyCameraBlock> AllCameras = new List<IMyCameraBlock>();
    GridTerminalSystem.GetBlocksOfType(AllCameras);
    if (AllCameras.Count < 1) {
        hasPrimaryWeapon = false;
    }
    for (int i = 0; i < AllCameras.Count; i++) {
        if (CurrentRemote.Orientation.Forward == AllCameras[i].Orientation.Forward && AllCameras.IsFunctional && AllCameras.IsWorking) { // Pointed in the right direction
            RaycastCameras.Add(AllCameras[i]);
            AllCameras[i].EnableRaycast = true;
        }
    }
    if (RaycastCameras.Count < 1) {
        hasPrimaryWeapon = false;
    }

    // Add weapons to primary weapon list if they are pointed in the same direction as the remote control
    List<IMyUserControllableGun> AllWeapons = new List<IMyUserControllableGun>();
    GridTerminalSystem.GetBlocksOfType(AllWeapons);
    for (int i = 0; i < AllWeapons.Count; i++) {
        if (CurrentRemote.Orientation.Forward == AllWeapons[i].Orientation.Forward) { // Pointed in the right direction
            PrimaryWeapons.Add(AllWeapons[i]);
        }
    }
    if (PrimaryWeapons.Count == 0) {
        hasPrimaryWeapon = false;
    }

    // Setup Remote Control
    CurrentRemote.SpeedLimit = 100f; // Max
    CurrentRemote.ClearWaypoints(); // Reset
    // CurrentRemote.AddWaypoint(CurrentRemote.GetPosition()+ (new Vector3D(100,0,0)), "Initial Position");
    CurrentRemote.SetAutoPilotEnabled(false);

    CurrentRemote.FlightMode = FlightMode.OneWay; // Set flightmode to one way
    CurrentRemote.SetDockingMode(false); // Don't need to be careful
    CurrentRemote.SetCollisionAvoidance(false); // It sucks
    // Setup Beacon
    Beacon.Radius = 50000f; // Max range

    ShootWeapons(PrimaryWeapons, false); // Don't keep shooting for reasons

    ApplyBlockStatesforAiStates(0);

    AiState = 0;
}