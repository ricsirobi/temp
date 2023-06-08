using System;
using System.Collections.Generic;
using JSGames.Platform.PlayFab;
using Microsoft.AppCenter.Unity;
using SOD.Event;
using UnityEngine;

public class UiSelectProfile : KAUI
{
	public const string PROFILE_CREATION_COUNT = "PROFILE_CREATION_COUNT";

	public float _ScaleRange = 0.15f;

	public float _ScaleSpeed = 0.5f;

	public int _MaxProfiles = 6;

	public int _FreeSlotID = 8208;

	public int _CostForDeletion = 50;

	public string[] _StoresToInit;

	public List<QuickLaunchBtnInfo> _QuickLaunchBtnInfo = new List<QuickLaunchBtnInfo>();

	public string _ProfileSelectedMessage = "OnProfileSelected";

	public GameObject _MessageObject;

	public GameObject _AvatarStartMarkerAlone;

	public GameObject _AvatarStartMarker;

	public DragonStartMarkers _DragonStartMarkers;

	public GameObject _PetManager;

	public GameObject _QuickLaunchGroup;

	public Camera _MainCamera;

	public string _CustomizationBundleURL = "RS_DATA/PfUiAvatarCustomizationDO";

	public string _CustomizationAssetName = "PfUiAvatarCustomizationDO";

	public string _NewFeaturesDBAssetPath = "RS_DATA/PfUiNewFeaturesDO";

	public string _NewFeaturesAssetName = "PfUiNewFeaturesDO";

	public LocaleString _SecurityTitleText = new LocaleString("Delete Profile");

	public LocaleString _SecurityHeadingText = new LocaleString("Type the word {{SECURITY_WORD}} in the field above to complete deletion of account");

	public LocaleString _SecurityWordText = new LocaleString("DELETE");

	public LocaleString _NameEnteredErrorText = new LocaleString("The word entered does not match");

	public LocaleString _NameText = new LocaleString("Name: ");

	public LocaleString _MaxProfilesText = new LocaleString("You can create only 6 profiles per account!");

	public LocaleString _BuyProfileSlotTitleText = new LocaleString("Buy Profile Slot");

	public LocaleString _NotEnoughVCashText = new LocaleString("You dont have enough VCash to buy this slot. Do you want to buy VCash?");

	public LocaleString _StoreUnavailableText = new LocaleString("Store Unavailable at this time. Please try again later.");

	public LocaleString _SlotPurchaseProcessingText = new LocaleString("Processing slot purchase.");

	public LocaleString _SlotPurchaseSuccessfulText = new LocaleString("Slot purchase successful.");

	public LocaleString _SlotPurchaseFailedText = new LocaleString("Slot purchase failed.");

	public LocaleString _MemberOnlySlotText = new LocaleString("Become Member to Unlock.");

	public LocaleString _PurchaseSlotText = new LocaleString("Purchase with Gems.");

	public LocaleString _OpenSlotText = new LocaleString("Create New Profile");

	public LocaleString _BuyProfileConfirmationText = new LocaleString("Are you sure you want to buy a new profile slot for {{COUNT}} gems?");

	public LocaleString _ProfileDeletionSuccessText = new LocaleString("The profile has been deleted");

	public LocaleString _MemberSlotTitleText = new LocaleString("Member Profile");

	public LocaleString _MemberSlotUnlockText = new LocaleString("Become a member to unlock this profile slot.");

	public LocaleString _NotEnoughVCashToDeleteText = new LocaleString("You do not have enough VCash to delete this profile");

	public LocaleString _ProfileDeletionErrorText = new LocaleString("Could not delete profile. Please try again later");

	public LocaleString _DeleteProfileConfirmationText = new LocaleString("Are you sure you want to delete this profile?");

	public LocaleString _NonMemberLastSlotDeleteText = new LocaleString("You need {{COUNT}} gems to delete this profile. Are you sure you want to delete your profile?");

	public LocaleString _CannotDeleteOneActiveSlotText = new LocaleString("You need to have at least one active profile in your account. Create a new profile before deleting this.");

	public LocaleString _StoreDataUnavailableText = new LocaleString("Server Unavailable at this time. Please Login again.");

	public LocaleString _AvatarDataSaveErrorTitleText = new LocaleString("Retry");

	public LocaleString _AvatarDataSaveErrorText = new LocaleString("Sorry! Profile didn't saved correctly. Please Retry.");

	public LocaleString _IAPSynchWaitText = new LocaleString("Synchronizing pending purchases. Please Wait...");

	public LocaleString _IAPSynchFailedText = new LocaleString("Synchronizing purchases failed. Try next time...");

	public LocaleString _MMOEnableWarningText = new LocaleString("Enabling MMO may affect your game's performance. Do you want to go ahead?");

	public LocaleString _ServerErrorTitleText = new LocaleString("Error");

	public LocaleString _MultiplayerDisabledOnServerText = new LocaleString("You need to turn multiplayer on. You can do that from your account at www.schoolofdragons.com.");

	public LocaleString _MMODisableWarningText = new LocaleString("Do you really want to disable MMO?");

	public LocaleString _MMOTitleText = new LocaleString("MMO Warning");

	public LocaleString _IAPSynchSuccessText = new LocaleString("Synchronizing purchase successful. Gems will reflect now...");

	public LocaleString _OfflineSynchWaitText = new LocaleString("Synchronizing offline data. Please Wait...");

	public LocaleString _FBLoginText = new LocaleString("Logging in to existing account...");

	public LocaleString _DragonBusyText = new LocaleString("You cannot race now as your eligible Dragon is busy in a quest mission");

	private int mSlotStoreID;

	public int _SlotItemID = 7971;

	public int _MemberSlotItemID = 13126;

	public const string LAST_PLAYED_KEY = "LastMemberDBShown";

	public const string MEMBERSHIP_COUNT = "MemberDBWarning";

	public int _MembershipWarningFreq = 1;

	public int _MembershipWarningCount = 5;

	[NonSerialized]
	public bool mIsPetLoadingDone;

	[NonSerialized]
	public bool mIsPetAvailable;

	private float mLegScale;

	private float mTopScale;

	private GameObject mPetManagerGO;

	private string mDeleteProfileUserID;

	private List<ChildInfo> mChildInfo = new List<ChildInfo>();

	private UserProfileDataList mChildListInfo;

	private ChildInfo mCurrentChild;

	public PoolInfo[] _TurnOffPoolInfo;

	public List<SubscriptionNotificationData> _SubscriptionNotificationData = new List<SubscriptionNotificationData>();

	private UiSelectProfileMenu mProfileMenu;

	private KAWidget mSelectBtn;

	private KAWidget mCreateBtn;

	private KAWidget mBackBtn;

	private KAWidget mGemTotal;

	private KAWidget mGemTotalTxt;

	private KAWidget mWhatsNewBtn;

	private KAWidget mOptionsBtn;

	private KAToggleButton mBtnMMO;

	private bool mAvatarReady;

	private bool mFirstLaunch;

	private LoadInfo mSelectedQuickLaunchInfo = new LoadInfo();

	private string mSelectedQuickLaunchValue = string.Empty;

	private UiAvatarCustomization mUiAvatarCustomization;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mIsCustomizationScreen;

	private int mSlotCost;

	private int mSlotInventoryMax;

	private bool mStoreDataLoaded;

	private bool mIsReady;

	private bool mInitChildList;

	private bool mIsUiStateSet;

	private const string mLastPlayedChildName = "LAST_PLAYED_CHILD_NAME";

	private bool mAvatarDataFailed;

	private int mDataLoadingCount;

	private static bool mServerDownCheckDone;

	private bool mAvatarDataErrorDBSet;

	private bool mMMOStatus;

	public bool pAvatarDataNull { get; set; }

	public bool pFirstLaunch => mFirstLaunch;

	public List<ChildInfo> pChildInfo => mChildInfo;

	public ChildInfo pCurrentChild
	{
		get
		{
			return mCurrentChild;
		}
		set
		{
			mCurrentChild = value;
		}
	}

	public UserProfileDataList pChildListInfo
	{
		get
		{
			return mChildListInfo;
		}
		set
		{
			mChildListInfo = value;
		}
	}

	public void Init()
	{
		mGemTotal = FindItem("GemTotal");
		mGemTotalTxt = FindItem("txtCoinTotal");
		mSelectBtn = FindItem("SelectBtn");
		mCreateBtn = FindItem("CreateBtn");
		mBackBtn = FindItem("BackButton");
		mWhatsNewBtn = FindItem("WhatsNewBtn");
		mOptionsBtn = FindItem("BtnDWDragonsHUDOptions");
		mBtnMMO = (KAToggleButton)FindItem("BtnMMO");
		mSelectBtn.SetState(KAUIState.DISABLED);
		mCreateBtn.SetState(KAUIState.DISABLED);
		mProfileMenu = (UiSelectProfileMenu)GetMenu("UiSelectProfileMenu");
		mBackBtn.SetVisibility(inVisible: true);
		pAvatarDataNull = false;
		if (!string.IsNullOrEmpty(ProductConfig.pToken))
		{
			InitStores(useForceLoad: false);
			ParentData.Init();
			ShowBusy(isBusy: true);
			Money.Init();
			Money.AddNotificationObject(base.gameObject);
		}
		else
		{
			mStoreDataLoaded = true;
			mFirstLaunch = true;
			ProcessProfileCreation(null);
		}
		SnChannel.AddToTurnOffPools(_TurnOffPoolInfo);
	}

	private void InitStores(bool useForceLoad)
	{
		int[] array = new int[_StoresToInit.Length];
		for (int i = 0; i < _StoresToInit.Length; i++)
		{
			array[i] = Convert.ToInt32(GameConfig.GetKeyData(_StoresToInit[i]));
		}
		if (array != null && array.Length != 0)
		{
			ItemStoreDataLoader.Load(array, OnStoreLoaded, null, (!useForceLoad) ? (-1) : 0);
		}
	}

	private void AvatarLoadEventHandler(bool inStatus)
	{
		if (!inStatus)
		{
			mAvatarDataErrorDBSet = true;
			ShowGenericDB("UiAvatarDataErrorDB", _AvatarDataSaveErrorTitleText.GetLocalizedString(), _AvatarDataSaveErrorText.GetLocalizedString(), null, null, "KillAvatarDataErrorDB", null);
			mCreateBtn.SetVisibility(inVisible: true);
			mCreateBtn.SetState(KAUIState.INTERACTIVE);
			mSelectBtn.SetVisibility(inVisible: false);
			_QuickLaunchGroup.SetActive(value: false);
			AvAvatar.SetPosition(Vector3.up * 5000f);
		}
		else
		{
			mCreateBtn.SetVisibility(inVisible: false);
			mSelectBtn.SetVisibility(inVisible: true);
		}
	}

	public void OnDisable()
	{
		Money.RemoveNotificationObject(base.gameObject);
	}

	private void GetDetailedChildList()
	{
		ShowBusy(isBusy: true);
		WsWebService.GetDetailedChildList(ProductConfig.pToken, ServiceEventHandler, null);
	}

	public void SelectProfile(string avatarName = null)
	{
		string text = avatarName;
		if (string.IsNullOrEmpty(text) && mChildInfo != null && mChildInfo.Count > 0)
		{
			text = mChildInfo[0]._Name;
		}
		else if (mCurrentChild != null)
		{
			mCurrentChild._Name = avatarName;
			foreach (ChildInfo item in mChildInfo)
			{
				if (item._UserID == mCurrentChild._UserID)
				{
					item._Name = avatarName;
					break;
				}
			}
			KAWidget kAWidget = FindItem("BtnProfile_" + mCurrentChild._UserID);
			if (kAWidget != null)
			{
				kAWidget.FindChildItem("AvatarInfo").FindChildItem("TxtName").SetText(_NameText.GetLocalizedString() + mCurrentChild._Name);
			}
		}
		LoginChildByName(text);
	}

	private void LoginChildByName(string childName)
	{
		foreach (ChildInfo item in mChildInfo)
		{
			if (item._Name.Trim().ToLower().Equals(childName.Trim().ToLower()))
			{
				mIsUiStateSet = false;
				ResetPreviousData();
				mCurrentChild = item;
				ShowBusy(isBusy: true);
				AppCenter.SetUserId(mCurrentChild._UserID);
				WsWebService.LoginChild(ProductConfig.pToken, mCurrentChild._UserID, UtUtilities.GetLocaleLanguage(), ServiceEventHandler, item);
			}
			UpdateUserProfileForUserID(item);
			UpdateGroupNameLogo(item._UserID);
		}
		mIsReady = false;
	}

	private void UpdateGroupNameLogo(string UserID)
	{
		ChildInfo childByUserID = GetChildByUserID(UserID);
		if (childByUserID != null && childByUserID._UserProfileData != null && childByUserID._UserProfileData.Groups != null)
		{
			Group group = new Group();
			group.Name = childByUserID._UserProfileData.Groups[0].Name;
			group.Logo = childByUserID._UserProfileData.Groups[0].Logo;
			group.Color = childByUserID._UserProfileData.Groups[0].Color;
			childByUserID._ChildGroup = group;
		}
	}

	private ChildInfo GetChildByName(string childName)
	{
		foreach (ChildInfo item in mChildInfo)
		{
			if (item._Name.Trim().ToLower().Equals(childName.Trim().ToLower()))
			{
				return item;
			}
		}
		return null;
	}

	private ChildInfo GetChildByUserID(string userID)
	{
		if (string.IsNullOrEmpty(userID))
		{
			return null;
		}
		return mChildInfo.Find((ChildInfo info) => info._UserID.ToLower().Equals(userID.ToLower()));
	}

	public void UpdateChildList(UserProfileDataList userProfileDisplayData)
	{
		if (userProfileDisplayData == null)
		{
			return;
		}
		mChildListInfo = userProfileDisplayData;
		mChildInfo.Clear();
		UserProfileData[] userProfiles = mChildListInfo.UserProfiles;
		foreach (UserProfileData userProfileData in userProfiles)
		{
			ChildInfo childInfo = new ChildInfo();
			childInfo._UserID = userProfileData.ID;
			if (userProfileData.AvatarInfo.AvatarData != null)
			{
				childInfo._Name = userProfileData.AvatarInfo.AvatarData.DisplayName;
			}
			else
			{
				childInfo._Name = userProfileData.AvatarInfo.UserInfo.Username;
			}
			mChildInfo.Add(childInfo);
		}
	}

	public void UpdateProfileButtons()
	{
		if (mChildListInfo == null)
		{
			return;
		}
		mProfileMenu.ClearItems();
		foreach (ChildInfo item in mChildInfo)
		{
			KAWidget kAWidget = DuplicateWidget("TemplateAvatarNames");
			kAWidget.name = "BtnProfile_" + item._UserID;
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.SetState(KAUIState.INTERACTIVE);
			KAWidget kAWidget2 = kAWidget.FindChildItem("AvatarInfo");
			kAWidget2.FindChildItem("TxtName").SetText(_NameText.GetLocalizedString() + item._Name);
			if (item._ChildGroup != null)
			{
				kAWidget2.FindChildItem("TxtClan").SetText(item._ChildGroup.Name);
				ClanData clanData = new ClanData(item._ChildGroup);
				kAWidget2.SetUserData(clanData);
				clanData.Load();
			}
			if (item._UserProfileData != null && item._UserProfileData.AvatarInfo != null && item._UserProfileData.AvatarInfo.Achievements != null)
			{
				UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(item._UserProfileData.AvatarInfo.Achievements, 12);
				KAWidget kAWidget3 = kAWidget.FindChildItem("UDTStarsIcon");
				if (kAWidget3 != null)
				{
					kAWidget3.SetVisibility(inVisible: true);
					UDTUtilities.UpdateUDTStars(kAWidget3.transform, userAchievementInfoByType, "StarsIconBkg", "StarsIconFrameBkg");
				}
				KAWidget kAWidget4 = kAWidget.FindChildItem("TxtUDTPoints");
				if (kAWidget4 != null)
				{
					int num = 0;
					if (userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue)
					{
						num = userAchievementInfoByType.AchievementPointTotal.Value;
					}
					kAWidget4.SetText(num.ToString());
					kAWidget4.SetVisibility(inVisible: true);
				}
			}
			mProfileMenu.AddWidget(kAWidget);
			item._UiItemName = kAWidget.name;
			if (item == mCurrentChild)
			{
				mProfileMenu.SetSelectedItem(kAWidget);
			}
		}
		if (Money.pIsReady)
		{
			mGemTotalTxt.SetText(Money.pCashCurrency.ToString());
		}
		if (!UiLogin.pIsGuestUser)
		{
			CreateUnOccupiedSlots();
		}
	}

	public void OnMoneyUpdated()
	{
		if (mGemTotalTxt != null)
		{
			mGemTotalTxt.SetText(Money.pCashCurrency.ToString());
		}
	}

	public bool CanDelete()
	{
		int num = mChildListInfo.UserProfiles.Length;
		if (!SubscriptionInfo.pIsMember)
		{
			int quantity = ParentData.pInstance.pInventory.GetQuantity(_SlotItemID);
			if (quantity == 0 && num <= 1)
			{
				string localizedString = _NonMemberLastSlotDeleteText.GetLocalizedString();
				localizedString = localizedString.Replace("{{COUNT}}", _CostForDeletion.ToString());
				ConfirmationDialog(localizedString, yes: true, no: true, ok: false, close: false, _SecurityTitleText.GetLocalizedString(), "", "DeleteProfile");
				return false;
			}
			if (quantity > 0 && num == 1)
			{
				string localizedString2 = _CannotDeleteOneActiveSlotText.GetLocalizedString();
				DeleteProfileDialog(localizedString2, yes: false, no: false, ok: true, close: false, _SecurityTitleText.GetLocalizedString(), "");
				return false;
			}
		}
		return true;
	}

	private void CreateUnOccupiedSlots()
	{
		ProfileSlotsData profileSlotData = MonetizationData.GetProfileSlotData();
		if (profileSlotData == null)
		{
			return;
		}
		string text = "UnoccupiedSlot_";
		int num = mChildListInfo.UserProfiles.Length;
		int quantity = ParentData.pInstance.pInventory.GetQuantity(_FreeSlotID);
		int quantity2 = ParentData.pInstance.pInventory.GetQuantity(_SlotItemID);
		int num2 = profileSlotData.mFreeSlotCount + profileSlotData.mMemberFreeSlotCount + mSlotInventoryMax + quantity;
		int num3 = profileSlotData.mFreeSlotCount;
		bool flag = ParentData.pInstance.HasItem(_MemberSlotItemID) || (SubscriptionInfo.pIsMember && !SubscriptionInfo.pIsTrialMember);
		if (flag)
		{
			num3 += profileSlotData.mMemberFreeSlotCount;
		}
		int num4 = num3 + quantity2 + quantity - num;
		for (int i = 0; i < num4; i++)
		{
			CreateAndAddUnOccupiedProfileMenuItem(text + "CreateProfile", _OpenSlotText.GetLocalizedString(), ProfileSlotState.CREATE_PROFILE).FindChildItem("LockInfo").GetUITexture().gameObject.SetActive(value: false);
		}
		int num5 = num2 - (num4 + num);
		if (flag)
		{
			for (int j = 0; j < num5; j++)
			{
				CreateAndAddUnOccupiedProfileMenuItem(text + "Buy", _PurchaseSlotText.GetLocalizedString(), ProfileSlotState.BUY_WITH_VCASH);
			}
			return;
		}
		for (int k = 0; k < profileSlotData.mMemberFreeSlotCount; k++)
		{
			CreateAndAddUnOccupiedProfileMenuItem(text + "BecomeMember", _MemberOnlySlotText.GetLocalizedString(), ProfileSlotState.MEMBER_UNLOCK);
		}
		for (num5 -= profileSlotData.mMemberFreeSlotCount; num5 > 0; num5--)
		{
			CreateAndAddUnOccupiedProfileMenuItem(text + "Buy", _PurchaseSlotText.GetLocalizedString(), ProfileSlotState.BUY_WITH_VCASH);
		}
	}

	private KAWidget CreateAndAddUnOccupiedProfileMenuItem(string inBtnName, string inBtnText, ProfileSlotState inSlotState)
	{
		KAWidget kAWidget = DuplicateWidget("TemplateAvatarNames");
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.SetState(KAUIState.INTERACTIVE);
		kAWidget.FindChildItem("AvatarInfo").SetVisibility(inVisible: false);
		KAWidget kAWidget2 = kAWidget.FindChildItem("LockInfo");
		kAWidget2.SetVisibility(inVisible: true);
		KAWidget kAWidget3 = kAWidget.FindChildItem("GemsInfo");
		kAWidget3.SetInteractive(isInteractive: false);
		if (inSlotState == ProfileSlotState.BUY_WITH_VCASH)
		{
			kAWidget3.SetVisibility(inVisible: true);
			kAWidget3.FindChildItem("TxtGems").SetText(mSlotCost.ToString());
		}
		else
		{
			kAWidget3.SetVisibility(inVisible: false);
		}
		kAWidget.FindChildItem("DeleteBtn").SetVisibility(inVisible: false);
		kAWidget.name = inBtnName;
		kAWidget2.SetText(inBtnText);
		ProfileSlotUserData profileSlotUserData = new ProfileSlotUserData();
		profileSlotUserData.mProfileSlotState = inSlotState;
		kAWidget.SetUserData(profileSlotUserData);
		mProfileMenu.AddWidget(kAWidget);
		return kAWidget;
	}

	private void ShowBusy(bool isBusy)
	{
		if (isBusy)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		SetInteractive(!isBusy);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		string text = item.name;
		if (text.Contains("BtnProfile"))
		{
			if ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && !UtUtilities.IsConnectedToWWW())
			{
				return;
			}
			{
				foreach (ChildInfo item2 in mChildInfo)
				{
					if (mCurrentChild != item2 && item2._UiItemName.Equals(text))
					{
						_QuickLaunchGroup.SetActive(value: false);
						mIsUiStateSet = false;
						SetState(KAUIState.DISABLED);
						ResetPreviousData();
						mCurrentChild = item2;
						ShowBusy(isBusy: true);
						AppCenter.SetUserId(mCurrentChild._UserID);
						WsWebService.LoginChild(ProductConfig.pToken, mCurrentChild._UserID, UtUtilities.GetLocaleLanguage(), ServiceEventHandler, item2);
						break;
					}
				}
				return;
			}
		}
		if (item == mSelectBtn)
		{
			if ((!UtPlatform.IsMobile() && !UtPlatform.IsWSA()) || UtUtilities.IsConnectedToWWW())
			{
				if (UtPlatform.IsMobile())
				{
					AdManager.DisplayAd(AdEventType.PLAY_TAPPED, AdOption.FULL_SCREEN);
				}
				GotoGame();
			}
			return;
		}
		if (item == mCreateBtn)
		{
			mAvatarDataFailed = true;
			UiAvatarCustomization.pChildToken = WsWebService.pUserToken;
			ProcessProfileCreation(AvatarData.pInstanceInfo.mInstance);
			return;
		}
		if (item == mBackBtn)
		{
			ResetPet();
			if (AvatarData.pInstanceInfo != null)
			{
				AvatarData.pInstanceInfo.UnloadBundleData();
			}
			AvatarData.Reset();
			UnityEngine.Object.Destroy(AvAvatar.pObject);
			SetVisibility(inVisible: false);
			if (PlayerPrefs.GetInt("SafeAppClose") == 2)
			{
				PlayerPrefs.SetInt("SafeAppClose", 1);
			}
			RsResourceManager.LoadLevel(GameConfig.GetKeyData("LoginScene"));
			mIsCustomizationScreen = false;
			return;
		}
		if (text.Contains("UnoccupiedSlot_"))
		{
			_QuickLaunchGroup.SetActive(value: false);
			if ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && !UtUtilities.IsConnectedToWWW())
			{
				return;
			}
			mSelectBtn.SetDisabled(isDisabled: true);
			ProfileSlotUserData profileSlotUserData = (ProfileSlotUserData)item.GetUserData();
			if (profileSlotUserData.mProfileSlotState == ProfileSlotState.CREATE_PROFILE)
			{
				mAvatarDataFailed = false;
				ProcessProfileCreation(null);
			}
			else if (profileSlotUserData.mProfileSlotState == ProfileSlotState.BUY_WITH_VCASH)
			{
				if (UtPlatform.IsMobile())
				{
					AdManager.DisplayAd(AdEventType.GEMS_SLOT, AdOption.FULL_SCREEN);
				}
				ProcessBuySlot();
			}
			else if (profileSlotUserData.mProfileSlotState == ProfileSlotState.MEMBER_UNLOCK)
			{
				if (UtPlatform.IsMobile())
				{
					AdManager.DisplayAd(AdEventType.MEMBER_SLOT, AdOption.FULL_SCREEN);
				}
				ProcessNonMemberRegisterUnlock();
			}
			return;
		}
		if (text.Equals("DeleteBtn"))
		{
			if ((!UtPlatform.IsMobile() && !UtPlatform.IsWSA()) || UtUtilities.IsConnectedToWWW())
			{
				KAWidget parentItem = item.GetParentItem();
				mDeleteProfileUserID = parentItem.name.Remove(0, 11);
				if (UserInfo.pInstance != null && CanDelete())
				{
					ConfirmationDialog(_DeleteProfileConfirmationText.GetLocalizedString(), yes: true, no: true, ok: false, close: false, _SecurityTitleText.GetLocalizedString(), "", "DeleteProfile");
				}
			}
			return;
		}
		if (item == mGemTotal)
		{
			ProceedToStore();
			return;
		}
		if (item == mWhatsNewBtn)
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			RsResourceManager.LoadAssetFromBundle(_NewFeaturesDBAssetPath, _NewFeaturesAssetName, OnFeaturesDBLoaded, typeof(GameObject));
			return;
		}
		if (item == mBtnMMO)
		{
			mBtnMMO.SetChecked(!mBtnMMO.IsChecked());
			if (!UserInfo.pInstance.MultiplayerEnabled)
			{
				SetVisibility(inVisible: false);
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _MultiplayerDisabledOnServerText.GetLocalizedString(), _ServerErrorTitleText.GetLocalizedString(), base.gameObject, null, null, "OnClose", null, inDestroyOnClick: true);
			}
			else if (mBtnMMO.IsChecked())
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _MMOEnableWarningText.GetLocalizedString(), _MMOTitleText.GetLocalizedString(), base.gameObject, "OnMMOChangeYes", "OnMMOChangeNo", null, null, inDestroyOnClick: true);
			}
			else
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _MMODisableWarningText.GetLocalizedString(), _MMOTitleText.GetLocalizedString(), base.gameObject, "OnMMOChangeYes", "OnMMOChangeNo", null, null, inDestroyOnClick: true);
			}
			return;
		}
		if (item == mOptionsBtn)
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = GameConfig.GetKeyData("OptionsAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OptionsBundleReady, typeof(GameObject));
			return;
		}
		foreach (QuickLaunchBtnInfo item3 in _QuickLaunchBtnInfo)
		{
			if (item == item3._UnLockedButton)
			{
				mSelectedQuickLaunchInfo = item3._LoadInfo;
				if (item.name.Contains("Stable"))
				{
					UserNotifyLoadStableQuest.StartStableQuestOnStableLoad = true;
					SanctuaryManager.pMountedState = false;
					GotoGame();
					break;
				}
				if (item.name.Contains("Race"))
				{
					TimedMissionManager.Init();
					UnlockInfo[] unlockInfo = item3._UnlockInfo;
					foreach (UnlockInfo unlockInfo2 in unlockInfo)
					{
						if (unlockInfo2._UnlockType == UnlockType.DRAGON_STAGE)
						{
							mSelectedQuickLaunchValue = unlockInfo2._UnlockValue;
						}
					}
					KAUICursorManager.SetDefaultCursor("Loading");
					break;
				}
				if (item.name.Contains("Farm"))
				{
					FarmManager.pCurrentFarmData = null;
					GotoGame();
				}
				else
				{
					GotoGame();
				}
			}
			if (item == item3._LockedButton && !string.IsNullOrEmpty(item3.pLockedButtonText))
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", item3.pLockedButtonText, null, "");
				break;
			}
		}
	}

	private void OptionsBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserdata)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			UiOptions component = obj.GetComponent<UiOptions>();
			if (component != null)
			{
				component._BundlePath = inURL;
			}
			obj.name = "PfUiOptions";
			component.DisableWidgetsForVikingSelect(enable: true);
			UiOptions.OnOptions = OnOptionsClosed;
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private void OnOptionsClosed()
	{
		UiOptions.OnOptions = null;
		SetInteractive(interactive: true);
	}

	private void OnClose()
	{
		SetVisibility(inVisible: true);
	}

	private void OnMMOChangeNo()
	{
	}

	private void OnMMOChangeYes()
	{
		mMMOStatus = !mMMOStatus;
		mBtnMMO.SetChecked(!mBtnMMO.IsChecked());
		PlayerPrefs.SetInt("USE_MMO" + UserInfo.pInstance.UserID, mMMOStatus ? 1 : 0);
	}

	public void OnFeaturesDBLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		if (inLoadEvent == RsResourceLoadEvent.COMPLETE)
		{
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject).name = _NewFeaturesAssetName;
		}
	}

	private void DeleteProfile()
	{
		if (!string.IsNullOrEmpty(mDeleteProfileUserID))
		{
			WsWebService.DeleteProfile(mDeleteProfileUserID, ServiceEventHandler, null);
			ShowBusy(isBusy: true);
		}
		else
		{
			ShowBusy(isBusy: false);
		}
	}

	private void GotoGame()
	{
		WsTokenMonitor.pCheckToken = true;
		if (_MessageObject != null)
		{
			SetInteractive(interactive: false);
			if (mCurrentChild != null)
			{
				PlayerPrefs.SetString("LAST_PLAYED_CHILD_NAME", mCurrentChild._Name);
			}
			if (mIsPetAvailable && mPetManagerGO != null && ProductData.pIsReady && !ProductData.pPairData.KeyExists("FSUpdatePairData"))
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				mPetManagerGO.GetComponent<ProfilePetManager>().UpdateObsCoursePairData();
			}
			else
			{
				OnObsCoursePairDataSaveDone();
			}
		}
		AnalyticAgent.LogFTUEEvent(FTUEEvent.GOTOGAME);
	}

	public void OnObsCoursePairDataSaveDone()
	{
		if (ProductData.pIsReady && !ProductData.pPairData.KeyExists("FSUpdatePairData"))
		{
			ProductData.pPairData.SetValueAndSave("FSUpdatePairData", "done");
		}
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage(_ProfileSelectedMessage, mSelectedQuickLaunchInfo, SendMessageOptions.RequireReceiver);
			mSelectedQuickLaunchInfo = new LoadInfo();
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	private void ResetPreviousData()
	{
		if ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && AvAvatar.pObject != null)
		{
			AvatarData.pInstanceInfo.UnloadBundleData();
		}
		if (AvAvatar.pObject != null)
		{
			UnityEngine.Object.Destroy(AvAvatar.pObject);
			AvAvatar.pObject = null;
		}
		AvatarData.Reset();
		UserInfo.Reset();
		UserProfile.Reset();
		UserRankData.Reset();
		PairData.Reset();
		Money.ReInit();
		ProductData.Reset();
		TimedMissionManager.Reset();
		CommonInventoryData.Reset();
		DailyBonusAndPromo.ResetShowcasedOffers();
		mSelectBtn.SetState(KAUIState.DISABLED);
		mCreateBtn.SetState(KAUIState.DISABLED);
		ResetPet();
		mAvatarReady = false;
		mIsPetLoadingDone = false;
		mIsPetAvailable = false;
	}

	private void ShowAvatarCustomizationScreen()
	{
		KillGenericDB();
		ResetPet();
		SetVisibility(inVisible: false);
		ShowBusy(isBusy: false);
		RsResourceManager.LoadAssetFromBundle(_CustomizationBundleURL, _CustomizationAssetName, OnAvatarCustomizationLoaded, typeof(GameObject));
		mIsCustomizationScreen = true;
	}

	public void OnAvatarCustomizationLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			UiAvatarCustomization.pSkipLoginChild = mAvatarDataFailed;
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			gameObject.name = _CustomizationAssetName;
			mUiAvatarCustomization = gameObject.GetComponentInChildren<UiAvatarCustomization>();
			mUiAvatarCustomization.pIsNewProfile = true;
			mUiAvatarCustomization.pDefaultTabIndex = mUiAvatarCustomization.AvatarTabIndex;
			mUiAvatarCustomization.pUiSelectProfile = this;
			mUiAvatarCustomization._CloseMsgObject = base.gameObject;
			AnalyticAgent.LogFTUEEvent(FTUEEvent.CHARACTER_CUSTOMIZATION_STARTED);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	public void AvatarCreatorClosed(bool inProfileCreated)
	{
		UtDebug.Log("Avatar creator closed new!");
		if (!mFirstLaunch && !UiLogin.pIsGuestUser)
		{
			SetVisibility(inVisible: true);
		}
		mIsCustomizationScreen = false;
		mAvatarDataFailed = false;
		UiAvatarCustomization.pChildToken = string.Empty;
	}

	private void InitProfile(ChildInfo child)
	{
		if (child._UserProfileData == null || child._UserProfileData.AvatarInfo.AvatarData == null || pAvatarDataNull)
		{
			AvAvatar.Init();
		}
		else
		{
			UserProfile.Init(child._UserProfileData);
		}
		ProductData.Init();
		ServerTime.Init();
		CommonInventoryData.Init();
	}

	protected override void Update()
	{
		base.Update();
		if (!mAvatarReady && mIsReady && !mIsCustomizationScreen && AvatarData.pIsReady && AvAvatar.pObject != null && UserInfo.pIsReady && ProductData.pIsReady && WsUserMessage.pIsReady && mIsPetLoadingDone && mDataLoadingCount <= 0)
		{
			mAvatarReady = true;
			if (mBtnMMO != null && mBtnMMO.GetVisibility())
			{
				string key = "USE_MMO" + UserInfo.pInstance.UserID;
				mMMOStatus = UserInfo.pInstance.MultiplayerEnabled && (PlayerPrefs.HasKey(key) ? (PlayerPrefs.GetInt(key, 1) == 1) : UtPlatform.GetMMODefaultState());
				mBtnMMO.SetChecked(!mMMOStatus);
			}
			if (!AvatarData.pInitializedFromPreviousSave || !AvatarData.pInstanceInfo.CheckVersion())
			{
				AvatarLoadEventHandler(inStatus: false);
			}
			else
			{
				AvatarLoadEventHandler(inStatus: true);
			}
			AvatarData.SetDisplayNameVisible(AvAvatar.pObject, inVisible: false, SubscriptionInfo.pIsMember);
			if (!mCreateBtn.GetVisibility())
			{
				if (mIsPetAvailable || _AvatarStartMarkerAlone == null)
				{
					AvAvatar.position = _AvatarStartMarker.transform.position;
					AvAvatar.mTransform.rotation = _AvatarStartMarker.transform.rotation;
				}
				else
				{
					AvAvatar.position = _AvatarStartMarkerAlone.transform.position;
					AvAvatar.mTransform.rotation = _AvatarStartMarkerAlone.transform.rotation;
				}
			}
			if (!UiLogin.pIsGuestUser)
			{
				RsResourceManager.DestroyLoadScreen();
			}
			if (!mFirstLaunch && !UiLogin.pIsGuestUser && !GetVisibility())
			{
				SetVisibility(inVisible: true);
			}
			if (mFirstLaunch || UiLogin.pIsGuestUser)
			{
				AvAvatar.SetActive(inActive: false);
			}
			if (WsUserMessage.pInstance != null)
			{
				WsUserMessage.pInstance.ShowMessage(35, delete: true);
			}
		}
		if (!mInitChildList && ParentData.pIsReady && WsUserMessage.pIsReady)
		{
			mInitChildList = true;
			GetDetailedChildList();
		}
		if (!mIsReady && mChildListInfo != null && !mIsCustomizationScreen && UserProfile.pProfileData != null && Money.pIsReady && SubscriptionInfo.pIsReady && MonetizationData.pIsReady && ParentData.pIsReady && mStoreDataLoaded && mDataLoadingCount <= 0 && UserRankData.pIsReady)
		{
			mIsReady = true;
			ChildInfo childByUserID = GetChildByUserID(UserProfile.pProfileData.AvatarInfo.UserInfo.UserID);
			if ((childByUserID != null && (childByUserID._UserProfileData == null || childByUserID._UserProfileData.AvatarInfo.AvatarData == null)) || pAvatarDataNull)
			{
				childByUserID._UserProfileData = UserProfile.pProfileData;
			}
			if (childByUserID._UserProfileData.AvatarInfo != null && childByUserID._UserProfileData.AvatarInfo.AvatarData == null)
			{
				pAvatarDataNull = true;
				ProcessProfileCreation(null, resetPreviousData: false);
				return;
			}
			UpdateProfileButtons();
			LogEvents();
		}
		if (!mIsReady && !mIsCustomizationScreen && GetState() == KAUIState.INTERACTIVE)
		{
			SetState(KAUIState.NOT_INTERACTIVE);
			mSelectBtn.SetState(KAUIState.DISABLED);
			mCreateBtn.SetState(KAUIState.DISABLED);
			ShowBusy(isBusy: true);
		}
		if (mAvatarReady && mIsPetLoadingDone && !mIsUiStateSet && mIsReady && ServerTime.pIsReady && CommonInventoryData.pIsReady && EventManager.pIsReady)
		{
			mIsUiStateSet = true;
			_QuickLaunchGroup.SetActive(value: true);
			InitQuickLaunchButtons();
			SetState(KAUIState.INTERACTIVE);
			ShowBusy(isBusy: false);
			mSelectBtn.SetInteractive(isInteractive: true);
			mCreateBtn.SetInteractive(isInteractive: true);
			if (IAPManager.pInstance.UnAssignedReceiptPending() && mChildInfo != null && mChildInfo.Count > 0)
			{
				UiSelectViking.Init(OnSelectItemNameProcessed, IAPManager.pInstance._IAPUnAssignedReceiptsText.GetLocalizedString(), mChildInfo);
			}
			if (!mAvatarDataErrorDBSet)
			{
				KillGenericDB();
			}
			if (!mServerDownCheckDone)
			{
				CheckServerDownTime();
				mServerDownCheckDone = true;
			}
			if (UiLogin.pIsGuestUser || mFirstLaunch)
			{
				if (mUiAvatarCustomization != null)
				{
					UnityEngine.Object.Destroy(mUiAvatarCustomization.gameObject);
				}
				mFirstLaunch = false;
				GotoGame();
			}
			CheckForMembershipStatus();
			if (PlayfabManager<PlayFabManagerDO>.Instance != null)
			{
				PlayfabManager<PlayFabManagerDO>.Instance.UpdateCharacterStatistics("Dragons", RaisedPetData.GetActiveDragons().Count);
			}
		}
		if (mAvatarReady && mIsPetLoadingDone && TimedMissionManager.pIsReady && !string.IsNullOrEmpty(mSelectedQuickLaunchValue))
		{
			if (!IsDragonBusy(mSelectedQuickLaunchValue))
			{
				GotoGame();
			}
			else
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _DragonBusyText._Text, null, "");
				mSelectedQuickLaunchInfo = new LoadInfo();
			}
			mSelectedQuickLaunchValue = string.Empty;
		}
	}

	private void OnSelectItemNameProcessed(UiSelectViking.Status status, ChildInfo child)
	{
		if (status == UiSelectViking.Status.Accepted)
		{
			KAUICursorManager.SetExclusiveLoadingGear(status: true);
			IAPManager.pInstance.ProcessUnAssignedReceipts(child._UserID, OnUnAssignedReceiptSyncDone);
		}
	}

	private void OnUnAssignedReceiptSyncDone(bool done)
	{
		KAUICursorManager.SetExclusiveLoadingGear(status: false);
		GameUtilities.DisplayOKMessage("PfKAUIGenericDB", IAPManager.pInstance._IAPSynchSuccessText.GetLocalizedString(), null, "");
	}

	private void LogEvents()
	{
		if (PlayfabManager<PlayFabManagerDO>.Instance != null)
		{
			int pCashCurrency = Money.pCashCurrency;
			PlayfabManager<PlayFabManagerDO>.Instance.UpdatePlayerStatistics("Gems", pCashCurrency);
			int value = ((SubscriptionInfo.pInstance.BillFrequency > 0) ? 1 : 0);
			PlayfabManager<PlayFabManagerDO>.Instance.UpdatePlayerStatistics("Member", value);
		}
	}

	private void InitQuickLaunchButtons()
	{
		foreach (QuickLaunchBtnInfo item in _QuickLaunchBtnInfo)
		{
			int num = 0;
			UnlockInfo[] unlockInfo = item._UnlockInfo;
			foreach (UnlockInfo unlockInfo2 in unlockInfo)
			{
				bool flag = false;
				if (!string.IsNullOrEmpty(unlockInfo2._UnlockValue))
				{
					switch (unlockInfo2._UnlockType)
					{
					case UnlockType.TUTORIAL:
						flag = ProductData.TutorialComplete(unlockInfo2._UnlockValue);
						break;
					case UnlockType.DRAGON_STAGE:
						flag = IsDragonAvailable(unlockInfo2._UnlockValue);
						break;
					}
					if (!flag)
					{
						item.pLockedButtonText = unlockInfo2._LockedText.GetLocalizedString();
					}
					else
					{
						num++;
					}
				}
			}
			bool flag2 = false;
			if (item._CheckAllConditions && num == item._UnlockInfo.Length)
			{
				flag2 = true;
			}
			else if (!item._CheckAllConditions && num > 0)
			{
				flag2 = true;
			}
			item._LockedButton.SetVisibility(!flag2);
			item._UnLockedButton.SetVisibility(flag2);
		}
	}

	private bool IsDragonAvailable(string growthStage)
	{
		int result = -1;
		int.TryParse(growthStage, out result);
		if (RaisedPetData.pActivePets == null)
		{
			return false;
		}
		foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
		{
			if (value == null)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(raisedPetData.PetTypeID);
				int ageIndex = RaisedPetData.GetAgeIndex(raisedPetData.pStage);
				if (!sanctuaryPetTypeInfo._Flightless && ageIndex >= sanctuaryPetTypeInfo._MinAgeToMount && ageIndex >= result && !SanctuaryManager.IsPetLocked(raisedPetData))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsDragonBusy(string growthStage)
	{
		int result = -1;
		int.TryParse(growthStage, out result);
		if (RaisedPetData.pActivePets == null)
		{
			return true;
		}
		foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
		{
			if (value == null)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(raisedPetData.PetTypeID);
				int ageIndex = RaisedPetData.GetAgeIndex(raisedPetData.pStage);
				if (!sanctuaryPetTypeInfo._Flightless && ageIndex >= sanctuaryPetTypeInfo._MinAgeToMount && ageIndex >= result && !TimedMissionManager.pInstance.IsPetEngaged(raisedPetData.RaisedPetID))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void ResetPet()
	{
		if (!(null == mPetManagerGO))
		{
			ProfilePetManager component = mPetManagerGO.GetComponent<ProfilePetManager>();
			if (component._CurPetInstance != null)
			{
				UnityEngine.Object.Destroy(component._CurPetInstance.gameObject);
			}
			UnityEngine.Object.Destroy(mPetManagerGO);
			mPetManagerGO = null;
			mIsPetLoadingDone = false;
			mIsPetAvailable = false;
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_DETAILED_CHILD_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					UpdateChildList((UserProfileDataList)inObject);
					if (UiLogin.pHasUserJustRegistered)
					{
						ProcessProfileCreation(null);
						break;
					}
					string text2 = null;
					if (PlayerPrefs.HasKey("LAST_PLAYED_CHILD_NAME"))
					{
						text2 = PlayerPrefs.GetString("LAST_PLAYED_CHILD_NAME");
						if (GetChildByName(text2) == null)
						{
							text2 = null;
						}
					}
					SelectProfile(text2);
				}
				else
				{
					UtDebug.Log("GET_DETAILED_CHILD_LIST return null!!");
					mChildListInfo = null;
					mChildInfo.Clear();
					mFirstLaunch = true;
					ProcessProfileCreation(null);
				}
				break;
			case WsServiceEvent.ERROR:
				Debug.LogError("GET_DETAILED_CHILD_LIST returned ERROR!!");
				SetState(KAUIState.DISABLED);
				break;
			}
			break;
		case WsServiceType.DELETE_PROFILE:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject == null)
				{
					break;
				}
				switch ((DeleteProfileStatus)inObject)
				{
				case DeleteProfileStatus.SUCCESS:
					DeleteProfileDialog(_ProfileDeletionSuccessText.GetLocalizedString(), yes: false, no: false, ok: true, close: false, _SecurityTitleText.GetLocalizedString(), "DeleteProfileSuccess");
					KAUICursorManager.SetDefaultCursor("Arrow");
					if (PlayfabManager<PlayFabManagerDO>.Instance != null)
					{
						PlayfabManager<PlayFabManagerDO>.Instance.DeleteCharacter(mDeleteProfileUserID);
					}
					break;
				case DeleteProfileStatus.ALREADY_DELETED:
				case DeleteProfileStatus.NOT_A_PROFILE_ACCOUNT:
				case DeleteProfileStatus.OWNER_ID_NOT_FOUND:
				case DeleteProfileStatus.PROFILE_NOT_OWNED_BY_THIS_OWNER:
				case DeleteProfileStatus.PROFILE_NOT_FOUND:
				case DeleteProfileStatus.PROFILE_CANNOT_BE_DELETED:
				case DeleteProfileStatus.ERROR:
					DeleteProfileDialog(_ProfileDeletionErrorText.GetLocalizedString(), yes: false, no: false, ok: true, close: false, _SecurityTitleText.GetLocalizedString(), "DeleteProfileFailure");
					KAUICursorManager.SetDefaultCursor("Arrow");
					break;
				case DeleteProfileStatus.IN_SUFFICIENT_VCASH_FUNDS:
					DeleteProfileDialog(_NotEnoughVCashToDeleteText.GetLocalizedString(), yes: false, no: false, ok: true, close: false, _SecurityTitleText.GetLocalizedString(), "DeleteProfileFailure");
					KAUICursorManager.SetDefaultCursor("Arrow");
					break;
				}
				break;
			case WsServiceEvent.ERROR:
				DeleteProfileDialog(_NotEnoughVCashText.GetLocalizedString(), yes: false, no: false, ok: true, close: false, _SecurityTitleText.GetLocalizedString(), "DeleteProfileFailure");
				KAUICursorManager.SetDefaultCursor("Arrow");
				break;
			}
			break;
		case WsServiceType.LOGIN_CHILD:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					string text = (string)inObject;
					if (text != "")
					{
						WsWebService.SetToken(text);
						InitProfile((ChildInfo)inUserData);
						if (null != _PetManager)
						{
							mPetManagerGO = UnityEngine.Object.Instantiate(_PetManager);
							mPetManagerGO.GetComponent<ProfilePetManager>()._ProfileSelectUi = this;
						}
						if (PlayfabManager<PlayFabManagerDO>.Instance != null)
						{
							RegisterChildRequest request = PlayfabManager<PlayFabManagerDO>.Instance.CreateRegisterChildRequest((ChildInfo)inUserData, text);
							PlayfabManager<PlayFabManagerDO>.Instance.RegisterChild(request);
						}
					}
					else
					{
						InitProfile((ChildInfo)inUserData);
						UtDebug.LogError("WEB SERVICE CALL LoginChild returned empty!!!");
					}
				}
				else
				{
					ShowBusy(isBusy: false);
					UtDebug.LogError("WEB SERVICE CALL LoginChild returned NULL!!!");
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL LoginChild FAILED!!!");
				ShowBusy(isBusy: false);
				break;
			}
			break;
		}
	}

	private void OnClickOK()
	{
		KillGenericDB();
	}

	private void ProcessProfileCreation(AvatarData inData, bool resetPreviousData = true)
	{
		if (resetPreviousData)
		{
			ResetPreviousData();
		}
		AvAvatar.CreateDefault(inData);
		AvAvatar.SetPosition(Vector3.up * 5000f);
		ShowAvatarCustomizationScreen();
	}

	private void ProcessBuySlot()
	{
		if (Money.pIsReady)
		{
			KillGenericDB();
			if (Money.pCashCurrency < mSlotCost)
			{
				mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Buy Profile Slot");
				mKAUIGenericDB.SetTitle(_BuyProfileSlotTitleText.GetLocalizedString());
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
				mKAUIGenericDB.SetText(_NotEnoughVCashText.GetLocalizedString(), interactive: false);
				mKAUIGenericDB._MessageObject = base.gameObject;
				mKAUIGenericDB._YesMessage = "ProceedToStore";
				mKAUIGenericDB._NoMessage = "EnablePreviousProfile";
				KAUI.SetExclusive(mKAUIGenericDB);
			}
			else
			{
				mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Buy Profile Slot");
				mKAUIGenericDB.SetTitle(_BuyProfileSlotTitleText.GetLocalizedString());
				mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
				string localizedString = _BuyProfileConfirmationText.GetLocalizedString();
				localizedString = localizedString.Replace("{{COUNT}}", mSlotCost.ToString());
				mKAUIGenericDB.SetText(localizedString, interactive: false);
				mKAUIGenericDB._MessageObject = base.gameObject;
				mKAUIGenericDB._YesMessage = "PurchaseProfileSlot";
				mKAUIGenericDB._NoMessage = "EnablePreviousProfile";
				KAUI.SetExclusive(mKAUIGenericDB);
			}
		}
	}

	private void PurchaseProfileSlot()
	{
		KillGenericDB();
		if (!UtUtilities.IsConnectedToWWW())
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Profile Purchase Done");
			mKAUIGenericDB.SetTitle(_BuyProfileSlotTitleText.GetLocalizedString());
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mKAUIGenericDB.SetText(_SlotPurchaseFailedText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._OKMessage = "KillGenericDB";
			KAUI.SetExclusive(mKAUIGenericDB);
		}
		else
		{
			ParentData.pInstance.pInventory.AddPurchaseItem(_SlotItemID, 1, ItemPurchaseSource.PROFILE_SELECTION.ToString());
			ParentData.pInstance.pInventory.DoPurchase(2, mSlotStoreID, SlotPurchaseDone);
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Process Profile Slot");
			mKAUIGenericDB.SetTitle(_BuyProfileSlotTitleText.GetLocalizedString());
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB.SetText(_SlotPurchaseProcessingText.GetLocalizedString(), interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	private void ProcessNonMemberRegisterUnlock()
	{
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Member Profile");
		mKAUIGenericDB.SetTitle(_MemberSlotTitleText.GetLocalizedString());
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB.SetText(_MemberSlotUnlockText.GetLocalizedString(), interactive: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._NoMessage = "EnablePreviousProfile";
		mKAUIGenericDB._YesMessage = "BecomeMember";
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void DeleteProfileDialog(string message, bool yes, bool no, bool ok, bool close, string title, string okCallbackMethod, string yesCallbackMethod = "", string noCallbackMethod = "")
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", title);
		mKAUIGenericDB.SetButtonVisibility(yes, no, ok, close);
		mKAUIGenericDB.SetText(message, interactive: false);
		mKAUIGenericDB.SetTitle(title);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = okCallbackMethod;
		mKAUIGenericDB._YesMessage = yesCallbackMethod;
		mKAUIGenericDB._NoMessage = noCallbackMethod;
		mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void ConfirmationDialog(string message, bool yes, bool no, bool ok, bool close, string title, string okCallbackMethod, string yesCallbackMethod = "", string noCallbackMethod = "")
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfUiConfirmationDB", title);
		UiConfirmationDB obj = (UiConfirmationDB)mKAUIGenericDB;
		string localizedString = _SecurityHeadingText.GetLocalizedString();
		localizedString = localizedString.Replace("{{SECURITY_WORD}}", _SecurityWordText.GetLocalizedString());
		obj.SetSecurityText(localizedString, _SecurityWordText.GetLocalizedString());
		obj.SetErrorText(title, _NameEnteredErrorText.GetLocalizedString());
		obj.SetButtonVisibility(yes, no, ok, close);
		obj.SetText(message, interactive: false);
		obj.SetTitle(title);
		obj._MessageObject = base.gameObject;
		obj._OKMessage = okCallbackMethod;
		obj._YesMessage = yesCallbackMethod;
		obj._NoMessage = noCallbackMethod;
		obj.SetDestroyOnClick(isDestroy: true);
		KAUI.SetExclusive(obj);
	}

	private void DeleteProfileFailure()
	{
		ShowBusy(isBusy: false);
	}

	private void DeleteProfileSuccess()
	{
		ResetPreviousData();
		GetDetailedChildList();
	}

	private void PopMembershipStore()
	{
		KillGenericDB();
		if (!IAPManager.pIsAvailable)
		{
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mKAUIGenericDB.SetText(_StoreUnavailableText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._OKMessage = "KillGenericDB";
			KAUI.SetExclusive(mKAUIGenericDB);
		}
		else if (UtUtilities.IsConnectedToWWW())
		{
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
		}
	}

	private void ProceedToStore()
	{
		KillGenericDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void ShowGenericDB(string dbName, string dbTitle, string dbText, string yesMessage, string noMessage, string okMessage, string closeMessage)
	{
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", dbName);
		if (!string.IsNullOrEmpty(dbTitle))
		{
			mKAUIGenericDB.SetTitle(dbTitle);
		}
		mKAUIGenericDB.SetText(dbText, interactive: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._YesMessage = yesMessage;
		mKAUIGenericDB._NoMessage = noMessage;
		mKAUIGenericDB._OKMessage = okMessage;
		mKAUIGenericDB._CloseMessage = closeMessage;
		mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void KillAvatarDataErrorDB()
	{
		mAvatarDataErrorDBSet = false;
		KillGenericDB();
	}

	protected void OnStoreClosed()
	{
		AvAvatar.pState = AvAvatar.pPrevState;
		CompleteSlotPurchase();
	}

	private void CompleteSlotPurchase()
	{
		if (Money.pCashCurrency >= mSlotCost)
		{
			PurchaseProfileSlot();
		}
	}

	public void OnStoreLoaded(List<StoreData> storeList, object inUserData)
	{
		if (storeList != null)
		{
			mSlotStoreID = Convert.ToInt32(GameConfig.GetKeyData("ProfileSlotStoreID"));
			ItemData itemData = storeList.Find((StoreData s) => s._ID == mSlotStoreID)?.FindItem(_SlotItemID);
			if (itemData != null)
			{
				mSlotCost = itemData.FinalCashCost;
				mSlotInventoryMax = itemData.InventoryMax;
				mStoreDataLoaded = true;
			}
		}
		else
		{
			UtDebug.LogError("Failed to load profile store!");
		}
	}

	public void SlotPurchaseDone(CommonInventoryResponse ret)
	{
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Profile Purchase Done");
		mKAUIGenericDB.SetTitle(_BuyProfileSlotTitleText.GetLocalizedString());
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		if (ret != null && ret.Success)
		{
			UpdateProfileButtons();
			mKAUIGenericDB.SetText(_SlotPurchaseSuccessfulText.GetLocalizedString(), interactive: false);
			foreach (ChildInfo item in mChildInfo)
			{
				item._UserProfileData.GameCurrency = ret.UserGameCurrency.GameCurrency;
				item._UserProfileData.CashCurrency = ret.UserGameCurrency.CashCurrency;
			}
		}
		else
		{
			mKAUIGenericDB.SetText(_SlotPurchaseFailedText.GetLocalizedString(), interactive: false);
		}
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "SelectLastProfile";
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		mProfileMenu = null;
		_MenuList = null;
	}

	private void BecomeMember()
	{
		KillGenericDB();
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		if (!SubscriptionInfo.pIsMember || SubscriptionInfo.pIsTrialMember)
		{
			return;
		}
		foreach (KAWidget item in mProfileMenu.GetItems())
		{
			if (item.name.Contains("BecomeMember"))
			{
				item.name = "UnoccupiedSlot_CreateProfile";
				KAWidget kAWidget = item.FindChildItem("LockInfo");
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.SetText(_OpenSlotText.GetLocalizedString());
				kAWidget.GetUITexture().gameObject.SetActive(value: false);
				ProfileSlotUserData profileSlotUserData = new ProfileSlotUserData();
				profileSlotUserData.mProfileSlotState = ProfileSlotState.CREATE_PROFILE;
				item.SetUserData(profileSlotUserData);
				break;
			}
		}
	}

	private void UpdateUserProfileForUserID(ChildInfo inChildInfo)
	{
		if (mChildListInfo == null || inChildInfo == null)
		{
			return;
		}
		UserProfileData[] userProfiles = mChildListInfo.UserProfiles;
		foreach (UserProfileData userProfileData in userProfiles)
		{
			if (userProfileData.ID.Equals(inChildInfo._UserID))
			{
				inChildInfo._UserProfileData = userProfileData;
			}
		}
	}

	private void GetGroupForUserID(string userID)
	{
		mDataLoadingCount++;
		Group.Get(userID, GetEventHandler, userID);
	}

	private void GetEventHandler(GetGroupsResult result, object inUserData)
	{
		mDataLoadingCount--;
		if (result != null && result.Success)
		{
			string userID = (string)inUserData;
			ChildInfo childByUserID = GetChildByUserID(userID);
			if (childByUserID != null)
			{
				childByUserID._ChildGroup = result.Groups[0];
			}
		}
	}

	private void CheckServerDownTime()
	{
		if (ParentData.pIsReady && ServerDownScheduled.ShouldShowMessage(ParentData.pInstance.pUserInfo.UserID))
		{
			ServerDownMessage message = ServerDown.pInstance.Scheduled.GetMessage();
			if (message != null)
			{
				string text = message.Text;
				text = text.Replace("{from}", ServerDown.pInstance.Scheduled.StartTime.Trim());
				text = text.Replace("{to}", ServerDown.pInstance.Scheduled.EndTime.Trim());
				ShowGenericDB(message.Title, null, text, null, null, "KillGenericDB", null);
			}
		}
	}

	private void SelectLastProfile()
	{
		KillGenericDB();
		SelectProfile((mCurrentChild == null) ? null : mCurrentChild._Name);
	}

	private void EnablePreviousProfile()
	{
		KillGenericDB();
		foreach (KAWidget item in mProfileMenu.GetItems())
		{
			if (mCurrentChild != null && item.name == mCurrentChild._UiItemName)
			{
				mProfileMenu.SetSelectedItem(item);
			}
		}
		_QuickLaunchGroup.SetActive(value: true);
		mSelectBtn.SetDisabled(isDisabled: false);
	}

	private void CheckForMembershipStatus()
	{
		string value = ProductData.pPairData.GetValue("LastMemberDBShown");
		DateTime dateTime = DateTime.MinValue;
		if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
		{
			try
			{
				dateTime = DateTime.Parse(value, UtUtilities.GetCultureInfo("en-US"));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				ProductData.pPairData.SetValueAndSave("LastMemberDBShown", UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).ToString(UtUtilities.GetCultureInfo("en-US")));
			}
		}
		else
		{
			ProductData.pPairData.SetValueAndSave("LastMemberDBShown", UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).ToString(UtUtilities.GetCultureInfo("en-US")));
		}
		TimeSpan timeSpan = UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime) - dateTime;
		int intValue = ProductData.pPairData.GetIntValue("MemberDBWarning", 0);
		if (timeSpan.Days >= _MembershipWarningFreq && intValue < _MembershipWarningCount)
		{
			LoadMembershipDB(intValue);
		}
	}

	private void LoadMembershipDB(int count)
	{
		SubscriptionNotificationType subscriptionType = SubscriptionInfo.GetSubscriptionNotificationType();
		if (subscriptionType != 0)
		{
			LocaleString messageText = _SubscriptionNotificationData.Find((SubscriptionNotificationData ele) => ele._Type == subscriptionType)._MessageText;
			LocaleString titleText = _SubscriptionNotificationData.Find((SubscriptionNotificationData ele) => ele._Type == subscriptionType)._TitleText;
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "SubscriptionNotificationDB");
			mKAUIGenericDB.SetTitle(titleText._Text);
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB.SetText(messageText._Text, interactive: false);
			mKAUIGenericDB.SetDestroyOnClick(isDestroy: true);
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._YesMessage = "PurchaseMembership";
			KAUI.SetExclusive(mKAUIGenericDB);
			ProductData.pPairData.SetValueAndSave("MemberDBWarning", (count + 1).ToString());
			ProductData.pPairData.SetValueAndSave("LastMemberDBShown", UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime).ToString(UtUtilities.GetCultureInfo("en-US")));
		}
	}

	private void PurchaseMembership()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}
}
