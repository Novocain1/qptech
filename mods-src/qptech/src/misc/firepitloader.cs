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

namespace qptech.src
{
    //Production Furnace
    // - find fuel chest
    // - find fireplace(s) beside
    // - find output chest
    // MAIN LOOP
    //  - if fireplace has a finished good, and output chest not full move to output chest (end)
    //  - if input chest is empty (end)
    //  - if fireplace has stuff cooking and has fuel (end)
    //  - if fireplace has stuff cooking and no fuel and fuel chest has fuel:
    //           - take fuel from input
    //           - add fuel to fire
    //           (end)
    //  
    public class FirepitLoader : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockEntityClass("FirepitLoader", typeof(FirepitLoaderEntity));
        }

        public class FirepitLoaderEntity : BlockEntity
        {
            public override void Initialize(ICoreAPI api)
            {
                base.Initialize(api);
                RegisterGameTickListener(OnTick, 200);
            }
            BlockEntity checkblock;
            
            public void OnTick(float par)
            {
                
                //Check for fireplace (use first found)
                BlockPos checkPos = new BlockPos(Pos.X, Pos.Y-1, Pos.Z);
                

                var firepit = Api.World.BlockAccessor.GetBlockEntity(checkPos) as BlockEntityFirepit;
                //NO Firepit then you musta quit
                if (firepit == null) { return; }
                if (firepit.inputSlot == null) { return; }
                if (firepit.inputSlot.StackSize > 0) { return; }
                //Find Input chest
                checkPos = new BlockPos(Pos.X, Pos.Y + 1, Pos.Z);
                var inputContainer = Api.World.BlockAccessor.GetBlockEntity(checkPos) as BlockEntityContainer;
                if (inputContainer == null) { return; }
                ItemSlot sourceSlot = inputContainer.Inventory.GetAutoPullFromSlot(BlockFacing.DOWN);
                if (sourceSlot == null) { return; }
                int quantity = 1;
                ItemStackMoveOperation op = new ItemStackMoveOperation(Api.World, EnumMouseButton.Left, 0, EnumMergePriority.DirectMerge, quantity);

                int qmoved = sourceSlot.TryPutInto(firepit.inputSlot, ref op);
                firepit.outputSlot.MarkDirty();
                sourceSlot.MarkDirty();
            }
        }
    }
}
