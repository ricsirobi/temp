public class KAUISelectTabMenu : KAUIMenu
{
	public InventoryTab pSelectedTab { get; set; }

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_ParentUi != null)
		{
			((KAUISelect)_ParentUi).ChangeCategory(inWidget);
		}
	}
}
