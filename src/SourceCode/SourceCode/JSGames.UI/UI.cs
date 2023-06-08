using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
[DisallowMultipleComponent]
public class UI : UIBase
{
	public static UI _GlobalExclusiveUI = null;

	public UIButton _BackButton;

	protected List<UI> mChildUIs = new List<UI>();

	private UIManager mUIManager;

	[NonSerialized]
	private bool mInitialized;

	private bool mOnInitializeCalled;

	private int mNotInteractiveRequests;

	private Dictionary<string, HashSet<UIToggleButton>> mToggleButtonGroups = new Dictionary<string, HashSet<UIToggleButton>>();

	protected static int mOnClickFrameCount = -1;

	public object pCustomData { get; set; }

	public Canvas pCanvas { get; private set; }

	public CanvasScaler pCanvasScaler { get; private set; }

	public float pScaleFactor
	{
		get
		{
			return pCanvas.scaleFactor;
		}
		set
		{
			pCanvas.scaleFactor = value;
		}
	}

	public List<UI> pChildUIs => mChildUIs;

	public UIEvents pEventReceiver { get; protected set; }

	public virtual void AddWidget(UIWidget widget)
	{
		if (widget.pParentWidget == null && widget.pParentUI != null)
		{
			widget.pParentUI.RemoveWidget(widget, destroy: false, removeReferences: false);
		}
		else if (widget.pParentWidget != null)
		{
			widget.pParentWidget.RemoveWidget(widget, destroy: false, removeReferences: false);
		}
		mChildWidgets.Add(widget);
		widget.transform.SetParent(_ProxyTransform);
		widget.pEventTarget = pEventReceiver;
		widget.Initialize(this, null);
	}

	public virtual void RemoveWidget(UIWidget widget, bool destroy = false, bool removeReferences = true)
	{
		if (widget.pParentWidget == null && widget.pParentUI == this)
		{
			mChildWidgets.Remove(widget);
			if (destroy)
			{
				widget.transform.SetParent(null);
				UnityEngine.Object.Destroy(widget.gameObject);
			}
			else if (removeReferences)
			{
				widget.transform.SetParent(null);
				widget.pEventTarget = null;
				widget.Initialize(null, null);
			}
		}
	}

	public int FindUIIndex(UI ui)
	{
		return mChildUIs.IndexOf(ui);
	}

	public UI GetUIAt(int index)
	{
		if (index < mChildUIs.Count)
		{
			return mChildUIs[index];
		}
		return null;
	}

	public void AddUI(UI ui)
	{
		if (ui.pParentUI != null)
		{
			ui.pParentUI.RemoveUI(ui);
		}
		mChildUIs.Add(ui);
		ui.transform.SetParent(_ProxyTransform);
		ui.pEventTarget = pEventReceiver;
		ui.Initialize(this);
		ui.TriggerOnInitializeRecursive();
	}

	public void RemoveUI(UI ui, bool destroy = false)
	{
		if (ui.pParentUI == this)
		{
			mChildUIs.Remove(ui);
			if (destroy)
			{
				ui.transform.SetParent(null);
				UnityEngine.Object.Destroy(ui.gameObject);
			}
			else
			{
				ui.transform.SetParent(null);
				ui.pEventTarget = null;
				ui.Initialize(null);
			}
		}
	}

	public virtual void ClearChildren()
	{
		foreach (UIWidget mChildWidget in mChildWidgets)
		{
			mChildWidget.transform.SetParent(null);
			UnityEngine.Object.Destroy(mChildWidget.gameObject);
		}
		mChildWidgets.Clear();
		foreach (UI mChildUI in mChildUIs)
		{
			mChildUI.transform.SetParent(null);
			UnityEngine.Object.Destroy(mChildUI.gameObject);
		}
		mChildUIs.Clear();
	}

	public void RegisterToggleButtonInGroup(string groupName, UIToggleButton toggleButton)
	{
		if (mToggleButtonGroups.TryGetValue(groupName, out var value))
		{
			value.Add(toggleButton);
			return;
		}
		value = new HashSet<UIToggleButton>();
		value.Add(toggleButton);
		mToggleButtonGroups[groupName] = value;
	}

	public void UnregisterToggleButtonInGroup(string groupName, UIToggleButton toggleButton)
	{
		if (mToggleButtonGroups.TryGetValue(groupName, out var value))
		{
			value.Remove(toggleButton);
		}
		else
		{
			Debug.LogWarning("Toggle Button group name not found: " + groupName);
		}
	}

	public void UncheckOtherToggleButtonsInGroup(string groupName, UIToggleButton otherThan)
	{
		if (!mToggleButtonGroups.TryGetValue(groupName, out var value))
		{
			return;
		}
		foreach (UIToggleButton item in value)
		{
			if (item != otherThan)
			{
				item.pChecked = false;
			}
		}
	}

	public UIToggleButton GetCheckedToggleButtonInGroup(string groupName)
	{
		if (mToggleButtonGroups.TryGetValue(groupName, out var value))
		{
			foreach (UIToggleButton item in value)
			{
				if (item.pChecked)
				{
					return item;
				}
			}
		}
		return null;
	}

	protected virtual void OnInitialize()
	{
	}

	protected virtual void OnAnimEnd(UIWidget widget, int animIdx)
	{
	}

	protected virtual void OnDrag(UIWidget widget, PointerEventData eventData)
	{
	}

	protected virtual void OnDrop(UIWidget widget, PointerEventData eventData)
	{
	}

	protected virtual void OnBeginDrag(UIWidget widget, PointerEventData eventData)
	{
	}

	protected virtual void OnEndDrag(UIWidget widget, PointerEventData eventData)
	{
	}

	protected virtual void OnClick(UIWidget widget, PointerEventData eventData)
	{
		mOnClickFrameCount = Time.frameCount;
	}

	protected virtual void OnPress(UIWidget widget, bool isPressed, PointerEventData eventData)
	{
	}

	protected virtual void OnPressRepeated(UIWidget widget)
	{
	}

	protected virtual void OnHover(UIWidget widget, bool isHovering, PointerEventData eventData)
	{
	}

	protected virtual void OnEndEdit(UIEditBox editBox, string text)
	{
	}

	protected virtual void OnValueChanged(UIEditBox editBox, string text)
	{
	}

	protected virtual void OnCheckedChanged(UIToggleButton toggleButton, bool isChecked)
	{
	}

	protected virtual void OnSelected(UIWidget widget, UI fromUI)
	{
	}

	protected virtual void Initialize(UI parentUI)
	{
		if (!mInitialized)
		{
			mInitialized = true;
			base.pRectTransform = base.transform as RectTransform;
			if (_ProxyTransform == null)
			{
				_ProxyTransform = base.pRectTransform;
			}
			pCanvas = GetComponent<Canvas>();
			pCanvasScaler = GetComponent<CanvasScaler>();
			mUIManager = Singleton<UIManager>.pInstance;
			if (mUIManager == null)
			{
				UtDebug.LogError("No UIManager in the scene");
			}
			pEventReceiver = new UIEvents();
			pEventReceiver.OnAnimEnd += OnAnimEnd;
			pEventReceiver.OnDrag += OnDrag;
			pEventReceiver.OnDrop += OnDrop;
			pEventReceiver.OnBeginDrag += OnBeginDrag;
			pEventReceiver.OnEndDrag += OnEndDrag;
			pEventReceiver.OnClick += OnClick;
			pEventReceiver.OnPress += OnPress;
			pEventReceiver.OnPressRepeated += OnPressRepeated;
			pEventReceiver.OnHover += OnHover;
			pEventReceiver.OnEndEdit += OnEndEdit;
			pEventReceiver.OnValueChanged += OnValueChanged;
			pEventReceiver.OnCheckedChanged += OnCheckedChanged;
			pEventReceiver.OnSelected += OnSelected;
			CacheWidgets(_ProxyTransform);
			mGraphicsStatesCached = true;
		}
		base.pParentUI = parentUI;
		mState = _State;
		mVisible = _Visible;
		if (parentUI != null)
		{
			mParentState = parentUI.pStateInHierarchy;
			mParentVisible = parentUI.pVisibleInHierarchy;
		}
		WidgetState previousStateInHierarchy = mStateInHierarchy;
		mStateInHierarchy = ((mState < mParentState) ? mState : mParentState);
		mVisibleInHierarchy = (mParentVisible ? mVisible : mParentVisible);
		OnStateInHierarchyChanged(previousStateInHierarchy, mStateInHierarchy);
		OnVisibleInHierarchyChanged(mVisibleInHierarchy);
		foreach (UIWidget mChildWidget in mChildWidgets)
		{
			mChildWidget.pEventTarget = pEventReceiver;
			mChildWidget.Initialize(this, null);
		}
		foreach (UI mChildUI in mChildUIs)
		{
			mChildUI.pEventTarget = pEventReceiver;
			mChildUI.Initialize(this);
		}
	}

	private void TriggerOnInitializeRecursive()
	{
		foreach (UI mChildUI in mChildUIs)
		{
			mChildUI.TriggerOnInitializeRecursive();
		}
		if (!mOnInitializeCalled)
		{
			mOnInitializeCalled = true;
			OnInitialize();
		}
	}

	protected void CacheWidgets(Transform cacheTransform)
	{
		int childCount = cacheTransform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = cacheTransform.GetChild(i);
			UIWidget component = child.GetComponent<UIWidget>();
			if (component != null)
			{
				mChildWidgets.Add(component);
				continue;
			}
			UI component2 = child.GetComponent<UI>();
			if (component2 != null)
			{
				mChildUIs.Add(component2);
				continue;
			}
			Graphic component3 = child.GetComponent<Graphic>();
			if (component3 != null && !(component3 is NonDrawableGraphic))
			{
				GraphicState graphicState = new GraphicState(component3);
				graphicState.pOriginallyEnabled = true;
				mGraphicsStates.Add(graphicState);
			}
			if (child.childCount > 0)
			{
				CacheWidgets(child);
			}
		}
	}

	protected override void UpdateUnityComponents()
	{
		base.UpdateUnityComponents();
		pCanvas.enabled = mVisibleInHierarchy;
	}

	protected sealed override void UpdateStateInHierarchy()
	{
		WidgetState num = mStateInHierarchy;
		base.UpdateStateInHierarchy();
		if (num == mStateInHierarchy)
		{
			return;
		}
		foreach (UI mChildUI in mChildUIs)
		{
			mChildUI.OnParentSetState(mStateInHierarchy);
		}
	}

	protected sealed override void UpdateVisibleInHierarchy()
	{
		bool num = mVisibleInHierarchy;
		base.UpdateVisibleInHierarchy();
		if (num == mVisibleInHierarchy)
		{
			return;
		}
		foreach (UI mChildUI in mChildUIs)
		{
			mChildUI.OnParentSetVisible(mVisibleInHierarchy);
		}
	}

	protected override void OnStateInHierarchyChanged(WidgetState previousStateInHierarchy, WidgetState newStateInHierarchy)
	{
		UpdateUnityComponents();
	}

	protected override void OnVisibleInHierarchyChanged(bool newVisibleInHierarchy)
	{
		UpdateUnityComponents();
	}

	protected virtual void Awake()
	{
		if (base.transform.parent == null || base.transform.parent.GetComponentInParent<UI>() == null || base.transform.parent.GetComponentInParent<UIWidget>() != null)
		{
			Initialize(null);
			TriggerOnInitializeRecursive();
		}
	}

	protected override void OnVisibleChanged(bool newVisible)
	{
		base.OnVisibleChanged(newVisible);
		if (!newVisible)
		{
			RemoveExclusive();
		}
	}

	public virtual void SetExclusive()
	{
		SetExclusive(new Color(0f, 0f, 0f, 0.5f));
	}

	public void SetExclusive(Color color)
	{
		Image image = GetComponent<Image>();
		if (image == null)
		{
			image = base.gameObject.AddComponent<Image>();
		}
		image.sprite = null;
		image.color = color;
		image.raycastTarget = true;
		if (Singleton<UIManager>.pInstance != null)
		{
			Singleton<UIManager>.pInstance.AddToExclusiveListOnTop(this);
		}
	}

	public void RemoveExclusive()
	{
		Image component = base.gameObject.GetComponent<Image>();
		if (component != null)
		{
			UnityEngine.Object.DestroyImmediate(component);
		}
		if (Singleton<UIManager>.pInstance != null)
		{
			Singleton<UIManager>.pInstance.RemoveFromExclusiveList(this);
		}
	}

	public void SetInteractive(bool interactive)
	{
		mNotInteractiveRequests += ((!interactive) ? 1 : (-1));
		if (mNotInteractiveRequests < 0)
		{
			mNotInteractiveRequests = 0;
		}
		if (interactive)
		{
			if (mNotInteractiveRequests == 0)
			{
				pState = WidgetState.INTERACTIVE;
			}
		}
		else
		{
			pState = WidgetState.NOT_INTERACTIVE;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (Input.GetKeyUp(KeyCode.Escape) && mOnClickFrameCount != Time.frameCount && (_GlobalExclusiveUI == null || _GlobalExclusiveUI == this) && _BackButton != null && _BackButton.pInteractableInHierarchy && _BackButton.pVisible && _BackButton.pState == WidgetState.INTERACTIVE && _BackButton.gameObject.activeSelf)
		{
			mOnClickFrameCount = Time.frameCount;
			pEventReceiver.TriggerOnClick(_BackButton, null);
		}
	}

	private void OnDisable()
	{
	}
}
