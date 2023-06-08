using System;
using UnityEngine;

[RequireComponent(typeof(UIScrollBar))]
public class KAScrollBar : KAWidget
{
	public delegate void OnPressBackground(KAScrollBar inScrollBar, bool inPressed);

	public OnPressBackground onScrollbarBackgroundPress;

	public KAScrollButton _UpArrow;

	public KAScrollButton _DownArrow;

	public float _ScrollRepeatedDelay = 0.8f;

	public bool _DisableArrowOnScroll = true;

	private float mPressRepeatedStartTime;

	private bool mPressRepeated;

	private UIScrollBar mScrollbar;

	private bool mIsEventsRegistered;

	public UIScrollBar pScrollbar
	{
		get
		{
			if (mScrollbar == null)
			{
				mScrollbar = GetComponent<UIScrollBar>();
			}
			return mScrollbar;
		}
		set
		{
			mScrollbar = value;
		}
	}

	public UIWidget foregroundWidget
	{
		get
		{
			return mScrollbar.foregroundWidget;
		}
		set
		{
			mScrollbar.foregroundWidget = value;
		}
	}

	public UIWidget backgroundWidget
	{
		get
		{
			return mScrollbar.backgroundWidget;
		}
		set
		{
			mScrollbar.backgroundWidget = value;
		}
	}

	public float value
	{
		get
		{
			return mScrollbar.value;
		}
		set
		{
			mScrollbar.value = value;
		}
	}

	public float barSize
	{
		get
		{
			return mScrollbar.barSize;
		}
		set
		{
			mScrollbar.barSize = value;
		}
	}

	public UIScrollBar.Direction direction
	{
		get
		{
			if (mScrollbar.fillDirection == UIProgressBar.FillDirection.TopToBottom || mScrollbar.fillDirection == UIProgressBar.FillDirection.BottomToTop)
			{
				return UIScrollBar.Direction.Vertical;
			}
			if (mScrollbar.fillDirection == UIProgressBar.FillDirection.LeftToRight || mScrollbar.fillDirection == UIProgressBar.FillDirection.RightToLeft)
			{
				return UIScrollBar.Direction.Horizontal;
			}
			return mScrollbar.mDir;
		}
		set
		{
			mScrollbar.mDir = value;
		}
	}

	public void Init(OnPressBackground onPressBackground)
	{
		onScrollbarBackgroundPress = (OnPressBackground)Delegate.Combine(onScrollbarBackgroundPress, onPressBackground);
	}

	protected override void Awake()
	{
		base.Awake();
		mScrollbar = GetComponent<UIScrollBar>();
		if (mScrollbar == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected void Start()
	{
		EventDelegate.Add(mScrollbar.onChange, ScrollValueChanged);
	}

	protected override void Update()
	{
		base.Update();
		if (!mIsEventsRegistered && pScrollbar.backgroundWidget != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(pScrollbar.backgroundWidget.gameObject);
			uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPressScrollbarBackground));
			mIsEventsRegistered = true;
		}
	}

	private void ScrollValueChanged()
	{
		UpdateButtonStates();
	}

	public bool CanScrollRepeated(bool inPressed)
	{
		if (mPressRepeated != inPressed)
		{
			mPressRepeated = inPressed;
			if (mPressRepeated)
			{
				mPressRepeatedStartTime = Time.realtimeSinceStartup;
			}
		}
		else if (mPressRepeated && Time.realtimeSinceStartup - mPressRepeatedStartTime >= _ScrollRepeatedDelay)
		{
			return true;
		}
		return false;
	}

	public void Reset()
	{
		if (_UpArrow != null)
		{
			_UpArrow.SetVisibility(pScrollbar.alpha > 0.001f);
			_UpArrow.SetState(KAUIState.DISABLED);
		}
		if (_DownArrow != null)
		{
			_DownArrow.SetVisibility(pScrollbar.alpha > 0.001f);
			_DownArrow.SetState(KAUIState.INTERACTIVE);
		}
		pScrollbar.value = 0f;
	}

	public void Scroll(bool isUp, float inValue)
	{
		pScrollbar.value += (isUp ? (0f - inValue) : inValue);
	}

	private void UpdateButtonStates()
	{
		if (_UpArrow != null)
		{
			_UpArrow.SetState((mScrollbar.value <= 0.001f) ? (_DisableArrowOnScroll ? KAUIState.DISABLED : KAUIState.INTERACTIVE) : KAUIState.INTERACTIVE);
		}
		if (_DownArrow != null)
		{
			_DownArrow.SetState((mScrollbar.value >= 0.999f) ? (_DisableArrowOnScroll ? KAUIState.DISABLED : KAUIState.INTERACTIVE) : KAUIState.INTERACTIVE);
		}
	}

	public void UpdateButtonsOnPageFlip(int currentPage, int totalPage)
	{
		if (_UpArrow != null)
		{
			_UpArrow.SetState((currentPage <= 1) ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
		}
		if (_DownArrow != null)
		{
			_DownArrow.SetState((currentPage >= totalPage) ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
		}
	}

	private void ResizeColliders()
	{
		if (pScrollbar != null)
		{
			if (pScrollbar.foregroundWidget != null && pScrollbar.foregroundWidget.autoResizeBoxCollider)
			{
				pScrollbar.foregroundWidget.ResizeCollider();
			}
			if (pScrollbar.backgroundWidget != null && pScrollbar.backgroundWidget.autoResizeBoxCollider)
			{
				pScrollbar.backgroundWidget.ResizeCollider();
			}
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (pScrollbar != null)
		{
			if (_UpArrow != null)
			{
				_UpArrow.SetVisibility(inVisible);
			}
			if (_DownArrow != null)
			{
				_DownArrow.SetVisibility(inVisible);
			}
			if (pScrollbar.backgroundWidget != null)
			{
				pScrollbar.backgroundWidget.enabled = inVisible;
			}
			if (pScrollbar.foregroundWidget != null)
			{
				pScrollbar.foregroundWidget.enabled = inVisible;
			}
			if (inVisible)
			{
				ResizeColliders();
			}
		}
	}

	public override void SetState(KAUIState inState)
	{
		base.SetState(inState);
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
	}

	protected override void EnableCollider(bool inEnable)
	{
		base.EnableCollider(inEnable);
		if (pScrollbar != null)
		{
			if (pScrollbar.foregroundWidget != null && pScrollbar.foregroundWidget.collider != null)
			{
				pScrollbar.foregroundWidget.collider.enabled = inEnable;
			}
			if (pScrollbar.backgroundWidget != null && pScrollbar.backgroundWidget.collider != null)
			{
				pScrollbar.backgroundWidget.collider.enabled = inEnable;
			}
		}
	}

	protected void OnPressScrollbarBackground(GameObject go, bool isPressed)
	{
		if (onScrollbarBackgroundPress != null)
		{
			onScrollbarBackgroundPress(this, isPressed);
		}
	}
}
