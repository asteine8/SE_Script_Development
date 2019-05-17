
// Requirements: Gyro, Camera, Remote Control, Programmable block

/**
 * Program States:
 *      0: Passive, no controls
 *      1: Tracking Target
 */

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// Program Config - If you need to tweak things, do it here
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

// Raycast Config
int NUM_RUNS_PER_RAYCAST_TRACK = 2; // Run raycast tracking every x main method calls
double MAX_RAYCAST_RANGE = 2000; // Maximum distance a camera can raycast (meters)

double RAYCAST_NET_SEPERATION = 5; // meters
double MAX_CASTS_IN_NET = 10; // Maximum casts in detection scan net

// Gyro Config
double GAIN = 5;
double MAX_ANGULAR_VELOCITY = 10;

// Weapon Config
double MUZZEL_VELOCITY = 200; // (m/s) [400=gatling,200=missile]
double MAX_DEGREES_TO_FIRE = 2.5; // How many degrees the ship can be pointed off from the target orientation to be able to fire

// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// Program Internals - Don't modify things under here
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

// Raycast Internal Variables
int raycastTrackerCount = 0;
int camIndex = 0;
double timeSinceLastRaycast = 0; // In seconds
MyDetectedEntityInfo LastRaycastInfo;

// General Internal Variables
int programState = 0;
bool programRunnable = true;

bool fireWeapons = false;
bool isShooting = false;

double angleToTarget = 0;

// Block Definitions
List<IMyGyro> Gyros = new List<IMyGyro>();
List<IMyCameraBlock> RaycastingCameras = new List<IMyCameraBlock>();
List<IMyUserControllableGun> PrimaryWeapons = new List<IMyUserControllableGun>();

IMyRemoteControl ReferenceRemote;

public Program() {
    if (AssignBlocks()) {
        Echo("Program Initialized");
        programRunnable = true;
    }
    else {
        Echo("Cannot run program");
        programRunnable = false;
    }

    Runtime.UpdateFrequency = UpdateFrequency.None; // Start off without updates
}

void Main(string arg) {
    // Handle Arguments
    arg = arg.ToUpper();
    if (arg == "SCAN") { // Scan forwards for a target (Enables tracking if it hits something)
        MyDetectedEntityInfo ScanResult = new MyDetectedEntityInfo();
        try {
            ScanResult = RaycastForward(ReferenceRemote, RaycastingCameras, MAX_RAYCAST_RANGE);
        }
        catch { // Raycasting failed due to a camera block not existing
            if (!AssignBlocks()) programRunnable = false; // Reassignment failed - stop executing
        }

        if (!ScanResult.IsEmpty() && ScanResult.Relationship == MyRelationsBetweenPlayerAndBlock.Enemies) { // Lets-a-go!
            LastRaycastInfo = ScanResult;
            ApplyStateToGyroOverrides(Gyros, true); // Enable gyro control

            timeSinceLastRaycast = 0; // Reset time since we now have a new valid target
            raycastTrackerCount = 0;

            programState = 1; // Track the target
            Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 0.6Hz
        }
        else {
            LastRaycastInfo = new MyDetectedEntityInfo();
        }
    }
    else if (arg == "OFF") { // Turn off tracking
        programState = 0; // Just goto passive
        ShootWeapons(PrimaryWeapons, false);
        ApplyStateToGyroOverrides(Gyros, false);
    }
    else if (arg == "AUTOFIRE_ON") { // Turn on autofire
        fireWeapons = true;
    }
    else if (arg == "AUTOFIRE_OFF") { // Turn off autofire
        fireWeapons = false;
    }
    else if (arg == "RESET_BLOCKS") { // Reassign blocks - renables program if blocks replaced
        programRunnable = AssignBlocks(); // If it succeeds, restart the program
    }
    else if (arg == "CONTROL_GYROS_ON") { // Turn on gyro control
        ApplyStateToGyroOverrides(Gyros, true);
    }
    else if (arg == "CONTROL_GYROS_OFF") { // Turn off gyro control
        ApplyStateToGyroOverrides(Gyros, false);
    }


    if (programRunnable) {
        if (LastRaycastInfo.IsEmpty()) programState = 0;

        switch (programState) {
            case 0: // Passive state
                Runtime.UpdateFrequency = UpdateFrequency.None; // Don't waste processing power on empty runs
                ShootWeapons(PrimaryWeapons, false);
                ApplyStateToGyroOverrides(Gyros, false);
                break;
            case 1: // Track target
                timeSinceLastRaycast += Runtime.TimeSinceLastRun.TotalSeconds; // Add time since last raycast
                raycastTrackerCount++; // Increment counter

                // Update raycast tracker
                if (raycastTrackerCount == NUM_RUNS_PER_RAYCAST_TRACK && !LastRaycastInfo.IsEmpty()) { // Time to raycast
                    MyDetectedEntityInfo RaycastResult = new MyDetectedEntityInfo();
                    try {
                        RaycastResult = TrackTarget(ReferenceRemote, LastRaycastInfo, RaycastingCameras, timeSinceLastRaycast);
                    }
                    catch { // Raycasting failed due to a camera block not existing
                        if (!AssignBlocks()) programRunnable = false; // Reassignment failed - stop executing
                    }

                    if (!RaycastResult.IsEmpty() && RaycastResult.EntityId == LastRaycastInfo.EntityId) { // Valid target
                        timeSinceLastRaycast = 0; // Reset time since we now have a new valid target

                        LastRaycastInfo = RaycastResult;
                    }
                    else if (RaycastResult.IsEmpty()) {
                        LastRaycastInfo = RaycastResult; // Stop false positives
                        ApplyStateToGyroOverrides(Gyros, false); // Stop gyro control since there is nothing to look at
                        ShootWeapons(PrimaryWeapons, false);
                    }
                    
                    raycastTrackerCount = 0; // Reset timer
                }


                // Do things if we actually have a target
                if (!LastRaycastInfo.IsEmpty()) {

                    // Point at target
                    Vector3D launchDirection = Get1stOrderLaunchVector(ReferenceRemote, LastRaycastInfo.HitPosition.Value, LastRaycastInfo.Velocity, MUZZEL_VELOCITY);
                    Vector3D targetPos = ReferenceRemote.GetPosition() + (launchDirection * 1000);
                    try {
                        GyroTurn6(targetPos, GAIN, Gyros, ReferenceRemote, 0, MAX_ANGULAR_VELOCITY);
                    }
                    catch { // A gyro is not availible
                        if (!AssignBlocks()) programRunnable = false; // Reassignment failed - stop executing
                    }


                    // Handle autofire function
                    if (fireWeapons && !isShooting) {
                        Vector3D ForwardRef = ReferenceRemote.WorldMatrix.Forward;
                        angleToTarget = GetAngleBetweenVectors(ForwardRef, launchDirection);
                        if (angleToTarget > MAX_DEGREES_TO_FIRE) {
                            ShootWeapons(PrimaryWeapons, false); // Don't shoot and waste ammo
                            isShooting = false;
                        }
                        else {
                            ShootWeapons(PrimaryWeapons, true);
                            isShooting = true;
                        }
                    }
                    if (!fireWeapons && isShooting) {
                        ShootWeapons(PrimaryWeapons, false);
                        isShooting = false;
                    }
                }
                break;
        }
    }
}

// Returns false if some blocks are not present
bool AssignBlocks() {

    List<IMyRemoteControl> AllRemotes = new List<IMyRemoteControl>();
    GridTerminalSystem.GetBlocksOfType(AllRemotes);
    for (int i = 0; i < AllRemotes.Count; i++) {
        if (AllRemotes[i].IsWorking && AllRemotes[i].IsFunctional) {
            ReferenceRemote = AllRemotes[i]; // Grab first availible remote
            break;
        }
        if (i == AllRemotes.Count - 1) {
            return false; // No remote availible, cannot run script
        }
    }
    Base6Directions.Direction RefForward = ReferenceRemote.Orientation.Forward; // Get the forward direction


    GridTerminalSystem.GetBlocksOfType(Gyros);
    if (Gyros.Count == 0) return false; // No gyros availible, cannot run script


    RaycastingCameras = new List<IMyCameraBlock>(); // Reset camera list to empty
    List<IMyCameraBlock> AllCameras = new List<IMyCameraBlock>();
    GridTerminalSystem.GetBlocksOfType(AllCameras);
    for (int i = 0; i < AllCameras.Count; i++) {
        // Check if working and pointed in the same direction as the remote
        if (AllCameras[i].IsFunctional && AllCameras[i].IsWorking & AllCameras[i].Orientation.Forward == RefForward) {
            RaycastingCameras.Add(AllCameras[i]);
        }
    }
    if (RaycastingCameras.Count == 0) return false; // We need cameras to raycast


    PrimaryWeapons = new List<IMyUserControllableGun>(); // Convert to empty list
    List<IMyUserControllableGun> AllGunz = new List<IMyUserControllableGun>();
    GridTerminalSystem.GetBlocksOfType(AllGunz);
    for (int i = 0; i < AllGunz.Count; i++) {
        if (AllGunz[i].IsFunctional && AllGunz[i].IsWorking && AllGunz[i].Orientation.Forward == RefForward) {
            PrimaryWeapons.Add(AllGunz[i]);
        }
    }

    SetCameraRaycastingState(RaycastingCameras, true); // Enable raycasting on cameras
    ShootWeapons(PrimaryWeapons, false);
    ApplyStateToGyroOverrides(Gyros, false);

    return true; // We have everything, return true :)
}

// Raycast forwards by target distance from a raycasting camera
MyDetectedEntityInfo RaycastForward(IMyRemoteControl REF_RC, List<IMyCameraBlock> Cameras, double targetDistance) {
    MyDetectedEntityInfo RaycastResult = Cameras[camIndex++].Raycast(targetDistance);
    if (camIndex == Cameras.Count) camIndex = 0;
    return RaycastResult;
}

// Continue tracking a detected target
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

// Set raycasting state for a camera list
void SetCameraRaycastingState(List<IMyCameraBlock> Cameras, bool state) {
    for (int i = 0; i < Cameras.Count; i++) {
        Cameras[i].EnableRaycast = state;
    }
}

/**
 * Calculates the first order launch vector
 * 
 * @param REF_RC    Reference Remote Control
 * @param St        Target Position
 * @param Vt        Target Velocity
 * @param Vf        Projectile speed
 * 
 * @return          The direction vector 
 */
Vector3D Get1stOrderLaunchVector(IMyRemoteControl REF_RC, Vector3D St, Vector3D Vt, double Vf) {
    Vector3D Sp = REF_RC.GetPosition(); // Initial projectile location
    Vector3D Vp = REF_RC.GetShipVelocities().LinearVelocity; // Projectile additive velocity

    double Spx = Sp.X; double Spy = Sp.Y; double Spz = Sp.Z;
    double Stx = St.X; double Sty = St.Y; double Stz = St.Z;
    double Vpx = Vp.X; double Vpy = Vp.Y; double Vpz = Vp.Z;
    double Vtx = Vt.X; double Vty = Vt.Y; double Vtz = Vt.Z;

    double a = Spx*Vpx - Stx*Vpx + Spy*Vpy - Sty*Vpy + Spz*Vpz - Stz*Vpz - Spx*Vtx + Stx*Vtx - Spy*Vty + Sty*Vty - Spz*Vtz + Stz*Vtz;
    double b = Math.Sqrt(
                    Math.Pow((Spy*Vpy - Sty*Vpy + Spz*Vpz - Stz*Vpz + Spx*(Vpx - Vtx) + Stx*(-Vpx + Vtx) - Spy*Vty + Sty*Vty - Spz*Vtz + Stz*Vtz),2)
                    + (Math.Pow(Spx,2) + Math.Pow(Spy,2) + Math.Pow(Spz,2) - 2*Spx*Stx + Math.Pow(Stx,2) - 2*Spy*Sty + Math.Pow(Sty,2) - 2*Spz*Stz + Math.Pow(Stz,2))
                    *(Math.Pow(Vf,2) - Math.Pow(Vpx,2) - Math.Pow(Vpy,2) - Math.Pow(Vpz,2) + 2*Vpx*Vtx - Math.Pow(Vtx,2) + 2*Vpy*Vty - Math.Pow(Vty,2) + 2*Vpz*Vtz - Math.Pow(Vtz,2))
                    );
    double c = (Math.Pow(Vf,2) + Math.Pow(Vpx,2) + Math.Pow(Vpy,2) + Math.Pow(Vpz,2) - 2*Vpx*Vtx + Math.Pow(Vtx,2) - 2*Vpy*Vty + Math.Pow(Vty,2) - 2*Vpz*Vtz + Math.Pow(Vtz,2));

    if (c==0) return Vector3D.Zero; // Unable to divide by 0

    double t1 = -((a - b) / c);
    double t2 = -((a + b) / c);

    double t;

    if (t1 < 0 && t2 < 0) return Vector3D.Zero; // Invalid: No targets availible

    if (t1 < 0) {
        t = t2;
    }
    else if (t2 < 0) {
        t = t1;
    }
    else { // Both positive roots
        t = (t1 < t2) ? t1 : t2; // Get the smaller of the two
        // t = (t1 < t2) ? t2 : t1; // Get the larger of the two
    }

    Vector3D VelLaunch = (St-Sp)/t + Vt - Vp;
    return Vector3D.Normalize(VelLaunch);
}

// Rdav's gyro control function - modified to accept a list of gyros
void GyroTurn6(Vector3D TARGET, double GAIN, List<IMyGyro> Gyros, IMyRemoteControl REF_RC, double ROLLANGLE,double MAXANGULARVELOCITY) {
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

double GetAngleBetweenVectors(Vector3D a, Vector3D b) {
    a = Vector3D.Normalize(a);
    b = Vector3D.Normalize(b);

    return Math.Acos(Vector3D.Dot(a,b)) * (180/Math.PI); // Return angle in degrees
}