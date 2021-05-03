using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace QptechFurniture.src
{
    /// <summary>
    /// Inventory with one fuel slot, one ore slot, one output slot and an optional 4 cooking slots
    /// </summary>
    public class InventoryCooking : InventoryBase, ISlotProvider 
    {

        ItemSlot[] slots;

        public BlockPos pos;

        /// <summary>
        /// Returns the cooking slots
        /// </summary>
        public ItemSlot[] Slots
        {
            get { return slots; }
        }

        public int CookingContainerMaxSlotStackSize
        {
            get
            {
                return slots[1].Itemstack.Collectible.Attributes["maxContainerSlotStackSize"].AsInt(64);
            }
        }

        public InventoryCooking(string inventoryID, ICoreAPI api) : base(inventoryID, api)
        {
            // slot 0 = fuel
            // slot 1 = input
            // slot 2 = output
            slots = GenEmptySlots(3);
            baseWeight = 4f;

        }

        public InventoryCooking(string className, string instanceID, ICoreAPI api) : base(className, instanceID, api)
        {
            slots = GenEmptySlots(3);
            baseWeight = 4f;
        }

        public override void LateInitialize(string inventoryID, ICoreAPI api)
        {
            base.LateInitialize(inventoryID, api);
        }

        public override int Count
        {
            get { return slots.Length; }
        }

        public override ItemSlot this[int slotId]
        {
            get
            {
                if (slotId < 0 || slotId >= Count) return null;
                return slots[slotId];
            }
            set
            {
                if (slotId < 0 || slotId >= Count) throw new ArgumentOutOfRangeException(nameof(slotId));
                if (value == null) throw new ArgumentNullException(nameof(value));
                slots[slotId] = value;
            }
        }

        public override void DidModifyItemSlot(ItemSlot slot, ItemStack extractedStack = null)
        {
            base.DidModifyItemSlot(slot, extractedStack);
        }

        public override float GetTransitionSpeedMul(EnumTransitionType transType, ItemStack stack)
        {
            return base.GetTransitionSpeedMul(transType, stack);
        }


        public override void FromTreeAttributes(ITreeAttribute tree)
        {
            List<ItemSlot> modifiedSlots = new List<ItemSlot>();
            slots = SlotsFromTreeAttributes(tree, slots, modifiedSlots);
            for (int i = 0; i < modifiedSlots.Count; i++) DidModifyItemSlot(modifiedSlots[i]);
        }



        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            SlotsToTreeAttributes(slots, tree);
        }

        public override void OnItemSlotModified(ItemSlot slot)
        {
            base.OnItemSlotModified(slot);
        }

        protected override ItemSlot NewSlot(int i)
        {
            if (i == 0) return new ItemSlotSurvival(this); // Fuel
            if (i == 1) return new ItemSlotCooking(this, 2); // Input for cooking.
            if (i == 2) return new ItemSlotOutput(this); // Output for things that are smelted

            return new ItemSlotWatertight(this);
        }


        public override WeightedSlot GetBestSuitedSlot(ItemSlot sourceSlot, List<ItemSlot> skipSlots = null)
        {
            WeightedSlot slot = base.GetBestSuitedSlot(sourceSlot, skipSlots);
            return slot;
        }


        public override float GetSuitability(ItemSlot sourceSlot, ItemSlot targetSlot, bool isMerge)
        {
            ItemStack stack = sourceSlot.Itemstack;

            if (targetSlot == slots[0] && (stack.Collectible.CombustibleProps == null || stack.Collectible.CombustibleProps.BurnTemperature <= 0)) return 0;
            if (targetSlot == slots[1] && (stack.Collectible.CombustibleProps == null || stack.Collectible.CombustibleProps.SmeltedStack == null)) return 0.5f;


            return base.GetSuitability(sourceSlot, targetSlot, isMerge);
        }


        public string GetOutputText()
        { 
            ItemStack inputStack = slots[1].Itemstack;

            if (inputStack == null) return null;
            if (inputStack.Collectible is BlockSmeltingContainer) return "You can't use (Crucible) in this, Use (Kiln)";
   
            ItemStack smeltedStack = inputStack.Collectible.CombustibleProps?.SmeltedStack?.ResolvedItemstack;
            if (smeltedStack == null) return null;
            if (inputStack.Collectible.CombustibleProps.RequiresContainer) return "Can't smelt, requires smelting container (i.e. Crucible)";
            return Lang.Get("firepit-gui-willcreate", inputStack.StackSize / inputStack.Collectible.CombustibleProps.SmeltedRatio, smeltedStack.GetName());
        }
    }
}