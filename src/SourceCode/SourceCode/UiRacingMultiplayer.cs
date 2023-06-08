using System.Collections.Generic;
using UnityEngine;

public class UiRacingMultiplayer : UiLobbyBase
{
	public class PlayerDataInMenu
	{
		public MMOAvatar _MMOAvatar;

		public bool _Ready;
	}

	private enum RaceType
	{
		SINGLE_PLAYER,
		MULTI_PLAYER
	}

	public int _MinPlayers = 5;

	public int _MaxSkillDisplay = 3;

	public GameObject _ChatWindow;

	public LocaleString _ExitConfirmMsg = new LocaleString("Are you sure you want to exit?");

	public LocaleString _SinglePlayerTransitionText = new LocaleString("You are getting into single player mode");

	public LocaleString _PleaseWait = new LocaleString("Please wait while you are being connected to a room..");

	public LocaleString _UnableToJoin = new LocaleString("Unable to join the room right now");

	public LocaleString _WaitingForPlayers = new LocaleString("Waiting for players");

	public LocaleString _ReadyToRaceText = new LocaleString("Join a Race.");

	public LocaleString _DragonBusyText = new LocaleString("Dragon is busy with stable quest");

	public LocaleString _RacePenaltyText = new LocaleString("[Review]You did not finish your previous race. You must wait before entering another race. You may enter in:");

	public LocaleString _LobbyPenaltyText = new LocaleString("[Review]Previous race was aborted, You must wait before entering another race. You may enter in:");

	public float _JoinWaitTime = 10f;

	public float _CoolDownDuration = 2f;

	public static UiRacingMultiplayer pInstance;

	private float mJoinTimeDuration;

	private UiRacingPlayerListMenu mPlayersMenu;

	private AvPhotoManager mStillPhotoManager;

	private KAWidget mDragonPic;

	private KAWidget mPlayerDragonPicBtn;

	private KAWidget[] mDragonSkill;

	private KAWidget mDragonInfo;

	private KAWidget mStartRacing;

	private bool mIsPlayerPetReady;

	private bool mHasJoinedRoomCheck;

	private bool mIsReadyToRace;

	private KACheckBox mIsReadyCB;

	private float mCoolDownTimer;

	private KAWidget mTextWaitingForPlayers;

	private MainMenu mMainMenu;

	private KAWidget mButtonBack;

	private KAWidget mButtonHelp;

	private KAWidget mButtonCloseHelp;

	private KAWidget mButtonSP;

	private KAWidget mButtonEquip;

	private bool mTutorialActive;

	private List<PlayerDataInMenu> mPlayersToBeAdded = new List<PlayerDataInMenu>();

	private UiDragonsInfoCardItem mInfoCardItem;

	private RaceType mSelectedRaceType;

	protected override void Start()
	{
		base.Start();
		if (!pInstance)
		{
			pInstance = this;
		}
		mPlayersMenu = (UiRacingPlayerListMenu)GetMenu("UiRacingPlayerListMenu");
		mInfoCardItem = (UiDragonsInfoCardItem)FindItem("TemplateInfoCardItem");
		mButtonBack = FindItem("BtnBack");
		mButtonHelp = FindItem("BtnHelp");
		mButtonCloseHelp = FindItem("BtnCloseHelp");
		mButtonSP = FindItem("BtnSP");
		mButtonEquip = FindItem("BtnEquip");
		if (!ProductData.TutorialComplete("CheckpointRacingTutorial"))
		{
			ShowTutorial(active: true);
			ProductData.AddTutorial("CheckpointRacingTutorial");
		}
	}

	public override void OnDragonSelectionMenuScroll(UIScrollBar scroll = null)
	{
		base.OnDragonSelectionMenuScroll();
		if (mIsReadyToRace)
		{
			mIsReadyCB.SetChecked(isChecked: false);
			mIsReadyToRace = false;
			UpdateReadyStatus();
		}
	}

	private void UpdateReadyStatus()
	{
		if (mCoolDownTimer == 0f)
		{
			if (!mIsReadyToRace)
			{
				BeginCoolDown();
			}
			mTextWaitingForPlayers.SetVisibility(mIsReadyToRace);
			mMainMenu.pRacingMMOClient.SendPlayerReady(mIsReadyToRace);
		}
	}

	private void BeginCoolDown()
	{
		mCoolDownTimer = Time.time;
		mIsReadyCB.SetDisabled(isDisabled: true);
	}

	public void EnableUi(MainMenu menu)
	{
		if (RacingManager.Instance != null)
		{
			RacingManager.Instance.State = RacingManagerState.None;
		}
		mHasJoinedRoomCheck = false;
		mJoinTimeDuration = Time.realtimeSinceStartup;
		SetState(KAUIState.DISABLED);
		mIsReadyToRace = false;
		mMainMenu = menu;
		SetVisibility(inVisible: true);
		SetState(KAUIState.INTERACTIVE);
		mMainMenu.ShowKAUIDialog("PfKAUIGenericDB", "Please wait", "", "", "", "", destroyDB: true, _PleaseWait);
		UiChatHistory._IsVisible = true;
		if (_ChatWindow != null && !mTutorialActive)
		{
			_ChatWindow.SetActive(value: true);
		}
		if (mPlayersMenu != null)
		{
			mPlayersMenu.ClearItems();
		}
		if (mDragonSkill == null)
		{
			mDragonSkill = new KAWidget[_MaxSkillDisplay];
		}
		mIsReadyCB = (KACheckBox)FindItem("BtnPlayerReady");
		mIsReadyCB.SetChecked(isChecked: false);
		mStartRacing = FindItem("BtnStartRacing");
		mStillPhotoManager = AvPhotoManager.Init("PfMessagePhotoMgr");
		mDragonPic = FindItem("PlayerDragonPic");
		mTextWaitingForPlayers = FindItem("TxtWaitingForPlayers");
		mTextWaitingForPlayers.SetTextByID(_WaitingForPlayers._ID, _WaitingForPlayers._Text);
		for (int i = 1; i <= _MaxSkillDisplay; i++)
		{
			mDragonSkill[i - 1] = FindItem("SkillItem" + i);
			if (mDragonSkill[i - 1] != null)
			{
				mDragonSkill[i - 1].SetVisibility(inVisible: false);
			}
		}
		mDragonInfo = FindItem("TxtDragonInfo");
		mPlayerDragonPicBtn = FindItem("BtnMe");
		mIsPlayerPetReady = false;
		if (!mIsPlayerPetReady && SanctuaryManager.pCurPetInstance != null)
		{
			mIsPlayerPetReady = true;
			SetDragonSelectionMenu();
		}
		else
		{
			SetInteractive(interactive: false);
		}
		mPlayerDragonPicBtn.SetVisibility(inVisible: false);
		mInfoCardItem.SetVisibility(inVisible: false);
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.pNotifyObjOnIdlePopUpClose = base.gameObject;
		}
	}

	private void OnSelectDragonFinish(int petID)
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.gameObject.SetActive(value: false);
		}
		StartRace(mSelectedRaceType);
		SetInteractive(interactive: true);
	}

	private void OnSelectDragonFailed(int petID)
	{
		SetInteractive(interactive: true);
		mIsReadyToRace = false;
		mIsReadyCB.SetChecked(isChecked: false);
	}

	private void OnCloseIdlePopUp()
	{
		if (mPlayersMenu != null)
		{
			mPlayersMenu.ClearItems();
		}
	}

	public void AddPlayerToList(MMOAvatar player)
	{
		PlayerDataInMenu playerDataInMenu = new PlayerDataInMenu();
		playerDataInMenu._MMOAvatar = player;
		mPlayersToBeAdded.Add(playerDataInMenu);
	}

	public void AddPlayer(MMOAvatar player)
	{
		int numItems = mPlayersMenu.GetNumItems();
		KAWidget kAWidget = mPlayersMenu.DuplicateWidget(mPlayersMenu._Template);
		kAWidget.name = player.pUserID;
		KAWidget kAWidget2 = kAWidget.FindChildItem("PlayerSelected");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: true);
		}
		KAWidget kAWidget3 = kAWidget.FindChildItem("Playername");
		kAWidget3.SetText(player.pAvatarData.mInstance.DisplayName);
		kAWidget3.SetVisibility(inVisible: true);
		KAWidget kAWidget4 = kAWidget.FindChildItem("PlayerPicBkg");
		if (kAWidget4 != null)
		{
			kAWidget4.SetVisibility(inVisible: true);
		}
		KAWidget kAWidget5 = kAWidget.FindChildItem("PlayerStateOff");
		if (kAWidget5 != null)
		{
			kAWidget5.SetVisibility(inVisible: true);
		}
		KAWidget kAWidget6 = kAWidget.FindChildItem("PlayerPic");
		if (null != kAWidget6)
		{
			AvPhotoSetter @object = new AvPhotoSetter(kAWidget6);
			mStillPhotoManager.TakePhotoUI(player.pUserID, (Texture2D)kAWidget6.GetTexture(), @object.PhotoCallback, null);
			kAWidget6.SetVisibility(inVisible: true);
		}
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.SetDisabled(isDisabled: false);
		SetPlayerState(player.pUserID, ready: false);
		mPlayersMenu.AddWidgetAt(numItems, kAWidget);
	}

	public void RemovePlayer(MMOAvatar player)
	{
		KAWidget kAWidget = mPlayersMenu.FindItem(player.pUserID);
		if (kAWidget != null)
		{
			mPlayersMenu.RemoveWidget(kAWidget);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!mHasJoinedRoomCheck || SanctuaryManager.pCurPetInstance == null)
		{
			return;
		}
		base.OnClick(inWidget);
		UiDragonsInfoCardItem uiDragonsInfoCardItem = (UiDragonsInfoCardItem)mDragonSelectionMenu.GetItemsInView()[0];
		if (inWidget == mIsReadyCB)
		{
			if (!RacingManager.Instance.Ready)
			{
				if (mIsReadyCB.IsChecked())
				{
					mIsReadyCB.SetChecked(isChecked: false);
				}
				UtDebug.Log("Block player until Penalty data to be load.", 768u);
				return;
			}
			RacingManager.Instance.CheckResetRequired();
			if (RacingManager.Instance.GetPenaltyBlockDuration().TotalSeconds <= 0.0)
			{
				if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(uiDragonsInfoCardItem.pSelectedPetID))
				{
					GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _DragonBusyText.GetLocalizedString(), base.gameObject, "OnDBClose");
					if (mIsReadyCB.IsChecked())
					{
						mIsReadyCB.SetChecked(isChecked: false);
					}
				}
				else
				{
					RacingManager.Instance.Reset();
					mIsReadyToRace = mIsReadyCB.IsChecked();
					LoadDragonAndStartRace(RaceType.MULTI_PLAYER);
				}
			}
			else
			{
				string inText = ((RacingManager.Instance.GetPenaltyType() == RacingManagerState.InLobby) ? _LobbyPenaltyText.GetLocalizedString() : _RacePenaltyText.GetLocalizedString());
				mIsReadyCB.SetChecked(isChecked: false);
				((KAUIGenericTimerDB)GameUtilities.DisplayOKMessage("PfKAUIGenericDBTimer", inText, null, base.gameObject, "OnDBClose")).Init(showTimer: true, RacingManager.Instance.GetPenaltyEndTime(), TimeFormat.HHMMSS);
			}
		}
		else if (inWidget == mButtonSP)
		{
			if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(uiDragonsInfoCardItem.pSelectedPetID))
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _DragonBusyText.GetLocalizedString(), base.gameObject, "OnDBClose");
			}
			else
			{
				LoadDragonAndStartRace(RaceType.SINGLE_PLAYER);
			}
		}
		else if (inWidget == mPlayerDragonPicBtn)
		{
			if (mDragonSelectionMenu != null)
			{
				mDragonSelectionMenu.SetVisibility(inVisible: true);
			}
			mPlayerDragonPicBtn.SetVisibility(inVisible: false);
			mInfoCardItem.SetVisibility(inVisible: false);
		}
		else if (inWidget == mButtonBack)
		{
			mMainMenu.ShowKAUIDialog("PfKAUIGenericDBSm", "ExitConfirmDB", "ConfirmExit", "DestroyDB", "", "", destroyDB: true, _ExitConfirmMsg, base.gameObject);
		}
		else if (inWidget == mButtonEquip)
		{
			mMainMenu.ShowEquipScreen(GameScreen.GAME_HOST_JOIN_SCREEN);
		}
		else if (inWidget == mButtonHelp)
		{
			ShowTutorial(active: true);
		}
		else if (inWidget == mButtonCloseHelp)
		{
			ShowTutorial(active: false);
		}
	}

	private void OnDBClose()
	{
	}

	private void LoadDragonAndStartRace(RaceType type)
	{
		mSelectedRaceType = type;
		List<KAWidget> itemsInView = mDragonSelectionMenu.GetItemsInView();
		if (itemsInView.Count >= 1)
		{
			LoadSelectedDragon(itemsInView[0]);
		}
		else
		{
			StartRace(type);
		}
	}

	private void StartRace(RaceType type)
	{
		int ageIndex = RaisedPetData.GetAgeIndex(SanctuaryManager.pCurPetData.pStage);
		if (SanctuaryManager.pCurPetInstance.IsActionAllowed(PetActions.RACING) && ageIndex >= SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToFly)
		{
			switch (type)
			{
			case RaceType.SINGLE_PLAYER:
				MainStreetMMOClient.pInstance.pNotifyObjOnIdlePopUpClose = null;
				MainStreetMMOClient.pInstance.Disconnect();
				mMainMenu.ShowKAUIDialog("PfKAUIGenericDB", "Message", "", "", "", "", destroyDB: true, _SinglePlayerTransitionText);
				Invoke("DisplaySingleplayerUI", mMainMenu._DisplayTime);
				break;
			case RaceType.MULTI_PLAYER:
				UpdateReadyStatus();
				break;
			}
		}
		else
		{
			CheckRacingConstraints(ageIndex);
		}
	}

	private void CheckRacingConstraints(int inDragonAge)
	{
		mIsReadyCB.SetChecked(isChecked: false);
		mIsReadyToRace = false;
		if (inDragonAge < SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToFly)
		{
			SetInteractive(interactive: false);
			mPlayersMenu.SetInteractive(interactive: false);
			mDragonSelectionMenu.SetInteractive(interactive: false);
			DragonAgeUpConfig.ShowAgeUpUI(OnDragonAgeUpDone, SanctuaryManager.pCurPetData.pStage);
		}
		else
		{
			UiPetEnergyGenericDB.Show(base.gameObject, null, null, isLowEnergy: true);
		}
	}

	private void DisplaySingleplayerUI()
	{
		DestroyDB();
		SetVisibility(inVisible: false);
		UiChatHistory._IsVisible = false;
		RacingManager.pIsSinglePlayer = true;
		mMainMenu.MoveNext(GameScreen.GAME_MODE_SCREEN);
	}

	protected override void Update()
	{
		if (mCoolDownTimer > 0f)
		{
			float num = Time.time - mCoolDownTimer;
			if (num >= _CoolDownDuration - 1f)
			{
				mCoolDownTimer = 0f;
				mIsReadyCB.SetDisabled(isDisabled: false);
				mStartRacing.SetText(_ReadyToRaceText.GetLocalizedString());
			}
			else
			{
				mStartRacing.SetText((_CoolDownDuration - num).ToString("0"));
			}
		}
		if (GetVisibility())
		{
			if (!mIsPlayerPetReady && SanctuaryManager.pCurPetInstance != null)
			{
				mIsPlayerPetReady = true;
				SetDragonSelectionMenu();
				SetInteractive(interactive: true);
			}
			if (!mHasJoinedRoomCheck && mMainMenu != null)
			{
				if (MainStreetMMOClient.pIsReady)
				{
					mHasJoinedRoomCheck = true;
					DestroyDB();
				}
				else if (Time.realtimeSinceStartup - mJoinTimeDuration >= _JoinWaitTime)
				{
					mHasJoinedRoomCheck = true;
					DestroyDB();
					mMainMenu.ShowKAUIDialog("PfKAUIGenericDB", "Connect Failed", "ConfirmRetry", "ConfirmExit", "", "", destroyDB: true, _UnableToJoin, base.gameObject);
				}
			}
			if (mPlayersToBeAdded.Count > 0)
			{
				PlayerDataInMenu playerDataInMenu = mPlayersToBeAdded[0];
				mPlayersToBeAdded.RemoveAt(0);
				AddPlayer(playerDataInMenu._MMOAvatar);
				if (playerDataInMenu._Ready)
				{
					SetPlayerState(playerDataInMenu._MMOAvatar.pUserID, playerDataInMenu._Ready);
				}
			}
			if (Input.GetKeyUp(KeyCode.Escape) && GetState() != KAUIState.NOT_INTERACTIVE)
			{
				if (mTutorialActive)
				{
					ShowTutorial(active: false);
				}
				else
				{
					OnClick(mButtonBack);
				}
			}
		}
		base.Update();
	}

	public void SetPlayerState(string userId, bool ready)
	{
		if (!(UserInfo.pInstance.UserID != userId))
		{
			return;
		}
		foreach (PlayerDataInMenu item in mPlayersToBeAdded)
		{
			if (item._MMOAvatar.pUserID == userId)
			{
				item._Ready = ready;
				break;
			}
		}
		mPlayersMenu.UpdatePlayerState(userId, ready);
	}

	public void LobbyTransition()
	{
		mTextWaitingForPlayers.SetVisibility(inVisible: false);
	}

	public void ShowMMODragonInfo(string userId)
	{
		if (MainStreetMMOClient.pInstance == null)
		{
			return;
		}
		if (mDragonSelectionMenu != null)
		{
			mDragonSelectionMenu.SetVisibility(inVisible: false);
		}
		if (mInfoCardItem != null)
		{
			MMOAvatar player = MainStreetMMOClient.pInstance.GetPlayer(userId);
			if (player != null && player.pSanctuaryPet != null)
			{
				mInfoCardItem.SetMessageObject(base.gameObject);
				mInfoCardItem.SetVisibility(inVisible: true);
				mInfoCardItem.pUserID = userId;
				mInfoCardItem.pSelectedPetData = player.pSanctuaryPet.pData;
				mInfoCardItem.RefreshUI();
				mInfoCardItem.SetButtons(selectBtn: false, visitBtn: false, moveInBtn: false);
			}
		}
		mPlayerDragonPicBtn.SetVisibility(inVisible: true);
	}

	private void PetPicServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != WsServiceEvent.COMPLETE)
		{
			return;
		}
		if (inObject != null)
		{
			RsResourceManager.Load(((ImageData)inObject).ImageURL, OnResLoadingEvent);
			return;
		}
		ImageUserData imageUserData = (ImageUserData)inUserData;
		UtDebug.Log("No image data loaded for slot" + imageUserData._Index, 1);
		imageUserData._Index++;
		if (imageUserData._Index < 2)
		{
			WsWebService.GetImageDataByUserId(imageUserData._Type, "EggColor", imageUserData._Index, PetPicServiceEventHandler, imageUserData);
		}
	}

	private void OnResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			OnResourceReady(inURL, inObject);
		}
	}

	private void OnResourceReady(string inURL, object inObject)
	{
		Texture2D texture2D = (Texture2D)inObject;
		if (mDragonPic != null && texture2D != null)
		{
			texture2D.name = "DragonRacingPetImage";
			mDragonPic.SetTexture(texture2D);
		}
		mPlayerDragonPicBtn.SetVisibility(inVisible: true);
	}

	private void ConfirmExit()
	{
		DestroyDB();
		UiChatHistory._IsVisible = false;
		if (_ChatWindow != null)
		{
			_ChatWindow.SetActive(value: false);
		}
		SetVisibility(inVisible: false);
		mMainMenu.ClearPictureCache();
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.pNotifyObjOnIdlePopUpClose = null;
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED, forceUpdate: true);
		}
		MainStreetMMOClient.pInstance.ForceEnableMultiplayer(enable: false);
		UtUtilities.LoadLevel(mMainMenu._LastLevelToLoad);
		RacingManager.Instance.DestroyInstance();
	}

	private void ConfirmRetry()
	{
		DestroyDB();
		mMainMenu.ShowKAUIDialog("PfKAUIGenericDB", "Please wait", "", "", "", "", destroyDB: true, _PleaseWait);
		mHasJoinedRoomCheck = false;
		mJoinTimeDuration = Time.realtimeSinceStartup;
	}

	private void DestroyDB()
	{
		if (mMainMenu != null)
		{
			mMainMenu.DestroyKAUIDB();
		}
	}

	private void SetDragonInfo(SanctuaryPet inPet, MMOAvatar inMMO)
	{
		if (mDragonInfo != null && inPet.pData != null)
		{
			string inUserData = inPet.pData.Name;
			PetRankData.LoadUserRank(inPet.pData, OnUserRankReady, forceLoad: false, inUserData);
		}
	}

	private void OnUserRankReady(UserRank rank, object userData)
	{
		if (rank != null && userData != null)
		{
			string obj = (string)userData;
			string text = rank.RankID.ToString();
			string text2 = obj + " (" + text + ")";
			mDragonInfo.SetText(text2);
		}
	}

	private void SetDragonPicture(GameObject inDragonObject, KAWidget inWidget, string inUserID)
	{
		if (inWidget != null)
		{
			Texture2D dragonPicture = mMainMenu.GetDragonPicture(inDragonObject, inWidget, inUserID);
			if (dragonPicture != null)
			{
				inWidget.SetTexture(dragonPicture);
			}
		}
	}

	private void OnDragonAgeUpDone()
	{
		SetInteractive(interactive: true);
		mPlayersMenu.SetInteractive(interactive: true);
		mDragonSelectionMenu.SetInteractive(interactive: true);
	}

	private void ShowTutorial(bool active)
	{
		mButtonCloseHelp.SetVisibility(active);
		mButtonBack.SetVisibility(!active);
		mButtonHelp.SetVisibility(!active);
		_ChatWindow.SetActive(!active);
		mPlayersMenu.SetVisibility(!active);
		mTutorialActive = active;
	}
}
