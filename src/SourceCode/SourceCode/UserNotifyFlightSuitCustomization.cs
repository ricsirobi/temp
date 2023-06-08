public class UserNotifyFlightSuitCustomization : UserNotify
{
	private AvAvatarState mPrevAvatarState;

	public override void OnWaitBeginImpl()
	{
		mPrevAvatarState = AvAvatar.pState;
		UserItemData freeToCustomEquipSuit = GetFreeToCustomEquipSuit();
		if (freeToCustomEquipSuit != null)
		{
			UiAvatarItemCustomization.Init(new UserItemData[1] { freeToCustomEquipSuit }, null, OnCloseCustomizeItem, multiItemCustomizationUI: false);
			mPrevAvatarState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
		}
		else
		{
			base.OnWaitEnd();
		}
	}

	private void OnCloseCustomizeItem(KAUISelectItemData inItem)
	{
		AvatarData.Save();
		if (AvatarData.pInstanceInfo.FlightSuitEquipped())
		{
			AvatarDataPart avatarDataPart = AvatarData.pInstanceInfo.FindPart(AvatarData.pPartSettings.AVATAR_PART_WING);
			if (avatarDataPart != null)
			{
				UiAvatarItemCustomization.ApplyCustomizationOnPart(AvAvatar.pObject, AvatarData.pPartSettings.AVATAR_PART_WING, AvatarData.pInstance, avatarDataPart.UserInventoryId.Value);
			}
		}
		OnWaitEnd();
	}

	protected override void OnWaitEnd()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = mPrevAvatarState;
		base.OnWaitEnd();
	}

	private UserItemData GetFreeToCustomEquipSuit()
	{
		if (AvatarData.pInstanceInfo.FlightSuitEquipped())
		{
			AvatarDataPart avatarDataPart = AvatarData.pInstanceInfo.FindPart(AvatarData.pPartSettings.AVATAR_PART_WING);
			if (avatarDataPart != null)
			{
				UserItemData userItemData = CommonInventoryData.pInstance.FindItemByUserInventoryID(avatarDataPart.UserInventoryId.Value);
				if (userItemData != null && userItemData.UserItemAttributes != null)
				{
					userItemData.UserItemAttributes.Init();
					Pair pair = userItemData.UserItemAttributes.FindByKey("FreeTkt");
					if (pair != null && pair.PairValue.Equals(true.ToString()))
					{
						return userItemData;
					}
				}
			}
		}
		return null;
	}
}
