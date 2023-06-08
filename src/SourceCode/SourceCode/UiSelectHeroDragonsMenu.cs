public class UiSelectHeroDragonsMenu : KAUIMenu
{
	private UiSelectHeroDragons mParentUi;

	protected override void Start()
	{
		base.Start();
		mParentUi = (UiSelectHeroDragons)_ParentUi;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		mParentUi.DoDragonAction(inWidget);
	}
}
