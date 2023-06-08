using System;
using System.Collections;
using System.IO;
using UnityEngine;

[Serializable]
public class GrBitmap
{
	public Texture2D _Texture;

	public string _Name = "";

	public int _Width;

	public int _Height;

	public Color[] _Colors;

	private BlendOp mAlphaBlendMode = BlendOp.REPLACE;

	private BlendOp mColorBlendMode = BlendOp.REPLACE;

	private RepeatMode mRepeatMode;

	private Color mConstant = new Color(1f, 1f, 1f, 1f);

	public GrBitmap(string fileName, int w, int h, bool loadPix)
	{
		_Texture = new Texture2D(w, h);
		LoadFromExtFile(fileName, loadPix);
	}

	public GrBitmap(string bname, int w, int h, bool c, Color clearColor)
	{
		_Name = bname;
		_Width = w;
		_Height = h;
		_Colors = new Color[_Width * _Height];
		if (c)
		{
			Clear(clearColor);
		}
	}

	public GrBitmap(Texture2D t)
	{
		ChangeTexture(t, loadPix: true);
	}

	public GrBitmap(Texture2D t, bool c, Color clearColor)
	{
		ChangeTexture(t, loadPix: true);
		if (c)
		{
			Clear(clearColor);
			Apply();
		}
	}

	public void SetConstant(Color cc)
	{
		mConstant = cc;
	}

	public void Apply()
	{
		Apply(t: false);
	}

	public void Apply(bool t)
	{
		if (_Texture != null)
		{
			_Texture.SetPixels(_Colors);
			_Texture.Apply(t);
		}
	}

	public void SetBlendMode(BlendOp colorOp, BlendOp alphaOp, RepeatMode repeatMode)
	{
		mAlphaBlendMode = alphaOp;
		mColorBlendMode = colorOp;
		mRepeatMode = repeatMode;
	}

	public Color GetPixel(int x, int y)
	{
		return _Colors[x + y * _Width];
	}

	public void SetPixel(int x, int y, Color c)
	{
		_Colors[x + y * _Width] = c;
	}

	public void Clear(Color c)
	{
		ClearRect(c, new Rect(0f, 0f, _Width, _Height));
	}

	public void LoadPixels(GrBitmap bmp)
	{
		if (bmp._Texture != null)
		{
			_Colors = bmp._Texture.GetPixels();
		}
		else if (bmp._Colors != null)
		{
			int num = bmp._Colors.Length;
			_Colors = new Color[num];
			for (int i = 0; i < num; i++)
			{
				_Colors[i] = bmp._Colors[i];
			}
		}
	}

	public void SaveToExtFile(string fileName)
	{
		byte[] buffer = _Texture.EncodeToPNG();
		using BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(fileName));
		binaryWriter.Write(buffer);
		binaryWriter.Close();
	}

	public void LoadFromExtFile(string fileName, bool loadPix)
	{
		using BinaryReader binaryReader = new BinaryReader(File.OpenRead(fileName));
		byte[] data = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
		binaryReader.Close();
		if (_Texture.LoadImage(data))
		{
			ChangeTexture(_Texture, loadPix);
		}
		_Texture.name = fileName;
	}

	public Texture2D GetTexture()
	{
		return _Texture;
	}

	public void ChangeTexture(Texture2D t, bool loadPix)
	{
		if (t == null)
		{
			Debug.LogError("!!!!!!!GrBitmap texture should not be null !!!");
			return;
		}
		_Texture = t;
		_Name = t.name;
		_Width = t.width;
		_Height = t.height;
		_Colors = t.GetPixels();
		if (loadPix)
		{
			ReloadPixels();
		}
	}

	public void ChangeTextureCorrect(Texture2D t, bool loadPix)
	{
		if (t == null)
		{
			Debug.LogError("!!!!!!!GrBitmap texture should not be null !!!");
			return;
		}
		_Texture = t;
		_Name = t.name;
		_Width = t.width;
		_Height = t.height;
		if (loadPix)
		{
			ReloadPixels();
		}
	}

	public void ReloadPixels()
	{
		if ((bool)_Texture)
		{
			_Colors = _Texture.GetPixels();
		}
	}

	public void ClearRect(Color c, Rect r)
	{
		int num = (int)(r.x + r.y * (float)_Width);
		int num2 = 0;
		for (int i = 0; (float)i < r.height; i++)
		{
			num2 = num;
			for (int j = 0; (float)j < r.width; j++)
			{
				_Colors[num2] = c;
				num2++;
			}
			num += _Width;
		}
	}

	public void BlitTo(GrBitmap dst, int dx, int dy)
	{
		BlitTo(dst, new Rect(0f, 0f, _Width, _Height), new Rect(dx, dy, _Width, _Height));
	}

	public void BlitTo(GrBitmap dst, Rect srcRect, Rect dstRect)
	{
		int num = 0;
		int num2 = 0;
		int xs = (int)srcRect.x;
		int ys = (int)srcRect.y;
		int w = (int)srcRect.width;
		int h = (int)srcRect.height;
		int xd = (int)dstRect.x;
		int yd = (int)dstRect.y;
		int wd = (int)dstRect.width;
		int hd = (int)dstRect.height;
		if (w == 0)
		{
			w = _Width;
		}
		if (h == 0)
		{
			h = _Height;
		}
		if (wd == 0)
		{
			wd = w;
		}
		if (hd == 0)
		{
			hd = h;
		}
		if (w == wd && h == hd)
		{
			if (ClipRectsFast(ref xs, ref ys, ref w, ref h, _Width, _Height, ref xd, ref yd, dst._Width, dst._Height))
			{
				return;
			}
			hd = h;
			wd = w;
			int num3 = xs + ys * _Width;
			int num4 = xd + yd * dst._Width;
			int num5 = num3;
			int num6 = num4;
			for (num2 = 0; num2 < hd; num2++)
			{
				num5 = num3;
				num6 = num4;
				for (num = 0; num < wd; num++)
				{
					dst._Colors[num6] = BlendColors(_Colors[num5], dst._Colors[num6]);
					num5++;
					num6++;
				}
				num3 += _Width;
				num4 += dst._Width;
			}
		}
		else if (mRepeatMode == RepeatMode.STAMP)
		{
			if (ClipRectsStretch(ref xs, ref ys, ref w, ref h, _Width, _Height, ref xd, ref yd, ref wd, ref hd, dst._Width, dst._Height))
			{
				return;
			}
			float num7 = (float)w / (float)wd;
			float num8 = (float)h / (float)hd;
			int num9 = xd + yd * dst._Width;
			int num10 = num9;
			float num11 = ys;
			for (num2 = 0; num2 < hd; num2++)
			{
				float num12 = (int)num11;
				if (num12 - (float)ys >= (float)h)
				{
					num12 = ys + h - 1;
				}
				float num13 = (float)xs + num12 * (float)_Width;
				float num14 = num13;
				num10 = num9;
				for (num = 0; num < wd; num++)
				{
					if (num13 - num14 >= (float)w)
					{
						num13 = num14 + (float)w - 1f;
					}
					dst._Colors[num10] = BlendColors(_Colors[(int)num13], dst._Colors[num10]);
					num13 += num7;
					num10++;
				}
				num11 += num8;
				num9 += dst._Width;
			}
		}
		else
		{
			if ((mRepeatMode != RepeatMode.REPEAT && mRepeatMode != RepeatMode.ALIGNEDREPEAT) || ClipRectsRepeat(ref xs, ref ys, ref w, ref h, _Width, _Height, ref xd, ref yd, ref wd, ref hd, dst._Width, dst._Height))
			{
				return;
			}
			int num15 = 0;
			int num16 = 0;
			if (mRepeatMode == RepeatMode.ALIGNEDREPEAT)
			{
				num15 = yd % h;
			}
			int num17 = yd * wd + xd;
			int num18 = num17;
			for (num2 = 0; num2 < hd; num2++)
			{
				if (num15 >= h)
				{
					num15 -= h;
				}
				num18 = num17;
				num16 = ((mRepeatMode == RepeatMode.ALIGNEDREPEAT) ? (xd % w) : 0);
				for (num = 0; num < wd; num++)
				{
					if (num16 >= w)
					{
						num16 -= w;
					}
					dst._Colors[num18] = BlendColors(_Colors[num16 + num15 * w], dst._Colors[num18]);
					num18++;
					num16++;
				}
				num17 += dst._Width;
				num15++;
			}
		}
	}

	public void FlipUpDown()
	{
		FlipUpDown(new Rect(0f, 0f, 0f, 0f));
	}

	public void FlipUpDown(Rect fRect)
	{
		int num = 0;
		int num2 = 0;
		int num3 = (int)fRect.x;
		int num4 = (int)fRect.y;
		int num5 = (int)fRect.width;
		int num6 = (int)fRect.height;
		if (num5 == 0)
		{
			num5 = _Width;
		}
		if (num6 == 0)
		{
			num6 = _Height;
		}
		if (num3 + num5 > _Width || num4 + num6 > _Height)
		{
			Debug.LogError("Bitmap Flip out of range ");
			return;
		}
		Color[] array = new Color[_Width * _Height];
		int num7 = num3 + num4 * _Width;
		int num8 = num7 + (num6 - 1) * _Width;
		int num9 = num7;
		int num10 = num8;
		for (num2 = 0; num2 < num6; num2++)
		{
			num9 = num7;
			num10 = num8;
			for (num = 0; num < num5; num++)
			{
				array[num10] = _Colors[num9];
				num9++;
				num10++;
			}
			num7 += _Width;
			num8 -= _Width;
		}
		_Colors = array;
	}

	public Color BlendColors(Color src, Color dst)
	{
		Color result = new Color(src.r, src.g, src.b, src.a);
		switch (mAlphaBlendMode)
		{
		case BlendOp.NONE:
			result.a = dst.a;
			break;
		case BlendOp.REPLACE:
			result.a = src.a;
			break;
		case BlendOp.ADD:
			result.a = src.a + dst.a;
			if (result.a > 1f)
			{
				result.a = 1f;
			}
			break;
		case BlendOp.MULTIPLY:
			result.a = src.a * dst.a;
			break;
		case BlendOp.FROMCOLOR:
			result.a = (src.r + src.g + src.b) * 0.333333f;
			break;
		case BlendOp.FROMCOLORINVERSE:
			result.a = 1f - (src.r + src.g + src.b) * 0.333333f;
			break;
		case BlendOp.FROMCONSTANT:
			result.a = mConstant.a;
			break;
		case BlendOp.INVERSE:
			result.a = 1f - src.a;
			break;
		}
		switch (mColorBlendMode)
		{
		case BlendOp.NONE:
			result.r = dst.r;
			result.g = dst.g;
			result.b = dst.b;
			break;
		case BlendOp.REPLACE:
			result.r = src.r;
			result.g = src.g;
			result.b = src.b;
			break;
		case BlendOp.ADD:
			result.r = src.r + dst.r;
			if (result.r > 1f)
			{
				result.r = 1f;
			}
			result.g = src.g + dst.g;
			if (result.g > 1f)
			{
				result.g = 1f;
			}
			result.b = src.b + dst.b;
			if (result.b > 1f)
			{
				result.b = 1f;
			}
			break;
		case BlendOp.MULTIPLY:
			result.r = src.r * dst.r;
			result.g = src.g * dst.g;
			result.b = src.b * dst.b;
			break;
		case BlendOp.FROMALPHA:
			result.r = src.a;
			result.g = src.a;
			result.b = src.a;
			break;
		case BlendOp.USEALPHA:
			result.r = src.r * src.a + dst.r * (1f - src.a);
			result.g = src.g * src.a + dst.g * (1f - src.a);
			result.b = src.b * src.a + dst.b * (1f - src.a);
			break;
		case BlendOp.OVERLAY:
		{
			float num = (dst.r + dst.g + dst.b) * 0.33333f;
			if (num > 0.5f)
			{
				result.r = 1f - (1f - dst.r) * (1f - src.r);
				result.g = 1f - (1f - dst.g) * (1f - src.g);
				result.b = 1f - (1f - dst.b) * (1f - src.b);
				if (num < 0.66f)
				{
					float num2 = (num - 0.5f) / 0.16f;
					float num3 = 1f - num2;
					result.r = dst.r * num3 + result.r * num2;
					result.g = dst.g * num3 + result.g * num2;
					result.b = dst.b * num3 + result.b * num2;
				}
				break;
			}
			return dst;
		}
		case BlendOp.FROMCONSTANT:
			result.r = mConstant.r;
			result.g = mConstant.g;
			result.b = mConstant.b;
			break;
		case BlendOp.INVERSE:
			result.r = 1f - src.r;
			result.g = 1f - src.g;
			result.b = 1f - src.b;
			break;
		}
		return result;
	}

	private bool IsBorder(int[] mask, GrBitmap outline, int x, int y, int w, Rect boundingRect)
	{
		if (boundingRect.width > 0f && !boundingRect.Contains(new Vector2(x, y)))
		{
			return true;
		}
		if (mask[x + y * w] > 0)
		{
			return true;
		}
		if (outline.GetPixel(x, y).r < 0.01f)
		{
			return true;
		}
		return false;
	}

	public void FloodFill(GrBitmap source, GrBitmap outline, int x, int y, Rect boundingRect)
	{
		bool flag = false;
		bool flag2 = false;
		Vector2 vector = new Vector2(x, y);
		int num = _Width * _Height;
		int[] array = new int[num];
		int num2 = 0;
		int num3 = 0;
		for (num2 = 0; num2 < num; num2++)
		{
			array[num2] = 0;
		}
		Stack stack = new Stack();
		stack.Push(vector);
		while (stack.Count > 0)
		{
			vector = (Vector2)stack.Pop();
			int num4 = (int)vector.x;
			int num5 = (int)vector.y;
			while (num5 >= 0 && !IsBorder(array, outline, num4, num5, _Width, boundingRect))
			{
				num5--;
			}
			num5++;
			flag = (flag2 = false);
			for (; num5 < _Height && !IsBorder(array, outline, num4, num5, _Width, boundingRect); num5++)
			{
				array[num4 + num5 * _Width] = 1;
				if (!flag && vector.x > 0f && !IsBorder(array, outline, num4 - 1, num5, _Width, boundingRect))
				{
					stack.Push(new Vector2(num4 - 1, num5));
					flag = true;
				}
				else if (flag && (vector.x > 0f || IsBorder(array, outline, num4 - 1, num5, _Width, boundingRect)))
				{
					flag = false;
				}
				if (!flag2 && vector.x < (float)(_Width - 1) && !IsBorder(array, outline, num4 + 1, num5, _Width, boundingRect))
				{
					stack.Push(new Vector2(num4 + 1, num5));
					flag2 = true;
				}
				else if (flag2 && (num4 < _Width - 1 || IsBorder(array, outline, num4 + 1, num5, _Width, boundingRect)))
				{
					flag2 = false;
				}
			}
		}
		int width = source._Width;
		int height = source._Height;
		for (num2 = 0; num2 < _Height; num2++)
		{
			for (num3 = 0; num3 < _Width; num3++)
			{
				if (array[num2 * _Width + num3] == 1)
				{
					SetPixel(num3, num2, source.GetPixel(num3 % width, num2 % height));
				}
			}
		}
		Apply();
	}

	private bool ClipRectsFast(ref int xs, ref int ys, ref int w, ref int h, int wsrc, int hsrc, ref int xd, ref int yd, int wdes, int hdes)
	{
		float num = xd;
		float num2 = yd;
		float num3 = xs;
		float num4 = ys;
		float num5 = w;
		float num6 = h;
		if (num3 > (float)wsrc || num4 > (float)hsrc)
		{
			return true;
		}
		if (num3 + num5 < 0f || num4 + num6 < 0f)
		{
			return true;
		}
		if (num > (float)wdes || num2 > (float)hdes)
		{
			return true;
		}
		if (num + num5 < 0f || num2 + num6 < 0f)
		{
			return true;
		}
		if (num3 < 0f)
		{
			num -= num3;
			num5 += num3;
			num3 = 0f;
		}
		if (num4 < 0f)
		{
			num2 -= num4;
			num6 += num4;
			num4 = 0f;
		}
		if (num3 + num5 > (float)wsrc)
		{
			num5 -= num3 + num5 - (float)wsrc;
		}
		if (num4 + num6 > (float)hsrc)
		{
			num6 -= num4 + num6 - (float)hsrc;
		}
		if (num + num5 > (float)wdes)
		{
			num5 -= num + num5 - (float)wdes;
		}
		if (num2 + num6 > (float)hdes)
		{
			num6 -= num2 + num6 - (float)hdes;
		}
		if (num < 0f)
		{
			num3 -= num;
			num5 += num;
			num = 0f;
		}
		if (num2 < 0f)
		{
			num4 -= num2;
			num6 += num2;
			num2 = 0f;
		}
		xd = (int)num;
		yd = (int)num2;
		xs = (int)num3;
		ys = (int)num4;
		w = (int)num5;
		h = (int)num6;
		return false;
	}

	private bool ClipRectsStretch(ref int xs, ref int ys, ref int ws, ref int hs, int wsrc, int hsrc, ref int xd, ref int yd, ref int wd, ref int hd, int wdes, int hdes)
	{
		float num = xd;
		float num2 = yd;
		float num3 = xs;
		float num4 = ys;
		float num5 = ws;
		float num6 = hs;
		float num7 = wd;
		float num8 = hd;
		float num9 = num7 / num5;
		float num10 = num8 / num6;
		if (num3 >= (float)wsrc || num4 >= (float)hsrc)
		{
			return true;
		}
		if (num3 + num5 <= 0f || num4 + num6 <= 0f)
		{
			return true;
		}
		if (num >= (float)wdes || num2 >= (float)hdes)
		{
			return true;
		}
		if (num + num7 <= 0f || num2 + num8 <= 0f)
		{
			return true;
		}
		if (num3 < 0f)
		{
			num5 += num3;
			num -= num3 * num9;
			num7 += num3 * num9;
			num3 = 0f;
		}
		if (num4 < 0f)
		{
			num6 += num4;
			num2 -= num4 * num10;
			num8 += num4 * num10;
			num4 = 0f;
		}
		if (num3 + num5 > (float)wsrc)
		{
			float num11 = num3 + num5 - (float)wsrc;
			num5 -= num11;
			num7 -= num11 * num9;
		}
		if (num4 + num6 > (float)hsrc)
		{
			float num12 = num4 + num6 - (float)hsrc;
			num6 -= num12;
			num8 -= num12 * num10;
		}
		if (num + num7 > (float)wdes)
		{
			num5 -= (num + num7 - (float)wdes) / num9;
			num7 -= num + num7 - (float)wdes;
		}
		if (num2 + num8 > (float)hdes)
		{
			num6 -= (num2 + num8 - (float)hdes) / num10;
			num8 -= num2 + num8 - (float)hdes;
		}
		if (num < 0f)
		{
			num3 += (0f - num) / num9;
			num5 -= (0f - num) / num9;
			num7 -= 0f - num;
			num = 0f;
		}
		if (num2 < 0f)
		{
			num4 += (0f - num2) / num10;
			num6 -= (0f - num2) / num10;
			num8 -= 0f - num2;
			num2 = 0f;
		}
		xd = (int)num;
		yd = (int)num2;
		xs = (int)num3;
		ys = (int)num4;
		ws = (int)num5;
		hs = (int)num6;
		wd = (int)num7;
		hd = (int)num8;
		return false;
	}

	private bool ClipRectsRepeat(ref int xs, ref int ys, ref int ws, ref int hs, int wsrc, int hsrc, ref int xd, ref int yd, ref int wd, ref int hd, int wdes, int hdes)
	{
		float num = xd;
		float num2 = yd;
		float num3 = xs;
		float num4 = ys;
		float num5 = wd;
		float num6 = hd;
		float num7 = ws;
		float num8 = hs;
		if (num >= (float)wdes || num2 >= (float)hdes)
		{
			return true;
		}
		if (num + num5 <= 0f || num2 + num6 <= 0f)
		{
			return true;
		}
		if (num3 >= (float)wsrc || num4 >= (float)hsrc)
		{
			return true;
		}
		if (num3 + num7 <= 0f || num4 + num8 <= 0f)
		{
			return true;
		}
		if (num3 < 0f)
		{
			num3 = 0f;
		}
		if (num4 < 0f)
		{
			num4 = 0f;
		}
		if (num + num5 > (float)wdes)
		{
			num5 -= num + num5 - (float)wdes;
		}
		if (num2 + num6 > (float)hdes)
		{
			num6 -= num2 + num6 - (float)hdes;
		}
		if (num < 0f)
		{
			num5 += num;
			num = 0f;
		}
		if (num2 < 0f)
		{
			num6 += num2;
			num2 = 0f;
		}
		xd = (int)num;
		yd = (int)num2;
		xs = (int)num3;
		ys = (int)num4;
		ws = (int)num7;
		hs = (int)num8;
		wd = (int)num5;
		hd = (int)num6;
		return false;
	}
}
