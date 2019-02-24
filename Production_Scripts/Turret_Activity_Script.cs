// Turret Detection Script
// Rain42

/**
 * This script enables marked sound and light blocks when a marked turret
 * detects a targetable grid. Arguments to test alarm and start/ stop automatic
 * updates (0.6Hz)
 * 
 * Instructions:
 * 		1: Add the "alarmTag" ("alarm" by default) to all sound and light blocks that will be triggered by the alarm
 * 		2: Add the "turretTag" ("designator" by defaut) to all turrets that will trigger the alarm
 * 		3: Load this script into a programmable block and run it.
 * 
 * Commands: (Case Insensitive)
 * "run" - Starts automatic updates
 * "stop" - Stops automatic updates
 * "testalarmson" - Turns all alarm blocks on (Plays sound blocks)
 * "testalarmsoff" - Turns all alarm blocks off (Stops sound blocks)
 */



// Tags (Not case sensitive)
string alarmTag = "alarm";
string turretTag = "designator";

// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
// Program stuff (No touchy under here)
// +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

// Constants
const bool ON = true;
const bool OFF = false;

List<IMySoundBlock> soundBlocks = new List<IMySoundBlock>();
List<IMyLightingBlock> lightBlocks = new List<IMyLightingBlock>();
List<IMyLargeTurretBase> detectorTurrets = new List<IMyLargeTurretBase>();

bool targetDetected = false;
bool prevTargetDetected = false;

public Program() {
    // Get Turret Blocks with tag
    List<IMyLargeTurretBase> allTurrets = new List<IMyLargeTurretBase>();
    GridTerminalSystem.GetBlocksOfType(allTurrets);
    for (int i = 0; i < allTurrets.Count; i++) {
        string turretName = allTurrets[i].CustomName.ToLower();
        if (turretName.Contains(turretTag.ToLower())) {
            detectorTurrets.Add(allTurrets[i]);
        }
    }

    // Get sound blocks with tag
    List<IMySoundBlock> allSoundBlocks = new List<IMySoundBlock>();
    GridTerminalSystem.GetBlocksOfType(allSoundBlocks);
    for (int i = 0; i < allSoundBlocks.Count; i++) {
        string soundBlockName = allSoundBlocks[i].CustomName.ToLower();
        if (soundBlockName.Contains(alarmTag.ToLower())) {
            soundBlocks.Add(allSoundBlocks[i]);
        }
    }

    // Get lighting blocks with tag
    List<IMyLightingBlock> allLightBlocks = new List<IMyLightingBlock>();
    GridTerminalSystem.GetBlocksOfType(allLightBlocks);
    for (int i = 0; i < allLightBlocks.Count; i++) {
        string lightBlockName = allLightBlocks[i].CustomName.ToLower();
        if (lightBlockName.Contains(alarmTag.ToLower())) {
            lightBlocks.Add(allLightBlocks[i]);
        }
    }

	Runtime.UpdateFrequency = UpdateFrequency.Update100; // Update at 0.6Hz
}

void Main(string arg) {
	if (arg.ToLower() == "run") {
		Runtime.UpdateFrequency = UpdateFrequency.Update100; // Update at 0.6Hz
	}
	if (arg.ToLower() == "stop") {
		Runtime.UpdateFrequency = UpdateFrequency.None; // No more updates
	}
	if (arg.ToLower() == "testalarmson") {
		ApplyStateToLights(lightBlocks, ON);
		ApplyStateToSoundblocks(soundBlocks, ON);
	}
	if (arg.ToLower() == "testalarmsoff") {
		ApplyStateToLights(lightBlocks, OFF);
		ApplyStateToSoundblocks(soundBlocks, OFF);
	}
	
	prevTargetDetected = targetDetected;
	targetDetected = TargetDetected(detectorTurrets);

	// Do edge detection to reduce block calls
	if (targetDetected != prevTargetDetected) {
		if (targetDetected) { // Turn stuff on
			ApplyStateToLights(lightBlocks, ON);
			ApplyStateToSoundblocks(soundBlocks, ON);
		}
		else { // Turn stuff off
			ApplyStateToLights(lightBlocks, OFF);
			ApplyStateToSoundblocks(soundBlocks, OFF);
		}
	}
}

bool TargetDetected(List<IMyLargeTurretBase> turrets) {
	for (int i = 0; i < turrets.Count; i++) {
		MyDetectedEntityInfo turretTargetInfo = turrets[i].GetTargetedEntity();
		if (turretTargetInfo.IsEmpty() == false) {
			return true;
		}
	}
	return false;
}

void ApplyStateToLights(List<IMyLightingBlock> lights, bool state) {
	for (int i = 0; i < lights.Count; i++) {
		lights[i].ApplyAction((state==ON) ? "OnOff_On" : "OnOff_Off");
	}
}
void ApplyStateToSoundblocks(List<IMySoundBlock> soundBlocks, bool state) {
	for (int i = 0; i < soundBlocks.Count; i++) {
		if (state == ON) {
			soundBlocks[i].Play();
		}
		else {
			soundBlocks[i].Stop();
		}
	}
}