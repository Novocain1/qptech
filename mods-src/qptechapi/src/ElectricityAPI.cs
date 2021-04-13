using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace Electricity.API
{

<<<<<<< Updated upstream:mods-src/qptech/src/Electricity/ElectricityAPI.cs

    /// <summary>
    /// https://discord.gg/VbYQc7yfnF
    /// 
    /// Class to Distribute electricity between block entities
    /// note this is barebones - what connects to what, and whether it needs power, and to use up power
    /// you would have to create your own tick (default 75) to go through and distribute available power if applicable
    /// (or it can passively accept power)
    /// 
    /// Suggested guidelines
    /// LV -  16 Volts
    /// MV -  64 Volts
    /// HV - 256 Volts
    /// 
    /// OnTick length to handle your electricity updates - 75
    /// </summary>
=======
    //Class to Distribute electricity between block entities
    // note this is barebones - what connects to what, and whether it needs power, and to use up power
    // you would have to create your own tick (default 75) to go through and distribute available power if applicable
    // (or it can passively accept power)
>>>>>>> Stashed changes:mods-src/qptechapi/src/ElectricityAPI.cs
    public interface IElectricity
    {
        //the block entity this IElectricity belongs to
        BlockEntity EBlock { get; }
        //MaxAmps - How much power can be transferred in one turn

        int MaxAmps { get;  }
        //MaxVolts - the voltage class of this device (should match whatever its hooked up to)
        int MaxVolts { get; }
        //if device is on
        bool IsOn { get;  }
        //Receive an offer for a power packet, return how much power it uses
        int ReceivePacketOffer(IElectricity from, int inVolt, int inAmp);
        //return if this object needs any power
        int NeedPower();
        //Try to accept an IElectricity as a connected power source (return false to refuse connection)
        bool TryInputConnection(IElectricity connectto);
        //Try to accept an IElectricity as a connected power destination
        bool TryOutputConnection(IElectricity connectto);
        //Delete any connection the the supplied IElectricity (should be called on every connection when the block is deleted, removed etc)
        void RemoveConnection(IElectricity disconnect);

    }

}
