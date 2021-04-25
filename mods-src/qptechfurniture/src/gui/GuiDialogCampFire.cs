using System;
using System.Linq;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Vintagestory.GameContent
{
    public class GuiDialogBlockCampFire : GuiDialogBlockEntity
    {
        string currentOutputText;

        long lastRedrawMs;
        EnumPosFlag screenPos;

        protected override double FloatyDialogPosition => 0.6;
        protected override double FloatyDialogAlign => 0.8;

        public override double DrawOrder => 0.2;

        public GuiDialogBlockCampFire(string dialogTitle, InventoryBase Inventory, BlockPos BlockEntityPosition,
                                           SyncedTreeAttribute tree, ICoreClientAPI capi)
            : base(dialogTitle, Inventory, BlockEntityPosition, capi)
        {
            if (IsDuplicate) return;
            tree.OnModified.Add(new TreeModifiedListener() { listener = OnAttributesModified });
            Attributes = tree;
        }

        private void OnInventorySlotModified(int slotid)
        {
            SetupDialog();
        }

        void SetupDialog()
        {
            ItemSlot hoveredSlot = capi.World.Player.InventoryManager.CurrentHoveredSlot;
            if (hoveredSlot != null && hoveredSlot.Inventory?.InventoryID != Inventory?.InventoryID)
            {
                //capi.Input.TriggerOnMouseLeaveSlot(hoveredSlot); - wtf is this for?
                hoveredSlot = null;
            }

            string newOutputText = Attributes.GetString("outputText", "");

            GuiElementDynamicText outputTextElem;

            if (SingleComposer != null)
            {
                outputTextElem = SingleComposer.GetDynamicText("outputText");
                outputTextElem.SetNewText(newOutputText, true);
                SingleComposer.GetCustomDraw("symbolDrawer").Redraw();

                currentOutputText = newOutputText;

                outputTextElem.Bounds.fixedOffsetY = 0;

                if (outputTextElem.QuantityTextLines > 2)
                {
                    outputTextElem.Bounds.fixedOffsetY = -outputTextElem.Font.GetFontExtents().Height / RuntimeEnv.GUIScale * 0.65;
                }

                outputTextElem.Bounds.CalcWorldBounds();

                return;
            }

            currentOutputText = newOutputText;
    

            ElementBounds stoveBounds = ElementBounds.Fixed(0, 0, 210 + 10, 250);

            ElementBounds inputSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 30 + 40, 1, 1);
            ElementBounds fuelSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 0, 130 + 50, 1, 1);
            ElementBounds outputSlotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.None, 153, 30 + 40, 1, 1);


            // 2. Around all that is 10 pixel padding
            ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
            bgBounds.BothSizing = ElementSizing.FitToChildren;
            bgBounds.WithChildren(stoveBounds);

            // 3. Finally Dialog
            ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
                .WithFixedAlignmentOffset(IsRight(screenPos) ? -GuiStyle.DialogToScreenPadding : GuiStyle.DialogToScreenPadding, 0)
                .WithAlignment(IsRight(screenPos) ? EnumDialogArea.RightMiddle : EnumDialogArea.LeftMiddle);
            

            SingleComposer = capi.Gui
                .CreateCompo("blockentitystove" + BlockEntityPosition, dialogBounds)
                .AddShadedDialogBG(bgBounds)
                .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
                .BeginChildElements(bgBounds)
                    .AddDynamicCustomDraw(stoveBounds, OnBgDraw, "symbolDrawer")
                    .AddDynamicText("", CairoFont.WhiteDetailText(), EnumTextOrientation.Left, ElementBounds.Fixed(0, 18, 210, 45), "outputText")
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 0 }, fuelSlotBounds, "fuelslot")
                    .AddDynamicText("", CairoFont.WhiteDetailText(), EnumTextOrientation.Left, fuelSlotBounds.RightCopy(17, 16).WithFixedSize(60, 30), "fueltemp")
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 1 }, inputSlotBounds, "oreslot")
                    .AddDynamicText("", CairoFont.WhiteDetailText(), EnumTextOrientation.Left, inputSlotBounds.RightCopy(28, 15).WithFixedSize(60, 40), "oretemp")
                    .AddItemSlotGrid(Inventory, SendInvPacket, 1, new int[] { 2 }, outputSlotBounds, "outputslot")
                .EndChildElements()
                .Compose();

            lastRedrawMs = capi.ElapsedMilliseconds;

            if (hoveredSlot != null)
            {
                SingleComposer.OnMouseMove(new MouseEvent(capi.Input.MouseX, capi.Input.MouseY));
            }

            outputTextElem = SingleComposer.GetDynamicText("outputText");
            outputTextElem.SetNewText(currentOutputText, true);
            outputTextElem.Bounds.fixedOffsetY = 0;

            if (outputTextElem.QuantityTextLines > 2)
            {
                outputTextElem.Bounds.fixedOffsetY = -outputTextElem.Font.GetFontExtents().Height / RuntimeEnv.GUIScale * 0.65;
            }
            outputTextElem.Bounds.CalcWorldBounds();


        }


        private void OnAttributesModified()
        {
            if (!IsOpened()) return;

            float ftemp = Attributes.GetFloat("furnaceTemperature");
            float otemp = Attributes.GetFloat("oreTemperature");

            string fuelTemp = ftemp.ToString("#");
            string oreTemp = otemp.ToString("#");

            fuelTemp += fuelTemp.Length > 0 ? "°C" : "";
            oreTemp += oreTemp.Length > 0 ? "°C" : "";

            if (ftemp > 0 && ftemp <= 20) fuelTemp = Lang.Get("Cold");
            if (otemp > 0 && otemp <= 20) oreTemp = Lang.Get("Cold");

            SingleComposer.GetDynamicText("fueltemp").SetNewText(fuelTemp);
            SingleComposer.GetDynamicText("oretemp").SetNewText(oreTemp);

            if (capi.ElapsedMilliseconds - lastRedrawMs > 500)
            {
                if (SingleComposer != null) SingleComposer.GetCustomDraw("symbolDrawer").Redraw();
                lastRedrawMs = capi.ElapsedMilliseconds;
            }
        }



        private void OnBgDraw(Context ctx, ImageSurface surface, ElementBounds currentBounds)
        {

            // 1. Fire
            ctx.Save();
            Matrix m = ctx.Matrix;
            m.Translate(GuiElement.scaled(5), GuiElement.scaled(53 + 70));
            m.Scale(GuiElement.scaled(0.25), GuiElement.scaled(0.25));
            ctx.Matrix = m;
            capi.Gui.Icons.DrawFlame(ctx);

            double dy = 210 - 210 * (Attributes.GetFloat("fuelBurnTime", 0) / Attributes.GetFloat("maxFuelBurnTime", 1));
            ctx.Rectangle(0, dy, 200, 210 - dy);
            ctx.Clip();
            LinearGradient gradient = new LinearGradient(0, GuiElement.scaled(250), 0, 0);
            gradient.AddColorStop(0, new Color(1, 1, 0, 1));
            gradient.AddColorStop(1, new Color(1, 0, 0, 1));
            ctx.SetSource(gradient);
            capi.Gui.Icons.DrawFlame(ctx, 0, false, false);
            gradient.Dispose();
            ctx.Restore();


            // 2. Arrow Right
            ctx.Save();
            m = ctx.Matrix;
            m.Translate(GuiElement.scaled(63), GuiElement.scaled(1 + 72));
            m.Scale(GuiElement.scaled(0.6), GuiElement.scaled(0.6));
            ctx.Matrix = m;
            capi.Gui.Icons.DrawArrowRight(ctx, 2);

            double cookingRel = Attributes.GetFloat("oreCookingTime") / Attributes.GetFloat("maxOreCookingTime", 1);


            ctx.Rectangle(5, 0, 125 * cookingRel, 100);
            ctx.Clip();
            gradient = new LinearGradient(0, 0, 200, 0);
            gradient.AddColorStop(0, new Color(0, 0.4, 0, 1));
            gradient.AddColorStop(1, new Color(0.2, 0.6, 0.2, 1));
            ctx.SetSource(gradient);
            capi.Gui.Icons.DrawArrowRight(ctx, 0, false, false);
            gradient.Dispose();
            ctx.Restore();
        }



        private void SendInvPacket(object packet)
        {
            capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, packet);
        }


        private void OnTitleBarClose()
        {
            TryClose();
        }


        public override void OnGuiOpened()
        {
            base.OnGuiOpened();
            Inventory.SlotModified += OnInventorySlotModified;

            screenPos = GetFreePos("smallblockgui");
            OccupyPos("smallblockgui", screenPos);
            SetupDialog();
        }

        public override void OnGuiClosed()
        {
            Inventory.SlotModified -= OnInventorySlotModified;

            SingleComposer.GetSlotGrid("fuelslot").OnGuiClosed(capi);
            SingleComposer.GetSlotGrid("oreslot").OnGuiClosed(capi);
            SingleComposer.GetSlotGrid("outputslot").OnGuiClosed(capi);

            base.OnGuiClosed();

            FreePos("smallblockgui", screenPos);
        }
    }
}