public class UiBuddyListMenu : KAUIMenu
{
	private void OnEnable()
	{
		if (UiBuddyList.pInstance != null)
		{
			UiBuddyList.pInstance.UpdateSelection();
		}
	}

	protected override void OnGridReposition()
	{
		base.OnGridReposition();
		if (UiBuddyList.pInstance != null)
		{
			UiBuddyList.pInstance.UpdateSelection();
		}
	}

	public override void LoadItem(KAWidget inWidget)
	{
		base.LoadItem(inWidget);
		((BuddyListData)inWidget.GetUserData())?.Load();
	}
}
