using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.MathTools;

namespace qptech.src
{
    class BEEGenerator:BEElectric
    {
        //how many power packets we can generate - will see if every more than one
        bool usesFuel = false;          //Whether item uses fuel
        protected List<string> fuelCodes;   //Valid item & block codes usable as fuel
        
        protected int fuelTicks = 0;    //how many OnTicks a piece of fuel will last for
        int fuelCounter = 0;            //counts down to use fuel
        protected int genAmps = 1;      //how many amps (power packets) are generated per OnTick
        BlockFacing fuelHopperFace;     //which face fuel is loaded from
        bool fueled = false;            //whether device is currently fueld
        bool animInit = false;
        bool usesFuelWhileOn = false;  //always use fuel, even if no load (unless turned off)
        
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (Block.Attributes != null)
            {
                genAmps = Block.Attributes["genAmps"].AsInt(genAmps);
                fuelHopperFace = BlockFacing.FromCode(Block.Attributes["fuelHopperFace"].AsString("up"));
                fuelHopperFace = OrientFace(Block.Code.ToString(), fuelHopperFace);
                string[] fc = Block.Attributes["fuelCodes"].AsArray<string>();
                if (fc != null) { fuelCodes = fc.ToList<string>(); }
                fuelTicks = Block.Attributes["fuelTicks"].AsInt(1);
                usesFuel = Block.Attributes["usesFuel"].AsBool(false);
                usesFuelWhileOn = Block.Attributes["usesFuelWhileOn"].AsBool(false);
                fuelCounter = 0;
            }
            if (api.World.Side == EnumAppSide.Client&&animUtil!=null)
            {
                float rotY = Block.Shape.rotateY;
                animUtil.InitializeAnimator("run", new Vec3f(0, rotY, 0));
                animUtil.StartAnimation(new AnimationMetaData() { Animation = "run", Code = "run", AnimationSpeed = 1, EaseInSpeed = 1, EaseOutSpeed = 1, Weight = 1, BlendMode = EnumAnimationBlendMode.Average });
                animInit = true;
            }
            
        }
        public override void OnTick(float par)
        {
            
            base.OnTick(par);
            if (isOn) {
                GeneratePower(); //Create power packets if possible, base.Ontick will handle distribution attempts
                
            }
            
        }
        //Attempts to generate power
        public virtual void GeneratePower()
        {
            bool trypower = DoGeneratePower();
            
                
            if (Api.World.Side == EnumAppSide.Client && animUtil != null && animInit)
            {
                
                if (trypower)
                {
                    
                    if (animUtil.activeAnimationsByAnimCode.Count==0){
                        animUtil.StartAnimation(new AnimationMetaData() { Animation = "run", Code = "run", AnimationSpeed = 1, EaseInSpeed = 1, EaseOutSpeed = 1, Weight = 1, BlendMode = EnumAnimationBlendMode.Average });
                    }
                }
                else
                {
                    animUtil.StopAnimation("run");
                }

            }

            if (trypower) { capacitor = Math.Min(capacitance, capacitor + MaxAmps); }
            
            return;
        }

        public virtual bool DoGeneratePower()
        {
            if (!isOn) { return false; }
            if (!usesFuel) { return true; } //if we don't use fuel, we can make power
            if (capacitor == capacitance && !usesFuelWhileOn) { return false; }//not necessary to generate power
            if (fueled && fuelCounter < fuelTicks) //on going burning of current fuel item
            {
                fuelCounter++;
                return true;
            }
            //Now we begin trying to fuel
            fueled = false;fuelCounter = 0;
            BlockPos bp = Pos.Copy().Offset(fuelHopperFace);
            BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
            var inputContainer = checkblock as BlockEntityContainer;
            if (inputContainer == null) { return false; } //no fuel container at all
            if (inputContainer.Inventory.Empty) { return false; } //the fuel container is empty
            //check each inventory slot in the container
            for (int c = 0; c < inputContainer.Inventory.Count; c++)
            {
                ItemSlot checkslot = inputContainer.Inventory[c];
                if (checkslot == null) { continue; }
                if (checkslot.StackSize == 0) { continue; }
                
                bool match = false;
                if (checkslot.Itemstack.Item != null && fuelCodes.Contains(checkslot.Itemstack.Item.Code.ToString())) { match = true; }
                else if (checkslot.Itemstack.Block != null && fuelCodes.Contains(checkslot.Itemstack.Block.Code.ToString())) { match = true; }
                if (match&& checkslot.StackSize > 0)
                {
                    
                    checkslot.TakeOut(1);
                    checkslot.MarkDirty();
                    fueled = true;
                }
            }
            return fueled;
        }
        //generators don't receive power
        public override int ReceivePacketOffer(BEElectric from,int volt, int amp)
        {
            return 0;
        }
        BlockEntityAnimationUtil animUtil
        {
            get {
                BEBehaviorAnimatable bea = GetBehavior<BEBehaviorAnimatable>();
                if (bea == null) { return null; }
                return GetBehavior<BEBehaviorAnimatable>().animUtil;
            }
        }
        //TODO need turn on and turn off functions
        public override void TogglePower()
        {
            
            if (justswitched) { return; }
            isOn = !isOn;
            justswitched = true;
            if (Api.World.Side == EnumAppSide.Client&&animUtil!=null)
            {
                if (!animInit)
                {
                    float rotY = Block.Shape.rotateY;
                    animUtil.InitializeAnimator("run", new Vec3f(0, rotY, 0));
                    animInit = true;
                }
                if (isOn)
                {
                    
                    animUtil.StartAnimation(new AnimationMetaData() { Animation = "run", Code = "run", AnimationSpeed = 1, EaseInSpeed = 1, EaseOutSpeed = 1, Weight = 1, BlendMode = EnumAnimationBlendMode.Average });
                }
                else
                {
                    animUtil.StopAnimation("run");
                }

            }
            Api.World.PlaySoundAt(new AssetLocation("sounds/electriczap"), Pos.X, Pos.Y, Pos.Z, null, false, 8, 1);
        }
        public override int NeedPower()
        {
            return 0;
        }
    }
}
