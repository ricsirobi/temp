using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.UI;

public class UIMenu : UI
{
	public Action<bool> OnVisibilityChanged;

	public Action<WidgetState> OnMenuStateChanged;

	public UIWidget _Template;

	public MinMax _WidgetPoolCount;

	public RectTransform _ViewPortRectTransform;

	public MaskableGraphic _SelectionHighlight;

	public MaskableGraphic _HoverHighlight;

	public bool _UpdateOnlyOnVisible;

	[NonSerialized]
	private bool mInitialized;

	private GameObject mWidgetPoolParent;

	private UIWidget mSelectedWidget;

	private List<UIWidget> mWidgetPool = new List<UIWidget>();

	private PageFlip mPageFlip;

	private Vector2 mPrevScrollValue = Vector2.zero;

	private List<UIWidget> mCurrentVisibleWidgets = new List<UIWidget>();

	private List<UIWidget> mNewVisibleWidgets = new List<UIWidget>();

	private const string POOLED_WIDGET_NAME = "PooledWidget";

	private const string WIDGET_POOL_PARENT_NAME = "PoolParent";

	private const string WIDGET_POOL_COUNT_ERROR = "_WidgetPoolCount.Max cannot be lesser than _WidgetPoolCount.Min";

	public bool pIsPageFlipEnabled => mPageFlip != null;

	protected ScrollRect pScrollRect { get; private set; }

	public LayoutGroup pLayoutGroup { get; private set; }

	private bool pIsWidgetPoolingEnabled
	{
		get
		{
			if (!(_WidgetPoolCount.Min > 0f))
			{
				return _WidgetPoolCount.Max > 0f;
			}
			return true;
		}
	}

	public UIWidget pSelectedWidget
	{
		get
		{
			return mSelectedWidget;
		}
		set
		{
			mSelectedWidget = value;
			UpdateHighlightState(_SelectionHighlight, mSelectedWidget);
			if (base.pEventTarget != null)
			{
				base.pEventTarget.TriggerOnSelected(mSelectedWidget, this);
			}
		}
	}

	public UIWidget AddWidget(string widgetName)
	{
		UIWidget uIWidget = GetWidgetFromPool();
		if (uIWidget == null)
		{
			uIWidget = _Template.Duplicate();
		}
		uIWidget.name = widgetName;
		AddWidget(uIWidget);
		uIWidget.pVisible = true;
		uIWidget.transform.localScale = _Template.transform.localScale;
		OnAddingWidget(uIWidget);
		return uIWidget;
	}

	protected virtual void OnAddingWidget(UIWidget widget)
	{
		if (pIsPageFlipEnabled)
		{
			mPageFlip.UpdatePageCount();
		}
		if (_UpdateOnlyOnVisible)
		{
			StartCoroutine(UpdateWidgetsInViewWithDelay());
		}
	}

	public override void RemoveWidget(UIWidget widget, bool destroy = false, bool removeReferences = true)
	{
		if (pIsWidgetPoolingEnabled)
		{
			base.RemoveWidget(widget);
			AddWidgetToPool(widget);
		}
		else
		{
			base.RemoveWidget(widget, destroy, removeReferences);
		}
		if (widget == pSelectedWidget)
		{
			pSelectedWidget = null;
		}
		OnRemovingWidget();
	}

	protected virtual void OnRemovingWidget()
	{
		if (pIsPageFlipEnabled)
		{
			mPageFlip.UpdatePageCount();
		}
		if (_UpdateOnlyOnVisible)
		{
			StartCoroutine(UpdateWidgetsInViewWithDelay());
		}
	}

	public override void ClearChildren()
	{
		if (pIsWidgetPoolingEnabled)
		{
			for (int num = base.pChildWidgets.Count - 1; num >= 0; num--)
			{
				UIWidget widget = base.pChildWidgets[num];
				RemoveWidget(widget, destroy: false, removeReferences: false);
			}
			mCurrentVisibleWidgets.Clear();
			mNewVisibleWidgets.Clear();
		}
		base.ClearChildren();
		ResetScrollRect();
		pSelectedWidget = null;
	}

	private void ResetScrollRect()
	{
		if (!(pScrollRect != null))
		{
			return;
		}
		if (pScrollRect.vertical)
		{
			if (pScrollRect.verticalScrollbar != null)
			{
				pScrollRect.verticalNormalizedPosition = ((pScrollRect.verticalScrollbar.direction != Scrollbar.Direction.TopToBottom) ? 1 : 0);
			}
			else
			{
				pScrollRect.verticalNormalizedPosition = 1f;
			}
		}
		if (pScrollRect.horizontal)
		{
			if (pScrollRect.horizontalScrollbar != null)
			{
				pScrollRect.horizontalNormalizedPosition = ((pScrollRect.horizontalScrollbar.direction != 0) ? 1 : 0);
			}
			else
			{
				pScrollRect.horizontalNormalizedPosition = 0f;
			}
		}
	}

	protected virtual void UnloadWidget(UIWidget widget)
	{
	}

	protected virtual void LoadWidget(UIWidget widget)
	{
	}

	protected override void Initialize(UI parentUI)
	{
		base.Initialize(parentUI);
		if (mInitialized)
		{
			return;
		}
		mInitialized = true;
		pScrollRect = GetComponent<ScrollRect>();
		if (pScrollRect != null)
		{
			ScrollableRect scrollableRect = pScrollRect as ScrollableRect;
			if (scrollableRect != null)
			{
				scrollableRect.Initialize();
			}
		}
		pLayoutGroup = _ProxyTransform.GetComponent<LayoutGroup>();
		base.pEventReceiver.OnClick += OnMenuWidgetClick;
		base.pEventReceiver.OnHover += OnMenuWidgetHover;
		base.pEventReceiver.OnBeginDrag += OnMenuWidgetBeginDrag;
		base.pEventReceiver.OnDrag += OnMenuWidgetDrag;
		base.pEventReceiver.OnEndDrag += OnMenuWidgetEndDrag;
		if (pScrollRect != null && _UpdateOnlyOnVisible)
		{
			pScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
		}
		if (_HoverHighlight != null)
		{
			_HoverHighlight.enabled = false;
		}
		if (_SelectionHighlight != null)
		{
			_SelectionHighlight.enabled = false;
		}
		if (pIsWidgetPoolingEnabled && _Template != null)
		{
			CreateWidgetPool();
		}
		InitPageFlip();
	}

	private void OnScrollValueChanged(Vector2 value)
	{
		if (mPrevScrollValue != value)
		{
			mPrevScrollValue = value;
			UpdateWidgetsInView();
		}
	}

	private IEnumerator UpdateWidgetsInViewWithDelay()
	{
		yield return new WaitForEndOfFrame();
		UpdateWidgetsInView();
	}

	private void UpdateWidgetsInView()
	{
		GetWidgetsInView(mNewVisibleWidgets);
		if (mNewVisibleWidgets != null)
		{
			foreach (UIWidget mCurrentVisibleWidget in mCurrentVisibleWidgets)
			{
				if (!mNewVisibleWidgets.Contains(mCurrentVisibleWidget))
				{
					UnloadWidget(mCurrentVisibleWidget);
				}
			}
			foreach (UIWidget mNewVisibleWidget in mNewVisibleWidgets)
			{
				if (!mCurrentVisibleWidgets.Contains(mNewVisibleWidget))
				{
					LoadWidget(mNewVisibleWidget);
				}
			}
			List<UIWidget> list = mCurrentVisibleWidgets;
			mCurrentVisibleWidgets = mNewVisibleWidgets;
			mNewVisibleWidgets = list;
		}
		mNewVisibleWidgets.Clear();
	}

	private void GetWidgetsInView(List<UIWidget> widgetList)
	{
		widgetList.Clear();
		if (pScrollRect == null || pScrollRect.viewport == null || base.pChildWidgets.Count == 0)
		{
			return;
		}
		for (int i = 0; i < base.pChildWidgets.Count; i++)
		{
			if (RectTransformUtility.RectangleContainsScreenPoint(pScrollRect.viewport, base.pChildWidgets[i].GetScreenPosition()))
			{
				widgetList.Add(base.pChildWidgets[i]);
			}
		}
	}

	private void InitPageFlip()
	{
		mPageFlip = GetComponent<PageFlip>();
		if (mPageFlip != null)
		{
			mPageFlip.Initialize(pScrollRect);
		}
	}

	protected void OnMenuWidgetClick(UIWidget widget, PointerEventData eventData)
	{
		if (base.pEventTarget != null)
		{
			base.pEventTarget.TriggerOnClick(widget, eventData);
		}
		pSelectedWidget = widget;
		UpdateHighlightState(_SelectionHighlight, widget);
	}

	protected void OnMenuWidgetHover(UIWidget widget, bool hover, PointerEventData eventData)
	{
		UpdateHighlightState(_HoverHighlight, widget, hover);
	}

	protected void OnMenuWidgetBeginDrag(UIWidget widget, PointerEventData eventData)
	{
		if (pScrollRect != null && pScrollRect.enabled)
		{
			pScrollRect.OnBeginDrag(eventData);
		}
	}

	protected void OnMenuWidgetDrag(UIWidget widget, PointerEventData eventData)
	{
		if (pScrollRect != null && pScrollRect.enabled)
		{
			pScrollRect.OnDrag(eventData);
		}
	}

	protected void OnMenuWidgetEndDrag(UIWidget widget, PointerEventData eventData)
	{
		if (pScrollRect != null && pScrollRect.enabled)
		{
			pScrollRect.OnEndDrag(eventData);
			if (pIsPageFlipEnabled)
			{
				mPageFlip.OnEndDrag(eventData);
			}
		}
	}

	protected void CreateWidgetPool()
	{
		if (_WidgetPoolCount.Max < _WidgetPoolCount.Min)
		{
			UtDebug.LogError("_WidgetPoolCount.Max cannot be lesser than _WidgetPoolCount.Min");
			return;
		}
		mWidgetPoolParent = new GameObject("PoolParent");
		mWidgetPoolParent.transform.SetParent(base.transform);
		for (int i = 0; (float)i < _WidgetPoolCount.Min; i++)
		{
			UIWidget widget = _Template.Duplicate();
			AddWidgetToPool(widget);
		}
	}

	protected void AddWidgetToPool(UIWidget widget)
	{
		if ((float)mWidgetPool.Count < _WidgetPoolCount.Max)
		{
			widget.pVisible = false;
			widget.transform.SetParent(mWidgetPoolParent.transform);
			widget.name = "PooledWidget";
			mWidgetPool.Add(widget);
		}
		else
		{
			UnityEngine.Object.Destroy(widget.gameObject);
		}
	}

	protected UIWidget GetWidgetFromPool()
	{
		if (mWidgetPool.Count == 0)
		{
			return null;
		}
		UIWidget result = mWidgetPool[mWidgetPool.Count - 1];
		mWidgetPool.RemoveAt(mWidgetPool.Count - 1);
		return result;
	}

	protected int GetNumItemsPerPage()
	{
		GridLayoutGroup gridLayoutGroup = pLayoutGroup as GridLayoutGroup;
		if (gridLayoutGroup == null)
		{
			return 0;
		}
		Rect rect = RectTransformUtility.PixelAdjustRect(_ViewPortRectTransform, base.pCanvas);
		rect.width -= gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
		rect.height -= gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;
		float num = rect.width * rect.height;
		float num2 = (gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x) * (gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y);
		return Mathf.CeilToInt(num / num2);
	}

	private void UpdateHighlightState(MaskableGraphic highlightGraphic, UIWidget widget, bool highlight = true)
	{
		if (!(highlightGraphic == null))
		{
			if (highlight && widget != null)
			{
				highlightGraphic.enabled = true;
				highlightGraphic.rectTransform.SetParent(widget.pRectTransform);
				highlightGraphic.rectTransform.anchoredPosition = Vector3.zero;
			}
			else
			{
				highlightGraphic.rectTransform.SetParent(base.pRectTransform);
				highlightGraphic.enabled = false;
			}
		}
	}

	protected override void OnVisibleInHierarchyChanged(bool newVisible)
	{
		base.OnVisibleInHierarchyChanged(newVisible);
		if (_UpdateOnlyOnVisible && newVisible)
		{
			UpdateWidgetsInView();
		}
		if (OnVisibilityChanged != null)
		{
			OnVisibilityChanged(newVisible);
		}
	}

	protected override void OnStateInHierarchyChanged(WidgetState previousState, WidgetState newState)
	{
		base.OnStateInHierarchyChanged(previousState, newState);
		if (pScrollRect != null)
		{
			pScrollRect.enabled = newState == WidgetState.INTERACTIVE;
			if (pScrollRect.horizontalScrollbar != null)
			{
				pScrollRect.horizontalScrollbar.interactable = newState == WidgetState.INTERACTIVE;
			}
			if (pScrollRect.verticalScrollbar != null)
			{
				pScrollRect.verticalScrollbar.interactable = newState == WidgetState.INTERACTIVE;
			}
		}
		if (OnMenuStateChanged != null)
		{
			OnMenuStateChanged(newState);
		}
	}
}
