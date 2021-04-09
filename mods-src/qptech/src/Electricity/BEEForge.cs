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

namespace qptech.src
{
    //electric forge for heating up ingots
    class BEEForge:BEEBaseDevice, IHeatSource
    {
        ItemStack contents;
        ForgeContentsRenderer renderer;
        double lastTickTotalHours;
        float runningTemp=1100; //max temperature of device
        float tempIncrease=1500;//temperature increase per hour
        int stackSize = 4; //how many items can go in

        public ItemStack Contents => contents;
        /*
         * allow ingots in/out (needs inventory)
         * heat ingots if electricity
         * max heat, heat time(?)
         * only draw electricity if loaded
         * 
         * TODO
         * - add glow, particles
         * - render ingots
         * - need BlockForge class as well
         * - need model
         * 
         */
        public override void Initialize(ICoreAPI api)
        {
            if (contents != null) contents.ResolveBlockOrItem(api.World);
            base.Initialize(api);
            RegisterGameTickListener(OnCommonTick, 200);
        }

        private void OnCommonTick(float dt)
        {
            lastTickTotalHours = Api.World.Calendar.TotalHours;
            if (contents != null && deviceState==enDeviceState.RUNNING)
            {
                double hoursPassed = Api.World.Calendar.TotalHours - lastTickTotalHours;
                float temp = contents.Collectible.GetTemperature(Api.World, contents);
                if (temp < runningTemp)
                {
                    float tempGain = (float)(hoursPassed * tempIncrease);

                    contents.Collectible.SetTemperature(Api.World, contents, Math.Min(runningTemp, temp + tempGain));
                }
            }
        }
        
        protected override void DoDeviceComplete()
        {
            deviceState = enDeviceState.IDLE;
        }
        protected override void DoDeviceStart()
        {
            if (capacitor >= requiredAmps&&isOn&&contents.StackSize>0)
            {

                tickCounter = 0;
                deviceState = enDeviceState.RUNNING;

                //sounds/blocks/doorslide.ogg
                DoDeviceProcessing();
            }
            else { DoFailedStart(); }
        }
        protected override void DoDeviceProcessing()
        {
            /*if (tickCounter >= processingTicks)
            {
                DoDeviceComplete();
                return;
            }*/
            if (capacitor < requiredAmps||contents.StackSize==0)
            {
                DoDeviceComplete();
                return;
            }
            tickCounter++;
            capacitor -= requiredAmps;
        }

        public float GetHeatStrength(IWorldAccessor world, BlockPos heatSourcePos, BlockPos heatReceiverPos)
        {
            return deviceState==enDeviceState.RUNNING ? 7 : 0;
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            contents = tree.GetItemstack("contents");
            
            lastTickTotalHours = tree.GetDouble("lastTickTotalHours");

            if (Api != null)
            {
                contents?.ResolveBlockOrItem(Api.World);
            }
            /*if (renderer != null)
            {
                renderer.SetContents(contents, fuelLevel, burning, true);
            }*/
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetItemstack("contents", contents);
            tree.SetDouble("lastTickTotalHours", lastTickTotalHours);
        }
        internal bool OnPlayerInteract(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (!byPlayer.Entity.Controls.Sneak)
            {
                if (contents == null) return false;
                ItemStack split = contents.Clone();
                split.StackSize = 1;
                contents.StackSize--;

                if (contents.StackSize == 0) contents = null;

                if (!byPlayer.InventoryManager.TryGiveItemstack(split))
                {
                    world.SpawnItemEntity(split, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                //renderer?.SetContents(contents, fuelLevel, burning, true);
                MarkDirty();
                Api.World.PlaySoundAt(new AssetLocation("sounds/block/ingot"), Pos.X, Pos.Y, Pos.Z, byPlayer, false);

                return true;

            }
            else
            {
                if (slot.Itemstack == null) return false;

                // Add fuel
                /*CombustibleProperties combprops = slot.Itemstack.Collectible.CombustibleProps;
                if (combprops != null && combprops.BurnTemperature > 1000)
                {
                    if (fuelLevel >= 5 / 16f) return false;
                    fuelLevel += 1 / 16f;

                    if (slot.Itemstack.Collectible is ItemCoal || slot.Itemstack.Collectible is ItemOre)
                    {
                        Api.World.PlaySoundAt(new AssetLocation("sounds/block/charcoal"), byPlayer, byPlayer, true, 16);
                    }
                    (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                    renderer?.SetContents(contents, fuelLevel, burning, false);
                    MarkDirty();

                    if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
                    {
                        slot.TakeOut(1);
                        slot.MarkDirty();
                    }


                    return true;
                }*/


                string firstCodePart = slot.Itemstack.Collectible.FirstCodePart();
                bool forgableGeneric = slot.Itemstack.Collectible.Attributes?.IsTrue("forgable") == true;

                // Add heatable item
                if (contents == null && (firstCodePart == "ingot" || firstCodePart == "metalplate" || firstCodePart == "workitem" || forgableGeneric))
                {
                    contents = slot.Itemstack.Clone();
                    contents.StackSize = 1;

                    slot.TakeOut(1);
                    slot.MarkDirty();

                    //renderer?.SetContents(contents, fuelLevel, burning, true);
                    MarkDirty();
                    Api.World.PlaySoundAt(new AssetLocation("sounds/block/ingot"), Pos.X, Pos.Y, Pos.Z, byPlayer, false);

                    return true;
                }

                // Merge heatable item
                if (!forgableGeneric && contents != null && contents.Equals(Api.World, slot.Itemstack, GlobalConstants.IgnoredStackAttributes) && contents.StackSize < stackSize && contents.StackSize < contents.Collectible.MaxStackSize)
                {
                    float myTemp = contents.Collectible.GetTemperature(Api.World, contents);
                    float histemp = slot.Itemstack.Collectible.GetTemperature(Api.World, slot.Itemstack);

                    contents.Collectible.SetTemperature(world, contents, (myTemp * contents.StackSize + histemp * 1) / (contents.StackSize + 1));
                    contents.StackSize++;

                    slot.TakeOut(1);
                    slot.MarkDirty();

                    //renderer?.SetContents(contents, fuelLevel, burning, true);
                    Api.World.PlaySoundAt(new AssetLocation("sounds/block/ingot"), Pos.X, Pos.Y, Pos.Z, byPlayer, false);

                    MarkDirty();
                    return true;
                }

                return false;
            }
        }
        public override void OnBlockBroken()
        {
            base.OnBlockBroken();

            if (contents != null)
            {
                Api.World.SpawnItemEntity(contents, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
            }

            //ambientSound?.Dispose();
        }
    }
}
