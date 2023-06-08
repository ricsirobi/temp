using UnityEngine.EventSystems;

namespace JSGames.UI.TerrorMail;

public class UITerrorMailMessageThread : UI
{
	public UITerrorMail _UITerrorMail;

	public UIMenu _Menu;

	public UIWidget _ReplyBtn;

	public UIWidget _IgnoreBtn;

	public UIWidget _DeleteBtn;

	public UIWidget _ReportBtn;

	private UIWidget mSelectedWidget;

	public void UpdateSocialButtonVisibility(bool showIgnoreBtn, bool showReplyBtn, bool showReportBtn, bool showDeleteBtn)
	{
		_IgnoreBtn.pVisible = showIgnoreBtn;
		_ReplyBtn.pVisible = showReplyBtn;
		_ReportBtn.pVisible = showReportBtn;
		_DeleteBtn.pVisible = showDeleteBtn;
	}

	public void ClearMessageItems()
	{
		_Menu.ClearChildren();
	}

	protected override void OnClick(UIWidget inWidget, PointerEventData eventData)
	{
		base.OnClick(inWidget, eventData);
		if (inWidget == _ReplyBtn)
		{
			_UITerrorMail.OnClickReply();
			return;
		}
		mSelectedWidget = inWidget.pParentWidget;
		if (inWidget.name == _IgnoreBtn.name)
		{
			_UITerrorMail.OnClickIgnorePlayer();
		}
		else if (inWidget.name == _ReportBtn.name)
		{
			_UITerrorMail.OnClickReportPlayer();
		}
		else if (inWidget.name == _DeleteBtn.name)
		{
			_UITerrorMail.ShowDeleteConfirmation(base.gameObject, "OnConfirmDelete");
		}
	}

	private void OnConfirmDelete()
	{
		Message message = null;
		if (mSelectedWidget != null)
		{
			message = mSelectedWidget.pData as Message;
		}
		if (message != null)
		{
			_UITerrorMail.pMessageBoard.DeleteMessage(message.MessageID.Value, OnMessageDelete);
		}
		else
		{
			_UITerrorMail.OnDelete();
		}
	}

	private void OnMessageDelete(bool success)
	{
		if (!success)
		{
			UtDebug.LogError("Delete message FAILED!!!");
		}
		else
		{
			_Menu.RemoveWidget(mSelectedWidget);
		}
	}
}
