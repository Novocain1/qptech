using Vintagestory.API.Client;

namespace qptech.src
{
    class BEMPMotor : BEEBaseDevice
    {
        public void UsePowerP() => UsePower();

        protected override void DoFailedProcessing()
        {
            base.DoFailedProcessing();
            ChangeCapacitor(requiredFlux);
        }

        protected override void UsePower()
        {
            base.UsePower();
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            base.OnTesselation(mesher, tessThreadTesselator);
            mesher.AddMeshData((Block as BlockElectricMotor)?.statorMesh);
            return true;
        }
    }
}
