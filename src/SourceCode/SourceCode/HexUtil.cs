using System;
using System.Globalization;
using UnityEngine;

public static class HexUtil
{
	public static int HexToInt(string value)
	{
		try
		{
			return int.Parse(value, NumberStyles.HexNumber);
		}
		catch (Exception)
		{
			return 0;
		}
	}

	public static string IntToHex(int value)
	{
		string text = value.ToString("X");
		if (text.Length == 1)
		{
			text = "0" + text;
		}
		return text;
	}

	public static string ColorStringToHex(string value)
	{
		string[] array = value.Split(',');
		string result = "";
		if (array.Length == 3)
		{
			result = IntToHex(int.Parse(array[0].Trim())) + IntToHex(int.Parse(array[1].Trim())) + IntToHex(int.Parse(array[2].Trim())) + "FF";
		}
		else if (array.Length == 4)
		{
			result = IntToHex(int.Parse(array[0].Trim())) + IntToHex(int.Parse(array[1].Trim())) + IntToHex(int.Parse(array[2].Trim())) + IntToHex(int.Parse(array[3].Trim()));
		}
		return result;
	}

	public static string FloatToHex(float value)
	{
		return IntToHex(int.Parse(Mathf.Round(value).ToString()));
	}

	public static bool HexToColor(string value, out Color color)
	{
		if (value.Length != 8)
		{
			color = Color.white;
			return false;
		}
		string value2 = value.Substring(0, 2);
		string value3 = value.Substring(2, 2);
		string value4 = value.Substring(4, 2);
		string value5 = value.Substring(6, 2);
		float num = (float)HexToInt(value2) / 255f;
		float num2 = (float)HexToInt(value3) / 255f;
		float num3 = (float)HexToInt(value4) / 255f;
		float num4 = (float)HexToInt(value5) / 255f;
		if (num < 0f || num2 < 0f || num3 < 0f || num4 < 0f)
		{
			color = Color.white;
			return false;
		}
		color = new Color(num, num2, num3, num4);
		return true;
	}

	public static Color HexToRGB(string value)
	{
		if (value.Length < 6)
		{
			return Color.white;
		}
		float num = (float)HexToInt(value.Substring(0, 2)) / 255f;
		float num2 = (float)HexToInt(value.Substring(2, 2)) / 255f;
		float num3 = (float)HexToInt(value.Substring(4, 2)) / 255f;
		if (num < 0f || num2 < 0f || num3 < 0f)
		{
			return Color.white;
		}
		return new Color(num, num2, num3);
	}

	public static string ColorToHex(Color color)
	{
		return FloatToHex(color.r * 255f) + FloatToHex(color.g * 255f) + FloatToHex(color.b * 255f) + FloatToHex(color.a * 255f);
	}

	public static string ColorToHexNoAlpha(Color color)
	{
		return FloatToHex(color.r * 255f) + FloatToHex(color.g * 255f) + FloatToHex(color.b * 255f);
	}
}
