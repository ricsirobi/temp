public class UiClansRequestDB : KAUIGenericDB
{
	public void SetDefaultMessage(LocaleString inMessageText)
	{
		KAEditBox kAEditBox = (KAEditBox)FindItem("TxtMessage");
		if (kAEditBox != null)
		{
			kAEditBox._DefaultText = inMessageText;
			kAEditBox.SetText(inMessageText.GetLocalizedString());
		}
	}

	public string GetMessage()
	{
		KAEditBox kAEditBox = (KAEditBox)FindItem("TxtMessage");
		if (kAEditBox != null)
		{
			return kAEditBox.GetText();
		}
		return "";
	}

	public void SetClanName(string inClanName)
	{
		KAWidget kAWidget = FindItem("TxtClanName");
		if (kAWidget != null)
		{
			kAWidget.SetText(inClanName);
		}
	}
}
