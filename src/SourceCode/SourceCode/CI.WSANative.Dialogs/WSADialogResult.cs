namespace CI.WSANative.Dialogs;

public class WSADialogResult
{
	public string ButtonPressed { get; private set; }

	public WSADialogResult(string buttonPressed)
	{
		ButtonPressed = buttonPressed;
	}
}
