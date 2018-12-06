

IMyTextPanel LCD;
IMyCameraBlock ChargeCam;

List<IMyBlockGroup> Groups = new List<IMyBlockGroup>();
List<IMyBlockGroup> CameraGroups = new List<IMyBlockGroup>();
List<IMyCameraBlock> Cameras = new List<IMyCameraBlock>();

string ScannerGroupTag = "[Scanner]";

List<MyDetectedEntityInfo> DetectedObjects = new List<MyDetectedEntityInfo>();

const float coneLimit = 45f; // Max abs value for pitch and yaw
const float maxPitch = 0f;
const float minPitch = -45f;
float pitchIteration = -coneLimit;
float yawIteration = -coneLimit;
// float stepSize = 0.1f;

const float YawStepSize = 7.5f;
const float PitchStepSize = 1f;

const double raycastRange = 25;

int counter = 0;


public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Start out with updates every 10 ticks

    GridTerminalSystem.GetBlockGroups(Groups);
    for (int i = 0; i < Groups.Count; i++) {
        if (Groups[i].Name.Contains(ScannerGroupTag)) {
            CameraGroups.Add(Groups[i]);
        }
    }
    GridTerminalSystem.GetBlocksOfType(Cameras);
    for (int i = 0; i < Cameras.Count; i++) {
        Cameras[i].EnableRaycast = true;
    }
    ChargeCam = Cameras[0];
}

void Main(string arg) {
    GridScan();
    if (counter == 1) {
        LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

        string output = "MaxScanDist: " + ChargeCam.AvailableScanRange.ToString("0") + "\n";

        for (int i = 0; i < DetectedObjects.Count; i++) {
            // output += DetectedObjects[i].Position.ToString("0.0") + "\n";
            output += DetectedObjects[i].Name + "\n";
        }

        LCD.WritePublicText(output);

        counter = 0;
    }
    else {
        counter ++;
    }
}

void GridScan() {
    for (int groupIndex = 0; groupIndex < CameraGroups.Count; groupIndex++) {
        CameraGroups[groupIndex].GetBlocksOfType(Cameras);
        for (int camIndex = 0; camIndex < Cameras.Count; camIndex++) {

            if (!Cameras[camIndex].CanScan(raycastRange)) {
                return; // Camara not charged enough
            }

            float relativeYaw = -coneLimit + (2*coneLimit/Cameras.Count)*camIndex + coneLimit/Cameras.Count;
            relativeYaw += yawIteration / Cameras.Count;
            // float relativeYaw = yawIteration;
            float relativePitch = pitchIteration;
            MyDetectedEntityInfo info = Cameras[camIndex].Raycast(raycastRange, relativePitch, relativeYaw);
            if (!info.IsEmpty() && info.Type != MyDetectedEntityType.Planet && info.Type != MyDetectedEntityType.Asteroid) {
                bool IsDuplicate = false;
                for (int i = 0; i < DetectedObjects.Count; i++) {
                    if (info.EntityId == DetectedObjects[i].EntityId) {
                        IsDuplicate = true;
                    }
                }
                if (!IsDuplicate) {
                    DetectedObjects.Add(info);
                }
            }
        }
    }
    if (yawIteration >= coneLimit) {
        yawIteration = -coneLimit;
        if (pitchIteration >= maxPitch) {
            pitchIteration = minPitch;
        }
        else {
            pitchIteration += PitchStepSize;
        }
    }
    else {
        yawIteration += YawStepSize;
    }
    Echo("Yaw: " + yawIteration.ToString("0.0") + "\nPitch: " + pitchIteration.ToString("0.0"));
}