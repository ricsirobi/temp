public class UiProfileGiftsMenu : KAUIMenu
{
	public override void LoadItem(KAWidget item)
	{
		base.LoadItem(item);
		((UiMessageInfoUserData)item.GetUserData())?.Load();
	}
}
