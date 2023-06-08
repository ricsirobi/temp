public class UiProfileDragonTrainerTitleMenu : KAUIDropDownMenu
{
	protected override void Start()
	{
		_OnlyUpdateWhenBecomingVisible = true;
		base.Start();
	}

	public override void UpdateState(bool isDropped)
	{
		base.UpdateState(isDropped);
		if (isDropped)
		{
			base.pMenuGrid.repositionNow = true;
		}
	}

	public void Init(UiProfileDragonTrainerInfo inParent)
	{
		ClearItems();
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(inParent._TrainerTitleCategoryID);
		if (items != null && items.Length != 0)
		{
			SetVisibility(inVisible: true);
			UserItemData[] array = items;
			foreach (UserItemData userItemData in array)
			{
				KAWidget kAWidget = AddWidget(userItemData.Item.ItemName, null);
				kAWidget.SetText(userItemData.Item.ItemName);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		UpdateState(isDropped: false);
	}
}
