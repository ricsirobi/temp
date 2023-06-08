public class UiClansCrestSelectorMenu : KAUIMenu
{
	private UiClansCrestSelector mParent;

	protected override void Start()
	{
		base.Start();
		mParent = (UiClansCrestSelector)_ParentUi;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mParent != null)
		{
			mParent.OnCrestSelected(inWidget);
		}
	}

	public override void LoadItem(KAWidget inWidget)
	{
		base.LoadItem(inWidget);
		((ClansCrestSelectorData)inWidget.GetUserData())?.Load();
	}
}
