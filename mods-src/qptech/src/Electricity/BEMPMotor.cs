using Vintagestory.API.Client;

namespace qptech.src
{
    class BEMPMotor : BEEBaseDevice
    {



        protected override void DoDeviceProcessing()
        {
            if (capacitor >= requiredFlux)
            {
                ChangeCapacitor(-requiredFlux);
            }
            else
            {
                deviceState = enDeviceState.IDLE;
            }
        }

        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            base.OnTesselation(mesher, tessThreadTesselator);
            mesher.AddMeshData((Block as BlockElectricMotor)?.statorMesh);
            return true;
        }
    }
}
