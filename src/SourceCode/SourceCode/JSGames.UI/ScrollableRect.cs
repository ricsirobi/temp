using System;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

[RequireComponent(typeof(UIMenu))]
public class ScrollableRect : ScrollRect
{
	public UIWidget _Background;

	public UIWidget _ScrollUpButton;

	public UIWidget _ScrollDownButton;

	public UIWidget _ScrollRightButton;

	public UIWidget _ScrollLeftButton;

	[Tooltip("The scroll step value in pixels")]
	public float _ScrollStep = 10f;

	private UIMenu mMenu;

	private bool mInitialized;

	private UIEvents mEventReceiver = new UIEvents();

	private Bounds mViewBound;

	private bool pHorizontalScrollButtonsNeeded => m_ContentBounds.size.x > mViewBound.size.x + 0.01f;

	private bool pVerticalScrollButtonsNeeded => m_ContentBounds.size.y > mViewBound.size.y + 0.01f;

	public void Initialize()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			mMenu = GetComponent<UIMenu>();
			mEventReceiver.OnPressRepeated += OnWidgetPressRepeated;
			UIMenu uIMenu = mMenu;
			uIMenu.OnVisibilityChanged = (Action<bool>)Delegate.Combine(uIMenu.OnVisibilityChanged, new Action<bool>(OnMenuVisiblilityChanged));
			UIMenu uIMenu2 = mMenu;
			uIMenu2.OnMenuStateChanged = (Action<WidgetState>)Delegate.Combine(uIMenu2.OnMenuStateChanged, new Action<WidgetState>(OnMenuStateChanged));
			if (_Background != null)
			{
				_Background.pEventTarget = mEventReceiver;
				_Background.Initialize(mMenu, null);
			}
			if (_ScrollUpButton != null)
			{
				_ScrollUpButton.pEventTarget = mEventReceiver;
				_ScrollUpButton.Initialize(mMenu, null);
			}
			if (_ScrollDownButton != null)
			{
				_ScrollDownButton.pEventTarget = mEventReceiver;
				_ScrollDownButton.Initialize(mMenu, null);
			}
			if (_ScrollRightButton != null)
			{
				_ScrollRightButton.pEventTarget = mEventReceiver;
				_ScrollRightButton.Initialize(mMenu, null);
			}
			if (_ScrollLeftButton != null)
			{
				_ScrollLeftButton.pEventTarget = mEventReceiver;
				_ScrollLeftButton.Initialize(mMenu, null);
			}
		}
	}

	protected void OnWidgetPressRepeated(UIWidget widget)
	{
		if (widget == _ScrollUpButton && base.vertical)
		{
			base.verticalNormalizedPosition += _ScrollStep / m_ContentBounds.size.y;
		}
		else if (widget == _ScrollDownButton && base.vertical)
		{
			base.verticalNormalizedPosition -= _ScrollStep / m_ContentBounds.size.y;
		}
		else if (widget == _ScrollRightButton && base.horizontal)
		{
			base.horizontalNormalizedPosition += _ScrollStep / m_ContentBounds.size.x;
		}
		else if (widget == _ScrollLeftButton && base.horizontal)
		{
			base.horizontalNormalizedPosition -= _ScrollStep / m_ContentBounds.size.x;
		}
	}

	private void OnMenuVisiblilityChanged(bool newVisible)
	{
		if (_Background != null)
		{
			_Background.OnParentSetVisible(newVisible);
		}
		if (_ScrollDownButton != null)
		{
			_ScrollDownButton.OnParentSetVisible(newVisible);
		}
		if (_ScrollUpButton != null)
		{
			_ScrollUpButton.OnParentSetVisible(newVisible);
		}
		if (_ScrollRightButton != null)
		{
			_ScrollRightButton.OnParentSetVisible(newVisible);
		}
		if (_ScrollLeftButton != null)
		{
			_ScrollLeftButton.OnParentSetVisible(newVisible);
		}
	}

	private void OnMenuStateChanged(WidgetState newState)
	{
		if (_Background != null)
		{
			_Background.OnParentSetState(newState);
		}
		if (_ScrollDownButton != null)
		{
			_ScrollDownButton.OnParentSetState(newState);
		}
		if (_ScrollUpButton != null)
		{
			_ScrollUpButton.OnParentSetState(newState);
		}
		if (_ScrollRightButton != null)
		{
			_ScrollRightButton.OnParentSetState(newState);
		}
		if (_ScrollLeftButton != null)
		{
			_ScrollLeftButton.OnParentSetState(newState);
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (Application.isPlaying && mMenu != null && mMenu.pVisibleInHierarchy)
		{
			UpdateScrollButtonsVisibility(pVerticalScrollButtonsNeeded, base.vertical, base.verticalScrollbarVisibility, _ScrollUpButton, _ScrollDownButton);
			UpdateScrollButtonsVisibility(pHorizontalScrollButtonsNeeded, base.horizontal, base.horizontalScrollbarVisibility, _ScrollRightButton, _ScrollLeftButton);
		}
	}

	public override void SetLayoutHorizontal()
	{
		base.SetLayoutHorizontal();
		mViewBound = new Bounds(base.viewRect.rect.center, base.viewRect.rect.size);
	}

	public override void SetLayoutVertical()
	{
		base.SetLayoutVertical();
		mViewBound = new Bounds(base.viewRect.rect.center, base.viewRect.rect.size);
	}

	public void SetScrollValue(float normalizedPosition)
	{
		normalizedPosition = Mathf.Clamp01(normalizedPosition);
		if (base.vertical)
		{
			base.verticalNormalizedPosition = normalizedPosition;
		}
		if (base.horizontal)
		{
			base.horizontalNormalizedPosition = normalizedPosition;
		}
	}

	private void UpdateScrollButtonsVisibility(bool isScrollButtonsNeeded, bool xAxisEnabled, ScrollbarVisibility scrollbarVisibility, UIWidget next, UIWidget prev)
	{
		if (next != null && prev != null)
		{
			isScrollButtonsNeeded = ((scrollbarVisibility == ScrollbarVisibility.Permanent) ? xAxisEnabled : isScrollButtonsNeeded);
			if (next.gameObject.activeSelf != isScrollButtonsNeeded)
			{
				next.gameObject.SetActive(isScrollButtonsNeeded);
			}
			if (prev.gameObject.activeSelf != isScrollButtonsNeeded)
			{
				prev.gameObject.SetActive(isScrollButtonsNeeded);
			}
		}
	}
}
