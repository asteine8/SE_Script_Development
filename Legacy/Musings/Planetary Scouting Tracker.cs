/*
    Black: \uE149
    White: \uE2FF
    Orange: \uE2D1
    Blue: \uE12D
    Yellow: \uE2F0
    Red: \uE2D1
    Purple: \uE1CF
    Green: \uE1F9

    // Graphical Matrix Size = 48x48
*/

IMyTextPanel LCD;


const int matSize = 48;
char[,] matrix = new char[matSize, matSize];


public Program() {
    LCD = GridTerminalSystem.GetBlockWithName("LCD") as IMyTextPanel;
}

void Main(string arg) {
    LCD.WritePublicText(Render(matrix));
}

string Render(char[,] pixels) {
    string output = "";

    for (int y = 0; y < matSize; y++) {
        for (int x = 0; x < matSize; x++) {
            output += pixels[x,y];
        }
        output += "\n";
    }
    return output;
}