using System;

public class UiRacingSingleplayerMenu : KAUIMenu
{
	protected override void Start()
	{
		base.Start();
		KAScrollButton upArrow = mHorizontalScrollbar._UpArrow;
		upArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(upArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
		KAScrollButton downArrow = mHorizontalScrollbar._DownArrow;
		downArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Combine(downArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
	}

	public void ScrollButtonClicked(KAScrollBar scrollBar, KAScrollButton scrollButton, bool isRepeated)
	{
		((UiRacingSingleplayer)_ParentUi).SetTrackWithIndex(mCurrentPage - 1);
	}

	protected override void OnDestroy()
	{
		KAScrollButton upArrow = mHorizontalScrollbar._UpArrow;
		upArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Remove(upArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
		KAScrollButton downArrow = mHorizontalScrollbar._DownArrow;
		downArrow.onButtonClicked = (KAScrollButton.OnScrollButtonClicked)Delegate.Remove(downArrow.onButtonClicked, new KAScrollButton.OnScrollButtonClicked(ScrollButtonClicked));
	}
}
