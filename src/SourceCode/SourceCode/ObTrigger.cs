using System;
using System.Globalization;
using UnityEngine;

public class ObTrigger : KAMonoBase
{
	public bool _Active = true;

	public bool _UseGlobalActive = true;

	public bool _StopTutorialOnLoadLevel;

	public string _LoadLevel = "";

	public string _StartMarker = "";

	public GameObject _ActivateObject;

	public bool _Activate = true;

	public bool _DeactivateOnExit;

	public bool _ForMembersOnly = true;

	public bool _NoRides;

	public bool _Confirm;

	public AudioClip _NonMemberVO;

	public AudioClip _RankLockedVO;

	public AudioClip _NoRidesVO;

	public TriggerStrings _Strings;

	public int _PairDataID;

	public string _LastPlayedDataPairKey;

	public int _GameUnlockRank;

	public int _NumEntriesToLockedLevel;

	public bool _CanSaveLastPlayedDate = true;

	public bool _ForceDismountBeforeExit;

	public TextAsset _TutorialTextAsset;

	public string _LongIntro;

	public string _ShortIntro;

	public AudioClip _ConfirmVO;

	public Transform _AvatarSafeSpot;

	private bool mTutorialPlayed;

	private bool mIsInitialized;

	protected GameObject mUiGenericDB;

	protected bool mInTrigger;

	protected PairData mLastPlayedData;

	protected int mNumAllowedEntries;

	protected KAUIGenericDB uiGenericDB;

	public virtual void Update()
	{
		if (_LastPlayedDataPairKey.Length > 0 && !mIsInitialized && UserInfo.pIsReady)
		{
			mIsInitialized = true;
			PairData.Load(_PairDataID, OnPairDataReady, null);
		}
	}

	public void OnPairDataReady(bool success, PairData pData, object inUserData)
	{
		mLastPlayedData = pData;
	}

	public void DecrementNumAllowedEntries()
	{
		mNumAllowedEntries--;
		SavePlayedDateData();
	}

	public void SavePlayedDateData()
	{
		if (ServerTime.pIsReady && _LastPlayedDataPairKey.Length > 0)
		{
			mLastPlayedData.SetValue(_LastPlayedDataPairKey, ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US")));
			mLastPlayedData.SetValue(_LastPlayedDataPairKey + "_ENTRY_COUNT", mNumAllowedEntries.ToString());
			PairData.Save(_PairDataID);
		}
	}

	public virtual void DoTriggerAction(GameObject other)
	{
		if (_ForceDismountBeforeExit && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		}
		AvAvatarController componentInChildren = AvAvatar.pObject.GetComponentInChildren<AvAvatarController>();
		if (componentInChildren != null && componentInChildren.pIsPlayerGliding)
		{
			componentInChildren.OnGlideLanding();
		}
		if (MainStreetMMOClient.pInstance != null && _ForMembersOnly)
		{
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.MEMBERS_ONLY);
		}
		if (_LoadLevel.Length > 0)
		{
			if (UiAvatarControls.pInstance != null)
			{
				UiAvatarControls.pInstance.StopDrag();
			}
			AvAvatar.SetActive(inActive: false);
			if (_StartMarker != "")
			{
				AvAvatar.pStartLocation = _StartMarker;
			}
			if (_StopTutorialOnLoadLevel)
			{
				TutorialManager.StopTutorials();
			}
			RsResourceManager.LoadLevel(_LoadLevel);
			return;
		}
		if (_ActivateObject != null)
		{
			_ActivateObject.SetActive(_Activate);
		}
		if (!mTutorialPlayed)
		{
			mTutorialPlayed = true;
			if (_TutorialTextAsset != null && !TutorialManager.StartTutorial(_TutorialTextAsset, _LongIntro, bMarkDone: true, 268435468u, null))
			{
				TutorialManager.StartTutorial(_TutorialTextAsset, _ShortIntro, bMarkDone: false, 268435468u, null);
			}
		}
		SendMessage("OnTrigger", null, SendMessageOptions.DontRequireReceiver);
	}

	public virtual void DoTriggerExit()
	{
		if (_ForMembersOnly)
		{
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED);
		}
		if (_DeactivateOnExit && _ActivateObject != null)
		{
			_ActivateObject.SetActive(!_Activate);
		}
	}

	public void OnConfirmDBYes()
	{
		Input.ResetInputAxes();
		UnityEngine.Object.Destroy(mUiGenericDB);
		SnChannel.StopPool("VO_Pool");
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		DoTriggerAction(null);
	}

	protected virtual void ConfirmTrigger()
	{
		mUiGenericDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDBSm"));
		mUiGenericDB.name = "TriggerConfirm";
		uiGenericDB = mUiGenericDB.GetComponent<KAUIGenericDB>();
		uiGenericDB._MessageObject = base.gameObject;
		uiGenericDB._YesMessage = "OnConfirmDBYes";
		uiGenericDB._NoMessage = "CloseDialogBox";
		uiGenericDB.SetTextByID(_Strings._ConfirmText._ID, _Strings._ConfirmText._Text, interactive: false);
		uiGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		if (_ConfirmVO != null)
		{
			SnChannel.Play(_ConfirmVO, "VO_Pool", inForce: true);
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
	}

	public virtual void OnTriggerEnter(Collider inCollider)
	{
		if ((inCollider != null && (mInTrigger || (_UseGlobalActive && !ObClickable.pGlobalActive) || !_Active || !AvAvatar.IsCurrentPlayer(inCollider.gameObject))) || (_LoadLevel.Length > 0 && !UnlockManager.IsSceneUnlocked(_LoadLevel, inShowUi: false, delegate(bool success)
		{
			if (success)
			{
				OnTriggerEnter(inCollider);
			}
		})))
		{
			return;
		}
		mInTrigger = true;
		if (!CheckMemberStatus() || !CheckRideStatus())
		{
			return;
		}
		if (_Confirm)
		{
			ConfirmTrigger();
			return;
		}
		LocaleString text = null;
		if (!EquipmentCheck.pInstance.CheckEquippedItemsCriteria(_LoadLevel, ref text))
		{
			if (_AvatarSafeSpot != null)
			{
				AvAvatar.position = _AvatarSafeSpot.position;
			}
			DisplayTextBox(text._ID, text._Text);
		}
		else
		{
			DoTriggerAction(inCollider ? inCollider.gameObject : null);
		}
	}

	public virtual void OnTriggerExit(Collider inCollider)
	{
		if (mInTrigger && AvAvatar.IsCurrentPlayer(inCollider.gameObject))
		{
			mInTrigger = false;
			DoTriggerExit();
		}
	}

	protected virtual bool CheckMemberStatus()
	{
		if (!_ForMembersOnly)
		{
			return true;
		}
		if (mLastPlayedData != null && ServerTime.pIsReady && UserRankData.pIsReady && _LastPlayedDataPairKey.Length > 0)
		{
			bool pIsMember = SubscriptionInfo.pIsMember;
			if (pIsMember && UserRankData.pInstance.RankID >= _GameUnlockRank)
			{
				return true;
			}
			string stringValue = mLastPlayedData.GetStringValue(_LastPlayedDataPairKey, "");
			mNumAllowedEntries = mLastPlayedData.GetIntValue(_LastPlayedDataPairKey + "_ENTRY_COUNT", _NumEntriesToLockedLevel);
			DateTime result = ServerTime.pCurrentTime;
			DateTime dateTime = ServerTime.pCurrentTime.ToLocalTime();
			bool flag = false;
			if (DateTime.TryParse(stringValue, UtUtilities.GetCultureInfo("en-US"), DateTimeStyles.None, out result))
			{
				result = result.ToLocalTime();
				if (dateTime.Date <= result.Date)
				{
					if (mNumAllowedEntries <= 0)
					{
						flag = true;
					}
				}
				else
				{
					mNumAllowedEntries = _NumEntriesToLockedLevel;
				}
			}
			if (!flag)
			{
				if (_CanSaveLastPlayedDate)
				{
					DecrementNumAllowedEntries();
				}
				else
				{
					mLastPlayedData.SetValueAndSave(_LastPlayedDataPairKey + "_ENTRY_COUNT", mNumAllowedEntries.ToString());
				}
				return true;
			}
			AudioClip audioClip = null;
			LocaleString localeString;
			if (pIsMember)
			{
				audioClip = _RankLockedVO;
				localeString = _Strings._RankLockedText;
			}
			else
			{
				audioClip = _NonMemberVO;
				localeString = _Strings._MembersOnlyText;
			}
			if (!string.IsNullOrEmpty(localeString._Text))
			{
				DisplayTextBox(localeString._ID, localeString._Text);
			}
			if (audioClip != null)
			{
				SnChannel.Play(audioClip, "VO_Pool", inForce: true);
			}
		}
		else
		{
			if (SubscriptionInfo.pIsMember)
			{
				return true;
			}
			if (!string.IsNullOrEmpty(_Strings._MembersOnlyText._Text))
			{
				DisplayTextBox(_Strings._MembersOnlyText._ID, _Strings._MembersOnlyText._Text);
			}
			if (_NonMemberVO != null)
			{
				SnChannel.Play(_NonMemberVO, "VO_Pool", inForce: true);
			}
		}
		return false;
	}

	protected virtual bool CheckRideStatus()
	{
		if (!_NoRides || !AvAvatar.IsPlayerOnRide())
		{
			return true;
		}
		if (!string.IsNullOrEmpty(_Strings._NoRideText._Text))
		{
			DisplayTextBox(_Strings._NoRideText._ID, _Strings._NoRideText._Text);
		}
		if (_NoRidesVO != null)
		{
			SnChannel.Play(_NoRidesVO, "VO_Pool", inForce: true);
		}
		return false;
	}

	protected virtual void DisplayTextBox(int msgid, string msgtxt)
	{
		mUiGenericDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
		component._MessageObject = base.gameObject;
		component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		component.SetTextByID(msgid, msgtxt, interactive: false);
		component._OKMessage = "CloseDialogBox";
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
	}

	protected virtual void CloseDialogBox()
	{
		if (mUiGenericDB != null)
		{
			UnityEngine.Object.Destroy(mUiGenericDB);
		}
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}
}
