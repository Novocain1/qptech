using System;
using System.Linq;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace qptech.src
{
    class GUIAssemblerStatus:GuiDialogBlockEntity
    {
        ICoreClientAPI api;
        public GUIAssemblerStatus(string dialogTitle, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle,blockEntityPos,capi)
        {
            api = capi;
        }
        public void SetupDialog(BEEAssembler bea)
        {
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

            // Just a simple 300x100 pixel box with 40 pixels top spacing for the title bar
            ElementBounds textBounds = ElementBounds.Fixed(0, 40, 300, 600);

            // Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(textBounds);
            string guicomponame = bea.Pos.ToString()+"Assembler";
            string statustext = "REQUIRE INPUT: <strong>" + bea.RM.ToUpper()+"</strong><br>MAKING: <strong>"+bea.FG.ToUpper()+"</strong><br>STATUS: <strong>"+bea.Status+"</strong>";
            if (bea.Materials.Length > 0&&bea.DeviceState==BEEBaseDevice.enDeviceState.MATERIALHOLD)
            {
                statustext += "<br>USABLE MATERIALS:<br><strong>";
                int c = 0;
                foreach (string i in bea.Materials)
                {
                    if (c != 0) { statustext += ", "; }
                    statustext += i.ToUpper();
                    c++;
                }
                statustext += "</strong>";
            }
            SingleComposer = capi.Gui.CreateCompo(guicomponame, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar("Assembler Status", OnTitleBarCloseClicked)
                .AddRichtext(statustext, CairoFont.WhiteDetailText(), textBounds)
                
                .Compose()
            ;

        }
        public override bool TryOpen()
        {
            
            return base.TryOpen();
        }
        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }
        public override void OnRenderGUI(float deltaTime)
        {

            base.OnRenderGUI(deltaTime);
            if (api == null) { return; }
            LoadedTexture textTexture = new LoadedTexture(api);
            
            api.Render.Render2DLoadedTexture(textTexture, 0, 0);
            
        }

    }
}
