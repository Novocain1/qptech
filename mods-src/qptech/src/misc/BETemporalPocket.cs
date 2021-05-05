using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
        public bool Busy => (accessing!="");
        protected override void OnInvOpened(IPlayer player)
        {
            if (simpleinventory.openinventories == null) { simpleinventory.openinventories = new List<string>(); }
           // if (simpleinventory.openinventories.Contains(player.PlayerUID)) { return; }
            //if (accessing != "") { return; }
            //accessing = player.PlayerUID;
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
            
            this.MarkDirty();
        }
        public override void OnBlockBroken()
        {
            Cleanup();
            base.OnBlockBroken();
        }
        void Cleanup()
        {
            
            this.Inventory.DiscardAll();
            if (accessing != "" && simpleinventory.openinventories != null && simpleinventory.openinventories.Contains(accessing))
            {
                simpleinventory.openinventories.Remove(accessing);
            }
        }
        public override void OnBlockRemoved()
        {
            Cleanup();
            base.OnBlockRemoved();
        }
        public override void OnBlockUnloaded()
        {
            Cleanup();
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
        public Dictionary<string, string> testdic;        
        
        
        public simpleinventory()
        {
            if (openinventories == null) { openinventories = new List<string>(); }
            testdic = new Dictionary<string, string>();
            testdic.Add("keya", "valuea");
            testdic.Add("keyb", "valueb");
        }
        public void StoreInventory(List<ItemSlot> itemslots)
        {
            
        
            foreach (ItemSlot itemslot in itemslots)
            {
                if (itemslot.StackSize > 0)
                {

        
                }
            }

        }
        public static List<string> openinventories;
    }
}
