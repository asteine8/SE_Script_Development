
// Result of research: IMySlimBlocks are not availible for non-terminal blocks. However they still can be detected via bool IMyCubeGrid.CubeExists().

List<IMySlimBlock> SlimBlocks = new List<IMySlimBlock>();

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks (0.6 Hz)
}

void Main(string arg) {
    SlimBlocks = new List<IMySlimBlock>();
    IMyCubeGrid Cubes = Me.CubeGrid as IMyCubeGrid;
    // Echo(Cubes.ToString());
    // IMyCubeGrid.GetBlocks(SlimBlocks);

    // Echo(Cubes.Max.ToString());
    // Echo(Cubes.Min.ToString());
    // Echo(Cubes.GridSize.ToString("0.00") + "m");
    VRageMath.Vector3I testPos = new VRageMath.Vector3I(0,0,0);
    for (int x = Cubes.Min.X; x <= Cubes.Max.X; x++) {
        for (int y = Cubes.Min.Y; y <= Cubes.Max.Y; y++) {
            for (int z = Cubes.Min.Z; z <= Cubes.Max.Z; z++) {
                testPos.X = x;
                testPos.Y = y;
                testPos.Z = z;
                try {
                    if (Cubes.CubeExists(testPos)) {
                        SlimBlocks.Add(Cubes.GetCubeBlock(testPos) as IMySlimBlock);
                        // Echo(Cubes.GetCubeBlock(testPos).ToString() + "\n");
                    }
                }
                catch {
                    Echo("fail");
                }
                
            }
        }
    }
    // Echo("hit");
    Echo(SlimBlocks.Count.ToString());
    // Echo(SlimBlocks.GetType().ToString());
    // Echo(SlimBlocks[2].BuildLevelRatio.ToString());
    for (int i = 0; i < SlimBlocks.Count; i++) {
        try {
            Echo(SlimBlocks[i].FatBlock.DefinitionDisplayNameText);
        }
        catch {
            Echo("Fail");
        }
    }
}