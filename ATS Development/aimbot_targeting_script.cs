#region User Variables

// +++++ General +++++
bool TARGET_NEUTRALS = false; // Can aim at neutrals
bool TARGET_FRIENDLYS = false; // Can aim at friendlies

double GAME_MAX_SPEED = 100; // Max Speed in map (Just put the fasted speed here) (m/s)

// +++++ Block Tags +++++
// Text that block names MUST contain to be tagged
const string REF_CONTROLLER_TAG = "<reference>";
const string TARG_SPEAKER_TAG = "<targ_speaker>";
const string TURRET_DESIGNATOR_TAG = "<designator>";
const string LCD_TAG = "<lcd>";

// +++++ Raycasting +++++
float RAYCAST_SCAN_DEGREE_DEVIATION = 0.2F; // Degree deviation between raycasts in a raycast scan. Set small for fighters!
int RAYCAST_SCAN_RADIUS = 2; // "Radius" of the square of raycast scans (ie: a radius of two creates a 3x3 raycast scan grid)
double MAX_RAYCAST_RANGE = 1000; // Maximum distance a camera can raycast (meters)
int RAYCASTING_PERIOD = 2; // How many calls to wait between raycasting

#endregion

#region Program Variables

// +++++ Blocks +++++
List<IMyCameraBlock> Cameras = new List<IMyCameraBlock>();
List<IMyLargeTurretBase> DesignatorTurrets = new List<IMyLargeTurretBase>();
List<IMyGyro> Gyros = new List<IMyGyro>();

IMyShipController ReferenceControl; bool HAS_REFERENCE_CONTROLLER = false;
IMySoundBlock TargetingSpeaker;

// +++++ General +++++
const double UPDATE_FREQUENCY = 0.167;
const double UPDATES_PER_SECOND = 6;

bool systemOperational = false; // The BIG one!

int blockUpdateCycleCount = 0;
const int CYCLES_FOR_BLOCK_UPDATE = 36;

bool TARGET_LOCK = false; // If we have a target lock
bool AIMBOT_ENABLED = false; // If we are doing gyro control or not

enum WEAPON_TYPE {GATLING, ROCKET};

// +++++ Raycasting +++++
MyDetectedEntityInfo targetInfo;
MyDetectedEntityInfo prevTargetInfo;
int raycastCycleCount = 0;
double timeSinceLastRaycast = 0;
bool RAYCASTING_ENABLED = false;
bool RAYCAST_POSSIBLE = false;
int camIndex = 0;

// +++++ Turret Tracking +++++
bool TURRET_TRACKING_ENABLED = false;
bool TURRET_TRACKING_POSSIBLE = false;

// +++++ Sound +++++
bool speakerAlertPlaying = false;
bool SOUND_ENABLED = true;
bool SOUND_AVAILABLE = false;

// +++++ Interface and Displays +++++
int displayCycleCount = 0;
const int MAX_DISPLAY_CYCLES = 4;
string[] wheelAnimationStates = {"|","/","-","\\"};

#endregion

#region Main Program

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Set update frequency to every 10 ticks
    registerBlocks();
}

public void Main(string arg) {
    // Do block update cycles
    if (blockUpdateCycleCount++ == CYCLES_FOR_BLOCK_UPDATE) {
        registerBlocks();
        blockUpdateCycleCount = 0;
    }
    
    // Part arguments
    if (arg.Length > 0) {
        // Make lowercase for case insensivitivy
        parseArgument(arg.ToLower());
    }

    // Do Raycast Tracking
    if (RAYCASTING_ENABLED && RAYCAST_POSSIBLE) {
        runRaycasting();
    }

    // Do Turret Tracking
    if (TURRET_TRACKING_ENABLED && TURRET_TRACKING_POSSIBLE) {
        updateTurretTargetInfo();
    }

    // Update speaker block
    if (SOUND_AVAILABLE) {
        updateSound();
    }

    // Update echos to programmable block 'display'
    updateEchos();
}

void parseArgument(string arg) {
    switch(arg) {
        case "aimbot_enable":
            AIMBOT_ENABLED = true;
            break;
        case "aimbot_disable":
            AIMBOT_ENABLED = false;
            break;
        case "aimbot_toggle":
            AIMBOT_ENABLED = (AIMBOT_ENABLED) ? false : true;
            break;

        case "tracking_turrets":
            if (getFirstDetectedValidTarget()) {
                RAYCASTING_ENABLED = false;
                TURRET_TRACKING_ENABLED = true;
                TARGET_LOCK = true;
            }
            break;
        case "tracking_raycast":
            targetInfo = RaycastScanForTarget(Cameras, RAYCAST_SCAN_DEGREE_DEVIATION, RAYCAST_SCAN_RADIUS);
            if (targetInfo.IsEmpty() == false && isValidTarget(targetInfo)) { // Only enable tracking if a valid targetInfo is locked
                RAYCASTING_ENABLED = true;
                TURRET_TRACKING_ENABLED = false;
                TARGET_LOCK = true;
                timeSinceLastRaycast = 0;
            }
            break;
        case "tracking_disable":
            RAYCASTING_ENABLED = false;
            TURRET_TRACKING_ENABLED = false;
            TARGET_LOCK = false;
            break;

        case "aimbot_missiles":
            break;
        case "aimbot_gatlings":
            break;

        case "sound_enable":
            SOUND_ENABLED = true;
            break;
        case "sound_disable":
            SOUND_ENABLED = false;
            break;
        case "sound_toggle":
            SOUND_ENABLED = (SOUND_ENABLED) ? false : true;
            break;
    }
}

#endregion

#region Register Blocks

void registerBlocks() {
    // Get Reference Control Block
    bool foundReference = false;
    List<IMyShipController> Controllers = new List<IMyShipController>();
    GridTerminalSystem.GetBlocksOfType(Controllers);
    foreach(IMyShipController Controller in Controllers) {
        if (Controller.CustomName.ToLower().Contains(REF_CONTROLLER_TAG)) {
            ReferenceControl = Controller;
            foundReference = true;
            break;
        }
    }
    if (!foundReference) HAS_REFERENCE_CONTROLLER = false;
    else HAS_REFERENCE_CONTROLLER = true;

    // Register Cameras
    Base6Directions.Direction Ref_Forward = ReferenceControl.Orientation.Forward;
    List<IMyCameraBlock> AllCameras = new List<IMyCameraBlock>();
    Cameras = new List<IMyCameraBlock>(); // Reset camera list
    GridTerminalSystem.GetBlocksOfType(AllCameras);
    foreach(IMyCameraBlock Camera in AllCameras) { // Only get forward facing cameras
        if (Camera.Orientation.Forward == Ref_Forward) {
            Cameras.Add(Camera);
        }
    }
    if (Cameras.Count >= 0) {
        RAYCAST_POSSIBLE = true;
        SetCameraRaycastingState(Cameras, true); // Enable raycasting on all availible cameras
    }
    else RAYCAST_POSSIBLE = false;

    // Get Targeting Speaker Block
    bool foundSpeaker = false;
    List<IMySoundBlock> Speakers = new List<IMySoundBlock>();
    GridTerminalSystem.GetBlocksOfType(Speakers);
    foreach(IMySoundBlock Speaker in Speakers) {
        if (Speaker.CustomName.ToLower().Contains(TARG_SPEAKER_TAG)) {
            TargetingSpeaker = Speaker;
            foundSpeaker = true;
            break;
        }
    }
    if (foundSpeaker) SOUND_AVAILABLE = true;
    else SOUND_AVAILABLE = false;

    // Get Targeting Designator Turret(s)
    DesignatorTurrets = new List<IMyLargeTurretBase>(); // Reset Turret List
    List<IMyLargeTurretBase> Turrets = new List<IMyLargeTurretBase>();
    GridTerminalSystem.GetBlocksOfType(Turrets);
    foreach(IMyLargeTurretBase Turret in Turrets) {
        if (Turret.CustomName.ToLower().Contains(TURRET_DESIGNATOR_TAG)) {
            DesignatorTurrets.Add(Turret);
        }
    }
    if (DesignatorTurrets.Count > 0) TURRET_TRACKING_POSSIBLE = true;
    else TURRET_TRACKING_POSSIBLE = false;

    // Get gyroscopes
    GridTerminalSystem.GetBlocksOfType(Gyros);

    // Check for if system is operational
    if (foundReference && Gyros.Count > 0) {
        if (TURRET_TRACKING_POSSIBLE || RAYCAST_POSSIBLE) {
            systemOperational = true;
        }
        else systemOperational = false;
    }
    else systemOperational = false;
}

#endregion

#region Raycast Tracking

void runRaycasting() {
    if (raycastCycleCount > RAYCASTING_PERIOD) {
        MyDetectedEntityInfo RaycastResult = TrackTarget(ReferenceControl, targetInfo, Cameras, timeSinceLastRaycast);
        // Check Basic Validity
        if (RaycastResult.IsEmpty() == false && isValidTarget(RaycastResult)) {
            // Ensure that the new hit is the same target
            if (RaycastResult.EntityId == targetInfo.EntityId) {
                prevTargetInfo = targetInfo;
                targetInfo = RaycastResult;
            }
            // Not the same target... stop tracking
            else {
                RAYCASTING_ENABLED = false;
                TARGET_LOCK = false;
            }
        }
        // Didn't hit anything... stop tracking
        else {
            RAYCASTING_ENABLED = false;
            TARGET_LOCK = false;
        }

        timeSinceLastRaycast = 0;
        raycastCycleCount = 0;
    }

    timeSinceLastRaycast += UPDATE_FREQUENCY;
    raycastCycleCount++;
}

Vector3D estimateTargetPosition(MyDetectedEntityInfo targetInfo, double timeSinceLastTrack) {
    return new Vector3D(
            targetInfo.Position + (targetInfo.Velocity * (float)timeSinceLastTrack)
            );
}

/**
 * Casts a raycast scan in the current direction of the reference remote to the target Distance in a square grid with
 * degreeDeviation angles between casts
 */
MyDetectedEntityInfo RaycastScanForTarget(List<IMyCameraBlock> Cameras, float degreeDeviation, int gridRadius) {
    // Stab forward first to see if we can skip a grid scan
    MyDetectedEntityInfo RaycastResult = Cameras[camIndex++].Raycast(MAX_RAYCAST_RANGE);
    if (camIndex == Cameras.Count) camIndex = 0;
    if (!RaycastResult.IsEmpty() && isValidTarget(RaycastResult)) return RaycastResult;

    // Scan in a grid
    for (int i = -gridRadius; i <= gridRadius; i++) {
        for (int j = -gridRadius; i <= gridRadius; i++) {
            if (i==0 && j==0) continue; // Already did forward scan
            RaycastResult = Cameras[camIndex++].Raycast(MAX_RAYCAST_RANGE, i*degreeDeviation, j*degreeDeviation);
            if (camIndex == Cameras.Count) camIndex = 0;
            if (!RaycastResult.IsEmpty()) {
                if (isValidTarget(RaycastResult)) return RaycastResult;
            }
        }
    }

    // Should return an empty raycast if this fails
    return RaycastResult;
}

// Raycast forwards by target distance from a raycasting camera
MyDetectedEntityInfo RaycastForward(List<IMyCameraBlock> Cameras, double targetDistance) {
    MyDetectedEntityInfo RaycastResult = Cameras[camIndex++].Raycast(targetDistance);
    if (camIndex == Cameras.Count) camIndex = 0;
    return RaycastResult;
}

// Attempts to hit target again by appling first order motion prediction
MyDetectedEntityInfo TrackTarget(IMyShipController REF_RC, MyDetectedEntityInfo LastHit, List<IMyCameraBlock> Cameras, double timeSinceLastTrack) {
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

bool isValidTarget(MyDetectedEntityInfo entity) {
    // Only track grids
    switch(entity.Type) {
        case MyDetectedEntityType.SmallGrid:
	    case MyDetectedEntityType.LargeGrid:
            break;
        default:
            return false;
    }

    // Check Relations
    switch(entity.Relationship) {
        case MyRelationsBetweenPlayerAndBlock.Owner:
        case MyRelationsBetweenPlayerAndBlock.Friends:
        case MyRelationsBetweenPlayerAndBlock.FactionShare:
            if (TARGET_FRIENDLYS) break;
            else return false;
        case MyRelationsBetweenPlayerAndBlock.Neutral:
            if (TARGET_NEUTRALS) break;
            else return false;
        case MyRelationsBetweenPlayerAndBlock.NoOwnership:
        case MyRelationsBetweenPlayerAndBlock.Enemies:
            break;
    }

    return true;
}

#endregion

#region Turret Tracking

bool getFirstDetectedValidTarget() {
    // Just get info from first turret that detects a target
    foreach(IMyLargeTurretBase Turret in DesignatorTurrets) {
        if (Turret.HasTarget) {
            if (isValidTurretTarget(Turret.GetTargetedEntity())) {
                targetInfo = Turret.GetTargetedEntity();
                return true;
            }
        }
    }
    return false;
}

void updateTurretTargetInfo() {
    bool foundTarget = false;
    // Just get info from first turret that detects our target
    foreach(IMyLargeTurretBase Turret in DesignatorTurrets) {
        if (Turret.HasTarget) {
            // Check if same target
            if (Turret.GetTargetedEntity().EntityId == targetInfo.EntityId) {
                prevTargetInfo = targetInfo;
                targetInfo = Turret.GetTargetedEntity();
                foundTarget = true;
                break;
            }
        }
        Turret.ResetTargetingToDefault();
    }

    if (!foundTarget) {
        TARGET_LOCK = false;
        TURRET_TRACKING_ENABLED = false;
    }
}

bool isValidTurretTarget(MyDetectedEntityInfo entity) {
    // Check Relations
    switch(entity.Relationship) {
        case MyRelationsBetweenPlayerAndBlock.Owner:
        case MyRelationsBetweenPlayerAndBlock.Friends:
        case MyRelationsBetweenPlayerAndBlock.FactionShare:
            if (TARGET_FRIENDLYS) break;
            else return false;
        case MyRelationsBetweenPlayerAndBlock.Neutral:
            if (TARGET_NEUTRALS) break;
            else return false;
        case MyRelationsBetweenPlayerAndBlock.NoOwnership:
        case MyRelationsBetweenPlayerAndBlock.Enemies:
            break;
    }

    return true;
}

#endregion

#region targeting_calculations
/*
    Whip's targeting functions. Some edits were made to adapt the code to my aplication but the bulk of the mathy
    stuff is his. Boy am I glad that Whip made this, otherwise I would have never come up with a decent targeting
    function. X_X. All credits therefore go to Whip for this region
*/
    Vector3D GetInterceptPoint(
            IMyShipController REF_CONTROLLER, 
            MyDetectedEntityInfo currentTargetInfo,
            MyDetectedEntityInfo lastTargetInfo,
            WEAPON_TYPE weaponType,
            double muzzleVelocity)
    {
        Vector3D shooterVelocity = REF_CONTROLLER.GetShipVelocities().LinearVelocity;
        Vector3D shooterPosition = REF_CONTROLLER.GetPosition();
        Vector3D targetVelocity = new Vector3D(currentTargetInfo.Velocity);
        Vector3D targetPosition = currentTargetInfo.Position;
        Vector3D lastTargetVelocity = new Vector3D(lastTargetInfo.Velocity);


        if (weaponType == WEAPON_TYPE.ROCKET) {
            return CalculateMissileInterceptPoint(
                    muzzleVelocity,
                    UPDATES_PER_SECOND,
                    shooterVelocity,
                    shooterPosition,
                    targetVelocity,
                    targetPosition,
                    lastTargetVelocity
            );
        }
        else {
            return CalculateProjectileInterceptPoint(
                    muzzleVelocity,
                    UPDATES_PER_SECOND,
                    shooterVelocity,
                    shooterPosition,
                    targetVelocity,
                    targetPosition,
                    lastTargetVelocity
            );
        }
    }

    Vector3D CalculateProjectileInterceptPoint(
            double projectileSpeed,
            double updateFrequency,
            Vector3D shooterVelocity,
            Vector3D shooterPosition,
            Vector3D targetVelocity,
            Vector3D targetPosition,
            Vector3D lastTargetVelocity)
    {
        double temp = 0;
        return CalculateProjectileInterceptPoint(
                projectileSpeed,
                updateFrequency,
                shooterVelocity,
                shooterPosition,
                targetVelocity,
                targetPosition,
                lastTargetVelocity,
                out temp);
    }

    Vector3D CalculateProjectileInterceptPoint(
            double projectileSpeed,
            double updateFrequency,
            Vector3D shooterVelocity,
            Vector3D shooterPosition,
            Vector3D targetVelocity,
            Vector3D targetPosition,
            Vector3D lastTargetVelocity,
            out double timeToIntercept)
    {
        timeToIntercept = -1;

        var directHeading = targetPosition - shooterPosition;
        var directHeadingNorm = Vector3D.Normalize(directHeading);
        var distanceToTarget = Vector3D.Dot(directHeading, directHeadingNorm);

        var relativeVelocity = targetVelocity - shooterVelocity;

        var parallelVelocity = relativeVelocity.Dot(directHeadingNorm) * directHeadingNorm;
        var normalVelocity = relativeVelocity - parallelVelocity;
        var diff = projectileSpeed * projectileSpeed - normalVelocity.LengthSquared();
        if (diff < 0)
            return targetPosition;

        var projectileForwardSpeed = Math.Sqrt(diff);
        var projectileForwardVelocity = projectileForwardSpeed * directHeadingNorm;
        timeToIntercept = distanceToTarget / projectileForwardSpeed;

        var interceptPoint = shooterPosition + (projectileForwardVelocity + normalVelocity) * timeToIntercept;
        var targetAcceleration = updateFrequency * (targetVelocity - lastTargetVelocity);

        /*
            * We return here if we are at or over the max speed as predicting acceleration becomes an exercise in folly
            * as the solution becomes numerical and not analytical. We also return if acceleration is really close to
            * zero for obvious reasons.
            */
        if (targetVelocity.LengthSquared() >= GAME_MAX_SPEED * GAME_MAX_SPEED || Vector3D.IsZero(targetAcceleration, 1e-3))
            return interceptPoint;

        /*
            * Getting our time to critcal point where we hit the speed cap.
            * vf = vi + a*t
            * (vf - vi) / a
            */
        var velocityInAccelDirn = VectorMath.Projection(targetVelocity, targetAcceleration).Length() * Math.Sign(Vector3D.Dot(targetVelocity, targetAcceleration));
        var timeToSpeedCap = (GAME_MAX_SPEED - velocityInAccelDirn) / targetAcceleration.Length();

        /*
            * This is our estimate adding on the displacement due to the target acceleration UNTIL
            * it hits the speed cap.
            * vf^2 = vi^2 + 2*a*d
            * d = v * t + .5 * a * t^2
            */
        var timeAcceleration = Math.Min(timeToSpeedCap, timeToIntercept);
        var timePostAcceleration = timeToIntercept - timeAcceleration;
        var adjustedInterceptPoint = interceptPoint + 0.5 * targetAcceleration * timeAcceleration * timeAcceleration;
        var parallelAccelerationRatio = 1; //Math.Abs(VectorMath.CosBetween(targetVelocity, targetAcceleration));
        return (1 - parallelAccelerationRatio) * interceptPoint + parallelAccelerationRatio * adjustedInterceptPoint;
    }

    Vector3D CalculateMissileInterceptPoint(
            double projectileSpeed,
            double updateFrequency,
            Vector3D shooterVelocity,
            Vector3D shooterPosition,
            Vector3D targetVelocity,
            Vector3D targetPosition,
            Vector3D lastTargetVelocity)
    {
        double interceptTimeEstimate = 0.0;
        var firstInterceptGuess = CalculateProjectileInterceptPoint(
            projectileSpeed,
            updateFrequency,
            Vector3D.Zero,
            shooterPosition,
            targetVelocity,
            targetPosition,
            lastTargetVelocity,
            out interceptTimeEstimate);

        /*
            * In this method, we use two empirical regression equations to predict how missiles will
            * behave once they hit the speed cap. One for average velocity and one for lateral displacement.
            */
        var forwardDirection = firstInterceptGuess - shooterPosition;
        var lateralShooterVelocity = VectorMath.Rejection(shooterVelocity, forwardDirection);
        var forwardShooterVelocity = shooterVelocity - lateralShooterVelocity;
        var lateralShooterSpeed = lateralShooterVelocity.Length();
        var forwardShooterSpeed = forwardShooterVelocity.Length() * Math.Sign(forwardShooterVelocity.Dot(forwardDirection));
        var averageMissileVelocity = CalculateMissileAverageVelocity(forwardShooterSpeed, lateralShooterSpeed, interceptTimeEstimate);
        var displacement = CalculateMissileLateralDisplacement(forwardShooterSpeed, lateralShooterSpeed, interceptTimeEstimate);
        var firstDisplacementVec = lateralShooterSpeed == 0 ? Vector3D.Zero : -displacement * lateralShooterVelocity / lateralShooterSpeed;
        return firstInterceptGuess + firstDisplacementVec;
    }

    //Whip's CalculateMissileLateralDisplacement Method v3 - 5/4/18
    #region Empirical Curve Fits
    /*
    * These nasty bastards were compiled by numerically simulating the behavior of missiles under differing initial
    * conditions and then using regression to interpolate missile flight characteristics for given inputs.
    */
    double[] coeffs = new double[0];
    double[] coeffs800 = new double[] { 8.273620e-07, -4.074818e-04, -1.664885e-03, -4.890871e-05, 5.584003e-01, -1.036071e-01 };
    double[] coeffs700 = new double[] { 8.345401e-07, -4.074575e-04, -1.664838e-03, -4.894641e-05, 5.583784e-01, -1.036277e-01 };
    double[] coeffs600 = new double[] { 8.574764e-07, -4.073949e-04, -1.664709e-03, -4.901365e-05, 5.583180e-01, -1.036942e-01 };
    double[] coeffs500 = new double[] { 9.285435e-07, -4.072316e-04, -1.664338e-03, -4.918805e-05, 5.581436e-01, -1.039112e-01 };
    double[] coeffs400 = new double[] { 1.095375e-06, -4.068213e-04, -1.663140e-03, -5.029227e-05, 5.576205e-01, -1.044476e-01 };
    double[] coeffs300 = new double[] { 1.667607e-06, -4.056386e-04, -1.659285e-03, -5.870287e-05, 5.559914e-01, -1.062660e-01 };
    double[] coeffs200 = new double[] { 3.244393e-06, -4.037420e-04, -1.646552e-03, -9.560673e-05, 5.508962e-01, -1.113855e-01 };
    double[] coeffs100 = new double[] { 8.703641e-06, -4.043099e-04, -1.600068e-03, -3.163374e-04, 5.347254e-01, -1.310425e-01 };
    double[] coeffs050 = new double[] { 1.715572e-05, -4.076979e-04, -1.520712e-03, -7.353804e-04, 5.084648e-01, -1.620291e-01 };

    double CalculateMissileLateralDisplacement(double forwardVelocity, double lateralVelocity, double timeToIntercept = 4)
    {
        if (timeToIntercept > 4)
            coeffs = coeffs800;
        else if (timeToIntercept > 3.5)
            coeffs = coeffs700;
        else if (timeToIntercept > 3)
            coeffs = coeffs600;
        else if (timeToIntercept > 2.5)
            coeffs = coeffs500;
        else if (timeToIntercept > 2)
            coeffs = coeffs400;
        else if (timeToIntercept > 1.5)
            coeffs = coeffs300;
        else if (timeToIntercept > 1)
            coeffs = coeffs200;
        else if (timeToIntercept > 0.5)
            coeffs = coeffs100;
        else
            coeffs = coeffs050;

        var num1 = coeffs[0] * forwardVelocity * forwardVelocity;
        var num2 = coeffs[1] * lateralVelocity * lateralVelocity;
        var num3 = coeffs[2] * lateralVelocity * forwardVelocity;
        var num4 = coeffs[3] * forwardVelocity;
        var num5 = coeffs[4] * lateralVelocity;
        var num6 = coeffs[5];
        return num1 + num2 + num3 + num4 + num5 + num6;
    }

    double[] coeffsAvgVel = new double[0];
    double[] coeffsAvgVel800 = new double[] { -1.723360e-04, 1.230321e-04, -2.023321e-04, 5.127529e-02, 4.642541e-03, 2.097784e+02 };
    double[] coeffsAvgVel700 = new double[] { -1.968387e-04, 1.405248e-04, -2.310997e-04, 5.856561e-02, 5.302618e-03, 2.093470e+02 };
    double[] coeffsAvgVel600 = new double[] { -2.294639e-04, 1.638162e-04, -2.694036e-04, 6.827262e-02, 6.181505e-03, 2.087726e+02 };
    double[] coeffsAvgVel500 = new double[] { -2.750528e-04, 1.963625e-04, -3.229274e-04, 8.183671e-02, 7.409618e-03, 2.079700e+02 };
    double[] coeffsAvgVel400 = new double[] { -3.432477e-04, 2.450474e-04, -4.029921e-04, 1.021268e-01, 9.246714e-03, 2.067693e+02 };
    double[] coeffsAvgVel300 = new double[] { -4.564063e-04, 3.258322e-04, -5.358466e-04, 1.357950e-01, 1.229508e-02, 2.047771e+02 };
    double[] coeffsAvgVel200 = new double[] { -6.808684e-04, 4.860776e-04, -7.993778e-04, 2.025794e-01, 1.834184e-02, 2.008252e+02 };
    double[] coeffsAvgVel100 = new double[] { -1.339773e-03, 9.564752e-04, -1.572969e-03, 3.986240e-01, 3.609201e-02, 1.892246e+02 };
    double[] coeffsAvgVel050 = new double[] { -2.247077e-03, 1.795438e-03, -2.796223e-03, 7.398802e-01, 6.514080e-02, 1.670806e+02 };

    double CalculateMissileAverageVelocity(double forwardVelocity, double lateralVelocity, double timeToIntercept = 4)
    {
        if (timeToIntercept > 4)
            coeffsAvgVel = coeffsAvgVel800;
        else if (timeToIntercept > 3)
            coeffsAvgVel = coeffsAvgVel700;
        else if (timeToIntercept > 2.5)
            coeffsAvgVel = coeffsAvgVel600;
        else if (timeToIntercept > 2)
            coeffsAvgVel = coeffsAvgVel500;
        else if (timeToIntercept > 1.5)
            coeffsAvgVel = coeffsAvgVel400;
        else if (timeToIntercept > 1)
            coeffsAvgVel = coeffsAvgVel300;
        else if (timeToIntercept > 0.5)
            coeffsAvgVel = coeffsAvgVel200;
        else if (timeToIntercept > 0.25)
            coeffsAvgVel = coeffsAvgVel100;
        else
            coeffsAvgVel = coeffsAvgVel050;

        var num1 = coeffsAvgVel[0] * forwardVelocity * forwardVelocity;
        var num2 = coeffsAvgVel[1] * lateralVelocity * lateralVelocity;
        var num3 = coeffsAvgVel[2] * lateralVelocity * forwardVelocity;
        var num4 = coeffsAvgVel[3] * forwardVelocity;
        var num5 = coeffsAvgVel[4] * lateralVelocity;
        var num6 = coeffsAvgVel[5];
        return num1 + num2 + num3 + num4 + num5 + num6;
    }
    #endregion

    #region helper_classes

    public static class VectorMath
    {
        public static Vector3D SafeNormalize(Vector3D a)
        {
            if (Vector3D.IsZero(a))
                return Vector3D.Zero;

            if (Vector3D.IsUnit(ref a))
                return a;

            return Vector3D.Normalize(a);
        }

        public static Vector3D Reflection(Vector3D a, Vector3D b, double rejectionFactor = 1) //reflect a over b
        {
            Vector3D project_a = Projection(a, b);
            Vector3D reject_a = a - project_a;
            return project_a - reject_a * rejectionFactor;
        }

        public static Vector3D Rejection(Vector3D a, Vector3D b) //reject a on b
        {
            if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
                return Vector3D.Zero;

            return a - a.Dot(b) / b.LengthSquared() * b;
        }

        public static Vector3D Projection(Vector3D a, Vector3D b)
        {
            if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
                return Vector3D.Zero;

            return a.Dot(b) / b.LengthSquared() * b;
        }

        public static double ScalarProjection(Vector3D a, Vector3D b)
        {
            if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
                return 0;

            if (Vector3D.IsUnit(ref b))
                return a.Dot(b);

            return a.Dot(b) / b.Length();
        }

        public static double AngleBetween(Vector3D a, Vector3D b) //returns radians
        {
            if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
                return 0;
            else
                return Math.Acos(MathHelper.Clamp(a.Dot(b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1));
        }

        public static double CosBetween(Vector3D a, Vector3D b, bool useSmallestAngle = false) //returns radians
        {
            if (Vector3D.IsZero(a) || Vector3D.IsZero(b))
                return 0;
            else
                return MathHelper.Clamp(a.Dot(b) / Math.Sqrt(a.LengthSquared() * b.LengthSquared()), -1, 1);
        }

        public static bool IsDotProductWithinTolerance(Vector3D a, Vector3D b, double tolerance)
        {
            double dot = Vector3D.Dot(a, b);
            double num = a.LengthSquared() * b.LengthSquared() * tolerance * tolerance;
            return dot * dot > num;
        }
    }

    #endregion
#endregion

#region Gyro Control

#endregion

#region Sound Control

// Call each script update to handle sound stuff. Will play while TARGET_LOCK
void updateSound() {
    if (TARGET_LOCK) {
        if (!speakerAlertPlaying) {
            enableSound();
            speakerAlertPlaying = true;
        }
    }
    else {
        if (speakerAlertPlaying) {
            disableSound();
            speakerAlertPlaying = false;
        }
    }
}

void setupSoundBlock() {
    // Set sound block loop time to 10 minutes;
    TargetingSpeaker.LoopPeriod = 600F;
    
    // Stop sound if playing
    disableSound();
}

void enableSound() {
    TargetingSpeaker.ApplyAction("PlaySound");
}

void disableSound() {
    TargetingSpeaker.ApplyAction("StopSound");
}

#endregion

#region Debug and Interface Feedback

void updateEchos() {
    if (displayCycleCount == MAX_DISPLAY_CYCLES) displayCycleCount = 0;

    // Do title and little animation thingy
    Echo("Aimbot Targeting System " + wheelAnimationStates[displayCycleCount]);

    // Report Registered Block Counts
    Echo("+++++ Blocks Registered: +++++");
    Echo(Cameras.Count.ToString() + " Raycast Cameras");
    Echo(DesignatorTurrets.Count.ToString() + " Designator Turrets");
    Echo(Gyros.Count.ToString() + " Gyroscopes");
    Echo("Has Reference Controller? " + ((HAS_REFERENCE_CONTROLLER) ? "Yes" : "No"));
    Echo("Has Speaker? " + ((SOUND_AVAILABLE) ? "Yes" : "No"));

    // Report System Status
    Echo("\n+++++ Systems: ++++++");
    Echo((systemOperational) ? "ATS is Operational" : "ATS is Offline");
    Echo((RAYCAST_POSSIBLE) ? "Can Raycast" : "Cannot Raycast");
    Echo((TURRET_TRACKING_POSSIBLE) ? "Can Turret Track" : "Cannot Turret Track");

    Echo("\nLast Execution Duration: " + Runtime.LastRunTimeMs + " ms");


    displayCycleCount++;
}

#endregion
