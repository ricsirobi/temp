public class DailyQuestRewardWidget : RewardWidget
{
	private ItemData mItemData;

	public ItemData pItemData => mItemData;

	protected override void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		base.OnLoadItemDataReady(itemID, dataItem, inUserData);
		if (dataItem != null)
		{
			mItemData = dataItem;
		}
	}
}
