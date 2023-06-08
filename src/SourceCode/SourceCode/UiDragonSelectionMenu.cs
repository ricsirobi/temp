using System;

public class UiDragonSelectionMenu : KAUIMenu
{
	protected override void Start()
	{
		if (mHorizontalScrollbar != null)
		{
			KAScrollButton upArrow = mHorizontalScrollbar._UpArrow;
			upArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(upArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
			KAScrollButton downArrow = mHorizontalScrollbar._DownArrow;
			downArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(downArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
			KAScrollBar kAScrollBar = mHorizontalScrollbar;
			kAScrollBar.onScrollbarBackgroundPress = (KAScrollBar.OnPressBackground)Delegate.Combine(kAScrollBar.onScrollbarBackgroundPress, new KAScrollBar.OnPressBackground(OnScrollChanged));
		}
	}

	public void ScrollButtonClicked(KAScrollBar scrollBar, KAScrollButton scrollButton, bool isRepeated)
	{
		((UiLobbyBase)_ParentUi).OnDragonSelectionMenuScroll();
	}

	private void OnScrollChanged(KAScrollBar sb, bool inPressed)
	{
		((UiLobbyBase)_ParentUi).OnDragonSelectionMenuScroll();
	}

	public override void OnPress(KAWidget inWidget, bool inPressed)
	{
		base.OnPress(inWidget, inPressed);
		if (!inPressed)
		{
			((UiLobbyBase)_ParentUi).OnSwipe(UICamera.currentTouch.totalDelta);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (mHorizontalScrollbar != null)
		{
			KAScrollButton upArrow = mHorizontalScrollbar._UpArrow;
			upArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Remove(upArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
			KAScrollButton downArrow = mHorizontalScrollbar._DownArrow;
			downArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Remove(downArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
			KAScrollBar kAScrollBar = mHorizontalScrollbar;
			kAScrollBar.onScrollbarBackgroundPress = (KAScrollBar.OnPressBackground)Delegate.Remove(kAScrollBar.onScrollbarBackgroundPress, new KAScrollBar.OnPressBackground(OnScrollChanged));
		}
	}
}
