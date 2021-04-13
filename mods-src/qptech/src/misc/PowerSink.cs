using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using Vintagestory.API.Client;
using ElectricityAPI;


namespace qptech.src.misc
{
    //test object for IElectricity - will simply use up all power offered to it
    //this is not a sink for water, it is a sink for power, do not use with water
    //Note this block currently will not attempt to find power sources, but will link up with
    //any power source offered to it.
    class PowerSink : BlockEntity, IElectricity
    {
        //for fun we'll track how much power was stolen
        double stolenpowercounter=0;
        //some objects may actually use this to check how close they are etc - we don't care we just want all the power...
        public BlockEntity EBlock { get { return this as BlockEntity; } }
        //report a ridiculous amount of amps, but this isn't tracked by this device anyways
        public int MaxAmps => 2048;
        //this doesn't actually matter right now - most devices would explode or something bad during a voltage mismatch
        public int MaxVolts => 16; 
        

        //Always on baby!
        public bool IsOn => true;

        //always report we need a lot of power because we're greedy like that
        public int NeedPower()
        {
            return MaxAmps;
        }

        //Take any amps offered and report them taken
        public int ReceivePacketOffer(IElectricity from, int inVolt, int inAmp)
        {
            stolenpowercounter += inAmp; //keeping score
            return inAmp;
        }

        //Do nothing, we don't track our power connections, we don't even care!
        public void RemoveConnection(IElectricity disconnect)
        {
            
        }

        //Just accept any incoming power
        public bool TryInputConnection(IElectricity connectto)
        {
            return true;
        }

        //Reject any connections requesting power
        public bool TryOutputConnection(IElectricity connectto)
        {
            return true;
        }

        //This will display how much power is stolen in the block info of the UI
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            
            dsc.AppendLine("I've stolen "+stolenpowercounter.ToString()+" power packets LOL");
        }
    }
    
    //usually would load in a loading class for your mod, but we'll do it here
    class LoadPowerSink : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockEntityClass("PowerSink", typeof(PowerSink));
        }
    }
}
