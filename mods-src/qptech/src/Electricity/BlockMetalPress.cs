using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;


namespace qptech.src
{
    class BlockMetalPress:ElectricalBlock
    {
        static Dictionary<string, string> variantlist;
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (variantlist == null) { LoadVariantList(api); }
            //must sneak click
            if (!byPlayer.Entity.Controls.Sneak) { return base.OnBlockInteractStart(world, byPlayer, blockSel); }
            //must have a relevant item
            ItemStack stack = byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack;
            string itemorblockcode = "";
            if (stack == null)
            {
                BEEAssembler bee = (BEEAssembler)api.World.BlockAccessor.GetBlockEntity(blockSel.Position);
                if (bee != null) { bee.OpenStatusGUI(); return true; }
                return base.OnBlockInteractStart(world, byPlayer, blockSel);
            }
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

        static void LoadVariantList(ICoreAPI api)
        {

            //TODO Need to either swap the hardcoded path name out or figure out correct reference for the path
            //string path = Path.Combine(GamePaths.Cache, @"assets\machines\config\clayformerswaps.json");
            //string ad = AppDomain.CurrentDomain.BaseDirectory;
            //string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            //TODO Make sure to uncomment loading code once some swaps have been setup


            /*try
            {
                variantlist = api.Assets.TryGet("qptech:config/metalpressswaps.json").ToObject<Dictionary<string, string>>();
            }
            catch
            {
                variantlist = new Dictionary<string, string>();
                api.World.Logger.Error("metalpressswaps was not found sowee!");
            }*/
            variantlist = new Dictionary<string, string>();


        }
        public static string GetModCacheFolder(string modArchiveName)
        {
            var modCacheDir = new DirectoryInfo(Path.Combine(GamePaths.DataPath, "Cache", "unpack"));
            var myModCacheDir = modCacheDir.EnumerateDirectories()
                .FirstOrDefault(p => p.Name.StartsWith(modArchiveName));
            return myModCacheDir?.FullName;
        }
    }
}
