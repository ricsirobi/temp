using System;
using UnityEngine;

public class UiAdReward : KAUI
{
	private enum UiAdState
	{
		None,
		CoolDown,
		AdShowing,
		AdFetching,
		Available
	}

	public AdEventType _AdEventType = AdEventType.HUD_MYSTERY_CHEST;

	public AdType _AdType = AdType.REWARDED_VIDEO;

	public float _ToolTipTime = 5f;

	public float _FlashTime = 5f;

	public float _FlashInterval = 0.2f;

	public float _PulseEffectTime = 1f;

	public float _PulseInterval = 5f;

	public Vector3 _Scale = Vector3.one;

	public string _PlayfabAdLocation = "HudAds";

	private KAWidget mAdUiBtn;

	private KAWidget mCooldownTimer;

	private KAWidget mCooldownProgress;

	private KAWidget mGlowEffect;

	private float mCoolDowntime;

	private TimeSpan mCoolDownRemaintime = TimeSpan.MinValue;

	private DateTime mCoolDownEndtime = DateTime.MinValue;

	private float mTickInterval = 0.9f;

	private float mTimeCounter;

	private float mPercentRemain;

	private float mFlashCounter;

	private bool mShowFlashEffect;

	private bool mShowPulseEffect;

	private float mPulseCounter;

	private bool mCoolDownInitialized;

	private UiAdState mUiAdState;

	private float mShowAdWaitTimer = 15f;

	private bool mHideButton;

	protected override void Start()
	{
		base.Start();
		AdRewardManager.Init();
		mAdUiBtn = FindItem("HudMystery");
		if (mAdUiBtn != null)
		{
			mCooldownTimer = FindItem("TxtTimer");
			mCooldownProgress = FindItem("MeterBar");
			mGlowEffect = FindItem("GlowEffect");
		}
		else
		{
			UtDebug.LogError(" UI Ad btn is null ");
		}
		mUiAdState = UiAdState.None;
	}

	protected override void Update()
	{
		if (AdManager.pInstance.HideAdEventForMember(_AdEventType) || FUEManager.pIsFUERunning || mHideButton)
		{
			return;
		}
		if (!mCoolDownInitialized && AdRewardManager.mInstance != null)
		{
			if (AdManager.pInstance != null && AdManager.pInstance.AdSupported(_AdEventType, _AdType))
			{
				mCoolDowntime = AdManager.pInstance.CoolDownTime(_AdEventType);
				AdRewardManager.mInstance.RegisterEvent(SyncCoolDownTimer);
				SyncCoolDownTimer();
				mCoolDownInitialized = true;
			}
		}
		else if (mUiAdState == UiAdState.AdFetching)
		{
			mTimeCounter -= Time.deltaTime;
			if (AdRewardManager.mInstance != null)
			{
				mTimeCounter = 0f;
				ShowRewards();
			}
			else if (mTimeCounter < 0f)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				KAUI.RemoveExclusive(this);
				mUiAdState = UiAdState.Available;
			}
		}
		else if (mShowPulseEffect)
		{
			mPulseCounter += Time.deltaTime;
			if (mPulseCounter >= _PulseInterval)
			{
				mPulseCounter = 0f;
				ShowPulseEffect();
			}
		}
		if ((mUiAdState == UiAdState.Available || mUiAdState == UiAdState.CoolDown) && !GetVisibility() && AdManager.pInstance != null)
		{
			SetVisibility(AdManager.pInstance.AdAvailable(_AdEventType, _AdType, showErrorMessage: false));
		}
		UpdateCountDown();
	}

	private void ShowPulseEffect()
	{
		if (mGlowEffect != null)
		{
			mGlowEffect.SetVisibility(inVisible: true);
		}
		TweenScale tweenScale = TweenScale.Begin(mAdUiBtn.gameObject, _PulseEffectTime * 0.5f, _Scale);
		tweenScale.eventReceiver = base.gameObject;
		tweenScale.callWhenFinished = "AdButtonScaledUp";
	}

	private void AdButtonScaledUp()
	{
		TweenScale tweenScale = TweenScale.Begin(mAdUiBtn.gameObject, _PulseEffectTime * 0.5f, Vector3.one);
		tweenScale.eventReceiver = base.gameObject;
		tweenScale.callWhenFinished = "AdButtonNormal";
	}

	private void AdButtonNormal()
	{
		if (mGlowEffect != null)
		{
			mGlowEffect.SetVisibility(inVisible: false);
		}
		mAdUiBtn.gameObject.transform.localScale = Vector3.one;
	}

	private void UpdateCountDown()
	{
		if (mCoolDownRemaintime.TotalSeconds > 0.0)
		{
			if (mUiAdState != UiAdState.CoolDown)
			{
				mUiAdState = UiAdState.CoolDown;
			}
			mTimeCounter += Time.deltaTime;
			if (mTimeCounter >= mTickInterval)
			{
				mTimeCounter = 0f;
				mCoolDownRemaintime = mCoolDownEndtime.Subtract(ServerTime.pCurrentTime);
				mPercentRemain = (float)(mCoolDownRemaintime.TotalHours / (double)mCoolDowntime);
				if (mCoolDownRemaintime.TotalSeconds <= (double)_FlashTime)
				{
					mShowFlashEffect = true;
				}
			}
			UpdateCoolDownTimer();
		}
		else if (mShowFlashEffect)
		{
			mShowFlashEffect = false;
			mAdUiBtn.SetVisibility(inVisible: true);
		}
		UpdateCoolDownProgress();
		if (mShowFlashEffect)
		{
			mFlashCounter += Time.deltaTime;
			if (mFlashCounter > _FlashInterval)
			{
				mAdUiBtn.SetVisibility(!mAdUiBtn.GetVisibility());
				mFlashCounter = 0f;
			}
		}
	}

	private void UpdateCoolDownTimer()
	{
		if (mCooldownProgress != null && mCoolDownRemaintime.TotalSeconds > 0.0)
		{
			string text = $"{mCoolDownRemaintime.Hours.ToString():00}:{mCoolDownRemaintime.Minutes.ToString():00}:{mCoolDownRemaintime.Seconds.ToString():00}";
			mCooldownTimer.SetText(text);
		}
	}

	private void UpdateCoolDownProgress()
	{
		if (!(mCooldownProgress != null))
		{
			return;
		}
		if (mUiAdState == UiAdState.CoolDown)
		{
			if (mPercentRemain > 0f)
			{
				mCooldownProgress.SetVisibility(inVisible: true);
				mCooldownProgress.SetProgressLevel(mPercentRemain);
			}
			else
			{
				mUiAdState = UiAdState.Available;
			}
		}
		else if (mUiAdState == UiAdState.Available && !mShowPulseEffect)
		{
			mShowPulseEffect = true;
			mCooldownProgress.SetVisibility(inVisible: false);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (!(item == mAdUiBtn))
		{
			return;
		}
		if (AdManager.pInstance != null)
		{
			if (AdManager.pInstance.UnderCoolDown(_AdEventType))
			{
				if (mCooldownTimer != null && !mCooldownTimer.GetVisibility())
				{
					mCooldownTimer.SetVisibility(inVisible: true);
					Invoke("HideCooldownTimer", _ToolTipTime);
				}
			}
			else if (AdRewardManager.mInstance != null)
			{
				ShowRewards();
			}
			else
			{
				AdRewardManager.Init();
				mUiAdState = UiAdState.AdFetching;
				mTimeCounter = mShowAdWaitTimer;
				KAUICursorManager.SetDefaultCursor("Loading");
				KAUI.SetExclusive(this);
			}
		}
		else
		{
			UtDebug.LogError(" AdManager instance is null");
		}
	}

	private void ShowRewards()
	{
		mUiAdState = UiAdState.AdShowing;
		AdRewardManager.mInstance.ShowAdRewards(_AdEventType);
		mShowPulseEffect = false;
	}

	private void SyncCoolDownTimer()
	{
		if (AdManager.pInstance != null)
		{
			mCoolDownEndtime = AdManager.pInstance.CoolDownEndTime(_AdEventType);
			if (mCoolDownEndtime > ServerTime.pCurrentTime)
			{
				mUiAdState = UiAdState.CoolDown;
				mCoolDownRemaintime = mCoolDownEndtime.Subtract(ServerTime.pCurrentTime);
			}
			else
			{
				mUiAdState = UiAdState.Available;
				mCoolDownRemaintime = TimeSpan.Zero;
			}
			SetVisibility(AdManager.pInstance.AdAvailable(_AdEventType, _AdType, showErrorMessage: false));
		}
	}

	private void HideCooldownTimer()
	{
		if (mCooldownTimer != null)
		{
			mCooldownTimer.SetVisibility(inVisible: false);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (AdRewardManager.mInstance != null && AdManager.pInstance != null && AdManager.pInstance.AdSupported(_AdEventType, _AdType))
		{
			AdRewardManager.mInstance.UnRegisterEvent(SyncCoolDownTimer);
		}
	}

	public void HideButton(bool Hide)
	{
		mHideButton = Hide;
		if (Hide)
		{
			SetVisibility(inVisible: false);
		}
	}
}
