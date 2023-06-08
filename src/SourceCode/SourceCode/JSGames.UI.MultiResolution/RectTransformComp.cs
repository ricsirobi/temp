using System;
using UnityEngine;

namespace JSGames.UI.MultiResolution;

[Serializable]
public class RectTransformComp : UIComponent
{
	public const string RectTransformCompName = "RectTransform";

	public Vector3 AnchoredPosition;

	public Vector2 AnchorMin;

	public Vector2 AnchorMax;

	public Vector2 SizeDelta;

	public Vector3 Pivot;

	public Quaternion Rotation;

	public Vector3 Scale;

	public RectTransformComp(Component comp)
		: base(comp)
	{
	}

	public bool Equals(RectTransformComp rectTransformComp)
	{
		if (rectTransformComp.AnchoredPosition != AnchoredPosition)
		{
			return false;
		}
		if (rectTransformComp.AnchorMax != AnchorMax)
		{
			return false;
		}
		if (rectTransformComp.AnchorMin != AnchorMin)
		{
			return false;
		}
		if (rectTransformComp.SizeDelta != SizeDelta)
		{
			return false;
		}
		if (rectTransformComp.Pivot != Pivot)
		{
			return false;
		}
		if (rectTransformComp.Pivot != Pivot)
		{
			return false;
		}
		if (rectTransformComp.Rotation != Rotation)
		{
			return false;
		}
		if (rectTransformComp.Scale != Scale)
		{
			return false;
		}
		return true;
	}

	public override void ReadComponentData(Component Comp)
	{
		RectTransform component = Comp.GetComponent<RectTransform>();
		AnchoredPosition = component.anchoredPosition;
		AnchorMax = component.anchorMax;
		AnchorMin = component.anchorMin;
		SizeDelta = component.sizeDelta;
		Pivot = component.pivot;
		Rotation = component.rotation;
		Scale = component.localScale;
	}
}
