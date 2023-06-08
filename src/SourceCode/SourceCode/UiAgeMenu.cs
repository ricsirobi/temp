public class UiAgeMenu : KAUIDropDownMenu
{
	public int _NumOfYears = 90;

	public LocaleString _DefaultText = new LocaleString("Year");

	protected override void Start()
	{
		_OnlyUpdateWhenBecomingVisible = true;
		base.Start();
	}

	public override void UpdateState(bool isDropped)
	{
		base.UpdateState(isDropped);
		if (isDropped)
		{
			base.pMenuGrid.repositionNow = true;
		}
	}

	public override void SetSelectedItem(KAWidget inWidget)
	{
		base.SetSelectedItem(inWidget);
	}
}
