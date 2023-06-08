public class UiJournalProfile : KAUI, IJournal
{
	private bool mIsLoading;

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			mIsLoading = true;
			if (UiProfile.pInstance == null)
			{
				UiProfile.ShowProfile();
				return;
			}
			UiProfile.pShowCloseButton = false;
			UiProfile.pInstance.OnActive(inStatus: true);
		}
		else if (UiProfile.pInstance != null)
		{
			UiProfile.pInstance.OnActive(inStatus: false);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mIsLoading)
		{
			mIsLoading = UiProfile.pInstance == null || UiProfile.pInstance.pLoading;
		}
	}

	public void ActivateUI(int uiIndex, bool addToList = true)
	{
	}

	public void Clear()
	{
		if (UiProfile.pInstance != null)
		{
			UiProfile.pInstance.CloseUI();
		}
	}

	public void Exit()
	{
	}

	public bool IsBusy()
	{
		return mIsLoading;
	}

	public void ProcessClose()
	{
	}
}
