using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using qptech.src.misc;



namespace qptech.src
{
    class ElectricityLoader:ModSystem
    {
        
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
           
            api.RegisterBlockEntityClass("BEEWire", typeof(BEEWire));
            api.RegisterBlockEntityClass("BEEAssembler", typeof(BEEAssembler));
            api.RegisterBlockEntityClass("BEEGenerator", typeof(BEEGenerator));
            api.RegisterBlockClass("ElectricalBlock", typeof(ElectricalBlock));
            api.RegisterBlockClass("BlockWire", typeof(BlockWire));
            api.RegisterBlockClass("BlockCannedMeal", typeof(BlockCannedMeal));
            api.RegisterItemClass("ItemQuarryTool", typeof(ItemQuarryTool));
            api.RegisterItemClass("ItemJetPack", typeof(ItemJetPack));
            api.RegisterBlockEntityClass("BEEForge", typeof(BEEForge));
            api.RegisterBlockClass("BlockEForge", typeof(BlockEForge));
            api.RegisterBlockEntityClass("BEPowerFlag", typeof(BEPowerFlag));
            //api.RegisterBlockEntityBehaviorClass("BEBMPMotor", typeof(BEBMPMotor));
            //api.RegisterBlockClass("BlockPoweredRotor", typeof(BlockPoweredRotor));
            //api.RegisterBlockClass("BlockElectricMotor", typeof(BlockElectricMotor));
            //api.RegisterBlockEntityClass("BEEMotor", typeof(BEEMotor));

        }
    }
}
