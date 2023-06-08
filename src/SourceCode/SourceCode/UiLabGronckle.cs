public class UiLabGronckle : KAUI
{
	public KAUIMenu _Menu;

	public void AddBellyIcon(LabItem labItem)
	{
		if (_Menu != null && _Menu._Template != null)
		{
			KAWidget kAWidget = _Menu.AddWidget(_Menu._Template.name);
			CoBundleItemData coBundleItemData = new CoBundleItemData(labItem.Icon, "");
			coBundleItemData._Item = kAWidget;
			coBundleItemData.LoadResource();
			kAWidget.SetUserData(coBundleItemData);
			kAWidget.SetText("1");
			kAWidget.name = labItem.Name;
		}
	}

	public void UpdateBellyItemCount(string itemName, int count)
	{
		KAWidget kAWidget = _Menu.FindItem(itemName);
		if (kAWidget != null)
		{
			kAWidget.SetText(count.ToString());
		}
	}

	public void Clear()
	{
		_Menu.ClearItems();
	}
}
