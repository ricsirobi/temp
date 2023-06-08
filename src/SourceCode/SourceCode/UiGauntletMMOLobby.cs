using UnityEngine;

public class UiGauntletMMOLobby : KAUI
{
	public GauntletRailShootManager _GameManager;

	public UiGSBuddySelect _BuddySelectScreen;

	public LocaleString _ReadyText;

	public LocaleString _NotReadyText;

	public AudioClip[] _HelpVO;

	public AudioClip _NonMemberVO;

	private KAWidget mBtnBack;

	private KAWidget mBtnHelp;

	private KAWidget mTxtMessage;

	private KAWidget mBtnReady;

	private KAWidget mBtnNotReady;

	private KAWidget mBtnInvite;

	private KAWidget mIcoLock;

	private KAWidget[] mStatus;

	private KAWidget[] mName;

	private KAWidget[] mPicture;

	private KAToggleButton mBtnTrainingLevel;

	private KAToggleButton mBtnRedPlanetLevel;

	private bool mIsRedPlanetLocked;

	protected override void Start()
	{
		base.Start();
		mBtnBack = FindItem("btnBack");
		mBtnHelp = FindItem("btnHelp");
		mTxtMessage = FindItem("txtMessage");
		mBtnReady = FindItem("btnReady");
		mBtnNotReady = FindItem("btnNotReady");
		mBtnInvite = FindItem("btnInvite");
		mBtnTrainingLevel = (KAToggleButton)FindItem("btnTrainingLevel");
		mBtnRedPlanetLevel = (KAToggleButton)FindItem("btnRedPlanetLevel");
		mIcoLock = mBtnRedPlanetLevel.FindChildItem("IcoLock");
		mStatus = new KAWidget[2];
		mStatus[0] = FindItem("txtPlayerStatus");
		mStatus[1] = FindItem("txtOpponentStatus");
		mName = new KAWidget[2];
		mName[0] = FindItem("txtPlayerName");
		mName[1] = FindItem("txtOpponentName");
		mPicture = new KAWidget[2];
		mPicture[0] = FindItem("PicPlayer");
		mPicture[1] = FindItem("PicOpponent");
	}

	public override void SetVisibility(bool t)
	{
		base.SetVisibility(t);
		if (t)
		{
			CancelInvoke("EnableInviteBtn");
			mBtnReady.SetVisibility(inVisible: true);
			mBtnNotReady.SetVisibility(inVisible: false);
			_GameManager.pIsMultiplayer = true;
			_GameManager._UiConsumable.SetGameData(_GameManager, "TargetPractice", 1, inForceUpdate: true);
			if (_GameManager != null)
			{
				AvAvatar.pObject = _GameManager.pOldAvatarObject;
			}
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (!(GauntletMMOClient.pInstance != null) || !(_GameManager != null))
		{
			return;
		}
		if (item == mBtnBack)
		{
			OnBackBtn();
		}
		else if (item == mBtnHelp)
		{
			if (_HelpVO != null)
			{
				int num = Random.Range(0, _HelpVO.Length);
				SnChannel.Play(_HelpVO[num], "VO_Pool", inForce: false);
			}
		}
		else if (item == mBtnReady)
		{
			if (_GameManager.IsPetTooTired())
			{
				SetInteractive(interactive: false);
				_GameManager.ProcessPetTired(base.gameObject);
			}
			else
			{
				mBtnReady.SetVisibility(inVisible: false);
				mBtnNotReady.SetVisibility(inVisible: true);
				GauntletMMOClient.pInstance.OnPlayerReady();
			}
		}
		else if (item == mBtnNotReady)
		{
			mBtnReady.SetVisibility(inVisible: true);
			mBtnNotReady.SetVisibility(inVisible: false);
			GauntletMMOClient.pInstance.OnPlayerNotReady();
		}
		else if (item == mBtnInvite)
		{
			SetVisibility(t: false);
			SetState(KAUIState.DISABLED);
			_BuddySelectScreen.SetVisibility(t: true);
		}
		else if (item == mBtnTrainingLevel)
		{
			SetCourseSelected(1);
			GauntletMMOClient.pInstance.OnCourseChanged(1);
		}
		else if (item == mBtnRedPlanetLevel)
		{
			if (mIsRedPlanetLocked)
			{
				mBtnRedPlanetLevel.SetChecked(isChecked: true);
				SnChannel.Play(_NonMemberVO, "VO_Pool", inForce: false);
			}
			else
			{
				SetCourseSelected(2);
				GauntletMMOClient.pInstance.OnCourseChanged(2);
			}
		}
	}

	public void PetEnergyProcessed()
	{
		SetInteractive(interactive: true);
	}

	public void OnBackBtn()
	{
		int num = 0;
		foreach (GauntletMMOPlayer pPlayer in GauntletMMOClient.pInstance.pPlayers)
		{
			if (pPlayer._IsReady)
			{
				num++;
			}
		}
		if (num < 2)
		{
			SnChannel.StopPool("VO_Pool");
			Input.ResetInputAxes();
			GauntletMMOClient.pInstance.DestroyMMO(inLogout: true);
			SetVisibility(t: false);
			_GameManager._MultiplayerMenuScreen.SetVisibility(inVisible: true);
		}
	}

	public void SetLobbyMessage(string inText, int inTextID)
	{
		mTxtMessage.SetTextByID(inTextID, inText);
	}

	public void OnUserJoined(GauntletMMOPlayer inPlayer)
	{
		int markerIndex = GetMarkerIndex(inPlayer);
		if (markerIndex >= 0 && markerIndex < mPicture.Length)
		{
			inPlayer.SetItemData(mPicture[markerIndex], mName[markerIndex], mStatus[markerIndex]);
			inPlayer.UpdateStatus(_ReadyText, _NotReadyText);
		}
		else
		{
			Debug.LogError("Error : Index Out of range : index : " + markerIndex + ", Array length : " + mPicture.Length);
		}
	}

	public void ShowInviteBtn(bool isShow)
	{
		mBtnInvite.SetVisibility(isShow);
	}

	private int GetMarkerIndex(GauntletMMOPlayer inPlayer)
	{
		if (UserInfo.pInstance == null)
		{
			Debug.LogError("UserInfo.pInstance is null");
			return 0;
		}
		if (GauntletMMOClient.pInstance == null)
		{
			Debug.LogError("GauntletMMOClient.pInstance is null");
			return 0;
		}
		string userID = UserInfo.pInstance.UserID;
		if (inPlayer._UserID == userID)
		{
			return 0;
		}
		int num = 1;
		foreach (GauntletMMOPlayer pPlayer in GauntletMMOClient.pInstance.pPlayers)
		{
			if (inPlayer == pPlayer)
			{
				return num;
			}
			if (pPlayer._UserID != userID)
			{
				num++;
			}
		}
		return num;
	}

	public void UpdateReadyStatus(GauntletMMOPlayer inPlayer)
	{
		inPlayer.UpdateStatus(_ReadyText, _NotReadyText);
	}

	public void DisableInvite(bool isDisabled)
	{
		mBtnInvite.SetDisabled(isDisabled);
	}

	public void OnBuddyInvite(string[] inBuddies)
	{
		SetVisibility(t: true);
		SetState(KAUIState.INTERACTIVE);
		if (inBuddies != null && inBuddies.Length != 0)
		{
			foreach (string userID in inBuddies)
			{
				BuddyList.pInstance.InviteBuddy(userID, null);
			}
			mBtnInvite.SetDisabled(isDisabled: true);
			Invoke("EnableInviteBtn", 30f);
		}
	}

	private void EnableInviteBtn()
	{
		mBtnInvite.SetDisabled(isDisabled: false);
	}

	public void SetCourseSelected(int courseID)
	{
		mBtnTrainingLevel.SetChecked(courseID != 1);
		mBtnTrainingLevel.SetInteractive(courseID != 1);
		mBtnRedPlanetLevel.SetChecked(courseID != 2);
		mBtnRedPlanetLevel.SetInteractive(courseID != 2);
	}

	public void DisableCourseSelection(bool isDisabled)
	{
		mBtnTrainingLevel.SetDisabled(isDisabled);
		mBtnRedPlanetLevel.SetDisabled(isDisabled);
	}

	public void LockLobby()
	{
		SetState(KAUIState.DISABLED);
		_BuddySelectScreen.SetVisibility(t: false);
	}

	public void LockRedPlanetLevel(bool isRedPlanetLocked)
	{
		mIsRedPlanetLocked = isRedPlanetLocked;
		mIcoLock.SetVisibility(isRedPlanetLocked);
	}
}
