using System;
using UnityEngine;

namespace CI.WSANative.Device;

public static class WSANativeDevice
{
	public static void EnableFlashlight(WSANativeColour colour = null)
	{
	}

	public static void DisableFlashlight()
	{
	}

	public static byte[] CaptureScreenshot()
	{
		Texture2D texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		texture2D.Apply();
		return texture2D.EncodeToPNG();
	}

	public static void Vibrate(int seconds)
	{
	}

	public static void CapturePicture(int imageWidth, int imageHeight, Action<byte[]> response)
	{
	}
}
