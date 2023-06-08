public class UiJournalMessageBoard : KAUI, IJournal
{
	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			if (UiMessages.pInstance == null)
			{
				UiMessages.pShowCloseButton = false;
				UiMessageBoard.ShowBoard(UserInfo.pInstance.UserID);
			}
			else
			{
				UiMessages.pInstance.OnActive(inVisible: true);
			}
		}
		else if (UiMessages.pInstance != null)
		{
			UiMessages.pInstance.OnActive(inVisible: false);
		}
	}

	public void ProcessClose()
	{
	}

	public bool IsBusy()
	{
		return false;
	}

	public bool IsReadyToClose()
	{
		return true;
	}

	public void ActivateUI(int uiIndex, bool addToList)
	{
	}

	public void Exit()
	{
	}

	public void Clear()
	{
		if (UiMessages.pInstance != null)
		{
			UiMessages.pInstance.Close();
		}
	}
}
