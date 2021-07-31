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

namespace qptech.src
{
    class HVACEnvironmentUpdater:ModSystem
    {
        
        public override void Start(ICoreAPI api)
        {
           /* base.Start(api);
            
            api.Event.OnGetClimate += (ref ClimateCondition climate, BlockPos pos, EnumGetClimateMode mode, double totalDays) =>
            {
                float hvacbonus = RoomStats.GetStorageBonus(api, pos);
                if (hvacbonus != 0)
                {
                    climate.Temperature += hvacbonus;
                }
            };*/
        }

    }
}
