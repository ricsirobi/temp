using System;
using UnityEngine;

namespace JSGames.UI;

public class UISafeArea : KAMonoBase
{
	[Header("Bottom-Corner Anchored Object Configuration")]
	public bool _CanBottomCornerWidgetsMoveUp;

	public float _IgnoreThresholdFromEdges = 0.2f;

	[Space]
	public RectTransform _SafeAreaTransform;

	private ScreenOrientation mLastOrientation = ScreenOrientation.Landscape;

	private Vector2 mLastResolution = Vector2.zero;

	private Vector2 mLastSafeArea = Vector2.zero;

	private Canvas mCanvas;

	private Vector2 mLastSafeAreaMin = Vector2.zero;

	private Rect mForceSafeAreaRect;

	public event Action OnOrientationChanged;

	public event Action OnResolutionChanged;

	private void Awake()
	{
		float num = _IgnoreThresholdFromEdges * (float)Screen.width;
		mForceSafeAreaRect = new Rect(new Vector2(num, 0f), new Vector2((float)Screen.width - num * 2f, Screen.height));
		mCanvas = GetCanvas();
		mLastOrientation = Screen.orientation;
		mLastResolution.x = Screen.width;
		mLastResolution.y = Screen.height;
		if (_SafeAreaTransform == null && base.transform.Find("SafeArea") != null)
		{
			_SafeAreaTransform = base.transform.Find("SafeArea").GetComponent<RectTransform>();
		}
	}

	private void Start()
	{
		if (mCanvas.renderMode != 0)
		{
			RenderMode renderMode = mCanvas.renderMode;
			mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			ApplySafeArea(GetScreenSafeArea());
			mCanvas.renderMode = renderMode;
		}
		else
		{
			ApplySafeArea(GetScreenSafeArea());
		}
	}

	private void Update()
	{
		if (UtPlatform.IsMobile())
		{
			if (Screen.orientation != mLastOrientation)
			{
				OrientationChanged();
			}
			if (GetScreenSafeArea().size != mLastSafeArea)
			{
				ApplySafeArea(GetScreenSafeArea());
			}
		}
		else if ((float)Screen.width != mLastResolution.x || (float)Screen.height != mLastResolution.y)
		{
			ResolutionChanged();
		}
	}

	private Rect GetScreenSafeArea()
	{
		return Screen.safeArea;
	}

	private void ApplySafeArea(Rect safeArea)
	{
		if (!(_SafeAreaTransform == null) && !(mCanvas == null))
		{
			Vector2 position = safeArea.position;
			Vector2 anchorMax = safeArea.position + safeArea.size;
			position.x /= mCanvas.pixelRect.width;
			position.y /= mCanvas.pixelRect.height;
			anchorMax.x /= mCanvas.pixelRect.width;
			anchorMax.y /= mCanvas.pixelRect.height;
			_SafeAreaTransform.anchorMin = position;
			_SafeAreaTransform.anchorMax = anchorMax;
			if (!_CanBottomCornerWidgetsMoveUp)
			{
				UpdateBottomCornerWidgetsPosition((mLastSafeAreaMin - safeArea.position).y / mCanvas.scaleFactor);
			}
			mLastSafeArea = safeArea.size;
			mLastSafeAreaMin = safeArea.position;
		}
	}

	private void UpdateBottomCornerWidgetsPosition(float offsetY)
	{
		foreach (RectTransform item in _SafeAreaTransform)
		{
			UpdateBottomCornerWidgetsPosition(item, offsetY);
		}
	}

	private void UpdateBottomCornerWidgetsPosition(RectTransform rectTransform, float offsetY)
	{
		if (rectTransform.anchorMin.y == 0f && rectTransform.anchorMin.x == rectTransform.anchorMax.x && (rectTransform.anchorMin.x == 1f || rectTransform.anchorMin.x == 0f) && !RectTransformToScreenSpace(rectTransform).Overlaps(mForceSafeAreaRect))
		{
			Vector2 anchoredPosition = rectTransform.anchoredPosition;
			anchoredPosition.y += offsetY;
			rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public Rect RectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 vector = Vector2.Scale(transform.rect.size, transform.lossyScale);
		Rect result = new Rect(transform.position.x, transform.position.y, vector.x, vector.y);
		result.x -= transform.pivot.x * vector.x;
		result.y -= transform.pivot.y * vector.y;
		return result;
	}

	private void OrientationChanged()
	{
		mLastOrientation = Screen.orientation;
		mLastResolution.x = Screen.width;
		mLastResolution.y = Screen.height;
		if (this.OnOrientationChanged != null)
		{
			this.OnOrientationChanged();
		}
	}

	private void ResolutionChanged()
	{
		mLastResolution.x = Screen.width;
		mLastResolution.y = Screen.height;
		if (this.OnResolutionChanged != null)
		{
			this.OnResolutionChanged();
		}
	}

	private Canvas GetCanvas()
	{
		Canvas component = GetComponent<Canvas>();
		if (component == null && base.transform.root != null)
		{
			component = base.transform.root.GetComponent<Canvas>();
		}
		return component;
	}
}
