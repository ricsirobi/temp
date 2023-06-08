using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObTriggerDragonCheck : ObTrigger
{
	[Serializable]
	public class BlockerData
	{
		public List<int> _AllowedPetTypes;

		public bool _RequireMountedState;

		public GameObject _BlockEntryPrefab;

		public LocaleString _BlockedText = new LocaleString("You need a different pet to enter this area.");
	}

	[Serializable]
	public class BlockedStages
	{
		public RaisedPetStage _PetStage;

		public AudioClip _SpecificVO;

		public LocaleString _StageText = new LocaleString("");
	}

	public bool _AllowFlightlessDragon = true;

	public LocaleString _FlightlessPetBlockedText = new LocaleString("This Dragon cannot fly, please make another dragon your Active dragon to participate.");

	public bool _AllowUnmountablePet = true;

	public LocaleString _UnmountablePetBlockedText = new LocaleString("Your Dragon need to be older to mount, please make another dragon your Active dragon to participate.");

	public LocaleString _NoPetText;

	public BlockedStages[] _BlockedStages;

	public AudioClip _DefaultBlockedVO;

	public AudioClip _NoMythieVO;

	public AudioClip _MythieSleepingVO;

	public AudioClip _HideAndSeekWarning;

	public bool _CanEnterIfNoRaisedPet;

	public bool _CanEnterIfPetSleeping;

	public BlockedStages[] _MythieSleepingVOs;

	public AudioClip _DefaultMythieSleepingVO;

	public float _MinEnergyValue;

	public float _MinHappinessValue;

	public AudioClip _TooTiredVO;

	public LocaleString _TooTiredText = new LocaleString("Your pet is too angry/tired to shoot.");

	public string _AgeUpPromptTrigger;

	public BlockerData _BlockerData;

	public bool _AlertUser = true;

	public string _AgeUpAssetName = "";

	protected KAUIGenericDB mGenericDBUi;

	protected Collider mTriggeredCollider;

	private bool mSkipAgeUpPrompt;

	private AudioClip mCurrentClip;

	private AvAvatarState mPrevAvatarState;

	private bool mIsAgeUpTriggered;

	private bool mIsBlockedStagePrompt;

	public override void OnTriggerEnter(Collider inCollider)
	{
		if (inCollider != null)
		{
			mTriggeredCollider = inCollider;
			if (inCollider == null || !AvAvatar.IsCurrentPlayer(inCollider.gameObject) || mInTrigger)
			{
				return;
			}
		}
		if ((_UseGlobalActive && !ObClickable.pGlobalActive) || !_Active || RsResourceManager.pLevelLoadingScreen)
		{
			return;
		}
		if (_BlockerData != null && _BlockerData._BlockEntryPrefab != null && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pTypeInfo != null)
		{
			if (!_BlockerData._AllowedPetTypes.Contains(SanctuaryManager.pCurPetInstance.pTypeInfo._TypeID) || (_BlockerData._RequireMountedState && !SanctuaryManager.pCurPetInstance.pIsMounted))
			{
				_BlockerData._BlockEntryPrefab.SetActive(value: true);
				ShowDialog(_BlockerData._BlockedText.GetLocalizedString());
				return;
			}
			_BlockerData._BlockEntryPrefab.SetActive(value: false);
		}
		if (!_AllowFlightlessDragon && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pTypeInfo != null && SanctuaryManager.pCurPetInstance.pTypeInfo._Flightless)
		{
			ShowDialog(_FlightlessPetBlockedText.GetLocalizedString());
			return;
		}
		RaisedPetData currentInstance = RaisedPetData.GetCurrentInstance(SanctuaryManager.pCurrentPetType);
		if (currentInstance != null && !mIsBlockedStagePrompt)
		{
			if (_AlertUser)
			{
				if (_BlockedStages != null)
				{
					BlockedStages[] blockedStages = _BlockedStages;
					foreach (BlockedStages blockedStages2 in blockedStages)
					{
						if (blockedStages2._PetStage == currentInstance.pStage)
						{
							mCurrentClip = null;
							if (blockedStages2._SpecificVO != null)
							{
								mCurrentClip = blockedStages2._SpecificVO;
							}
							else if (_DefaultBlockedVO != null)
							{
								mCurrentClip = _DefaultBlockedVO;
							}
							else
							{
								mSkipAgeUpPrompt = false;
								ShowDialog(blockedStages2._StageText.GetLocalizedString());
							}
							if (mCurrentClip != null)
							{
								SnChannel.Play(mCurrentClip, "VO_Pool", 0, inForce: true);
							}
							mIsBlockedStagePrompt = true;
							return;
						}
					}
				}
				if (!IsUnMountableStateAllowed() && !mIsBlockedStagePrompt)
				{
					mSkipAgeUpPrompt = false;
					mIsBlockedStagePrompt = true;
					ShowDialog(_UnmountablePetBlockedText.GetLocalizedString());
					return;
				}
			}
			if (!mSkipAgeUpPrompt || !_AlertUser)
			{
				bool isUnmountableAllowed = ((_BlockedStages.Length == 0) ? IsUnMountableStateAllowed() : (IsUnMountableStateAllowed() && SanctuaryManager.pCurPetInstance.pData.pStage > _BlockedStages.Max((BlockedStages ele) => ele._PetStage)));
				if (!mIsAgeUpTriggered && DragonAgeUpConfig.Trigger(_AgeUpPromptTrigger, OnDragonAgeUpDone, null, RaisedPetStage.NONE, isUnmountableAllowed, _AlertUser, base.gameObject, _AgeUpAssetName))
				{
					mIsAgeUpTriggered = true;
					MakeUIInactiveAndPauseAvatar();
					return;
				}
			}
			else
			{
				mSkipAgeUpPrompt = false;
			}
			if (currentInstance.pIsSleeping && !_CanEnterIfPetSleeping)
			{
				mCurrentClip = _DefaultMythieSleepingVO;
				if (mCurrentClip == null)
				{
					mCurrentClip = _NoMythieVO;
				}
				BlockedStages[] blockedStages = _MythieSleepingVOs;
				foreach (BlockedStages blockedStages3 in blockedStages)
				{
					if (blockedStages3._PetStage == currentInstance.pStage)
					{
						mCurrentClip = blockedStages3._SpecificVO;
					}
				}
				SnChannel.Play(mCurrentClip, "VO_Pool", 0, inForce: true);
				return;
			}
			if (SanctuaryManager.pCurPetInstance != null)
			{
				if (SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HAPPINESS) < Mathf.Abs(_MinHappinessValue))
				{
					ShowPetTiredDialog(isLowEnergy: false);
					return;
				}
				if (SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.ENERGY) < Mathf.Abs(_MinEnergyValue))
				{
					ShowPetTiredDialog(isLowEnergy: true);
					return;
				}
			}
		}
		else if (CanHandleNoRaisedPet())
		{
			if (!mIsBlockedStagePrompt && _AlertUser)
			{
				ShowDialog(_NoPetText.GetLocalizedString());
			}
			return;
		}
		base.OnTriggerEnter(inCollider);
	}

	protected virtual bool IsUnMountableStateAllowed()
	{
		if (!_AllowUnmountablePet && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pAge < SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToMount)
		{
			return false;
		}
		return true;
	}

	protected virtual bool CanHandleNoRaisedPet()
	{
		return !_CanEnterIfNoRaisedPet;
	}

	protected virtual bool IsPetTooTired()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			if (!(SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.ENERGY) < _MinEnergyValue))
			{
				return SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HAPPINESS) < _MinHappinessValue;
			}
			return true;
		}
		return false;
	}

	protected virtual void ShowDialog(string inText)
	{
		if (!string.IsNullOrEmpty(inText))
		{
			if (mUiGenericDB != null)
			{
				UnityEngine.Object.Destroy(mUiGenericDB);
			}
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
			mGenericDBUi = (KAUIGenericDB)gameObject.GetComponent("KAUIGenericDB");
			mGenericDBUi.OnMessageReceived += OnShowDialogMessageReceived;
			mGenericDBUi.SetText(inText, interactive: false);
			mGenericDBUi.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mGenericDBUi._OKMessage = (mSkipAgeUpPrompt ? "OkMessage" : "CloseAndShowAgeUpPrompt");
		}
	}

	protected virtual void OnShowDialogMessageReceived(string message)
	{
		mGenericDBUi.OnMessageReceived -= OnShowDialogMessageReceived;
		if (!(message == "OkMessage"))
		{
			if (message == "CloseAndShowAgeUpPrompt")
			{
				CloseAndShowAgeUpPrompt();
			}
		}
		else
		{
			OkMessage();
		}
	}

	protected virtual void ShowPetTiredDialog(bool isLowEnergy)
	{
		if (_TooTiredVO != null)
		{
			mCurrentClip = _TooTiredVO;
			SnChannel.Play(mCurrentClip, "VO_Pool", 0, inForce: true);
		}
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		UiPetEnergyGenericDB.Show(base.gameObject, mSkipAgeUpPrompt ? "OkMessage" : "CloseAndShowAgeUpPrompt", mSkipAgeUpPrompt ? "OkMessage" : "CloseAndShowAgeUpPrompt", isLowEnergy);
	}

	private void OnCloseDialog()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		if (UiAvatarControls.pInstance != null && !UiAvatarControls.pInstance.GetVisibility())
		{
			UiAvatarControls.pInstance.SetVisibility(inVisible: true);
		}
		if (mGenericDBUi != null)
		{
			UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
		}
	}

	protected virtual void CloseAndShowAgeUpPrompt()
	{
		OnCloseDialog();
		if (DragonAgeUpConfig.Trigger(_AgeUpPromptTrigger, OnDragonAgeUpDone, SanctuaryManager.pCurPetData, RaisedPetStage.NONE, _AllowUnmountablePet))
		{
			MakeUIInactiveAndPauseAvatar();
		}
		else
		{
			mIsBlockedStagePrompt = false;
		}
	}

	protected virtual void OkMessage()
	{
		OnCloseDialog();
	}

	protected override bool CheckMemberStatus()
	{
		if (SanctuaryManager.pCurPetData != null && SanctuaryData.FindSanctuaryPetTypeInfo(SanctuaryManager.pCurPetData.PetTypeID)._AllowAccessToMemberOnlyFeatures)
		{
			return true;
		}
		return base.CheckMemberStatus();
	}

	protected void MakeUIInactiveAndPauseAvatar()
	{
		if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
		{
			mPrevAvatarState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		AvAvatar.SetUIActive(inActive: false);
	}

	protected virtual void OnDragonAgeUpDone()
	{
		ResetData();
		if (_AlertUser)
		{
			OnTriggerEnter(mTriggeredCollider);
		}
		mIsBlockedStagePrompt = false;
	}

	protected virtual void OnDragonCustomizationDone()
	{
		ResetData();
		OnTriggerEnter(mTriggeredCollider);
		mIsBlockedStagePrompt = false;
	}

	protected void ResetData()
	{
		if (AvAvatar.pState == AvAvatarState.PAUSED)
		{
			AvAvatar.pState = mPrevAvatarState;
		}
		mIsAgeUpTriggered = false;
		AvAvatar.SetUIActive(inActive: true);
		mSkipAgeUpPrompt = true;
	}
}
