public class UiJournalAchievements : UiAchievements, IJournal
{
	public void Exit()
	{
	}

	public void Clear()
	{
	}

	public void ProcessClose()
	{
	}

	public void ActivateUI(int uiIndex, bool isActive = true)
	{
	}

	public bool IsBusy()
	{
		return mLoadingAchData;
	}
}
