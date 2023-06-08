using System;
using UnityEngine;

[Serializable]
public class KAOrientationInfo
{
	public bool _ApplyEffect;

	public KAOrientationWidget _ApplyTo;

	private void ApplyUIWidgetData(UIWidget uiWidget, KATransformData transformData)
	{
		uiWidget.transform.localPosition = transformData._LocalPosition;
		uiWidget.pOrgPosition = transformData._LocalPosition;
		uiWidget.transform.localRotation = Quaternion.Euler(transformData._LocalRotation);
		uiWidget.transform.localScale = transformData._LocalScale;
		uiWidget.pOrgScale = transformData._LocalScale;
	}

	private void ApplyWidgetData(KAWidget widget, KATransformData transformData, UIAnchor.Side anchor)
	{
		Vector3 pOrgPosition = new Vector3(transformData._LocalPosition.x, transformData._LocalPosition.y, widget.pOrgPosition.z);
		widget.SetPosition(pOrgPosition.x, pOrgPosition.y);
		widget.pOrgPosition = pOrgPosition;
		widget.SetRotation(Quaternion.Euler(transformData._LocalRotation));
		pOrgPosition = transformData._LocalScale;
		Vector3 pScaleFactor = pOrgPosition - widget.pOrgScale;
		widget.pScaleFactor = pScaleFactor;
		widget.SetScale(pOrgPosition);
		widget.pOrgScale = pOrgPosition;
		widget.pAnchor.side = anchor;
		widget.pAnchor.enabled = true;
	}

	public void DoEffect(bool isPortrait)
	{
		if (!_ApplyEffect || !(_ApplyTo._Widget != null))
		{
			return;
		}
		if (_ApplyTo._UIWidgets.Length != 0)
		{
			KAOrientationWidgetData[] uIWidgets = _ApplyTo._UIWidgets;
			foreach (KAOrientationWidgetData kAOrientationWidgetData in uIWidgets)
			{
				if (isPortrait)
				{
					if (kAOrientationWidgetData._Portrait._SmallScreenData._Apply && KAUIManager.IsSmallScreen())
					{
						ApplyUIWidgetData(kAOrientationWidgetData._UIWidget, kAOrientationWidgetData._Portrait._SmallScreenData);
					}
					else if (kAOrientationWidgetData._Portrait._OrientationData._Apply)
					{
						ApplyUIWidgetData(kAOrientationWidgetData._UIWidget, kAOrientationWidgetData._Portrait._OrientationData);
					}
				}
				else if (kAOrientationWidgetData._Landscape._SmallScreenData._Apply && KAUIManager.IsSmallScreen())
				{
					ApplyUIWidgetData(kAOrientationWidgetData._UIWidget, kAOrientationWidgetData._Landscape._SmallScreenData);
				}
				else if (kAOrientationWidgetData._Landscape._OrientationData._Apply)
				{
					ApplyUIWidgetData(kAOrientationWidgetData._UIWidget, kAOrientationWidgetData._Landscape._OrientationData);
				}
			}
		}
		else if (isPortrait)
		{
			if (_ApplyTo._Portrait._SmallScreenData._Apply && KAUIManager.IsSmallScreen())
			{
				ApplyWidgetData(_ApplyTo._Widget, _ApplyTo._Portrait._SmallScreenData, _ApplyTo._Portrait._Anchor);
			}
			else if (_ApplyTo._Portrait._OrientationData._Apply)
			{
				ApplyWidgetData(_ApplyTo._Widget, _ApplyTo._Portrait._OrientationData, _ApplyTo._Portrait._Anchor);
			}
		}
		else if (_ApplyTo._Landscape._SmallScreenData._Apply && KAUIManager.IsSmallScreen())
		{
			ApplyWidgetData(_ApplyTo._Widget, _ApplyTo._Landscape._SmallScreenData, _ApplyTo._Landscape._Anchor);
		}
		else if (_ApplyTo._Landscape._OrientationData._Apply)
		{
			ApplyWidgetData(_ApplyTo._Widget, _ApplyTo._Landscape._OrientationData, _ApplyTo._Landscape._Anchor);
		}
	}
}
