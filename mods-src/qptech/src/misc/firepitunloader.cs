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
    //Firepit Unloader by WQP
    //This block will check for a fireplace on top of itself, and pull out any complete
    //  items and put them in suitable containers below
    //
    public class FirepitUnloader : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockEntityClass("FirepitUnloader", typeof(FirepitUnloaderEntity));
        }

        public class FirepitUnloaderEntity : BlockEntity
        {
            public override void Initialize(ICoreAPI api)
            {
                base.Initialize(api);
                RegisterGameTickListener(OnTick, 500);
            }
            BlockEntity checkblock;
            
            public void OnTick(float par)
            {
                
                //find a firepit above us
                BlockPos checkPos = new BlockPos(Pos.X, Pos.Y+1, Pos.Z);
                
                checkblock = Api.World.BlockAccessor.GetBlockEntity(checkPos);
                //Nothing at firepit location, nothing we can do, there is no point, good day sir!
                                
                var firepit = checkblock as BlockEntityFirepit;
                //NO Firepit then you musta quit
                if (firepit == null) { return; }
                if (firepit.outputStack == null) { return; }
                if (firepit.outputStack.StackSize == 0) { return; }
                checkPos = new BlockPos(Pos.X, Pos.Y -1, Pos.Z);
                var outputContainer = Api.World.BlockAccessor.GetBlockEntity(checkPos) as BlockEntityContainer;
                if (outputContainer == null) { return; }
               
                ItemSlot targetSlot = outputContainer.Inventory.GetAutoPushIntoSlot(BlockFacing.UP, firepit.outputSlot);
                if (targetSlot != null)
                {
                    int quantity = 1;
                    ItemStackMoveOperation op = new ItemStackMoveOperation(Api.World, EnumMouseButton.Left, 0, EnumMergePriority.DirectMerge, quantity);

                    int qmoved = firepit.outputSlot.TryPutInto(targetSlot, ref op);
                    firepit.outputSlot.MarkDirty();
                    targetSlot.MarkDirty();
                }
                              

            }
        }
    }
}
