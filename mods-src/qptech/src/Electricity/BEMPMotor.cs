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
    }
}
