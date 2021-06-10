using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
namespace qptechweapons.src.item
{
    public class FilletKnife : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            api.RegisterItemClass("ItemFilletKnife", typeof(ItemFilletKnife));
        }
    }
    class ItemFilletKnife:Item
    {
        public override void OnHeldInteractStop(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            
            if (entitySel == null) return;


            EntityBehaviorHarvestable bh = entitySel.Entity.GetBehavior<EntityBehaviorHarvestable>();
            //byEntity.World.Logger.Debug("{0} knife interact stop, seconds used {1} / {2}, entity: {3}", byEntity.World.Side, secondsUsed, bh?.HarvestDuration, entitySel.Entity);

            if (bh != null && bh.Harvestable )
            {
                bh.SetHarvested((byEntity as EntityPlayer)?.Player);
                slot?.Itemstack?.Collectible.DamageItem(byEntity.World, byEntity, slot, 3);
            }
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {


            EntityBehaviorHarvestable bh;
            if (byEntity.Controls.Sneak && entitySel != null && (bh = entitySel.Entity.GetBehavior<EntityBehaviorHarvestable>()) != null && bh.Harvestable)
            {
                byEntity.World.PlaySoundAt(new AssetLocation("sounds/player/scrape"), entitySel.Entity, (byEntity as EntityPlayer)?.Player, false, 12);
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            handling = EnumHandHandling.NotHandled;
        }
    }
}
