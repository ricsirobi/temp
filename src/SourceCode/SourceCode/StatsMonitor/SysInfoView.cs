using UnityEngine;
using UnityEngine.UI;

namespace StatsMonitor;

internal class SysInfoView : SMView
{
	private int _width;

	private int _height;

	private Text _text;

	private bool _isDirty;

	internal SysInfoView(StatsMonitorWidget statsMonitorWidget)
	{
		_statsMonitorWidget = statsMonitorWidget;
		Invalidate();
	}

	internal override void Reset()
	{
		_text.text = "";
	}

	internal override void Update()
	{
		if (_isDirty)
		{
			string text = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoOdd) + ">OS:" + SystemInfo.operatingSystem + "</color>\n<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoEven) + ">CPU:" + SystemInfo.processorType + " [" + SystemInfo.processorCount + " cores]</color>\n<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoOdd) + ">GRAPHICS:" + SystemInfo.graphicsDeviceName + "\nAPI:" + SystemInfo.graphicsDeviceVersion + "\nShader Level:" + SystemInfo.graphicsShaderLevel + ", Video RAM:" + SystemInfo.graphicsMemorySize + " MB</color>\n<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoEven) + ">SYSTEM RAM:" + SystemInfo.systemMemorySize + " MB</color>\n<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorSysInfoOdd) + ">SCREEN:" + Screen.currentResolution.width + " x " + Screen.currentResolution.height + " @" + Screen.currentResolution.refreshRate + "Hz,\nwindow size:" + Screen.width + " x " + Screen.height + " " + Screen.dpi + "dpi</color>";
			_text.text = text;
			_height = _statsMonitorWidget.padding + (int)_text.preferredHeight + _statsMonitorWidget.padding;
			Invalidate(SMViewInvalidationType.Layout);
			_statsMonitorWidget.Invalidate(SMViewInvalidationType.Layout, StatsMonitorWidget.SMInvalidationFlag.Text, invalidateChildren: false);
			_isDirty = false;
		}
	}

	internal override void Dispose()
	{
		SMView.Destroy(_text);
		_text = null;
		base.Dispose();
	}

	internal void SetWidth(float width)
	{
		_width = (int)width;
	}

	protected override GameObject CreateChildren()
	{
		GameObject obj = Object.Instantiate(_statsMonitorWidget.TemplateViewPrefab, _statsMonitorWidget.transform, worldPositionStays: false);
		obj.name = "SysInfoView";
		SMGraphicsUtil sMGraphicsUtil = new SMGraphicsUtil(obj, _statsMonitorWidget.colorFPS, _statsMonitorWidget.fontFace, _statsMonitorWidget.fontSizeSmall);
		_text = sMGraphicsUtil.Text("Text", "", null, 0, null, fitH: false);
		return obj;
	}

	protected override void UpdateStyle()
	{
		_text.font = _statsMonitorWidget.fontFace;
		_text.fontSize = _statsMonitorWidget.FontSizeSmall;
		if (_statsMonitorWidget.colorOutline.a > 0f)
		{
			SMGraphicsUtil.AddOutlineAndShadow(_text.gameObject, _statsMonitorWidget.colorOutline);
		}
		else
		{
			SMGraphicsUtil.RemoveEffects(_text.gameObject);
		}
		_isDirty = true;
	}

	protected override void UpdateLayout()
	{
		int padding = _statsMonitorWidget.padding;
		_text.rectTransform.anchoredPosition = new Vector2(padding, -padding);
		_text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _width - padding * 2);
		_height = padding + (int)_text.preferredHeight + padding;
		SetRTransformValues(0f, 0f, _width, _height, Vector2.one);
		_isDirty = true;
	}
}
