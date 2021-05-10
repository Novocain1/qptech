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
using Vintagestory.API.Client;



namespace qptech.src
{
    /// <summary>
    /// The BEEAssembler device uses energy and will create an item and place in
    /// an output chest if the relevant item is found on the input chest. Also
    /// will optionally check for the temperature of the input material (for using smelted metals etc)
    /// 
    /// If give a list of materials it will attempt to pattern match the input and output:
    /// eg: ingredient of "ingot" with an output of "game:metalplate" and a material list of
    /// "copper","tin" would look for copper & tin ingots and make the proper type of plate
    /// </summary>
    class BEEAssembler:BEEBaseDevice
    {
        protected string recipe = "game:bowl-raw";
        protected string blockoritem = "block";
        protected int outputQuantity = 1;
        protected string ingredient = "clay";
        protected string ingredient_subtype ="";
        protected int inputQuantity = 4;
        protected int internalQuantity = 0; //will store ingredients virtually
        protected float animationSpeed = 0.05f;
        protected double processingTime = 10;
        protected float heatRequirement = 0;
        public string Making => outputQuantity.ToString()+"x "+ recipe + ingredient_subtype;
        public string RM
        {
            get
            {
                string outstring=inputQuantity.ToString() + "x " + ingredient + ingredient_subtype;
                if (heatRequirement > 0) { outstring += " at " + heatRequirement.ToString() + "°C"; }
                return outstring;
            }
        }
        string[] materials;
        protected BlockFacing rmInputFace; //what faces will be checked for input containers
        protected BlockFacing outputFace; //what faces will be checked for output containers
        //protected BlockFacing recipeFace; //what face will be used to look for a container with the model object
         DummyInventory dummy;
        double processstarted;
        float lastheatreading = 0;
        /// </summary>
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            
            
            if (Block.Attributes != null) {
                //requiredAmps = Block.Attributes["requiredAmps"].AsInt(requiredAmps);
                rmInputFace = BlockFacing.FromCode(Block.Attributes["inputFace"].AsString("up"));
                
                outputFace = BlockFacing.FromCode(Block.Attributes["outputFace"].AsString("down"));
                animationSpeed = Block.Attributes["animationSpeed"].AsFloat(animationSpeed);
                inputQuantity = Block.Attributes["inputQuantity"].AsInt(inputQuantity);
                outputQuantity = Block.Attributes["outputQuantity"].AsInt(outputQuantity);
                recipe = Block.Attributes["recipe"].AsString(recipe);
                ingredient = Block.Attributes["ingredient"].AsString(ingredient);
                rmInputFace = OrientFace(Block.Code.ToString(), rmInputFace);
                outputFace = OrientFace(Block.Code.ToString(), outputFace);
                processingTime = Block.Attributes["processingTime"].AsDouble(processingTime);
                heatRequirement = Block.Attributes["heatRequirement"].AsFloat(heatRequirement);
                blockoritem = Block.Attributes["blockoritem"].AsString(blockoritem);
                materials=Block.Attributes["materials"].AsArray<string>(materials);
            }

            dummy = new DummyInventory(api);
            
          
        }


        public void OpenStatusGUI()
        {
            ICoreClientAPI capi = Api as ICoreClientAPI;
            if (capi != null)
            {
                if (gas == null)
                {
                    gas = new GUIAssemblerStatus("Assembler Status", Pos, capi);
                    
                    gas.TryOpen();
                    gas.SetupDialog(this);
                    
                }
                else
                {
                    gas.TryClose();
                    gas.TryOpen();
                    gas.SetupDialog(this);
                }
            }
            
        }

        protected override void DoDeviceStart()
        {
            //TODO
            
            if (Capacitor < requiredAmps) { return; }//not enough power
            FetchMaterial();
            processstarted = Api.World.Calendar.TotalHours;      
            if (internalQuantity<inputQuantity) { deviceState = enDeviceState.MATERIALHOLD; return; }//check for and extract the required RM
            //TODO - do we make sure there's an output container?
            if (Capacitor >= requiredAmps)
            {
                internalQuantity = 0;
                tickCounter = 0;
                deviceState = enDeviceState.RUNNING;
                
                if (Api.World.Side == EnumAppSide.Client && animUtil != null)
                {
                    if (!animInit)
                    {
                        float rotY = Block.Shape.rotateY;
                        animUtil.InitializeAnimator("process", new Vec3f(0, rotY, 0));
                        animInit = true;
                    }
                    animUtil.StartAnimation(new AnimationMetaData()
                    {
                        Animation = "process",
                        Code = "process",
                        AnimationSpeed = animationSpeed,
                        EaseInSpeed = 1,
                        EaseOutSpeed = 1,
                        Weight = 1,
                        BlendMode = EnumAnimationBlendMode.Add
                    });
                    
                }

                //sounds/blocks/doorslide.ogg
                DoDeviceProcessing();
            }
            else { DoFailedStart(); }
        }
        protected override void DoDeviceProcessing()
        {
            
            if (Api.World.Calendar.TotalHours>= processingTime + processstarted)
            {
                DoDeviceComplete();
                return;
            }
            if (Capacitor < requiredAmps)
            {
                DoFailedProcessing();
                return;
            }
            tickCounter++;
            ChangeCapacitor(-requiredAmps);

        }
        GUIAssemblerStatus gas;
        protected override void DoDeviceComplete()
        {

           
            
            deviceState = enDeviceState.IDLE;
            string userecipe = recipe;
            if (ingredient_subtype != "")
            {
                userecipe += "-" + ingredient_subtype;
            }
            Block outputBlock = Api.World.GetBlock(new AssetLocation(userecipe));
            Item outputItem = Api.World.GetItem(new AssetLocation(userecipe));
            if (outputBlock == null&&outputItem==null) { deviceState = enDeviceState.ERROR;return; }
            ItemStack outputStack;
            if (outputBlock!=null)
            {
                outputStack = new ItemStack(outputBlock, outputQuantity);
            }
            else
            {
                outputStack = new ItemStack(outputItem, outputQuantity);
            }
            
            dummy[0].Itemstack = outputStack;
            outputStack.Collectible.SetTemperature(Api.World, outputStack, lastheatreading);
            BlockPos bp = Pos.Copy().Offset(outputFace);
            BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
            var outputContainer = checkblock as BlockEntityContainer;
            if (outputContainer != null)
            {
                WeightedSlot tryoutput = outputContainer.Inventory.GetBestSuitedSlot(dummy[0]);

                if (tryoutput.slot != null) {
                    ItemStackMoveOperation op = new ItemStackMoveOperation(Api.World, EnumMouseButton.Left, 0, EnumMergePriority.DirectMerge, outputQuantity);
                    
                    dummy[0].TryPutInto(tryoutput.slot, ref op);
                    
                }
            }
 
            if (!dummy.Empty)
            {
                //If no storage then spill on the ground
                Vec3d pos = Pos.ToVec3d();
                
                dummy.DropAll(pos);
            }
            Api.World.PlaySoundAt(new AssetLocation("sounds/doorslide"), Pos.X, Pos.Y, Pos.Z, null, false, 8, 1);
            if (Api.World.Side == EnumAppSide.Client && animUtil != null)
            {
                
                animUtil.StopAnimation("process");
            }
        }

        protected void FetchMaterial()
        {
            internalQuantity = Math.Min(internalQuantity, inputQuantity); //this shouldn't be necessary
           

            BlockPos bp = Pos.Copy().Offset(rmInputFace);
            BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
            var inputContainer = checkblock as BlockEntityContainer;
            if (inputContainer == null) { return; }
            if (inputContainer.Inventory.Empty) { return; }
            for (int c = 0; c < inputContainer.Inventory.Count; c++)
            {
                ItemSlot checkslot = inputContainer.Inventory[c];
                if (checkslot == null) { continue; }
                if (checkslot.StackSize < inputQuantity) { continue; }
                bool match = false;
                Item checkitem = checkslot.Itemstack.Item;
                Block checkiblock = checkslot.Itemstack.Block;
                ingredient_subtype = "";

                if (checkitem == null && checkiblock == null) { continue; }
                if (checkitem != null) {
                    string fcp = checkitem.FirstCodePart().ToString();
                    string lcp = checkitem.LastCodePart().ToString();
                    //no materials list so we don't need check for subtypes
                    if (checkitem.FirstCodePart() == ingredient && (materials == null || materials.Length == 0)) { match = true; }
                    else if (checkitem.FirstCodePart().ToString() == ingredient && materials.Contains(checkitem.LastCodePart().ToString()))
                    {
                        match = true;
                        ingredient_subtype = checkitem.LastCodePart();
                    }

                }
                else if (checkiblock != null) {
                    if(checkiblock.FirstCodePart().ToString() == ingredient && (materials == null || materials.Length == 0)) { match = true; }
                    else if (checkiblock.FirstCodePart().ToString()==ingredient && materials.Contains(checkiblock.LastCodePart().ToString())){
                        match = true;
                        ingredient_subtype = checkiblock.LastCodePart();
                    }
                }
                if (match)
                {
                    bool heatok = true;
                    lastheatreading= checkslot.Itemstack.Collectible.GetTemperature(Api.World, checkslot.Itemstack);
                    if (heatRequirement > 0 && lastheatreading<heatRequirement)
                    {
                        heatok = false;
                    }
                    if (heatok)
                    {
                        int reqQty = Math.Min(checkslot.StackSize, inputQuantity - internalQuantity);
                        checkslot.TakeOut(reqQty);
                        internalQuantity += reqQty;
                        checkslot.MarkDirty();
                    }
                }
            }
                
            
            return;
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            internalQuantity = tree.GetInt("internalQuantity");
            processstarted = tree.GetDouble("processstarted");
            
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("internalQuantity", internalQuantity);
            tree.SetDouble("processstarted", processstarted);
        }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            
            dsc.AppendLine("RM   :" + internalQuantity.ToString() + "/" + inputQuantity.ToString());
            dsc.AppendLine("Make :" + recipe);
            if (deviceState == enDeviceState.RUNNING)
            {
                double timeleft = (processingTime + processstarted - Api.World.Calendar.TotalHours);
                timeleft = Math.Floor(timeleft * 100);
                
                dsc.AppendLine("Time Rem :"+timeleft.ToString());
            }
            if (heatRequirement > 0)
            {
                dsc.AppendLine("Input Item Heat must be " + heatRequirement.ToString() + "C");
            }
            if (materials != null && materials.Length > 0)
            {
                dsc.AppendLine("Usable Materials:");
                foreach (string ing in materials)
                {
                    dsc.AppendLine(ing + ",");
                }
            }
        }
    }
}
