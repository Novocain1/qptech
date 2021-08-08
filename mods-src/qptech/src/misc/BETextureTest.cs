using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.API.Util;


namespace qptech.src
{
    public class BETextureTest : BlockEntity,ITexPositionSource
    {
        List<string> gaugetextures;
        int texno = 0;
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                //capi.BlockTextureAtlas.Positions[Block.Textures[path].Baked.TextureSubId];
                return capi.BlockTextureAtlas.Positions[Block.Textures[gaugetextures[texno]].Baked.TextureSubId];
            }
        }
        public Size2i AtlasSize => (Api as ICoreClientAPI).BlockTextureAtlas.Size;
        ICoreClientAPI capi;
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            if (api is ICoreClientAPI)
            {
                capi = api as ICoreClientAPI;
            }
            gaugetextures = new List<string>();
            gaugetextures.Add("roundgauge-0");
            gaugetextures.Add("roundgauge-50");
            gaugetextures.Add("roundgauge-100");

        }
        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder dsc)
        {
            base.GetBlockInfo(forPlayer, dsc);
            dsc.Append("HI");

            //dsc.AppendLine("IN:" + inputConnections.Count.ToString() + " OUT:" + outputConnections.Count.ToString());
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            texno++;
            if (texno >= gaugetextures.Count) { texno = 0; }
            Shape shape = capi.TesselatorManager.GetCachedShape(new AssetLocation("machines:block/metal/electric/roundgauge0"));
            MeshData meshdata;
            capi.Tesselator.TesselateShape("roundgauge0", shape, out meshdata, this);
            mesher.AddMeshData(meshdata);
            return true;
        }
    }

    
    public class TextureTestLoader : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockEntityClass("BETextureTest", typeof(BETextureTest));
        }
    }
}
