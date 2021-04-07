using ProtoBuf;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace qptech.src
{
    class Qptech : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);


            // Blocks Class
            api.RegisterBlockClass("BlockSingleSink", typeof(BlockSingleSink));
            api.RegisterBlockClass("BlockDoubleSink", typeof(BlockDoubleSink));
            api.RegisterBlockClass("BlockIceBox", typeof(BlockIceBox)); // taking from Github new one and better and less work.
            api.RegisterBlockClass("ModdedBlockLiquidContainerBase", typeof(ModdedBlockLiquidContainerBase));
            // Block Entity Class 
            api.RegisterBlockEntityClass("SingleSink", typeof(BlockEntitySingleSink));
            api.RegisterBlockEntityClass("DoubleSink", typeof(BlockEntityDoubleSink));
            api.RegisterBlockEntityClass("IceBox", typeof(BlockEntityIceBox));  // taking from Github new one and better and less work.
            api.RegisterBlockEntityClass("ModdedFirepit", typeof(BlockEntityModdedFirepit));
        }
    }
}