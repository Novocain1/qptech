using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace QptechFurniture.src
{
    class SmelthingSlot : ItemSlot
    {

        public int outputSlotId;

        public SmelthingSlot(InventoryBase inventory, int outputSlotId)
          : base(inventory)
          => this.outputSlotId = outputSlotId;


        private bool IsCooking(ItemSlot itemSlot)
        {
            if (itemSlot.Itemstack != null)
            { return itemSlot.Itemstack.Collectible is BlockSmeltingContainer is false; }
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