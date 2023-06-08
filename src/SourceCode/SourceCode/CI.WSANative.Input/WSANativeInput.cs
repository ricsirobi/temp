using System;

namespace CI.WSANative.Input;

public static class WSANativeInput
{
	public static Action<WSAPointerProperties> PointerPressed { get; set; }

	public static Action<WSAPointerProperties> PointerReleased { get; set; }
}
