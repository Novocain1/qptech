﻿using System;
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
    //electric forge for heating up ingots using electricity
    class BEEForge:BEEBaseDevice, IHeatSource
    {
        ItemStack contents;
        EForgeContentsRenderer renderer;
        double lastTickTotalHours;
        float maxHeat=1100;                                 //max temperature of device default 1100
        float degreesPerHour=1500;                          //temperature increase per hour default 1500
        int maxItems = 4;                                   //how many items can go in
        float stackRenderHeight =0.07f;                     //this is basically the height for the itemstack
        string elementShapeName = "machines:dummy-element-lit";      //what item to load heating element's shape & texture from
        public override bool IsOn => base.IsOn&&contents!=null;
        public ItemStack Contents => contents;
        
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            
            if (contents != null) contents.ResolveBlockOrItem(api.World);
            if (Block.Attributes != null)
            {
                maxHeat = Block.Attributes["maxHeat"].AsFloat(maxHeat);
                degreesPerHour = Block.Attributes["degreesPerHour"].AsFloat(degreesPerHour);
                maxItems = Block.Attributes["maxItems"].AsInt(maxItems);
                stackRenderHeight = Block.Attributes["stackRenderHeight"].AsFloat(stackRenderHeight);
                elementShapeName = Block.Attributes["elementShapeName"].AsString(elementShapeName);
            }
            if (api is ICoreClientAPI)
            {
                ICoreClientAPI capi = (ICoreClientAPI)api;
                capi.Event.RegisterRenderer(renderer = new EForgeContentsRenderer(Pos, capi, elementShapeName), EnumRenderStage.Opaque, "forge");
                renderer.SetContents(contents, stackRenderHeight, (deviceState==enDeviceState.RUNNING), true);

                RegisterGameTickListener(OnClientTick, 50);
            }
            RegisterGameTickListener(OnCommonTick, 200);
        }

        private void OnCommonTick(float dt)
        {
            
            //if (deviceState!=enDeviceState.RUNNING) { DoDeviceStart(); }
            if (contents != null && deviceState==enDeviceState.RUNNING)
            {
                double hoursPassed = Api.World.Calendar.TotalHours - lastTickTotalHours;
                float temp = contents.Collectible.GetTemperature(Api.World, contents);
                if (temp < maxHeat)
                {
                    float tempGain = (float)(hoursPassed * degreesPerHour);

                    contents.Collectible.SetTemperature(Api.World, contents, Math.Min(maxHeat, temp + tempGain));
                }
            }
            lastTickTotalHours = Api.World.Calendar.TotalHours;
        }
        
        protected override void DoDeviceComplete()
        {
            deviceState = enDeviceState.IDLE;
            MarkDirty(true);
        }
        protected override void DoDeviceStart()
        {
            
            if (capacitor >= requiredAmps&&IsOn&&contents.StackSize>0)
            {

                tickCounter = 0;
                deviceState = enDeviceState.RUNNING;
                MarkDirty(true);
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
            if (contents == null) { DoDeviceComplete(); return; }
            if (capacitor < requiredAmps||contents.StackSize==0)
            {
                DoDeviceComplete();
                return;
            }
            //tickCounter++;
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
            if (renderer != null)
            {
                renderer.SetContents(contents, stackRenderHeight, burning, true);
            }
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

                renderer?.SetContents(contents, stackRenderHeight, burning, true);
                MarkDirty(true);
                Api.World.PlaySoundAt(new AssetLocation("sounds/block/ingot"), Pos.X, Pos.Y, Pos.Z, byPlayer, false);

                return true;

            }
            else
            {
                if (slot.Itemstack == null) return false;

                // Add fuel
                CombustibleProperties combprops = slot.Itemstack.Collectible.CombustibleProps;
                if (combprops != null && combprops.BurnTemperature > 1000)
                {
                    

                    
                    (Api as ICoreClientAPI)?.World.Player.TriggerFpAnimation(EnumHandInteract.HeldItemInteract);

                    renderer?.SetContents(contents, stackRenderHeight, burning, false);
                    MarkDirty(true);

                    if (byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative)
                    {
                        slot.TakeOut(1);
                        slot.MarkDirty();
                    }


                    return true;
                }


                string firstCodePart = slot.Itemstack.Collectible.FirstCodePart();
                bool forgableGeneric = slot.Itemstack.Collectible.Attributes?.IsTrue("forgable") == true;

                // Add heatable item
                if (contents == null && (firstCodePart == "ingot" || firstCodePart == "metalplate" || firstCodePart == "workitem" || forgableGeneric))
                {
                    contents = slot.Itemstack.Clone();
                    contents.StackSize = 1;

                    slot.TakeOut(1);
                    slot.MarkDirty();

                    renderer?.SetContents(contents, stackRenderHeight, burning, true);
                    MarkDirty();
                    Api.World.PlaySoundAt(new AssetLocation("sounds/block/ingot"), Pos.X, Pos.Y, Pos.Z, byPlayer, false);

                    return true;
                }

                // Merge heatable item
                if (!forgableGeneric && contents != null && contents.Equals(Api.World, slot.Itemstack, GlobalConstants.IgnoredStackAttributes) && contents.StackSize < maxItems && contents.StackSize < contents.Collectible.MaxStackSize)
                {
                    float myTemp = contents.Collectible.GetTemperature(Api.World, contents);
                    float histemp = slot.Itemstack.Collectible.GetTemperature(Api.World, slot.Itemstack);

                    contents.Collectible.SetTemperature(world, contents, (myTemp * contents.StackSize + histemp * 1) / (contents.StackSize + 1));
                    contents.StackSize++;

                    slot.TakeOut(1);
                    slot.MarkDirty();

                    renderer?.SetContents(contents, stackRenderHeight, burning, true);
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
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            if (contents == null) { dsc.AppendLine("EMPTY"); return; }
            if (contents.StackSize == 0) { dsc.AppendLine("EMPTY");return; }
            string d = contents.StackSize.ToString()+" of "+ contents.Item.Code.ToString();
            d += " at " + contents.Collectible.GetTemperature(Api.World,contents).ToString() + "C";
            dsc.AppendLine(d);
        }
        
        bool burning => (deviceState == enDeviceState.RUNNING);
        private void OnClientTick(float dt)
        {
            //if (Api?.Side == EnumAppSide.Client && clientSidePrevBurning != burning)
            //{
            //    ToggleAmbientSounds(IsBurning);
            //    clientSidePrevBurning = IsBurning;
            //}

            //if (burning && Api.World.Rand.NextDouble() < 0.13)
            //{
            //    smokeParticles.MinPos.Set(Pos.X + 4 / 16f, Pos.Y + 14 / 16f, Pos.Z + 4 / 16f);
             //   int g = 50 + Api.World.Rand.Next(50);
             //   smokeParticles.Color = ColorUtil.ToRgba(150, g, g, g);
            //    Api.World.SpawnParticles(smokeParticles);
            //}
            if (renderer != null)
            {
                renderer.SetContents(contents, stackRenderHeight, burning, false);
            }
        }
        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            renderer?.Dispose();
        }
        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();
            if (renderer != null)
            {
                renderer.Dispose();
                renderer = null;
            }

            
        }

        //Keeping this OnTesselation here as it's a good reference for the future, but it's all taken care of in the renderer
        /*
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            
            ICoreClientAPI clientApi = (ICoreClientAPI)Api;
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            MeshData mesh = clientApi.TesselatorManager.GetDefaultBlockMesh(block);
            if (mesh == null) return true;

            mesher.AddMeshData(mesh);
            
           
            Block elementBlock = Api.World.GetBlock(new AssetLocation("machines:dummy-element-lit"));
           
            if (deviceState != enDeviceState.RUNNING)
            {
                elementBlock = Api.World.GetBlock(new AssetLocation("machines:dummy-element-unlit"));
                
            }
            if (elementBlock == null) { return true; }
            clientApi.Tesselator.TesselateBlock(elementBlock, out mesh);
            mesher.AddMeshData(mesh);
            return true;

        }
        */
        protected override void UsePower()
        {
            if (!isOn) { deviceState=enDeviceState.IDLE;return; }
            if (DeviceState == enDeviceState.IDLE || DeviceState == enDeviceState.MATERIALHOLD)
            {
                DoDeviceStart();
            }
            else if (deviceState == enDeviceState.WARMUP)
            {
                tickCounter++;
                if (tickCounter == 10) { tickCounter = 0; deviceState = enDeviceState.IDLE; }
            }
            else { DoDeviceProcessing(); }
        }

    }
}