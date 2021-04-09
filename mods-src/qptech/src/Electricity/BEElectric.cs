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
    public class BEElectric : BlockEntity
    {
        /*base class to handle electrical devices*/
        protected int maxAmps=1;    //how many packets that can move at once
        protected int maxVolts=16;  //how many volts it can handle before exploding
        protected int capacitance=1;//how many packets it can store
        protected  int capacitor;  //packets currently stored (the ints store the volts for each packet)
        protected bool isOn=true;        //if it's not on it won't do any power processing
        protected List<BEElectric>outputConnections; //what we are connected to
        protected List<BEElectric>inputConnections; //what we are connected to
        protected List<BEElectric> usedconnections; //track if already traded with in a given turn
        protected List<BlockFacing> distributionFaces; //what faces are valid for distributing power
        protected List<BlockFacing> receptionFaces; //what faces are valid for receiving power
        public int MaxAmps { get { return maxAmps; } }
        public int MaxVolts { get { return maxVolts; } }

        public bool IsPowered { get { return false; } }
        public bool IsOn { get { return isOn; } }
        protected bool notfirsttick = false;
        protected bool justswitched = false; //create a delay after the player switches power

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            //TODO need to load list of valid faces from the JSON for this stuff
            SetupIOFaces();
            if (Electricity.electricalDevices == null) { Electricity.electricalDevices = new List<BEElectric>(); }
            Electricity.electricalDevices.Add(this);
            
            if (outputConnections == null) { outputConnections = new List<BEElectric>(); }
            if (inputConnections == null) { inputConnections = new List<BEElectric>(); }
            if (Block.Attributes == null) { api.World.Logger.Error("ERROR BEE INITIALIZE HAS NO BLOCK");return; }
            maxAmps = Block.Attributes["maxAmps"].AsInt(maxAmps);
            maxVolts = Block.Attributes["maxVolts"].AsInt(maxVolts);
            capacitance = Block.Attributes["capacitance"].AsInt(capacitance);
            
            RegisterGameTickListener(OnTick, 100);
            notfirsttick = false;
        }

        //attempt to load power distribution and reception faces from attributes, and orient them to this blocks face if necessary
        public virtual void SetupIOFaces()
        {
             string[] cfaces= { };
            
            if (Block.Attributes == null)
            {
                distributionFaces = BlockFacing.HORIZONTALS.ToList<BlockFacing>();
                receptionFaces = BlockFacing.HORIZONTALS.ToList<BlockFacing>();
                return;
            }
            if (!Block.Attributes.KeyExists("receptionFaces")){ receptionFaces = BlockFacing.HORIZONTALS.ToList<BlockFacing>(); }
            else
            {
                cfaces = Block.Attributes["receptionFaces"].AsArray<string>(cfaces);
                receptionFaces = new List<BlockFacing>();
                foreach (string f in cfaces)
                {
                    receptionFaces.Add(OrientFace(Block.Code.ToString(),BlockFacing.FromCode(f)));
                }
            }
            
            if (!Block.Attributes.KeyExists("distributionFaces")) { distributionFaces = BlockFacing.HORIZONTALS.ToList<BlockFacing>(); }
            else
            {
                cfaces = Block.Attributes["distributionFaces"].AsArray<string>(cfaces);
                distributionFaces = new List<BlockFacing>();
                foreach (string f in cfaces)
                {
                    distributionFaces.Add(OrientFace(Block.Code.ToString(), BlockFacing.FromCode(f)));
                }
            }
            
        }
        public virtual void FindConnections()
        {
            FindInputConnections();
            FindOutputConnections();

        }
        protected virtual void FindInputConnections()
        {
            //BlockFacing probably has useful stuff to do this right
            
            foreach (BlockFacing bf in receptionFaces)
            {


                BlockPos bp = Pos.Copy().Offset(bf);
                
                BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
                var bee = checkblock as BEElectric;
                if (bee == null) { continue; }
                if (bee.TryOutputConnection(this) && !inputConnections.Contains(bee)) { inputConnections.Add(bee); }

            }
        }

        protected virtual void FindOutputConnections()
        {
            //BlockFacing probably has useful stuff to do this right
            
            foreach (BlockFacing bf in distributionFaces)
            {
                BlockPos bp = Pos.Copy().Offset(bf);
                BlockEntity checkblock = Api.World.BlockAccessor.GetBlockEntity(bp);
                var bee = checkblock as BEElectric;
                if (bee == null) { continue; }
                if (bee.TryInputConnection(this) && !outputConnections.Contains(bee)) { outputConnections.Add(bee); }

            }
        }
        
        //Allow devices to connection to each other
        //TODO add way to connect valid faces, especially if placed in different directions
        public virtual bool TryInputConnection(BEElectric connectto)
        {
            if (inputConnections == null) { inputConnections = new List<BEElectric>(); }
            Vec3d vector = connectto.Pos.ToVec3d() - Pos.ToVec3d();
            BlockFacing bf = BlockFacing.FromVector(vector.X,vector.Y,vector.Z);
            if (receptionFaces == null) { return false; }
            if (!receptionFaces.Contains(bf)) { return false; }
            if (!inputConnections.Contains(connectto)) { inputConnections.Add(connectto); MarkDirty(); }
            return true;
        }
        public virtual bool TryOutputConnection(BEElectric connectto)
        {
            if (outputConnections == null) { outputConnections = new List<BEElectric>(); }
            Vec3d vector = connectto.Pos.ToVec3d() - Pos.ToVec3d();
            BlockFacing bf = BlockFacing.FromVector(vector.X, vector.Y, vector.Z);
            if (distributionFaces == null) { return false; }
            if (!distributionFaces.Contains(bf)) { return false; }
            if (!outputConnections.Contains(connectto)) { outputConnections.Add(connectto); MarkDirty(); }
            return true;
        }

        //Tell a connection to remove itself
        public virtual void RemoveConnection(BEElectric disconnect)
        {
            inputConnections.Remove(disconnect);
            outputConnections.Remove(disconnect);
        }

        public override void OnBlockBroken()
        {
            base.OnBlockBroken();
            Electricity.electricalDevices.Remove(this);
            foreach (BEElectric bee in inputConnections) { bee.RemoveConnection(this); }
            foreach (BEElectric bee in outputConnections) { bee.RemoveConnection(this); }
        }
        public virtual void OnTick(float par)
        {
            if (!notfirsttick)
            {
                FindConnections();
                notfirsttick = true;
            }
            if (isOn) { DistributePower(); }
            usedconnections = new List<BEElectric>(); //clear record of connections for next tick
            justswitched = false;
        }

        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            dsc.AppendLine("   On:" + isOn.ToString());
            dsc.AppendLine("Volts:"+MaxVolts.ToString()+"V");
            dsc.AppendLine("Power:" + capacitor.ToString() + "/" + capacitance.ToString());
            dsc.AppendLine("IN:" + inputConnections.Count.ToString() + " OUT:" + outputConnections.Count.ToString());
        }
        
        //Used for other power devices to offer this device some energy
        public virtual int ReceivePacketOffer(BEElectric from, int inVolt, int inAmp) //eg 2
        {
            if (usedconnections == null) { usedconnections = new List<BEElectric>(); }
            if (!isOn) { return 0; }//Not even on
            if (inVolt > maxVolts) { DoOverload(); return 0; }//!TOO MANY VOLTS!
            if (inVolt < maxVolts) { return 0; }// not enough volts
            if (capacitor>=capacitance) { return 0; }//already full
            inAmp = Math.Min(inAmp, MaxAmps); //can only move a certain amount of amps - eg 2
            int useamps = Math.Min(inAmp, capacitance - capacitor); //2
            capacitor += useamps;//capacitor=2
            usedconnections.Add(from);
            if (useamps != 0) { MarkDirty(); }//not zero should be dirty
            return useamps;//return 2
        }
        
        //Attempt to send out power (can be overridden for devices that only use power)
        public virtual void DistributePower()
        {
            if (usedconnections == null) { usedconnections = new List<BEElectric>(); }
            if (!isOn) { return; } //can't generator power if off
            
            if (outputConnections == null) { return; } //nothing hooked up
            if (outputConnections.Count==0) { return; }
            
            int ampsMoved = 0;
            //Build a list of power demands (want to send from highest demand to lowest)
            //TODO could probably all be compressed in to one linq query
            Dictionary<BEElectric,int> powerRequests = new Dictionary< BEElectric,int>();
            foreach (BEElectric bee in outputConnections)
            {
                if (bee == null) { continue; }
                if (usedconnections.Contains(bee)) { continue; }
                if (bee.NeedPower() > 0) { powerRequests.Add(bee, bee.NeedPower()); }
            }
            if (powerRequests.Count == 0) { return; }
            var sortedDict = from entry in powerRequests orderby entry.Value descending select entry;

            foreach (KeyValuePair<BEElectric,int>kvp in sortedDict)
            {
                
                if (capacitor == 0) { break; } //no power to give 
                if (kvp.Key == null) { continue; }
                //if (kvp.Value > capacitor) { continue; } //Connection has more power than me skip
                if (usedconnections.Contains(kvp.Key)) { continue; } //already traded with this bee
                int powerOffer = Math.Min(capacitor, MaxAmps); //offer as much as possible 
                int powerUsed = kvp.Key.ReceivePacketOffer(this,MaxVolts, powerOffer);
                ampsMoved += powerUsed;
                capacitor -= powerUsed;
                
            }
            if (ampsMoved != 0) { MarkDirty(); }
            
        }
        
        public virtual void DoOverload()
        {
            //BOOOOM!
        }

        public virtual void TogglePower()
        {
            if (justswitched) { return; }
            isOn = !isOn;
            justswitched = true;
            Api.World.PlaySoundAt(new AssetLocation("sounds/electriczap"), Pos.X, Pos.Y, Pos.Z, null, false, 8, 1);
        }

        public virtual int NeedPower()
        {
            int needs = 0;
            if (isOn)
            {
                needs = capacitance - capacitor;
                if (needs < 0) { needs = 0; }
                needs = Math.Min(needs, MaxAmps);
            }
            return needs;
            
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            capacitor = tree.GetInt("capacitor");
            isOn = tree.GetBool("isOn");
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("capacitor", capacitor);
            tree.SetBool("isOn", isOn);
        }

        //Take a block code (that ends in a cardinal direction) and a BlockFacing,
        //and rotate it, returning the appropriate blockfacing
        public static BlockFacing OrientFace(string checkBlockCode, BlockFacing toChange)
        {
            if (!toChange.IsHorizontal) { return toChange; }
            if (checkBlockCode.EndsWith("east"))
            {

                toChange = toChange.GetCW();
            }
            else if (checkBlockCode.EndsWith("south"))
            {
                toChange = toChange.GetCW().GetCW();
            }
            else if (checkBlockCode.EndsWith("west"))
            {
                toChange = toChange.GetCCW();
            }
            return toChange;
        }
    }
}
