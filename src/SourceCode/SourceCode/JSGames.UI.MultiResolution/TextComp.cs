using System;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI.MultiResolution;

[Serializable]
public class TextComp : UIComponent
{
	public const string TextCompName = "Text";

	public int FontSize;

	public float LineSpacing;

	public bool RichText;

	public bool BestFit;

	public int BestFitMin;

	public int BestFitMax;

	public bool AlignByGeometry;

	public HorizontalWrapMode HorizontalWrapMode;

	public VerticalWrapMode VerticalWrapMode;

	public TextAnchor Alignment;

	public string TextJson;

	public TextComp(Component comp)
		: base(comp)
	{
	}

	public bool Equals(TextComp textComp)
	{
		if (textComp.FontSize != FontSize)
		{
			return false;
		}
		if (textComp.LineSpacing != LineSpacing)
		{
			return false;
		}
		if (textComp.RichText != RichText)
		{
			return false;
		}
		if (textComp.Alignment != Alignment)
		{
			return false;
		}
		if (textComp.BestFit != BestFit)
		{
			return false;
		}
		if (textComp.BestFitMin != BestFitMin)
		{
			return false;
		}
		if (textComp.BestFitMax != BestFitMax)
		{
			return false;
		}
		if (textComp.HorizontalWrapMode != HorizontalWrapMode)
		{
			return false;
		}
		if (textComp.VerticalWrapMode != VerticalWrapMode)
		{
			return false;
		}
		return true;
	}

	public override void ReadComponentData(Component Comp)
	{
		Text component = Comp.GetComponent<Text>();
		FontSize = component.fontSize;
		LineSpacing = component.lineSpacing;
		RichText = component.supportRichText;
		Alignment = component.alignment;
		BestFit = component.resizeTextForBestFit;
		BestFitMin = component.resizeTextMinSize;
		BestFitMax = component.resizeTextMaxSize;
		HorizontalWrapMode = component.horizontalOverflow;
		VerticalWrapMode = component.verticalOverflow;
	}
}
