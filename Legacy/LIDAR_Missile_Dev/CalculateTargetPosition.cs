
void Main (string arg) {

}

Vector3D CalculateTargetPosition(Vector3D LastKnownPosition, Vector3D LastKnownVelocity, double secondsSinceLIDARUpdate) {
    return LastKnownPosition + (LastKnownVelocity * secondsSinceLIDARUpdate);
}