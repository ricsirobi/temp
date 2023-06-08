using System;
using UnityEngine;
using UnityEngine.UI;

namespace StatsMonitor;

internal sealed class SMGraphicsUtil
{
	internal GameObject parent;

	internal Color defaultColor;

	internal Font defaultFontFace;

	internal int defaultFontSize;

	internal static Vector2 defaultEffectDistance = new Vector2(1f, -1f);

	internal SMGraphicsUtil(GameObject parent, Color defaultColor, Font defaultFontFace = null, int defaultFontSize = 16)
	{
		this.parent = parent;
		this.defaultFontFace = defaultFontFace;
		this.defaultFontSize = defaultFontSize;
		this.defaultColor = defaultColor;
	}

	internal Graphic Graphic(string name, Type type, float x = 0f, float y = 0f, float w = 0f, float h = 0f, Color? color = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(StatsMonitor.Widget.TemplateViewPrefab, parent.transform, worldPositionStays: false);
		gameObject.AddComponent(type);
		gameObject.name = name;
		Graphic component = gameObject.GetComponent<Graphic>();
		component.color = color ?? defaultColor;
		RectTransform component2 = gameObject.GetComponent<RectTransform>();
		component2.pivot = Vector2.up;
		component2.anchorMin = Vector2.up;
		component2.anchorMax = Vector2.up;
		component2.anchoredPosition = new Vector2(x, y);
		if (w > 0f && h > 0f)
		{
			component2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
			component2.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
		}
		return component;
	}

	internal Image Image(string name, float x = 0f, float y = 0f, float w = 0f, float h = 0f, Color? color = null)
	{
		return (Image)Graphic(name, typeof(Image), x, y, w, h, color);
	}

	internal RawImage RawImage(string name, float x = 0f, float y = 0f, float w = 0f, float h = 0f, Color? color = null)
	{
		return (RawImage)Graphic(name, typeof(RawImage), x, y, w, h, color);
	}

	internal Text Text(string name, float x = 0f, float y = 0f, float w = 0f, float h = 0f, string text = "", Color? color = null, int fontSize = 0, Font fontFace = null, bool fitH = false, bool fitV = false)
	{
		Text text2 = (Text)Graphic(name, typeof(Text), x, y, w, h, color);
		text2.font = fontFace ?? defaultFontFace;
		text2.fontSize = ((fontSize < 1) ? defaultFontSize : fontSize);
		if (fitH)
		{
			text2.horizontalOverflow = HorizontalWrapMode.Overflow;
		}
		if (fitV)
		{
			text2.verticalOverflow = VerticalWrapMode.Overflow;
		}
		text2.text = text;
		if (fitH || fitV)
		{
			FitText(text2, fitH, fitV);
		}
		return text2;
	}

	internal Text Text(string name, string text = "", Color? color = null, int fontSize = 0, Font fontFace = null, bool fitH = true, bool fitV = true)
	{
		Text text2 = (Text)Graphic(name, typeof(Text), 0f, 0f, 0f, 0f, color);
		text2.font = fontFace ?? defaultFontFace;
		text2.fontSize = ((fontSize < 1) ? defaultFontSize : fontSize);
		if (fitH)
		{
			text2.horizontalOverflow = HorizontalWrapMode.Overflow;
		}
		if (fitV)
		{
			text2.verticalOverflow = VerticalWrapMode.Overflow;
		}
		text2.text = text;
		if (fitH || fitV)
		{
			FitText(text2, fitH, fitV);
		}
		return text2;
	}

	internal static void FitText(Text text, bool h, bool v)
	{
		ContentSizeFitter contentSizeFitter = text.gameObject.GetComponent<ContentSizeFitter>();
		if (contentSizeFitter == null)
		{
			contentSizeFitter = text.gameObject.AddComponent<ContentSizeFitter>();
		}
		if (h)
		{
			contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		}
		if (v)
		{
			contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		}
	}

	internal static Outline AddOutline(GameObject obj, Color color, Vector2? distance = null)
	{
		Outline outline = obj.GetComponent<Outline>();
		if (outline == null)
		{
			outline = obj.AddComponent<Outline>();
		}
		outline.effectColor = color;
		outline.effectDistance = distance ?? defaultEffectDistance;
		return outline;
	}

	internal static Shadow AddShadow(GameObject obj, Color color, Vector2? distance = null)
	{
		Shadow shadow = obj.GetComponent<Shadow>();
		if (shadow == null)
		{
			shadow = obj.AddComponent<Shadow>();
		}
		shadow.effectColor = color;
		shadow.effectDistance = distance ?? defaultEffectDistance;
		return shadow;
	}

	internal static void AddOutlineAndShadow(GameObject obj, Color color, Vector2? distance = null)
	{
		Shadow shadow = obj.GetComponent<Shadow>();
		if (shadow == null)
		{
			shadow = obj.AddComponent<Shadow>();
		}
		shadow.effectColor = color;
		shadow.effectDistance = distance ?? defaultEffectDistance;
		Outline outline = obj.GetComponent<Outline>();
		if (outline == null)
		{
			outline = obj.AddComponent<Outline>();
		}
		outline.effectColor = color;
		outline.effectDistance = distance ?? defaultEffectDistance;
	}

	internal static void RemoveEffects(GameObject obj)
	{
		Shadow component = obj.GetComponent<Shadow>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component);
		}
		Outline component2 = obj.GetComponent<Outline>();
		if (component2 != null)
		{
			UnityEngine.Object.Destroy(component2);
		}
	}
}
