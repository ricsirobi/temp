using System;
using UnityEngine;
using UnityEngine.UI;

namespace StatsMonitor;

internal class GraphView : SMView
{
	private RawImage _image;

	private SMBitmap _graph;

	private int _oldWidth;

	private int _width;

	private int _height;

	private int _graphStartX;

	private int _graphMaxY;

	private int _memCeiling;

	private int _lastGCCollectionCount = -1;

	private Color?[] _fpsColors;

	public GraphView(StatsMonitorWidget statsMonitorWidget)
	{
		_statsMonitorWidget = statsMonitorWidget;
		Invalidate();
	}

	internal override void Reset()
	{
		if (_graph != null)
		{
			_graph.Clear();
		}
	}

	internal override void Update()
	{
		if (_graph != null)
		{
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int)Mathf.Ceil(_statsMonitorWidget.memTotal / (float)_memCeiling * (float)_height)), _statsMonitorWidget.colorMemTotal);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, (int)Mathf.Ceil(_statsMonitorWidget.memAlloc / (float)_memCeiling * (float)_height)), _statsMonitorWidget.colorMemAlloc);
			int num = (int)Mathf.Ceil(_statsMonitorWidget.memMono / (float)_memCeiling * (float)_height);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, num), _statsMonitorWidget.colorMemMono);
			int num2 = (int)_statsMonitorWidget.ms >> 1;
			if (num2 == num)
			{
				num2++;
			}
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, num2), _statsMonitorWidget.colorMS);
			_graph.SetPixel(_graphStartX, Mathf.Min(_graphMaxY, _statsMonitorWidget.fps / ((_statsMonitorWidget.fpsMax > 60) ? _statsMonitorWidget.fpsMax : 60) * _graphMaxY - 1), _statsMonitorWidget.colorFPS);
			if (_lastGCCollectionCount != GC.CollectionCount(0))
			{
				_lastGCCollectionCount = GC.CollectionCount(0);
				_graph.FillColumn(_graphStartX, 0, 5, _statsMonitorWidget.colorGCBlip);
			}
			_graph.Scroll(-1, _fpsColors[_statsMonitorWidget.fpsLevel]);
			_graph.Apply();
		}
	}

	internal override void Dispose()
	{
		if (_graph != null)
		{
			_graph.Dispose();
		}
		_graph = null;
		SMView.Destroy(_image);
		_image = null;
		base.Dispose();
	}

	internal void SetWidth(float width)
	{
		_width = (int)width;
	}

	protected override GameObject CreateChildren()
	{
		_fpsColors = new Color?[3];
		GameObject gameObject = UnityEngine.Object.Instantiate(_statsMonitorWidget.TemplateViewPrefab, _statsMonitorWidget.transform, worldPositionStays: false);
		gameObject.name = "GraphView";
		_graph = new SMBitmap(10, 10, _statsMonitorWidget.colorGraphBG);
		_image = gameObject.AddComponent<RawImage>();
		_image.rectTransform.sizeDelta = new Vector2(10f, 10f);
		_image.color = Color.white;
		_image.texture = _graph.texture;
		int systemMemorySize = SystemInfo.systemMemorySize;
		if (systemMemorySize <= 1024)
		{
			_memCeiling = 512;
		}
		else if (systemMemorySize > 1024 && systemMemorySize <= 2048)
		{
			_memCeiling = 1024;
		}
		else
		{
			_memCeiling = 2048;
		}
		return gameObject;
	}

	protected override void UpdateStyle()
	{
		if (_graph != null)
		{
			_graph.color = _statsMonitorWidget.colorGraphBG;
		}
		if (_statsMonitorWidget.colorOutline.a > 0f)
		{
			SMGraphicsUtil.AddOutlineAndShadow(_image.gameObject, _statsMonitorWidget.colorOutline);
		}
		else
		{
			SMGraphicsUtil.RemoveEffects(_image.gameObject);
		}
		_fpsColors[0] = null;
		_fpsColors[1] = new Color(_statsMonitorWidget.colorFPSWarning.r, _statsMonitorWidget.colorFPSWarning.g, _statsMonitorWidget.colorFPSWarning.b, _statsMonitorWidget.colorFPSWarning.a / 4f);
		_fpsColors[2] = new Color(_statsMonitorWidget.ColorFPSCritical.r, _statsMonitorWidget.ColorFPSCritical.g, _statsMonitorWidget.ColorFPSCritical.b, _statsMonitorWidget.ColorFPSCritical.a / 4f);
	}

	protected override void UpdateLayout()
	{
		if (_width > 0 && _statsMonitorWidget.graphHeight > 0 && (_statsMonitorWidget.graphHeight != _height || _oldWidth != _width))
		{
			_oldWidth = _width;
			_height = _statsMonitorWidget.graphHeight;
			_height = ((_height % 2 == 0) ? _height : (_height + 1));
			_graphStartX = _width - 1;
			_graphMaxY = _height - 1;
			_image.rectTransform.sizeDelta = new Vector2(_width, _height);
			_graph.Resize(_width, _height);
			_graph.Clear();
			SetRTransformValues(0f, 0f, _width, _height, Vector2.one);
		}
	}
}
