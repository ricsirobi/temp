using UnityEngine;

public class UiPlayerInfoDB : KAUI
{
	public string _HouseLevel = "";

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public UITexture _CrestLogo;

	public UITexture _CrestBackground;

	public LocaleString _WebServiceErrorText = new LocaleString("There was an error opening the player info page.  Please try again.");

	public InvitePlayerToClanResultInfo[] _InvitePlayerToClanResultInfo;

	public BuddyActionResultInfo[] _BuddyActionResultInfo;

	public GameObject _BtnSetMobile;

	public GameObject _BtnSetStandalone;

	private static UiPlayerInfoDB mInstance;

	private static UserProfile mUserProfile;

	private bool mLoading;

	private bool mLoadingClan;

	private Group mUserClan;

	private KAWidget mAddBtn;

	private KAWidget mInviteBtn;

	private KAWidget mFarmBtn;

	private KAWidget mProfileBtn;

	private KAWidget mMessageBoardBtn;

	private KAWidget mCloseBtn;

	private KAWidget mAvatarName;

	private KAWidget mClanLogo;

	private KAWidget mNoClanTxt;

	private KAWidget mIcoTrophy;

	private KAUIGenericDB mUiGenericDB;

	protected override void Awake()
	{
		base.Awake();
		if (UtPlatform.IsMobile())
		{
			_BtnSetMobile.SetActive(value: true);
		}
		else
		{
			_BtnSetStandalone.SetActive(value: true);
		}
	}

	protected override void Start()
	{
		base.Start();
		mAddBtn = FindItem("AddBtn");
		mInviteBtn = FindItem("InviteBtn");
		mFarmBtn = FindItem("FarmBtn");
		mProfileBtn = FindItem("ProfileBtn");
		mCloseBtn = FindItem("CloseBtn");
		mAvatarName = FindItem("TxtName");
		mClanLogo = FindItem("Logo");
		mNoClanTxt = FindItem("TxtNoClan");
		mIcoTrophy = FindItem("IcoTrophy");
		if (UtPlatform.IsMobile())
		{
			mMessageBoardBtn = FindItem("MessageBoardBtn");
		}
		SetVisibility(inVisible: false);
	}

	protected override void Update()
	{
		base.Update();
		if (mLoading)
		{
			if (mUserProfile.pIsError)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				CloseUI();
				ShowDialog(_WebServiceErrorText);
				mLoading = false;
			}
			else if (mUserProfile.pIsReady && !mLoadingClan)
			{
				mLoading = false;
				OnProfileDataReady();
			}
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mAddBtn)
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			BuddyList.pInstance.AddBuddy(mUserProfile.UserID, mUserProfile.GetDisplayName(), BuddyListEventHandler);
		}
		else if (item == mInviteBtn)
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			InvitePlayerRequest obj = new InvitePlayerRequest
			{
				InviteeIDs = new string[1]
			};
			obj.InviteeIDs[0] = mUserProfile.UserID;
			obj.GroupID = UserProfile.pProfileData.Groups[0].GroupID;
			WsWebService.InvitePlayer(obj, InvitePlayerEventHandler, null);
		}
		else if (item == mFarmBtn)
		{
			SetVisibility(inVisible: false);
			UiFarms.OpenFriendFarmListUI(mUserProfile.UserID, base.gameObject);
		}
		else if (item == mProfileBtn)
		{
			string userID = mUserProfile.UserID;
			CloseUI();
			ProfileLoader.ShowProfile(userID);
		}
		else if (item == mMessageBoardBtn)
		{
			string userID2 = mUserProfile.UserID;
			CloseUI();
			MessageBoardLoader.Load(userID2);
		}
		else if (item == mCloseBtn)
		{
			CloseUI();
		}
		else if (item == mClanLogo)
		{
			string userID3 = mUserProfile.UserID;
			CloseUI();
			UiClans.ShowClan(userID3, mUserClan);
		}
	}

	private void OnFarmUIClosed()
	{
		SetVisibility(inVisible: true);
	}

	public static void ShowPlayerInfo(string userID)
	{
		if (!(mInstance != null) || !(mUserProfile.UserID == userID))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			mUserProfile = UserProfile.LoadUserProfile(userID);
			RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("PlayerInfoAsset"), OnPlayerInfoLoadingEvent, typeof(GameObject));
		}
	}

	public static void OnPlayerInfoLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiPlayerInfo";
			obj.SendMessage("OpenUI");
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
			break;
		}
	}

	private void OpenUI()
	{
		if (mInstance == null)
		{
			mInstance = this;
		}
		mLoading = true;
		mLoadingClan = true;
		Group.Get(mUserProfile.UserID, OnGroupGet);
	}

	private void CloseUI()
	{
		Object.Destroy(base.gameObject);
		mInstance = null;
		mUserProfile = null;
		RsResourceManager.Unload(GameConfig.GetKeyData("PlayerInfoAsset"));
		RsResourceManager.UnloadUnusedAssets();
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
	}

	private void OnProfileDataReady()
	{
		SetVisibility(inVisible: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mAvatarName != null)
		{
			mAvatarName.SetText(mUserProfile.GetDisplayName());
		}
		mFarmBtn.SetDisabled(isDisabled: true);
		mAddBtn.SetDisabled(isDisabled: true);
		if (mUserProfile.UserID != UserInfo.pInstance.UserID && BuddyList.pIsReady)
		{
			BuddyStatus buddyStatus = BuddyList.pInstance.GetBuddyStatus(mUserProfile.UserID);
			if (buddyStatus == BuddyStatus.Unknown)
			{
				mAddBtn.SetDisabled(isDisabled: false);
			}
			if (buddyStatus != BuddyStatus.BlockedByOther && buddyStatus != BuddyStatus.BlockedByBoth && !string.IsNullOrEmpty(_HouseLevel))
			{
				mFarmBtn.SetDisabled(isDisabled: false);
			}
		}
		mClanLogo.SetDisabled(mUserClan == null);
		mNoClanTxt.SetVisibility(mUserClan == null);
		mInviteBtn.SetDisabled(isDisabled: true);
		if (UserProfile.pProfileData.HasGroup())
		{
			Group group = Group.GetGroup(UserProfile.pProfileData.Groups[0].GroupID);
			if ((mUserClan == null || group.GroupID != mUserClan.GroupID) && group.HasPermission((UserRole)UserProfile.pProfileData.Groups[0].RoleID.Value, "Invite"))
			{
				mInviteBtn.SetDisabled(isDisabled: false);
			}
		}
		UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(mUserProfile.AvatarInfo.Achievements, 11);
		mIcoTrophy.SetText((userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue) ? userAchievementInfoByType.AchievementPointTotal.Value.ToString() : "0");
		mIcoTrophy.SetVisibility(inVisible: true);
	}

	private void BuddyListEventHandler(WsServiceType inType, object inResult)
	{
		BuddyActionResult buddyActionResult = (BuddyActionResult)inResult;
		if (buddyActionResult == null)
		{
			buddyActionResult = new BuddyActionResult();
			buddyActionResult.Result = BuddyActionResultType.Unknown;
		}
		if (buddyActionResult.Result == BuddyActionResultType.Success)
		{
			mAddBtn.SetDisabled(isDisabled: true);
		}
		bool flag = false;
		BuddyActionResultInfo[] buddyActionResultInfo = _BuddyActionResultInfo;
		foreach (BuddyActionResultInfo buddyActionResultInfo2 in buddyActionResultInfo)
		{
			if (buddyActionResultInfo2._Status == buddyActionResult.Result)
			{
				flag = true;
				ShowDialog(buddyActionResultInfo2._StatusText);
				break;
			}
		}
		if (!flag)
		{
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	private void ShowDialog(LocaleString inText)
	{
		if (mUiGenericDB == null)
		{
			string text = inText.GetLocalizedString();
			if (text.Contains("[Name]"))
			{
				text = text.Replace("[Name]", mUserProfile.GetDisplayName());
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
			mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
			mUiGenericDB._MessageObject = base.gameObject;
			mUiGenericDB._CloseMessage = "OnCloseDB";
			mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
			mUiGenericDB.SetText(text, interactive: false);
			KAUI.SetExclusive(mUiGenericDB, _MaskColor);
		}
	}

	public void OnCloseDB()
	{
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
		SetVisibility(inVisible: true);
		SetInteractive(interactive: true);
	}

	private void OnGroupGet(GetGroupsResult result, object inUserData)
	{
		if (result != null && result.Success)
		{
			Group.AddGroup(result.Groups[0]);
			mUserClan = result.Groups[0];
			if (!string.IsNullOrEmpty(mUserClan.Logo))
			{
				string[] array = mUserClan.Logo.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnClanLogoLoadingEvent, typeof(Texture));
			}
			else
			{
				mLoadingClan = false;
			}
		}
		else
		{
			mLoadingClan = false;
		}
	}

	private void InvitePlayerEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != WsServiceEvent.COMPLETE)
		{
			return;
		}
		InvitePlayerResult invitePlayerResult = (InvitePlayerResult)inObject;
		if (invitePlayerResult != null)
		{
			InvitePlayerToClanResultInfo[] invitePlayerToClanResultInfo = _InvitePlayerToClanResultInfo;
			foreach (InvitePlayerToClanResultInfo invitePlayerToClanResultInfo2 in invitePlayerToClanResultInfo)
			{
				if (invitePlayerToClanResultInfo2._Status == invitePlayerResult.InviteeStatus[0].Status)
				{
					SetVisibility(inVisible: false);
					ShowDialog(invitePlayerToClanResultInfo2._StatusText);
					return;
				}
			}
		}
		SetInteractive(interactive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public void OnClanLogoLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			mLoadingClan = false;
			Color color = default(Color);
			if (_CrestLogo != null)
			{
				_CrestLogo.mainTexture = (Texture)inObject;
				mUserClan.GetFGColor(out color);
				_CrestLogo.color = color;
			}
			if (_CrestBackground != null)
			{
				mUserClan.GetBGColor(out color);
				_CrestBackground.color = color;
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mLoadingClan = false;
			break;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		mInstance = null;
	}
}
