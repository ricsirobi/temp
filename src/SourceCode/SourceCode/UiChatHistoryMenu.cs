public class UiChatHistoryMenu : KAUIMenu
{
	public override void UpdateScrollbars(bool inVisible)
	{
		if (UiChatHistory.mFade)
		{
			base.UpdateScrollbars(inVisible: false);
		}
		else
		{
			base.UpdateScrollbars(inVisible);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		_ParentUi.SendMessage("Click", inWidget);
	}

	public override void ClearItems()
	{
		base.ResetPosition();
		base.ClearItems();
	}

	public override void ResetPosition()
	{
		if (!base.pDragPanel.shouldMoveVertically || GetTopItemIdx() == 0)
		{
			base.ResetPosition();
		}
	}

	protected override void ResetScrollBar()
	{
	}
}
