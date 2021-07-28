using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace qptech.src
{
    class BlockMPGenerator : ElectricalBlock
    {
        public int powerInIndex;

        public MeshData statorMesh;

        public override void OnLoaded(ICoreAPI api)
        {
            int index = BlockFacing.FromCode(Variant["side"]).Opposite.Index;
            powerInIndex = index + 1 > 3 ? 0 : index + 1;

            if (Attributes != null)
            {
                if (api.Side.IsClient())
                {
                    ICoreClientAPI capi = (ICoreClientAPI)api;
                    Block statorBlock = capi.World.GetBlock(CodeWithVariant("rotorstator", "stator"));
                    capi.Tesselator.TesselateBlock(statorBlock, out statorMesh);
                }

            }

            base.OnLoaded(api);
        }

        public override void DidConnectAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {

        }

        public override bool HasMechPowerConnectorAt(IWorldAccessor world, BlockPos pos, BlockFacing face)
        {
            return powerInIndex == face.Index;
        }

        public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(world, blockPos, byItemStack);
            
            foreach (BlockFacing face in BlockFacing.HORIZONTALS)
            {
                BlockPos pos = blockPos.AddCopy(face);
                IMechanicalPowerBlock block = world.BlockAccessor.GetBlock(pos) as IMechanicalPowerBlock;
                if (block != null)
                {
                    if (block.HasMechPowerConnectorAt(world, pos, face.Opposite))
                    {
                        block.DidConnectAt(world, pos, face.Opposite);
                        WasPlaced(world, blockPos, face);
                    }
                }
            }
        }
    }
}
