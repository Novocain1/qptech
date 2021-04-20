using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace QptechFurniture.src
{


    class BlockEntityIceBox : BlockEntityGenericTypedContainer
    {
        bool isChilled;

        public float preserveBonus;

        public BlockEntityIceBox()
        {
            isChilled = true;
            preserveBonus = 0;
        }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            //bonus to apply if block is chilled
            preserveBonus = Block.Attributes["preserveBonus"][type].AsFloat(preserveBonus);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            preserveBonus = tree.GetFloat("preserveBonus");
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetFloat("preserveBonus", preserveBonus);

        }

        public override float GetPerishRate()
        {
            float prate = base.GetPerishRate();
            if (isChilled) { prate = prate * preserveBonus; }
            return prate;
        }
    }
}