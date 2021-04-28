using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace qptech.src
{
    class BlockClayformer:ElectricalBlock
    {
        static Dictionary<string, string> variantlist;
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (variantlist == null) { LoadVariantList(); }
            //must sneak click
            if (!byPlayer.Entity.Controls.Sneak) { return base.OnBlockInteractStart(world, byPlayer, blockSel); }
            //must have a relevant item
            ItemStack stack = byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack;
            string itemorblockcode = "";
            if (stack.Item != null) { itemorblockcode = stack.Item.Code.ToString(); }
            else if (stack.Block != null) { itemorblockcode = stack.Block.Code.ToString(); }
            if (itemorblockcode == "") { return base.OnBlockInteractStart(world, byPlayer, blockSel); }
            if (!variantlist.ContainsKey(itemorblockcode)) { return base.OnBlockInteractStart(world, byPlayer, blockSel); }
            byPlayer.InventoryManager.ActiveHotbarSlot.TakeOut(1);
            string newblockname = variantlist[itemorblockcode];
            newblockname += "-"+this.LastCodePart();
            Block newclayformer = world.GetBlock(new AssetLocation(newblockname));
            if (newclayformer == null) { return base.OnBlockInteractStart(world, byPlayer, blockSel); }
            world.BlockAccessor.SetBlock(newclayformer.BlockId, blockSel.Position);
            return true;
        }

        static void LoadVariantList()
        {
            variantlist = new Dictionary<string, string>();
            variantlist.Add("game:bowl-burned", "machines:clayformer-bowl");
            variantlist.Add("game:clayplanter-burnt","machines:clayformer-clayplanter");
        }
    }
}
