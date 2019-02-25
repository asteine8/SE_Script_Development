






// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// User defined variables
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

double MaxDetectionRange = 2000; // Maximum distance to start attacking
double ProjectileVelocity = 400; // (m/s) [400=gatling,100=missile,]

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// Actual Program Stuff
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


// Internal variables (change at your own risk)
double MaxRaycastRange = 1000;
double MaxEngageRange = 800; // Turret and bullet range (Could be extended for railgun ships)
double DistanceToWaypointTolerance = 100; // meters
double TargetingAngleTolerance = 1;

double MAX_ANGULAR_VELOCITY = 2;
double RotationGain = 2;



List<IMyUserControllableGun> PrimaryWeapons = new List<IMyUserControllableGun>();
List<IMyCameraBlock> RaycastCameras = new List<IMyCameraBlock>();
List<IMyGyro> Gyros = new List<IMyGyro>();

List<IMyRemoteControl> Remotes = new List<IMyRemoteControl>();
IMyRemoteControl CurrentRemote;

Vector3D LastKnowPlayerPosition;
Vector3D TargetPosition;

bool isRunning = true;
Random r = new Random();

int CurrentCameraIndex = 0;

int AiState = 0;
/**
 * States:
 *      0: Passive State (No enemies detected inside of detection range)
 *      1: Primary Weapon Attack (Fire directly at the target)
 *      2: Horsefly tactics (Flies around the target)
 *      3: Retreat Mode (Moves outside of engagement range)
 */

public Program() {
    // Get all remotes (Will use remote at index 0)
    GridTerminalSystem.GetBlocksOfType(Remotes);
    CurrentRemote = Remotes[0];

    GridTerminalSystem.GetBlocksOfType(Gyros);

    // Add weapons to primary weapon list if they are pointed in the same direction as the remote control
    List<IMyUserControllableGun> AllWeapons = new List<IMyUserControllableGun>();
    GridTerminalSystem.GetBlocksOfType(AllWeapons);
    for (int i = 0; i < AllWeapons.Count; i++) {
        if (CurrentRemote.Orientation.Forward == AllWeapons[i].Orientation.Forward) { // Pointed in the right direction
            PrimaryWeapons.Add(AllWeapons[i]);
        }
    }

    // Setup Remote Contro
    CurrentRemote.SpeedLimit = 100f; // Max
    CurrentRemote.ClearWaypoints(); // Reset
    CurrentRemote.FlightMode = FlightMode.OneWay; // Set flightmode to one way
    CurrentRemote.SetDockingMode(false); // Don't need to be careful
    CurrentRemote.SetCollisionAvoidance(false); // It sucks


    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Update at 0.6Hz
}

void Main(string arg) {

    switch (AiState) {
        case 0: // Passive
            if (CurrentRemote.GetNearestPlayer(out LastKnowPlayerPosition) == true) {
                if (Vector3D.Distance(LastKnowPlayerPosition, CurrentRemote.GetPosition()) < MaxDetectionRange) {
                    AiState = r.Next(0,4);
                    ApplyBlockStatesforAiStates(AiState);
                }
            }
            break;
        case 1: // Primary Attack
            if (CurrentRemote.GetNearestPlayer(out LastKnowPlayerPosition) == true) {
                if (Vector3D.Distance(LastKnowPlayerPosition, CurrentRemote.GetPosition()) > MaxDetectionRange) {
                    AiState = 0; // Go to passive, out of detection range
                    ApplyBlockStatesforAiStates(AiState);
                    break;
                }

                if (Vector3D.Distance(CurrentRemote,LastKnowPlayerPosition) > MaxEngageRange) {
                    AiState = 2; // Out of engagement range: go to horsefly mode
                    ApplyBlockStatesforAiStates(AiState);
                }
                    
                else { // Fire mah lazor
                    CurrentRemote.SetAutoPilotEnabled(false); // Turn off the autopilot
                    if (RaycastCameras[CurrentCameraIndex].CanScan(LastKnowPlayerPosition)) {
                        IMyDetectedEntityInfo raycastResult = RaycastCameras[CurrentCameraIndex].Raycast(LastKnowPlayerPosition);
                        CurrentCameraIndex = (CurrentCameraIndex == RaycastCameras.Count-1) ? 0 : CurrentCameraIndex + 1;

                         // We hit something and its an enemy!
                        if (raycastResult.IsEmpty() == false && raycastResult.Relation == MyRelationsBetweenPlayerAndBlock.Enemies) {
                            Vector3D target = GetTargetWithBasicLeading(CurrentRemote, raycastResult.HitPosition.Value, Vector3D(raycastResult.Velocity));
                            double angleToTargetVector = Math.Acos(Vector3D.Dot(Vector3D.Normalize(target - CurrentRemote.GetPosition()), Vector3D.Normalize(CurrentRemote.WorldMatrix.Forward)));
                            if (angleToTargetVector < TargetingAngleTolerance) {
                                ShootWeapons(PrimaryWeapons, true);
                            }
                            else {
                                ShootWeapons(PrimaryWeapons, false);
                            }
                        }
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
            
            break;
        case 3: // Retreat
            break;
    }

}

Vector3D GetTargetWithBasicLeading(IMyRemoteControl REF_REMOTE, Vector3D TargetPosition, Vector3D TargetVelocity, double ProjectileVelocity) {
    Vector3D CurrentPosition = REF_REMOTE.GetPosition();
    Vector3D CurrentVelocity = REF_REMOTE.GetShipVelocities().LinearVelocity;

    // Rough approximation (Not exact, but better than no leading at all: Should underlead)
    double timeToTarget = Vector3D.Distance(CurrentPosition, TargetPosition) / (ProjectileVelocity + Vector3D.ProjectOnVector(CurrentVelocity, TargetPosition-CurrentPosition));

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
            CurrentRemote.SetAutoPilotEnabled(true);
            ApplyStateToGyroOverrides(Gyros, false);
            break;
        case 3: // Retreat
            CurrentRemote.SetAutoPilotEnabled(true);
            ApplyStateToGyroOverrides(Gyros, false);
            break;
    }
}