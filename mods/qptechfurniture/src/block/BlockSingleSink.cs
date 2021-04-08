using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace QptechFurniture.src
{
	public class BlockSingleSink : ModdedBlockLiquidContainerBase
	{
		public override int GetContainerSlotId(IWorldAccessor world, BlockPos pos)
		{
			return 1;
		}

		public override int GetContainerSlotId(IWorldAccessor world, ItemStack containerStack)
		{
			return 1;
		}

		// Override to drop the barrel empty and drop its contents instead
		public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
		{
			if (world.Side == EnumAppSide.Server && (byPlayer == null || byPlayer.WorldData.CurrentGameMode != EnumGameMode.Creative))
			{
				ItemStack[] drops = new ItemStack[] { new ItemStack(this) };

				for (int i = 0; i < drops.Length; i++)
				{
					world.SpawnItemEntity(drops[i], new Vec3d(pos.X + 0.5, pos.Y + 0.5, pos.Z + 0.5), null);
				}

				world.PlaySoundAt(Sounds.GetBreakSound(byPlayer), pos.X, pos.Y, pos.Z, byPlayer);
			}

			if (EntityClass != null)
			{
				BlockEntity entity = world.BlockAccessor.GetBlockEntity(pos);
				if (entity != null)
				{
					entity.OnBlockBroken();
				}
			}

			world.BlockAccessor.SetBlock(0, pos);
		}

		public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handling)
		{

		}

		public override int TryPutContent(IWorldAccessor world, ItemStack containerStack, ItemStack contentStack, int desiredItems)
		{
			return 0;
		}

		public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
		{
			return new WorldInteraction[]
			{
				new WorldInteraction()
				{
					ActionLangCode = "heldhelp-place",
					MouseButton = EnumMouseButton.Right,
					ShouldApply = (wi, bs, es) => {
						return true;
					}
				}
			};
		}


		public int GetBarrelHashCode(IClientWorldAccessor world, ItemStack contentStack, ItemStack liquidStack)
		{
			string s = contentStack?.StackSize + "x" + contentStack?.Collectible.Code.ToShortString();
			s += liquidStack?.StackSize + "x" + liquidStack?.Collectible.Code.ToShortString();
			return s.GetHashCode();
		}

		public override void OnLoaded(ICoreAPI api)
		{

			if (Attributes?["capacityLitres"].Exists == true)
			{
				capacityLitresFromAttributes = Attributes["capacityLitres"].AsInt(50);
			}


			if (api.Side != EnumAppSide.Client) return;
			ICoreClientAPI capi = api as ICoreClientAPI;

			interactions = ObjectCacheUtil.GetOrCreate(api, "liquidContainerBase", () =>
			{
				List<ItemStack> liquidContainerStacks = new List<ItemStack>();

				foreach (CollectibleObject obj in api.World.Collectibles)
				{
					if ((obj is BlockBowl && obj.LastCodePart() != "raw") || obj is ILiquidSource || obj is ILiquidSink || obj is BlockWateringCan)
					{
						List<ItemStack> stacks = obj.GetHandBookStacks(capi);
						if (stacks != null) liquidContainerStacks.AddRange(stacks);
					}
				}

				ItemStack[] lstacks = liquidContainerStacks.ToArray();
				return new WorldInteraction[] {
					new WorldInteraction()
					{
					ActionLangCode = "blockhelp-bucket-rightclick",
					MouseButton = EnumMouseButton.Right,
					Itemstacks = liquidContainerStacks.ToArray()
					}
				};
			});
		}

		public override void OnHeldInteractStart(ItemSlot itemslot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling)
		{
			base.OnHeldInteractStart(itemslot, byEntity, blockSel, entitySel, firstEvent, ref handHandling);
		}

		public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
		{

			BlockEntitySingleSink besink = null;
			if (blockSel.Position != null)
			{
				besink = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BlockEntitySingleSink;
			}


			bool handled = base.OnBlockInteractStart(world, byPlayer, blockSel);

			if (!handled && !byPlayer.WorldData.EntityControls.Sneak && blockSel.Position != null)
			{
				if (besink != null)
				{
					besink.OnBlockInteract(byPlayer);
				}

				return true;
			}

			return handled;
		}

		public override void TryFillFromBlock(EntityItem byEntityItem, BlockPos pos)
		{
			// Don't fill when dropped as item in water
		}


	}
}