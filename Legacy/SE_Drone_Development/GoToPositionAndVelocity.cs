

public Program() {

}

void Main() {

}

// Goes to a target position and veloity as fast as possible
void GoToPositionAndVelocity (IMyRemoteControl RefRemote, List<IMyThrust> Thrusters, Vector3D targetPos, Vector3D targetVelocity, List<double> MaxAccels) {

}

// Calculates maximum acceleration given thrust and mass
List<float> GetAccelerationsForThrusters(IMyRemoteControl RefRemote, List<IMyThrust> Thrusters) {
    // Forward, Backward, Left, Right, Up, Down
    List<float> directionalThrust = new List<float>{0,0,0,0,0,0}; // Force in newtons
    List<float> directionalAcc = new List<float>{0,0,0,0,0,0}; // Acceleration in m/s^2
    float shipMass = RefRemote.CalculateShipMass().PhysicalMass;

    for (int i = 0; i < Thrusters.Count; i ++) {
        switch (RefRemote.Orientation.Forward) {
            case Base6Directions.GetOppositeDirection(Thrusters[i].Orientation.Forward): // Forward thrust
                directionalThrust[0] += Thrusters[i].MaxThrust;
                break;
            case Thrusters[i].Orientation.Forward: // Backwards thrust
                directionalThrust[1] += Thrusters[i].MaxThrust;
                break;
            case Base6Directions.GetOppositeDirection(Thrusters[i].Orientation.Left): // Left Thrust
                directionalThrust[2] += Thrusters[i].MaxThrust;
                break;
            case Thrusters[i].Orientation.Left: // Right thrust
                directionalThrust[3] += Thrusters[i].MaxThrust;
                break;
            case Base6Directions.GetOppositeDirection(Thrusters[i].Orientation.Up): // Up Thrust
                directionalThrust[4] += Thrusters[i].MaxThrust;
                break;
            case Thrusters[i].Orientation.Up: // Down thrust
                directionalThrust[5] += Thrusters[i].MaxThrust;
                break;
        }
    }

    for (int i = 0; i < directionalThrust.Count; i++) {
        directionalAcc[i] = directionalThrust[i]/shipMass; // a=F/m
    }

    return directionalAcc;
}