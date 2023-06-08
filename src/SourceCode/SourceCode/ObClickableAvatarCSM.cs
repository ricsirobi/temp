public class ObClickableAvatarCSM : ObClickable
{
	public override bool IsActive()
	{
		if (AvAvatar.pLevelState == AvAvatarLevelState.RACING || AvAvatar.pLevelState == AvAvatarLevelState.TARGETPRACTICE || AvAvatar.pSubState == AvAvatarSubState.GLIDING || AvAvatar.pSubState == AvAvatarSubState.FLYING || AvAvatar.pLevelState == AvAvatarLevelState.FLIGHTSCHOOL || AvAvatar.pSubState == AvAvatarSubState.WALLCLIMB || AvAvatar.pToolbar == null)
		{
			return false;
		}
		if (FUEManager.pIsFUERunning && !MissionManager.IsTaskActive("Action", "Name", "OpenCSM"))
		{
			return false;
		}
		if (AvAvatar.pObject != null && AvAvatar.pObject.GetComponent<AvAvatarController>().pPlayerCarrying)
		{
			return false;
		}
		UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		if (component._UiAvatarCSM == null || !component._UiAvatarCSM.gameObject.activeSelf || !component._UiAvatarCSM.gameObject.activeInHierarchy)
		{
			return false;
		}
		return base.IsActive();
	}
}
