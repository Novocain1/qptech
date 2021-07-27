using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace qptech.src
{
    class BEBMPGenerator : BEBehaviorMPRotor
    {
        BEMPGenerator generator { get => Blockentity as BEMPGenerator; }

        protected override float Resistance => ResistanceLink;
        protected override double AccelerationFactor => AccelerationFactorLink;
        protected override float TargetSpeed => TargetSpeedLink;
        protected override float TorqueFactor => TorqueFactorLink;

        public float ResistanceLink { get; set; }
        public double AccelerationFactorLink { get; set; }
        public float TargetSpeedLink { get; set; }
        public float TorqueFactorLink { get; set; }

        public float RequiredTorque { get; set; }

        public BEBMPGenerator(BlockEntity blockentity) : base(blockentity)
        {
            Blockentity = blockentity;

            string orientation = blockentity.Block.Variant["side"];
            ownFacing = BlockFacing.FromCode(orientation);
            OutFacingForNetworkDiscovery = ownFacing.Opposite;
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);

            ResistanceLink = 0.5f;
            AccelerationFactorLink = 0.9;
            TargetSpeedLink = 0.0f;
            TorqueFactorLink = 0.0f;

            if (Block?.Attributes != null)
            {
                RequiredTorque = Block.Attributes["requiredTorque"].AsFloat(RequiredTorque);
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
        }

        public void UpdateMP(float dt)
        {
            network?.updateNetwork(manager.getTickNumber());
        }
    }
}
