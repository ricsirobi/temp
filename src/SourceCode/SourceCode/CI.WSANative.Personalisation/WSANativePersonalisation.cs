using System;
using UnityEngine;

namespace CI.WSANative.Personalisation;

public static class WSANativePersonalisation
{
	public static Func<Color> _GetAccentColour;

	public static Color GetSystemAccentColour()
	{
		return Color.white;
	}
}
