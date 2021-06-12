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
                //Test if there is somewhere to put stuff
                BlockPos checkPos = new BlockPos(Pos.X, Pos.Y - 1, Pos.Z);
                var outputContainer = Api.World.BlockAccessor.GetBlockEntity(checkPos) as BlockEntityContainer;
                if (outputContainer == null) { return; }
                
                //find a firepit or forge above us
                checkPos = new BlockPos(Pos.X, Pos.Y+1, Pos.Z);
                
                checkblock = Api.World.BlockAccessor.GetBlockEntity(checkPos);
                //Nothing at firepit location, nothing we can do, there is no point, good day sir!
                
                var firepit = checkblock as BlockEntityFirepit;
                var forge = checkblock as BEEForge;
                ItemSlot targetSlot;
                //NO Firepit then you musta quit
                if (firepit == null&&forge==null) { return; }
                if (forge!=null && !(forge.Unloadable && forge.ContentsReady)) { return; }
                else {
                    
                    DummyInventory di = new DummyInventory(Api,1);
                    di.Slots[0].Itemstack = forge.Contents;
                    int quantity = 1;
                    
                    targetSlot = outputContainer.Inventory.GetAutoPushIntoSlot(BlockFacing.UP, di.Slots[0]);
                    ItemStackMoveOperation op = new ItemStackMoveOperation(Api.World, EnumMouseButton.Left, 0, EnumMergePriority.DirectMerge, quantity);

                    int qmoved = di[0].TryPutInto(targetSlot, ref op);
                    if (qmoved == 0) { 
                        forge.Contents.StackSize -= 1;
                        if (forge.Contents.StackSize == 0)
                        {
                            forge.ClearContents();
                        }
                    }
                    forge.MarkDirty(true);
                    
                    return;
                }
                if (firepit.outputStack == null) { return; }
                if (firepit.outputStack.StackSize == 0) { return; }
                
                
               
                targetSlot = outputContainer.Inventory.GetAutoPushIntoSlot(BlockFacing.UP, firepit.outputSlot);
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
