public class UiSelectProfileMenu : KAUIMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		_ParentUi.OnClick(inWidget);
	}

	protected override void OnGridReposition()
	{
		base.OnGridReposition();
		CenterSelectedItem();
	}

	protected override void UpdateVisibility(bool visible)
	{
		base.UpdateVisibility(visible);
		if (visible)
		{
			CenterSelectedItem();
		}
	}

	private void CenterSelectedItem()
	{
		if (mSelectedItem != null)
		{
			int selectedItemIndex = GetSelectedItemIndex();
			int numItemsPerPage = GetNumItemsPerPage();
			selectedItemIndex = ((selectedItemIndex >= numItemsPerPage) ? (selectedItemIndex - numItemsPerPage + 1) : 0);
			SetTopItemIdx(selectedItemIndex);
		}
	}
}
