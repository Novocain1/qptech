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
        AssetLocation workingitem;
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
        }

        void TryStart()
        {
            BlockPos bp = Pos.Copy().Offset(rmInputFace);
            BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
            deviceState = enDeviceState.MATERIALHOLD;
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
                
                if (!MacerationRecipe.FindMacerate(co, Api)) { continue; }
                workingitem=co.Code.Clone();
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
        static bool failload;
        public MacerationRecipe()
        {

        }
        public static bool FindMacerate(CollectibleObject co,ICoreAPI api)
        {
            if (maceratelist == null) { LoadMacerateList(api); }
            if (maceratelist == null) { return false; }//list failed to load
            if (maceratelist.ContainsKey(co.Code.ToString())) { return true; }
            return false;
        }
        static void LoadMacerateList(ICoreAPI api)
        {
            //maceratelist = new Dictionary<string, List<MacerationRecipe>>();
            maceratelist = api.Assets.TryGet("qptech:config/macerationrecipes.json").ToObject<Dictionary<string, List<MacerationRecipe>>>();
            if (maceratelist == null || maceratelist.Count == 0) { failload = true; }
        }
    }
}
