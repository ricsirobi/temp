using System;
using System.Collections.Generic;
using UnityEngine;

public class UserNotifyMembershipExpired : UserNotify
{
	public List<int> _MemberOnlyTicketItemID;

	public LocaleString _BecomeMemberText = new LocaleString("Your membership has expired - you can no longer use your membership perks.  Would you like to renew your membership?");

	public LocaleString _MembershipExpiredText = new LocaleString("Your membership has expired.");

	public LocaleString _RenewMembershipActionText = new LocaleString("renew your membership.");

	public LocaleString _BecomeMemberTitleText = new LocaleString("Membership Expired");

	public LocaleString _SelectActiveDragonText = new LocaleString("Please select another dragon to be your active dragon.");

	public LocaleString _MembershipUpgradeTitleText = new LocaleString("Membership Upgrade Required");

	public LocaleString _SelectActiveDragonUpgradeText = new LocaleString("At least a 3 months membership is required to use {{Dragon}}. Please select another dragon to be your active dragon.");

	public LocaleString _ExpiryDateWarningText = new LocaleString("Your Membership expires on %date%");

	public LocaleString _ExpiryDaysWarningText = new LocaleString("Your Membership at School of Dragons will expire soon!");

	public LocaleString _ExpiryHoursText = new LocaleString("Your Membership at School of Dragons will expire in %hours% hours");

	public LocaleString _ExpiryHourText = new LocaleString("Your Membership at School of Dragons will expire in %hours% hour");

	public static bool _ShowMemberWarning = true;

	public string _WarningAssetPath = "RS_DATA/PfUiRenewMembershipDBDO.unity3d/PfUiRenewMembershipDBDO";

	private KAUIGenericDB mKAUIGenericDB;

	private string mWasMemberKey = "WAS_MEMBER";

	private UiToolbar mToolbar;

	private AvAvatarState mPrevAvatarState = AvAvatarState.PAUSED;

	public override void OnWaitBeginImpl()
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		if (AvAvatar.pToolbar != null)
		{
			mToolbar = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		}
		if (SubscriptionInfo.pIsMember)
		{
			if (_ShowMemberWarning)
			{
				ProductData.pPairData.SetValueAndSave(mWasMemberKey, "true");
				if (IAPManager.IsMembershipExpiring(SubscriptionInfo.pIsTrialMember ? GameDataConfig.pInstance.TrialMembershipRenewalWarningDays : GameDataConfig.pInstance.MembershipRenewalWarningDays) && FUEManager.pIsFUERunning)
				{
					_ShowMemberWarning = false;
					LoadMembershipExpiryWarningDB();
				}
				else
				{
					CheckIsActivePetLocked();
				}
			}
			else
			{
				CheckIsActivePetLocked();
			}
			return;
		}
		MembershipItemsInfo.SaveMemberOnlyItems(_MemberOnlyTicketItemID);
		bool flag = true;
		if (ProductData.pPairData.FindByKey(mWasMemberKey) != null)
		{
			flag = bool.Parse(ProductData.pPairData.GetValue(mWasMemberKey));
		}
		if (flag && SubscriptionInfo.pIsMembershipExpired)
		{
			if (FUEManager.pIsFUERunning || (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.IsPetLocked(SanctuaryManager.pCurPetInstance.pData)))
			{
				ShowMembershipPrompt();
				return;
			}
			UiChatHistory.AddSystemNotification(_MembershipExpiredText.GetLocalizedString(), new MessageInfo(), OnSystemMessageClicked, ignoreDuplicateMessage: false, _RenewMembershipActionText.GetLocalizedString());
			OnWaitEnd();
		}
		else
		{
			CheckIsActivePetLocked();
		}
	}

	private void OnSystemMessageClicked(object msgObject)
	{
		if (AvAvatar.pState != AvAvatarState.PAUSED)
		{
			mPrevAvatarState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		UiChatHistory.SystemMessageAccepted(msgObject);
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	private void OnClose()
	{
		if (base.enabled)
		{
			OnWaitEnd();
		}
		else if (mPrevAvatarState != AvAvatarState.PAUSED)
		{
			AvAvatar.pState = mPrevAvatarState;
		}
	}

	protected override void OnWaitEnd()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		base.OnWaitEnd();
	}

	private void ShowDB()
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "MembershipExpired");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
	}

	private void ShowMembershipPrompt()
	{
		ShowDB();
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB.SetText(_BecomeMemberText.GetLocalizedString(), interactive: false);
		mKAUIGenericDB.SetTitle(_BecomeMemberTitleText.GetLocalizedString());
		mKAUIGenericDB._YesMessage = "OnYes";
		mKAUIGenericDB._NoMessage = "LockMembershipItems";
	}

	private void LockMembershipItems()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.pAvatarCustomization.RestoreAvatar();
			component.pAvatarCustomization.SaveCustomAvatar();
			UiToolbar.pAvatarModified = true;
		}
		ProductData.pPairData.SetValueAndSave(mWasMemberKey, "false");
		CheckIsActivePetLocked();
	}

	private void OnYes()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	private void OnCloseMembershipWarning()
	{
		CheckIsActivePetLocked();
	}

	private void OnIAPStoreClosed()
	{
		UtDebug.Log("Membership renewal status " + SubscriptionInfo.pIsMember);
		if (SubscriptionInfo.pIsMember)
		{
			if (SubscriptionInfo.IsOneMonthMembership())
			{
				CheckIsActivePetLocked();
				return;
			}
			mToolbar.OnIAPStoreClosed();
			OnClose();
		}
		else
		{
			LockMembershipItems();
		}
	}

	private void ShowDragonsList()
	{
		if (mToolbar != null && mToolbar.IsActive())
		{
			UiDragonsStable.OpenDragonListUI(base.gameObject);
		}
	}

	private void CheckIsActivePetLocked()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.IsPetLocked(SanctuaryManager.pCurPetInstance.pData))
		{
			ShowDB();
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			if (SubscriptionInfo.IsOneMonthMembership())
			{
				string localizedString = _SelectActiveDragonUpgradeText.GetLocalizedString();
				localizedString = localizedString.Replace("{{Dragon}}", SanctuaryManager.pCurPetInstance.pData.Name);
				mKAUIGenericDB.SetText(localizedString, interactive: false);
				mKAUIGenericDB.SetTitle(_MembershipUpgradeTitleText.GetLocalizedString());
			}
			else
			{
				mKAUIGenericDB.SetTitle(_BecomeMemberTitleText.GetLocalizedString());
				mKAUIGenericDB.SetText(_SelectActiveDragonText.GetLocalizedString(), interactive: false);
			}
			mKAUIGenericDB._CloseMessage = "ShowDragonsList";
			mKAUIGenericDB._OKMessage = "ShowDragonsList";
		}
		else
		{
			OnClose();
		}
	}

	private void OnStableUIOpened(UiDragonsStable uiStable)
	{
		uiStable.pUiDragonsListCard.pCurrentMode = UiDragonsListCard.Mode.ForceDragonSelection;
		uiStable.pUiDragonsListCard.SetCloseButtonVisibility(visible: false);
		uiStable.pUiDragonsInfoCard.SetCloseButtonVisibility(visible: false);
	}

	private void OnStableUIClosed()
	{
		OnClose();
	}

	private void LoadMembershipExpiryWarningDB()
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = _WarningAssetPath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MembershipWarningUiHandler, typeof(GameObject));
	}

	private void MembershipWarningUiHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			string text = string.Empty;
			TimeSpan timeSpan = SubscriptionInfo.pInstance.SubscriptionEndDate.Value - ServerTime.pCurrentTime;
			if (timeSpan.Days == 0)
			{
				if (timeSpan.Hours >= 2)
				{
					text = _ExpiryHoursText.GetLocalizedString();
				}
				else if (timeSpan.Hours >= 1)
				{
					text = _ExpiryHourText.GetLocalizedString();
				}
				else if (timeSpan.Hours < 1)
				{
					text = _ExpiryHourText.GetLocalizedString();
					text = text.Replace("%hours%", "<1");
				}
				text = text.Replace("%hours%", timeSpan.Hours.ToString());
			}
			else
			{
				text = _ExpiryDaysWarningText.GetLocalizedString();
			}
			string localizedString = _ExpiryDateWarningText.GetLocalizedString();
			localizedString = localizedString.Replace("%date%", SubscriptionInfo.pInstance.SubscriptionEndDate.Value.ToString("d"));
			KAUICursorManager.SetDefaultCursor("Arrow");
			UiMembershipExpiry component = UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<UiMembershipExpiry>();
			component.SetUpDB(text, localizedString);
			mKAUIGenericDB = component;
			mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
			mKAUIGenericDB._OKMessage = "OnYes";
			mKAUIGenericDB._CloseMessage = "OnCloseMembershipWarning";
			mKAUIGenericDB._MessageObject = base.gameObject;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load Membership Expiry Warning bundle", 100);
			OnWaitEnd();
			break;
		}
	}
}
