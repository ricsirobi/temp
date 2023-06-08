using UnityEngine;

namespace CI.WSANative.Image;

public static class WSANativeImage
{
	public static Texture2D LoadImage(byte[] image)
	{
		Texture2D texture2D = new Texture2D(1, 1);
		texture2D.LoadImage(image);
		return texture2D;
	}

	public static Texture2D Crop(Texture2D image, int x, int y, int width, int height)
	{
		Color[] pixels = image.GetPixels(x, y, width, height, 0);
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
		texture2D.SetPixels(pixels, 0);
		return texture2D;
	}
}
