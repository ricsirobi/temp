public class KAUIFuseItemData : KAUISelectItemData
{
	public int BluePrintSpecID;

	public KAUIFuseItemData(KAUISelectMenu menu, int SpecID, ItemData item, int wh, int quantity, InventoryTabType tabType = InventoryTabType.ICON)
		: base(menu, item, wh, quantity, tabType)
	{
		BluePrintSpecID = SpecID;
	}
}
