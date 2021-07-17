﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Client;

namespace qptech.src
{
    class BEEKiln:BEEBaseDevice
    {
        protected float animationSpeed = 0.5f;
        
        protected double internalheat;
        protected double stackheat;
        protected double restingheat = 20;
        protected double heatPerTick = 10;
        protected double insulationFactor = 0.99;
        protected double maxHeat = 2000;
        /// <summary>
        /// Internal heat starts at resting heat
        /// While processing will increse internal heat,
        /// somehow we will average stack heat (which starts at resting - or possibly
        /// pull from object?) up towards internal heat somehow
        /// Once heat is reached item is done - if not processing then heat will
        /// slowly fall to resting heat
        /// Insulation factor determines how quickly internal heat falls
        /// </summary>
        protected int processQty = 1; //how many items to process at once
        protected BlockFacing rmInputFace; //what faces will be checked for input containers
        protected BlockFacing outputFace ;
        string outputcode="";
        string inputcode="";
        string blockoritem="";
        double requiredheat;
        DummyInventory dummy;
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            if (Block.Attributes != null)
            {
                //requiredFlux = Block.Attributes["requiredFlux"].AsInt(requiredFlux);
                rmInputFace = BlockFacing.FromCode(Block.Attributes["inputFace"].AsString("up"));

                outputFace = BlockFacing.FromCode(Block.Attributes["outputFace"].AsString("down"));
                animationSpeed = Block.Attributes["animationSpeed"].AsFloat(animationSpeed);
                rmInputFace = OrientFace(Block.Code.ToString(), rmInputFace);
                outputFace = OrientFace(Block.Code.ToString(), outputFace);
                heatPerTick = Block.Attributes["heatPerTick"].AsDouble(heatPerTick);
                insulationFactor = Block.Attributes["insulationFactor"].AsDouble(insulationFactor);
                maxHeat = Block.Attributes["maxHeat"].AsDouble(maxHeat);
            }
            dummy = new DummyInventory(api);
        }
        protected override void DoDeviceStart()
        {
            //if (Api.World.Side is EnumAppSide.Client) { return; }
            if (Capacitor < requiredFlux) { DoFailedStart(); }
            tickCounter = 0;
            if (deviceState == enDeviceState.IDLE)
            {
                
                TryStart();
                if (deviceState == enDeviceState.IDLE) { DoCooling(); }
            }
            this.MarkDirty(true);
        }
        void DoCooling()
        {
            //if (Api.World.Side is EnumAppSide.Client) { return; }
            if (internalheat <= restingheat) { internalheat = restingheat;return; }
            internalheat *= insulationFactor;
            stackheat = (stackheat + stackheat + internalheat) / 3; //BS average code lol
            this.MarkDirty(true);
        }


        protected override void DoDeviceProcessing()
        {
            //if (Api.World.Side is EnumAppSide.Client) { return; }
            if (Capacitor < requiredFlux) { DoCooling(); return; }
            ChangeCapacitor(-requiredFlux);
            if (internalheat < maxHeat)
            {
                internalheat += heatPerTick;
                if (internalheat > maxHeat) { internalheat = maxHeat; }
            }
            stackheat = (stackheat + stackheat + internalheat) / 3; //BS average code lol
            if (stackheat >= requiredheat)
            {
                DoDeviceComplete();
            }
            this.MarkDirty(true);
        }

        protected override void DoDeviceComplete()
        {
            deviceState = enDeviceState.IDLE;
            ItemStack outputStack;
            
            Block outputBlock = Api.World.GetBlock(new AssetLocation(outputcode));
            Item outputItem = Api.World.GetItem(new AssetLocation(outputcode));
            if (outputBlock == null && outputItem == null) { deviceState = enDeviceState.ERROR; return; }
            if (outputBlock != null)
            {
                outputStack = new ItemStack(outputBlock, 1);
            }
            else
            {
                outputStack = new ItemStack(outputItem, 1);
            }
            dummy[0].Itemstack = outputStack;
            dummy.DropAll(Pos.ToVec3d());
            this.MarkDirty(true);
        }

        void TryStart()
        {
            
            

           //if (Api.World.Side is EnumAppSide.Client) { return; }
            BlockPos bp = Pos.Copy().Offset(rmInputFace);
            BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
            var inputContainer = checkblock as BlockEntityContainer;
            if (inputContainer == null) { return; }
            if (inputContainer.Inventory.Empty) { return; }
            for (int c = 0; c < inputContainer.Inventory.Count; c++)
            {
                ItemSlot checkslot = inputContainer.Inventory[c];
                if (checkslot == null) { continue; }
                if (checkslot.StackSize == 0) { continue; }
                Item checkitem = checkslot.Itemstack.Item;
                Block checkiblock = checkslot.Itemstack.Block;
                if (checkiblock != null)
                {
                    if (checkiblock.CombustibleProps!=null&& checkiblock.CombustibleProps.SmeltingType == EnumSmeltType.Fire)
                    {
                        Block outputblock = Api.World.GetBlock(checkiblock.CombustibleProps.SmeltedStack.Code);
                        Block outputitem = Api.World.GetBlock(checkiblock.CombustibleProps.SmeltedStack.Code);
                        
                        if (outputblock == null && outputitem==null) { continue; }
                        requiredheat = checkiblock.CombustibleProps.MeltingPoint;
                        stackheat = restingheat;
                        inputcode = checkiblock.Code.ToString();
                        if (outputblock != null)
                        {
                            inputcode = checkiblock.Code.ToString();
                            outputcode = outputblock.Code.ToString();
                            
                            blockoritem = "BLOCK";
                        }
                        else
                        {
                            inputcode = checkitem.Code.ToString();
                            outputcode = outputitem.Code.ToString();
                            blockoritem = "ITEM";
                        }
                        deviceState = enDeviceState.RUNNING;
                        checkslot.TakeOut(1);
                        checkblock.MarkDirty(true);
                        this.MarkDirty(true);
                        return;
                    }
                }
                else if (checkitem != null)
                {
                    if (checkitem.CombustibleProps != null && checkitem.CombustibleProps.SmeltingType == EnumSmeltType.Fire)
                    {
                        Block outputblock = Api.World.GetBlock(checkitem.CombustibleProps.SmeltedStack.Code);
                        Block outputitem = Api.World.GetBlock(checkitem.CombustibleProps.SmeltedStack.Code);
                        
                        if (outputblock == null && outputitem == null) { continue; }
                        requiredheat = checkitem.CombustibleProps.MeltingPoint;
                        stackheat = restingheat;
                        if (outputblock != null)
                        {
                            inputcode = checkitem.Code.ToString();
                            outputcode = outputblock.Code.ToString();
                            blockoritem = "BLOCK";
                        }
                        else
                        {
                            inputcode = checkitem.Code.ToString();
                            outputcode = outputitem.Code.ToString();
                            blockoritem = "ITEM";
                        }
                        deviceState = enDeviceState.RUNNING;
                        checkslot.TakeOut(1);
                        checkblock.MarkDirty(true);
                        this.MarkDirty(true);
                        return;
                    }

                }
            }

            }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);

            dsc.AppendLine("INTERNAL TEMP :" + internalheat.ToString());
            dsc.AppendLine("Make :" + outputcode);
            dsc.AppendLine("ITEMTEMP/REQ TEMP: " + stackheat.ToString()+"/"+ requiredheat.ToString());
            
        }
    }
}
