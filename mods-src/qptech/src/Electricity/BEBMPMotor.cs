using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent.Mechanics;

namespace qptech.src
{
    class BEBMPMotor : BEBehaviorMPRotor
    {
        BEMPMotor motor { get => Blockentity as BEMPMotor; }

        protected override float Resistance => ResistanceLink;
        protected override double AccelerationFactor => AccelerationFactorLink;
        protected override float TargetSpeed => TargetSpeedLink;
        protected override float TorqueFactor => TorqueFactorLink;

        public float ResistanceLink { get; set; }
        public double AccelerationFactorLink { get; set; }
        public float TargetSpeedLink { get; set; }
        public float TorqueFactorLink { get; set; }

        public BEBMPMotor(BlockEntity blockentity) : base(blockentity)
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

            Blockentity.RegisterGameTickListener(Update, 1000);
        }

        public void Update(float dt)
        {
            TorqueFactorLink = TargetSpeedLink = motor.IsOn && motor.IsPowered ? 1.0f : 0.0f;
            motor.UsePowerP();
            network?.updateNetwork(manager.getTickNumber());
        }
    }
}