public class UiMessageThreadMenu : KAUIMenu
{
	public override void LoadItem(KAWidget item)
	{
		MessageUserData messageUserData = (MessageUserData)item.GetUserData();
		if (!messageUserData.pLoaded)
		{
			messageUserData.pLoaded = true;
		}
	}
}
