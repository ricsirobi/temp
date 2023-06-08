using System.Collections.Generic;
using UnityEngine;

public class UiRacingLobby : KAUI
{
	private class DragonInfoData
	{
		public KAWidget _Widget;

		public string _DragonName;

		public DragonInfoData(KAWidget widget, string dragonName)
		{
			_Widget = widget;
			_DragonName = dragonName;
		}
	}

	public int _MaxSkillDisplay = 3;

	public int _MaxMMOPlayers = 5;

	public LocaleString _ExitConfirmText = new LocaleString("A race is in progress. Are you sure you want to exit to the main lobby?");

	public GameObject _ChatWindow;

	public float _RaceExitDBDisplayTime = 2f;

	private KAWidget mTxtTimer;

	private KAWidget mBtnReadyToRace;

	private KAWidget mBtnBack;

	private KAWidget mPlrBtnStateOn;

	private KAWidget mPlrBtnStateOff;

	private bool mIsReadyToRace;

	public Texture _GenericDragonTexture;

	private MainMenu mMainMenu;

	private Dictionary<string, LobbyPlayer> mPlayers = new Dictionary<string, LobbyPlayer>();

	private bool mIsTrackReady;

	public Dictionary<string, LobbyPlayer> pPlayers => mPlayers;

	protected override void Start()
	{
		base.Start();
		mTxtTimer = FindItem("TxtTimer");
		mTxtTimer.SetText("--");
		mBtnReadyToRace = FindItem("BtnReadyToRace");
		mBtnBack = FindItem("BtnBack");
		if (_ChatWindow != null)
		{
			_ChatWindow.SetActive(value: false);
		}
	}

	public void EnableUi(MainMenu menu)
	{
		if (RacingManager.Instance != null)
		{
			RacingManager.Instance.State = RacingManagerState.InLobby;
		}
		SetVisibility(inVisible: true);
		SetState(KAUIState.INTERACTIVE);
		mMainMenu = menu;
		ClearMMOWidgets();
		SetTrackInfo();
		((KACheckBox)FindItem("BtnPlayerRaceReady")).SetChecked(isChecked: false);
		mIsReadyToRace = false;
		KAWidget kAWidget = FindItem("PlayerInfo");
		KAWidget kAWidget2 = kAWidget.FindChildItem("Playername");
		string displayName = AvatarData.pInstance.DisplayName;
		string text = UserRankData.pInstance.RankID.ToString();
		string text2 = displayName + " (" + text + ")";
		kAWidget2.SetText(text2);
		kAWidget2.SetVisibility(inVisible: true);
		mPlrBtnStateOn = kAWidget.FindChildItem("PlayerStateOn");
		mPlrBtnStateOff = kAWidget.FindChildItem("PlayerStateOff");
		UiChatHistory._IsVisible = true;
		if (_ChatWindow != null)
		{
			_ChatWindow.SetActive(value: true);
		}
		KAWidget inWidget = kAWidget.FindChildItem("DragonPic");
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SetDragonPicture(SanctuaryManager.pCurPetInstance, inWidget, UserInfo.pInstance.UserID);
			SetDragonInfo(SanctuaryManager.pCurPetInstance, kAWidget, null);
		}
		kAWidget.SetVisibility(inVisible: true);
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.pNotifyObjOnIdlePopUpClose = base.gameObject;
		}
		mBtnBack.SetVisibility(inVisible: true);
	}

	private void OnCloseIdlePopUp()
	{
		ConfirmYes();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if ("BtnBack" == inWidget.name)
		{
			mMainMenu.ShowKAUIDialog("PfKAUIGenericDBSm", "ExitConfirmDB", "ConfirmYes", "DestroyDB", "", "", destroyDB: true, _ExitConfirmText, base.gameObject);
		}
		else if ("BtnEquip" == inWidget.name)
		{
			mMainMenu.ShowEquipScreen(GameScreen.GAME_LOBBY_SCREEN);
		}
		else if (inWidget.name == "BtnPlayerRaceReady")
		{
			KACheckBox kACheckBox = (KACheckBox)inWidget;
			mIsReadyToRace = kACheckBox.IsChecked();
			mMainMenu.pRacingMMOClient.SendPlayerReady(mIsReadyToRace, isToRoom: true);
			if (mPlrBtnStateOn != null)
			{
				mPlrBtnStateOn.SetVisibility(mIsReadyToRace);
			}
			if (mPlrBtnStateOff != null)
			{
				mPlrBtnStateOff.SetVisibility(!mIsReadyToRace);
			}
		}
	}

	private void SetTrackInfo()
	{
		KAWidget kAWidget = FindItem("TrackInfo");
		if (null != kAWidget)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("TrackName");
			KAWidget kAWidget3 = kAWidget.FindChildItem("TrackPic");
			kAWidget2.SetText(mMainMenu.pCurrentTrackData._TrackNameText._Text);
			kAWidget3.SetTexture(mMainMenu.pCurrentTrackData._TrackIcon);
		}
	}

	public void AddPlayer(MMOAvatar player)
	{
		if (!mPlayers.ContainsKey(player.pUserID))
		{
			LobbyPlayer value = new LobbyPlayer(player);
			mPlayers[player.pUserID] = value;
		}
		mBtnReadyToRace.SetDisabled(mPlayers.Count < 1);
	}

	protected override void Update()
	{
		base.Update();
		int num = 1;
		if (!GetVisibility())
		{
			return;
		}
		foreach (KeyValuePair<string, LobbyPlayer> mPlayer in mPlayers)
		{
			LobbyPlayer value = mPlayer.Value;
			if (value.mMMOPlayer.pIsSanctuaryPetReady && !value.mIsPicTaken)
			{
				KAWidget kAWidget = FindItem("MMO" + num);
				KAWidget kAWidget2 = kAWidget.FindChildItem("Playername");
				string displayName = value.mMMOPlayer.pAvatarData.mInstance.DisplayName;
				string text = value.mMMOPlayer.pRankData.RankID.ToString();
				string text2 = displayName + " (" + text + ")";
				kAWidget2.SetText(text2);
				KAWidget inWidget = kAWidget.FindChildItem("DragonPic");
				if (value.mMMOPlayer.pMMOAvatarType == MMOAvatarType.FULL)
				{
					SetDragonPicture(value.mMMOPlayer.pSanctuaryPet, inWidget, value.mMMOPlayer.pUserID);
					SetDragonInfo(value.mMMOPlayer.pSanctuaryPet, kAWidget, value.mMMOPlayer);
				}
				value.mIsPicTaken = true;
				value.mWidgetIndex = num;
				kAWidget.SetVisibility(inVisible: true);
				kAWidget.SetState(KAUIState.INTERACTIVE);
			}
			num++;
		}
	}

	public void SetPlayerState(string userId, bool isReady)
	{
		foreach (KeyValuePair<string, LobbyPlayer> mPlayer in mPlayers)
		{
			if (mPlayer.Value.mMMOPlayer.pUserID.Equals(userId))
			{
				KAWidget kAWidget = FindItem("MMO" + mPlayer.Value.mWidgetIndex);
				if (kAWidget != null)
				{
					KAWidget kAWidget2 = kAWidget.FindChildItem("PlayerStateOn");
					KAWidget kAWidget3 = kAWidget.FindChildItem("PlayerStateOff");
					kAWidget2.SetVisibility(isReady);
					kAWidget3.SetVisibility(!isReady);
				}
			}
		}
	}

	public void RemovePlayer(MMOAvatar player)
	{
		if (player == null)
		{
			return;
		}
		LobbyPlayer value = null;
		if (!mPlayers.TryGetValue(player.pUserID, out value) || value == null)
		{
			return;
		}
		KAWidget kAWidget = FindItem("MMO" + value.mWidgetIndex);
		if (!(kAWidget == null))
		{
			mPlayers.Remove(player.pUserID);
			ClearMMOWidget(kAWidget);
			if ((bool)mBtnReadyToRace)
			{
				mBtnReadyToRace.SetDisabled(mPlayers.Count < 1);
			}
		}
	}

	private void ClearMMOWidgets()
	{
		for (int i = 1; i <= _MaxMMOPlayers; i++)
		{
			KAWidget kAWidget = FindItem("MMO" + i);
			if (kAWidget == null)
			{
				break;
			}
			ClearMMOWidget(kAWidget);
		}
	}

	private void ClearMMOWidget(KAWidget widget)
	{
		KAWidget kAWidget = widget.FindChildItem("Playername");
		if (kAWidget != null)
		{
			kAWidget.SetText(string.Empty);
		}
		KAWidget kAWidget2 = widget.FindChildItem("DragonPic");
		if (kAWidget2 != null && _GenericDragonTexture != null)
		{
			kAWidget2.SetTexture(_GenericDragonTexture);
		}
		KAWidget kAWidget3 = widget.FindChildItem("TxtDragonInfo");
		if (kAWidget3 != null)
		{
			kAWidget3.SetText(string.Empty);
		}
		KAWidget kAWidget4 = widget.FindChildItem("PlayerStateOn");
		if (kAWidget4 != null)
		{
			kAWidget4.SetVisibility(inVisible: false);
		}
		KAWidget kAWidget5 = widget.FindChildItem("PlayerStateOff");
		if (kAWidget5 != null)
		{
			kAWidget5.SetVisibility(inVisible: true);
		}
	}

	public void UpdateTime(float count)
	{
		mTxtTimer.SetText(count.ToString());
		if (mMainMenu != null && count < mMainMenu._LobbyLockedTime)
		{
			mBtnBack.SetVisibility(inVisible: false);
		}
	}

	public void ConfirmYes()
	{
		UiChatHistory._IsVisible = false;
		if (_ChatWindow != null)
		{
			_ChatWindow.SetActive(value: false);
		}
		if (pPlayers.Count >= 1)
		{
			RacingManager.Instance.TrySetFinishReason(DNFType.LobbyExit);
		}
		RacingManager.Instance.ExitLobby();
		MainStreetMMOClient.pInstance.Logout();
		mPlayers.Clear();
		MainStreetMMOClient.pInstance.pNotifyObjOnIdlePopUpClose = null;
		DestroyDB();
		SetVisibility(inVisible: false);
		mMainMenu.MoveNext(GameScreen.GAME_HOST_JOIN_SCREEN);
	}

	private void DestroyDB()
	{
		mMainMenu.DestroyKAUIDB();
	}

	private void SetDragonPicture(SanctuaryPet inPet, KAWidget inWidget, string inUserID)
	{
		if (inWidget != null)
		{
			int num = (inPet.pData.ImagePosition.HasValue ? inPet.pData.ImagePosition.Value : 0);
			inWidget.SetVisibility(inVisible: false);
			if (!string.IsNullOrEmpty(inUserID) && UserInfo.pInstance.UserID != inUserID)
			{
				WsWebService.GetImageDataByUserId(inUserID, "EggColor", num, ImageDataServiceEventHandler, inWidget);
			}
			else
			{
				ImageData.Load("EggColor", num, base.gameObject);
			}
		}
	}

	private void ImageDataServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inType == WsServiceType.GET_IMAGE_BY_USER_ID && inObject != null)
		{
			ImageData imageData = (ImageData)inObject;
			((KAWidget)inUserData).SetTextureFromURL(imageData.ImageURL, base.gameObject);
		}
	}

	private void OnImageLoaded(ImageDataInstance img)
	{
		if (!(img.mIconTexture == null))
		{
			KAWidget kAWidget = FindItem("PlayerInfo").FindChildItem("DragonPic");
			if (kAWidget != null && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pData.ImagePosition.Value == img.mSlotIndex)
			{
				kAWidget.SetTexture(img.mIconTexture);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
	}

	private void OnTextureLoaded(KAWidget widget)
	{
		widget.SetVisibility(inVisible: true);
	}

	private void SetDragonInfo(SanctuaryPet inPet, KAWidget inParent, MMOAvatar inMMO)
	{
		KAWidget kAWidget = inParent.FindChildItem("TxtDragonInfo");
		if (kAWidget != null && inPet.pData != null)
		{
			if (inMMO != null)
			{
				string text = inPet.pData.Name + " (" + inPet.pData.pRank + ")";
				kAWidget.SetText(text);
				kAWidget.SetVisibility(inVisible: true);
			}
			else
			{
				DragonInfoData inUserData = new DragonInfoData(kAWidget, inPet.pData.Name);
				PetRankData.LoadUserRank(inPet.pData, OnUserRankReady, forceLoad: false, inUserData);
			}
		}
	}

	private void OnUserRankReady(UserRank rank, object userData)
	{
		if (rank != null && userData != null)
		{
			DragonInfoData obj = (DragonInfoData)userData;
			KAWidget widget = obj._Widget;
			string text = string.Concat(str2: rank.RankID.ToString(), str0: obj._DragonName, str1: " (", str3: ")");
			widget.SetText(text);
			widget.SetVisibility(inVisible: true);
		}
	}

	public void OnLevelReady()
	{
		KAInput.pInstance.ShowInputs(inShow: false);
	}
}
