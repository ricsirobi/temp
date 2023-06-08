using System;
using System.Collections.Generic;
using System.Linq;
using SOD.Event;
using UnityEngine;

public class UiToolbar : KAUI
{
	public delegate void OnAvatarUpdated();

	[Serializable]
	public class BuyTaskStoreMapping
	{
		public int _TaskID;

		public int _StoreID;

		public int _CategoryID;
	}

	[Serializable]
	public class RewardMultiplierUI
	{
		public int _PointType;

		public UIRewardMultiplier _UI;
	}

	public Vector3 _PicturePositionOffset = new Vector3(0.05f, 0.5f, 0.5f);

	public Vector3 _PictureLookAtOffset = new Vector3(0.05f, 0.8f, 0f);

	private static int mMessageCount;

	private static bool mAvatarModified;

	private static Texture mPortraitTexture;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public int _CompassItemID = 12978;

	public string _BackBtnLoadLevel = "";

	public string _BackBtnMarker = "";

	public int _BackBtnOffset = 100;

	public GameObject _UiMessageLog;

	public GameObject _UiChatHistoryMobile;

	public GameObject _UiChatHistory;

	public List<UiChatHistory.MessageAction> _MessageActions;

	public UiAvatarCSM _UiAvatarCSM;

	public KAUI[] _UiToolbars;

	[NonSerialized]
	public CoIdleManager mIdleMgr;

	public TextAsset _TutorialTextAsset;

	public string _LongIntro;

	public string _ShortIntro;

	public string _RenewMemberLogEventText = "RenewMemberFromExpiryWarning";

	public LocaleString _MembershipRenewText = new LocaleString("Membership expires soon! Renew now");

	public LocaleString _MembershipLastDayText = new LocaleString("Membership expires today! Renew now");

	public LocaleString _WarningDBText = new LocaleString("Do you want to quit?");

	public string _WorldMapBundle = "";

	public string _QuestBranchBundle = "";

	public UiSceneMap _SceneMap;

	public string _SceneMapBGTextureBundle = "RS_DATA/AniDWDragonsMapBerk.Unity3d/AniDWDragonsMapBerk";

	[NonSerialized]
	public bool pAutoExit;

	public StoreLoader.Selection _CoinsStoreInfo;

	public string _Store = "Player Equipment";

	public string _StoreCategory = "";

	public int _InventoryCategory = -1;

	public List<BuyTaskStoreMapping> _BuyTaskStoreMapping;

	public UiPetMeter _UiPetMeter;

	[Header("Glow values")]
	public float _GlowFlashTime = 5f;

	public float _GlowFlashInterval = 0.2f;

	private KAWidget mBackBtn;

	private KAWidget mPDABtn;

	private KAWidget mWorldMapBtn;

	private KAWidget mStoreBtn;

	private KAWidget mFireBtn;

	private KAWidget mPortrait;

	private KAWidget mOptionsBtn;

	private KAWidget mUDTStars;

	private KAWidget mTxtUDTPoints;

	private KAWidget mGemTotalBtn;

	private KAWidget mCoinTotalBtn;

	private KAWidget mQuestBranchBtn;

	private KAWidget mBtnChatHistory;

	private KAWidget mAvatarInfoBG;

	private KAWidget mAvatarXpMeterProgressBar;

	private KAWidget mAvatarLevel;

	private KAWidget mAvatarName;

	private KAWidget mAvatarHPMeterProgress;

	private KAWidget mAvatarHPTxt;

	private KAWidget mTxtPlayerXPTime;

	private KAWidget mTxtDragonXPTime;

	private KAWidget mGlowEffectCountDown;

	private KAWidget mGlowCountDownProgressBar;

	private KAWidget mTxtGlowCountDownTime;

	private KAWidget mGlowDial;

	public UIWidget[] _GlowColorTintChildList;

	private float mTimeCounter;

	private float mTimeFlashCounter;

	public float _TimeToShowCountDownTime = 0.9f;

	private bool mTutorialPlayed;

	private bool mIsRankInit;

	private bool mInitBackpack;

	private object mMessageInfo = new MessageInfo();

	private AvPhotoManager mPhotoManager;

	private List<FieldGuideChapter> mRecentlyUnlockedChapters = new List<FieldGuideChapter>();

	private AvAvatarController mAvatarController;

	private float mMaxHP = -1f;

	private float mCurrentHP = -1f;

	private UiAdReward mUiAdReward;

	private UiHUDPromoOffer mUiHUDPromoOffer;

	private KAUI[] mUiHUDEvents;

	public List<RewardMultiplierUI> RewardMultiplierUIs = new List<RewardMultiplierUI>();

	public static int pMessageCount
	{
		get
		{
			return mMessageCount;
		}
		set
		{
			mMessageCount = value;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.SendMessage("OnMessageCount", mMessageCount, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static bool pAvatarModified
	{
		set
		{
			if (value)
			{
				if (AvAvatar.pToolbar != null && AvAvatar.pToolbar.activeInHierarchy)
				{
					AvAvatar.pToolbar.SendMessage("OnAvatarModified", SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					mAvatarModified = true;
				}
				UiToolbar.AvatarUpdated?.Invoke();
			}
			else
			{
				mAvatarModified = value;
			}
		}
	}

	public static Texture pPortraitTexture => mPortraitTexture;

	public static event OnAvatarUpdated AvatarUpdated;

	protected override void Awake()
	{
		base.Awake();
		AvAvatar.pToolbar = base.gameObject;
		if (mAvatarModified)
		{
			mAvatarModified = false;
			mPortraitTexture = null;
		}
		InitChatUI();
	}

	protected override void Start()
	{
		base.Start();
		Initialize();
		MissionManager.AddMissionEventHandler(OnMissionEvent);
		UiFieldGuide.LoadFieldGuideData();
		GlowManager.OnGlowApplied += GlowColorApplied;
		TakePicture();
		AdRewardManager.Init();
	}

	public virtual void Initialize()
	{
		bool flag = UtPlatform.IsMobile();
		mBtnChatHistory = FindItem("BtnChatHistory");
		if (mBtnChatHistory != null)
		{
			mBtnChatHistory.SetVisibility(!flag);
		}
		mBackBtn = FindItem("BackBtn");
		mPDABtn = FindItem("PDABtn");
		mWorldMapBtn = FindItem("BtnOpenMap");
		mStoreBtn = FindItem("StoreBtn");
		mFireBtn = FindItem("FireBtn");
		mGemTotalBtn = FindItem("GemTotalBtn");
		mCoinTotalBtn = FindItem("CoinTotalBtn");
		mAvatarInfoBG = FindItem("AvatarInfo");
		mAvatarXpMeterProgressBar = FindItem("AvatarXpMeter");
		mAvatarLevel = FindItem("TxtAvatarLevel");
		mAvatarName = FindItem("TxtAvatarName");
		mOptionsBtn = FindItem("BtnDWDragonsHUDOptions");
		mGlowEffectCountDown = FindItem("PotionTimer");
		mGlowCountDownProgressBar = FindItem("MeterBar");
		mTxtGlowCountDownTime = FindItem("TxtTimer");
		mGlowDial = FindItem("GlowDial");
		mQuestBranchBtn = FindItem("BtnOpenQuestBranches");
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pData.pGlowEffect != null)
		{
			SetGlowCountDownColor(SanctuaryManager.pCurPetInstance.pData.pGlowEffect.GlowColor);
		}
		if (mGlowCountDownProgressBar != null)
		{
			mGlowCountDownProgressBar.SetProgressLevel(0f);
		}
		if (mTxtGlowCountDownTime != null)
		{
			mTxtGlowCountDownTime.SetText("");
		}
		GameObject gameObject = GameObject.Find("AvatarPic");
		if (gameObject != null)
		{
			mPortrait = gameObject.GetComponent<KAWidget>();
		}
		mUDTStars = FindItem("UDTStarsIcon");
		mTxtUDTPoints = FindItem("TxtUDTPoints");
		mTxtPlayerXPTime = FindItem("TxtPlayerXPTime");
		mTxtDragonXPTime = FindItem("TxtDragonXPTime");
		mAvatarHPMeterProgress = FindItem("MeterBarHPs");
		mAvatarHPTxt = FindItem("TxtMeterBarHPs");
		if (GameDataConfig.pInstance != null && SubscriptionInfo.pIsMember)
		{
			int days = (SubscriptionInfo.pIsTrialMember ? GameDataConfig.pInstance.TrialMembershipRenewalWarningDays : GameDataConfig.pInstance.MembershipRenewalWarningDays);
			if (!SubscriptionInfo._MembershipPurchased && IAPManager.IsMembershipExpiring(days))
			{
				TimeSpan timeSpan = SubscriptionInfo.pInstance.SubscriptionEndDate.Value - ServerTime.pCurrentTime;
				if (timeSpan.Days >= 1)
				{
					UiChatHistory.AddSystemNotification(_MembershipRenewText.GetLocalizedString().Replace("%day%", timeSpan.Days.ToString()), mMessageInfo, OnSystemMessageClicked);
				}
				else if (timeSpan.Days == 0)
				{
					UiChatHistory.AddSystemNotification(_MembershipLastDayText.GetLocalizedString(), mMessageInfo, OnSystemMessageClicked);
				}
			}
		}
		if (!string.IsNullOrEmpty(_BackBtnLoadLevel) || !string.IsNullOrEmpty(_BackBtnMarker))
		{
			mBackBtn.SetVisibility(inVisible: true);
		}
		DisplayName();
		SetHP();
		mIdleMgr = GetComponent<CoIdleManager>();
		if (mIdleMgr != null)
		{
			mIdleMgr.StartIdles();
		}
		SetTurnArrows();
		if (UserRankData.pIsReady)
		{
			OnUpdateRank();
		}
		Money.UpdateDisplay();
		OnMessageCount(mMessageCount);
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.AddToolbar(base.gameObject);
		}
		mUiAdReward = (UiAdReward)_UiList[6];
		mUiHUDPromoOffer = (UiHUDPromoOffer)_UiList[7];
		mUiHUDEvents = Array.FindAll(_UiList, (KAUI ui) => ui as UiHUDEvent != null);
		CheckQuestBranchBtnVisibility();
	}

	private void OnSystemMessageClicked(object messageInfo)
	{
		mMessageInfo = messageInfo;
		AvAvatar.pState = AvAvatarState.PAUSED;
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
	}

	public void OnUpdateRank()
	{
		if (UserRankData.pInstance == null || !UserRankData.pInstance.AchievementPointTotal.HasValue)
		{
			return;
		}
		SetLevelandRankProgress(UserRankData.pInstance.AchievementPointTotal.Value);
		if (mAvatarLevel != null)
		{
			mAvatarLevel.SetText(UserRankData.pInstance.RankID.ToString());
		}
		if (mUDTStars != null)
		{
			UDTUtilities.UpdateUDTStars(mUDTStars.transform, "ToolbarStarsIconBkg", "ToolbarStarsIconFrameBkg");
		}
		UserAchievementInfo userAchievementInfoByType = UserRankData.GetUserAchievementInfoByType(12);
		if (mTxtUDTPoints != null)
		{
			if (userAchievementInfoByType != null)
			{
				mTxtUDTPoints.SetText(userAchievementInfoByType.AchievementPointTotal.Value.ToString());
			}
			else
			{
				mTxtUDTPoints.SetText("0");
			}
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetUDTPoints();
		}
		if (AvAvatar.pObject != null && mAvatarController != null)
		{
			if (userAchievementInfoByType != null)
			{
				mAvatarController.SetUDTStarsVisible(AvatarData.pDisplayYourName, userAchievementInfoByType.AchievementPointTotal.Value);
			}
			else
			{
				mAvatarController.SetUDTStarsVisible(AvatarData.pDisplayYourName, 0);
			}
		}
	}

	public override void SetInteractive(bool interactive)
	{
		base.SetInteractive(interactive);
		if (_UiToolbars == null || _UiToolbars.Length == 0)
		{
			return;
		}
		KAUI[] uiToolbars = _UiToolbars;
		foreach (KAUI kAUI in uiToolbars)
		{
			if (kAUI != null)
			{
				kAUI.SetInteractive(interactive);
			}
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBackBtn)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			GameUtilities.DisplayGenericDB("PfKAUIGenericDBSm", _WarningDBText.GetLocalizedString(), "", base.gameObject, "QuitWarningAccepted", "QuitWarningDeclined", "", "", inDestroyOnClick: true);
		}
		else if (item == mBtnChatHistory)
		{
			UiChatHistory.ShowChatHistory();
		}
		else if (item == mWorldMapBtn)
		{
			if (!string.IsNullOrEmpty(_WorldMapBundle))
			{
				AvAvatar.SetUIActive(inActive: false);
				AvAvatar.pState = AvAvatarState.PAUSED;
				KAUICursorManager.SetDefaultCursor("Loading");
				RsResourceManager.LoadAssetFromBundle(_WorldMapBundle, OnWorldMapLoaded, typeof(GameObject));
			}
			else if (!string.IsNullOrEmpty(_SceneMapBGTextureBundle))
			{
				AvAvatar.SetUIActive(inActive: false);
				AvAvatar.pState = AvAvatarState.PAUSED;
				KAUICursorManager.SetDefaultCursor("Loading");
				string[] array = _SceneMapBGTextureBundle.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnSceneMapLoaded, typeof(Texture2D));
			}
		}
		else if (item == mPDABtn)
		{
			Input.ResetInputAxes();
			JournalLoader.Load("", "", setDefaultMenuItem: true, null);
		}
		else if (item == mStoreBtn)
		{
			KAUIStore._ShowUpsellOnStoreStart = true;
			if (MissionManager.IsTaskActive("Buy") && InteractiveTutManager._CurrentActiveTutorialObject == null)
			{
				mStoreBtn.PlayAnim("Normal");
				OpenStoreForBuyTask();
			}
			else
			{
				StoreLoader.Load(setDefaultMenuItem: true, _StoreCategory, _Store, base.gameObject);
			}
		}
		else if (item == mFireBtn)
		{
			AvAvatar.pObject.SendMessage("Fire", null, SendMessageOptions.DontRequireReceiver);
		}
		else if (item == mOptionsBtn)
		{
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array2 = GameConfig.GetKeyData("OptionsAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], OptionsBundleReady, typeof(GameObject));
		}
		else if (item == mGemTotalBtn)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
		}
		else if (item == mCoinTotalBtn)
		{
			StoreLoader.Load(setDefaultMenuItem: true, _CoinsStoreInfo._Category, _CoinsStoreInfo._Store, base.gameObject);
		}
		else if (item == mGlowEffectCountDown)
		{
			if (!mTxtGlowCountDownTime.GetVisibility())
			{
				mTxtGlowCountDownTime.SetVisibility(inVisible: true);
			}
		}
		else if (item == mQuestBranchBtn && !string.IsNullOrEmpty(_QuestBranchBundle))
		{
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Loading");
			RsResourceManager.LoadAssetFromBundle(_QuestBranchBundle, OnQuestBranchLoaded, typeof(GameObject));
		}
	}

	public void OnIAPStoreClosed()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		if (mMessageInfo != null && (!SubscriptionInfo.pIsMember || SubscriptionInfo._MembershipPurchased))
		{
			UiChatHistory.SystemMessageAccepted(mMessageInfo);
		}
	}

	private void QuitWarningDeclined()
	{
		AvAvatar.pState = AvAvatar.pPrevState;
	}

	private void QuitWarningAccepted()
	{
		if (!string.IsNullOrEmpty(_BackBtnLoadLevel))
		{
			base.gameObject.SetActive(value: false);
			TutorialManager.StopTutorials();
			AvAvatar.pStartLocation = _BackBtnMarker;
			AvAvatar.SetActive(inActive: false);
			RsResourceManager.LoadLevel(_BackBtnLoadLevel);
		}
		else if (!string.IsNullOrEmpty(_BackBtnMarker))
		{
			TeleportTo(GameObject.Find(_BackBtnMarker));
		}
	}

	public void OnQuestBranchLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiDragonsQuestBranches";
			obj.GetComponent<UiQuestBranches>();
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void OnWorldMapLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiWorldMap";
			UiWorldMap component = obj.GetComponent<UiWorldMap>();
			if (component != null)
			{
				component._MiniMap = this;
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void OnSceneMapLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (_SceneMap != null)
			{
				Texture2D bGTexture = (Texture2D)inObject;
				_SceneMap.SetBGTexture(bGTexture);
				_SceneMap._MiniMap = this;
				_SceneMap.Show(show: true);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void OptionsBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			UiOptions component = obj.GetComponent<UiOptions>();
			if (component != null)
			{
				component._BundlePath = inURL;
			}
			obj.name = "PfUiOptions";
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (UtPlatform.IsMobile())
			{
				AdManager.DisplayAd(AdEventType.SETTINGS_PAGE, AdOption.FULL_SCREEN);
			}
		}
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		if (pet != null)
		{
			OnUpdateRank();
		}
	}

	public void DisplayName()
	{
		if (mAvatarName != null && AvatarData.pInstance != null)
		{
			mAvatarName.SetText(AvatarData.pInstance.DisplayName);
		}
	}

	public void SetHP()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SetDragonHP();
		}
		else
		{
			SetAvatarHP();
		}
	}

	private void SetAvatarHP()
	{
		if (mAvatarController != null && (mMaxHP != mAvatarController._Stats._MaxHealth || mCurrentHP != mAvatarController._Stats._CurrentHealth))
		{
			mMaxHP = mAvatarController._Stats._MaxHealth;
			mCurrentHP = mAvatarController._Stats._CurrentHealth;
			if (mAvatarHPMeterProgress != null)
			{
				mAvatarHPMeterProgress.SetProgressLevel(mCurrentHP / mMaxHP);
			}
			if (mAvatarHPTxt != null)
			{
				mAvatarHPTxt.SetText(Mathf.CeilToInt(mCurrentHP) + " / " + Mathf.CeilToInt(mMaxHP));
			}
		}
	}

	private void SetDragonHP()
	{
		if (!(SanctuaryManager.pCurPetInstance != null))
		{
			return;
		}
		float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HEALTH, SanctuaryManager.pCurPetInstance.pData);
		float meterValue = SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HEALTH);
		if (mMaxHP != maxMeter || mCurrentHP != meterValue)
		{
			mMaxHP = maxMeter;
			mCurrentHP = meterValue;
			if (mAvatarHPMeterProgress != null)
			{
				mAvatarHPMeterProgress.SetProgressLevel(mCurrentHP / mMaxHP);
			}
			if (mAvatarHPTxt != null)
			{
				mAvatarHPTxt.SetText(Mathf.CeilToInt(mCurrentHP) + " / " + Mathf.CeilToInt(mMaxHP));
			}
		}
	}

	private void SetLevelandRankProgress(float CurrentPoint)
	{
		UserRank userRankByValue = UserRankData.GetUserRankByValue((int)CurrentPoint);
		if (userRankByValue == null)
		{
			return;
		}
		UserRank nextRank = UserRankData.GetNextRank(userRankByValue.RankID);
		if (nextRank != null)
		{
			float progressLevel = 1f;
			if (userRankByValue.RankID != nextRank.RankID)
			{
				progressLevel = (CurrentPoint - (float)userRankByValue.Value) / (float)(nextRank.Value - userRankByValue.Value);
			}
			if (mAvatarXpMeterProgressBar == null)
			{
				mAvatarXpMeterProgressBar = FindItem("AvatarXpMeter");
			}
			if (mAvatarXpMeterProgressBar != null)
			{
				mAvatarXpMeterProgressBar.SetProgressLevel(progressLevel);
			}
			else
			{
				UtDebug.LogError("mAvatarXpMeterProgressBar is NULL");
			}
		}
	}

	public void TakePicture()
	{
		if (mPortraitTexture == null)
		{
			mPhotoManager = AvPhotoManager.Init("PfToolbarPhotoMgr");
			Texture2D texture2D = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false);
			if (mPortrait != null)
			{
				Texture2D texture2D2 = (Texture2D)mPortrait.GetTexture();
				if (texture2D2 != null)
				{
					Color[] pixels = texture2D2.GetPixels();
					texture2D.SetPixels(pixels);
					texture2D.Apply();
				}
			}
			if (UserInfo.pInstance != null)
			{
				mPhotoManager.TakePhoto(UserInfo.pInstance.UserID, texture2D, ProfileAvPhotoCallback, null);
			}
		}
		else
		{
			ObStatus component = base.gameObject.GetComponent<ObStatus>();
			if (component != null)
			{
				component.pIsReady = true;
			}
			if (mPortrait != null)
			{
				mPortrait.SetTexture(mPortraitTexture);
			}
		}
	}

	public void ProfileAvPhotoCallback(Texture tex, object inUserData)
	{
		mPortraitTexture = tex;
		if (mPortrait != null)
		{
			mPortrait.SetTexture(mPortraitTexture);
		}
		ObStatus component = base.gameObject.GetComponent<ObStatus>();
		if (component != null)
		{
			component.pIsReady = true;
		}
	}

	public void DisableFireButton()
	{
		if (null != mFireBtn)
		{
			mFireBtn.enabled = false;
		}
	}

	public void EnableFireButton()
	{
		if (null != mFireBtn)
		{
			mFireBtn.enabled = false;
		}
	}

	public void SetTurnArrows()
	{
		KAWidget kAWidget = FindItem("TurnLt");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(AvatarData.pControlMode == 1);
		}
		KAWidget kAWidget2 = FindItem("TurnRt");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(AvatarData.pControlMode == 1);
		}
	}

	protected override void Update()
	{
		if (Application.isEditor && Input.GetKeyDown(KeyCode.Alpha5))
		{
			TakePicture();
		}
		if (pAutoExit)
		{
			pAutoExit = false;
			OnClick(mBackBtn);
			return;
		}
		if (GetVisibility())
		{
			UICursorManager.SetAutoHide(t: true);
		}
		base.Update();
		if (mAvatarController == null && AvAvatar.pObject != null)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		SetHP();
		if (WsUserMessage.pIsReady && !mTutorialPlayed && InteractiveTutManager._CurrentActiveTutorialObject == null)
		{
			mTutorialPlayed = true;
			if (_TutorialTextAsset != null && !TutorialManager.StartTutorial(_TutorialTextAsset, _LongIntro, bMarkDone: true, 12u, null))
			{
				TutorialManager.StartTutorial(_TutorialTextAsset, _ShortIntro, bMarkDone: false, 12u, null);
			}
		}
		if (!mIsRankInit && UserRankData.pIsReady)
		{
			mIsRankInit = true;
			OnUpdateRank();
		}
		if (MissionManager.pInstance != null && MissionManagerDO.GetPlayerActiveTask() != null && mStoreBtn != null)
		{
			KAAnimInfo currentAnimInfo = mStoreBtn.GetCurrentAnimInfo();
			if (MissionManager.IsTaskActive("Buy") && (currentAnimInfo == null || currentAnimInfo._Name != "Flash"))
			{
				StartFlashingStoreButton();
			}
		}
		if (AllowHotKeys())
		{
			if (KAInput.GetButtonUp("Map") && mWorldMapBtn.IsActive())
			{
				OnClick(mWorldMapBtn);
			}
			else if (KAInput.GetButtonUp("Compass"))
			{
				OpenCompass();
			}
			else if (KAInput.GetButtonUp("Options"))
			{
				OnClick(mOptionsBtn);
			}
			else if (KAInput.GetButtonUp("Settings") && (UtPlatform.IsEditor() || (!UtPlatform.IsAndroid() && !UtPlatform.IsiOS())))
			{
				OpenSettings();
			}
			else if (_UiPetMeter != null && _UiPetMeter.IsActive() && KAInput.GetButtonUp("DragonList") && FUEManager.IsInputEnabled("DragonList") && mAvatarController != null && !mAvatarController.IsFlyingOrGliding() && !mAvatarController.pPlayerMounted && !mAvatarController.pPlayerCarrying && AvAvatar.pSubState != AvAvatarSubState.WALLCLIMB)
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.EnableAllInputs(inActive: false);
				AvAvatar.SetUIActive(inActive: false);
				UiDragonsStable.OpenDragonListUI(base.gameObject);
			}
		}
		if (mInitBackpack && CommonInventoryData.pIsReady)
		{
			mInitBackpack = false;
			UiBackpack.Init(_InventoryCategory);
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.mGlowTimer.pIsTimerRunning)
		{
			if (mTxtGlowCountDownTime.GetVisibility())
			{
				mTimeCounter += Time.deltaTime;
				if (mTimeCounter > _TimeToShowCountDownTime)
				{
					mTxtGlowCountDownTime.SetVisibility(inVisible: false);
					mTimeCounter = 0f;
				}
			}
			if (SanctuaryManager.pCurPetInstance.mGlowTimer.pTimeInSecs <= (double)_GlowFlashTime)
			{
				mTimeFlashCounter += Time.deltaTime;
				if (mTimeFlashCounter > _GlowFlashInterval)
				{
					mGlowDial.SetVisibility(!mGlowDial.GetVisibility());
					mTimeFlashCounter = 0f;
				}
			}
			if (!mGlowEffectCountDown.GetVisibility())
			{
				mGlowEffectCountDown.SetVisibility(inVisible: true);
				SetGlowCountDownColor(SanctuaryManager.pCurPetInstance.pData.pGlowEffect.GlowColor);
			}
			if (SanctuaryManager.pCurPetInstance.mGlowTimer.pIsTimerUIMarkDirty)
			{
				if (mGlowCountDownProgressBar != null)
				{
					mGlowCountDownProgressBar.SetProgressLevel(SanctuaryManager.pCurPetInstance.mGlowTimer.pPercentRemain);
				}
				if (mTxtGlowCountDownTime != null)
				{
					mTxtGlowCountDownTime.SetText(SanctuaryManager.pCurPetInstance.mGlowTimer.pTimeInHHMMSS);
				}
				SanctuaryManager.pCurPetInstance.mGlowTimer.pIsTimerUIMarkDirty = false;
			}
		}
		else if (mGlowEffectCountDown.GetVisibility())
		{
			mGlowEffectCountDown.SetVisibility(inVisible: false);
		}
	}

	private void GlowColorApplied()
	{
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pData != null && SanctuaryManager.pCurPetInstance.pData.IsGlowAvailable())
		{
			mGlowDial.SetVisibility(inVisible: true);
			mTimeFlashCounter = 0f;
			SetGlowCountDownColor(SanctuaryManager.pCurPetInstance.pData.pGlowEffect.GlowColor);
		}
	}

	private void SetGlowCountDownColor(string tintColor)
	{
		for (int i = 0; i < _GlowColorTintChildList.Length; i++)
		{
			_GlowColorTintChildList[i].color = GlowManager.pInstance.GetColor(tintColor);
		}
	}

	public bool AllowHotKeys()
	{
		if (IsActive() && !_UiAvatarCSM.IsActive() && !_UiAvatarCSM._UiActions.IsActive() && UiBuddyList.pInstance == null && AvAvatar.pInputEnabled)
		{
			return AvAvatar.pState != AvAvatarState.PAUSED;
		}
		return false;
	}

	public void UpdateRankFromServer()
	{
		mIsRankInit = false;
	}

	public virtual void OnEnable()
	{
		SetTurnArrows();
		OnMessageCount(mMessageCount);
		FetchRewardMultiplier();
		CheckQuestBranchBtnVisibility();
		if (mAvatarModified)
		{
			OnAvatarModified();
		}
	}

	public virtual void OnDisable()
	{
		UICursorManager.SetAutoHide(t: false);
		KAUI.OnUIDisabled?.Invoke();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GlowManager.OnGlowApplied -= GlowColorApplied;
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	private void TeleportTo(GameObject go)
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.TeleportToObject(go);
	}

	public void OnMessageCount(int count)
	{
		_UiAvatarCSM.OnMessageCount(count);
	}

	public void OnAvatarModified()
	{
		mAvatarModified = false;
		mPortraitTexture = null;
		if (mPhotoManager != null)
		{
			mPhotoManager.pPictureCache.Remove(UserInfo.pInstance.UserID);
		}
		TakePicture();
	}

	public void OnEarnGemsClose()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		SetInteractive(interactive: true);
	}

	private void CheckForFieldGuideUnlocks(int unlockId, int type, int state)
	{
		if (UiFieldGuide.pFieldGuideData == null)
		{
			return;
		}
		bool flag = false;
		FieldGuideSubject[] subjects = UiFieldGuide.pFieldGuideData.Subjects;
		for (int i = 0; i < subjects.Length; i++)
		{
			FieldGuideTopic[] topics = subjects[i].Topics;
			for (int j = 0; j < topics.Length; j++)
			{
				FieldGuideSubTopic[] subTopics = topics[j].SubTopics;
				for (int k = 0; k < subTopics.Length; k++)
				{
					FieldGuideLesson[] lessons = subTopics[k].Lessons;
					for (int l = 0; l < lessons.Length; l++)
					{
						FieldGuideChapter[] chapters = lessons[l].Chapters;
						foreach (FieldGuideChapter fieldGuideChapter in chapters)
						{
							if (fieldGuideChapter.IsUnlocked(unlockId, type, state) && !mRecentlyUnlockedChapters.Contains(fieldGuideChapter))
							{
								flag = true;
								mRecentlyUnlockedChapters.Add(fieldGuideChapter);
							}
						}
					}
				}
			}
		}
		if (flag)
		{
			UiFieldGuide.pRecentlyUnlockedChapters.Clear();
			UiFieldGuide.pRecentlyUnlockedChapters.AddRange(mRecentlyUnlockedChapters);
			mRecentlyUnlockedChapters.Clear();
		}
	}

	private void StartFlashingStoreButton()
	{
		mStoreBtn.PlayAnim("Flash");
	}

	private void StopFlashingStoreButton()
	{
		mStoreBtn.PlayAnim("Normal");
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		Task task = null;
		Mission mission = null;
		switch (inEvent)
		{
		case MissionEvent.TASK_STARTED:
			task = (Task)inObject;
			if (task != null)
			{
				CheckForFieldGuideUnlocks(task._Mission.MissionID, 1, 1);
				CheckForFieldGuideUnlocks(task.TaskID, 2, 1);
			}
			break;
		case MissionEvent.TASK_COMPLETE:
			task = (Task)inObject;
			if (task != null)
			{
				CheckForFieldGuideUnlocks(task.TaskID, 2, 2);
				if (!MissionManager.IsTaskActive("Buy"))
				{
					StopFlashingStoreButton();
				}
			}
			break;
		case MissionEvent.MISSION_COMPLETE:
			mission = (Mission)inObject;
			if (mission != null)
			{
				CheckForFieldGuideUnlocks(mission.MissionID, 1, 2);
			}
			break;
		}
	}

	public void FetchRewardMultiplier()
	{
		List<RewardMultiplier> list = new List<RewardMultiplier>();
		List<RewardMultiplier> list2 = UserProfile.pProfileData?.AvatarInfo?.GetRewardMultipliers();
		if (list2 != null)
		{
			list.AddRange(list2);
		}
		if (RewardMultiplierManager._ArrayOfRewardTypeMultiplier != null && RewardMultiplierManager._ArrayOfRewardTypeMultiplier.RewardTypeMultiplier != null)
		{
			RewardTypeMultiplier[] rewardTypeMultiplier = RewardMultiplierManager._ArrayOfRewardTypeMultiplier.RewardTypeMultiplier;
			foreach (RewardTypeMultiplier rewardTypeMultiplier2 in rewardTypeMultiplier)
			{
				if (rewardTypeMultiplier2.RewardTypeID != 0 && !(rewardTypeMultiplier2.ToDate < ServerTime.pCurrentTime))
				{
					RewardMultiplier rewardMultiplier = new RewardMultiplier();
					rewardMultiplier.MultiplierEffectTime = rewardTypeMultiplier2.ToDate;
					rewardMultiplier.MultiplierFactor = rewardTypeMultiplier2.MultiplierFactor;
					rewardMultiplier.PointTypeID = rewardTypeMultiplier2.RewardTypeID;
					list.Add(rewardMultiplier);
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (RewardMultiplierUI multiplierUI in RewardMultiplierUIs)
		{
			List<RewardMultiplier> list3 = (from t in list.FindAll((RewardMultiplier t) => t.PointTypeID == multiplierUI._PointType)
				orderby t.MultiplierEffectTime
				select t).ToList();
			if (list3.Count > 0)
			{
				multiplierUI._UI?.Show(list3);
			}
			else
			{
				multiplierUI._UI?.TurnOffUI();
			}
		}
	}

	public void ShowPlayerHUD(bool show)
	{
		mAvatarInfoBG.SetVisibility(show);
		mAvatarXpMeterProgressBar.SetVisibility(show);
		mAvatarLevel.SetVisibility(show);
		mAvatarName.SetVisibility(show);
		mAvatarHPMeterProgress.SetVisibility(show);
		mAvatarHPTxt.SetVisibility(show);
	}

	private void InitChatUI()
	{
		bool flag = UtPlatform.IsMobile();
		if (_UiChatHistory != null)
		{
			_UiChatHistory.SetActive(!flag);
		}
		if (_UiChatHistoryMobile != null)
		{
			_UiChatHistoryMobile.SetActive(flag);
		}
		if (_UiMessageLog != null)
		{
			_UiMessageLog.SetActive(flag);
		}
		if (flag)
		{
			UiChatHistory._IsVisible = false;
		}
	}

	private void OpenCompass()
	{
		if (CommonInventoryData.pInstance.FindItem(_CompassItemID) != null)
		{
			UiCompass.ToggleCompass();
		}
	}

	private void OpenSettings()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = GameConfig.GetKeyData("OptionsAsset").Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OptionsBundleReady, typeof(GameObject));
	}

	private void OpenStoreForBuyTask()
	{
		Task playerActiveTask = MissionManagerDO.GetPlayerActiveTask();
		int outCategoryID = -1;
		int outStoreID = -1;
		bool buyTaskInfo = GetBuyTaskInfo(playerActiveTask, out outStoreID, out outCategoryID);
		if (!buyTaskInfo)
		{
			List<Task> tasks = MissionManager.pInstance.GetTasks("Buy", null, null);
			if (tasks != null)
			{
				foreach (Task item in tasks)
				{
					buyTaskInfo = GetBuyTaskInfo(item, out outStoreID, out outCategoryID);
					if (buyTaskInfo)
					{
						break;
					}
				}
			}
		}
		if (buyTaskInfo)
		{
			StoreLoader.Load(setDefaultMenuItem: true, outCategoryID, outStoreID, base.gameObject);
		}
		else
		{
			StoreLoader.Load(setDefaultMenuItem: true, _StoreCategory, _Store, base.gameObject);
		}
	}

	private bool GetBuyTaskInfo(Task inTask, out int outStoreID, out int outCategoryID)
	{
		outCategoryID = 0;
		outStoreID = 0;
		if (inTask != null && inTask != null && inTask.pData != null && inTask.pData.Type == "Buy")
		{
			if (_BuyTaskStoreMapping != null && _BuyTaskStoreMapping.Count > 0)
			{
				BuyTaskStoreMapping buyTaskStoreMapping = _BuyTaskStoreMapping.Find((BuyTaskStoreMapping t) => t._TaskID == inTask.TaskID);
				if (buyTaskStoreMapping != null)
				{
					outStoreID = buyTaskStoreMapping._StoreID;
					outCategoryID = buyTaskStoreMapping._CategoryID;
					return true;
				}
			}
			foreach (TaskObjective objective in inTask.pData.Objectives)
			{
				outCategoryID = objective.Get<int>("CategoryID");
				outStoreID = objective.Get<int>("StoreID");
				if (outCategoryID != 0 && outStoreID != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void GlidingEvent(bool gliding)
	{
		SetButtonsVisiblity(!gliding);
		UpdateChatUiOnGlide(gliding);
		_UiAvatarCSM.MakeAvatarParticleActive(!gliding);
	}

	private void UpdateChatUiOnGlide(bool isGliding)
	{
		if (UtPlatform.IsMobile())
		{
			if (_UiChatHistoryMobile != null)
			{
				_UiChatHistoryMobile.SetActive(!isGliding);
			}
			if (_UiMessageLog != null)
			{
				UiMessageLog component = _UiMessageLog.GetComponent<UiMessageLog>();
				if (component != null)
				{
					component.SetState(isGliding ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
				}
			}
		}
		else
		{
			if (_UiChatHistory != null)
			{
				_UiChatHistory.SetActive(!isGliding);
			}
			mBtnChatHistory.SetState(isGliding ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
		}
	}

	private void CheckQuestBranchBtnVisibility()
	{
		if (!(mQuestBranchBtn != null))
		{
			return;
		}
		if (FUEManager.pIsFUERunning)
		{
			mQuestBranchBtn.SetVisibility(inVisible: false);
			return;
		}
		List<int> missionIds = ((MissionManagerDO)MissionManager.pInstance)._QuestBranchMissions._MissionIds;
		int num = 0;
		if (missionIds == null || missionIds.Count < 1)
		{
			mQuestBranchBtn.SetVisibility(inVisible: false);
			return;
		}
		for (int i = 0; i < missionIds.Count; i++)
		{
			Mission mission = MissionManager.pInstance.GetMission(missionIds[i]);
			if (mission != null && mission.pCompleted)
			{
				num++;
			}
		}
		mQuestBranchBtn.SetVisibility(num != missionIds.Count);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (inVisible)
		{
			CheckQuestBranchBtnVisibility();
		}
	}

	public void SetBackBtnEnabled(bool enable)
	{
		if (mBackBtn != null)
		{
			mBackBtn.SetDisabled(!enable);
		}
	}

	public void SetButtonsVisiblity(bool visible)
	{
		if (mBackBtn != null)
		{
			mBackBtn.SetDisabled(!visible);
		}
		if (!UtPlatform.IsMobile())
		{
			if (mBtnChatHistory != null)
			{
				mBtnChatHistory.SetVisibility(visible);
			}
			if (!visible)
			{
				UiChatHistory._IsVisible = visible;
			}
		}
		else if (_UiMessageLog != null)
		{
			_UiMessageLog.SetActive(visible);
		}
		if (mPDABtn != null)
		{
			mPDABtn.SetVisibility(visible);
		}
		if (mQuestBranchBtn != null)
		{
			if (visible)
			{
				CheckQuestBranchBtnVisibility();
			}
			else
			{
				mQuestBranchBtn.SetVisibility(inVisible: false);
			}
		}
		if (mStoreBtn != null)
		{
			mStoreBtn.SetVisibility(visible);
		}
		if (mGemTotalBtn != null)
		{
			mGemTotalBtn.SetVisibility(visible);
		}
		if (mUiHUDEvents != null)
		{
			for (int i = 0; i < mUiHUDEvents.Length; i++)
			{
				mUiHUDEvents[i].gameObject.SetActive(visible);
			}
		}
		if (mCoinTotalBtn != null)
		{
			mCoinTotalBtn.SetVisibility(visible);
		}
		if (mGlowEffectCountDown != null)
		{
			mGlowEffectCountDown.SetVisibility(visible);
		}
		if (mUiAdReward != null)
		{
			mUiAdReward.HideButton(!visible);
		}
		if (mUiHUDPromoOffer != null)
		{
			mUiHUDPromoOffer.gameObject.SetActive(visible);
		}
	}
}
