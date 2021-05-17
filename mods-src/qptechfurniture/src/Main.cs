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
			api.RegisterBlockClass("BlockIceBox", typeof(BlockIceBox)); 
			api.RegisterBlockClass("ModdedBlockLiquidContainerBase", typeof(ModdedBlockLiquidContainerBase));
			api.RegisterBlockClass("BlockCampFire", typeof(BlockCampFire));
			api.RegisterBlockClass("BlockKiln", typeof(BlockKiln));
			api.RegisterBlockClass("BlockPlantLongContainer", typeof(BlockPlantLongContainer));

			// Item Class 
			api.RegisterItemClass("ItemCampFire", typeof(ItemCampFire));
			api.RegisterItemClass("ItemKiln", typeof(ItemKiln));
			// Block Entity Class 
			api.RegisterBlockEntityClass("SingleSink", typeof(BlockEntitySingleSink));
			api.RegisterBlockEntityClass("BlockEntityIceBox", typeof(BlockEntityIceBox));
			api.RegisterBlockEntityClass("CampFire", typeof(BlockEntityCampFire));
			api.RegisterBlockEntityClass("Kiln", typeof(BlockEntityKiln));
			api.RegisterBlockEntityClass("LongPlantContainer", typeof(BlockEntityLongPlantContainer));
		}
	}
}