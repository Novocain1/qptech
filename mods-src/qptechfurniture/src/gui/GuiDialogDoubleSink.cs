using Cairo;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace QptechFurniture.src
{
	public class GuiDialogDoubleSink : GuiDialogBlockEntity
	{
		EnumPosFlag screenPos;


		protected override double FloatyDialogPosition => 0.6;
		protected override double FloatyDialogAlign => 0.8;


		//public int Slot { get; set; } = 8;

		public override double DrawOrder => 0.2;


		public GuiDialogDoubleSink(string dialogTitle, InventoryBase inventory, BlockPos blockEntityPos, ICoreClientAPI capi) : base(dialogTitle, inventory, blockEntityPos, capi)
		{
			if (IsDuplicate) return;

		}

		void SetupDialog()
		{

			/*
			   double pad = GuiElementItemSlotGrid.unscaledSlotPadding;

			   int rows = (int)Math.Ceiling(Inventory.Count / 2f); // row col number so 2,4,6,8

			   ElementBounds barrelBoundsLeft = ElementBounds.Fixed(0, 30, 150, 200); // (0, 30, 150, 200);
			   ElementBounds barrelBoundsRight = ElementBounds.Fixed(0, 30, 150, 200); // (100, 30, 150, 200);

			   ElementBounds slotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.LeftFixed, pad, 20 + pad, 2, rows).FixedGrow(2 * pad, 1 * pad); // (EnumDialogArea.None, pad, 20 + pad, 2, rows).FixedGrow(2 * pad, 1 * pad);

			   ElementBounds fullnessMeterBounds = ElementBounds.Fixed(150, 25, 40, 195); // X,Y Pos and Width, Heigth  // Good (200, 25, 40, 200)

			   ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding, 10f);
			   bgBounds.BothSizing = ElementSizing.FitToChildren;
			   bgBounds.WithChildren(barrelBoundsLeft, barrelBoundsRight);

			   // 3. Finally Dialog
			   ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
				   .WithFixedAlignmentOffset(IsRight(screenPos) ? -GuiStyle.DialogToScreenPadding : GuiStyle.DialogToScreenPadding, 0)
				   .WithAlignment(IsRight(screenPos) ? EnumDialogArea.RightMiddle : EnumDialogArea.LeftMiddle)
			*/

			double pad = GuiElementItemSlotGrid.unscaledSlotPadding;

			int rows = (int)Math.Ceiling(Inventory.Count / 2f); // row col number so 2,4,6,8

			ElementBounds barrelBoundsLeft = ElementBounds.Fixed(0, 0, 150, 150); // (0, 30, 150, 200);
			ElementBounds barrelBoundsRight = ElementBounds.Fixed(0, 0, 150, 150); // (100, 30, 150, 200);

			ElementBounds slotBounds = ElementStdBounds.SlotGrid(EnumDialogArea.LeftFixed, pad, 20 + pad, 2, rows).FixedGrow(2 * pad, 1 * pad); // (EnumDialogArea.None, pad, 20 + pad, 2, rows).FixedGrow(2 * pad, 1 * pad);

			ElementBounds fullnessMeterBounds = ElementBounds.Fixed(150, 25, 40, 195); // X,Y Pos and Width, Heigth  // Good (200, 25, 40, 200)

			ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding, 30f);
			bgBounds.BothSizing = ElementSizing.FitToChildren;
			bgBounds.WithChildren(barrelBoundsLeft, barrelBoundsRight);

			// 3. Finally Dialog
			ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog
				.WithFixedAlignmentOffset(IsRight(screenPos) ? -GuiStyle.DialogToScreenPadding : GuiStyle.DialogToScreenPadding, 30f)
				.WithAlignment(IsRight(screenPos) ? EnumDialogArea.RightMiddle : EnumDialogArea.LeftMiddle)

			;

			if (!capi.Settings.Bool["immersiveMouseMode"])
			{
				//dialogBounds.fixedOffsetY += (barrelBoundsLeft.fixedHeight + 65);
			}



			SingleComposer = capi.Gui
			  .CreateCompo("blockentitydoublesink" + BlockEntityPosition, dialogBounds)
			  .AddShadedDialogBG(bgBounds)
			  .AddDialogTitleBar(DialogTitle, OnTitleBarClose)
			  .BeginChildElements(bgBounds)
				.AddItemSlotGrid(Inventory, SendInvPacket, 2, new int[] { 0, 2, 3, 4, 5, 6, 7, 8 }, slotBounds, "Slot") // ID Slots And Item Slot And Don't Use 1 it is being used by water.

					  //.AddSmallButton("Empty", onEmptyClick, ElementBounds.Fixed(0, 140, 80, 25), EnumButtonStyle.Normal, EnumTextOrientation.Center)
					  // .AddSmallButton("Fill", onFillClick, ElementBounds.Fixed(0, 100, 80, 25), EnumButtonStyle.Normal, EnumTextOrientation.Center)


					  .AddInset(fullnessMeterBounds.ForkBoundingParent(2, 2, 2, 2), 2)
					.AddDynamicCustomDraw(fullnessMeterBounds, fullnessMeterDraw, "liquidBar")


			  //.AddDynamicText(getContentsText(), CairoFont.WhiteDetailText(), EnumTextOrientation.Left, barrelBoundsRight, "contentText")

			  .EndChildElements()
		  .Compose();
		}
		public void UpdateContents()
		{
			SingleComposer.GetCustomDraw("liquidBar").Redraw();
		}

		private void fullnessMeterDraw(Context ctx, ImageSurface surface, ElementBounds currentBounds)
		{
			ItemSlot liquidSlot = Inventory[1];
			if (liquidSlot.Empty) return;

			BlockEntityDoubleSink besink = capi.World.BlockAccessor.GetBlockEntity(BlockEntityPosition) as BlockEntityDoubleSink;
			float itemsPerLitre = 1f;
			int capacity = besink.CapacityLitres;

			WaterTightContainableProps props = BlockLiquidContainerBase.GetInContainerProps(liquidSlot.Itemstack);
			if (props != null)
			{
				itemsPerLitre = props.ItemsPerLitre;
				capacity = Math.Max(capacity, props.MaxStackSize);
			}

			float fullnessRelative = liquidSlot.StackSize / itemsPerLitre / capacity;

			double offY = (1 - fullnessRelative) * currentBounds.InnerHeight;

			ctx.Rectangle(0, offY, currentBounds.InnerWidth, currentBounds.InnerHeight - offY);
			//ctx.SetSourceRGBA(ravg/255.0, gavg / 255.0, bavg / 255.0, aavg / 255.0);
			//ctx.Fill();

			CompositeTexture tex = liquidSlot.Itemstack.Collectible.Attributes?["waterTightContainerProps"]?["texture"]?.AsObject<CompositeTexture>(null, liquidSlot.Itemstack.Collectible.Code.Domain);
			if (tex != null)
			{
				ctx.Save();
				Matrix m = ctx.Matrix;
				m.Scale(GuiElement.scaled(3), GuiElement.scaled(3));
				ctx.Matrix = m;

				AssetLocation loc = tex.Base.Clone().WithPathAppendixOnce(".png");
				GuiElement.fillWithPattern(capi, ctx, loc.Path, true, false);

				ctx.Restore();
			}
		}

		private void SendInvPacket(object packet)
		{
			capi.Network.SendBlockEntityPacket(BlockEntityPosition.X, BlockEntityPosition.Y, BlockEntityPosition.Z, packet);
		}


		private void OnTitleBarClose()
		{
			TryClose();
		}


		private void OnInventorySlotModified(int slotid)
		{
			//SetupDialog();
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

			SingleComposer.GetSlotGrid("Slot").OnGuiClosed(capi);

			base.OnGuiClosed();

			FreePos("smallblockgui", screenPos);
		}



	}
}