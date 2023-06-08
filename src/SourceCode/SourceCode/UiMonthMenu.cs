public class UiMonthMenu : KAUIDropDownMenu
{
	public LocaleString _DefaultText = new LocaleString("Month");

	public int GetCurrentMonth()
	{
		return GetSelectedItemIndex() % 12 + 1;
	}
}
