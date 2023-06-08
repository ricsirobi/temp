using System;
using System.Collections.Generic;

public class UiMessageLog : KAUI
{
	public List<KAWidget> _MessageLogWidgets;

	private KAWidget mBtnHide;

	private KAWidget mBtnChatHistory;

	private KAWidget mBackground;

	private static bool mVisible = true;

	protected override void Start()
	{
		mBtnHide = FindItem("BtnHide");
		mBtnChatHistory = FindItem("BtnChatHistory");
		mBackground = FindItem("MessageLogBkg");
		if (GetVisibility() != mVisible)
		{
			ShowMessageLog(mVisible);
		}
		base.Start();
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (mBtnHide == inWidget)
		{
			ShowMessageLog(!mVisible);
		}
		else if (mBtnChatHistory == inWidget)
		{
			UiChatHistory.pOnChatUIClosed = (UiChatHistory.OnChatUIClosed)Delegate.Combine(UiChatHistory.pOnChatUIClosed, new UiChatHistory.OnChatUIClosed(OnChatUIClosed));
			UiChatHistory.ShowChatHistory();
			SetVisibility(inVisible: false);
		}
		base.OnClick(inWidget);
	}

	public void OnChatUIClosed()
	{
		UiChatHistory.pOnChatUIClosed = (UiChatHistory.OnChatUIClosed)Delegate.Remove(UiChatHistory.pOnChatUIClosed, new UiChatHistory.OnChatUIClosed(OnChatUIClosed));
		SetVisibility(inVisible: true);
	}

	private void ShowMessageLog(bool show)
	{
		mBtnChatHistory.SetVisibility(show);
		mBackground.SetVisibility(show);
		if (_MessageLogWidgets != null)
		{
			foreach (KAWidget messageLogWidget in _MessageLogWidgets)
			{
				messageLogWidget.SetVisibility(show);
			}
		}
		mVisible = show;
	}

	public void ShowMessages(List<string> messages)
	{
		if (_MessageLogWidgets == null || _MessageLogWidgets.Count == 0)
		{
			return;
		}
		for (int i = 0; i < _MessageLogWidgets.Count; i++)
		{
			if (!(_MessageLogWidgets[i] == null))
			{
				int num = messages.Count - 1;
				_MessageLogWidgets[i].SetText((i <= num) ? messages[num - i] : "");
			}
		}
	}
}
