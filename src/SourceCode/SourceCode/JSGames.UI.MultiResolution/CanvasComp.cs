using System;
using UnityEngine;

namespace JSGames.UI.MultiResolution;

[Serializable]
public class CanvasComp : UIComponent
{
	public const string CanvasCompName = "Canvas";

	public RenderMode RenderMode;

	public int SortOrder;

	public bool PixelPerfect;

	public bool OverridePixelPerfect;

	public CanvasComp(Component comp)
		: base(comp)
	{
	}

	public bool Equals(CanvasComp canvasComp)
	{
		if (canvasComp.RenderMode != RenderMode)
		{
			return false;
		}
		if (canvasComp.SortOrder != SortOrder)
		{
			return false;
		}
		if (canvasComp.PixelPerfect != PixelPerfect)
		{
			return false;
		}
		if (canvasComp.OverridePixelPerfect != OverridePixelPerfect)
		{
			return false;
		}
		return true;
	}

	public override void ReadComponentData(Component Comp)
	{
		Canvas component = Comp.GetComponent<Canvas>();
		RenderMode = component.renderMode;
		SortOrder = component.sortingOrder;
		PixelPerfect = component.pixelPerfect;
		OverridePixelPerfect = component.overridePixelPerfect;
	}
}
