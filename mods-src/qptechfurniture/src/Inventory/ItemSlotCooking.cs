using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace QptechFurniture.src
{
    class ItemSlotCooking : ItemSlot
    {

        public int outputSlotId;

        public ItemSlotCooking(InventoryBase inventory, int outputSlotId)
          : base(inventory)
          => this.outputSlotId = outputSlotId;


        private bool IsCooking(ItemSlot itemSlot)
        {
            if (itemSlot.Itemstack != null)
            { return itemSlot.Itemstack.Collectible is BlockSmeltingContainer is false; } // Block people from using things in a slot. :P
            //{ return itemSlot.Itemstack.Collectible is ItemFood is false && itemSlot.Itemstack.Collectible is BlockCookingContainer is false; }
            else
                return false;
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