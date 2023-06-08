using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.UI;

public class UIDropDown : UI
{
	public class CanvasSortInfo
	{
		public bool _OverrideSorting;

		public int _SortingLayerID;

		public int _SortingOrder;
	}

	public LocaleString _DefaultText;

	public UIWidget _DisplayWidget;

	public UIMenu _Menu;

	public bool _IsDropped;

	public bool _UpdateDisplayWidgetBackground;

	public bool _OverrideSorting = true;

	private UIWidget mBlockerWidget;

	private Canvas mRootCanvas;

	private Canvas mCanvas;

	private RectTransform mRootCanvasTransform;

	private CanvasSortInfo mOriginalSortInfo;

	private CanvasSortInfo mAppliedSortInfo;

	private const int CanvasSortOrder = 15000;

	public bool pIsDropped
	{
		get
		{
			return _Menu.pVisible;
		}
		set
		{
			_IsDropped = value;
			mBlockerWidget.pVisible = value;
			_Menu.pVisible = value;
			if (_OverrideSorting)
			{
				ApplyCanvasSortInfo(_IsDropped ? mAppliedSortInfo : mOriginalSortInfo);
			}
		}
	}

	public UIWidget pSelectedWidget
	{
		get
		{
			return _Menu.pSelectedWidget;
		}
		set
		{
			_Menu.pSelectedWidget = value;
		}
	}

	protected override void Initialize(UI parentUI)
	{
		base.Initialize(parentUI);
		mCanvas = GetComponent<Canvas>();
		FindRootCanvas(base.transform, ref mRootCanvas);
		CreateBlocker();
		_DisplayWidget.pText = _DefaultText.GetLocalizedString();
	}

	protected void CreateBlocker()
	{
		mRootCanvasTransform = mRootCanvas.transform as RectTransform;
		if (mBlockerWidget == null)
		{
			GameObject gameObject = new GameObject("Blocker");
			RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
			rectTransform.SetParent(GetComponent<Canvas>().transform, worldPositionStays: false);
			rectTransform.SetAsFirstSibling();
			rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			rectTransform.sizeDelta = Vector2.zero;
			mBlockerWidget = gameObject.AddComponent<UIWidget>();
			gameObject.AddComponent<NonDrawableGraphic>();
			AddWidget(mBlockerWidget);
			if (_OverrideSorting)
			{
				mOriginalSortInfo = new CanvasSortInfo();
				mOriginalSortInfo._OverrideSorting = mCanvas.overrideSorting;
				mOriginalSortInfo._SortingLayerID = mCanvas.sortingLayerID;
				mOriginalSortInfo._SortingOrder = mCanvas.sortingOrder;
				mAppliedSortInfo = new CanvasSortInfo();
				mAppliedSortInfo._OverrideSorting = true;
				mAppliedSortInfo._SortingLayerID = mCanvas.sortingLayerID;
				mAppliedSortInfo._SortingOrder = 15000;
			}
		}
		ResizeBlocker();
	}

	protected void ApplyCanvasSortInfo(CanvasSortInfo sortInfo)
	{
		mCanvas.overrideSorting = sortInfo._OverrideSorting;
		mCanvas.sortingLayerID = sortInfo._SortingLayerID;
		mCanvas.sortingOrder = sortInfo._SortingOrder;
	}

	protected void FindRootCanvas(Transform rootCanvasOfTransform, ref Canvas rootCanvas)
	{
		Canvas canvas = null;
		while (rootCanvasOfTransform != null && rootCanvas == null)
		{
			canvas = rootCanvasOfTransform.GetComponent<Canvas>();
			if (canvas != null)
			{
				canvas = canvas.rootCanvas;
				rootCanvasOfTransform = canvas.transform;
				if (rootCanvasOfTransform.GetComponent<CanvasScaler>() != null)
				{
					rootCanvas = canvas;
				}
			}
			rootCanvasOfTransform = rootCanvasOfTransform.parent;
		}
		if (rootCanvas == null)
		{
			rootCanvas = canvas;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (base.transform.hasChanged)
		{
			ResizeBlocker();
			base.transform.hasChanged = false;
		}
	}

	protected void ResizeBlocker()
	{
		if (mBlockerWidget != null && mRootCanvasTransform != null)
		{
			RectTransform rectTransform = mBlockerWidget.pRectTransform;
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, mRootCanvasTransform.rect.width);
			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mRootCanvasTransform.rect.height);
			rectTransform.position = mRootCanvasTransform.position;
		}
	}

	protected override void OnClick(UIWidget widget, PointerEventData eventData)
	{
		if (widget == _DisplayWidget || widget == mBlockerWidget)
		{
			pIsDropped = !pIsDropped;
			if (base.pEventTarget != null)
			{
				base.pEventTarget.TriggerOnClick(widget, eventData);
			}
		}
		else if (widget.pParentUI == _Menu && base.pEventTarget != null)
		{
			base.pEventTarget.TriggerOnClick(widget, eventData);
		}
	}

	protected override void OnSelected(UIWidget widget, UI fromUI)
	{
		base.OnSelected(widget, fromUI);
		if (widget == null)
		{
			_DisplayWidget.pText = _DefaultText.GetLocalizedString();
		}
		else
		{
			_DisplayWidget.pText = widget.pText;
			if (_UpdateDisplayWidgetBackground)
			{
				_DisplayWidget.pSprite = widget.pSprite;
			}
			pIsDropped = false;
		}
		if (base.pEventTarget != null)
		{
			base.pEventTarget.TriggerOnSelected(widget, this);
		}
	}

	protected override void SetParamsFromPublicVariables()
	{
		base.SetParamsFromPublicVariables();
		if (pIsDropped != _IsDropped)
		{
			pIsDropped = _IsDropped;
		}
	}
}
