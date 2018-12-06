
IMyRemoteControl Remote;
IMyTextPanel LCD;

public Program() {
    Remote = GridTerminalSystem.GetBlockWithName("Remote Control") as IMyRemoteControl;
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;

    SetGyroOverrides(true);
}

void Main(string arg) {
    // ApplyRotationToGyros(.1f,0f,0f, Remote);
    ApplyRotationToGyros(0f,0f,0f, Remote);
}

void SetGyroOverrides(bool onOff) {
    List<IMyGyro> Gyros = new List<IMyGyro>();
    GridTerminalSystem.GetBlocksOfType(Gyros); // Get all gyros on grid

    // Cycle through all gyros
    for (int g = 0; g < Gyros.Count; g++) {
        Gyros[g].GyroOverride = onOff;
    }
}

void ApplyRotationToGyros(float yaw, float pitch, float roll, IMyRemoteControl ReferenceRemote) {
    List<IMyGyro> Gyros = new List<IMyGyro>();
    GridTerminalSystem.GetBlocksOfType(Gyros); // Get all gyros on grid

    MyBlockOrientation ReferenceOrientation = ReferenceRemote.Orientation; // Get orientations of reference
    MyBlockOrientation GyroOrientation;

    Base6Directions.Direction GyroForward;
    Base6Directions.Direction GyroUp; // Predeclare gyro variables

    Base6Directions.Direction RefForward = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Forward);
    Base6Directions.Direction RefBackward = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Backward);
    Base6Directions.Direction RefUp = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Up);
    Base6Directions.Direction RefDown = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Down);
    Base6Directions.Direction RefLeft = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Left);
    Base6Directions.Direction RefRight = ReferenceOrientation.TransformDirection(Base6Directions.Direction.Right); // Get Reference orientation directions relative to the grid

    for (int g = 0; g < Gyros.Count; g++) {
        GyroOrientation = Gyros[g].Orientation; // Get gyro orientation

        GyroForward = GyroOrientation.TransformDirection(Base6Directions.Direction.Forward);
        GyroUp = GyroOrientation.TransformDirection(Base6Directions.Direction.Up); // Get gyro cardinal directions relative to their orientation to the grid

        // Let's look at roll first
        if (GyroForward == RefForward) {
            Gyros[g].Roll = roll; // We know this off the bat

            if (GyroUp == RefUp) {
                Gyros[g].Yaw = yaw;
                Gyros[g].Pitch = pitch;
            }
            else if (GyroUp == RefDown) {
                Gyros[g].Yaw = -yaw;
                Gyros[g].Pitch = -pitch;
            }
            else if (GyroUp == RefLeft) {
                Gyros[g].Yaw = pitch;
                Gyros[g].Pitch = -yaw;
            }
            else { // GyroUp == RefRight
                Gyros[g].Yaw = -pitch;
                Gyros[g].Pitch = yaw;
            }
        }
        else if (GyroForward == RefBackward) {
            Gyros[g].Roll = -roll; // We know this off the bat

            if (GyroUp == RefUp) {
                Gyros[g].Yaw = -yaw;
                Gyros[g].Pitch = -pitch;
            }
            else if (GyroUp == RefDown) {
                Gyros[g].Yaw = yaw;
                Gyros[g].Pitch = pitch;
            }
            else if (GyroUp == RefLeft) {
                Gyros[g].Yaw = pitch;
                Gyros[g].Pitch = yaw;
            }
            else { // GyroUp == RefRight
                Gyros[g].Yaw = -pitch;
                Gyros[g].Pitch = -yaw; 
            }
        }
        else if (GyroForward == RefUp) {
            Gyros[g].Roll = -yaw; // We know this off the bat

            if (GyroUp == RefBackward) {
                Gyros[g].Yaw = roll;
                Gyros[g].Pitch = pitch;
            }
            else if (GyroUp == RefForward) {
                Gyros[g].Yaw = -roll;
                Gyros[g].Pitch = -pitch;
            }
            else if (GyroUp == RefLeft) {
                Gyros[g].Yaw = pitch;
                Gyros[g].Pitch = -roll;
            }
            else { // GyroUp == RefRight
                Gyros[g].Yaw = -pitch;
                Gyros[g].Pitch = roll;
            }
        }
        else if (GyroForward == RefDown) {
            Gyros[g].Roll = yaw; // We know this off the bat

            if (GyroUp == RefForward) {
                Gyros[g].Yaw = -roll;
                Gyros[g].Pitch = pitch;
            }
            else if (GyroUp == RefBackward) {
                Gyros[g].Yaw = roll;
                Gyros[g].Pitch = -pitch;
            }
            else if (GyroUp == RefLeft) {
                Gyros[g].Yaw = pitch;
                Gyros[g].Pitch = roll;
            }
            else { // GyroUp == RefRight
                Gyros[g].Yaw = -pitch;
                Gyros[g].Pitch = -roll;
            }
        }
        else if (GyroForward == RefLeft) {
            Gyros[g].Roll = -pitch; // We know this off the bat

            if (GyroUp == RefUp) {
                Gyros[g].Yaw = yaw;
                Gyros[g].Pitch = roll;
            }
            else if (GyroUp == RefDown) {
                Gyros[g].Yaw = -yaw;
                Gyros[g].Pitch = -roll;
            }
            else if (GyroUp == RefForward) {
                Gyros[g].Yaw = -roll;
                Gyros[g].Pitch = yaw;
            }
            else { // GyroUp == RefBackward
                Gyros[g].Yaw = roll;
                Gyros[g].Pitch = -yaw;
            }
        }
        else { // GyroForward == RefRight
            Gyros[g].Roll = pitch; // We know this off the bat

            if (GyroUp == RefUp) {
                Gyros[g].Yaw = yaw;
                Gyros[g].Pitch = -roll;
            }
            else if (GyroUp == RefDown) {
                Gyros[g].Yaw = -yaw;
                Gyros[g].Pitch = roll;
            }
            else if (GyroUp == RefForward) {
                Gyros[g].Yaw = -roll;
                Gyros[g].Pitch = -yaw;
            }
            else { // GyroUp == RefBackward
                Gyros[g].Yaw = roll;
                Gyros[g].Pitch = yaw;
            }
        }
    }
}