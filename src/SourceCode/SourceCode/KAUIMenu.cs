using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KAUIMenu : KAUI
{
	[Serializable]
	public class GridLayoutProperties
	{
		public int _MaxItemsPerLine = 3;

		public float _CellWidth = 165f;

		public float _CellHeight = 200f;

		public Vector3 _GridStartPosition = new Vector3(-398f, 382f, 0f);

		public KAWidget _Template;

		public KAWidget _HighlightWidget;

		public KAWidget _SelectedWidget;
	}

	public delegate void OnPageChange(int pageNumber);

	public KAUI _ParentUi;

	public KAWidget _Template;

	public KAUIMenuGrid _DefaultGrid;

	public bool _CenterOnItem;

	public bool _PageFlip;

	public bool _ResetItemsOnVisibility = true;

	public bool _PageFlipSnapOnRelease = true;

	public float _PageSnapStrength = 10f;

	public float _CenterSnapStrength = 10f;

	public float _ScrollDelta = 0.1f;

	public bool _AllowDrag = true;

	public bool _OnlyUpdateWhenBecomingVisible;

	public bool _EnableGridLayoutForDevices;

	public GridLayoutProperties _GridLayoutSmallDevices;

	public GridLayoutProperties _GridLayoutLargeDevices;

	public KAWidget _HighlightWidget;

	public KAWidget _SelectedWidget;

	public KAWidget _PageNumberWidget;

	public LocaleString _PageNumberText = new LocaleString("Page");

	public GameObject _BackgroundObject;

	public OptimizedMenu _OptimizedMenu;

	public Collider _Collider;

	[SerializeField]
	protected int _ItemPoolCount = 10;

	[SerializeField]
	protected int _ItemPoolBaseCount;

	protected bool mViewChanged = true;

	protected KAUIDraggablePanel mDragPanel;

	protected KAScrollBar mVerticalScrollbar;

	protected KAScrollBar mHorizontalScrollbar;

	protected UIPanel mPanel;

	protected KAUIMenuGrid mCurrentGrid;

	protected KAWidget mSelectedItem;

	protected UICenterOnChild mCenterOnChild;

	protected SpringPanel mSpringPanel;

	protected bool mRecenterItem;

	protected Stack mPoolWidgets = new Stack();

	protected GameObject mPool;

	protected int mPageCount = 1;

	protected int mCurrentPage = 1;

	private bool mLastVisible;

	private float mPageSize;

	protected Vector3 mInitialPosition;

	private Vector3 mPosition;

	private Vector4 mInitialClipRange;

	protected Vector3 mLastPosition = Vector3.zero;

	private Vector3 mPanelCachedPosition;

	private List<KAWidget> mItemsInView = new List<KAWidget>();

	public OnPageChange onPageChange;

	private int mDefaultFocusIndex = -1;

	public KAScrollBar pVerticalScrollbar => mVerticalScrollbar;

	public KAScrollBar pHortizontalScrollbar => mHorizontalScrollbar;

	public KAUIDraggablePanel pDragPanel
	{
		get
		{
			if (mDragPanel == null)
			{
				mDragPanel = GetComponentInChildren<KAUIDraggablePanel>();
			}
			return mDragPanel;
		}
	}

	public UIPanel pPanel
	{
		get
		{
			if (mPanel == null)
			{
				mPanel = GetComponentInChildren<UIPanel>();
			}
			return mPanel;
		}
	}

	public bool pRecenterItem
	{
		get
		{
			return mRecenterItem;
		}
		set
		{
			if (_CenterOnItem)
			{
				mRecenterItem = value;
			}
		}
	}

	public bool pViewChanged
	{
		get
		{
			return mViewChanged;
		}
		set
		{
			if (value)
			{
				PanelChanged();
			}
			else
			{
				mViewChanged = false;
			}
		}
	}

	public bool pIsOptimized => _OptimizedMenu._Enable;

	public KAUIMenuGrid pMenuGrid => mCurrentGrid;

	public bool pIsInitialized => pPanel != null;

	public int GetDefaultFocusIndex()
	{
		return mDefaultFocusIndex;
	}

	public void SetDefaultFocusIndex(int index)
	{
		mDefaultFocusIndex = index;
	}

	protected override void Awake()
	{
		base.Awake();
		if (_EnableGridLayoutForDevices)
		{
			GridLayoutProperties gridLayoutProperties = (KAUIManager.IsSmallScreen() ? _GridLayoutSmallDevices : _GridLayoutLargeDevices);
			if (gridLayoutProperties._Template != null)
			{
				_Template = gridLayoutProperties._Template;
			}
			if ((bool)gridLayoutProperties._HighlightWidget)
			{
				_HighlightWidget = gridLayoutProperties._HighlightWidget;
			}
			if (gridLayoutProperties._SelectedWidget != null)
			{
				_SelectedWidget = gridLayoutProperties._SelectedWidget;
			}
		}
		Initialize();
	}

	private void Initialize()
	{
		mPanelCachedPosition = Vector3.zero;
		UIPanel uIPanel = pPanel;
		uIPanel.onGeometryUpdated = (UIPanel.OnGeometryUpdated)Delegate.Combine(uIPanel.onGeometryUpdated, new UIPanel.OnGeometryUpdated(PanelChanged));
		if (pDragPanel != null)
		{
			if (pDragPanel.verticalScrollBar != null)
			{
				mVerticalScrollbar = pDragPanel.verticalScrollBar.GetComponent<KAScrollBar>();
			}
			if (pDragPanel.horizontalScrollBar != null)
			{
				mHorizontalScrollbar = pDragPanel.horizontalScrollBar.GetComponent<KAScrollBar>();
			}
			if (mVerticalScrollbar != null)
			{
				if (mVerticalScrollbar._UpArrow != null && mVerticalScrollbar._DownArrow != null)
				{
					KAScrollButton upArrow = mVerticalScrollbar._UpArrow;
					upArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(upArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ButtonClicked));
					KAScrollButton downArrow = mVerticalScrollbar._DownArrow;
					downArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(downArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ButtonClicked));
				}
				mVerticalScrollbar.Init(OnScrollBackgroundPress);
			}
			if (mHorizontalScrollbar != null)
			{
				if (mHorizontalScrollbar._UpArrow != null && mHorizontalScrollbar._DownArrow != null)
				{
					KAScrollButton upArrow2 = mHorizontalScrollbar._UpArrow;
					upArrow2.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(upArrow2.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ButtonClicked));
					KAScrollButton downArrow2 = mHorizontalScrollbar._DownArrow;
					downArrow2.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(downArrow2.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ButtonClicked));
				}
				mHorizontalScrollbar.Init(OnScrollBackgroundPress);
			}
		}
		pDragPanel.Init(this);
		mCurrentGrid = _DefaultGrid;
		CreateWidgetPool();
		if (_CenterOnItem)
		{
			mCenterOnChild = mCurrentGrid.gameObject.AddComponent<UICenterOnChild>();
			mCenterOnChild.onFinished = OnCenterItemFinished;
			mCenterOnChild.springStrength = _CenterSnapStrength;
			_PageFlip = false;
		}
		mInitialPosition = pDragPanel.transform.localPosition;
		mPosition = mInitialPosition;
		mInitialClipRange = pPanel.baseClipRegion;
		KAUIMenuGrid kAUIMenuGrid = mCurrentGrid;
		kAUIMenuGrid.onReposition = (UITable.OnReposition)Delegate.Combine(kAUIMenuGrid.onReposition, new UITable.OnReposition(OnGridReposition));
		if (pIsOptimized)
		{
			_OptimizedMenu.Initialize(this);
		}
	}

	private void CreateWidgetPool()
	{
		if (!(_Template != null) || _ItemPoolCount <= 0)
		{
			return;
		}
		mPool = new GameObject("WidgetPool");
		mPool.gameObject.transform.parent = base.gameObject.transform.parent;
		for (int i = 0; i < _ItemPoolBaseCount; i++)
		{
			KAWidget kAWidget = DuplicateWidget(_Template);
			kAWidget.gameObject.transform.parent = mPool.transform;
			kAWidget.transform.localPosition = new Vector3(0f, 10000f, 0f);
			kAWidget.SetVisibility(inVisible: false);
			kAWidget.gameObject.name = "PoolWidget" + (mPoolWidgets.Count + 1);
			mPoolWidgets.Push(kAWidget);
			UIPanel component = kAWidget.GetComponent<UIPanel>();
			if (component != null)
			{
				UnityEngine.Object.Destroy(component);
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		if (_EnableGridLayoutForDevices)
		{
			GridLayoutProperties gridLayoutProperties = (KAUIManager.IsSmallScreen() ? _GridLayoutSmallDevices : _GridLayoutLargeDevices);
			mCurrentGrid.maxPerLine = gridLayoutProperties._MaxItemsPerLine;
			mCurrentGrid.cellWidth = gridLayoutProperties._CellWidth;
			mCurrentGrid.cellHeight = gridLayoutProperties._CellHeight;
			mCurrentGrid.transform.localPosition = gridLayoutProperties._GridStartPosition;
		}
	}

	public void OnPressBackground(GameObject go, bool isPressed)
	{
		if (_ResetItemsOnVisibility)
		{
			ResetPosition();
		}
		if (mCurrentGrid != null)
		{
			UpdateGridItems(mCurrentGrid);
		}
		UpdatePageCount();
	}

	private void PanelChanged()
	{
		if (mPanelCachedPosition != pPanel.transform.position)
		{
			mViewChanged = true;
			mPanelCachedPosition = pPanel.transform.position;
		}
	}

	public void OnScrollBackgroundPress(KAScrollBar inScrollbar, bool isPressed)
	{
		if (pIsOptimized && isPressed)
		{
			Vector2 zero = Vector2.zero;
			if (inScrollbar.direction == UIScrollBar.Direction.Horizontal)
			{
				zero.x = inScrollbar.value;
			}
			else if (inScrollbar.direction == UIScrollBar.Direction.Vertical)
			{
				zero.y = inScrollbar.value;
			}
			_OptimizedMenu.SnapTo(zero);
			pRecenterItem = true;
		}
	}

	public void OnDragBackground(GameObject go, Vector2 delta)
	{
	}

	public void OnPressForeground(GameObject go, bool isPressed)
	{
		if (_ResetItemsOnVisibility)
		{
			mDragPanel.ResetPosition();
		}
		if (_ResetItemsOnVisibility)
		{
			ResetPosition();
		}
	}

	public void OnDragForeground(GameObject go, Vector2 delta)
	{
	}

	public Rect CalculateWidgetScreenRect(UIWidget inWidget)
	{
		Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(inWidget.cachedTransform);
		Vector3 vector = UICamera.currentCamera.WorldToScreenPoint(bounds.min);
		Vector3 vector2 = UICamera.currentCamera.WorldToScreenPoint(bounds.max);
		return new Rect(vector.x, vector.y, vector2.x - vector.x, vector2.y - vector.y);
	}

	public virtual void ResetPosition()
	{
		if (pDragPanel != null)
		{
			pDragPanel.ResetPosition();
		}
	}

	public void Resize(float x, float y)
	{
		pPanel.baseClipRegion = new Vector4(mInitialClipRange.x - x / 2f, mInitialClipRange.y - y / 2f, mInitialClipRange.z + x, mInitialClipRange.w + y);
		mPosition = mInitialPosition + new Vector3(x, y, 0f);
		base.transform.localPosition = mPosition;
		pPanel.clipOffset = Vector2.zero;
	}

	protected virtual void LateUpdate()
	{
		UpdateSwipeCollider();
		if (base.transform.localPosition != mLastPosition)
		{
			if (pIsOptimized)
			{
				_OptimizedMenu.CheckClippedWidgets(base.transform.localPosition - mLastPosition);
			}
			mViewChanged = true;
		}
		if (Time.frameCount % 5 == 0 && GetVisibility() && mViewChanged && (!_OnlyUpdateWhenBecomingVisible || !mLastVisible))
		{
			mViewChanged = false;
			UpdateItemColliders();
			UpdateScrollbars(inVisible: true);
			LoadItemsInView();
			CheckClosestWidget();
			if (_PageFlip && _PageFlipSnapOnRelease)
			{
				Vector3 vector = mPosition - pDragPanel.transform.localPosition;
				mCurrentPage = Mathf.RoundToInt((IsHorizontalMovement() ? vector.x : (0f - vector.y)) / mPageSize) + 1;
				mCurrentPage = Mathf.Clamp(mCurrentPage, 1, mPageCount);
			}
		}
		if (mRecenterItem)
		{
			UpdateCenterOnChild();
		}
		mLastVisible = GetVisibility();
		mLastPosition = base.transform.localPosition;
	}

	private void UpdatePageNumber()
	{
		if (_PageNumberWidget != null)
		{
			_PageNumberWidget.SetText(_PageNumberText.GetLocalizedString() + " " + GetCurrentPage() + "/" + GetPageCount());
		}
	}

	private void UpdateCenterOnChild()
	{
		if (mCenterOnChild != null)
		{
			mCenterOnChild.Recenter();
		}
		pRecenterItem = false;
	}

	protected virtual void UpdateItemColliders()
	{
		_ = pIsOptimized;
	}

	private void UpdateSwipeCollider()
	{
		if (_Collider != null)
		{
			UpdateCollider(_Collider);
		}
		else
		{
			UpdateCollider(collider);
		}
	}

	private void UpdateCollider(Collider menuCollider)
	{
		if (menuCollider != null && pPanel != null && menuCollider is BoxCollider)
		{
			Vector3 center = ((BoxCollider)menuCollider).center;
			center.y = pPanel.baseClipRegion.y - base.transform.localPosition.y;
			center.x = pPanel.baseClipRegion.x - base.transform.localPosition.x;
			((BoxCollider)menuCollider).center = center;
		}
	}

	protected virtual void ButtonClicked(KAScrollBar scrollBar, KAScrollButton scrollButton, bool isRepeated)
	{
		if (scrollBar == null || scrollButton == null || (_PageFlip && isRepeated))
		{
			return;
		}
		if (_PageFlip)
		{
			if (scrollButton._DirectionUp)
			{
				MoveToPreviousPage();
			}
			else
			{
				MoveToNextPage();
			}
			scrollBar.UpdateButtonsOnPageFlip(mCurrentPage, mPageCount);
			return;
		}
		scrollBar.Scroll(scrollButton._DirectionUp, _ScrollDelta);
		Vector2 zero = Vector2.zero;
		if (scrollBar.direction == UIScrollBar.Direction.Horizontal)
		{
			zero.x = scrollBar.value;
		}
		else if (scrollBar.direction == UIScrollBar.Direction.Vertical)
		{
			zero.y = scrollBar.value;
		}
		if (pIsOptimized)
		{
			_OptimizedMenu.SnapTo(zero);
		}
		pRecenterItem = true;
	}

	public KAWidget GetSelectedItem()
	{
		return mSelectedItem;
	}

	public int GetSelectedItemIndex()
	{
		if (pIsOptimized)
		{
			return mSelectedItem._MenuItemIndex;
		}
		return mItemInfo.IndexOf(mSelectedItem);
	}

	public virtual void SetSelectedItem(KAWidget inWidget)
	{
		if (mSelectedItem != null)
		{
			mSelectedItem.OnSelected(inSelected: false);
		}
		mSelectedItem = inWidget;
		if (mSelectedItem != null)
		{
			mSelectedItem.OnSelected(inSelected: true);
		}
		UpdateHighlightState(inWidget, _SelectedWidget, inWidget != null);
		if (_ParentUi != null)
		{
			_ParentUi.SetSelectedItem(this, inWidget);
		}
	}

	protected virtual void UpdateGridItems(KAUIMenuGrid activeGrid)
	{
		mCurrentGrid = activeGrid;
		UpdatePageCount();
		mCurrentGrid.repositionNow = true;
	}

	protected virtual void ResetScrollBar()
	{
		if (!(pDragPanel == null))
		{
			pDragPanel.UpdateScrollbars(recalculateBounds: true);
			if (mVerticalScrollbar != null)
			{
				mVerticalScrollbar.Reset();
			}
			if (mHorizontalScrollbar != null)
			{
				mHorizontalScrollbar.Reset();
			}
		}
	}

	public override void AddWidget(KAWidget inItem)
	{
		if (pPanel == null)
		{
			Initialize();
		}
		base.AddWidget(inItem);
		AddToGrid(inItem);
	}

	public virtual KAWidget AddWidget(string widgetName, KAWidgetUserData userData)
	{
		KAWidget kAWidget = null;
		if (!pIsOptimized || (pIsOptimized && _OptimizedMenu.pCanAddWidget))
		{
			kAWidget = ((mPoolWidgets.Count <= 0) ? DuplicateWidget(_Template) : GetPoolWidget());
		}
		if (kAWidget != null)
		{
			kAWidget.gameObject.SetActive(value: true);
			kAWidget.transform.name = widgetName;
			kAWidget.transform.localScale = _Template.transform.localScale;
			kAWidget.SetUserData(userData);
			kAWidget._MenuItemIndex = mItemInfo.Count;
			pRecenterItem = true;
			AddWidget(kAWidget);
			kAWidget.SetVisibility(inVisible: true);
		}
		if (pIsOptimized)
		{
			_OptimizedMenu.AddUserData(kAWidget, userData);
		}
		return kAWidget;
	}

	public virtual KAWidget AddWidget(string inWidgetName)
	{
		return AddWidget(inWidgetName, null);
	}

	public override void AddWidget(KAWidget inItem, UIAnchor.Side anchorSide)
	{
		base.AddWidget(inItem, anchorSide);
		AddToGrid(inItem);
	}

	public override void AddWidgetAt(int index, KAWidget inItem)
	{
		base.AddWidgetAt(index, inItem);
		AddToGrid(inItem);
	}

	public override void AddWidgetAt(int index, KAWidget inItem, UIAnchor.Side anchorSide)
	{
		base.AddWidgetAt(index, inItem, anchorSide);
		AddToGrid(inItem);
	}

	public override KAWidget AddWidget(Type inType, Texture2D inTexture, Shader inShader, string inWidgetName)
	{
		KAWidget kAWidget = base.AddWidget(inType, inTexture, inShader, inWidgetName);
		AddToGrid(kAWidget);
		return kAWidget;
	}

	private void AddToGrid(KAWidget inItem)
	{
		if (mCurrentGrid != null)
		{
			inItem.pUI = this;
			inItem.transform.parent = mCurrentGrid.transform;
			inItem.transform.localPosition = new Vector3(0f, 0f, 0f);
			inItem.OnParentSetVisibility(GetVisibility());
			inItem.OnParentSetState(GetState());
			UpdateGridItems(mCurrentGrid);
			mViewChanged = true;
		}
	}

	protected void ResetHighlightWidgets(KAWidget inWidget)
	{
		if (_HighlightWidget != null && (inWidget == null || inWidget == _HighlightWidget.transform.parent))
		{
			_HighlightWidget.transform.parent = _HighlightWidget.pAnchor.transform;
			_HighlightWidget.SetVisibility(inVisible: false);
		}
		if (_SelectedWidget != null && (inWidget == null || inWidget == _SelectedWidget.transform.parent))
		{
			_SelectedWidget.transform.parent = _SelectedWidget.pAnchor.transform;
			_SelectedWidget.SetVisibility(inVisible: false);
		}
	}

	public override void RemoveWidget(KAWidget inWidget)
	{
		ResetHighlightWidgets(inWidget);
		mItemInfo.Remove(inWidget);
		if (mPool != null && _ItemPoolCount > 0 && mPoolWidgets.Count < _ItemPoolCount)
		{
			inWidget.gameObject.transform.parent = mPool.transform;
			inWidget.ResetWidget();
			inWidget.SetVisibility(inVisible: false);
			inWidget.transform.localPosition = new Vector3(0f, 10000f, 0f);
			mPoolWidgets.Push(inWidget);
		}
		else
		{
			UnityEngine.Object.Destroy(inWidget.gameObject);
		}
		mCurrentGrid.repositionNow = true;
		UpdatePageCount();
		PanelChanged();
	}

	public virtual List<Transform> GetTransforms(KAUIMenuGrid inGrid = null)
	{
		List<Transform> list = new List<Transform>();
		foreach (KAWidget item in mItemInfo)
		{
			list.Add(item.transform);
		}
		return list;
	}

	public List<KAWidget> GetItems()
	{
		return mItemInfo;
	}

	public void SetItems(List<KAWidget> list)
	{
		mItemInfo = list;
	}

	public int GetNumItems()
	{
		if (pIsOptimized)
		{
			return _OptimizedMenu.GetDataCount();
		}
		return mItemInfo.Count;
	}

	public int GetNumItemsByBounds()
	{
		if (pPanel == null || mCurrentGrid.transform.childCount == 0)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < mCurrentGrid.transform.childCount; i++)
		{
			Transform child = mCurrentGrid.transform.GetChild(i);
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(pPanel.transform, child.transform);
			num2 = ((mCurrentGrid.arrangement != 0) ? ((int)Math.Round(bounds.extents.x / (mCurrentGrid.cellWidth / 2f), MidpointRounding.AwayFromZero)) : ((int)Math.Round(bounds.extents.y / (mCurrentGrid.cellHeight / 2f), MidpointRounding.AwayFromZero)));
			num += Mathf.Abs(num2);
		}
		return num;
	}

	public KAWidget GetItemAt(int index)
	{
		if (index < mItemInfo.Count)
		{
			return mItemInfo[index];
		}
		return null;
	}

	public override void SetState(KAUIState inState)
	{
		base.SetState(inState);
		if (mVerticalScrollbar != null)
		{
			mVerticalScrollbar.SetState(inState);
		}
		if (mHorizontalScrollbar != null)
		{
			mHorizontalScrollbar.SetState(inState);
		}
		UpdateColliderState();
	}

	private void UpdateColliderState()
	{
		if (_Collider != null)
		{
			UpdateColliderState(_Collider);
		}
		else
		{
			UpdateColliderState(collider);
		}
	}

	private void UpdateColliderState(Collider menuCollider)
	{
		if (menuCollider == null)
		{
			return;
		}
		if (_AllowDrag)
		{
			if (GetVisibility() && GetState() == KAUIState.INTERACTIVE)
			{
				menuCollider.enabled = true;
			}
			else
			{
				menuCollider.enabled = false;
			}
		}
		else
		{
			menuCollider.enabled = false;
		}
	}

	public virtual void UpdateScrollbars(bool inVisible)
	{
		bool flag = pDragPanel.showScrollBars == UIScrollView.ShowCondition.Always;
		if (mVerticalScrollbar != null)
		{
			bool visibility = (flag || pDragPanel.shouldMoveVertically) && inVisible;
			mVerticalScrollbar.SetVisibility(visibility);
		}
		if (mHorizontalScrollbar != null)
		{
			bool visibility2 = (flag || pDragPanel.shouldMoveHorizontally) && inVisible;
			mHorizontalScrollbar.SetVisibility(visibility2);
		}
	}

	protected override void UpdateVisibility(bool inVisible)
	{
		base.UpdateVisibility(inVisible);
		if (inVisible)
		{
			mPanelCachedPosition = Vector3.zero;
			pDragPanel.InvalidateBounds();
		}
		UpdateScrollbars(inVisible);
		UpdateColliderState();
		if (_BackgroundObject != null)
		{
			UIWidget[] componentsInChildren = _BackgroundObject.GetComponentsInChildren<UIWidget>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = inVisible;
			}
		}
		if (_HighlightWidget != null && _HighlightWidget.pAnchor != null && _HighlightWidget.transform.parent != _HighlightWidget.pAnchor.transform)
		{
			_HighlightWidget.SetVisibility(inVisible);
		}
		if (_SelectedWidget != null && _SelectedWidget.transform.parent != _SelectedWidget.pAnchor.transform)
		{
			_SelectedWidget.SetVisibility(inVisible);
		}
		if (mItemInfo.Count > 0 && _ResetItemsOnVisibility)
		{
			mCurrentPage = 1;
			ResetPosition();
		}
	}

	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		base.OnHover(inWidget, inIsHover);
		UpdateHighlightState(inWidget, _HighlightWidget, inIsHover);
	}

	protected void UpdateHighlightState(KAWidget inWidget, KAWidget highlightWidget, bool highlight)
	{
		if (inWidget != null && inWidget.pParentWidget != null)
		{
			inWidget = inWidget.pParentWidget;
		}
		if (!(highlightWidget != null))
		{
			return;
		}
		if (highlight)
		{
			if (inWidget != null)
			{
				highlightWidget.transform.parent = inWidget.transform;
				highlightWidget.transform.position = Vector3.zero;
				highlightWidget.transform.localPosition = new Vector3(0f, 0f, inWidget.transform.localPosition.z);
				highlightWidget.ResetWidget(resetPosition: false);
				highlightWidget.SetVisibility(inVisible: false);
				highlightWidget.SetVisibility(inVisible: true);
			}
		}
		else
		{
			highlightWidget.SetVisibility(inVisible: false);
		}
	}

	public override void OnSelect(bool inSelected)
	{
	}

	public override void OnSelect(KAWidget inWidget, bool inSelected)
	{
		base.OnSelect(inWidget, inSelected);
		if (inWidget != null && inWidget.pParentWidget != null)
		{
			inWidget = inWidget.pParentWidget;
		}
		if (inSelected)
		{
			SetSelectedItem(inWidget);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_ParentUi != null)
		{
			_ParentUi.OnClick(inWidget);
		}
	}

	public override void OnPress(KAWidget inWidget, bool inPressed)
	{
		base.OnPress(inWidget, inPressed);
		if (_AllowDrag)
		{
			pDragPanel.Press(inPressed);
		}
		mCurrentGrid.pDragDelta = Vector2.zero;
		if (_PageFlip && _PageFlipSnapOnRelease && !inPressed)
		{
			Vector3 vector = mPosition - pDragPanel.transform.localPosition;
			int num = Mathf.RoundToInt((IsHorizontalMovement() ? vector.x : (0f - vector.y)) / mPageSize);
			GoToPage(num + 1);
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		if (_AllowDrag)
		{
			Vector3 localPosition = mDragPanel.transform.localPosition;
			pDragPanel.Drag();
			Vector3 relative = localPosition - mDragPanel.transform.localPosition;
			Vector3 normalized = relative.normalized;
			if ((mDragPanel.dragEffect != UIScrollView.DragEffect.MomentumAndSpring && mDragPanel.restrictWithinPanel && ((inDelta.x < 0f && normalized.x < 0f) || (inDelta.x > 0f && normalized.x > 0f))) || (inDelta.y < 0f && normalized.y < 0f) || (inDelta.y > 0f && normalized.y > 0f))
			{
				mDragPanel.MoveRelative(relative);
			}
		}
	}

	public override void OnScroll(KAWidget inWidget, float inScroll)
	{
		base.OnScroll(inWidget, inScroll);
		if (pDragPanel != null)
		{
			pDragPanel.Scroll(inScroll);
		}
	}

	public override void OnSwipe(KAWidget inSwipedWidget, Vector2 inSwipedTotalDelta)
	{
		base.OnSwipe(inSwipedWidget, inSwipedTotalDelta);
		if (_ParentUi != null)
		{
			_ParentUi.OnSwipe(inSwipedWidget, inSwipedTotalDelta);
		}
	}

	public virtual void UnloadItem(KAWidget inWidget)
	{
	}

	public virtual void LoadItem(KAWidget inWidget)
	{
	}

	public virtual void ClearItems()
	{
		ResetHighlightWidgets(null);
		ResetPosition();
		ClearMenu();
		mCurrentPage = 1;
		UpdateScrollbars(inVisible: false);
		UpdatePageNumber();
	}

	protected void ClearMenu()
	{
		foreach (KAWidget item in mItemInfo)
		{
			if (mPool != null && _ItemPoolCount > 0 && mPoolWidgets.Count < _ItemPoolCount)
			{
				item.gameObject.transform.parent = mPool.transform;
				item.ResetWidget();
				item.SetVisibility(inVisible: false);
				item.transform.localPosition = new Vector3(0f, 10000f, 0f);
				mPoolWidgets.Push(item);
			}
			else
			{
				item.transform.parent = null;
				UnityEngine.Object.Destroy(item.gameObject);
			}
		}
		mItemInfo.Clear();
		_OptimizedMenu.Clear();
	}

	private void CleanupWidgetPool()
	{
		List<object> list = new List<object>();
		foreach (object mPoolWidget in mPoolWidgets)
		{
			if (mPoolWidget as KAWidget == null)
			{
				Debug.LogError("Found invalid item in pool");
			}
			else
			{
				list.Add(mPoolWidget);
			}
		}
		if (list.Count == mPoolWidgets.Count)
		{
			return;
		}
		mPoolWidgets.Clear();
		foreach (object item in list)
		{
			mPoolWidgets.Push(item);
		}
	}

	public override KAWidget FindItemAt(int inIndex)
	{
		if (pIsOptimized)
		{
			foreach (KAWidget item in mItemInfo)
			{
				if (item._MenuItemIndex == inIndex)
				{
					return item;
				}
			}
		}
		return base.FindItemAt(inIndex);
	}

	private KAWidget GetPoolWidget()
	{
		if (mPoolWidgets == null || mPoolWidgets.Count == 0)
		{
			return null;
		}
		CleanupWidgetPool();
		return mPoolWidgets.Pop() as KAWidget;
	}

	public int GetPageCount()
	{
		mPageCount = mCurrentGrid.GetPageCount(pPanel);
		return mPageCount;
	}

	public int GetCurrentPage()
	{
		return mCurrentPage;
	}

	public void MoveToNextPage()
	{
		float num = Mathf.Round((float)(mCurrentPage - 1) * mPageSize);
		Vector3 vector = mPosition - pDragPanel.transform.localPosition;
		if (Mathf.Round(IsHorizontalMovement() ? vector.x : (0f - vector.y)) < num)
		{
			GoToPage(mCurrentPage);
		}
		else if (mCurrentPage < mPageCount)
		{
			mCurrentPage++;
			GoToPage(mCurrentPage);
		}
	}

	public void MoveToPreviousPage()
	{
		float num = Mathf.Round((float)(mCurrentPage - 1) * mPageSize);
		Vector3 vector = mPosition - pDragPanel.transform.localPosition;
		if (Mathf.Round(IsHorizontalMovement() ? vector.x : (0f - vector.y)) > num)
		{
			GoToPage(mCurrentPage);
		}
		else if (mCurrentPage > 1)
		{
			mCurrentPage--;
			GoToPage(mCurrentPage);
		}
	}

	protected void UpdatePageCount()
	{
		if (_PageFlip)
		{
			mPageCount = mCurrentGrid.GetPageCount(pPanel);
			if (IsHorizontalMovement())
			{
				mPageSize = pPanel.baseClipRegion.z;
			}
			else
			{
				mPageSize = pPanel.baseClipRegion.w;
			}
			UpdatePageNumber();
		}
	}

	private bool IsHorizontalMovement()
	{
		if (mCurrentGrid.arrangement == UIGrid.Arrangement.Horizontal)
		{
			return true;
		}
		if (mCurrentGrid.arrangement == UIGrid.Arrangement.Vertical)
		{
			return true;
		}
		return false;
	}

	public virtual void GoToPage(int inPageNumber, bool instant = false)
	{
		if (mCurrentGrid.hideInactive)
		{
			UpdatePageCount();
		}
		if (inPageNumber < 1)
		{
			inPageNumber = 1;
		}
		else if (inPageNumber > mPageCount)
		{
			inPageNumber = mPageCount;
		}
		Vector3 localPosition = pDragPanel.transform.localPosition;
		if (IsHorizontalMovement())
		{
			localPosition.x = mPosition.x - (float)(inPageNumber - 1) * mPageSize;
		}
		else
		{
			localPosition.y = mPosition.y + (float)(inPageNumber - 1) * mPageSize;
		}
		mCurrentPage = inPageNumber;
		if (instant)
		{
			Vector3 vector = localPosition - pDragPanel.transform.localPosition;
			Vector4 baseClipRegion = pPanel.baseClipRegion;
			baseClipRegion.x -= vector.x;
			baseClipRegion.y -= vector.y;
			pPanel.baseClipRegion = baseClipRegion;
			pDragPanel.transform.localPosition = localPosition;
			pDragPanel.UpdateScrollbars(recalculateBounds: false);
		}
		else
		{
			mSpringPanel = SpringPanel.Begin(pDragPanel.gameObject, localPosition, _PageSnapStrength);
		}
		if (onPageChange != null)
		{
			onPageChange(inPageNumber);
		}
		UpdatePageNumber();
	}

	public int GetNumItemsPerPage()
	{
		int num = 0;
		num = ((mCurrentGrid.arrangement != UIGrid.Arrangement.Vertical) ? Mathf.RoundToInt(pPanel.baseClipRegion.w / mCurrentGrid.cellHeight) : Mathf.RoundToInt(pPanel.baseClipRegion.z / mCurrentGrid.cellWidth));
		return num * mCurrentGrid.maxPerLine;
	}

	public virtual int GetTopItemIdx()
	{
		if (pDragPanel == null)
		{
			Debug.LogError("Missing KAUIDraggablePanel component");
			return -1;
		}
		int num = 0;
		if (_PageFlip)
		{
			if (mCurrentPage <= mPageCount)
			{
				int numItemsPerPage = GetNumItemsPerPage();
				num = (mCurrentPage - 1) * numItemsPerPage;
			}
		}
		else
		{
			Vector2 scrollValue = pDragPanel.GetScrollValue();
			Vector3[] worldCorners = mPanel.worldCorners;
			if (mCurrentGrid.arrangement == UIGrid.Arrangement.Vertical)
			{
				num = Mathf.RoundToInt(scrollValue.x / mCurrentGrid.cellWidth) * mCurrentGrid.maxPerLine;
			}
			else
			{
				Bounds bounds = mDragPanel.bounds;
				float num2 = base.transform.TransformPoint(bounds.max).y - mCurrentGrid.cellHeight * 0.5f - worldCorners[1].y;
				if (num2 < 0f)
				{
					num2 = 0f;
				}
				num = Mathf.RoundToInt(num2 / mCurrentGrid.cellHeight) * mCurrentGrid.maxPerLine;
			}
		}
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	public virtual void SetTopItemIdxByBounds(int idx)
	{
		int numItemsByBounds = GetNumItemsByBounds();
		int numItemsPerPage = GetNumItemsPerPage();
		if (numItemsByBounds > numItemsPerPage)
		{
			SetTopItemIdxByIdx(idx, numItemsPerPage);
		}
	}

	public virtual void SetTopItemIdx(int idx)
	{
		int numItems = GetNumItems();
		int numItemsPerPage = GetNumItemsPerPage();
		if (idx < numItems && numItems > numItemsPerPage)
		{
			SetTopItemIdxByIdx(idx, numItemsPerPage);
		}
	}

	public virtual void SetTopItemCenterOnChild(KAWidget inWidget)
	{
		if (!(inWidget == null) && _CenterOnItem)
		{
			mCenterOnChild.CenterOn(inWidget.transform);
		}
	}

	public void FocusWidget(KAWidget widget)
	{
		int num = FindItemIndex(widget);
		if (pHortizontalScrollbar != null)
		{
			pHortizontalScrollbar.value = (float)num / (float)GetNumItems();
		}
		else if (pVerticalScrollbar != null)
		{
			pVerticalScrollbar.value = (float)num / (float)GetNumItems();
		}
	}

	private void SetTopItemIdxByIdx(int inIdx, int inNumOfItemPerPage)
	{
		Vector4 baseClipRegion = pPanel.baseClipRegion;
		if (pPanel.clipping == UIDrawCall.Clipping.SoftClip)
		{
			baseClipRegion.z -= pPanel.clipSoftness.x * 2f;
			baseClipRegion.w -= pPanel.clipSoftness.y * 2f;
		}
		int num = inIdx / mCurrentGrid.maxPerLine;
		int numItems = GetNumItems();
		int num2 = numItems / mCurrentGrid.maxPerLine;
		if (numItems % mCurrentGrid.maxPerLine != 0)
		{
			num2++;
		}
		Vector2 vector = default(Vector2);
		if (mCurrentGrid.arrangement == UIGrid.Arrangement.Vertical)
		{
			float num3 = baseClipRegion.z / mCurrentGrid.cellWidth;
			vector.x = Mathf.InverseLerp(0f, (float)num2 - num3, num);
		}
		else
		{
			float num4 = baseClipRegion.w / mCurrentGrid.cellHeight;
			vector.y = Mathf.InverseLerp(0f, (float)num2 - num4, num);
		}
		pDragPanel.SetDragAmount(vector.x, vector.y, updateScrollbars: false);
	}

	public List<KAWidget> GetItemsInView()
	{
		int topItemIdx = GetTopItemIdx();
		if (topItemIdx < 0)
		{
			return null;
		}
		int numItemsPerPage = GetNumItemsPerPage();
		int num = topItemIdx + numItemsPerPage + mCurrentGrid.maxPerLine;
		if (num >= GetNumItems())
		{
			num = GetNumItems();
		}
		List<KAWidget> list = new List<KAWidget>();
		for (int i = topItemIdx; i < num; i++)
		{
			KAWidget kAWidget = FindItemAt(i);
			if (kAWidget != null)
			{
				list.Add(kAWidget);
			}
		}
		return list;
	}

	public virtual void LoadItemsInView()
	{
		List<KAWidget> itemsInView = GetItemsInView();
		if (itemsInView == null)
		{
			return;
		}
		foreach (KAWidget item in mItemsInView)
		{
			if (!itemsInView.Contains(item))
			{
				UnloadItem(item);
			}
		}
		foreach (KAWidget item2 in itemsInView)
		{
			LoadItem(item2);
		}
		mItemsInView = itemsInView;
	}

	protected virtual void OnGridReposition()
	{
		ResetScrollBar();
		ResetPosition();
		_OptimizedMenu.CalculateBounds();
		mPanel.Refresh();
		mViewChanged = true;
		if (mDefaultFocusIndex != -1)
		{
			FocusWidget(GetItemAt(mDefaultFocusIndex));
		}
		mDefaultFocusIndex = -1;
	}

	protected virtual void OnCenterItemFinished()
	{
		CheckClosestWidget();
	}

	protected void CheckClosestWidget()
	{
		if (!_CenterOnItem || mItemInfo.Count <= 0)
		{
			return;
		}
		float num = float.MaxValue;
		KAWidget kAWidget = null;
		Vector4 finalClipRegion = mPanel.finalClipRegion;
		Vector3 vector = new Vector3(finalClipRegion.x + base.transform.position.x, finalClipRegion.y + base.transform.position.y, base.gameObject.transform.position.z);
		for (int i = 0; i < mItemInfo.Count; i++)
		{
			KAWidget kAWidget2 = mItemInfo[i];
			float num2 = Vector3.SqrMagnitude(kAWidget2.transform.position - vector);
			if (num2 < num)
			{
				num = num2;
				kAWidget = kAWidget2;
			}
		}
		if (mSelectedItem != kAWidget)
		{
			OnSelect(kAWidget, inSelected: true);
		}
	}

	protected float GetScrollValue()
	{
		float result = 1f;
		Bounds bounds = mDragPanel.bounds;
		Vector2 vector = bounds.min;
		Vector2 vector2 = bounds.max;
		if (vector2.y > vector.y)
		{
			Vector4 finalClipRegion = mPanel.finalClipRegion;
			float num = finalClipRegion.w * 0.5f;
			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				num -= mPanel.clipSoftness.y;
			}
			float num2 = finalClipRegion.y - num - vector.y;
			float num3 = vector2.y - num - finalClipRegion.y;
			float num4 = vector2.y - vector.y;
			num2 = Mathf.Clamp01(num2 / num4);
			num3 = Mathf.Clamp01(num3 / num4);
			float num5 = num2 + num3;
			result = ((num5 > 0.001f) ? (1f - num2 / num5) : 0f);
		}
		return result;
	}
}
