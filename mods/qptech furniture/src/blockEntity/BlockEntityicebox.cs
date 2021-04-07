using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace qptech.src
{


    public class Usless : BlockEntityIceBox
        {
             public float chilltick { get; set; } = 0;
        }

    public class BlockEntityIceBox : BlockEntityGenericTypedContainer
    {
        bool isChilled = false;
        public bool useIce = true;
        public float preserveBonus = 0.5f;
        public double useIceCounter = 10000;
        double chilltick = 0;
        double lastdays;

        public override float GetPerishRate()
        {
            float prate = base.GetPerishRate();
            if (isChilled) { prate = prate * preserveBonus; }
            return prate;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            lastdays = Api.World.Calendar.TotalDays;
            //bonus to apply if block is chilled
            preserveBonus = Block.Attributes["preserveBonus"].AsFloat(preserveBonus);
            //whether to check for and use up ice if chilled
            useIce = Block.Attributes["useIce"].AsBool(useIce);
            //use up an ice block every this many days
            useIceCounter = Block.Attributes["useIceCounter"].AsDouble(useIceCounter);
            //hacky check for old ice boxes
            if (useIceCounter >= 14) { useIceCounter = 2; }
        }

        protected override void OnTick(float dt)
        {
            base.OnTick(dt);

            if (!useIce) { isChilled = true; return; }
            isChilled = false;
            ItemSlot chillslot = null;
            //how many days have passed?
            double deltaDays = Api.World.Calendar.TotalDays - lastdays;
            if (deltaDays < 0) { deltaDays = 0; }
            lastdays = Api.World.Calendar.TotalDays;
            //increase ice use countdown clock by how much time has passed
            chilltick += deltaDays;
            foreach (ItemSlot slot in Inventory)
            {
                if (slot.Itemstack == null) { continue; }
                if (slot.Itemstack.Block == null) { continue; }
                if (slot.Itemstack.Block.BlockMaterial.ToString() == "Ice")
                {
                    isChilled = true;
                    chillslot = slot;
                    chilltick += deltaDays;
                    continue;
                }

            }
            if (!isChilled) { chilltick = 0; return; }
            if (chilltick >= useIceCounter)
            {
                int qtytotake = (int)(chilltick / useIceCounter);
                chillslot.TakeOut(qtytotake);//note this may still result in "Free" freezer time
                chilltick = 0;
            }

        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            //dsc.AppendLine("" + lastdays.ToString());
            //dsc.AppendLine("Time:" + chilltick.ToString() + "");
            dsc.AppendLine("Chilled:" + isChilled.ToString());
        }
    }
}