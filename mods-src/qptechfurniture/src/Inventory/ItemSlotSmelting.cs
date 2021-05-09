using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace QptechFurniture.src
{
    class ItemSlotSmelthing : ItemSlot
    {



        public string[] itemlist = { "crucible", "rawbrick", "refractorybrick", "ingot", "ingotmold", "toolmold" };

        public int outputSlotId;

        public ItemSlotSmelthing(InventoryBase inventory, int outputSlotId)
          : base(inventory)
          => this.outputSlotId = outputSlotId;

        private bool IsCooking(ItemSlot itemSlot)
        {
            if (itemSlot.Itemstack != null)
             {
                  return 
                        itemSlot.Itemstack.Collectible.Code.GetName().StartsWith(itemlist[0]) ||
                         itemSlot.Itemstack.Collectible.Code.GetName().StartsWith(itemlist[1]) ||
                          itemSlot.Itemstack.Collectible.Code.GetName().StartsWith(itemlist[2]) ||
                           itemSlot.Itemstack.Collectible.Code.GetName().StartsWith(itemlist[3]) ||
                            itemSlot.Itemstack.Collectible.Code.GetName().StartsWith(itemlist[4]) ||
                             itemSlot.Itemstack.Collectible.Code.GetName().StartsWith(itemlist[5]);
             }
             else return false;

        }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            if (IsCooking(itemstackFromSourceSlot))
                return base.CanHold(itemstackFromSourceSlot);
            else
            {
                itemstackFromSourceSlot.MarkDirty();
                this.MarkDirty();
                return false;
            }
        }

        public override bool CanTakeFrom(ItemSlot sourceSlot, EnumMergePriority priority = EnumMergePriority.AutoMerge)
        {
            if (IsCooking(sourceSlot))
                return base.CanTakeFrom(sourceSlot);
            else
            {
                sourceSlot.MarkDirty();
                this.MarkDirty();
                return false;
            }
        }

    }
}