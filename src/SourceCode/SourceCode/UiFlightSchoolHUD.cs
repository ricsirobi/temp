using UnityEngine;

public class UiFlightSchoolHUD : KAUI
{
	public float _PowerMeterFillRate = 0.33f;

	public AudioClip _FlySound;

	public AudioClip _MeterFillSound;

	public TextAsset _TutorialTextAsset;

	public string _Intro = "tnFlightSchoolOC";

	public AudioClip _FireTargetHitSFX;

	public AudioClip _MeterFullSFX;

	public SnSettings _DlgSettings;

	public AudioClip[] _HelpDlgs;

	public ParticleSystem _PowerMeterParticle;

	public string _PowerShotWeapon;

	public float _DelaySecsAfterCutMeterFullRelease;

	public string _WingsOpeningAnim = "FlyCut";

	public float _WingsOpenAnimCrossFadeLength = 1f;

	private KAWidget mBackBtn;

	private KAWidget mHelpBtn;

	private KAWidget mNumPickupsTxt;

	private KAWidget mNumRingsTxt;

	private KAWidget mNumTargetablesTxt;

	private KAWidget mCounterTxt;

	private KAWidget mTimerTxt;

	private KAWidget mExtraTimerTxt;

	private KAWidget mTimerBG;

	private KAWidget mMeterBarPower;

	private KAWidget mMeterBarPowerBkg;

	private KAWidget mWrongWayBackground;

	private KAWidget mTxtWrongWay;

	private KAWidget mTimerNeedle;

	private float mTimerNeedleStartRot;

	private float mPowerMeterCurrVal;

	private bool mPowerMeterFillSndPlayed;

	private float mTimerAfterCutMeterFullRelease = -1f;

	private PetWeaponManager mWeaponsManager;

	protected GameObject mUiGenericDB;

	private SnChannel mChannel;

	private SnChannel mChannelFire;

	private SnChannel mChannelMeterFill;

	private int mCurrentHelpIdx;

	private ObstacleCourseGame mGame;

	public ObstacleCourseGame pGame
	{
		get
		{
			return mGame;
		}
		set
		{
			mGame = value;
		}
	}

	private void OnEnable()
	{
		mHelpBtn = FindItem("BtnHelp");
		mNumPickupsTxt = FindItem("BtnNumPickups");
		mNumRingsTxt = FindItem("BtnNumRings");
		mNumTargetablesTxt = FindItem("BtnNumTargetables");
		mCounterTxt = FindItem("BtnCounter");
		mTimerTxt = FindItem("BtnTimer");
		mTimerBG = FindItem("BtnTimer");
		mBackBtn = FindItem("BackBtn");
		mTimerNeedle = mTimerTxt.FindChildItem("AniNeedle");
		mWrongWayBackground = FindItem("BkgWrongWay");
		mTxtWrongWay = FindItem("TxtWrongWay");
		mMeterBarPower = FindItem("MeterBarPower");
		mMeterBarPowerBkg = FindItem("MeterBarPowerBkg");
		mExtraTimerTxt = FindItem("TxtExtraTime");
		StopTimer();
		mTimerNeedleStartRot = mTimerNeedle._RotateSpeed;
		mChannel = SnChannel.Play(_FlySound, "FlySnd_Pool", inForce: true);
		if (mChannel != null)
		{
			mChannel.pLoop = true;
		}
		if (_TutorialTextAsset != null)
		{
			TutorialManager.StartTutorial(_TutorialTextAsset, _Intro, bMarkDone: false, 0u, null);
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			mWeaponsManager = SanctuaryManager.pCurPetInstance.gameObject.GetComponentInChildren<PetWeaponManager>();
			if (mWeaponsManager != null && IsPowerMeterEnabled())
			{
				mWeaponsManager.pFiredMessageObject = base.gameObject;
				ResetToNoPower();
			}
		}
	}

	private void OnDisable()
	{
		if (mChannel != null)
		{
			mChannel.Stop();
		}
		if (mChannelFire != null)
		{
			mChannelFire.Stop();
		}
		if (mChannelMeterFill != null)
		{
			mChannelMeterFill.Stop();
		}
		TutorialManager.StopTutorials();
		if (mWeaponsManager != null && IsPowerMeterEnabled())
		{
			mWeaponsManager.pFiredMessageObject = null;
			ResetToNoPower();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (AvAvatar.pInputEnabled && UiAvatarControls.pInstance != null && UiAvatarControls.pInstance.pFireBtnReady && KAInput.GetButton("DragonFire") && IsPowerMeterEnabled())
		{
			UpdatePowerMeter();
		}
		else if (KAInput.GetButtonUp("DragonFire") && IsCutEnabledForMeterFull())
		{
			mTimerAfterCutMeterFullRelease = _DelaySecsAfterCutMeterFullRelease;
		}
		if (!(mTimerAfterCutMeterFullRelease > -1f))
		{
			return;
		}
		mTimerAfterCutMeterFullRelease -= Time.deltaTime;
		if (mTimerAfterCutMeterFullRelease <= 0f)
		{
			ResetToNoPower();
			mTimerAfterCutMeterFullRelease = -1f;
			if (IsCutEnabledForMeterFull())
			{
				UiAvatarControls.pInstance.pEnableFireOnButtonUp = true;
			}
		}
	}

	private void UpdatePowerMeter()
	{
		if (mPowerMeterCurrVal < 1f)
		{
			if (!mPowerMeterFillSndPlayed)
			{
				if (_MeterFillSound != null)
				{
					mChannelMeterFill = SnChannel.Play(_MeterFillSound, "SFX_Pool", inForce: true, null);
				}
				PlayWingsOpeningAnim(inPlay: true, _WingsOpenAnimCrossFadeLength);
				mPowerMeterFillSndPlayed = true;
				mBackBtn.SetDisabled(isDisabled: true);
			}
			mPowerMeterCurrVal += Time.deltaTime * _PowerMeterFillRate;
			if (mPowerMeterCurrVal >= 1f)
			{
				mPowerMeterCurrVal = 1f;
				if (IsCutEnabledForMeterFull())
				{
					UiAvatarControls.pInstance.pEnableFireOnButtonUp = false;
				}
			}
			SetPowerMeterVal(mPowerMeterCurrVal);
		}
		if (IsCutEnabledForMeterFull())
		{
			KAInput.pInstance.EnableInputType("WingFlap", InputType.ALL, inEnable: false);
		}
	}

	public override void OnClick(KAWidget item)
	{
		if (item == mBackBtn)
		{
			if (pGame != null)
			{
				pGame.Quit();
			}
		}
		else if (item == mHelpBtn)
		{
			PlayHelpDlg();
		}
	}

	public void StopTimer()
	{
		PauseTimer();
		ResetTimer();
	}

	public void ResetTimer()
	{
	}

	public void PauseTimer()
	{
	}

	public void StartTimer(float timeSeconds)
	{
	}

	public void UpdateCounterDispay(string collected)
	{
		mCounterTxt.SetText(collected);
	}

	public void UpdateCollectablesCount(string inNumPickupsCollectedStr, bool isShowNumPickups, string inNumRingsCollectedStr, bool isShowNumRings, string inNumTargetablesCollectedStr, bool isShowNumTargetables)
	{
		mNumPickupsTxt.SetText(inNumPickupsCollectedStr);
		mNumPickupsTxt.SetVisibility(isShowNumPickups);
		mNumRingsTxt.SetText(inNumRingsCollectedStr);
		mNumRingsTxt.SetVisibility(isShowNumRings);
		mNumTargetablesTxt.SetText(inNumTargetablesCollectedStr);
		mNumTargetablesTxt.SetVisibility(isShowNumTargetables);
	}

	public void UpdateTimeDisplay(float timeLeft)
	{
		mTimerTxt.SetText(timeLeft.ToString("#.00"));
	}

	public void Collect(GameObject collectedGO)
	{
		if (_FireTargetHitSFX != null && collectedGO.GetComponent<OCTarget>() != null)
		{
			SnChannel.Play(_FireTargetHitSFX, "SFX_Pool", inForce: true, null);
		}
		if ((bool)pGame && !pGame._TimeTrial)
		{
			pGame.OnObjectCollected(collectedGO);
		}
	}

	public void HitObject(GameObject inObject)
	{
		if (_FireTargetHitSFX != null)
		{
			SnChannel.Play(_FireTargetHitSFX, "SFX_Pool", inForce: true, null);
		}
		if ((bool)pGame && !pGame._TimeTrial)
		{
			pGame.OnObjectCollected(inObject);
		}
	}

	public void ShowWrongWayText(string inText)
	{
		mTxtWrongWay.SetText(inText);
		mTxtWrongWay.SetVisibility(!string.IsNullOrEmpty(inText));
		mWrongWayBackground.SetVisibility(!string.IsNullOrEmpty(inText));
	}

	public void HideButtons()
	{
		mHelpBtn.SetVisibility(inVisible: false);
		mNumPickupsTxt.SetVisibility(inVisible: false);
		mNumRingsTxt.SetVisibility(inVisible: false);
		mNumTargetablesTxt.SetVisibility(inVisible: false);
		mCounterTxt.SetVisibility(inVisible: false);
		mTimerBG.SetVisibility(inVisible: false);
		mBackBtn.SetVisibility(inVisible: false);
		if (mMeterBarPowerBkg != null)
		{
			mMeterBarPowerBkg.SetVisibility(inVisible: false);
		}
		if (mMeterBarPower != null)
		{
			mMeterBarPower.SetVisibility(inVisible: false);
		}
		KAInput.pInstance.EnableInputType("WingFlap", InputType.UI_BUTTONS, inEnable: false);
		KAInput.pInstance.EnableInputType("DragonBrake", InputType.UI_BUTTONS, inEnable: false);
		if (SanctuaryManager.pCurPetInstance != null)
		{
			KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, SanctuaryManager.pCurPetInstance.IsMountAllowed());
		}
	}

	public void ShowButtons(bool bCollection)
	{
		KAInput.pInstance.EnableInputType("DragonMount", InputType.UI_BUTTONS, inEnable: false);
		if (mNumPickupsTxt != null)
		{
			mNumPickupsTxt.SetVisibility(inVisible: false);
		}
		if (mNumRingsTxt != null)
		{
			mNumRingsTxt.SetVisibility(inVisible: false);
		}
		if (mNumTargetablesTxt != null)
		{
			mNumTargetablesTxt.SetVisibility(inVisible: false);
		}
		if (mCounterTxt != null)
		{
			mCounterTxt.SetVisibility(bCollection);
		}
		if (mTimerBG != null)
		{
			mTimerBG.SetVisibility(inVisible: true);
		}
		if (mBackBtn != null)
		{
			mBackBtn.SetVisibility(inVisible: true);
		}
		if (mMeterBarPowerBkg != null)
		{
			mMeterBarPowerBkg.SetVisibility(inVisible: true);
		}
		if (mMeterBarPower != null)
		{
			mMeterBarPower.SetVisibility(inVisible: true);
			mMeterBarPower.SetProgressLevel(0f);
		}
	}

	public void SetPowerMeterVal(float fillPercentage)
	{
		if (mMeterBarPower != null)
		{
			mMeterBarPower.SetProgressLevel(fillPercentage);
		}
		if (fillPercentage >= 1f)
		{
			if (mMeterBarPowerBkg != null && mMeterBarPowerBkg.GetCurrentAnim() != "FlashRed")
			{
				mMeterBarPowerBkg.PlayAnim("FlashRed");
			}
			if (_PowerMeterParticle != null)
			{
				_PowerMeterParticle.Play();
			}
			if (IsPowerShotEnabled())
			{
				mWeaponsManager.PlayDragonOnFireParticle(isPlay: true);
				mWeaponsManager.SetDragonOnFire(1f);
				if (!string.IsNullOrEmpty(_PowerShotWeapon))
				{
					mWeaponsManager._PowerUpWeapon = _PowerShotWeapon;
				}
			}
			if (_MeterFullSFX != null)
			{
				SnChannel.Play(_MeterFullSFX, "SFX_Pool", inForce: true, null);
			}
		}
		else
		{
			if (mMeterBarPowerBkg != null && mMeterBarPowerBkg.GetCurrentAnim() != "Normal")
			{
				mMeterBarPowerBkg.PlayAnim("Normal");
			}
			if (_PowerMeterParticle != null)
			{
				_PowerMeterParticle.Stop();
			}
			if (IsPowerShotEnabled())
			{
				mWeaponsManager.PlayDragonOnFireParticle(isPlay: false);
				mWeaponsManager.SetDragonOnFire(fillPercentage / 2f);
				mWeaponsManager._PowerUpWeapon = null;
			}
		}
	}

	public void PlayHelpDlg()
	{
		if (_HelpDlgs.Length != 0)
		{
			if (mCurrentHelpIdx >= _HelpDlgs.Length)
			{
				mCurrentHelpIdx = 0;
			}
			SnChannel.Play(_HelpDlgs[mCurrentHelpIdx], _DlgSettings, inForce: true);
			mCurrentHelpIdx++;
		}
	}

	public void OnWeaponFired()
	{
		ResetToNoPower();
	}

	private void ResetToNoPower()
	{
		if (SanctuaryManager.pCurPetInstance != null && mPowerMeterCurrVal > 0f)
		{
			PlayWingsOpeningAnim(inPlay: false, 0.1f);
		}
		if (mChannelMeterFill != null)
		{
			mChannelMeterFill.Stop();
		}
		mPowerMeterFillSndPlayed = false;
		mPowerMeterCurrVal = 0f;
		SetPowerMeterVal(mPowerMeterCurrVal);
		if (IsCutEnabledForMeterFull())
		{
			UiAvatarControls.pInstance.pEnableFireOnButtonUp = true;
			KAInput.pInstance.EnableInputType("WingFlap", InputType.ALL, inEnable: true);
		}
		mBackBtn.SetDisabled(isDisabled: false);
	}

	private bool IsPowerMeterEnabled()
	{
		if (mGame != null && mGame._PowerMeterFullAction != 0)
		{
			return mTimerAfterCutMeterFullRelease == -1f;
		}
		return false;
	}

	private bool IsPowerShotEnabled()
	{
		if (mGame != null)
		{
			return mGame._PowerMeterFullAction == ObstacleCourseGame.PowerMeterFullAction.POWERSHOT_AFTER_METERFULL;
		}
		return false;
	}

	private bool IsCutEnabledForMeterFull()
	{
		if (mGame != null)
		{
			return mGame._PowerMeterFullAction == ObstacleCourseGame.PowerMeterFullAction.CUT_ENABLE_AFTER_METERFULL;
		}
		return false;
	}

	public void PlayWingsOpeningAnim(bool inPlay, float inCrossFadeLength)
	{
		Animation animation = SanctuaryManager.pCurPetInstance.animation;
		string text = _WingsOpeningAnim;
		AnimationState animationState = animation[text];
		if (animationState == null)
		{
			text = "Dive";
			animationState = animation[text];
		}
		if (animationState != null)
		{
			if (inPlay)
			{
				SanctuaryManager.pCurPetInstance.pEnablePetAnim = false;
				animationState.wrapMode = WrapMode.ClampForever;
				animationState.enabled = true;
				animation.CrossFade(text, inCrossFadeLength, PlayMode.StopAll);
			}
			else
			{
				animationState.wrapMode = WrapMode.Loop;
				animation.CrossFade("FlyIdle", inCrossFadeLength, PlayMode.StopAll);
				Invoke("AfterWingAnimation", inCrossFadeLength);
			}
		}
	}

	private void AfterWingAnimation()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.pEnablePetAnim = true;
		}
	}

	public void ExtraTimeAlert(bool play, float time)
	{
		if (play && mTimerTxt.pAnim2D != null)
		{
			if (!mTimerTxt.pAnim2D.mIsPlaying)
			{
				mTimerTxt.PlayAnim("Flash");
				mExtraTimerTxt.SetVisibility(inVisible: true);
			}
			mExtraTimerTxt.SetText(time.ToString("0.00"));
			mTimerNeedle.SetRotateSpeed(0f);
		}
		else
		{
			mTimerTxt.StopAnim("Flash");
			mExtraTimerTxt.SetVisibility(inVisible: false);
			mTimerNeedle.SetRotateSpeed(mTimerNeedleStartRot);
		}
	}
}
