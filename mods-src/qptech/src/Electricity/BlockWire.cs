using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common.Entities;

namespace qptech.src { 
    class BlockWire: ElectricalBlock
    {
        public override void OnEntityCollide(IWorldAccessor world, Entity entity, BlockPos pos, BlockFacing facing, Vec3d collideSpeed, bool isImpact)
        {
            base.OnEntityCollide(world, entity, pos, facing, collideSpeed, isImpact);
            var mywire= world.BlockAccessor.GetBlockEntity(pos) as BEEWire;
            if (mywire != null) { mywire.EntityCollide(entity); }
        }
    }
}
