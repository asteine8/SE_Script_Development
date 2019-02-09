

IMyRemoteControl RefRemote;

List<IMyThrust> Thrusters = new List<IMyThrust>();

// Reference directions of the grid (Based on reference remote control orientation)
Base6Directions.Direction RefForward;
Base6Directions.Direction RefBackward;
Base6Directions.Direction RefLeft;
Base6Directions.Direction RefRight;
Base6Directions.Direction RefUp;
Base6Directions.Direction RefDown;

double[] maxAccelerationsBase6;

public Program() {
    RefRemote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
    GridTerminalSystem.GetBlocksOfType("Thrusters");

    RefForward = refRemote.Orientation.Forward;
    RefBackward = Base6Directions.GetOppositeDirection(RefForward);
    RefLeft = refRemote.Orientation.Forward;
    RefRight = Base6Directions.GetOppositeDirection(RefForward);
    RefUp = refRemote.Orientation.Forward;
    RefDown = Base6Directions.GetOppositeDirection(RefForward);

    maxAccelerationsBase6 = GetMaxAccelerationInBase6(refRemote, Thrusters);
}

double[] GetMaxAccelerationInBase6(IMyRemoteControl refRemote, List<IMyThrust> thrusters) {
    // In direction that thruster applies force towards
    double[] thrusterForces = new double[6]; // [forwards, backwards, left, right, up, down]
    double massOfGrid = (double)refRemote.CalculateShipMass().TotalMass;
    for (int i = 0; i < thrusters.Size(); i++) {
        switch (Base6Directions.GetOppositeDirection(thrusters[i].Orientation.Forward)) {
            case RefForward:
                thrusterForces[0] += (double)thrusters[i].MaxEffectiveThrust/massOfGrid;
                break;
            case RefBackward:
                thrusterForces[1] += (double)thrusters[i].MaxEffectiveThrust/massOfGrid;
                break;
            case RefLeft:
                thrusterForces[2] += (double)thrusters[i].MaxEffectiveThrust/massOfGrid;
                break;
            case RefRight:
                thrusterForces[3] += (double)thrusters[i].MaxEffectiveThrust/massOfGrid;
                break;
            case RefUp:
                thrusterForces[4] += (double)thrusters[i].MaxEffectiveThrust/massOfGrid;
                break;
            case RefDown:
                thrusterForces[5] += (double)thrusters[i].MaxEffectiveThrust/massOfGrid;
                break;
        }
    }
    return thrusterForces;
}

double GetMaxAccelerationAlongVector(IMyRemoteControl refRemote, double[] base6Acceleration, Vector3D dir) {
    Vector3D RefForwardVect = refRemote.WorldMatrix.Forward;
    Vector3D RefUpVect = refRemote.WorldMatrix.Up;
    Vector3D RefRightVect = refRemote.WorldMatrix.Right;

    // Apply scalar projection onto each vector, return opposite values if scalar is negative
}

