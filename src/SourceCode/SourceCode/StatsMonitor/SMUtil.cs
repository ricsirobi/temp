using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace StatsMonitor;

internal sealed class SMUtil
{
	internal static string StripHTMLTags(string s)
	{
		return Regex.Replace(s, "<.*?>", string.Empty);
	}

	internal static string Color32ToHex(Color32 color)
	{
		return color.r.ToString("x2") + color.g.ToString("x2") + color.b.ToString("x2") + color.a.ToString("x2");
	}

	internal static Color HexToColor32(string hex)
	{
		if (hex.Length < 1)
		{
			return Color.black;
		}
		return new Color32(byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber), byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber));
	}

	public static float Normalize(float a, float min = 0f, float max = 255f)
	{
		if (max <= min)
		{
			return 1f;
		}
		return (Mathf.Clamp(a, min, max) - min) / (max - min);
	}

	internal static void ResetTransform(GameObject obj)
	{
		obj.transform.position = Vector3.zero;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.rotation = Quaternion.identity;
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localScale = Vector3.one;
	}

	internal static RectTransform RTransform(GameObject obj, Vector2 anchor, float x = 0f, float y = 0f, float w = 0f, float h = 0f)
	{
		RectTransform rectTransform = obj.GetComponent<RectTransform>();
		if (rectTransform == null)
		{
			rectTransform = obj.AddComponent<RectTransform>();
		}
		RectTransform rectTransform2 = rectTransform;
		RectTransform rectTransform3 = rectTransform;
		Vector2 vector2 = (rectTransform.anchorMax = anchor);
		Vector2 pivot = (rectTransform3.anchorMin = vector2);
		rectTransform2.pivot = pivot;
		rectTransform.anchoredPosition = new Vector2(x, y);
		if (w > 0f)
		{
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
		}
		if (h > 0f)
		{
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
		}
		return rectTransform;
	}

	internal static void AddToUILayer(GameObject obj)
	{
		int num = LayerMask.NameToLayer("UI");
		if (num > -1)
		{
			obj.layer = num;
		}
	}

	internal static float DPIScaleFactor(bool round = false)
	{
		float dpi = Screen.dpi;
		if (dpi <= 0f)
		{
			return -1f;
		}
		float num = dpi / 96f;
		if (num < 1f)
		{
			return 1f;
		}
		if (!round)
		{
			return num;
		}
		return Mathf.Round(num);
	}
}
