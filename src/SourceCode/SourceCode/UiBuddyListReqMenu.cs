public class UiBuddyListReqMenu : KAUIMenu
{
	public override void LoadItem(KAWidget inWidget)
	{
		base.LoadItem(inWidget);
		((BuddyListData)inWidget.GetUserData())?.Load();
	}
}
