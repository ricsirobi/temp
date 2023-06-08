namespace CI.WSANative.Dialogs;

public class WSADialogCommand
{
	public string ButtonName { get; private set; }

	public WSADialogCommand(string buttonName)
	{
		ButtonName = buttonName;
	}
}
