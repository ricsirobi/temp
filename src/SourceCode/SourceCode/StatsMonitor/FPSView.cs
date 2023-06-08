using UnityEngine;
using UnityEngine.UI;

namespace StatsMonitor;

internal class FPSView : SMView
{
	private Text _text;

	private string[] _fpsTemplates;

	internal FPSView(StatsMonitorWidget statsMonitorWidget)
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
		_text.text = _fpsTemplates[_statsMonitorWidget.fpsLevel] + _statsMonitorWidget.fps + "FPS</color>";
	}

	internal override void Dispose()
	{
		SMView.Destroy(_text);
		_text = null;
		base.Dispose();
	}

	protected override GameObject CreateChildren()
	{
		_fpsTemplates = new string[3];
		GameObject obj = Object.Instantiate(_statsMonitorWidget.TemplateViewPrefab, _statsMonitorWidget.transform, worldPositionStays: false);
		obj.name = "FPSView";
		SMGraphicsUtil sMGraphicsUtil = new SMGraphicsUtil(obj, _statsMonitorWidget.colorFPS, _statsMonitorWidget.fontFace, _statsMonitorWidget.fontSizeSmall);
		_text = sMGraphicsUtil.Text("Text", "000FPS");
		_text.alignment = TextAnchor.MiddleCenter;
		return obj;
	}

	protected override void UpdateStyle()
	{
		_text.font = _statsMonitorWidget.fontFace;
		_text.fontSize = _statsMonitorWidget.FontSizeLarge;
		_text.color = _statsMonitorWidget.colorFPS;
		if (_statsMonitorWidget.colorOutline.a > 0f)
		{
			SMGraphicsUtil.AddOutlineAndShadow(_text.gameObject, _statsMonitorWidget.colorOutline);
		}
		else
		{
			SMGraphicsUtil.RemoveEffects(_text.gameObject);
		}
		_fpsTemplates[0] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPS) + ">";
		_fpsTemplates[1] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSWarning) + ">";
		_fpsTemplates[2] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSCritical) + ">";
	}

	protected override void UpdateLayout()
	{
		int padding = _statsMonitorWidget.padding;
		_text.rectTransform.anchoredPosition = new Vector2(padding, -padding);
		_text.rectTransform.anchoredPosition = Vector2.zero;
		RectTransform rectTransform = _text.rectTransform;
		RectTransform rectTransform2 = _text.rectTransform;
		Vector2 vector2 = (_text.rectTransform.pivot = new Vector2(0.5f, 0.5f));
		Vector2 anchorMin = (rectTransform2.anchorMax = vector2);
		rectTransform.anchorMin = anchorMin;
		int num = padding + (int)_text.preferredWidth + padding;
		int num2 = padding + (int)_text.preferredHeight + padding;
		num = ((num % 2 == 0) ? num : (num + 1));
		SetRTransformValues(0f, 0f, num, num2, Vector2.one);
	}
}
