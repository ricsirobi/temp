using System;

namespace CI.WSANative.Web;

public static class WSANativeWebView
{
	public static Action<Uri> NavigationStarting { get; set; }

	public static Action<Uri, Uri> UnviewableContentIdentified { get; set; }

	public static Action<Uri> DOMContentLoaded { get; set; }

	public static void Create(int x, int y, int width, int height, Uri uri)
	{
	}

	public static void Destroy()
	{
	}

	public static void Navigate(Uri uri)
	{
	}
}
