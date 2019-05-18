const double tAcc = 2; // Time to accelerate (Seconds)
const double maxSpeed = 100; // Max speed
const double dampeningRadius = 10; // Reduce thruster strength in this radius to reduce occilations

IMyShipController Reference;
List<IMyThrust> thrusters = new List<IMyThrust>();

List<IMyThrust> forwardThrusters = new List<IMyThrust>();
List<IMyThrust> backwardThrusters = new List<IMyThrust>();
List<IMyThrust> upThrusters = new List<IMyThrust>();
List<IMyThrust> downThrusters = new List<IMyThrust>();
List<IMyThrust> leftThrusters = new List<IMyThrust>();
List<IMyThrust> rightThrusters = new List<IMyThrust>();
double[] maxThrustAxes;

IMyTextPanel debugLCD;
string lcdText;

// GPS:Target Pos:-14448.55:-19846.87:-7071.89:
Vector3D testDestination = new Vector3D(-14448.55, -19846.87, -7071.89);
double testSpeed = 50;

public Program() {
    GridTerminalSystem.GetBlocksOfType(thrusters);
    Reference = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyShipController;

    debugLCD = GridTerminalSystem.GetBlockWithName("Debug LCD") as IMyTextPanel;

    Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update at 6Hz
}

public void Main(string arg) {
    lcdText = "";

    maxThrustAxes = GetMaxAccelerations(Reference, thrusters, (double)Reference.CalculateShipMass().PhysicalMass); // ~50ns

    lcdText += "ShipMass = " + Reference.CalculateShipMass().PhysicalMass.ToString("0.00") + "\n";

    Go2PointSpeed(Reference, thrusters, maxThrustAxes, testDestination, testSpeed);

    lcdText += "Last Runtime Execution(ms): " + Runtime.LastRunTimeMs.ToString("0.00") + "\n";

    debugLCD.WriteText(lcdText);
}

/**
 * The main point and speed function. Now with 100% more maths!
 */
public void Go2PointSpeed(IMyShipController REFERENCE, List<IMyThrust> thrusters, double[] maxThrustAxes, Vector3D targetPos, double targetSpeed) {
    Vector3D vector2Target = targetPos - REFERENCE.GetPosition();
    Vector3D shipVelocity = REFERENCE.GetShipVelocities().LinearVelocity; // m/s
    Vector3D gravity = REFERENCE.GetNaturalGravity(); // m/s^2
    lcdText += "Gravity: " + gravity.ToString("0.00") + "||" + gravity.Length().ToString("0.000") + "\n";
    lcdText += "Velocity: " + shipVelocity.ToString("0.00") + "\n";

    // Calculate acceleration for skew correction
    Vector3D skewUnitV = Vector3D.Normalize(Vector3D.Cross(Vector3D.Cross(vector2Target, shipVelocity), vector2Target));
    Vector3D vSkew = ScalarProjection(shipVelocity, skewUnitV) * skewUnitV;
    Vector3D aSkew = -vSkew/tAcc;
    if (System.Double.IsNaN(aSkew.X)) aSkew = Vector3D.Zero;
    lcdText += "Skew Accel: " + aSkew.ToString("0.00") + "\n";

    // Calculate minimum distance to stop given current acceleration vector and velocity
    // Make give us a little less acceleration than we actually have to be safe
    double accelAlongAxis = (0.90 * GetMaxAccelerationAlongAxis(REFERENCE, maxThrustAxes, -vector2Target)) - ScalarProjection(gravity, vector2Target);
    double dStop = (Math.Pow(targetSpeed, 2) - Math.Pow(ScalarProjection(shipVelocity, vector2Target), 2)) / (2 * accelAlongAxis);
    lcdText += "dStop: " + dStop.ToString("0.00") + "\n";
    lcdText += "axisAccel=" + GetMaxAccelerationAlongAxis(REFERENCE, maxThrustAxes, -vector2Target).ToString("0.00") + "\n";

    // Calculate acceleration towards target position and velocity
    Vector3D aAccel;
    // Check if we need to deaccelerate rn
    if (Math.Abs(dStop) >= vector2Target.Length() && ScalarProjection(shipVelocity, vector2Target) > 0) { // deaccelerate
        aAccel = -shipVelocity / tAcc;
        lcdText += "Deaccelerating\n";
    }
    else { // accelerate
        double aAccelScale = (maxSpeed - ScalarProjection(shipVelocity, vector2Target)) / tAcc;
        aAccel = aAccelScale * Vector3D.Normalize(vector2Target);
        lcdText += "Accelerating\n";
    }
    lcdText += "To Target Accel: " + aAccel.ToString("0.00") + "\n";
    
    // Sum vectors and calculate final acceleration vector
    Vector3D shipAccel = aSkew + aAccel + (-gravity);

    // Turn off overrides to stop
    if (targetSpeed == 0 && vector2Target.Length() < dampeningRadius) {
        shipAccel = Vector3D.Zero;
    }

    lcdText += "Total Accel: " + shipAccel.ToString("0.00") + "\n";

    ApplyAccelerationToThrusters(REFERENCE, maxThrustAxes, shipAccel);
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
    double[] maxThrust = new double[6]; // Forward, Backward, Left, Right, Up, Down

    Base6Directions.Direction refForward = REFERENCE.Orientation.Forward;
    Base6Directions.Direction refBackward = Base6Directions.GetOppositeDirection(refForward);
    Base6Directions.Direction refLeft = REFERENCE.Orientation.Left;
    Base6Directions.Direction refRight = Base6Directions.GetOppositeDirection(refLeft);
    Base6Directions.Direction refUp = REFERENCE.Orientation.Up;
    Base6Directions.Direction refDown = Base6Directions.GetOppositeDirection(refUp);

    // Also rebuild thruster groups
    forwardThrusters = new List<IMyThrust>();
    backwardThrusters = new List<IMyThrust>();
    upThrusters = new List<IMyThrust>();
    downThrusters = new List<IMyThrust>();
    leftThrusters = new List<IMyThrust>();
    rightThrusters = new List<IMyThrust>();

    for (int i = 0; i < thrusters.Count; i++) {
        if (thrusters[i].Orientation.Forward == refBackward) {
            maxThrust[0] += (double)thrusters[i].MaxEffectiveThrust;
            forwardThrusters.Add(thrusters[i]);
        }
        else if (thrusters[i].Orientation.Forward == refForward) {
            maxThrust[1] += (double)thrusters[i].MaxEffectiveThrust;
            backwardThrusters.Add(thrusters[i]);
        }
        else if (thrusters[i].Orientation.Forward == refRight) {
            maxThrust[2] += (double)thrusters[i].MaxEffectiveThrust;
            leftThrusters.Add(thrusters[i]);
        }
        else if (thrusters[i].Orientation.Forward == refLeft) {
            maxThrust[3] += (double)thrusters[i].MaxEffectiveThrust;
            rightThrusters.Add(thrusters[i]);
        }
        else if (thrusters[i].Orientation.Forward == refDown) {
            maxThrust[4] += (double)thrusters[i].MaxEffectiveThrust;
            upThrusters.Add(thrusters[i]);
        }
        else if (thrusters[i].Orientation.Forward == refUp) {
            maxThrust[5] += (double)thrusters[i].MaxEffectiveThrust;
            downThrusters.Add(thrusters[i]);
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

/**
 * Applies an acceleration vector to all grid thrusters and automaticly clamps to max thrust
 */
public void ApplyAccelerationToThrusters(IMyShipController REFERENCE, double[] maxThrustAxes, Vector3D accel) {
    // Forward, Backward, Left, Right, Up, Down

    double forwardTargetAccel = ScalarProjection(accel, REFERENCE.WorldMatrix.Forward);
    double leftTargetAccel = ScalarProjection(accel, REFERENCE.WorldMatrix.Left);
    double upTargetAccel = ScalarProjection(accel, REFERENCE.WorldMatrix.Up);

    lcdText += "\n++Aligned Accels:\n";
    lcdText += "forward=" + forwardTargetAccel.ToString("0.00") + "\n";
    lcdText += "up=" + upTargetAccel.ToString("0.00") + "\n";
    lcdText += "left=" + leftTargetAccel.ToString("0.00") + "\n";

    if (forwardTargetAccel > 0) {
        double forwardOverridePercent = forwardTargetAccel/maxThrustAxes[0];
        forwardOverridePercent = (forwardOverridePercent > 1) ? 1 : forwardOverridePercent;
        foreach(IMyThrust forwardThruster in forwardThrusters) {
            forwardThruster.ThrustOverridePercentage = (float)forwardOverridePercent;
        }
        foreach(IMyThrust backwardThruster in backwardThrusters) {
            backwardThruster.ThrustOverridePercentage = 0;
        }
    }
    else {
        double backwardOverridePercent = -forwardTargetAccel/maxThrustAxes[1];
        backwardOverridePercent = (backwardOverridePercent > 1) ? 1 : backwardOverridePercent;
        foreach(IMyThrust backwardThruster in backwardThrusters) {
            backwardThruster.ThrustOverridePercentage = (float)backwardOverridePercent;
        }
        foreach(IMyThrust forwardThruster in forwardThrusters) {
            forwardThruster.ThrustOverridePercentage = 0;
        }
    }

    if (leftTargetAccel > 0) {
        double leftOverridePercent = leftTargetAccel/maxThrustAxes[2];
        leftOverridePercent = (leftOverridePercent > 1) ? 1 : leftOverridePercent;
        foreach(IMyThrust leftThruster in leftThrusters) {
            leftThruster.ThrustOverridePercentage = (float)leftOverridePercent;
        }
        foreach(IMyThrust rightThruster in rightThrusters) {
            rightThruster.ThrustOverridePercentage = 0;
        }
    }
    else {
        double rightOverridePercent = -leftTargetAccel/maxThrustAxes[3];
        rightOverridePercent = (rightOverridePercent > 1) ? 1 : rightOverridePercent;
        foreach(IMyThrust rightThruster in rightThrusters) {
            rightThruster.ThrustOverridePercentage = (float)rightOverridePercent;
        }
        foreach(IMyThrust leftThruster in leftThrusters) {
            leftThruster.ThrustOverridePercentage = 0;
        }
    }

    if (upTargetAccel > 0) {
        double upOverridePercent = upTargetAccel/maxThrustAxes[4];
        upOverridePercent = (upOverridePercent > 1) ? 1 : upOverridePercent;
        foreach(IMyThrust upThruster in upThrusters) {
            upThruster.ThrustOverridePercentage = (float)upOverridePercent;
        }
        foreach(IMyThrust downThruster in downThrusters) {
            downThruster.ThrustOverridePercentage = 0;
        }
    }
    else {
        double downOverridePercent = -upTargetAccel/maxThrustAxes[5];
        downOverridePercent = (downOverridePercent > 1) ? 1 : downOverridePercent;
        foreach(IMyThrust downThruster in downThrusters) {
            downThruster.ThrustOverridePercentage = (float)downOverridePercent;
        }
        foreach(IMyThrust upThruster in upThrusters) {
            upThruster.ThrustOverridePercentage = 0;
        }
    }
}

