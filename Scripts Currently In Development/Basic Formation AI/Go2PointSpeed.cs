public void Go2PointSpeed(IMyShipController REFERENCE, List<IMyThrust> thrusters, double[] maxThrustAxes, Vector3D targetPos, double targetSpeed) {

}

/**
 * Returns the maximum acceleration of the current grid along a vector axis
 */
public double GetMaxAccelerationAlongAxis(IMyShipController REFERENCE, double[] maxThrustAxes, Vector3D axis) {
    Vector3D alignedAxis;

    alignedAxis.X = ScalarProjection(axis, REFERENCE.WorldMatrix.Forward);
    alignedAxis.Y = ScalarProjection(axis, REFERENCE.WorldMatrix.Up);
    alignedAxis.Z = ScalarProjection(axis, REFERENCE.WorldMatrix.Left);

    // scale axis so max vector axis is +/-1
    alignedAxis /= alignedAxis.AbsMax();

    Vector3D acceleration; // Forward, Backward, Left, Right, Up, Down

    if (alignedAxis.X > 0) acceleration.X = maxThrustAxes[0] * Math.Abs(alignedAxis.X);
    else acceleration.X = maxThrustAxes[1] * Math.Abs(alignedAxis.X);

    if (alignedAxis.Z > 0) acceleration.Z = maxThrustAxes[2] * Math.Abs(alignedAxis.Z);
    else acceleration.Z = maxThrustAxes[3] * Math.Abs(alignedAxis.Z);

    if (alignedAxis.Y > 0) acceleration.Y = maxThrustAxes[4] * Math.Abs(alignedAxis.Y);
    else acceleration.Y = maxThrustAxes[5] * Math.Abs(alignedAxis.Y);

    return acceleration.Length();
}

/**
 * Returns a 6 element array that describes the maximum acceleration of the grid along the 6
 * possible cardinal thrust directions. Needs mass and a reference
 */
public double[] GetMaxAccelerations(IMyShipController REFERENCE, List<IMyThrust> thrusters, double shipMass) {
    double maxThrust = new double[6]; // Forward, Backward, Left, Right, Up, Down

    Direction refForward = REFERENCE.Orientation.Forward;
    Direction refBackward = Base6Directions.GetOppositeDirection(refForward);
    Direction refLeft = REFERENCE.Orientation.Left;
    Direction refRight = Base6Directions.GetOppositeDirection(refLeft);
    Direction refUp = REFERENCE.Orientation.Up;
    Direction refDown = Base6Directions.GetOppositeDirection(refUp);

    for (int i = 0; i < thrusters.Count; i++) {
        switch (thrusters[i].Orientation.Forward) {
            case refForward:
                maxThrust[0] += (double)thrusters[i].MaxEffectiveThrust;
                break;
            case refBackward:
                maxThrust[1] += (double)thrusters[i].MaxEffectiveThrust;
                break;
            case refLeft:
                maxThrust[2] += (double)thrusters[i].MaxEffectiveThrust;
                break;
            case refRight:
                maxThrust[3] += (double)thrusters[i].MaxEffectiveThrust;
                break;
            case refUp:
                maxThrust[4] += (double)thrusters[i].MaxEffectiveThrust;
                break;
            case refDown:
                maxThrust[5] += (double)thrusters[i].MaxEffectiveThrust;
                break;
        }
    }

    maxThrust[0] /= shipMass;
    maxThrust[1] /= shipMass;
    maxThrust[2] /= shipMass;
    maxThrust[3] /= shipMass;
    maxThrust[4] /= shipMass;
    maxThrust[5] /= shipMass;

    return maxThrust;
}

/**
 * Preforms a scalar projection of vector onto baseVect
 */
public double ScalarProjection(Vector3D vector, Vector3D baseVect) {
    return Vector3D.Dot(baseVect, vector) / baseVect.Length(); 
}