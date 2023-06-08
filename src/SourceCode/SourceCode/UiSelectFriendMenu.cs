public class UiSelectFriendMenu : KAUIMenu
{
	public override void LoadItem(KAWidget inWidget)
	{
		base.LoadItem(inWidget);
		((UiSelectFriendItemData)inWidget.GetUserData())?.Load();
	}
}
