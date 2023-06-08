using System;

public class UiRacingDragonSelectionMenu : KAUIMenu
{
	protected override void Start()
	{
		base.Start();
		KAScrollButton upArrow = mHorizontalScrollbar._UpArrow;
		upArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(upArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
		KAScrollButton downArrow = mHorizontalScrollbar._DownArrow;
		downArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(downArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
		KAScrollBar kAScrollBar = mHorizontalScrollbar;
		kAScrollBar.onScrollbarBackgroundPress = (KAScrollBar.OnPressBackground)Delegate.Combine(kAScrollBar.onScrollbarBackgroundPress, new KAScrollBar.OnPressBackground(OnScrollChanged));
	}

	public void ScrollButtonClicked(KAScrollBar scrollBar, KAScrollButton scrollButton, bool isRepeated)
	{
		((UiRacingMultiplayer)_ParentUi).OnDragonSelectionMenuScroll();
	}

	private void OnScrollChanged(KAScrollBar sb, bool inPressed)
	{
		((UiRacingMultiplayer)_ParentUi).OnDragonSelectionMenuScroll();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		KAScrollButton upArrow = mHorizontalScrollbar._UpArrow;
		upArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Remove(upArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
		KAScrollButton downArrow = mHorizontalScrollbar._DownArrow;
		downArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Remove(downArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
		KAScrollBar kAScrollBar = mHorizontalScrollbar;
		kAScrollBar.onScrollbarBackgroundPress = (KAScrollBar.OnPressBackground)Delegate.Remove(kAScrollBar.onScrollbarBackgroundPress, new KAScrollBar.OnPressBackground(OnScrollChanged));
	}
}
