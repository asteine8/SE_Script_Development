// 1st Oder Aimbot.cs
// Rain42 11/21/18

/*
This script, when run, will attempt to lock onto a target by raycasting forward and around the forward raycast. If an object
is detected, this script will calculate a first order intercept using position and velocity and fire weapons in the block
group specified by the argument of the programmable block call. The velocity of the fastest weapon in the group will be used
to calculate the intercept. You may also want to note that the ship position is set to be at the remote location, so make
sure the the remote position isn't anywhere crazy and near the axis of the cockpit.

If the argument of the programmable block's run is a number, the script will calculate lead using the number as a projectile
velocity and will point at the intercept position but will not fire weapons (But this can be done manually while in this mode)

If another argument is recieved while tracking, the currently tracked entity will be used for the new block group/ projectile
speed tracking (You don't have to relock target)

Special arguments:
    "Stop": Stops tracking the current target (Deactivates gyro overrides and raycasting)
*/



// ++User modifiable variables++

double maxRaycastRange = 1600; // Will not raycast past this distance to help with preformance
double raycastSearchRadius = 24; // meters
double distanceBetweenSearchRaycasts = 8; // meters

double thresholdToFire = 1; // In degrees the threshold between target orientation and current orientation that is acceptable enough to fire weapons at
double maxAutofireRange = 805; // Will not fire weapons at target if farther than this number of meters away from target

int raycastPeriod = 2; // How many 10 ticks to wait until the next raycast (frequency = 6/raycastPeriod)
int gyroUpdatePeriod = 0; // How many 10 ticks to wait between gyroscope updates (frequency = 6/gyroUpdatePeriod)
int weaponUpdatePeriod = 6; // How many 10 ticks to wait between weapon updates (frequency = 6/weaponUpdatePeriod)

double rotationGain = 8; // Increase if ship is turning too slow
double rollGain = 2; // you don't need this so much
double maxAngularVelocity = 30; // keep in range [15,30]


// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//                         Please don't modify anything under here unless you know what you're doing
// ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++



// Block Definitions

List<IMyGyro> Gyros = new List<IMyGyro>();
List<IMyCameraBlock> Cameras = new List<IMyCameraBlock>(); // All Cameras
List<IMyCameraBlock> RaycastCameras = new List<IMyCameraBlock>(); // Only Cameras for raycasting

IMyBlockGroup GunGroup;
List<IMyUserControllableGun> Guns = new List<IMyUserControllableGun>();

IMyRemoteControl Remote;

// Variable Defininitions

int programState = 0;
/*
Program States:
    0: Idle, do nothing
    1: Raycast for target search
    2: Target locked, point at intercept (Continues to raycast to track target)
    3: Stop Tracking, reset to idle
*/

double projectileVelocity = 0; // m/s
Vector3D TargetIntercept; // Intercept point to aim at
Quaternion TargetOrientation; // What we're trying to rotate to

List<MyDetectedEntityInfo> TargetInfo; // Really don't need to store more than this...

int lastUsedCameraIndex = 0; // Index of camera last used to raycast (Start at 0 to advoid errors)

bool WeaponsHot = false; // If true weapons will be activated

int raycastCounter = 0;
int gyroCounter = 0;
int weaponsCounter = 0;

double programTime = 0;
double lastRaycastTime = 0;

int maxStoredRaycasts = 4; // max number of data point to use

// Init
public Program() {
    Runtime.UpdateFrequency = 0; // Start out without updates
    // Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz

    TargetInfo = new List<MyDetectedEntityInfo>(maxStoredRaycasts);

    List<IMyRemoteControl> Remotes = new List<IMyRemoteControl>();
    GridTerminalSystem.GetBlocksOfType(Remotes);
    Remote = Remotes[0]; // Grab First Remote Control

    GridTerminalSystem.GetBlocksOfType(Gyros);

    GridTerminalSystem.GetBlocksOfType(Cameras);
    for (int i = 0; i < Cameras.Count; i++) {
        if (Cameras[i].Orientation.Forward == Remote.Orientation.Forward) { // Only get forward facing cameras for raycasting
            RaycastCameras.Add(Cameras[i]);
        }
    }

    TargetIntercept = Vector3D.Zero; // Initialize to zero vector to advoid errors

    SetCamerasRaycastOn(true, RaycastCameras); // Enable raycasting right from the start to accululate charge

    TryEcho("Initialized Program");
}

void Main(string argument) {
    if (argument.ToLower() == "stop") {
        programState = 3; // Kill all and abort
    }
    if (programState != 0) {
        programTime += 0.16666667;
    }
    switch (programState) {
        case 0: // Idle, listen for commands
            int pVelocity;
            bool success = Int32.TryParse(argument, out pVelocity);
            if (success) { // String is a number, track without autofire
                WeaponsHot = false; // Weapons not on autofire
                projectileVelocity = (double)pVelocity;
                programState = 1; // Move to search for target;
                Runtime.UpdateFrequency = UpdateFrequency.Once; // Update once again at the next tick

                TryEcho("Initiating Tarcking without weapons hot\nProjectile velocity set to\n" + projectileVelocity.ToString("0.00"));
            }
            else { // String is NaN, track with autofire
                try {
                    WeaponsHot = true; // Enable autofire
                    GunGroup = GridTerminalSystem.GetBlockGroupWithName(argument.Trim());
                    GunGroup.GetBlocksOfType(Guns); // Get guns from group with argument name
                    projectileVelocity = GetMaxProjectileVelocityFromGroup(GunGroup);
                    
                    programState = 1; // Move to search for target;
                    Runtime.UpdateFrequency = UpdateFrequency.Once; // Update once again at the next tick

                    TryEcho("Initiating tracking with autofire on\nweapon group: " + argument.Trim() + " with velocity at\n" + projectileVelocity.ToString("0.00"));
                }
                catch (Exception e) { // Group DNE
                    TryEcho("Group DNE");
                    // Don't move on b/c this is not the group you're looking for
                }
            }
            break;
        case 1: // Raycast for target search
            MyDetectedEntityInfo RaycastResult;
            if (RaycastSearchForwards(Remote, RaycastCameras, maxRaycastRange, raycastSearchRadius, distanceBetweenSearchRaycasts, out RaycastResult)) {
                // Looks like we hit something, lets see what it is
                string TargetInfoString = "";
                TargetInfoString += "Hit a " + RaycastResult.Type.ToString() + " with name: " + RaycastResult.Name + "\n";
                TargetInfoString += "Relationship: " + RaycastResult.Relationship.ToString() + "\n";
                TargetInfoString += "Size: " + RaycastResult.BoundingBox.Size.ToString("0.0");
                TryEcho(TargetInfoString, true);

                TargetInfo = ShiftInRaycastData(TargetInfo, RaycastResult, maxStoredRaycasts);
                lastRaycastTime = programTime;

                programState = 2; // Move on to tracking
                SetGyroOverrides(true, Gyros); // Enable gyro overrides so we can point at target
                Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz since we have a target
            }
            else {
                TryEcho("You didn't hit anything...");
                programState = 3; // Reset program
                Runtime.UpdateFrequency = UpdateFrequency.Once; // Update once again at the next tick
            }
            break;
        case 2: // Target locked, point at intercept (Continues to raycast to track target)
            raycastCounter++;

            if (raycastCounter >= raycastPeriod) { // Update raycast tracking
                MyDetectedEntityInfo RResult = RaycastToMaintainLock(Remote, RaycastCameras, TargetInfo[0], maxRaycastRange);
                TryEcho("Raycasting: " + programTime.ToString("0.0"));
                if (RResult.IsEmpty()) { // Didn't hit anything or hit something other than the current target
                    TryEcho("Target Lock Lost", true);
                    programState = 3;
                    break;
                }
                else if (RResult.EntityId != TargetInfo[0].EntityId && RResult.EntityId != Me.EntityId) {
                    TryEcho("Hit another target", true);
                    programState = 3;
                    break;
                }
                TargetInfo = ShiftInRaycastData(TargetInfo, RResult, maxStoredRaycasts);
                lastRaycastTime = programTime;
                raycastCounter = 0;
            }

            gyroCounter++;
            if (gyroCounter >= gyroUpdatePeriod) { // Update gyroscopes
                Vector3D LastKnownTargetPosition = new Vector3D(TargetInfo[0].HitPosition.Value);
                double timeElapsedSinceLastKnown = Math.Abs(programTime-lastRaycastTime);
                Vector3D EstimatedCurrentPosition = LastKnownTargetPosition + (new Vector3D(TargetInfo[0].Velocity))*timeElapsedSinceLastKnown;
                Predict1stOrderIntercept(Remote, EstimatedCurrentPosition, TargetInfo[0].Velocity, projectileVelocity, out TargetIntercept);
                // TargetIntercept = TargetInfo[0].HitPosition.Value;
                TargetOrientation = CalculateQuaternionOrientation(Remote, TargetIntercept);
                TurnToQuaternion(TargetOrientation, Gyros, Remote, rotationGain, rollGain, maxAngularVelocity);
                gyroCounter = 0;
            }
            
            weaponsCounter++;
            if (weaponsCounter >= weaponUpdatePeriod) {
                double a = GetAngleToTarget(Remote, TargetIntercept);
                TryEcho("\nAngleToTarget = " + a.ToString("0.000"));
                if (a <= thresholdToFire && Vector3D.Distance(TargetIntercept, Remote.GetPosition()) <= maxAutofireRange && WeaponsHot) {
                    ShootGuns(Guns, true);
                }
                else ShootGuns(Guns, false);
                weaponsCounter = 0;
            }



            if (argument.Length != 0) { // We have an argument...do something different
                int pV;
                bool suc = Int32.TryParse(argument, out pV);
                if (suc) {
                    WeaponsHot = false; // Weapons not on autofire
                    projectileVelocity = (double)pV;
                }
                else {
                    WeaponsHot = true; // Enable autofire
                    GunGroup = GridTerminalSystem.GetBlockGroupWithName(argument.Trim());
                    GunGroup.GetBlocksOfType(Guns); // Get guns from group with argument name
                    projectileVelocity = GetMaxProjectileVelocityFromGroup(GunGroup);
                }
            }
            break;
        case 3: // Stop Tracking, reset to idle
            programState = 0;
            WeaponsHot = false;
            SetGyroOverrides(false, Gyros);
            ShootGuns(Guns, false);

            programTime = 0; // Reset program time
            lastRaycastTime = 0;
            Runtime.UpdateFrequency = 0; // Stop automatic updates (For preformance reasons)

            raycastCounter = 0;
            gyroCounter = 0;
            weaponsCounter = 0;
            TryEcho("Program Stopped", true);
            break;
    }
}

// Raycasts forwards to distance with radius and density at distance from remote position
// Note: Raycast is in a square box with "radius" being 1/2*width and 1/2*length
bool RaycastSearchForwards(IMyRemoteControl Ref_Controller, List<IMyCameraBlock> Cams, double distance, double radiusOfSearch, double raycastDensityAtDistance, out MyDetectedEntityInfo RaycastResult) {
    Vector3D ShipPosition = Ref_Controller.GetPosition();

    SetCamerasRaycastOn(true, Cams); // Enable raycasting

    List<Vector3D> Raycasts = new List<Vector3D>();
    Raycasts.Add(ShipPosition + Vector3D.Normalize(Ref_Controller.WorldMatrix.Forward)*distance); // Target of first raycast position set to index 0

    // Generate raycast position
    int raycastsPerD = (int)Math.Floor(2*radiusOfSearch/raycastDensityAtDistance); // Number of raycasts per dimension
    double raycastSpacing = 2*radiusOfSearch / (double)raycastsPerD;
    for (int x = 0; x < (raycastsPerD/2); x++) {
        for (int y = 0; y < (raycastsPerD/2); y++) {
            Vector3D RaycastTarget = Raycasts[0];
            if (x==0&&y==0) continue;
            Raycasts.Add(RaycastTarget + Vector3D.Normalize(Ref_Controller.WorldMatrix.Right)*raycastSpacing*x + Vector3D.Normalize(Ref_Controller.WorldMatrix.Up)*raycastSpacing*y);
            Raycasts.Add(RaycastTarget - Vector3D.Normalize(Ref_Controller.WorldMatrix.Right)*raycastSpacing*x + Vector3D.Normalize(Ref_Controller.WorldMatrix.Up)*raycastSpacing*y);
            Raycasts.Add(RaycastTarget + Vector3D.Normalize(Ref_Controller.WorldMatrix.Right)*raycastSpacing*x - Vector3D.Normalize(Ref_Controller.WorldMatrix.Up)*raycastSpacing*y);
            Raycasts.Add(RaycastTarget - Vector3D.Normalize(Ref_Controller.WorldMatrix.Right)*raycastSpacing*x - Vector3D.Normalize(Ref_Controller.WorldMatrix.Up)*raycastSpacing*y);
        }
    }

    // Raycast until we hit something
    for (int i = 0; i < Raycasts.Count; i++) {
        lastUsedCameraIndex = (lastUsedCameraIndex == Cams.Count-1) ? 0 : lastUsedCameraIndex + 1;
        RaycastResult = Cams[lastUsedCameraIndex].Raycast(Raycasts[i]);
        if (RaycastResult.IsEmpty() == false) { // We hit something!!!!!!!!! Go get em
            TryEcho("Target Hit! Tracking...");
            return true; // break the loop
        }
    }
    TryEcho("No targets hit");
    RaycastResult = new MyDetectedEntityInfo();
    return false; // We didn't hit anything :(
}

// Raycast to predicted position using a first order prediction algorithm (Target = center of bounding box)
MyDetectedEntityInfo RaycastToMaintainLock(IMyRemoteControl Ref_Controller, List<IMyCameraBlock> Cams, MyDetectedEntityInfo LastKnownInfo, double maxDistance) {
    Vector3D ShipPosition = Ref_Controller.GetPosition();
    Vector3D LastKnownTargetPosition = LastKnownInfo.Position;
    Vector3D LastKnownTargetVelocity = new Vector3D(LastKnownInfo.Velocity);

    double timeElapsedSinceLastKnown = Math.Abs(programTime-lastRaycastTime);

    Vector3D PredictedCurrentPosition = LastKnownTargetPosition + LastKnownTargetVelocity * timeElapsedSinceLastKnown;
    Vector3D RaycastTarget;

    if ((PredictedCurrentPosition - ShipPosition).Length() > maxDistance) { // Out of range
        RaycastTarget = ShipPosition + Vector3D.Normalize(PredictedCurrentPosition-ShipPosition)*maxDistance; // Just raycast in direction at max range
    }
    else { // In range
        RaycastTarget = PredictedCurrentPosition + Vector3D.Normalize(PredictedCurrentPosition-ShipPosition)*10; // Overextend raycast by 10 meters
    }

    lastUsedCameraIndex = (lastUsedCameraIndex == Cams.Count-1) ? 0 : lastUsedCameraIndex + 1;
    return Cams[lastUsedCameraIndex].Raycast(RaycastTarget); // Raycast at target
}

// Not really compatible with modded weapons (yet)
double GetMaxProjectileVelocityFromGroup(IMyBlockGroup Group) {
    List<IMySmallGatlingGun> GatlingGuns = new List<IMySmallGatlingGun>();
    List<IMySmallMissileLauncher> RocketLaunchers = new List<IMySmallMissileLauncher>();
    List<IMySmallMissileLauncherReload> ReloadableRocketLaunchers = new List<IMySmallMissileLauncherReload>();

    Group.GetBlocksOfType(GatlingGuns);
    Group.GetBlocksOfType(RocketLaunchers);
    Group.GetBlocksOfType(ReloadableRocketLaunchers);

    double MaxPV = 0;

    if (RocketLaunchers.Count > 0 || ReloadableRocketLaunchers.Count > 0) {
        MaxPV = 200; // m/s
    }
    else if (GatlingGuns.Count > 0) {
        MaxPV = 400; // m/s
    }
    return MaxPV;
}

double GetAngleToTarget(IMyRemoteControl Ref_Controller, Vector3D Target) {
    Vector3D ForwardVector = Vector3D.Normalize(Ref_Controller.WorldMatrix.Forward);
    Vector3D TargetVector = Vector3D.Normalize(Target - Ref_Controller.GetPosition());

    return 180*Math.Acos(Vector3D.Dot(ForwardVector,TargetVector))/Math.PI; // Convert to degrees
}

void SetCamerasRaycastOn(bool state, List<IMyCameraBlock> Cams) {
    for (int i = 0; i < Cams.Count; i++) {
        Cams[i].EnableRaycast = state;
    }
}

void SetGyroOverrides(bool state, List<IMyGyro> Gs) {
    for (int i = 0; i < Gs.Count; i++) {
        Gs[i].GyroOverride = state;
    }
}

void TryEcho(string str, bool append = false) {
    Echo(str);
    try { // Advoid LCD does not point to any actual block errors
        IMyTextPanel LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
        LCD.WritePublicText(str, append);
    }
    catch(Exception e) {
        Echo(e.Message); // Echo the error message
    }
}

List<MyDetectedEntityInfo> ShiftInRaycastData(List<MyDetectedEntityInfo> RaycastList, MyDetectedEntityInfo data, int MaxListLength) {
    if (RaycastList.Count == MaxListLength) { // remove last element if already at capacity
        RaycastList.RemoveAt(MaxListLength-1);
    }
    RaycastList.Insert(0,data);
    return RaycastList;
}

/* Modified Gyro Rotation Script:
    Base code from RDav.
    Modified to use quaternions instead of a target position
*/
void TurnToQuaternion(Quaternion TargetOrientation, List<IMyGyro> Gyros, IMyRemoteControl REF_RC, double GAIN, double RollGain, double MAXANGULARVELOCITY) {
    //Ensures Autopilot Not Functional
    REF_RC.SetAutoPilotEnabled(false);

    //Detect Forward, Up & Pos
    Vector3D ShipForward = REF_RC.WorldMatrix.Forward;
    Vector3D ShipUp = REF_RC.WorldMatrix.Up;
    Vector3D ShipPos = REF_RC.GetPosition();
    Quaternion Quat_Two = Quaternion.CreateFromForwardUp(ShipForward, ShipUp);
    var InvQuat = Quaternion.Inverse(Quat_Two);

    double ROLLANGLE = QuaternionToYawPitchRoll(Quaternion.Inverse(Quat_Two) * TargetOrientation).X; // Get roll angle to target quaternion

    //Create And Use Inverse Quatinion                   
    Vector3D DirectionVector = Vector3D.Transform(Vector3D.Forward, TargetOrientation); // Modified to use quaternion orientation
    Vector3D RCReferenceFrameVector = Vector3D.Transform(DirectionVector, InvQuat); //Target Vector In Terms Of RC Block

    //Convert To Local Azimuth And Elevation
    double ShipForwardAzimuth = 0; double ShipForwardElevation = 0;
    Vector3D.GetAzimuthAndElevation(RCReferenceFrameVector, out ShipForwardAzimuth, out ShipForwardElevation);

    for (int i = 0; i < Gyros.Count; i++) {
        //Does Some Rotations To Provide For any Gyro-Orientation
        var RC_Matrix = REF_RC.WorldMatrix.GetOrientation();
        var Vector = Vector3.Transform((new Vector3D(ShipForwardElevation, ShipForwardAzimuth, ROLLANGLE)), RC_Matrix); //Converts To World
        var TRANS_VECT = Vector3.Transform(Vector, Matrix.Transpose(Gyros[i].WorldMatrix.GetOrientation()));  //Converts To Gyro Local

        //Applies To Scenario
        Gyros[i].Pitch = (float)MathHelper.Clamp((-TRANS_VECT.X * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Yaw = (float)MathHelper.Clamp(((-TRANS_VECT.Y) * GAIN), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        Gyros[i].Roll = (float)MathHelper.Clamp(((-TRANS_VECT.Z) * RollGain), -MAXANGULARVELOCITY, MAXANGULARVELOCITY);
        // Gyros[i].GyroOverride = true;
    }
}

Vector3 QuaternionToYawPitchRoll(Quaternion quat) {
    double q0 = (double)quat.W;
    double q1 = (double)quat.X;
    double q2 = (double)quat.Y;
    double q3 = (double)quat.Z;

    double Roll = Math.Atan2(2*(q0*q1+q2*q3), 1 - 2*(q1*q1+q2*q2));
    double Pitch = Math.Asin(2*(q0*q2-q3*q1));
    double Yaw = Math.Atan2(2*(q0*q3+q1*q2), 1 - 2*(q2*q2+q3*q3));

    return new Vector3((float)Yaw, (float)Pitch, (float)Roll);
}

// Predicts a 1st Order intercept from target posistion and velocity
bool Predict1stOrderIntercept(IMyRemoteControl Ref_Controller, Vector3D TargetPosition, Vector3D TargetVelocity, double projectileSpeed, out Vector3D InterceptPosition) {
    Vector3D ShipPosistion = Ref_Controller.GetPosition();
    TargetVelocity = TargetVelocity - Ref_Controller.GetShipVelocities().LinearVelocity; // Modification for relativity
    if ((TargetPosition-ShipPosistion).Length() < 1) { // We're 1 meter away from our target, don't even bother to waste time on the calculations
        Echo("Too Bloody Close, aborting calculation");
        InterceptPosition = TargetPosition; // return current position so we still try to aim
        return false;
    }
    
    if (TargetVelocity.Length() <= 0.01) { // Target is not moving, projectile will intercept at current target posistion
        InterceptPosition = TargetPosition;
        return true; // "Prediction" was still a success
    }
    if (projectileSpeed <= 0.1) { // Projectile is reeeeeaaaaaaly slow. Just return the target position as intercept and pray
        InterceptPosition = TargetPosition;
        return false; // Five dollars says this will miss since the target is moving
    }

    // Now that we have a moving target a ways from the ship, we should calculate the intercept position

    // Calcuate the time to intercept position using das qwuadwatic formula
    Vector3D RelativeTargetPosition = TargetPosition - ShipPosistion;

    double t = 0;
    double a = Math.Pow(TargetVelocity.Length(), 2)-Math.Pow(projectileSpeed, 2);
    double b = 2*Vector3D.Dot(TargetVelocity, RelativeTargetPosition);
    double c = Math.Pow(RelativeTargetPosition.Length(), 2);

    // t = -b + Math.Sqrt(b^2-4*a*c); // use the (+) version
    t = -b - Math.Sqrt(Math.Pow(b,2)-4*a*c); // use the (-) version
    t /= 2*a;

    t = (t<0) ? 0 : t; // Check for negatives (would be bad)

    InterceptPosition = TargetPosition + TargetVelocity*t; // Use found time to get final intercept position

    // Debugging Stuff

    Echo("Intercept Function Debug line:");
    Echo("A:" + a.ToString("0.0") + " B:" + b.ToString("0.0") + " C:" + c.ToString("0.0"));
    Echo("t = " + t.ToString("0.0"));
    Echo(InterceptPosition.ToString("0.0"));

    return true;
}

// Checks for gravity and forces orientation for remote-down to be as close to gravity-down as possible
Quaternion CalculateQuaternionOrientation(IMyRemoteControl Ref_Controller, Vector3D Target) {
    Vector3D Forward = Vector3D.Normalize(Target - Ref_Controller.GetPosition());
    Vector3D Up;
    if (Ref_Controller.GetNaturalGravity().Length() < 0.05) { // Basicly no gravity, maintain current roll
        Up = Vector3D.Normalize(Ref_Controller.WorldMatrix.Up);
    }
    else { // We need to deal with gravity, try to keep ship down as down as possible
        Vector3D Left = Vector3D.Normalize(Vector3D.Cross(Forward, Ref_Controller.GetNaturalGravity()));
        Up = Vector3D.Cross(Forward, Left);
    }

    return Quaternion.CreateFromForwardUp(Forward, Up);
}

void ShootGuns(List<IMyUserControllableGun> Gunz, bool state) {
    for (int i = 0; i < Gunz.Count; i++) {
        if (state) Gunz[i].ApplyAction("Shoot_On");
        else Gunz[i].ApplyAction("Shoot_Off");
    }
}