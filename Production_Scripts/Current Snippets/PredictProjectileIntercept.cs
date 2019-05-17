
// Predicts a 1st Order intercept from target posistion and velocity
bool Predict1stOrderIntercept(IMyRemoteControl Ref_Controller, Vector3D TargetPosition, Vector3D TargetVelocity, double projectileSpeed, out Vector3D InterceptPosition) {
    Vector3D ShipPosistion = Ref_Controller.GetPosition();
    if ((TargetPosition-ShipPosistion).Length() < 1) { // We're 1 meter away from our target, don't even bother to waste time on the calculations
        Echo("Too Bloody Close, aborting calculation");
        InterceptPosition = TargetPosition; // return current position so we still try to aim
        return false;
    }
    
    if (TargetVelocity <= 0.01) { // Target is not moving, projectile will intercept at current target posistion
        InterceptPosition = TargetPosition;
        return true; // "Prediction" was still a success
    }
    if (projectileSpeed <= 0.1) { // Projectile is reeeeeaaaaaaly slow. Just return the target position as intercept and pray
        InterceptPosition = TargetPosition;
        return false; // Five dollars says this will miss since the target is moving
    }

    // Now that we have a moving target a ways from the ship, we should calculate the intercept position

    // Calcuate the time to intercept position using das qwuadwatic formula
    Vector3D RelativeTargetPosition = TargetPosition - ShipPosistion;

    double t = 0;
    double a = (TargetVelocity.Length()^2-projectileSpeed^2);
    double b = 2*Vector3D.Dot(TargetVelocity, RelativeTargetPosition);
    double c = RelativeTargetPosition.Length()^2;

    // t = -b + Math.Sqrt(b^2-4*a*c); // use the (+) version
    t = -b - Math.Sqrt(b^2-4*a*c); // use the (-) version
    t /= 2*a;

    t = (t<0) ? 0 : t; // Check for negatives (would be bad)

    InterceptPosition = TargetPosition + TargetVelocity*t; // Use found time to get final intercept position

    // Debugging Stuff

    Echo("Intercept Function Debug line:");
    Echo("A:" + a.ToString("0.0") + " B:" + b.ToString("0.0") + " C:" + c.ToString("0.0"));
    Echo("t = " + t.ToString("0.0"));
    Echo(InterceptPosition.ToString("0.0"));

    return true;
}

// Predict a 1st order intercept from target position and velocity assuming that projectiles inherit velocity from parent
bool Predict1stOrderInterceptWithAdditiveV(IMyRemoteControl Ref_Controller, Vector3D TargetPosition, Vector3D TargetVelocity, double projectileSpeed, out Vector3D InterceptPosition) {
    Vector3D ShipPosistion = Ref_Controller.GetPosition();
    Vector3D ShipVelocity = Ref_Controller.GetShipVelocities().LinearVelocity;

    if ((TargetPosition-ShipPosistion).Length() < 1) { // We're 1 meter away from our target, don't even bother to waste time on the calculations
        // Echo("Too Bloody Close, aborting calculation");
        InterceptPosition = TargetPosition; // return current position so we still try to aim
        return false;
    }
    
    if (TargetVelocity <= 0.01) { // Target is not moving, projectile will intercept at current target posistion
        InterceptPosition = TargetPosition;
        return true; // "Prediction" was still a success
    }
    if (projectileSpeed <= 0.1) { // Projectile is reeeeeaaaaaaly slow. Just return the target position as intercept and pray
        InterceptPosition = TargetPosition;
        return false; // Five dollars says this will miss since the target is moving
    }

    // Now that we have a moving target a ways from the ship, we should calculate the intercept position

    // Calcuate the time to intercept position using das qwuadwatic formula
    double t = 0;
    double a = Math.Pow(projectileSpeed,2) + 2*Vector3D.Dot(ShipVelocity,)

}

