using System;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI.MultiResolution;

[Serializable]
public class CanvasScalerComp : UIComponent
{
	public const string CanvasScalerCompName = "CanvasScaler";

	public Vector2 ScalerResolution;

	public CanvasScaler.ScaleMode UIScaleMode;

	public CanvasScaler.ScreenMatchMode ScreenMatchMode;

	public CanvasScalerComp(Component comp)
		: base(comp)
	{
	}

	public bool Equals(CanvasScalerComp canvasScalerComp)
	{
		if (canvasScalerComp.ScalerResolution != ScalerResolution)
		{
			return false;
		}
		if (canvasScalerComp.UIScaleMode != UIScaleMode)
		{
			return false;
		}
		if (canvasScalerComp.ScreenMatchMode != ScreenMatchMode)
		{
			return false;
		}
		return true;
	}

	public override void ReadComponentData(Component Comp)
	{
		CanvasScaler component = Comp.GetComponent<CanvasScaler>();
		ScalerResolution = component.referenceResolution;
		UIScaleMode = component.uiScaleMode;
		ScreenMatchMode = component.screenMatchMode;
	}
}
