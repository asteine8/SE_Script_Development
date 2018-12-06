
IMyTextPanel LCD1;
IMyTextPanel LCD2;

string LCD1Name = "Component Inventory LCD1";
string LCD2Name = "Component Inventory LCD2";
string CargoTag = "Component Cargo";

public class ItemGroup {
    public int numItems;
    public string ItemType;

    public ItemGroup(int nItems, string iType) {
        numItems = nItems;
        ItemType = iType;
    }
}

public Program() {
    // Runtime.UpdateFrequency = UpdateFrequency.Update100; // Set update frequency to every 100 ticks (0.6Hz)

    LCD1 = GridTerminalSystem.GetBlockWithName(LCD1Name) as IMyTextPanel;
    LCD2 = GridTerminalSystem.GetBlockWithName(LCD2Name) as IMyTextPanel;
}

void Main(string argument) {
    List<IMyCargoContainer> Cargo = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType(Cargo);



    List<ItemGroup> ItemGroups = new List<ItemGroup>();

    for (int i = 0; i < Cargo.Count; i++) {

        if (Cargo[i].CustomData.Equals(CargoTag)) {

            List<IMyInventoryItem> InvItems = new List<IMyInventoryItem>();
            InvItems = Cargo[i].GetInventory(0).GetItems();
 
            for (int j = 0; j < InvItems.Count; j++) {
                string[] splitInfo = InvItems[j].ToString().Split(new string[]{@"/", " "}, StringSplitOptions.RemoveEmptyEntries);
                
                // for (int k = 0; k < splitInfo.Length; k++) {
                //     Echo(k.ToString() + splitInfo[k]);
                // }

                int nItems = Convert.ToInt32(Double.Parse(splitInfo[0].Remove(splitInfo[0].Length - 1)));
                string iType = splitInfo[2];

                bool IsDuplicate = false;

                for (int k = 0; k < ItemGroups.Count; k++) {
                    if (ItemGroups[k].ItemType.Equals(iType)) {
                        ItemGroups[k].numItems += nItems;
                        IsDuplicate = true;
                    }
                }
                if (!IsDuplicate) {
                    ItemGroups.Add(new ItemGroup(nItems, iType));
                }

            }
        }
    }

    string output1 = " ==Items in storage==\n\n";
    string output2 = " ==Items in storage==\n\n";

    if (ItemGroups.Count > 10) {
        for (int i = 0; i < Convert.ToInt32(Math.Round((float)ItemGroups.Count/2)); i++) {
            output1 += " " + ItemGroups[i].numItems + " " + ItemGroups[i].ItemType + "\n";
        }
        for (int i = Convert.ToInt32(Math.Round((float)ItemGroups.Count/2)); i < ItemGroups.Count; i++) {
            output2 += " " + ItemGroups[i].numItems + " " + ItemGroups[i].ItemType + "\n";
        }
    }
    else {
        for (int i = 0; i < ItemGroups.Count; i++) {
            output1 += " " + ItemGroups[i].numItems + " " + ItemGroups[i].ItemType + "\n";
        }
    }
    // Echo(output);
    LCD1.WritePublicText(output1);
    LCD2.WritePublicText(output2);
}