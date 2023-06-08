using System;
using System.Collections.Generic;

namespace CI.WSANative.Pickers;

public static class WSANativeFilePicker
{
	public static void PickSingleFile(string commitButtonText, WSAPickerViewMode viewMode, WSAPickerLocationId suggestedStartLocation, string[] filters, Action<WSAStorageFile> response)
	{
	}

	public static void PickMultipleFiles(string commitButtonText, WSAPickerViewMode viewMode, WSAPickerLocationId suggestedStartLocation, string[] filters, Action<List<string>> response)
	{
	}

	public static void PickSaveFile(string commitButtonText, string defaultFileExtension, string suggestedFileName, WSAPickerLocationId suggestedStartLocation, IList<KeyValuePair<string, IList<string>>> validFileTypes, Action<WSAStorageFile> response)
	{
	}
}
