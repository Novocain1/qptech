using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Server;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.ServerMods;
using qptech.src.extensions;


namespace qptech.src
{
    class BETemporalPocket:BlockEntityGenericContainer
    {
        string accessing = "";
        protected override void OnInvOpened(IPlayer player)
        {
            if (simpleinventory.openinventories == null) { simpleinventory.openinventories = new List<string>(); }
            if (simpleinventory.openinventories.Contains(player.PlayerUID)) { return; }
            if (accessing != "") { return; }
            accessing = player.PlayerUID;
            simpleinventory.openinventories.Add(player.PlayerUID);
            TryLoadInventory(player);
            base.OnInvOpened(player);
        }

        protected override void OnInvClosed(IPlayer player)
        {
            TrySaveInventory(player);
            simpleinventory.openinventories.Remove(player.PlayerUID);
            accessing = "";
            this.MarkDirty();
            
            base.OnInvClosed(player);
        }
        
        void TryLoadInventory(IPlayer player)
        {
            this.Inventory.DiscardAll();
            simpleinventory loadedinv = ApiExtensions.LoadOrCreateDataFile<simpleinventory>(Api, "helloworld.json");
            
            if (loadedinv == null) { return; }
            if (loadedinv.inventory == null) { return; }
            if (loadedinv.inventory.Count == 0) { return; }
            ItemSlot[] myslots = this.Inventory.ToArray();
            int maxslots = Math.Min(myslots.Length, loadedinv.blockoritem.Count);

            for (int c= 0;c<maxslots;c++)
            {
                //if too many slots are stored the inventory will be truncated
                
                if (loadedinv.blockoritem[c] == "BLOCK")
                {
                    Block outputItem = Api.World.GetBlock(new AssetLocation(loadedinv.inventory[c]));
                    if (outputItem == null) {  continue; }

                    ItemStack outputStack = new ItemStack(outputItem, loadedinv.quantity[c]);
                    myslots[c].Itemstack = outputStack;
                    
                }
                else if (loadedinv.blockoritem[c] == "ITEM")
                {
                    Item outputItem = Api.World.GetItem(new AssetLocation(loadedinv.inventory[c]));
                    if (outputItem == null) { continue; }

                    ItemStack outputStack = new ItemStack(outputItem, loadedinv.quantity[c]);
                    myslots[c].Itemstack = outputStack;
                }
            }
            this.MarkDirty();
        }
        public override void OnBlockBroken()
        {
            this.Inventory.DiscardAll();
            base.OnBlockBroken();
        }

        public override void OnBlockRemoved()
        {
            this.Inventory.DiscardAll();
            base.OnBlockRemoved();
        }
        public override void OnBlockUnloaded()
        {
            this.Inventory.DiscardAll();
            base.OnBlockUnloaded();
        }
        void TrySaveInventory(IPlayer player)
        {
            simpleinventory items = new simpleinventory();
            items.uid = player.PlayerUID;
            items.uname = player.PlayerName;
            items.StoreInventory(this.Inventory.ToList());
            
            ApiExtensions.SaveDataFile<simpleinventory>(Api, "helloworld.json", items);
            this.Inventory.DiscardAll();
        }
    }

    public class simpleinventory
    {
        public string uid;
        public string uname;
        public List<string> inventory;
        public List<int> quantity;
        public List<string> blockoritem;
        
        //TODO NEED TO FIX THIS UP SO THAT THE KEY IS SLOT# AND NOT INVENTORY KEY
        public simpleinventory()
        {
            if (openinventories == null) { openinventories = new List<string>(); }
            inventory = new List<string>();
            quantity = new List<int>();
            blockoritem = new List<string>();
        }
        public void StoreInventory(List<ItemSlot> itemslots)
        {
            inventory = new List<string>();
            quantity = new List<int>();
            blockoritem = new List<string>();
            foreach (ItemSlot itemslot in itemslots)
            {
                if (itemslot.StackSize > 0)
                {
                    
                    if (itemslot.Itemstack.Item != null)
                    {
                        string code = itemslot.Itemstack.Item.Code.ToShortString();
                        inventory.Add(code);
                        quantity.Add(itemslot.StackSize);
                        blockoritem.Add("ITEM");
                    }
                    if (itemslot.Itemstack.Block != null)
                    {
                        string code = itemslot.Itemstack.Block.Code.ToShortString();
                        inventory.Add(code);
                        quantity.Add(itemslot.StackSize);
                        blockoritem.Add("BLOCK");
                    }
                }
            }

        }
        public static List<string> openinventories;
    }
}
