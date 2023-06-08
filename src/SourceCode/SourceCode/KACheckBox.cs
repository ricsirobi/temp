public class KACheckBox : KAToggleButton
{
	public UISprite _CheckedSprite;

	public override void OnChecked()
	{
		base.OnChecked();
		if (_CheckedSprite != null)
		{
			_CheckedSprite.gameObject.SetActive(mChecked);
		}
	}
}
