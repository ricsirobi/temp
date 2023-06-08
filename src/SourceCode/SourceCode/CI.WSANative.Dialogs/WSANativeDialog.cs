using System;
using System.Collections.Generic;

namespace CI.WSANative.Dialogs;

public static class WSANativeDialog
{
	public static void ShowDialog(string title, string message)
	{
	}

	public static void ShowDialogWithOptions(string title, string message, Action<WSADialogResult> response)
	{
	}

	public static void ShowDialogWithOptions(string title, string message, List<WSADialogCommand> commands, int defaultCommandIndex, int cancelCommandIndex, Action<WSADialogResult> response)
	{
	}
}
