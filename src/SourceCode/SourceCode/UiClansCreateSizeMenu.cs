public class UiClansCreateSizeMenu : KAUIMenu
{
	public override void GoToPage(int inPageNumber, bool instant = false)
	{
		base.GoToPage(inPageNumber, instant);
		UiClansCreate uiClansCreate = (UiClansCreate)_ParentUi;
		if ((bool)uiClansCreate)
		{
			uiClansCreate.UpdateGemsForType(UiClansCreate.ClanTicketType.ClanSize);
		}
	}

	public int GetClanSize()
	{
		int result = -1;
		KAWidget itemAt = GetItemAt(GetTopItemIdx());
		if (itemAt == null)
		{
			itemAt = GetItemAt(0);
		}
		int.TryParse(itemAt.GetText(), out result);
		return result;
	}
}
