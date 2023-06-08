using UnityEngine;
using UnityEngine.UI;

namespace StatsMonitor;

internal class StatsView : SMView
{
	private Text _text1;

	private Text _text2;

	private Text _text3;

	private Text _text4;

	private string[] _fpsTemplates;

	private string _fpsMinTemplate;

	private string _fpsMaxTemplate;

	private string _fpsAvgTemplate;

	private string _fxuTemplate;

	private string _msTemplate;

	private string _objTemplate;

	private string _memTotalTemplate;

	private string _memAllocTemplate;

	private string _memMonoTemplate;

	internal StatsView(StatsMonitorWidget statsMonitorWidget)
	{
		_statsMonitorWidget = statsMonitorWidget;
		Invalidate();
	}

	internal override void Reset()
	{
		Text text = _text1;
		Text text2 = _text2;
		Text text3 = _text3;
		string text5 = (_text4.text = "");
		string text7 = (text3.text = text5);
		string text9 = (text2.text = text7);
		text.text = text9;
	}

	internal override void Update()
	{
		_text1.text = _fpsTemplates[_statsMonitorWidget.fpsLevel] + _statsMonitorWidget.fps + "</color>";
		_text2.text = _fpsMinTemplate + ((_statsMonitorWidget.fpsMin > -1) ? _statsMonitorWidget.fpsMin : 0) + "</color>\n" + _fpsMaxTemplate + ((_statsMonitorWidget.fpsMax > -1) ? _statsMonitorWidget.fpsMax : 0) + "</color>";
		_text3.text = _fpsAvgTemplate + _statsMonitorWidget.fpsAvg + "</color> " + _msTemplate + _statsMonitorWidget.ms.ToString("F1") + "MS</color> " + _fxuTemplate + _statsMonitorWidget.fixedUpdateRate + " </color>\n" + _objTemplate + "OBJ:" + _statsMonitorWidget.renderedObjectCount + "/" + _statsMonitorWidget.renderObjectCount + "/" + _statsMonitorWidget.objectCount + "</color>";
		_text4.text = _memTotalTemplate + _statsMonitorWidget.memTotal.ToString("F1") + "MB</color> " + _memAllocTemplate + _statsMonitorWidget.memAlloc.ToString("F1") + "MB</color> " + _memMonoTemplate + _statsMonitorWidget.memMono.ToString("F1") + "MB</color>";
	}

	internal override void Dispose()
	{
		SMView.Destroy(_text1);
		SMView.Destroy(_text2);
		SMView.Destroy(_text3);
		SMView.Destroy(_text4);
		_text1 = (_text2 = (_text3 = (_text4 = null)));
		base.Dispose();
	}

	protected override GameObject CreateChildren()
	{
		_fpsTemplates = new string[3];
		GameObject obj = Object.Instantiate(_statsMonitorWidget.TemplateViewPrefab, _statsMonitorWidget.transform, worldPositionStays: false);
		obj.name = "StatsView";
		SMGraphicsUtil sMGraphicsUtil = new SMGraphicsUtil(obj, _statsMonitorWidget.colorFPS, _statsMonitorWidget.fontFace, _statsMonitorWidget.fontSizeSmall);
		_text1 = sMGraphicsUtil.Text("Text1", "FPS:000");
		_text2 = sMGraphicsUtil.Text("Text2", "MIN:000\nMAX:000");
		_text3 = sMGraphicsUtil.Text("Text3", "AVG:000\n[000.0 MS]");
		_text4 = sMGraphicsUtil.Text("Text4", "TOTAL:000.0MB ALLOC:000.0MB MONO:00.0MB");
		return obj;
	}

	protected override void UpdateStyle()
	{
		_text1.font = _statsMonitorWidget.fontFace;
		_text1.fontSize = _statsMonitorWidget.FontSizeLarge;
		_text2.font = _statsMonitorWidget.fontFace;
		_text2.fontSize = _statsMonitorWidget.FontSizeSmall;
		_text3.font = _statsMonitorWidget.fontFace;
		_text3.fontSize = _statsMonitorWidget.FontSizeSmall;
		_text4.font = _statsMonitorWidget.fontFace;
		_text4.fontSize = _statsMonitorWidget.FontSizeSmall;
		if (_statsMonitorWidget.colorOutline.a > 0f)
		{
			SMGraphicsUtil.AddOutlineAndShadow(_text1.gameObject, _statsMonitorWidget.colorOutline);
			SMGraphicsUtil.AddOutlineAndShadow(_text2.gameObject, _statsMonitorWidget.colorOutline);
			SMGraphicsUtil.AddOutlineAndShadow(_text3.gameObject, _statsMonitorWidget.colorOutline);
			SMGraphicsUtil.AddOutlineAndShadow(_text4.gameObject, _statsMonitorWidget.colorOutline);
		}
		else
		{
			SMGraphicsUtil.RemoveEffects(_text1.gameObject);
			SMGraphicsUtil.RemoveEffects(_text2.gameObject);
			SMGraphicsUtil.RemoveEffects(_text3.gameObject);
			SMGraphicsUtil.RemoveEffects(_text4.gameObject);
		}
		_fpsTemplates[0] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPS) + ">FPS:";
		_fpsTemplates[1] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSWarning) + ">FPS:";
		_fpsTemplates[2] = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSCritical) + ">FPS:";
		_fpsMinTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSMin) + ">MIN:";
		_fpsMaxTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSMax) + ">MAX:";
		_fpsAvgTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFPSAvg) + ">AVG:";
		_fxuTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorFXD) + ">FXD:";
		_msTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorMS) + ">";
		_objTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorObjCount) + ">";
		_memTotalTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorMemTotal) + ">TOTAL:";
		_memAllocTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorMemAlloc) + ">ALLOC:";
		_memMonoTemplate = "<color=#" + SMUtil.Color32ToHex(_statsMonitorWidget.colorMemMono) + ">MONO:";
	}

	protected override void UpdateLayout()
	{
		int padding = _statsMonitorWidget.padding;
		int spacing = _statsMonitorWidget.spacing;
		int num = _statsMonitorWidget.spacing / 4;
		_text1.text = PadString(_text1.text, 7, 1);
		_text2.text = PadString(_text2.text.Split('\n')[0], 7, 2);
		_text3.text = PadString(_text3.text.Split('\n')[0], 20, 2);
		_text4.text = PadString(_text4.text, 39, 1);
		_text1.rectTransform.anchoredPosition = new Vector2(padding, -padding);
		int num2 = padding + (int)_text1.preferredWidth + spacing;
		_text2.rectTransform.anchoredPosition = new Vector2(num2, -padding);
		num2 += (int)_text2.preferredWidth + spacing;
		_text3.rectTransform.anchoredPosition = new Vector2(num2, -padding);
		num2 = padding;
		int num3 = (int)_text2.preferredHeight * 2;
		int num4 = padding + (((int)_text1.preferredHeight >= num3) ? ((int)_text1.preferredHeight) : num3) + num;
		_text4.rectTransform.anchoredPosition = new Vector2(num2, -num4);
		num4 += (int)_text4.preferredHeight + padding;
		float num5 = (float)padding + _text1.preferredWidth + (float)spacing + _text2.preferredWidth + (float)spacing + _text3.preferredWidth + (float)padding;
		float num6 = (float)padding + _text4.preferredWidth + (float)padding;
		int num7 = ((num5 > num6) ? ((int)num5) : ((int)num6));
		num7 = ((num7 % 2 == 0) ? num7 : (num7 + 1));
		SetRTransformValues(0f, 0f, num7, num4, Vector2.one);
	}

	private static string PadString(string s, int minChars, int numRows)
	{
		s = SMUtil.StripHTMLTags(s);
		if (s.Length >= minChars)
		{
			return s;
		}
		int num = minChars - s.Length;
		for (int i = 0; i < num; i++)
		{
			s += "_";
		}
		return s;
	}
}
