public class UiGSBuddySelectMenu : KAUIMenu
{
	public UiGSBuddySelect _UiGSBuddySelect;

	public override void OnClick(KAWidget item)
	{
		if ((KAToggleButton)item != null && _UiGSBuddySelect != null)
		{
			_UiGSBuddySelect.OnBuddyNameSelected(item);
		}
		base.OnClick(item);
	}
}
