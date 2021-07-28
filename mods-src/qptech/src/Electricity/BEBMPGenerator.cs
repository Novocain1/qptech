using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace qptech.src
{
    class BEBMPGenerator : BEBehaviorMPBase
    {
        BEMPGenerator generator { get => Blockentity as BEMPGenerator; }

        public float RequiredTorque { get; set; } = 5.0f;
        public float OwnResistance { get; set; } = 0.003f;

        public BEBMPGenerator(BlockEntity blockentity) : base(blockentity)
        {
            Blockentity = blockentity;

            OutFacingForNetworkDiscovery = BlockFacingExt.FromIndex((Block as BlockMPGenerator).powerInIndex);
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            if (Block?.Attributes != null)
            {
                RequiredTorque = Block.Attributes["requiredTorque"].AsFloat(RequiredTorque);
                OwnResistance = Block.Attributes["ownResistance"].AsFloat(OwnResistance);
            }

            switch (BlockFacing.FromCode(Block.Variant["side"]).Index)
            {
                case 0:
                    AxisSign = new int[] { 1, 0, 0 };
                    break;
                case 1:
                    AxisSign = new int[] { 0, 0, -1 };
                    break;
                case 2:
                    AxisSign = new int[] { 1, 0, 0 };
                    break;
                case 3:
                    AxisSign = new int[] { 0, 0, -1 };
                    break;
                default:
                    break;
            }

            Blockentity.RegisterGameTickListener(UpdateTF, 75);
            Blockentity.RegisterGameTickListener(UpdateMP, 1000);
        }

        public void UpdateTF(float dt)
        {
            switch (generator.DeviceState)
            {
                case BEEBaseDevice.enDeviceState.WARMUP:
                case BEEBaseDevice.enDeviceState.IDLE:
                case BEEBaseDevice.enDeviceState.MATERIALHOLD:
                case BEEBaseDevice.enDeviceState.ERROR:
                    break;
                case BEEBaseDevice.enDeviceState.RUNNING:
                    break;
                default:
                    break;
            }
            
            if (network?.TotalAvailableTorque >= RequiredTorque)
            {
                generator.ChangeCapacitor(35);
            }
        }

        public void UpdateMP(float dt)
        {
            network?.updateNetwork(manager.getTickNumber());
        }

        public override float GetResistance() => OwnResistance;
    }
}
