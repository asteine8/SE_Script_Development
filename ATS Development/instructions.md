# Instructions for Using the Aimbot Targeting Script #
Rain42 - 7/12/2020

ATS is a robust script that uses turret and raycast methods to find and lock onto enemy targets. Once a target has been selected and is being continuously tracked| this script uses a speaker to alert the user of the current target lock. While a target lock is established| a PID based gyro control system automatically turns towards a target with near-perfect weapon leading. This script is capable of leading targets for missiles| projectiles| and any other modded weapon that has a constant| relative velocity.

## Script Requirements ##

- 1 Programmable Block
- A cockpit| control seat| or remote control
- One or more forward facing (relative to the cockpit)| unobstructed cameras ***or*** one or more active turrets

## Installation ##
1. Name a forward facing control block (Can be a cockpit| control seat| or remote control) with the tag "&lt;reference&gt;" (Note that this tag is not case sensitive and only needs to be part of the block's name)
2. Load the ATS script into a programmable block and recompile and save. Ensure that there is script info in the programmable block's control menu

#### Optional: ####
2. (*To Setup Turret Tracking*) Name all turrets that will be designating targets to aim at with the tag "&lt;designator&gt;"
3. (*To Setup a Spound Block for targeting alerts*) Name the sound block in question with the tag "&lt;targ_speaker&gt;". Select a sound from the sound block's menu (Alert 2 is a good choice) and crank up the volume and range to values that can be heard from the control station

## Setup ##
### User Variables ###
#### Gyroscope Control
Variable | Description
-------- | -----------
P_TERM | Porportional Term for the gyroscope PID control system (See PID tuning)
I_TERM | Integral Term for the gyroscope PID control system (See PID tuning)
D_TERM | Derivative Term for the gyroscope PID control system (See PID tuning)
INTEGRAL_DECAY | Percent of integral to take to next PID cycle (See PID tuning)
ANGULAR_VELOCITY_SCALING_FOR_LEADING | Leads in front of actual target  intercept by this much times angular velocity and delta velocity between target and our grid (See PID tuning). Compensates for rotation lag
ROTATION_GAIN | Gain for PID output (final scalar before values write to gyros)
MAX_ANGULAR_VELOCITY | Maximum angular velocity to turn the craft (rad/s)
GAME_MAX_SPEED | Max Speed in map (Just put the fasted speed here) (m/s)
CYCLES_FOR_BLOCK_UPDATE | How many cycles (at 6Hz) to wait betwen automatically re-registering blocks
DEFAULT_MUZZLE_VELOCITY | The default muzzle velocity for selected weapon leading (m/s)
DEFAULT_WEAPON_TYPE | The default weapon type. Use WEAPON_TYPE.GATLING for most modded weapons. Can be either WEAPON_TYPE.GATLING or WEAPON_TYPE.ROCKET
RAYCAST_SCAN_DEGREE_DEVIATION | Degree deviation between raycasts in a raycast scan. Set small for fighters!
RAYCAST_SCAN_RADIUS = 2 | "Radius" of the square of raycast scans (ie: a radius of two creates a 3x3 raycast scan grid)
MAX_RAYCAST_RANGE | Maximum distance a camera can raycast (meters)
RAYCASTING_PERIOD | How many cycles (at 6Hz) to wait between raycasting at target again to update position (higher values decrease tracking so don't put this above 3)

### Toolbar Setup ###

Argument | Function
-------- | -----------
tracking_turrets | Enables tracking through any turrets on the grid. Disables raycast tracking.
tracking_raycast | Fires a raycast scan and enables raycast tracking if a target is hit. Disables turret tracking.
tracking_disable | Disables all tracking modes
aimbot_enable | Enables gyro control for the aimbot only while tracking is active
aimbot_disable | Disables gyro control for the aimbot
aimbot_toggle | Toggles gyro control for the aimbot
aimbot_missiles | Sets the aimbot lead tracking for missiles
aimbot_gatlings | Sets the aimbot lead tracking for gatling guns
sound_enable | Enables tracking alert sound
sound_disable | Disables tracking alert sound
sound_toggle | Toggles tracking alert sound






