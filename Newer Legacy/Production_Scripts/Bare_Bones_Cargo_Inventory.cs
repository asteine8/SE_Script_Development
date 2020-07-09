
int itemsPerLCD = 10;

public class Item {
    public string itemName;
    public int numItems;

    public MyItemType type;

    public Item(MyItemType t, int num) {
        type = t;
        numItems = num;
        itemName = "";
    }

    public void Add(int bNumItems) {
        this.numItems += bNumItems;
    }

    // public boolean isSameTypeAs(Item b) {
    //     return this.type == b.type;
    // }
}

public Program() {

}

public void Main(string arg) {
    List<Item> items = GetItemsInCargos("<component>");
    DisplayItemsOnLCDs(items, "<welderlcd>");
}

public List<Item> GetItemsInCargos(string cargoTag) {
    List<Item> items = new List<Item>();

    List<IMyCargoContainer> cargoBocks = new List<IMyCargoContainer>();
    GridTerminalSystem.GetBlocksOfType(cargoBocks);

    foreach (IMyCargoContainer cargoContainer in cargoBocks) {
        // Check if we need to count this cargo's inventory
        if (cargoContainer.CustomName.ToLower().Contains(cargoTag.ToLower())) {
            List<MyInventoryItem> cargoItems = new List<MyInventoryItem>();
            cargoContainer.GetInventory().GetItems(cargoItems);

            foreach (MyInventoryItem inventoryItem in cargoItems) {
                bool itemExists = false;
                for (int i = 0; i < items.Count; i++) {
                    // Add to existing item if it exists
                    if (inventoryItem.Type.Equals(items[i].type)) {
                        // items[i] = new Item(inventoryItem.Type, items[i].numItems + inventoryItem.Amount.ToIntSafe());
                        items[i].Add(inventoryItem.Amount.ToIntSafe());
                        itemExists = true;
                        break;
                    }
                }
                if (!itemExists) {
                    // new item
                    Item newItem = new Item(inventoryItem.Type, inventoryItem.Amount.ToIntSafe());
                    newItem.itemName = inventoryItem.Type.SubtypeId;
                    items.Add(newItem);
                }
            }


        }
    }
    return items;
}

public void DisplayItemsOnLCDs(List<Item> items, string lcdTag) {
    List<IMyTextPanel> LCDs = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(LCDs);

    int itemIndex = 0;
    foreach(IMyTextPanel LCD in LCDs) {
        // Check if this a display
        if (LCD.CustomName.ToLower().Contains(lcdTag.ToLower())) {
            string output = "";
            for (int i = itemIndex; i < itemIndex + itemsPerLCD && i < items.Count; i++) {
                output += items[i].numItems.ToString() + " x ";
                output += items[i].itemName + "\n";
            }
            LCD.WriteText(output);
            itemIndex += itemsPerLCD;
        }
    }
}