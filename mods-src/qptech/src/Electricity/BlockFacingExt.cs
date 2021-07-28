using Vintagestory.API.MathTools;

namespace qptech.src
{
    static class BlockFacingExt
    {
        public static BlockFacing FromIndex(int index)
        {
            switch (index)
            {
                case 0: return BlockFacing.NORTH;
                case 1: return BlockFacing.SOUTH;
                case 2: return BlockFacing.EAST;
                case 3: return BlockFacing.WEST;
                case 4: return BlockFacing.UP;
                case 5: return BlockFacing.DOWN;
            }

            return null;
        }
    }
}
