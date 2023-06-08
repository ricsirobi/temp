using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace StatsMonitor;

[DisallowMultipleComponent]
public class StatsMonitorWidget : MonoBehaviour
{
	internal enum SMViewType : byte
	{
		Background,
		FPSView,
		StatsView,
		GraphView,
		SysInfoView
	}

	internal enum SMInvalidationFlag : byte
	{
		Any,
		Text,
		Graph,
		Background,
		Outline,
		Scale
	}

	public const string OBJECT_NAME = "Stats Monitor Widget";

	public const string TEMPLATE_VIEW_PREFAB_NAME = "StatsMonitorTemplateViewPrefab";

	private const float MEMORY_DIVIDER = 1048576f;

	private const int MINMAX_SKIP_INTERVALS = 3;

	[SerializeField]
	private GameObject _templateViewPrefab;

	[SerializeField]
	private SMMode _mode;

	[SerializeField]
	private SMRenderMode _renderMode;

	[SerializeField]
	private SMStyle _style = SMStyle.Standard;

	[SerializeField]
	private SMTheme _theme;

	[SerializeField]
	private SMAlignment _alignment = SMAlignment.UpperRight;

	[SerializeField]
	private bool _keepAlive = true;

	[SerializeField]
	[Range(0.1f, 10f)]
	private float _statsUpdateInterval = 0.5f;

	[SerializeField]
	[Range(0.01f, 10f)]
	private float _graphUpdateInterval = 0.05f;

	[SerializeField]
	[Range(0.01f, 10f)]
	private float _objectsCountInterval = 2f;

	public bool inputEnabled = true;

	public KeyCode modKeyToggle = KeyCode.LeftShift;

	public KeyCode hotKeyToggle = KeyCode.BackQuote;

	public KeyCode modKeyAlignment = KeyCode.LeftControl;

	public KeyCode hotKeyAlignment = KeyCode.BackQuote;

	public KeyCode modKeyStyle = KeyCode.LeftAlt;

	public KeyCode hotKeyStyle = KeyCode.BackQuote;

	[Range(0f, 5f)]
	public int toggleTouchCount = 3;

	[Range(0f, 5f)]
	public int switchAlignmentTapCount = 3;

	[Range(0f, 5f)]
	public int switchStyleTapCount = 3;

	[SerializeField]
	internal Font fontFace;

	[SerializeField]
	[Range(8f, 128f)]
	internal int fontSizeLarge = 32;

	[SerializeField]
	[Range(8f, 128f)]
	internal int fontSizeSmall = 16;

	[SerializeField]
	[Range(0f, 100f)]
	internal int padding = 4;

	[SerializeField]
	[Range(0f, 100f)]
	internal int spacing = 2;

	[SerializeField]
	[Range(10f, 400f)]
	internal int graphHeight = 40;

	[SerializeField]
	[Range(1f, 10f)]
	internal int scale = 1;

	[SerializeField]
	internal bool autoScale = true;

	[SerializeField]
	internal Color colorBGUpper = SMUtil.HexToColor32("00314ABE");

	[SerializeField]
	internal Color colorBGLower = SMUtil.HexToColor32("002525C8");

	[SerializeField]
	internal Color colorGraphBG = SMUtil.HexToColor32("00800010");

	[SerializeField]
	internal Color colorFPS = Color.white;

	[SerializeField]
	internal Color colorFPSWarning = SMUtil.HexToColor32("FFA000FF");

	[SerializeField]
	internal Color colorFPSCritical = SMUtil.HexToColor32("FF0000FF");

	[SerializeField]
	internal Color colorFPSMin = SMUtil.HexToColor32("999999FF");

	[SerializeField]
	internal Color colorFPSMax = SMUtil.HexToColor32("CCCCCCFF");

	[SerializeField]
	internal Color colorFPSAvg = SMUtil.HexToColor32("00C8DCFF");

	[SerializeField]
	internal Color colorFXD = SMUtil.HexToColor32("C68D00FF");

	[SerializeField]
	internal Color colorMS = SMUtil.HexToColor32("C8C820FF");

	[SerializeField]
	internal Color colorObjCount = SMUtil.HexToColor32("00B270FF");

	[SerializeField]
	internal Color colorGCBlip = SMUtil.HexToColor32("00FF00FF");

	[SerializeField]
	internal Color colorMemTotal = SMUtil.HexToColor32("4080FFFF");

	[SerializeField]
	internal Color colorMemAlloc = SMUtil.HexToColor32("B480FFFF");

	[SerializeField]
	internal Color colorMemMono = SMUtil.HexToColor32("FF66D1FF");

	[SerializeField]
	internal Color colorSysInfoOdd = SMUtil.HexToColor32("D2EBFFFF");

	[SerializeField]
	internal Color colorSysInfoEven = SMUtil.HexToColor32("A5D6FFFF");

	[SerializeField]
	internal Color colorOutline = SMUtil.HexToColor32("00000000");

	[SerializeField]
	private bool _throttleFrameRate;

	[SerializeField]
	[Range(-1f, 200f)]
	private int _throttledFrameRate = -1;

	[SerializeField]
	[Range(0f, 100f)]
	private int _avgSamples = 50;

	[SerializeField]
	[Range(1f, 200f)]
	private int _warningThreshold = 40;

	[SerializeField]
	[Range(1f, 200f)]
	private int _criticalThreshold = 20;

	internal StatsMonitor wrapper;

	internal int fpsLevel;

	private float _fpsNew;

	private int _minMaxIntervalsSkipped;

	private int _currentAVGSamples;

	private float _currentAVGRaw;

	private float[] _accAVGSamples;

	private int _cachedVSync = -1;

	private int _cachedFrameRate = -1;

	private float _actualUpdateInterval;

	private float _intervalTimeCount;

	private float _intervalTimeCount2;

	private int _totalWidth;

	private int _totalHeight;

	private bool _isInitialized;

	private bool _isDisposed;

	private SMAnchor _anchor;

	private RawImage _background;

	private Texture2D _gradient;

	private FPSView _fpsView;

	private StatsView _statsView;

	private GraphView _graphView;

	private SysInfoView _sysInfoView;

	public int fps { get; private set; }

	public int fpsMin { get; private set; }

	public int fpsMax { get; private set; }

	public int fpsAvg { get; private set; }

	public float ms { get; private set; }

	public float fixedUpdateRate { get; private set; }

	public float memAlloc { get; private set; }

	public float memTotal { get; private set; }

	public float memMono { get; private set; }

	public int objectCount { get; private set; }

	public int renderObjectCount { get; private set; }

	public int renderedObjectCount { get; private set; }

	public GameObject TemplateViewPrefab
	{
		get
		{
			return _templateViewPrefab;
		}
		set
		{
			_templateViewPrefab = value;
		}
	}

	public SMMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			if (_mode == value || !Application.isPlaying)
			{
				return;
			}
			_mode = value;
			if (base.enabled)
			{
				if (_mode != SMMode.Inactive)
				{
					OnEnable();
					UpdateData();
					UpdateView();
				}
				else
				{
					OnDisable();
				}
			}
		}
	}

	public SMRenderMode RenderMode
	{
		get
		{
			return _renderMode;
		}
		set
		{
			if (_renderMode != value && Application.isPlaying)
			{
				_renderMode = value;
				if (base.enabled)
				{
					wrapper.SetRenderMode(_renderMode);
				}
			}
		}
	}

	public SMStyle Style
	{
		get
		{
			return _style;
		}
		set
		{
			if (_style != value && Application.isPlaying)
			{
				_style = value;
				if (base.enabled)
				{
					CreateChildren();
					UpdateData();
					UpdateView();
				}
			}
		}
	}

	public SMTheme Theme
	{
		get
		{
			return _theme;
		}
		set
		{
			if (_theme != value)
			{
				_theme = value;
				switch (_theme)
				{
				case SMTheme.Default:
					colorBGUpper.a = SMUtil.Normalize(190f);
					colorBGLower.a = SMUtil.Normalize(200f);
					colorGraphBG.a = SMUtil.Normalize(16f);
					colorOutline.a = SMUtil.Normalize(0f);
					break;
				case SMTheme.Plain:
					colorBGUpper.a = SMUtil.Normalize(0f);
					colorBGLower.a = SMUtil.Normalize(0f);
					colorGraphBG.a = SMUtil.Normalize(0f);
					colorOutline.a = SMUtil.Normalize(0f);
					break;
				case SMTheme.Opaque:
					colorBGUpper.a = SMUtil.Normalize(255f);
					colorBGLower.a = SMUtil.Normalize(255f);
					colorGraphBG.a = SMUtil.Normalize(255f);
					colorOutline.a = SMUtil.Normalize(0f);
					break;
				case SMTheme.Oblique:
					colorBGUpper.a = SMUtil.Normalize(0f);
					colorBGLower.a = SMUtil.Normalize(0f);
					colorGraphBG.a = SMUtil.Normalize(0f);
					colorOutline.a = SMUtil.Normalize(190f);
					break;
				}
				if (!_isDisposed && Application.isPlaying && base.enabled)
				{
					Invalidate(SMViewInvalidationType.All);
				}
			}
		}
	}

	public SMAlignment Alignment
	{
		get
		{
			return _alignment;
		}
		set
		{
			if (_alignment != value && Application.isPlaying)
			{
				_alignment = value;
				if (base.enabled)
				{
					Align(_alignment);
				}
			}
		}
	}

	public bool KeepAlive
	{
		get
		{
			return _keepAlive;
		}
		set
		{
			_keepAlive = value;
		}
	}

	public float StatsUpdateInterval
	{
		get
		{
			return _statsUpdateInterval;
		}
		set
		{
			if (!(Mathf.Abs(_statsUpdateInterval - value) < 0.001f) && Application.isPlaying)
			{
				_statsUpdateInterval = value;
				if (base.enabled)
				{
					DetermineActualUpdateInterval();
					RestartCoroutine();
				}
			}
		}
	}

	public float GraphUpdateInterval
	{
		get
		{
			return _graphUpdateInterval;
		}
		set
		{
			if (!(Mathf.Abs(_graphUpdateInterval - value) < 0.001f) && Application.isPlaying)
			{
				_graphUpdateInterval = value;
				if (base.enabled)
				{
					DetermineActualUpdateInterval();
					RestartCoroutine();
				}
			}
		}
	}

	public float ObjectsCountInterval
	{
		get
		{
			return _objectsCountInterval;
		}
		set
		{
			if (!(Mathf.Abs(_objectsCountInterval - value) < 0.001f) && Application.isPlaying)
			{
				_objectsCountInterval = value;
				if (base.enabled)
				{
					DetermineActualUpdateInterval();
					RestartCoroutine();
				}
			}
		}
	}

	public Font FontFace
	{
		get
		{
			return fontFace;
		}
		set
		{
			if (!(fontFace == value) && Application.isPlaying)
			{
				fontFace = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.All, SMInvalidationFlag.Text);
				}
			}
		}
	}

	public int FontSizeSmall
	{
		get
		{
			return fontSizeSmall;
		}
		set
		{
			if (fontSizeSmall != value && Application.isPlaying)
			{
				fontSizeSmall = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.All, SMInvalidationFlag.Text);
				}
			}
		}
	}

	public int FontSizeLarge
	{
		get
		{
			return fontSizeLarge;
		}
		set
		{
			if (fontSizeLarge != value && Application.isPlaying)
			{
				fontSizeLarge = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.All, SMInvalidationFlag.Text);
				}
			}
		}
	}

	public int Padding
	{
		get
		{
			return padding;
		}
		set
		{
			if (padding != value && Application.isPlaying)
			{
				padding = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.All, SMInvalidationFlag.Text);
				}
			}
		}
	}

	public int Spacing
	{
		get
		{
			return spacing;
		}
		set
		{
			if (spacing != value && Application.isPlaying)
			{
				spacing = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.All, SMInvalidationFlag.Text);
				}
			}
		}
	}

	public int GraphHeight
	{
		get
		{
			return graphHeight;
		}
		set
		{
			if (graphHeight != value && Application.isPlaying)
			{
				graphHeight = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.All, SMInvalidationFlag.Graph);
				}
			}
		}
	}

	public int Scale
	{
		get
		{
			return scale;
		}
		set
		{
			if (scale != value && Application.isPlaying)
			{
				scale = value;
				if (base.enabled && _mode == SMMode.Active)
				{
					Invalidate(SMViewInvalidationType.All, SMInvalidationFlag.Scale);
				}
			}
		}
	}

	public bool AutoScale
	{
		get
		{
			return autoScale;
		}
		set
		{
			if (autoScale != value && Application.isPlaying)
			{
				autoScale = value;
				if (base.enabled && _mode == SMMode.Active)
				{
					Invalidate(SMViewInvalidationType.All, SMInvalidationFlag.Scale);
				}
			}
		}
	}

	public Color ColorBgUpper
	{
		get
		{
			return colorBGUpper;
		}
		set
		{
			if (!(colorBGUpper == value) && Application.isPlaying)
			{
				colorBGUpper = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style, SMInvalidationFlag.Background);
				}
			}
		}
	}

	public Color ColorBgLower
	{
		get
		{
			return colorBGLower;
		}
		set
		{
			if (!(colorBGLower == value) && Application.isPlaying)
			{
				colorBGLower = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style, SMInvalidationFlag.Background);
				}
			}
		}
	}

	public Color ColorGraphBG
	{
		get
		{
			return colorGraphBG;
		}
		set
		{
			if (!(colorGraphBG == value) && Application.isPlaying)
			{
				colorGraphBG = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorFPS
	{
		get
		{
			return colorFPS;
		}
		set
		{
			if (!(colorFPS == value) && Application.isPlaying)
			{
				colorFPS = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorFPSWarning
	{
		get
		{
			return colorFPSWarning;
		}
		set
		{
			if (!(colorFPSWarning == value) && Application.isPlaying)
			{
				colorFPSWarning = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorFPSCritical
	{
		get
		{
			return colorFPSCritical;
		}
		set
		{
			if (!(colorFPSCritical == value) && Application.isPlaying)
			{
				colorFPSCritical = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorFPSMin
	{
		get
		{
			return colorFPSMin;
		}
		set
		{
			if (!(colorFPSMin == value) && Application.isPlaying)
			{
				colorFPSMin = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorFPSMax
	{
		get
		{
			return colorFPSMax;
		}
		set
		{
			if (!(colorFPSMax == value) && Application.isPlaying)
			{
				colorFPSMax = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorFPSAvg
	{
		get
		{
			return colorFPSAvg;
		}
		set
		{
			if (!(colorFPSAvg == value) && Application.isPlaying)
			{
				colorFPSAvg = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorFxd
	{
		get
		{
			return colorFXD;
		}
		set
		{
			if (!(colorFXD == value) && Application.isPlaying)
			{
				colorFXD = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorMS
	{
		get
		{
			return colorMS;
		}
		set
		{
			if (!(colorMS == value) && Application.isPlaying)
			{
				colorMS = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorGCBlip
	{
		get
		{
			return colorGCBlip;
		}
		set
		{
			if (!(colorGCBlip == value) && Application.isPlaying)
			{
				colorGCBlip = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorObjectCount
	{
		get
		{
			return colorObjCount;
		}
		set
		{
			if (!(colorObjCount == value) && Application.isPlaying)
			{
				colorObjCount = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorMemTotal
	{
		get
		{
			return colorMemTotal;
		}
		set
		{
			if (!(colorMemTotal == value) && Application.isPlaying)
			{
				colorMemTotal = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorMemAlloc
	{
		get
		{
			return colorMemAlloc;
		}
		set
		{
			if (!(colorMemAlloc == value) && Application.isPlaying)
			{
				colorMemAlloc = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorMemMono
	{
		get
		{
			return colorMemMono;
		}
		set
		{
			if (!(colorMemMono == value) && Application.isPlaying)
			{
				colorMemMono = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorSysInfoOdd
	{
		get
		{
			return colorSysInfoOdd;
		}
		set
		{
			if (!(colorSysInfoOdd == value) && Application.isPlaying)
			{
				colorSysInfoOdd = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorSysInfoEven
	{
		get
		{
			return colorSysInfoEven;
		}
		set
		{
			if (!(colorSysInfoEven == value) && Application.isPlaying)
			{
				colorSysInfoEven = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.Style);
				}
			}
		}
	}

	public Color ColorOutline
	{
		get
		{
			return colorOutline;
		}
		set
		{
			if (!(colorOutline == value) && Application.isPlaying)
			{
				colorOutline = value;
				if (base.enabled)
				{
					Invalidate(SMViewInvalidationType.All);
				}
			}
		}
	}

	public bool ThrottleFrameRate
	{
		get
		{
			return _throttleFrameRate;
		}
		set
		{
			if (_throttleFrameRate != value && Application.isPlaying)
			{
				_throttleFrameRate = value;
				if (base.enabled && _mode != SMMode.Inactive)
				{
					RefreshThrottledFrameRate();
				}
			}
		}
	}

	public int ThrottledFrameRate
	{
		get
		{
			return _throttledFrameRate;
		}
		set
		{
			if (_throttledFrameRate != value && Application.isPlaying)
			{
				_throttledFrameRate = value;
				if (base.enabled && _mode != SMMode.Inactive)
				{
					RefreshThrottledFrameRate();
				}
			}
		}
	}

	public int AverageSamples
	{
		get
		{
			return _avgSamples;
		}
		set
		{
			if (_avgSamples == value || !Application.isPlaying)
			{
				return;
			}
			_avgSamples = value;
			if (!base.enabled)
			{
				return;
			}
			if (_avgSamples > 0)
			{
				if (_accAVGSamples == null)
				{
					_accAVGSamples = new float[_avgSamples];
				}
				else if (_accAVGSamples.Length != _avgSamples)
				{
					Array.Resize(ref _accAVGSamples, _avgSamples);
				}
			}
			else
			{
				_accAVGSamples = null;
			}
			ResetAverageFPS();
			UpdateData();
			UpdateView();
		}
	}

	public int WarningThreshold
	{
		get
		{
			return _warningThreshold;
		}
		set
		{
			_warningThreshold = value;
		}
	}

	public int CriticalThreshold
	{
		get
		{
			return _criticalThreshold;
		}
		set
		{
			_criticalThreshold = value;
		}
	}

	public void Toggle()
	{
		if (_mode == SMMode.Inactive)
		{
			Mode = SMMode.Active;
		}
		else if (_mode == SMMode.Active)
		{
			Mode = SMMode.Inactive;
		}
	}

	public void NextStyle()
	{
		if (_style == SMStyle.Minimal)
		{
			Style = SMStyle.StatsOnly;
		}
		else if (_style == SMStyle.StatsOnly)
		{
			Style = SMStyle.Standard;
		}
		else if (_style == SMStyle.Standard)
		{
			Style = SMStyle.Full;
		}
		else if (_style == SMStyle.Full)
		{
			Style = SMStyle.Minimal;
		}
	}

	public void NextAlignment()
	{
		if (_alignment == SMAlignment.UpperLeft)
		{
			Align(SMAlignment.UpperCenter);
		}
		else if (_alignment == SMAlignment.UpperCenter)
		{
			Align(SMAlignment.UpperRight);
		}
		else if (_alignment == SMAlignment.UpperRight)
		{
			Align(SMAlignment.MiddleRight);
		}
		else if (_alignment == SMAlignment.MiddleRight)
		{
			Align(SMAlignment.LowerRight);
		}
		else if (_alignment == SMAlignment.LowerRight)
		{
			Align(SMAlignment.LowerCenter);
		}
		else if (_alignment == SMAlignment.LowerCenter)
		{
			Align(SMAlignment.LowerLeft);
		}
		else if (_alignment == SMAlignment.LowerLeft)
		{
			Align(SMAlignment.MiddleLeft);
		}
		else if (_alignment == SMAlignment.MiddleLeft)
		{
			Align(SMAlignment.UpperLeft);
		}
	}

	public void Align(SMAlignment alignment, int newScale = -1)
	{
		_alignment = alignment;
		switch (alignment)
		{
		case SMAlignment.UpperLeft:
			_anchor = StatsMonitor.anchors.upperLeft;
			break;
		case SMAlignment.UpperCenter:
			_anchor = StatsMonitor.anchors.upperCenter;
			break;
		case SMAlignment.UpperRight:
			_anchor = StatsMonitor.anchors.upperRight;
			break;
		case SMAlignment.MiddleRight:
			_anchor = StatsMonitor.anchors.middleRight;
			break;
		case SMAlignment.LowerRight:
			_anchor = StatsMonitor.anchors.lowerRight;
			break;
		case SMAlignment.LowerCenter:
			_anchor = StatsMonitor.anchors.lowerCenter;
			break;
		case SMAlignment.LowerLeft:
			_anchor = StatsMonitor.anchors.lowerLeft;
			break;
		case SMAlignment.MiddleLeft:
			_anchor = StatsMonitor.anchors.middleLeft;
			break;
		default:
			Debug.LogWarning("Align() Invalid value: " + alignment);
			break;
		}
		RectTransform component = base.gameObject.GetComponent<RectTransform>();
		component.anchoredPosition = _anchor.position;
		component.anchorMin = _anchor.min;
		component.anchorMax = _anchor.max;
		component.pivot = _anchor.pivot;
	}

	public void ResetMinMaxFPS()
	{
		fpsMin = -1;
		fpsMax = -1;
		_minMaxIntervalsSkipped = 0;
		if (Application.isPlaying)
		{
			UpdateData(forceUpdate: true);
		}
	}

	public void ResetAverageFPS()
	{
		if (Application.isPlaying)
		{
			fpsAvg = 0;
			_currentAVGSamples = 0;
			_currentAVGRaw = 0f;
			if (_avgSamples > 0 && _accAVGSamples != null)
			{
				Array.Clear(_accAVGSamples, 0, _accAVGSamples.Length);
			}
		}
	}

	public void Dispose()
	{
		StopCoroutine("Interval");
		DisposeChildren();
		UnityEngine.Object.Destroy(this);
		_isDisposed = true;
	}

	internal void Invalidate(SMViewInvalidationType type, SMInvalidationFlag flag = SMInvalidationFlag.Any, bool invalidateChildren = true)
	{
		UpdateFont();
		float num = 0f;
		float num2 = 0f;
		if (_fpsView != null)
		{
			if (invalidateChildren && (flag == SMInvalidationFlag.Any || flag == SMInvalidationFlag.Text))
			{
				_fpsView.Invalidate(type);
			}
			if (_fpsView.Width > num)
			{
				num = _fpsView.Width;
			}
			num2 += _fpsView.Height;
		}
		if (_statsView != null)
		{
			if (invalidateChildren && (flag == SMInvalidationFlag.Any || flag == SMInvalidationFlag.Text))
			{
				_statsView.Invalidate(type);
			}
			if (_statsView.Width > num)
			{
				num = _statsView.Width;
			}
			num2 += _statsView.Height;
		}
		if (_graphView != null)
		{
			_graphView.SetWidth(num);
			if (invalidateChildren && (flag == SMInvalidationFlag.Any || flag == SMInvalidationFlag.Graph || flag == SMInvalidationFlag.Text))
			{
				_graphView.Invalidate();
			}
			_graphView.Y = 0f - num2;
			if (_graphView.Width > num)
			{
				num = _graphView.Width;
			}
			num2 += _graphView.Height;
		}
		if (_sysInfoView != null)
		{
			_sysInfoView.SetWidth(num);
			if (invalidateChildren && (flag == SMInvalidationFlag.Any || flag == SMInvalidationFlag.Text))
			{
				_sysInfoView.Invalidate(type);
			}
			_sysInfoView.Y = 0f - num2;
			if (_sysInfoView.Width > num)
			{
				num = _sysInfoView.Width;
			}
			num2 += _sysInfoView.Height;
		}
		if (_style != 0 && (type == SMViewInvalidationType.All || (type == SMViewInvalidationType.Style && flag == SMInvalidationFlag.Background)))
		{
			CreateBackground();
		}
		if (!(num > 0f) && !(num2 > 0f))
		{
			return;
		}
		_totalWidth = (int)num;
		_totalHeight = (int)num2;
		base.gameObject.transform.localScale = Vector3.one;
		SMUtil.RTransform(base.gameObject, Vector2.one, 0f, 0f, _totalWidth, _totalHeight);
		if (autoScale)
		{
			int num3 = (int)SMUtil.DPIScaleFactor(round: true);
			if (num3 > -1)
			{
				scale = num3;
			}
			if (scale > 10)
			{
				scale = 10;
			}
			if (_totalWidth * scale > Screen.currentResolution.width)
			{
				scale--;
			}
		}
		base.gameObject.transform.localScale = new Vector3(scale, scale, 1f);
		Align(_alignment, scale);
	}

	private void CreateChildren()
	{
		UpdateFont();
		base.gameObject.transform.localScale = Vector3.one;
		switch (_style)
		{
		case SMStyle.Minimal:
			DisposeChild(SMViewType.Background);
			DisposeChild(SMViewType.StatsView);
			DisposeChild(SMViewType.GraphView);
			DisposeChild(SMViewType.SysInfoView);
			if (_fpsView == null)
			{
				_fpsView = new FPSView(this);
			}
			break;
		case SMStyle.StatsOnly:
			CreateBackground();
			DisposeChild(SMViewType.FPSView);
			DisposeChild(SMViewType.GraphView);
			DisposeChild(SMViewType.SysInfoView);
			if (_statsView == null)
			{
				_statsView = new StatsView(this);
			}
			break;
		case SMStyle.Standard:
			CreateBackground();
			DisposeChild(SMViewType.FPSView);
			DisposeChild(SMViewType.SysInfoView);
			if (_statsView == null)
			{
				_statsView = new StatsView(this);
			}
			if (_graphView == null)
			{
				_graphView = new GraphView(this);
			}
			break;
		case SMStyle.Full:
			CreateBackground();
			DisposeChild(SMViewType.FPSView);
			if (_statsView == null)
			{
				_statsView = new StatsView(this);
			}
			if (_graphView == null)
			{
				_graphView = new GraphView(this);
			}
			if (_sysInfoView == null)
			{
				_sysInfoView = new SysInfoView(this);
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		foreach (Transform item in base.transform)
		{
			SMUtil.AddToUILayer(item.gameObject);
			foreach (Transform item2 in item.transform)
			{
				SMUtil.AddToUILayer(item2.gameObject);
			}
		}
		Invalidate(SMViewInvalidationType.All);
	}

	private void DisposeChildren()
	{
		DisposeChild(SMViewType.Background);
		DisposeChild(SMViewType.FPSView);
		DisposeChild(SMViewType.StatsView);
		DisposeChild(SMViewType.GraphView);
		DisposeChild(SMViewType.SysInfoView);
	}

	private void DisposeChild(SMViewType viewType)
	{
		switch (viewType)
		{
		case SMViewType.Background:
			if (_background != null)
			{
				UnityEngine.Object.Destroy(_gradient);
				UnityEngine.Object.Destroy(_background);
				_gradient = null;
				_background = null;
			}
			break;
		case SMViewType.FPSView:
			if (_fpsView != null)
			{
				_fpsView.Dispose();
			}
			_fpsView = null;
			break;
		case SMViewType.StatsView:
			if (_statsView != null)
			{
				_statsView.Dispose();
			}
			_statsView = null;
			break;
		case SMViewType.GraphView:
			if (_graphView != null)
			{
				_graphView.Dispose();
			}
			_graphView = null;
			break;
		case SMViewType.SysInfoView:
			if (_sysInfoView != null)
			{
				_sysInfoView.Dispose();
			}
			_sysInfoView = null;
			break;
		default:
			Debug.LogWarning("DisposeChild() Invalid value: " + viewType);
			break;
		}
	}

	private void UpdateData(bool forceUpdate = false, float timeElapsed = -1f)
	{
		int num = (int)_fpsNew;
		ms = 1000f / _fpsNew;
		if (fps != num || forceUpdate)
		{
			fps = num;
		}
		if (fps <= _criticalThreshold)
		{
			fpsLevel = 2;
		}
		else if (fps <= _warningThreshold)
		{
			fpsLevel = 1;
		}
		else
		{
			if (fps > 999)
			{
				fps = 999;
			}
			fpsLevel = 0;
		}
		if (_style == SMStyle.Minimal)
		{
			return;
		}
		if (_minMaxIntervalsSkipped < 3)
		{
			if (!forceUpdate)
			{
				_minMaxIntervalsSkipped++;
			}
		}
		else
		{
			if (fpsMin == -1)
			{
				fpsMin = fps;
			}
			else if (fps < fpsMin)
			{
				fpsMin = fps;
			}
			if (fpsMax == -1)
			{
				fpsMax = fps;
			}
			else if (fps > fpsMax)
			{
				fpsMax = fps;
			}
		}
		if (_avgSamples == 0)
		{
			_currentAVGSamples++;
			_currentAVGRaw += ((float)fps - _currentAVGRaw) / (float)_currentAVGSamples;
		}
		else
		{
			_accAVGSamples[_currentAVGSamples % _avgSamples] = fps;
			_currentAVGSamples++;
			_currentAVGRaw = GetAccumulatedAVGSamples();
		}
		int num2 = Mathf.RoundToInt(_currentAVGRaw);
		if (fpsAvg != num2 || forceUpdate)
		{
			fpsAvg = num2;
		}
		fixedUpdateRate = 1f / Time.fixedDeltaTime;
		memTotal = (float)Profiler.GetTotalReservedMemoryLong() / 1048576f;
		memAlloc = (float)Profiler.GetTotalAllocatedMemoryLong() / 1048576f;
		memMono = (float)GC.GetTotalMemory(forceFullCollection: false) / 1048576f;
		_intervalTimeCount2 += timeElapsed;
		if (!(_intervalTimeCount2 >= _objectsCountInterval || timeElapsed < 0f || forceUpdate))
		{
			return;
		}
		GameObject[] array = UnityEngine.Object.FindObjectsOfType<GameObject>();
		objectCount = array.Length;
		int num4 = (renderedObjectCount = 0);
		renderObjectCount = num4;
		for (int i = 0; i < array.Length; i++)
		{
			Renderer component = array[i].GetComponent<Renderer>();
			if (component != null)
			{
				num4 = renderObjectCount + 1;
				renderObjectCount = num4;
				if (component.isVisible)
				{
					num4 = renderedObjectCount + 1;
					renderedObjectCount = num4;
				}
			}
		}
		_intervalTimeCount2 = 0f;
	}

	private void UpdateView(float timeElapsed = -1f)
	{
		_intervalTimeCount += timeElapsed;
		if (_intervalTimeCount >= _statsUpdateInterval || timeElapsed < 0f)
		{
			if (_fpsView != null)
			{
				_fpsView.Update();
			}
			if (_statsView != null)
			{
				_statsView.Update();
			}
			if (_sysInfoView != null)
			{
				_sysInfoView.Update();
			}
			if (_statsUpdateInterval > _graphUpdateInterval)
			{
				_intervalTimeCount = 0f;
			}
		}
		if (_intervalTimeCount >= _graphUpdateInterval || timeElapsed < 0f)
		{
			if (_graphView != null)
			{
				_graphView.Update();
			}
			if (_graphUpdateInterval >= _statsUpdateInterval)
			{
				_intervalTimeCount = 0f;
			}
		}
	}

	private float GetAccumulatedAVGSamples()
	{
		float num = 0f;
		for (int i = 0; i < _avgSamples; i++)
		{
			num += _accAVGSamples[i];
		}
		if (_currentAVGSamples >= _avgSamples)
		{
			return num / (float)_avgSamples;
		}
		return num / (float)_currentAVGSamples;
	}

	private void RefreshThrottledFrameRate()
	{
		RefreshThrottledFrameRate(disable: false);
	}

	private void RefreshThrottledFrameRate(bool disable)
	{
		if (_throttleFrameRate && !disable)
		{
			if (_cachedVSync == -1)
			{
				_cachedVSync = QualitySettings.vSyncCount;
				_cachedFrameRate = Application.targetFrameRate;
				QualitySettings.vSyncCount = 0;
			}
			Application.targetFrameRate = _throttledFrameRate;
		}
		else if (_cachedVSync != -1)
		{
			QualitySettings.vSyncCount = _cachedVSync;
			Application.targetFrameRate = _cachedFrameRate;
			_cachedVSync = -1;
		}
	}

	private void DetermineActualUpdateInterval()
	{
		_actualUpdateInterval = ((_graphUpdateInterval < _statsUpdateInterval) ? _graphUpdateInterval : _statsUpdateInterval);
	}

	private void RestartCoroutine()
	{
		StopCoroutine("Interval");
		StartCoroutine("Interval");
	}

	private void UpdateFont()
	{
		if (!(fontFace != null))
		{
			fontFace = (Font)Resources.Load("Fonts/terminalstats", typeof(Font));
			if (fontFace == null)
			{
				fontFace = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
			}
		}
	}

	private void CreateBackground()
	{
		if (Math.Abs(colorBGUpper.a) < 0.01f && Math.Abs(colorBGLower.a) < 0.01f)
		{
			DisposeChild(SMViewType.Background);
		}
		else if (_background == null)
		{
			_gradient = new Texture2D(2, 2);
			_gradient.filterMode = FilterMode.Bilinear;
			_gradient.wrapMode = TextureWrapMode.Clamp;
			_background = base.gameObject.GetComponent<RawImage>();
			if (_background == null)
			{
				_background = base.gameObject.AddComponent<RawImage>();
			}
			_background.color = Color.white;
			_background.texture = _gradient;
		}
		if (_background != null)
		{
			_gradient.SetPixel(0, 0, colorBGLower);
			_gradient.SetPixel(1, 0, colorBGLower);
			_gradient.SetPixel(0, 1, colorBGUpper);
			_gradient.SetPixel(1, 1, colorBGUpper);
			_gradient.Apply();
		}
	}

	private IEnumerator Interval()
	{
		while (true)
		{
			float previousUpdateTime = Time.unscaledTime;
			int previousUpdateFrames = Time.frameCount;
			yield return new WaitForSeconds(_actualUpdateInterval);
			float num = Time.unscaledTime - previousUpdateTime;
			int num2 = Time.frameCount - previousUpdateFrames;
			_fpsNew = (float)num2 / num;
			UpdateData(forceUpdate: false, num);
			UpdateView(num);
		}
	}

	private void Awake()
	{
		int num2 = (fpsMax = -1);
		fpsMin = num2;
		_accAVGSamples = new float[_avgSamples];
		_isInitialized = true;
		_isDisposed = false;
	}

	private void Update()
	{
		if (KAConsole.pUnlocked && _isInitialized && inputEnabled && Input.anyKeyDown)
		{
			if ((modKeyToggle != 0 && hotKeyToggle != 0 && Input.GetKey(modKeyToggle) && Input.GetKeyDown(hotKeyToggle)) || (modKeyToggle == KeyCode.None && hotKeyToggle != 0 && Input.GetKeyDown(hotKeyToggle)))
			{
				Toggle();
			}
			else if (_mode == SMMode.Active && ((modKeyAlignment != 0 && hotKeyAlignment != 0 && Input.GetKey(modKeyAlignment) && Input.GetKeyDown(hotKeyAlignment)) || (modKeyAlignment == KeyCode.None && hotKeyAlignment != 0 && Input.GetKeyDown(hotKeyAlignment))))
			{
				NextAlignment();
			}
			else if (_mode == SMMode.Active && ((modKeyStyle != 0 && hotKeyStyle != 0 && Input.GetKey(modKeyStyle) && Input.GetKeyDown(hotKeyStyle)) || (modKeyStyle == KeyCode.None && hotKeyStyle != 0 && Input.GetKeyDown(hotKeyStyle))))
			{
				NextStyle();
			}
		}
	}

	private void OnEnable()
	{
		if (_isInitialized && _mode != SMMode.Inactive)
		{
			fps = 0;
			memTotal = 0f;
			memAlloc = 0f;
			memMono = 0f;
			_intervalTimeCount = 0f;
			_intervalTimeCount2 = 0f;
			ResetMinMaxFPS();
			ResetAverageFPS();
			DetermineActualUpdateInterval();
			if (_mode == SMMode.Active)
			{
				CreateChildren();
			}
			StartCoroutine("Interval");
			UpdateView();
			Invoke("RefreshThrottledFrameRate", 0.5f);
		}
	}

	private void OnDisable()
	{
		if (_isInitialized)
		{
			StopCoroutine("Interval");
			if (IsInvoking("RefreshThrottledFrameRate"))
			{
				CancelInvoke("RefreshThrottledFrameRate");
			}
			RefreshThrottledFrameRate(disable: true);
			DisposeChildren();
		}
	}

	private void OnDestroy()
	{
		if (_isInitialized)
		{
			DisposeChildren();
			_isInitialized = false;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
