using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiHUDDreadFall : KAUI
{
	private KAWidget mProgressBar;

	private KAWidget mHUDDreadfall;

	private bool mShowHUDButton;

	private KAWidget mTimer;

	private KAWidget mIcoDreadfall;

	private UserAchievementTaskRedeemableRewards mRedeemRewards;

	private UserAchievementTask mAchievementTask;

	private bool mAchTaskReady;

	private bool mRedeemAchReady;

	private bool mHUDBtnProcessed;

	private bool mRedeemReady;

	[Header("Effects")]
	public float _HudButtonSheenDelay = 5f;

	public float _HudButtonSheenSpeed = 2f;

	public float _HudButtonSheenDuration = 1f;

	public float _HudBarFillSpeed = 2f;

	public Material _HudButtonIdle;

	public Material _HudButtonHighlight;

	public ParticleSystem _CandyParticleEffect;

	private UserItemData mUserItemData;

	private Coroutine mHudButtonCouroutine;

	private float mCurBarValue;

	private bool mRewardRedeemed;

	protected override void Start()
	{
		if (DreadfallAchievementManager.pInstance == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.Start();
		mProgressBar = FindItem("AchProgressBar");
		mHUDDreadfall = FindItem("BtnHUDDreadfall");
		mTimer = FindItem("TxtEventTimer");
		mAchTaskReady = false;
		mRedeemAchReady = false;
		mHUDBtnProcessed = false;
		mRedeemReady = false;
		mIcoDreadfall = FindItem("IcoDreadfall");
		InitAchievementData();
		mCurBarValue = DreadfallAchievementManager.pInstance.CachedBarValue;
		mProgressBar.SetProgressLevel(mCurBarValue);
		UiDreadfallRedeem.RedeemFinished += RedeemFinished;
	}

	private void OnEnable()
	{
		StartHUDAnim();
		mUserItemData = null;
		if (mRewardRedeemed)
		{
			InitAchievementData();
			mRewardRedeemed = false;
		}
	}

	private void OnDisable()
	{
		StopHUDAnim();
	}

	protected override void OnDestroy()
	{
		UiDreadfallRedeem.RedeemFinished -= RedeemFinished;
		base.OnDestroy();
	}

	protected override void Update()
	{
		base.Update();
		if (!CommonInventoryData.pIsReady)
		{
			mUserItemData = null;
			return;
		}
		if (mAchTaskReady && mRedeemAchReady && !mHUDBtnProcessed)
		{
			mAchTaskReady = false;
			mRedeemAchReady = false;
			mHUDBtnProcessed = true;
			ProcessForHUDButton();
		}
		SetVisibility(!FUEManager.pIsFUERunning && InteractiveTutManager._CurrentActiveTutorialObject == null && mShowHUDButton);
		if (mTimer != null && mShowHUDButton)
		{
			mTimer.SetText(UtUtilities.GetFormattedTime(DreadfallAchievementManager.pInstance.GetEventRemainingTime(), "D ", "H ", "M ", "S"));
		}
		if (mHUDBtnProcessed)
		{
			UpdateItemQuantity();
		}
		if (mProgressBar != null && mProgressBar.GetProgressLevel() < mCurBarValue)
		{
			float t = Time.deltaTime * _HudBarFillSpeed;
			mProgressBar.SetProgressLevel(Mathf.Lerp(mProgressBar.GetProgressLevel(), mCurBarValue, t));
		}
	}

	private void SetProgressBar(float value)
	{
		mCurBarValue = value;
		DreadfallAchievementManager.pInstance.CachedBarValue = mCurBarValue;
	}

	private void RedeemFinished(List<int> redeemFailList)
	{
		mCurBarValue = 0f;
		mProgressBar.SetProgressLevel(0f);
		mRewardRedeemed = true;
	}

	private void InitAchievementData()
	{
		mAchTaskReady = false;
		mRedeemAchReady = false;
		if (DreadfallAchievementManager.pInstance != null)
		{
			DreadfallAchievementManager.pInstance.GetAchievementTask(AchievementTaskReady);
			DreadfallAchievementManager.pInstance.GetRedeemableRewards(RedeemableAchievementReady);
		}
		mHUDBtnProcessed = false;
	}

	private void UpdateItemQuantity()
	{
		if (mUserItemData == null)
		{
			mUserItemData = CommonInventoryData.pInstance.FindItem(DreadfallAchievementManager.pInstance._DreadfallItemId);
		}
		if (mUserItemData != null && DreadfallAchievementManager.pInstance.CachedCandyQty != mUserItemData.Quantity)
		{
			InitAchievementData();
		}
	}

	private void ProcessForHUDButton()
	{
		if (DreadfallAchievementManager.pInstance != null)
		{
			mShowHUDButton = DreadfallAchievementManager.pInstance.EventInProgress();
			if (!mShowHUDButton && DreadfallAchievementManager.pInstance.GracePeriodInProgress())
			{
				mShowHUDButton = AchievementRedeemPending(mRedeemRewards);
				if (mProgressBar.GetVisibility())
				{
					mProgressBar.SetVisibility(inVisible: false);
				}
			}
			if (!AchievementRedeemPending(mRedeemRewards))
			{
				ShowProgression(mAchievementTask);
				mRedeemReady = false;
				StartHUDAnim();
			}
			else
			{
				mRedeemReady = true;
				StopHUDAnim();
				if (mIcoDreadfall != null)
				{
					mIcoDreadfall.GetUITexture().material = _HudButtonHighlight;
				}
				SetProgressBar(1f);
			}
		}
		mUserItemData = CommonInventoryData.pInstance.FindItem(DreadfallAchievementManager.pInstance._DreadfallItemId);
		if (mUserItemData != null && DreadfallAchievementManager.pInstance != null)
		{
			if (DreadfallAchievementManager.pInstance.CachedCandyQty > 0 && DreadfallAchievementManager.pInstance.CachedCandyQty != mUserItemData.Quantity)
			{
				CandyParticleEffect();
			}
			DreadfallAchievementManager.pInstance.CachedCandyQty = mUserItemData.Quantity;
		}
	}

	private void AchievementTaskReady(UserAchievementTask achTask)
	{
		if (achTask != null)
		{
			mAchievementTask = achTask;
			mAchTaskReady = true;
		}
		else
		{
			UtDebug.Log("Achievement Task null");
		}
	}

	private void RedeemableAchievementReady(UserAchievementTaskRedeemableRewards redeemableRewards)
	{
		mRedeemRewards = redeemableRewards;
		mRedeemAchReady = true;
	}

	private bool AchievementRedeemPending(UserAchievementTaskRedeemableRewards rewards)
	{
		if (rewards != null && mRedeemRewards.RedeemableRewards != null)
		{
			for (int i = 0; i < DreadfallAchievementManager.pInstance.AchievementTaskInfoList.AchievementTaskInfo.Length; i++)
			{
				AchievementTaskInfo taskInfo = DreadfallAchievementManager.pInstance.AchievementTaskInfoList.AchievementTaskInfo[i];
				if (DreadfallAchievementManager.pInstance.AchievementVisible(taskInfo) && Array.Find(rewards.RedeemableRewards, (UserAchievementTaskRedeemableReward x) => x.AchievementInfoID == taskInfo.AchievementInfoID) != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ShowProgression(UserAchievementTask achievementTask)
	{
		float progressBar = 1f;
		if (achievementTask.NextLevel.HasValue)
		{
			progressBar = DreadfallAchievementManager.pInstance.GetAchievementProgress(achievementTask.NextLevel.Value, achievementTask);
		}
		SetProgressBar(progressBar);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mHUDDreadfall)
		{
			UiDreadfall.Load();
		}
	}

	private void StartHUDAnim()
	{
		if (mIcoDreadfall != null && mHudButtonCouroutine == null && !mRedeemReady)
		{
			mHudButtonCouroutine = StartCoroutine(UpdateHUDIcon());
		}
	}

	private void StopHUDAnim()
	{
		if (mHudButtonCouroutine != null)
		{
			StopCoroutine(mHudButtonCouroutine);
		}
		mHudButtonCouroutine = null;
	}

	private IEnumerator UpdateHUDIcon()
	{
		while (!mRedeemReady)
		{
			AnimateIcon(enable: true);
			yield return new WaitForSeconds(_HudButtonSheenDuration);
			AnimateIcon(enable: false);
			yield return new WaitForSeconds(_HudButtonSheenDelay);
		}
	}

	private void AnimateIcon(bool enable)
	{
		if (mIcoDreadfall.GetUITexture().material != _HudButtonIdle)
		{
			mIcoDreadfall.GetUITexture().material = _HudButtonIdle;
		}
		mIcoDreadfall.GetUITexture().material.SetFloat("_USpeedR", enable ? _HudButtonSheenSpeed : 0f);
		mIcoDreadfall.GetUITexture().material.SetFloat("_UTileR", enable ? 1 : 0);
		mIcoDreadfall.GetUITexture().material.SetFloat("_VTileR", enable ? 1 : 0);
		if (mIcoDreadfall.GetUITexture().enabled)
		{
			mIcoDreadfall.GetUITexture().enabled = false;
			mIcoDreadfall.GetUITexture().enabled = true;
		}
	}

	private void CandyParticleEffect()
	{
		if (_CandyParticleEffect != null && mIcoDreadfall.GetUITexture().enabled)
		{
			_CandyParticleEffect.Play();
		}
	}
}
