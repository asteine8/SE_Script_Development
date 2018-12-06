

IMyLargeTurretBase TargetingTurret;
IMyCameraBlock RaycastCam;

IMyBlockGroup Gyros;
List<IMyGyro> Gyroscopes = new List<IMyGyro>();

IMyBlockGroup Guns;
List<IMyUserControllableGun> GatlingGuns = new List<IMyUserControllableGun>();

Random random = new Random(); // For generating random #'s

bool TargetDetected = false;

// Turret info
double Elevation = 0;
double Azimuth = 0;

double prevElevation = 0;
double prevAzimuth = 0;

double CurrentTurretInventoryValue = 0;
double PrevTurretInventoryValue = 0;



// Gyro Stuff

double rotScaler = 17; // Increases Speed to destination
double MinRot = 0.15;
double OnTargetThreshold = 0.002;

double AimSpeedThreshold = 0.45; // Starts using higher scalers at this Threshold (25 degree arc)
double aimRotScaler = 45; // Rot speed scaler when in AimSpeedThreshold

double FireThresh = 0.0375; // Fire when in this degree arc

double YawVel = 0;
double PitchVel = 0;

// Counters

int secondCounter = 0;
int fiveSecondCounter = 0;

int inactivityCounter = 0;
int inactiveCount = 30; // 3 seconds at 10Hz

// Raycasting Variables

double RaycastDistance = 600; // Meters

double MaxDistance = 450; // Meters
double MinDistance = 400; // Meters

// Thruster Groups - Direction tells what direction thrusters thrust towards

double MaxThrustMultiplier = 1;
double AttackRunThrustMultiplier = 1;
double StrafeThrustMultiplier = 0.75;

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


public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    Echo("");

    TargetingTurret = GridTerminalSystem.GetBlockWithName("Targeting Turret") as IMyLargeTurretBase;
    Elevation = TargetingTurret.Elevation; // Set values first to prevent misfire
    Azimuth = TargetingTurret.Azimuth;
    prevAzimuth = Azimuth;
    prevElevation = Elevation;
}

void Main(string argument) {
    TargetingTurret = GridTerminalSystem.GetBlockWithName("Targeting Turret") as IMyLargeTurretBase;
    
    Guns = GridTerminalSystem.GetBlockGroupWithName("Gatling Guns") as IMyBlockGroup;
    Guns.GetBlocksOfType(GatlingGuns); // Get list of gatling guns IMyUserControllableGun's

    Gyros = GridTerminalSystem.GetBlockGroupWithName("Gyros") as IMyBlockGroup;
    Gyros.GetBlocksOfType(Gyroscopes); // Get list of gyroscope IMyGyro's

    // Get turret orientation
    Elevation = TargetingTurret.Elevation;
    Azimuth = TargetingTurret.Azimuth;

    // Get turret inventory
    CurrentTurretInventoryValue = TargetingTurret.GetInventory(0).CurrentVolume.RawValue;

    // Trigger active mode from idle mode (Look at turret orientation)
    if (!TargetDetected) {
        if (Elevation != prevElevation || Azimuth != prevAzimuth) {
            Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update @ 10 Hz
            TargetDetected = true;
            Echo("TargetDetected");
        }
    }
    // Trigger inactive mode from active mode (Look at turrent orientation and inventory)
    else {
        if (Elevation == prevElevation && Azimuth == prevAzimuth && CurrentTurretInventoryValue == PrevTurretInventoryValue) {
            
            if (inactivityCounter == inactiveCount) { // Only apply inactivity mode if the inactivity counter has reached the inactive count requirement
                Runtime.UpdateFrequency = UpdateFrequency.Update1; // Update @ 1Hz
                TargetDetected = false;
                Echo("Going into inactive mode");

                TurnOffThrusterOverride(); // Turn off thruster overrides

                // Stop shooting
                for (int i = 0; i < GatlingGuns.Count; i++) {
                    GatlingGuns[i].ApplyAction("Shoot_Off");
                }

                for (int i = 0; i < Gyroscopes.Count; i++) {
                    Gyroscopes[i].SetValueFloat("Yaw", 0);
                    Gyroscopes[i].SetValueFloat("Pitch", 0);
                }
                inactivityCounter = 0;

                prevAzimuth = Azimuth;
                prevElevation = Elevation;
            }
            else {
                inactivityCounter ++; // Increment inacti`vity counter
            }
        }
    }

    if (TargetDetected) { // Only do preformance damaging stuff if a target is detected
        
        // Angle ship to point to at turret direction
        if (Math.Abs(Azimuth) < AimSpeedThreshold) {
            YawVel = -1 * Azimuth * aimRotScaler + MinRot;
        }
        else if (Math.Abs(Azimuth) > OnTargetThreshold) {
            YawVel = -1 * Azimuth * rotScaler;
        }
        else {
            YawVel = 0;
        }

        if (Math.Abs(Elevation) < AimSpeedThreshold) {
            PitchVel = 1 * Elevation * aimRotScaler + MinRot;
        }
        else if (Math.Abs(Elevation) > OnTargetThreshold) {
            PitchVel = 1 * Elevation * rotScaler;
        }
        else {
            PitchVel = 0;
        }

        if (Math.Abs(Elevation) < FireThresh && Math.Abs(Azimuth) < FireThresh) {
            // Drone is on target, fire weapons
            for (int i = 0; i < GatlingGuns.Count; i++) {
                GatlingGuns[i].ApplyAction("Shoot_On");
            }
        }
        else {
            // Drone is not on target, don't fire weapons
            for (int i = 0; i < GatlingGuns.Count; i++) {
                GatlingGuns[i].ApplyAction("Shoot_Off");
            }
        }

        for (int i = 0; i < Gyroscopes.Count; i++) {
            Gyroscopes[i].SetValueFloat("Yaw", (float)YawVel);
            Gyroscopes[i].SetValueFloat("Pitch", (float)PitchVel);
        }
        
        if (secondCounter == 9) { // Count to 9 for 10 cycles (1 second with 10Hz)
            secondCounter = 0; // Reset Counter
            Echo("Running Raycasting");
            modulateDistanceToTarget();
        }
        else {
            secondCounter ++;
        }
    }

    // Record History
    prevAzimuth = Azimuth;
    prevElevation = Elevation;
    Echo("Elevation: " + Elevation.ToString("0.0000"));
    Echo("Azimuth: " + Azimuth.ToString("0.0000"));
    Echo("Target Detected: " + TargetDetected.ToString());
}


void modulateDistanceToTarget() {
    RaycastCam = GridTerminalSystem.GetBlockWithName("Raycast Camera") as IMyCameraBlock;
    RaycastCam.EnableRaycast = true;

    // Get Block Groups as IMyBlockGroup Types
    ForwardThrust = GridTerminalSystem.GetBlockGroupWithName("ForwardThrust") as IMyBlockGroup;
    BackwardThrust = GridTerminalSystem.GetBlockGroupWithName("BackwardThrust") as IMyBlockGroup;
    UpThrust = GridTerminalSystem.GetBlockGroupWithName("UpThrust") as IMyBlockGroup;
    DownThrust = GridTerminalSystem.GetBlockGroupWithName("DownThrust") as IMyBlockGroup;
    LeftThrust = GridTerminalSystem.GetBlockGroupWithName("LeftThrust") as IMyBlockGroup;
    RightThrust = GridTerminalSystem.GetBlockGroupWithName("RightThrust") as IMyBlockGroup;

    // Get block lists from IMyBlockGroup's
    ForwardThrust.GetBlocksOfType(ForwardThrusters);
    BackwardThrust.GetBlocksOfType(BackwardThrusters);
    UpThrust.GetBlocksOfType(UpThrusters);
    DownThrust.GetBlocksOfType(DownThrusters);
    LeftThrust.GetBlocksOfType(LeftThrusters);
    RightThrust.GetBlocksOfType(RightThrusters);

    if (RaycastCam.CanScan(RaycastDistance)) {
        float raycastPitch = (float)(Elevation * 57.29578); // Calculate pitch and yaw to target
        float raycastYaw = (float)(-1 * Azimuth * 57.29578);
        MyDetectedEntityInfo raycastResults = RaycastCam.Raycast(RaycastDistance, raycastPitch, raycastYaw);

        if (raycastResults.IsEmpty() == false) {
            double distanceToTarget = Vector3D.Distance(RaycastCam.GetPosition(), raycastResults.HitPosition.Value);
            Echo("Raycast found: " + distanceToTarget.ToString("0.00"));
            if (distanceToTarget > MaxDistance) {
                // Thrust Forwards - Too Far
                float MaxThrustN = ForwardThrusters[0].MaxThrust;
                for (int i = 0; i < ForwardThrusters.Count; i++) { // Turn on forwards thrusters
                    ForwardThrusters[i].ThrustOverride = MaxThrustN * (float)MaxThrustMultiplier;
                }
                for (int i = 0; i < BackwardThrusters.Count; i++) { // Turn off backward thrust
                    BackwardThrusters[i].ThrustOverride = 0;
                }
            }
            else if (distanceToTarget < MinDistance) {
                // Thrust Backwards - Too Close
                float MaxThrustN = ForwardThrusters[0].MaxThrust;
                for (int i = 0; i < BackwardThrusters.Count; i++) { // Turn on backward thrust
                    BackwardThrusters[i].ThrustOverride = MaxThrustN * (float)MaxThrustMultiplier;
                }
                for (int i = 0; i < ForwardThrusters.Count; i++) { // Turn off forwards thrusters
                    ForwardThrusters[i].ThrustOverride = 0;
                }
            }
            else {
                // In Range - Deactivate forward / backward thrusting
                for (int i = 0; i < ForwardThrusters.Count; i++) { // Turn off forwards thrusters
                    ForwardThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < BackwardThrusters.Count; i++) { // Turn off backward thrust
                    BackwardThrusters[i].ThrustOverride = 0;
                }
            }
        }
        else {
            // No object detected, move to target
            float MaxThrustN = ForwardThrusters[0].MaxThrust;
            Echo("Raycast did not find anything, going on attack run");
            for (int i = 0; i < ForwardThrusters.Count; i++) {
                ForwardThrusters[i].ThrustOverride = MaxThrustN * (float)AttackRunThrustMultiplier;
            }
            for (int i = 0; i < BackwardThrusters.Count; i++) {
                BackwardThrusters[i].ThrustOverride = 0;
            }
        }
    }

    if (fiveSecondCounter == 4) {
        fiveSecondCounter = 0;
        PrevTurretInventoryValue = CurrentTurretInventoryValue; // Set previous value every 10 seconds
        // Pick a random direction to strafe
        Echo("Strafing in a random direction");
        
        int direction = random.Next(4); // Generates a random number from 0-3

        switch (direction) {
            case 0: // Up Thrust
                for (int i = 0; i < UpThrusters.Count; i++) {
                    UpThrusters[i].ThrustOverride = UpThrusters[0].MaxThrust * (float)StrafeThrustMultiplier;
                }
                for (int i = 0; i < DownThrusters.Count; i++) {
                    DownThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < LeftThrusters.Count; i++) {
                    LeftThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < RightThrusters.Count; i++) {
                    RightThrusters[i].ThrustOverride = 0;
                }
                break;
            case 1: // Down Thrust
                for (int i = 0; i < UpThrusters.Count; i++) {
                    UpThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < DownThrusters.Count; i++) {
                    DownThrusters[i].ThrustOverride = DownThrusters[0].MaxThrust * (float)StrafeThrustMultiplier;
                }
                for (int i = 0; i < LeftThrusters.Count; i++) {
                    LeftThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < RightThrusters.Count; i++) {
                    RightThrusters[i].ThrustOverride = 0;
                }
                break;
            case 2: // Left Thrust
                for (int i = 0; i < UpThrusters.Count; i++) {
                    UpThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < DownThrusters.Count; i++) {
                    DownThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < LeftThrusters.Count; i++) {
                    LeftThrusters[i].ThrustOverride = LeftThrusters[0].MaxThrust * (float)StrafeThrustMultiplier;
                }
                for (int i = 0; i < RightThrusters.Count; i++) {
                    RightThrusters[i].ThrustOverride = 0;
                }
                break;
            case 3: // Right Thrust
                for (int i = 0; i < UpThrusters.Count; i++) {
                    UpThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < DownThrusters.Count; i++) {
                    DownThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < LeftThrusters.Count; i++) {
                    LeftThrusters[i].ThrustOverride = 0;
                }
                for (int i = 0; i < RightThrusters.Count; i++) {
                    RightThrusters[i].ThrustOverride = RightThrusters[0].MaxThrust * (float)StrafeThrustMultiplier;
                }
                break;
        }
    }
    else {
        fiveSecondCounter ++;
    }
}

void TurnOffThrusterOverride() {
    // Get Block Groups as IMyBlockGroup Types
    ForwardThrust = GridTerminalSystem.GetBlockGroupWithName("ForwardThrust") as IMyBlockGroup;
    BackwardThrust = GridTerminalSystem.GetBlockGroupWithName("BackwardThrust") as IMyBlockGroup;
    UpThrust = GridTerminalSystem.GetBlockGroupWithName("UpThrust") as IMyBlockGroup;
    DownThrust = GridTerminalSystem.GetBlockGroupWithName("DownThrust") as IMyBlockGroup;
    LeftThrust = GridTerminalSystem.GetBlockGroupWithName("LeftThrust") as IMyBlockGroup;
    RightThrust = GridTerminalSystem.GetBlockGroupWithName("RightThrust") as IMyBlockGroup;

    // Get block lists from IMyBlockGroup's
    ForwardThrust.GetBlocksOfType(ForwardThrusters);
    BackwardThrust.GetBlocksOfType(BackwardThrusters);
    UpThrust.GetBlocksOfType(UpThrusters);
    DownThrust.GetBlocksOfType(DownThrusters);
    LeftThrust.GetBlocksOfType(LeftThrusters);
    RightThrust.GetBlocksOfType(RightThrusters);

    for (int i = 0; i < ForwardThrusters.Count; i++) {
        ForwardThrusters[i].ThrustOverride = 0;
    }
    for (int i = 0; i < BackwardThrusters.Count; i++) {
        BackwardThrusters[i].ThrustOverride = 0;
    }
    for (int i = 0; i < UpThrusters.Count; i++) {
        UpThrusters[i].ThrustOverride = 0;
    }
    for (int i = 0; i < DownThrusters.Count; i++) {
        DownThrusters[i].ThrustOverride = 0;
    }
    for (int i = 0; i < LeftThrusters.Count; i++) {
        LeftThrusters[i].ThrustOverride = 0;
    }
    for (int i = 0; i < RightThrusters.Count; i++) {
        RightThrusters[i].ThrustOverride = 0;
    }
}