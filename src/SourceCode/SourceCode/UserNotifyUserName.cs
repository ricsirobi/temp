public class UserNotifyUserName : UserNotify
{
	public string _AvatarNameSelectBundlePath = "RS_DATA/PfUiAvatarNameDO.unity3d/PfUiAvatarNameDO";

	public string _DefaultAvatarNamePrefix = "Viking-";

	public override void OnWaitBeginImpl()
	{
		CheckAndLoadAvatarNameSelection();
	}

	private void CheckAndLoadAvatarNameSelection()
	{
		if (AvatarData.pInstance != null && (string.IsNullOrEmpty(AvatarData.pInstance.DisplayName) || AvatarData.pInstance.DisplayName.Contains(_DefaultAvatarNamePrefix)))
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			UiSelectName.Init(OnSelectNameProcessed, null, null, UiSelectName.FailStatus.None, independent: true);
		}
		else
		{
			OnWaitEnd();
		}
	}

	private void OnSelectNameProcessed(UiSelectName.Status status, string name, bool suggestedNameSelected, UiSelectName uiSelectName)
	{
		switch (status)
		{
		case UiSelectName.Status.Accepted:
		case UiSelectName.Status.Closed:
			OnWaitEnd();
			break;
		case UiSelectName.Status.Loaded:
			if (uiSelectName != null)
			{
				uiSelectName.HideBackButton(hide: true);
			}
			break;
		}
	}

	protected override void OnWaitEnd()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		base.OnWaitEnd();
	}
}
