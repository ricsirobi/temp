public class UiClansDetailsMenu : KAUIMenu
{
	private UiClansDetails mUiClansDetails;

	protected override void Start()
	{
		base.Start();
		mUiClansDetails = (UiClansDetails)_ParentUi;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		mUiClansDetails.OnClanMemberSelected(inWidget);
	}
}
