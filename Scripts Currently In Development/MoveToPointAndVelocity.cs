
// Direction Order for Arrays: Forward, Backward, Left, Right, Up, Down

void GetDirectionalAcceleration(IMyRemoteControl REF_REMOTE, List<IMyThrust> Thrusters, out List<double> DirectionalAcceleration) {
    Base6Directions.Direction RefForward = REF_REMOTE.Orientation.Forward;
    Base6Directions.Direction RefBackward = Direction.GetOppositeDirection(RefForward);
    Base6Directions.Direction RefLeft = REF_REMOTE.Orientation.Left;
    Base6Directions.Direction RefRight = Direction.GetOppositeDirection(RefLeft);
    Base6Directions.Direction RefUp = REF_REMOTE.Orientation.Up;
    Base6Directions.Direction RefDown = Direction.GetOppositeDirection(RefUp);
    for (int i = 0; i < DirectionalThrusters.Count; i++) {
        Base6Directions.Direction ThrusterForward = DirectionalThrusters[i].Orientation.Forward;
        switch (ThrusterForward) {
            case RefBackward: // Forward Thrust
                
        }
    }
}

void MoveToPointAndVelocity(IMyRemoteControl REF_REMOTE, List<IMyThrust> Thrusters, List<double> DirectionalAcceleration, Vector3D TargetPosition, Vector3D TargetVelocity) {

}