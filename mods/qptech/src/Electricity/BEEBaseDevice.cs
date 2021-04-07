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

namespace qptech.src
{
    //Device to use up electricity
    //intermediate class, shouldn't generally be used
    public class BEEBaseDevice:BEElectric
    {
        //How many amps to run at maxVolts?
        public enum enDeviceState { IDLE, RUNNING, WARMUP, MATERIALHOLD, ERROR }
       
        protected int requiredAmps = 1;     //how many amps to run
        protected int processingTicks = 30; //how many ticks for process to run
        protected int tickCounter = 0;
        public int RequiredAmps { get { return requiredAmps; } }
        public bool IsPowered { get { return capacitor >= requiredAmps; } }

        protected enDeviceState deviceState = enDeviceState.WARMUP;
        public enDeviceState DeviceState { get { return deviceState; } }
        public override void OnTick(float par)
        {
            base.OnTick(par);
            //override as we don't want to transmit power
            UsePower();
        }
        protected bool animInit = false;
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            animInit = false;
            //if (Block == null || Block.Attributes == null) { return; }
            if (Block.Attributes != null) {
                requiredAmps = Block.Attributes["requiredAmps"].AsInt(requiredAmps);
                processingTicks = Block.Attributes["processingTicks"].AsInt(processingTicks);
            }
            distributionFaces = new List<BlockFacing>(); //no distribution for us!
        }
        protected virtual void UsePower()
        {
            if (!isOn) { return; }
            if (DeviceState == enDeviceState.IDLE||DeviceState==enDeviceState.MATERIALHOLD)
            {
                DoDeviceStart();
            }
            else if (deviceState == enDeviceState.WARMUP)
            {
                tickCounter++;
                if (tickCounter == 10) { tickCounter = 0;deviceState = enDeviceState.IDLE; }
            }
            else { DoDeviceProcessing(); }
        }

        protected virtual void DoDeviceStart()
        {
            if (capacitor >= requiredAmps)
            {
                
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
                    animUtil.StartAnimation(new AnimationMetaData() {
                        Animation = "process", Code = "process",
                        AnimationSpeed = 1f, EaseInSpeed = 1, EaseOutSpeed = 1,
                        Weight = 1, BlendMode = EnumAnimationBlendMode.Add
                    });
                    Api.World.PlaySoundAt(new AssetLocation("sounds/doorslide"), Pos.X, Pos.Y, Pos.Z, null, false, 8, 1);
                }
                
                //sounds/blocks/doorslide.ogg
                DoDeviceProcessing();
            }
            else { DoFailedStart(); }
        }

        protected virtual void DoDeviceProcessing()
        {
            if (tickCounter >= processingTicks)
            {
                DoDeviceComplete();
                return;
            }
            if (capacitor < requiredAmps)
            {
                DoFailedProcessing();
                return;
            }
            tickCounter++;
            capacitor -= requiredAmps;
        }
        //can do some feedback if device can't run
        protected virtual void DoFailedStart()
        {

        }
        //feedback if device cannot process
        protected virtual void DoFailedProcessing()
        {

        }
        //Do whatever needs doing on a successful cycle
        protected virtual void DoDeviceComplete()
        {
            deviceState = enDeviceState.IDLE;
            Block block = Api.World.GetBlock(new AssetLocation("game:bowl-raw"));
            ItemStack outputStack = new ItemStack(block);

            Vec3d pos = Pos.ToVec3d();
            pos.Y += 0.5f;
            Vec3d vel = new Vec3d(0, 0.25f, 0);
            Api.World.SpawnItemEntity(outputStack, pos,vel);
            //Api.World.SpawnItemEntity(grindedStack, this.Pos.ToVec3d().Add(0.5 + face.Normalf.X * 0.7, 0.75, 0.5 + face.Normalf.Z * 0.7), new Vec3d(face.Normalf.X * 0.02f, 0, face.Normalf.Z * 0.02f));
            if (Api.World.Side == EnumAppSide.Client && animUtil != null)
            {
                animUtil.StopAnimation("process");
            }
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            tickCounter = tree.GetInt("tickCounter");
            deviceState = (enDeviceState)tree.GetInt("deviceState");
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("tickCounter", tickCounter);
            tree.SetInt("deviceState", (int)deviceState);
        }
        protected BlockEntityAnimationUtil animUtil
        {
            get
            {
                BEBehaviorAnimatable bea = GetBehavior<BEBehaviorAnimatable>();
                if (bea == null) { return null; }
                return GetBehavior<BEBehaviorAnimatable>().animUtil;
            }
        }
        
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            dsc.AppendLine("Stat :" + DeviceState.ToString());
            dsc.AppendLine("Prog :" + tickCounter.ToString() + "/" + processingTicks.ToString());
        }

        
    }
}
