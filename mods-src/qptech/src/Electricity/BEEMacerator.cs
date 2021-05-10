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
    class BEEMacerator:BEEBaseDevice
    {
        protected BlockFacing rmInputFace=BlockFacing.FromCode("up"); //what faces will be checked for input containers
        protected BlockFacing outputFace = BlockFacing.FromCode("down");
        CollectibleObject workingitem;
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            SetupAnimation();
        }
        protected override void DoDeviceStart()
        {
            if (Capacitor < requiredAmps) { DoFailedStart(); }
            tickCounter = 0;
            TryStart();
                        
        }
        protected override void DoDeviceComplete()
        {
            deviceState = enDeviceState.IDLE;
            BlockPos bp = Pos.Copy().Offset(outputFace);
            BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
            var outputContainer = checkblock as BlockEntityContainer;
            DummyInventory dummy = new DummyInventory(Api);

            List<ItemStack> outputitems=MacerationRecipe.GetMacerate(workingitem,Api);
            foreach (ItemStack outitem in outputitems)
            {
                if (outitem == null) { continue; }
                dummy[0].Itemstack = outitem;
                //no output conatiner, spitout stuff
                if (outputContainer != null)
                {


                    WeightedSlot tryoutput = outputContainer.Inventory.GetBestSuitedSlot(dummy[0]);

                    if (tryoutput.slot != null)
                    {
                        ItemStackMoveOperation op = new ItemStackMoveOperation(Api.World, EnumMouseButton.Left, 0, EnumMergePriority.DirectMerge, dummy[0].StackSize);

                        dummy[0].TryPutInto(tryoutput.slot, ref op);

                 
                    }
                }
                Vec3d pos = bp.ToVec3d();

                dummy.DropAll(pos);
            }
        }

        void TryStart()
        {
            BlockPos bp = Pos.Copy().Offset(rmInputFace);
            BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
            deviceState = enDeviceState.MATERIALHOLD;
            workingitem = null;
            var inputContainer = checkblock as BlockEntityContainer;
            if (inputContainer == null) { return; }
            if (inputContainer.Inventory.Empty) { return; }
            for (int c = 0; c < inputContainer.Inventory.Count; c++)
            {
                ItemSlot checkslot = inputContainer.Inventory[c];
                if (checkslot == null) { continue; }
                if (checkslot.StackSize ==0) { continue; }
                bool match = false;
                Item checkitem = checkslot.Itemstack.Item;
                Block checkiblock = checkslot.Itemstack.Block;
                
                if (checkitem == null && checkiblock == null)
                {
                    continue;
                }
                CollectibleObject co;
                if (checkitem != null) { co = checkitem as CollectibleObject; }
                else { co = checkiblock as CollectibleObject; }
                
                if (!MacerationRecipe.CanMacerate(co, Api)) { continue; }
                workingitem = co;
                //Item has been set, need to pull one item from the stack
                deviceState = enDeviceState.RUNNING;
                checkslot.TakeOut(1);
                checkslot.MarkDirty();
                break;
            }
        }

        protected override void DoDeviceProcessing()
        {
            base.DoDeviceProcessing();
        }




        void SetupAnimation()
        {
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
                    AnimationSpeed = 1,
                    EaseInSpeed = 1,
                    EaseOutSpeed = 1,
                    Weight = 1,
                    BlendMode = EnumAnimationBlendMode.Add
                });

            }
        }
    }
    public class MacerationRecipe
    {
        public string outputmaterial;
        public int outputquantity=1;
        public float odds=1;

        static Dictionary<string, List<MacerationRecipe>> maceratelist;
        static Dictionary<string, string>basiccodeswaplist;
        static bool failload;
        public MacerationRecipe()
        {

        }
        public static bool CanMacerate(CollectibleObject co,ICoreAPI api)
        {
            //TODO: change this to "CanMacerate", add a FindMacerate that returns items:
            //   - from the maceratelist directly - adding extra output based on RNG
            //   - generically where possible - eg stone block to stone gravel etc
            if (maceratelist == null) { LoadMacerateList(api); }
            if (co == null) { return false; }
            if (basiccodeswaplist.ContainsKey(co.FirstCodePart())) { return true; }
            
            if (maceratelist == null) { return false; }//list failed to load
            if (maceratelist.ContainsKey(co.Code.ToString())) { return true; }
            return false;
        }

        public static List<ItemStack> GetMacerate(CollectibleObject co, ICoreAPI api)
        {
            
            List<ItemStack> outputstack = new List<ItemStack>();
            if (!CanMacerate(co, api)) { return outputstack; }
            string fcp = co.FirstCodePart();
            //Handle basic maceration - eg stone to equivalent gravel
            if (basiccodeswaplist.ContainsKey(fcp))
            {
                string al = co.Code.ToString();
                al=al.Replace(fcp, basiccodeswaplist[fcp]);
                
                Block outputBlock = api.World.GetBlock(new AssetLocation(al));
                Item outputItem = api.World.GetItem(new AssetLocation(al));
                if (outputBlock != null)
                {
                    ItemStack itmstk = new ItemStack(outputBlock,1);
                    outputstack.Add(itmstk);
                }
                if (outputItem != null)
                {
                    ItemStack itmstk = new ItemStack(outputItem, 1);
                    outputstack.Add(itmstk);
                }
            }
            return outputstack;
        }
        static void LoadMacerateList(ICoreAPI api)
        {
            //maceratelist = new Dictionary<string, List<MacerationRecipe>>();
            maceratelist = api.Assets.TryGet("qptech:config/macerationrecipes.json").ToObject<Dictionary<string, List<MacerationRecipe>>>();
            if (maceratelist == null || maceratelist.Count == 0) { failload = true; }
            basiccodeswaplist = new Dictionary<string, string>();
            basiccodeswaplist["rock"] = "gravel";
            basiccodeswaplist["gravel"] = "sand";

        }
    }
}
