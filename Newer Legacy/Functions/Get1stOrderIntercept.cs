/**
 * Calculates the first order launch vector
 * 
 * @param REF_RC    Reference Remote Control
 * @param St        Target Position
 * @param Vt        Target Velocity
 * @param Vf        Projectile speed
 * 
 * @return          The direction vector 
 */
Vector3D Get1stOrderLaunchVector(IMyRemoteControl REF_RC, Vector3D St, Vector3D Vt, double Vf) {
    Vector3D Sp = REF_RC.GetPosition(); // Initial projectile location
    Vector3D Vp = REF_RC.GetShipVelocities().LinearVelocity; // Projectile additive velocity

    double Spx = Sp.X; double Spy = Sp.Y; double Spz = Sp.Z;
    double Stx = St.X; double Sty = St.Y; double Stz = St.Z;
    double Vpx = Vp.X; double Vpy = Vp.Y; double Vpz = Vp.Z;
    double Vtx = Vt.X; double Vty = Vt.Y; double Vtz = Vt.Z;

    double a = Spx*Vpx - Stx*Vpx + Spy*Vpy - Sty*Vpy + Spz*Vpz - Stz*Vpz - Spx*Vtx + Stx*Vtx - Spy*Vty + Sty*Vty - Spz*Vtz + Stz*Vtz;
    double b = Math.Sqrt(
                    Math.Pow((Spy*Vpy - Sty*Vpy + Spz*Vpz - Stz*Vpz + Spx*(Vpx - Vtx) + Stx*(-Vpx + Vtx) - Spy*Vty + Sty*Vty - Spz*Vtz + Stz*Vtz),2)
                    + (Math.Pow(Spx,2) + Math.Pow(Spy,2) + Math.Pow(Spz,2) - 2*Spx*Stx + Math.Pow(Stx,2) - 2*Spy*Sty + Math.Pow(Sty,2) - 2*Spz*Stz + Math.Pow(Stz,2))
                    *(Math.Pow(Vf,2) - Math.Pow(Vpx,2) - Math.Pow(Vpy,2) - Math.Pow(Vpz,2) + 2*Vpx*Vtx - Math.Pow(Vtx,2) + 2*Vpy*Vty - Math.Pow(Vty,2) + 2*Vpz*Vtz - Math.Pow(Vtz,2))
                    );
    double c = (Math.Pow(Vf,2) + Math.Pow(Vpx,2) + Math.Pow(Vpy,2) + Math.Pow(Vpz,2) - 2*Vpx*Vtx + Math.Pow(Vtx,2) - 2*Vpy*Vty + Math.Pow(Vty,2) - 2*Vpz*Vtz + Math.Pow(Vtz,2));

    if (c==0) return Vector3D.Zero; // Unable to divide by 0

    double t1 = -((a - b) / c);
    double t2 = -((a + b) / c);

    double t;

    if (t1 < 0 && t2 < 0) return Vector3D.Zero; // Invalid: No targets availible

    if (t1 < 0) {
        t = t2;
    }
    else if (t2 < 0) {
        t = t1;
    }
    else { // Both positive roots
        t = (t1 < t2) ? t1 : t2; // Get the smaller of the two
    }

    Vector3D VelLaunch = (St-Sp)/t + Vt - Vp;
    Echo(VelLaunch.Length());
    return Vector3D.Normalize(VelLaunch);
}