using Vintagestory.API.Common;

namespace QptechFurniture.src
{
	class QptechFurniture : ModSystem
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
			api.RegisterBlockEntityClass("BlockEntityIceBox", typeof(BlockEntityIceBox));  // taking from Github new one and better and less work.
		}
	}
}