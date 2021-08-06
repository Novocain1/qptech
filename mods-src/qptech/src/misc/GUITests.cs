using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace qptech.src
{
    /// <summary>
    /// Renders a progress bar hud in the top left corner of the screen
    /// </summary>
    public class HudOverlaySample : ModSystem
    {
        ICoreClientAPI capi;
        WeirdProgressBarRenderer renderer;
        
        public override bool ShouldLoad(EnumAppSide side)
        {
           
             return false; //comment out to activate test
            return side == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;
            renderer = new WeirdProgressBarRenderer(api);
            
            api.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }


    public class WeirdProgressBarRenderer : IRenderer
    {

        MeshRef meshRef;
        ICoreClientAPI capi;
        ITexPositionSource tmpTextureSource;
        Matrixf mvMatrix = new Matrixf();
        
       

        public double RenderOrder { get { return 0; } }

        public int RenderRange { get { return 10; } }

        public WeirdProgressBarRenderer(ICoreClientAPI api)
        {
            this.capi = api;

            // This will get a quad with vertices inside [-1,-1] till [1,1]
            meshRef = api.Render.UploadMesh(QuadMeshUtil.GetQuad());
            tmpTextureSource = capi.Tesselator.GetTexSource(capi.World.GetBlock(new AssetLocation("platepile")));
        }


        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            IShaderProgram curShader = capi.Render.CurrentActiveShader;

            Vec4f color = new Vec4f(1, 1, 1, 1);
            curShader.Uniform("rgbaIn", color);
            curShader.Uniform("extraGlow", 0);
            curShader.Uniform("applyColor", 0);
            curShader.Uniform("tex2d", 0);
            //curShader.Uniform("noTexture", 1f);
            mvMatrix
                .Set(capi.Render.CurrentModelviewMatrix)
                .Translate(10, 10, 50)
                .Scale(100, 100, 0)
                .Translate(0.5f, 0.5f, 0)
                .Scale(0.5f, 0.5f, 0)
            ;

            curShader.UniformMatrix("projectionMatrix", capi.Render.CurrentProjectionMatrix);
            curShader.UniformMatrix("modelViewMatrix", mvMatrix.Values);

            capi.Render.RenderMesh(meshRef);


        }
        /*
         * This renderer will make a viewport if rendered on a mesh!
         * public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            IShaderProgram curShader = capi.Render.CurrentActiveShader;

            Vec4f color = new Vec4f(1, 1, 1, 1);
            curShader.Uniform("rgbaIn", color);
            curShader.Uniform("extraGlow", 0);
            curShader.Uniform("applyColor", 0);
            curShader.Uniform("tex2d", 0);
            //curShader.Uniform("noTexture", 1f);
            mvMatrix
                .Set(capi.Render.CurrentModelviewMatrix)
                .Translate(10, 10, 50)
                .Scale(100, 100, 0)
                .Translate(0.5f, 0.5f, 0)
                .Scale(0.5f, 0.5f, 0)
            ;

            curShader.UniformMatrix("projectionMatrix", capi.Render.CurrentProjectionMatrix);
            curShader.UniformMatrix("modelViewMatrix", mvMatrix.Values);

            capi.Render.RenderMesh(meshRef);


        }*/
        public void Dispose()
        {
            
            capi.Render.DeleteMesh(meshRef);
        }
    }
}