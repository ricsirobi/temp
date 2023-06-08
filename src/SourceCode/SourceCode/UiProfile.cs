using System.Collections;
using UnityEngine;

public class UiProfile : KAUI
{
	public delegate void OnProfileNameChanged();

	public UiProfileXPMeters _ProfileXPMeters;

	public string _BuddyListResourceName = "RS_DATA/PfUiBuddyListDO.unity3d/PfUiBuddyListDO";

	public static UiProfile pInstance = null;

	public static UserProfile pUserProfile;

	public GameObject _TextureObj;

	public Texture _Texture;

	public TextAsset _TutorialTextAsset;

	public string _Intro;

	public string _OtherIntro;

	public string _HouseLevel = "";

	public string _VisitLevel = "";

	public string _AvatarNameSelectBundlePath = "RS_DATA/PfUiAvatarNameDO.unity3d/PfUiAvatarNameDO";

	public LocaleString _AddBuddyLaterText = new LocaleString("You cannot do that at this time.  Please try again later.");

	public LocaleString _BuddyListFullText = new LocaleString("Your Friends list is full. You cannot add a new Friend.");

	public LocaleString _FriendBuddyListFullText = new LocaleString("This person's Friends list is full.");

	public LocaleString _AddBuddyWaitText = new LocaleString("This person is waiting for your approval.");

	public LocaleString _BlockedByOtherText = new LocaleString("This person is not accepting Friend requests.");

	public LocaleString _WebServiceErrorText = new LocaleString("There was an error opening your About Me page.  Please try again.");

	public LocaleString _IgnorePlayerText = new LocaleString("Are you sure you'd like to ignore this person?");

	public LocaleString _GenericErrorText = new LocaleString("There was an error connecting to the server.  Please try again.");

	public LocaleString _BuddyAddedText = new LocaleString("Your buddy request has been sent successfully");

	public LocaleString _NameChangeConfirmationText = new LocaleString("Are you sure you want to change your Avatar's name for {{GEMS}} gems?");

	public LocaleString _NameChangeConfirmationTitleText = new LocaleString("Change your Avatar's name?");

	public LocaleString _NotEnoughFeeTitleText = new LocaleString("Buy more gems?");

	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	private static bool mShowCloseButton = false;

	public int _ChangeNameStoreID = 93;

	public int _ChangeNameItemID = 13030;

	private int mEditNameCost;

	private Group mClan;

	private bool mLoading = true;

	private KAUIGenericDB mUiGenericDB;

	private bool mClanReady = true;

	private KAWidget mAddBtn;

	private KAWidget mIgnoreBtn;

	private KAWidget mModeratorBtn;

	private KAWidget mRankIcon;

	private KAWidget mSocialRankIcon;

	private KAWidget mHouseBtn;

	private KAWidget mProfilePic;

	private KAWidget mAvatarName;

	private KAWidget mExitBtn;

	private KAWidget mEditNameBtn;

	private KAWidget mBtnXP;

	private KAWidget mCrestBanner;

	private KAWidget mClanNameTxt;

	private KAWidget mTrophiesBtn;

	private KAWidget mUDTStars;

	private AvPhotoManager mPhotoManager;

	private static string mLastLevel = "";

	public static bool pShowCloseButton
	{
		get
		{
			return mShowCloseButton;
		}
		set
		{
			mShowCloseButton = value;
		}
	}

	public Group pClan => mClan;

	public bool pLoading => mLoading;

	public static string pLastLevel
	{
		get
		{
			return mLastLevel;
		}
		set
		{
			mLastLevel = value;
		}
	}

	public static event OnProfileNameChanged _OnProfileNameChanged;

	public void ProfileAvPhotoCallback(Texture tex, object inUserData)
	{
		if (mProfilePic != null)
		{
			mProfilePic.SetTexture(tex);
		}
	}

	public static void SetAnswerItem(KAWidget inWidget, Texture2D img, string imgURL, string tex)
	{
		if (inWidget == null)
		{
			return;
		}
		KAWidget kAWidget = inWidget.FindChildItem("AnswerIcon");
		if (!string.IsNullOrEmpty(imgURL))
		{
			if (kAWidget != null)
			{
				kAWidget.SetTextureFromURL(imgURL);
				kAWidget.SetText("");
				return;
			}
			inWidget.SetTextureFromURL(imgURL);
			if (inWidget.GetLabel() != null)
			{
				inWidget.SetText("");
			}
		}
		else
		{
			inWidget.SetText(tex);
		}
	}

	public static void SetAnswerItem(KAWidget it, ProfileAnswer ad)
	{
		SetAnswerItem(it, null, ad.ImageURL, ad.DisplayText);
	}

	public void OnSetVisibility(bool t)
	{
		SetVisibility(t);
	}

	public void OnActive(bool inStatus)
	{
		base.gameObject.SetActive(inStatus);
		if (inStatus)
		{
			OpenUI();
		}
	}

	private void OpenUI()
	{
		if (pInstance == null)
		{
			pInstance = this;
			StartCoroutine(GetClanData());
			base.gameObject.BroadcastMessage("OnOpenUI", pUserProfile.UserID, SendMessageOptions.DontRequireReceiver);
		}
		mLoading = true;
		if (mExitBtn != null)
		{
			mExitBtn.SetVisibility(mShowCloseButton);
		}
	}

	public void CloseUI()
	{
		MainStreetMMOClient.pForceShowModeration = false;
		base.gameObject.BroadcastMessage("OnCloseUI", SendMessageOptions.DontRequireReceiver);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		Object.Destroy(base.gameObject);
		pInstance = null;
		pUserProfile = null;
		Object.Destroy(mPhotoManager.gameObject);
		RsResourceManager.Unload(GameConfig.GetKeyData("ProfileAsset"));
		RsResourceManager.UnloadUnusedAssets();
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("ProfileScene"))
		{
			AvAvatar.pStartLocation = AvAvatar.pSpawnAtSetPosition;
			RsResourceManager.LoadLevel(mLastLevel);
		}
	}

	private void RankImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			Texture texture = (Texture)inObject;
			if (texture != null)
			{
				texture.name = "ProfileRankTexture";
			}
			mRankIcon.SetTexture(texture);
		}
	}

	private void SocialRankImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			Texture inTexture = (Texture)inObject;
			mSocialRankIcon.SetTexture(inTexture);
		}
	}

	public void OnTutorialDone(string tid)
	{
		SetInteractive(interactive: true);
	}

	protected override void Start()
	{
		base.Start();
		mAddBtn = FindItem("AddBtn");
		mIgnoreBtn = FindItem("IgnoreBtn");
		mModeratorBtn = FindItem("ModeratorBtn");
		mRankIcon = FindItem("RankIcon");
		mSocialRankIcon = FindItem("SocialRankIcon");
		mHouseBtn = FindItem("HouseBtn");
		mProfilePic = FindItem("AvatarProfilePic");
		mAvatarName = FindItem("NameTxt");
		mExitBtn = FindItem("ExitBtn");
		if (mCrestBanner == null)
		{
			mCrestBanner = FindItem("ClanCrestTemplate");
		}
		mTrophiesBtn = FindItem("TrophiesBtn");
		mUDTStars = FindItem("UDTStarsIcon");
		mEditNameBtn = FindItem("EditNameBtn");
		if (pUserProfile != null)
		{
			mEditNameBtn.SetDisabled(pUserProfile.UserID != UserInfo.pInstance.UserID);
		}
		if (mClanNameTxt == null)
		{
			mClanNameTxt = FindItem("ClanName");
		}
		mBtnXP = FindItem("BtnXP");
		mPhotoManager = AvPhotoManager.Init("PfProfilePhotoMgr");
		base.gameObject.BroadcastMessage("OnSetVisibility", false);
		if (UtPlatform.IsMobile())
		{
			AdManager.DisplayAd(AdEventType.PROFILE_PAGE, AdOption.FULL_SCREEN);
		}
		ChangeNameItemData();
	}

	public override KAWidget FindItem(string inWidgetName, bool recursive = true)
	{
		KAWidget kAWidget = base.FindItem(inWidgetName, recursive);
		if (kAWidget != null || !recursive)
		{
			return kAWidget;
		}
		if (_UiList == null)
		{
			return null;
		}
		KAUI[] uiList = _UiList;
		int num = 0;
		if (num < uiList.Length)
		{
			kAWidget = uiList[num].FindItem(inWidgetName, recursive);
		}
		return kAWidget;
	}

	private IEnumerator GetClanData()
	{
		mClanReady = false;
		mClan = null;
		if (UserInfo.pInstance.UserID == pUserProfile.UserID)
		{
			if (mClanNameTxt == null)
			{
				mClanNameTxt = FindItem("ClanName");
			}
			Group group;
			if (Group.pIsReady)
			{
				group = Group.GetGroup(UserProfile.pProfileData.GetGroupID());
			}
			else
			{
				Group.Reset();
				Group.Init(includeMemberCount: true);
				yield return new WaitUntil(() => Group.pIsReady);
				group = Group.GetGroup(UserProfile.pProfileData.GetGroupID());
			}
			if (group != null)
			{
				if (mCrestBanner == null)
				{
					mCrestBanner = FindItem("ClanCrestTemplate");
				}
				if (mCrestBanner != null)
				{
					ClanData clanData = new ClanData(group, mCrestBanner);
					mCrestBanner.SetUserData(clanData);
					clanData.Load();
				}
				if (mClanNameTxt != null)
				{
					mClanNameTxt.SetText(group.Name);
				}
				mClan = group;
				base.gameObject.BroadcastMessage("OnClan", mClan, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				if (mClanNameTxt != null)
				{
					mClanNameTxt.SetText(string.Empty);
				}
				base.gameObject.BroadcastMessage("OnClanFailed", SendMessageOptions.DontRequireReceiver);
			}
			mClanReady = true;
		}
		else
		{
			Group.Get(pUserProfile.UserID, OnGetGroup);
		}
	}

	private void OnGetGroup(GetGroupsResult result, object inUserData)
	{
		mClanReady = true;
		if (result != null && result.Success)
		{
			Group.AddGroup(result.Groups[0]);
			if (UserProfile.pProfileData.InGroup(result.Groups[0].GroupID))
			{
				mClan = result.Groups[0];
				base.gameObject.BroadcastMessage("OnClan", mClan, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.gameObject.BroadcastMessage("OnClanFailed", SendMessageOptions.DontRequireReceiver);
			}
			ClanData clanData = new ClanData(result.Groups[0], mCrestBanner);
			mCrestBanner.SetUserData(clanData);
			clanData.Load();
			mClanNameTxt.SetText(result.Groups[0].Name);
		}
		else
		{
			mClanNameTxt.SetText(string.Empty);
			base.gameObject.BroadcastMessage("OnClanFailed", SendMessageOptions.DontRequireReceiver);
		}
	}

	protected void EnableEdit(bool t)
	{
		mModeratorBtn.SetVisibility(!t);
		mExitBtn.SetVisibility(mShowCloseButton);
	}

	public virtual void ProfileDataReady(UserProfile p)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		base.gameObject.BroadcastMessage("OnSetVisibility", true);
		_ProfileXPMeters.OnSetVisibility(t: false);
		FindItem("MemberIcon").SetVisibility(p.IsMember());
		FindItem("NonMemberIcon").SetVisibility(!p.IsMember());
		mAvatarName.SetText(p.GetDisplayName());
		if (UserRankData.pIsReady && mUDTStars != null)
		{
			if (p.UserID == UserInfo.pInstance.UserID)
			{
				UDTUtilities.UpdateUDTStars(mUDTStars.transform, "ProfileStarsIconBkg", "ProfileStarsIconFrameBkg");
			}
			else
			{
				UDTUtilities.UpdateUDTStars(mUDTStars.transform, p.AvatarInfo.Achievements, "ProfileStarsIconBkg", "ProfileStarsIconFrameBkg");
			}
		}
		mAddBtn.SetVisibility(inVisible: false);
		mIgnoreBtn.SetVisibility(inVisible: false);
		mHouseBtn.SetVisibility(inVisible: false);
		if (pUserProfile.UserID != UserInfo.pInstance.UserID && BuddyList.pIsReady)
		{
			BuddyStatus buddyStatus = BuddyList.pInstance.GetBuddyStatus(pUserProfile.UserID);
			if (buddyStatus == BuddyStatus.Unknown)
			{
				mAddBtn.SetVisibility(inVisible: true);
			}
			if (buddyStatus != BuddyStatus.BlockedBySelf && buddyStatus != BuddyStatus.BlockedByBoth)
			{
				mIgnoreBtn.SetVisibility(inVisible: true);
			}
			if (buddyStatus != BuddyStatus.BlockedByOther && buddyStatus != BuddyStatus.BlockedByBoth && !string.IsNullOrEmpty(_HouseLevel))
			{
				mHouseBtn.SetVisibility(inVisible: true);
			}
		}
		if (mProfilePic != null)
		{
			mPhotoManager.TakePhoto(pUserProfile.UserID, null, ProfileAvPhotoCallback, null);
		}
		UserRank userRankByType = UserRankData.GetUserRankByType(1, p.AvatarInfo.Achievements);
		if (userRankByType != null)
		{
			KAWidget kAWidget = FindItem("RankBtn");
			if (kAWidget != null)
			{
				kAWidget.SetText(userRankByType.RankID.ToString());
			}
			if (!string.IsNullOrEmpty(userRankByType.Image))
			{
				RsResourceManager.Load(userRankByType.Image, RankImageLoadingEvent);
			}
		}
		UserRank userRankByType2 = UserRankData.GetUserRankByType(3, p.AvatarInfo.Achievements);
		if (userRankByType2 != null)
		{
			KAWidget kAWidget2 = FindItem("TxtSocialRank");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(userRankByType2.RankID.ToString());
			}
			if (!string.IsNullOrEmpty(userRankByType2.Image))
			{
				RsResourceManager.Load(userRankByType2.Image, SocialRankImageLoadingEvent);
			}
		}
		if (_TutorialTextAsset != null)
		{
			string inTutorial = ((pUserProfile.UserID == UserInfo.pInstance.UserID) ? _Intro : _OtherIntro);
			if (TutorialManager.StartTutorial(_TutorialTextAsset, inTutorial, bMarkDone: true, 2u, base.gameObject))
			{
				KAUI.SetExclusive(this, null);
				SetInteractive(interactive: false);
			}
		}
		UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(pUserProfile.AvatarInfo.Achievements, 11);
		if (mTrophiesBtn != null)
		{
			mTrophiesBtn.SetText((userAchievementInfoByType != null && userAchievementInfoByType.AchievementPointTotal.HasValue) ? userAchievementInfoByType.AchievementPointTotal.Value.ToString() : "0");
		}
		RsResourceManager.DestroyLoadScreen();
	}

	protected override void Update()
	{
		base.Update();
		if (mLoading)
		{
			if (pUserProfile != null && pUserProfile.pIsError)
			{
				ShowDialog(_WebServiceErrorText._ID, _WebServiceErrorText._Text, "OnCloseDBError");
				mLoading = false;
			}
			else if (pUserProfile != null && pUserProfile.pIsReady && mClanReady)
			{
				mLoading = false;
				base.gameObject.BroadcastMessage("EnableEdit", pUserProfile.UserID == UserInfo.pInstance.UserID);
				base.gameObject.BroadcastMessage("ProfileDataReady", pUserProfile);
			}
		}
	}

	private void BuddyListEventHandler(WsServiceType inType, object inResult)
	{
		if (inResult == null)
		{
			ShowDialog(_AddBuddyLaterText._ID, _AddBuddyLaterText._Text, "OnCloseDB");
			return;
		}
		BuddyActionResult buddyActionResult = (BuddyActionResult)inResult;
		switch (buddyActionResult.Result)
		{
		case BuddyActionResultType.Unknown:
			ShowDialog(_AddBuddyLaterText._ID, _AddBuddyLaterText._Text, "OnCloseDB");
			break;
		case BuddyActionResultType.Success:
			ShowDialog(_BuddyAddedText._ID, _BuddyAddedText._Text, "OnCloseDB");
			mAddBtn.SetVisibility(inVisible: false);
			break;
		case BuddyActionResultType.BuddyListFull:
			ShowDialog(_BuddyListFullText._ID, _BuddyListFullText._Text, "OnCloseDB");
			break;
		case BuddyActionResultType.FriendBuddyListFull:
			ShowDialog(_FriendBuddyListFullText._ID, _FriendBuddyListFullText._Text, "OnCloseDB");
			break;
		case BuddyActionResultType.AlreadyInList:
			mAddBtn.SetVisibility(inVisible: false);
			if (buddyActionResult.Status == BuddyStatus.BlockedByOther)
			{
				ShowDialog(_BlockedByOtherText._ID, _BlockedByOtherText._Text, "OnCloseDB");
			}
			else
			{
				ShowDialog(_AddBuddyWaitText._ID, _AddBuddyWaitText._Text, "OnCloseDB");
			}
			break;
		}
	}

	private void ShowDialog(int id, string text, string closeMessage)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._CloseMessage = closeMessage;
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: true);
		mUiGenericDB.SetTextByID(id, text, interactive: false);
		KAUI.SetExclusive(mUiGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public void OnCloseDBError()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
		CloseUI();
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("ProfileScene"))
		{
			RsResourceManager.LoadLevel(RsResourceManager.pLastLevel);
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
		SetInteractive(interactive: true);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "ExitBtn")
		{
			CloseUI();
		}
		else if (item == mAddBtn)
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			BuddyList.pInstance.AddBuddy(pUserProfile.UserID, pUserProfile.GetDisplayName(), BuddyListEventHandler);
		}
		else if (item == mIgnoreBtn)
		{
			GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
			mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
			mUiGenericDB._MessageObject = base.gameObject;
			mUiGenericDB._YesMessage = "OnIgnore";
			mUiGenericDB._NoMessage = "OnClose";
			mUiGenericDB.SetTextByID(_IgnorePlayerText._ID, _IgnorePlayerText._Text, interactive: false);
			mUiGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			KAUI.SetExclusive(mUiGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
		else if (item == mModeratorBtn)
		{
			UiModeratorDB component = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiModeratorDB")).GetComponent<UiModeratorDB>();
			component.SetVisibility(ModeratorType.REPORT);
			component._UserID = pUserProfile.UserID;
		}
		else if (item == mHouseBtn)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetActive(inActive: false);
			Input.ResetInputAxes();
			Object.Destroy(base.gameObject);
			MainStreetMMOClient.pInstance.JoinOwnerSpace(_HouseLevel, pUserProfile.UserID);
		}
		else if (item == mCrestBanner)
		{
			ClanData clanData = (ClanData)mCrestBanner.GetUserData();
			if (clanData != null)
			{
				UiClans.ShowClan(pUserProfile.UserID, clanData._Group);
			}
		}
		else if (item == mBtnXP)
		{
			_ProfileXPMeters.OnSetVisibility(t: true);
			KAUI.SetExclusive(_ProfileXPMeters);
		}
		else if (item == mEditNameBtn)
		{
			SetInteractive(interactive: false);
			LocaleString localeString = new LocaleString(_NameChangeConfirmationText.GetLocalizedString().Replace("{{GEMS}}", mEditNameCost.ToString()));
			localeString._ID = 0;
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", localeString.GetLocalizedString(), _NameChangeConfirmationTitleText.GetLocalizedString(), base.gameObject, "ChangeName", "CancelChangeName", "", "", inDestroyOnClick: true);
		}
	}

	private void ChangeNameItemData()
	{
		ItemStoreDataLoader.Load(_ChangeNameStoreID, OnStoreLoaded);
	}

	private void OnStoreLoaded(StoreData sd)
	{
		if (sd._ID == _ChangeNameStoreID)
		{
			ItemData itemData = sd.FindItem(_ChangeNameItemID);
			if (itemData != null)
			{
				mEditNameCost = itemData.FinalCashCost;
			}
		}
	}

	private void ChangeName()
	{
		if (Money.pCashCurrency >= mEditNameCost)
		{
			UiSelectName.Init(OnSelectNameProcessed, null, null, UiSelectName.FailStatus.None, independent: true);
		}
		else
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDBSm", _NotEnoughFeeText.GetLocalizedString(), _NotEnoughFeeTitleText.GetLocalizedString(), base.gameObject, "BuyGemsOnline", "CancelChangeName", "", "", inDestroyOnClick: true);
		}
	}

	private void CancelChangeName()
	{
		SetInteractive(interactive: true);
	}

	private void OnSelectNameProcessed(UiSelectName.Status status, string name, bool suggestedNameSelected, UiSelectName uiSelectName)
	{
		switch (status)
		{
		case UiSelectName.Status.Accepted:
			EditNameDone();
			break;
		case UiSelectName.Status.Closed:
			SetState(KAUIState.INTERACTIVE);
			break;
		case UiSelectName.Status.Loaded:
			if (uiSelectName != null)
			{
				uiSelectName.UpdatePurchaseDetails(_ChangeNameItemID, _ChangeNameStoreID);
			}
			break;
		}
	}

	private void BuyGemsOnline()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void EditNameDone()
	{
		UserProfile.pProfileData.AvatarInfo.AvatarData.DisplayName = UserInfo.pInstance.Username;
		pUserProfile.AvatarInfo.AvatarData.DisplayName = UserInfo.pInstance.Username;
		if (UiProfile._OnProfileNameChanged != null)
		{
			UiProfile._OnProfileNameChanged();
		}
		mAvatarName.SetText(UserInfo.pInstance.Username);
		SetInteractive(interactive: true);
	}

	private void OnIAPStoreClosed()
	{
		if (Money.pCashCurrency >= mEditNameCost)
		{
			ChangeName();
		}
		else
		{
			SetState(KAUIState.INTERACTIVE);
		}
	}

	private void IgnoreEventHandler(WsServiceType inType, object inResult)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		if (inResult == null)
		{
			GameObject gameObject = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSmSocial"));
			mUiGenericDB = gameObject.GetComponent<KAUIGenericDB>();
			mUiGenericDB._MessageObject = base.gameObject;
			mUiGenericDB._OKMessage = "OnClose";
			mUiGenericDB.SetTextByID(_GenericErrorText._ID, _GenericErrorText._Text, interactive: false);
			mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			KAUI.SetExclusive(mUiGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
	}

	public void OnClose()
	{
		KAUI.RemoveExclusive(mUiGenericDB);
		Object.Destroy(mUiGenericDB.gameObject);
	}

	public void OnIgnore()
	{
		KAUI.RemoveExclusive(mUiGenericDB);
		Object.Destroy(mUiGenericDB.gameObject);
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		mAddBtn.SetVisibility(inVisible: false);
		mIgnoreBtn.SetVisibility(inVisible: false);
		BuddyList.pInstance.BlockBuddy(pUserProfile.UserID, IgnoreEventHandler);
	}

	public static void OnAssetLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiProfile";
			obj.SendMessage("OpenUI");
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			MainStreetMMOClient.pForceShowModeration = false;
			break;
		}
	}

	public static void LoadProfileBundle(string bResName)
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = bResName.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadingEvent, typeof(GameObject));
	}

	public static void ShowProfile(string userID)
	{
		if (!(pInstance != null) || !(pUserProfile.UserID == userID))
		{
			mShowCloseButton = true;
			MainStreetMMOClient.pForceShowModeration = true;
			pUserProfile = UserProfile.LoadUserProfile(userID);
			LoadProfileBundle(GameConfig.GetKeyData("ProfileAsset"));
		}
	}

	public static void ShowProfile(bool isCloseBtnVisible = false)
	{
		BuddyList.Init();
		ShowProfile(UserInfo.pInstance.UserID);
		mShowCloseButton = isCloseBtnVisible;
	}
}
