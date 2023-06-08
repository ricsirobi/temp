using UnityEngine;
using UnityEngine.SceneManagement;

namespace StatsMonitor;

[DisallowMultipleComponent]
public class StatsMonitor : MonoBehaviour
{
	public const string NAME = "Stats Monitor";

	public const string VERSION = "1.3.5";

	internal static readonly SMAnchors anchors = new SMAnchors();

	internal bool isFirstScene = true;

	private StatsMonitorWidget _statsMonitorWidget;

	private Canvas _canvas;

	public static StatsMonitor instance { get; private set; }

	private static StatsMonitor InternalInstance
	{
		get
		{
			if (instance == null)
			{
				StatsMonitor statsMonitor = Object.FindObjectOfType<StatsMonitor>();
				if (statsMonitor != null)
				{
					instance = statsMonitor;
				}
				else
				{
					new GameObject("Stats Monitor").AddComponent<StatsMonitor>();
				}
			}
			return instance;
		}
	}

	public static StatsMonitorWidget Widget
	{
		get
		{
			if (!(instance != null))
			{
				return null;
			}
			return instance._statsMonitorWidget;
		}
	}

	public static StatsMonitor AddToScene()
	{
		return InternalInstance;
	}

	public void SetRenderMode(SMRenderMode renderMode)
	{
		switch (renderMode)
		{
		case SMRenderMode.Overlay:
			_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			_canvas.sortingOrder = 32767;
			break;
		case SMRenderMode.Camera:
		{
			Camera camera = Camera.current ?? Camera.main;
			_canvas.renderMode = RenderMode.ScreenSpaceCamera;
			_canvas.worldCamera = camera;
			_canvas.planeDistance = camera.nearClipPlane;
			_canvas.sortingLayerName = SortingLayer.layers[SortingLayer.layers.Length - 1].name;
			_canvas.sortingOrder = 32767;
			break;
		}
		}
	}

	private void CreateUI()
	{
		_canvas = base.gameObject.AddComponent<Canvas>();
		_canvas.pixelPerfect = true;
		RectTransform component = base.gameObject.GetComponent<RectTransform>();
		component.pivot = Vector2.up;
		component.anchorMin = Vector2.up;
		component.anchorMax = Vector2.up;
		component.anchoredPosition = new Vector2(0f, 0f);
		_statsMonitorWidget = GetComponentInChildren<StatsMonitorWidget>(includeInactive: true);
		_statsMonitorWidget.gameObject.SetActive(value: true);
		_statsMonitorWidget.wrapper = this;
	}

	private void DisposeInternal()
	{
		if (_statsMonitorWidget != null)
		{
			_statsMonitorWidget.Dispose();
		}
		Object.Destroy(this);
	}

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		if (base.transform.parent != null)
		{
			Debug.LogWarning("Stats Monitor has been moved to root. It needs to be in root to function properly. To add Stats Monitor to a scene always use the menu 'Game Object/Create Other/Stats Monitor'.");
			base.transform.parent = null;
		}
		if (_statsMonitorWidget == null || _statsMonitorWidget.KeepAlive)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
		CreateUI();
		if (_statsMonitorWidget != null)
		{
			SetRenderMode(_statsMonitorWidget.RenderMode);
		}
		SMUtil.AddToUILayer(base.gameObject);
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (!(_statsMonitorWidget == null))
		{
			if (!_statsMonitorWidget.KeepAlive && !isFirstScene)
			{
				DisposeInternal();
				return;
			}
			isFirstScene = false;
			_statsMonitorWidget.ResetMinMaxFPS();
			_statsMonitorWidget.ResetAverageFPS();
		}
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		Object.Destroy(base.gameObject);
		if (instance == this)
		{
			instance = null;
		}
	}
}
