using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace QptechFurniture.src
{
	public class BlockIceBox : BlockGenericTypedContainer
	{
		public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
		{
			return new WorldInteraction[]
			{
				new WorldInteraction()
				{
					ActionLangCode = "heldhelp-place",
					HotKeyCode = "sneak",
					MouseButton = EnumMouseButton.Right,
					ShouldApply = (wi, bs, es) => {
						return true;
					}
				}
			}.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
		}
	}
}