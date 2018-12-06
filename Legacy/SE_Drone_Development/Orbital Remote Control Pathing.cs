
// Public Variables
Vector3D OrbitCenter = new Vector3D(0,0,0); // Center of orbital pathing
double OrbitRadium = 1000; // Distance from OrbitCenter in meters

double ObstacleDetectionRaycastRange = 500; // Distance to check ahead for obsticles via raycast
double ObstacleAdvoidanceRadiusMultiplier = 1.25 // Scales dodge vector distance from obsticle center


// Single Block Objects
IMyRemoteControl Remote;
IMyCameraBlock ObstacleDetectionCamera;


// Block Lists
List<IMyCameraBlock> Cameras = new List<IMyCameraBlock>();


// Pathing Variables
Vector3D TargetVector;
Vector3D PrevTargetVector;




public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks (0.6Hz)
    
    GridTerminalSystem.GetBlocksOfType(Cameras);
    ObstacleDetectionCamera = Camera[0];
}

void Main(string argument) {

}

Vector3D DetectAndAdvoidObstacles() {
    MyDetectedEntityInfo info = ObstacleDetectionCamera.Raycast(ObstacleDetectionRaycastRange,0,0); // Raycast forwards and get data

    if (!info.IsEmpty()) { // Raycast hit something, do something about that
        Vector3D ObsticlePosition = info.Position; // Get center of obsticles
        double ObsticleRadius = info.BoundingBox.Size.Length() / 2; // Get radius of obsticle

        Vector3D v1 = ObstacleDetectionCamera.Position - ObsticlePosition; // Vector from obsticle center to drone position
        Vector3D v2 = info.HitPosition - ObticlePosition; // Vector from obticle center to raycast intersect

        Vector3D Normal = Vector3D.Cross(v1,v2);

        Normal = Normal * ( (ObsticleRadius / Normal.Length()) * ObstacleAdvoidanceRadiusMultiplier); // Scale up vector

        return Normal + ObsticlePosition; // Return vector in global grid
    }
    else {
        return null;
    }
}

Vector3D GetVectorFromAzumithElevation(double Azumith, double Elevation) {
    
}