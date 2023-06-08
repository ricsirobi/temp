using UnityEngine;

namespace StatsMonitor;

internal sealed class SMBitmap
{
	internal Texture2D texture;

	internal Color color;

	private readonly Rect _rect;

	internal SMBitmap(int width, int height, Color? color = null)
	{
		texture = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		texture.filterMode = FilterMode.Point;
		_rect = new Rect(0f, 0f, width, height);
		this.color = color ?? Color.black;
		Clear();
	}

	internal SMBitmap(float width, float height, Color? color = null)
	{
		texture = new Texture2D((int)width, (int)height, TextureFormat.ARGB32, mipChain: false);
		texture.filterMode = FilterMode.Point;
		this.color = color ?? Color.black;
		Clear();
	}

	internal void Resize(int width, int height)
	{
		texture.Reinitialize(width, height);
		texture.Apply();
	}

	internal void Clear(Color? color = null)
	{
		Color color2 = color ?? this.color;
		Color[] pixels = texture.GetPixels();
		int num = 0;
		while (num < pixels.Length)
		{
			pixels[num++] = color2;
		}
		texture.SetPixels(pixels);
		texture.Apply();
	}

	internal void FillRect(Rect? rect = null, Color? color = null)
	{
		Rect rect2 = rect ?? _rect;
		Color color2 = color ?? this.color;
		Color[] array = new Color[(int)(rect2.width * rect2.height)];
		int num = 0;
		while (num < array.Length)
		{
			array[num++] = color2;
		}
		texture.SetPixels((int)rect2.x, (int)rect2.y, (int)rect2.width, (int)rect2.height, array);
	}

	internal void FillRect(int x, int y, int w, int h, Color? color = null)
	{
		Color color2 = color ?? this.color;
		Color[] array = new Color[w * h];
		int num = 0;
		while (num < array.Length)
		{
			array[num++] = color2;
		}
		texture.SetPixels(x, y, w, h, array);
	}

	internal void FillColumn(int x, Color? color = null)
	{
		FillRect(new Rect(x, 0f, 1f, texture.height), color);
	}

	internal void FillColumn(int x, int y, int height, Color? color = null)
	{
		FillRect(new Rect(x, y, 1f, height), color);
	}

	internal void FillRow(int y, Color? color = null)
	{
		FillRect(new Rect(0f, y, texture.width, 1f), color);
	}

	internal void SetPixel(int x, int y, Color color)
	{
		texture.SetPixel(x, y, color);
	}

	internal void SetPixel(float x, float y, Color color)
	{
		texture.SetPixel((int)x, (int)y, color);
	}

	internal void Scroll(int x, Color? fillColor = null)
	{
		x = ~x + 1;
		texture.SetPixels(0, 0, texture.width - x, texture.height, texture.GetPixels(x, 0, texture.width - x, texture.height));
		FillRect(texture.width - x, 0, x, texture.height, fillColor);
	}

	internal void Apply()
	{
		texture.Apply();
	}

	internal void Dispose()
	{
		Object.Destroy(texture);
	}
}
