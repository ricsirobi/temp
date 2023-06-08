using System;
using System.Collections.Generic;
using UnityEngine;

public class KAUI : KAMonoBase
{
	public enum SwipeDirection
	{
		LEFT,
		RIGHT,
		UP,
		DOWN,
		NONE
	}

	public static KAUI _GlobalExclusiveUI = null;

	protected static int mOnClickFrameCount = -1;

	private static KAWidget mGlobalMouseOverItem = null;

	private static GameObject mMask;

	private static UITexture mMaskTexture;

	private static List<ExclusiveUIInfo> mExclusiveUIList = new List<ExclusiveUIInfo>();

	private static string mMaskShader = "Unlit/Transparent Colored";

	public KAUIMenu[] _MenuList;

	public KAUI[] _UiList;

	public string _BackButtonName;

	public KAOrientationUI _OrientationInfo;

	public bool _AllowSafeAreaScale;

	public static Action OnUIDisabled;

	protected bool mParentVisible = true;

	protected TweenPosition mPositionTweener;

	protected KAUIState mStateBeforeSlide;

	private bool? mCachedVisibility;

	private KAUIState mCachedState;

	private KAWidget mCancelBtn;

	private KAWidget mClickedItem;

	private int mCurrentClickedFrame;

	private bool mStarted;

	protected List<KAWidget> mItemInfo = new List<KAWidget>();

	[SerializeField]
	private bool _Visible = true;

	[SerializeField]
	private KAUIState _State;

	[SerializeField]
	protected float _SlideDuration;

	[SerializeField]
	protected Vector2 _SlideInPosition = Vector2.zero;

	[SerializeField]
	protected Vector2 _SlideOutPosition = Vector2.zero;

	protected KAUIEvents mEvents;

	private KAUIManager mUIManager;

	public KAUIEvents pEvents => mEvents;

	public KAUIManager pUIManager => mUIManager;

	protected virtual void Awake()
	{
		mUIManager = KAUIManager.pInstance;
		RegisterEvents();
		UpdateReferences(base.gameObject, add: true);
	}

	public static void UpdateReferences(GameObject obj, bool add)
	{
		UIWidget component = obj.GetComponent<UIWidget>();
		if (component != null)
		{
			UpdateReferences(component, add);
			return;
		}
		KAWidget component2 = obj.GetComponent<KAWidget>();
		if (component2 != null)
		{
			if (component2.pReferenceAdded == add)
			{
				return;
			}
			component2.pReferenceAdded = add;
			if (component2._TooltipInfo != null && component2._TooltipInfo._Atlas != null)
			{
				if (add)
				{
					component2._TooltipInfo._Atlas = AtlasManager.AddReference(component2._TooltipInfo._Atlas);
				}
				else
				{
					AtlasManager.RemoveReference(component2._TooltipInfo._Atlas);
				}
			}
		}
		KAScrollBar component3 = obj.GetComponent<KAScrollBar>();
		if (component3 != null)
		{
			if (component3.pReferenceAdded == add)
			{
				return;
			}
			component3.pReferenceAdded = add;
		}
		KATooltip component4 = obj.GetComponent<KATooltip>();
		if (component4 != null)
		{
			if (component4.pReferenceAdded == add)
			{
				return;
			}
			component4.pReferenceAdded = add;
		}
		foreach (Transform item in obj.transform)
		{
			UpdateReferences(item.gameObject, add);
		}
	}

	private static void UpdateReferences(UIWidget widget, bool add)
	{
		if (widget == null)
		{
			return;
		}
		if (add)
		{
			if (widget is UILabel)
			{
				FontManager.AddReference(widget as UILabel);
			}
			if (widget is UISprite)
			{
				UISprite obj = (UISprite)widget;
				obj.atlas = AtlasManager.AddReference(obj.atlas);
			}
			if (widget is UITexture)
			{
				AtlasManager.AddReference(widget as UITexture);
			}
		}
		else
		{
			if (widget is UILabel)
			{
				FontManager.RemoveReference(widget as UILabel);
			}
			if (widget is UISprite)
			{
				AtlasManager.RemoveReference((widget as UISprite).atlas);
			}
			if (widget is UITexture)
			{
				AtlasManager.RemoveReference(widget as UITexture);
			}
		}
	}

	protected virtual void Start()
	{
		mStarted = true;
		SetVisibility(_Visible);
		SetState(_State);
		if (!string.IsNullOrEmpty(_BackButtonName))
		{
			mCancelBtn = FindItem(_BackButtonName);
		}
		OnOrientation(KAUIManager.IsPortrait());
		if (_AllowSafeAreaScale)
		{
			float safeAreaHeightRatio = UtMobileUtilities.GetSafeAreaHeightRatio();
			base.transform.localScale -= new Vector3(safeAreaHeightRatio, safeAreaHeightRatio);
		}
	}

	protected virtual void Update()
	{
		if (mCachedVisibility.HasValue && mCachedVisibility.Value != _Visible)
		{
			SetVisibility(_Visible);
		}
		if (mCachedState != _State)
		{
			SetState(_State);
		}
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			if (mOnClickFrameCount != Time.frameCount && mCancelBtn != null && (_GlobalExclusiveUI == null || _GlobalExclusiveUI == this) && IsActive() && mCancelBtn.IsActive())
			{
				mOnClickFrameCount = Time.frameCount;
				mEvents.ProcessClickEvent(mCancelBtn);
			}
			if (!UtUtilities.IsKeyboardAttached() && UICamera.currentScheme != UICamera.ControlScheme.Touch)
			{
				UICamera.currentScheme = UICamera.ControlScheme.Touch;
				UICamera.hoveredObject = UICamera.fallThrough;
			}
		}
		if (KAInput.pInstance != null && KAInput.pInstance.pInputMode == KAInputMode.TOUCH && Input.touchCount == 0)
		{
			mGlobalMouseOverItem = null;
		}
	}

	public string GetName()
	{
		return GetType().ToString();
	}

	protected void RegisterEvents()
	{
		mEvents = new KAUIEvents();
		mEvents.OnHover += OnHover;
		mEvents.OnPressRepeated += OnPressRepeated;
		mEvents.OnPress += OnPress;
		mEvents.OnClick += OnClick;
		mEvents.OnTooltip += OnTooltip;
		mEvents.OnInput += OnInput;
		mEvents.OnSelect += OnSelect;
		mEvents.OnDrag += OnDrag;
		mEvents.OnSubmit += OnSubmit;
		mEvents.OnScroll += OnScroll;
		mEvents.OnDoubleClick += OnDoubleClick;
		mEvents.OnDrop += OnDrop;
		mEvents.OnSwipe += OnSwipe;
		mEvents.OnDragStart += OnDragStart;
		mEvents.OnDragEnd += OnDragEnd;
		if (KAUIManager.IsAutoRotate())
		{
			KAUIManager.OnOrientation += OnOrientation;
		}
	}

	protected void UnregisterEvents()
	{
		if (mEvents != null)
		{
			mEvents.Clear();
			mEvents = null;
			if (KAUIManager.IsAutoRotate())
			{
				KAUIManager.OnOrientation -= OnOrientation;
			}
		}
	}

	public virtual void OnHover(KAWidget inWidget, bool inIsHover)
	{
		if (!IsActive() || inWidget == null)
		{
			if (inWidget != null)
			{
				inWidget.OnHover(inIsHover: false);
			}
			return;
		}
		if (inIsHover)
		{
			mGlobalMouseOverItem = inWidget;
		}
		else
		{
			mGlobalMouseOverItem = null;
		}
		inWidget.OnHover(inIsHover);
	}

	public virtual void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		if (IsActive() && !(inWidget == null))
		{
			inWidget.OnPressRepeated(inPressed);
		}
	}

	public virtual void OnPress(KAWidget inWidget, bool inPressed)
	{
		if (!IsActive() || inWidget == null)
		{
			if (inWidget != null)
			{
				inWidget.OnPress(inPressed: false);
			}
			return;
		}
		if (KAInput.pInstance.IsTouchInput())
		{
			mGlobalMouseOverItem = inWidget;
			if (!inPressed)
			{
				mGlobalMouseOverItem = null;
			}
		}
		inWidget.OnPress(inPressed);
	}

	public virtual void OnClick(KAWidget inWidget)
	{
		if (IsActive() && !(inWidget == null))
		{
			if (KAInput.pInstance.IsTouchInput())
			{
				mGlobalMouseOverItem = null;
			}
			SetClickedItem(inWidget);
			if (inWidget.pUI == this)
			{
				inWidget.OnClick();
			}
			mOnClickFrameCount = Time.frameCount;
		}
	}

	public virtual void OnDragStart(KAWidget inWidget)
	{
		if (IsActive() && !(inWidget == null) && inWidget.pUI == this)
		{
			inWidget.OnDragStart();
		}
	}

	public virtual void OnDragEnd(KAWidget inWidget)
	{
		if (IsActive() && !(inWidget == null) && inWidget.pUI == this)
		{
			inWidget.OnDragEnd();
		}
	}

	public virtual void OnTooltip(KAWidget inWidget, bool inShow)
	{
		if (IsActive() && !(inWidget == null))
		{
			inWidget.OnTooltip(inShow);
		}
	}

	public virtual void OnInput(KAWidget inWidget, string inText)
	{
		if (IsActive() && !(inWidget == null))
		{
			inWidget.OnInput(inText);
		}
	}

	public virtual void OnSelect(bool inSelected)
	{
	}

	public virtual void OnSelect(KAWidget inWidget, bool inSelected)
	{
		if (IsActive() && !(inWidget == null))
		{
			inWidget.OnSelect(inSelected);
		}
	}

	public virtual void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		if (IsActive() && !(inWidget == null))
		{
			inWidget.OnDrag(inDelta);
		}
	}

	public virtual void OnScroll(KAWidget inWidget, float inScroll)
	{
	}

	public virtual void OnSubmit(KAWidget inWidget)
	{
		if (IsActive() && !(inWidget == null))
		{
			inWidget.OnSubmit();
		}
	}

	public virtual void OnDoubleClick(KAWidget inWidget)
	{
		if (IsActive() && !(inWidget == null))
		{
			SetClickedItem(inWidget);
			inWidget.OnDoubleClick();
		}
	}

	public virtual void OnDrop(KAWidget inDroppedWidget, KAWidget inTargetWidget)
	{
		if (IsActive() && !(inDroppedWidget == null) && !(inTargetWidget == null))
		{
			inDroppedWidget.OnDrop();
		}
	}

	private void ApplyUIData(KATransformData transformData)
	{
		base.transform.localPosition = transformData._LocalPosition;
		base.transform.localRotation = Quaternion.Euler(transformData._LocalRotation);
		base.transform.localScale = transformData._LocalScale;
	}

	public virtual void OnSwipe(KAWidget inSwipedWidget, Vector2 inSwipedTotalDelta)
	{
		if (IsActive() && !(inSwipedWidget == null))
		{
			inSwipedWidget.OnSwipe();
		}
	}

	public virtual void SetSelectedItem(KAUIMenu inMenu, KAWidget inWidget)
	{
	}

	public virtual void OnOrientation(bool isPortrait)
	{
		if (_OrientationInfo != null)
		{
			if (isPortrait)
			{
				if (_OrientationInfo._Portrait._SmallScreenData._Apply && KAUIManager.IsSmallScreen())
				{
					ApplyUIData(_OrientationInfo._Portrait._SmallScreenData);
				}
				else if (_OrientationInfo._Portrait._OrientationData._Apply)
				{
					ApplyUIData(_OrientationInfo._Portrait._OrientationData);
				}
			}
			else if (_OrientationInfo._Landscape._SmallScreenData._Apply && KAUIManager.IsSmallScreen())
			{
				ApplyUIData(_OrientationInfo._Landscape._SmallScreenData);
			}
			else if (_OrientationInfo._Landscape._OrientationData._Apply)
			{
				ApplyUIData(_OrientationInfo._Landscape._OrientationData);
			}
		}
		foreach (KAWidget item in mItemInfo)
		{
			item.UpdateOrientation(isPortrait);
		}
	}

	public KAWidget DuplicateWidget(string widgetName, UIAnchor.Side anchorSide = UIAnchor.Side.Center)
	{
		KAWidget kAWidget = FindItem(widgetName);
		if (kAWidget != null)
		{
			return DuplicateWidget(kAWidget, anchorSide);
		}
		return null;
	}

	public KAWidget DuplicateWidget(KAWidget inItem, UIAnchor.Side anchorSide = UIAnchor.Side.Center)
	{
		EnableTemplateColliders(inItem, enable: true);
		GameObject obj = base.gameObject.AddChild(inItem.gameObject);
		KAWidget component = obj.GetComponent<KAWidget>();
		obj.name = inItem.name;
		if (component != null)
		{
			AssignParent(component);
			if (!MoveWidgetToAnchor(component, anchorSide))
			{
				component.transform.parent = base.transform;
			}
		}
		EnableTemplateColliders(inItem, enable: false);
		return component;
	}

	private void EnableTemplateColliders(KAWidget widget, bool enable)
	{
		Collider[] componentsInChildren = widget.GetComponentsInChildren<Collider>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			KAWidget component = componentsInChildren[i].GetComponent<KAWidget>();
			if (!enable && component != null)
			{
				componentsInChildren[i].enabled = component.GetVisibility();
			}
			else
			{
				componentsInChildren[i].enabled = enable;
			}
		}
	}

	public void AssignParent(KAWidget widget)
	{
		widget.pUI = this;
		List<KAWidget> pChildWidgets = widget.pChildWidgets;
		if (pChildWidgets == null)
		{
			return;
		}
		foreach (KAWidget item in pChildWidgets)
		{
			AssignParent(item);
		}
	}

	public static KAWidget GetGlobalMouseOverItem()
	{
		return mGlobalMouseOverItem;
	}

	public virtual KAWidget GetClickedItem()
	{
		if (mCurrentClickedFrame == Time.frameCount)
		{
			return mClickedItem;
		}
		return null;
	}

	public void SetClickedItem(KAWidget item)
	{
		mCurrentClickedFrame = Time.frameCount;
		mClickedItem = item;
	}

	public virtual bool MoveWidgetToAnchor(KAWidget inItem, UIAnchor.Side anchorSide, bool createIfNotFound = false)
	{
		UIAnchor[] componentsInChildren = base.gameObject.GetComponentsInChildren<UIAnchor>();
		if (componentsInChildren == null)
		{
			return false;
		}
		UIAnchor[] array = componentsInChildren;
		foreach (UIAnchor uIAnchor in array)
		{
			if (uIAnchor != null && uIAnchor.side == anchorSide && uIAnchor.transform.parent == base.transform)
			{
				inItem.transform.parent = uIAnchor.transform;
				return true;
			}
		}
		if (createIfNotFound)
		{
			GameObject gameObject = new GameObject();
			gameObject.layer = base.gameObject.layer;
			gameObject.name = "Anchor-" + anchorSide;
			gameObject.transform.parent = base.transform;
			(gameObject.AddComponent(typeof(UIAnchor)) as UIAnchor).side = anchorSide;
			inItem.transform.parent = gameObject.transform;
			return true;
		}
		return false;
	}

	public virtual void AddWidget(KAWidget inItem, UIAnchor.Side anchorSide)
	{
		AddWidget(inItem);
		MoveWidgetToAnchor(inItem, anchorSide);
	}

	public virtual void AddWidget(KAWidget inItem)
	{
		if (inItem.pUI != null)
		{
			inItem.pUI.mItemInfo.Remove(inItem);
		}
		mItemInfo.Add(inItem);
		inItem.pUI = this;
		AssignParent(inItem);
		if (mStarted)
		{
			inItem.OnParentSetVisibility(_Visible);
		}
	}

	public virtual void AddWidgetAt(int index, KAWidget inItem, UIAnchor.Side anchorSide)
	{
		AddWidgetAt(index, inItem);
		MoveWidgetToAnchor(inItem, anchorSide);
	}

	public virtual void AddWidgetAt(int index, KAWidget inItem)
	{
		mItemInfo.Insert(index, inItem);
		inItem.pUI = this;
		AssignParent(inItem);
	}

	public virtual KAWidget AddWidget(Type inType, Texture2D inTexture, Shader inShader, string inWidgetName)
	{
		GameObject gameObject = CreateWidget(inTexture, inShader);
		gameObject.name = inWidgetName;
		KAWidget kAWidget = null;
		kAWidget = ((!(inType == typeof(KAButton))) ? gameObject.AddComponent<KAWidget>() : gameObject.AddComponent<KAButton>());
		AddWidget(kAWidget);
		return kAWidget;
	}

	public virtual void RemoveWidget(KAWidget inWidget)
	{
		mItemInfo.Remove(inWidget);
		UnityEngine.Object.Destroy(inWidget.gameObject);
	}

	private GameObject CreateWidget(Texture2D inTexture, Shader inShader)
	{
		Material material = new Material(inShader);
		GameObject obj = new GameObject();
		obj.layer = base.transform.gameObject.layer;
		obj.transform.parent = base.transform;
		obj.AddComponent<BoxCollider>().size = new Vector3(1f, 1f, 0f);
		UITexture uITexture = obj.AddWidget<UITexture>();
		material.mainTexture = inTexture;
		uITexture.material = material;
		uITexture.gameObject.transform.localScale = Vector3.one;
		return obj;
	}

	public KAUIMenu GetMenuByIndex(int index)
	{
		if (_MenuList != null && index < _MenuList.Length)
		{
			return _MenuList[index];
		}
		return null;
	}

	public KAUIMenu GetMenu(string menuClassName)
	{
		if (_MenuList == null)
		{
			return null;
		}
		KAUIMenu[] menuList = _MenuList;
		foreach (KAUIMenu kAUIMenu in menuList)
		{
			if (kAUIMenu.GetName().Equals(menuClassName))
			{
				return kAUIMenu;
			}
		}
		return null;
	}

	public void SelectMenu(KAUIMenu inMenu)
	{
		if (inMenu == null || _MenuList == null)
		{
			return;
		}
		KAUIMenu[] menuList = _MenuList;
		foreach (KAUIMenu kAUIMenu in menuList)
		{
			if (kAUIMenu != inMenu)
			{
				kAUIMenu.SetVisibility(inVisible: false);
			}
		}
		inMenu.SetVisibility(inVisible: true);
	}

	public virtual KAUIState GetState()
	{
		return _State;
	}

	public virtual void SetState(KAUIState inState)
	{
		switch (inState)
		{
		case KAUIState.DISABLED:
			EnableCollider(inEnable: false);
			break;
		case KAUIState.INTERACTIVE:
			EnableCollider(inEnable: true);
			break;
		case KAUIState.NOT_INTERACTIVE:
			EnableCollider(inEnable: false);
			break;
		}
		_State = inState;
		mCachedState = _State;
		if (_MenuList != null)
		{
			KAUIMenu[] menuList = _MenuList;
			for (int i = 0; i < menuList.Length; i++)
			{
				menuList[i]?.SetState(inState);
			}
		}
		if (_UiList != null)
		{
			KAUI[] uiList = _UiList;
			for (int i = 0; i < uiList.Length; i++)
			{
				uiList[i]?.SetState(inState);
			}
		}
		foreach (KAWidget item in mItemInfo)
		{
			if (item != null)
			{
				item.OnParentSetState(inState);
			}
		}
	}

	private void EnableCollider(bool inEnable)
	{
		if (collider != null)
		{
			collider.enabled = inEnable;
		}
	}

	public int GetItemCount()
	{
		if (mItemInfo != null)
		{
			return mItemInfo.Count;
		}
		return 0;
	}

	public virtual bool GetVisibility()
	{
		if (_Visible)
		{
			return mParentVisible;
		}
		return false;
	}

	public virtual void OnParentSetVisiblity(bool inVisible)
	{
		mParentVisible = inVisible;
		UpdateVisibility(mParentVisible && _Visible);
	}

	protected virtual void UpdateVisibility(bool inVisible)
	{
		foreach (KAWidget item in mItemInfo)
		{
			if (item != null)
			{
				item.OnParentSetVisibility(inVisible);
			}
		}
		if (_MenuList != null)
		{
			KAUIMenu[] menuList = _MenuList;
			for (int i = 0; i < menuList.Length; i++)
			{
				menuList[i]?.OnParentSetVisiblity(inVisible);
			}
		}
		if (_UiList != null)
		{
			KAUI[] uiList = _UiList;
			for (int i = 0; i < uiList.Length; i++)
			{
				uiList[i]?.OnParentSetVisiblity(inVisible);
			}
		}
		if (!inVisible && mGlobalMouseOverItem != null && mGlobalMouseOverItem.pUI == this)
		{
			mGlobalMouseOverItem = null;
		}
	}

	public virtual void SetVisibility(bool inVisible)
	{
		if (!mCachedVisibility.HasValue || mCachedVisibility.Value != inVisible)
		{
			_Visible = inVisible;
			mCachedVisibility = _Visible;
			UpdateVisibility(mParentVisible && _Visible);
		}
	}

	private void OnDisable()
	{
		if (mGlobalMouseOverItem != null && mGlobalMouseOverItem.pUI == this)
		{
			mGlobalMouseOverItem = null;
		}
	}

	public int GetPriority()
	{
		UIPanel component = GetComponent<UIPanel>();
		if (component != null)
		{
			return component.depth;
		}
		return -1;
	}

	public void SetPriority(int inPriority)
	{
		UIPanel component = GetComponent<UIPanel>();
		if (component != null)
		{
			component.depth = inPriority;
		}
	}

	public bool IsActive()
	{
		if (_State == KAUIState.INTERACTIVE)
		{
			return _Visible;
		}
		return false;
	}

	public static void SetExclusive(KAUI iFace, bool updatePriority = false)
	{
		SetExclusive(iFace, new Color(0.5f, 0.5f, 0.5f, 0.5f), updatePriority);
	}

	public static void SetExclusive(KAUI iFace, Texture2D inMaskTexture, bool updatePriority = false)
	{
		SetExclusive(iFace, inMaskTexture, Color.white, updatePriority);
	}

	public static void SetExclusive(KAUI iFace, Color inMaskColor, bool updatePriority = false)
	{
		SetExclusive(iFace, null, inMaskColor, updatePriority);
	}

	public static void SetExclusive(KAUI iFace, Texture2D inMaskTexture, Color inMaskColor, bool updatePriority = false)
	{
		if (iFace == null)
		{
			UtDebug.LogError("SetExclusive:Interface reference is not valid");
			return;
		}
		mExclusiveUIList.RemoveAll((ExclusiveUIInfo uiInfo) => uiInfo._UI == null);
		ExclusiveUIInfo exclusiveUIInfo = mExclusiveUIList.Find((ExclusiveUIInfo uiInfo) => uiInfo._UI == iFace);
		if (exclusiveUIInfo == null)
		{
			exclusiveUIInfo = new ExclusiveUIInfo(iFace, inMaskTexture, inMaskColor);
			if (updatePriority)
			{
				int count = mExclusiveUIList.Count;
				if (count > 0 && exclusiveUIInfo._UI.GetPriority() <= mExclusiveUIList[count - 1]._UI.GetPriority())
				{
					exclusiveUIInfo._UI.SetPriority(mExclusiveUIList[count - 1]._UI.GetPriority() + 1);
				}
			}
			mExclusiveUIList.Add(exclusiveUIInfo);
		}
		ShowMaskTexture(inShow: true, exclusiveUIInfo);
		_GlobalExclusiveUI = exclusiveUIInfo._UI;
	}

	public static void RemoveExclusive(KAUI iFace)
	{
		if (iFace == null)
		{
			UtDebug.LogError("RemoveExclusive:Interface reference is not valid");
		}
		else if (mExclusiveUIList.Count > 0)
		{
			mExclusiveUIList.RemoveAll((ExclusiveUIInfo uiInfo) => uiInfo._UI == null);
			ExclusiveUIInfo exclusiveUIInfo = mExclusiveUIList.Find((ExclusiveUIInfo uiInfo) => uiInfo._UI == iFace);
			if (exclusiveUIInfo != null)
			{
				mExclusiveUIList.Remove(exclusiveUIInfo);
			}
			if (mExclusiveUIList.Count > 0)
			{
				ExclusiveUIInfo exclusiveUIInfo2 = mExclusiveUIList[mExclusiveUIList.Count - 1];
				ShowMaskTexture(inShow: true, exclusiveUIInfo2);
				_GlobalExclusiveUI = exclusiveUIInfo2._UI;
			}
			else if (mExclusiveUIList.Count == 0)
			{
				ShowMaskTexture(inShow: false, null);
				_GlobalExclusiveUI = null;
			}
		}
	}

	private static void ShowMaskTexture(bool inShow, ExclusiveUIInfo exclusiveInfo)
	{
		UIPanel uIPanel = null;
		if (mMask == null)
		{
			mMask = new GameObject("PfMask");
			mMask.transform.localPosition = Vector3.zero;
			GameObject obj = new GameObject("Background");
			obj.transform.parent = mMask.transform;
			obj.transform.localPosition = Vector3.zero;
			mMaskTexture = obj.AddComponent<UITexture>();
			Material material = new Material(Shader.Find(mMaskShader));
			mMaskTexture.material = material;
			obj.AddComponent<KAUIStretch>()._ApplyScaleToCollider = true;
			NGUITools.AddWidgetCollider(obj);
			obj.GetComponent<BoxCollider>().center = Vector3.zero;
			uIPanel = mMask.AddComponent<UIPanel>();
		}
		else
		{
			uIPanel = mMask.GetComponent<UIPanel>();
		}
		mMask.SetActive(value: false);
		mMaskTexture.enabled = inShow;
		if (inShow && exclusiveInfo != null && exclusiveInfo._UI != null)
		{
			int layer = exclusiveInfo._UI.gameObject.layer;
			UtUtilities.SetLayerRecursively(mMask, layer);
			mMask.transform.parent = exclusiveInfo._UI.transform;
			mMask.transform.localPosition = Vector3.zero;
			float num = 1f / (1f - UtMobileUtilities.GetSafeAreaHeightRatio());
			mMask.transform.localScale = new Vector3(1f + num, 1f + num, 1f);
			uIPanel.depth = exclusiveInfo._UI.GetPriority() - 1;
			Texture2D texture2D = null;
			if (exclusiveInfo._Texture == null)
			{
				texture2D = new Texture2D(8, 8, TextureFormat.ARGB32, mipChain: false);
				for (int i = 0; i < texture2D.width; i++)
				{
					for (int j = 0; j < texture2D.height; j++)
					{
						texture2D.SetPixel(i, j, exclusiveInfo._Color);
					}
				}
				texture2D.Apply();
			}
			else
			{
				texture2D = exclusiveInfo._Texture;
			}
			mMaskTexture.GetComponent<KAUIStretch>()._Style = KAUIStretch.Style.Both;
			mMaskTexture.mainTexture = texture2D;
			mMaskTexture.color = exclusiveInfo._Color;
			mMaskTexture.MarkAsChanged();
		}
		uIPanel.enabled = inShow;
		mMask.SetActive(value: true);
		Collider component = mMaskTexture.GetComponent<Collider>();
		if (component != null)
		{
			component.enabled = inShow;
		}
	}

	public virtual KAWidget FindItem(string inWidgetName, bool recursive = true)
	{
		foreach (KAWidget item in mItemInfo)
		{
			if (item == null)
			{
				continue;
			}
			if (item.name == inWidgetName)
			{
				return item;
			}
			if (recursive)
			{
				KAWidget kAWidget = item.FindChildItem(inWidgetName, ShowWarning: false);
				if (kAWidget != null)
				{
					return kAWidget;
				}
			}
		}
		UtDebug.LogWarning("FindItem can't find item '" + inWidgetName + "'", 100);
		return null;
	}

	public virtual KAWidget FindItemAt(int inIndex)
	{
		if (inIndex >= 0 && inIndex < mItemInfo.Count)
		{
			return mItemInfo[inIndex];
		}
		return null;
	}

	public int FindItemIndex(KAWidget item)
	{
		return mItemInfo.IndexOf(item);
	}

	public void SlideIn()
	{
		SlideTo(_SlideInPosition, _SlideDuration);
	}

	public void SlideOut()
	{
		SlideTo(_SlideOutPosition, _SlideDuration);
	}

	public TweenPosition SlideTo(Vector2 start, Vector2 end, float duration, EventDelegate.Callback onFinishedCallback = null)
	{
		Vector3 localPosition = new Vector3(start.x, start.y, base.transform.localPosition.z);
		base.transform.localPosition = localPosition;
		return SlideTo(end, duration, onFinishedCallback);
	}

	public TweenPosition SlideTo(Vector2 end, float duration, EventDelegate.Callback onFinishedCallback = null)
	{
		mStateBeforeSlide = _State;
		Vector3 pos = new Vector3(end.x, end.y, base.transform.localPosition.z);
		SetState(KAUIState.NOT_INTERACTIVE);
		mPositionTweener = TweenPosition.Begin(base.gameObject, duration, pos);
		mPositionTweener.eventReceiver = base.gameObject;
		mPositionTweener.callWhenFinished = "OnSlideEnd";
		if (onFinishedCallback != null)
		{
			EventDelegate.Add(mPositionTweener.onFinished, onFinishedCallback);
		}
		return mPositionTweener;
	}

	public virtual void OnSlideEnd()
	{
		SetState(mStateBeforeSlide);
	}

	public virtual void SetInteractive(bool interactive)
	{
		if (interactive)
		{
			SetState(KAUIState.INTERACTIVE);
		}
		else
		{
			SetState(KAUIState.NOT_INTERACTIVE);
		}
	}

	public virtual void OnAnimEnd(KAWidget widget, int idx)
	{
	}

	public virtual void EndScaleTo(KAWidget widget)
	{
	}

	public virtual void EndMoveTo(KAWidget widget)
	{
	}

	public float GetMaxZUsedByWidgets()
	{
		float num = -100f;
		foreach (KAWidget item in mItemInfo)
		{
			if (item.transform.localPosition.z > num)
			{
				num = item.transform.localPosition.z;
			}
		}
		return num;
	}

	public float GetMaxZUsedByUIWidgets()
	{
		float num = 0f;
		foreach (KAWidget item in mItemInfo)
		{
			if (item.GetMaxZ() > num)
			{
				num = item.GetMaxZ();
			}
		}
		return num;
	}

	protected virtual void OnDestroy()
	{
		UnregisterEvents();
		UpdateReferences(base.gameObject, add: false);
	}

	public static int GetOnClickFrameCount()
	{
		return mOnClickFrameCount;
	}

	public static SwipeDirection GetDirection(Vector2 dist)
	{
		float num = 30f;
		if (Math.Max(Mathf.Abs(dist.x), Mathf.Abs(dist.y)) > num)
		{
			if (Mathf.Abs(dist.x) >= Mathf.Abs(dist.y))
			{
				if (dist.x > 0f)
				{
					return SwipeDirection.RIGHT;
				}
				return SwipeDirection.LEFT;
			}
			if (dist.y > 0f)
			{
				return SwipeDirection.UP;
			}
			return SwipeDirection.DOWN;
		}
		return SwipeDirection.NONE;
	}

	public virtual void OnShowUITooltip(KATooltip inTooltip, KAWidget inWidget, bool inShow)
	{
	}

	public virtual KAWidget CreateDragObject(KAWidget sourceWidget, int panelDepth)
	{
		KAWidget kAWidget = DuplicateWidget(sourceWidget);
		kAWidget.transform.position = sourceWidget.transform.position;
		kAWidget.SetUserData(sourceWidget.GetUserData());
		kAWidget.transform.parent = null;
		kAWidget.gameObject.SetActive(value: false);
		kAWidget.AttachToCursor(0f, 0f);
		kAWidget.gameObject.AddComponent<UIPanel>();
		kAWidget.gameObject.GetComponent<UIPanel>().depth = panelDepth;
		kAWidget.gameObject.SetActive(value: true);
		kAWidget.SetInteractive(isInteractive: false);
		UICamera.selectedObject = kAWidget.gameObject;
		KAUIManager.pInstance.pDragItem = kAWidget;
		return kAWidget;
	}
}
