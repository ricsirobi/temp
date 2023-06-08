public class KADropButton : KAButton
{
	public KAUIDropDownMenu _UIDropDown;

	public override void OnClick()
	{
		base.OnClick();
		_UIDropDown.UpdateState(!_UIDropDown.GetVisibility());
	}
}
