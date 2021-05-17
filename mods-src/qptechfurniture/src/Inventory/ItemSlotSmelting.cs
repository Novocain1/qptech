using Vintagestory.API.Common;

namespace QptechFurniture.src
{
    class ItemSlotSmelthing : ItemSlot
    {
        public int outputSlotId;

        public ItemSlotSmelthing(InventoryBase inventory, int outputSlotId)
          : base(inventory)
          => this.outputSlotId = outputSlotId;

        private bool Issmeltable(ItemSlot itemSlot)
        {
            if (itemSlot.Itemstack != null)
             {
                bool Issmeltable = itemSlot.Itemstack.Collectible.Attributes != null && itemSlot.Itemstack.Collectible.Attributes["kilnCanUse"].AsBool();
                return Issmeltable;
             }
             else 
                return false;

        }

        public override bool CanHold(ItemSlot itemstackFromSourceSlot)
        {
            if (Issmeltable(itemstackFromSourceSlot))
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
            if (Issmeltable(sourceSlot))
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