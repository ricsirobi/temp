public class UiMOBASelectDragonMenu : KAUIMenu
{
	private UiMOBASelectDragon mParentUi;

	protected override void Start()
	{
		base.Start();
		mParentUi = (UiMOBASelectDragon)_ParentUi;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		mParentUi.DoDragonAction(inWidget);
	}
}
