using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.Event;

public class UiHUDEvent : KAUI
{
	[Header("Event Name")]
	public string _EventName = "";

	[Header("System Message")]
	public LocaleString _ClaimRewardActionText = new LocaleString("[REVIEW] You earned a reward! [[input]] [-] HERE[c] to claim it.");

	[Header("Widgets")]
	[SerializeField]
	protected KAWidget m_ProgressBar;

	[SerializeField]
	protected KAWidget m_HUDEventBtn;

	[SerializeField]
	protected KAWidget m_Timer;

	[SerializeField]
	protected KAWidget m_IcoEvent;

	[SerializeField]
	protected KAWidget m_Alert;

	[SerializeField]
	protected KAWidget m_ClaimReward;

	protected bool mShowHUDButton;

	protected bool mRedeemReady;

	[Header("Effects")]
	public float _HudButtonSheenDelay = 5f;

	public float _HudButtonSheenSpeed = 2f;

	public float _HudButtonSheenDuration = 1f;

	public float _HudBarFillSpeed = 2f;

	public Material _HudButtonIdle;

	public Material _HudButtonHighlight;

	public ParticleSystem _CollectParticleEffect;

	[Header("Job Board")]
	public float _LootBoxCheckDelay = 0.5f;

	protected EventManager mEventManager;

	protected Coroutine mHudButtonCoroutine;

	private UserAchievementTaskRedeemableRewards mRedeemRewards;

	private UserAchievementTask mAchievementTask;

	private bool mAchTaskReady;

	private bool mRedeemAchReady;

	private bool mHUDBtnProcessed;

	private UserItemData mUserItemData;

	private bool mRewardRedeemed;

	protected override void Start()
	{
		base.Start();
		mEventManager = EventManager.Get(_EventName);
		EventManager eventManager = mEventManager;
		if ((object)eventManager != null && eventManager._Type == Type.ACHIEVEMENT_TASK)
		{
			InitAchievementData();
			m_ProgressBar.SetProgressLevel(mEventManager.CachedBarValue);
			return;
		}
		EventManager eventManager2 = mEventManager;
		if ((object)eventManager2 != null && eventManager2._Type == Type.MISSION)
		{
			InvokeRepeating("LootBoxCheck", 0f, _LootBoxCheckDelay);
		}
	}

	protected void OnEnable()
	{
		StartHUDAnim();
		EventManager eventManager = mEventManager;
		if ((object)eventManager != null && eventManager._Type == Type.ACHIEVEMENT_TASK)
		{
			mUserItemData = null;
			if (mRewardRedeemed)
			{
				InitAchievementData();
				mRewardRedeemed = false;
			}
		}
	}

	protected void OnDisable()
	{
		StopHUDAnim();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		EventManager eventManager = mEventManager;
		if ((object)eventManager != null && eventManager._Type == Type.MISSION)
		{
			CancelInvoke("LootBoxCheck");
		}
	}

	protected override void Update()
	{
		base.Update();
		if (CommonInventoryData.pIsReady)
		{
			SetVisibility(!FUEManager.pIsFUERunning && InteractiveTutManager._CurrentActiveTutorialObject == null && mShowHUDButton);
			if (mEventManager != null && mShowHUDButton)
			{
				m_Timer?.SetText(UtUtilities.GetFormattedTime(mEventManager.GetEventRemainingTime(), "D ", "H ", "M ", "S"));
			}
			EventManager eventManager = mEventManager;
			if ((object)eventManager != null && eventManager._Type == Type.ACHIEVEMENT_TASK)
			{
				UpdateAchievementTask();
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == m_HUDEventBtn)
		{
			OpenEventUI("HUD Button Click");
		}
	}

	public void OnLoaded(UiPrizeProgression inUiPrizeProgression)
	{
		if (inUiPrizeProgression != null)
		{
			inUiPrizeProgression.OnRedeemed = (UiRedeem.OnRedeemed)Delegate.Combine(inUiPrizeProgression.OnRedeemed, new UiRedeem.OnRedeemed(RedeemFinished));
		}
	}

	protected void StartHUDAnim()
	{
		if (m_IcoEvent != null && mHudButtonCoroutine == null)
		{
			mHudButtonCoroutine = StartCoroutine(UpdateHUDIcon());
		}
	}

	protected void StopHUDAnim()
	{
		if (mHudButtonCoroutine != null)
		{
			StopCoroutine(mHudButtonCoroutine);
		}
		mHudButtonCoroutine = null;
	}

	protected IEnumerator UpdateHUDIcon()
	{
		while (true)
		{
			UpdateState();
			if (mRedeemReady)
			{
				if (m_IcoEvent != null && m_IcoEvent.GetUITexture().material != _HudButtonHighlight)
				{
					m_IcoEvent.GetUITexture().material = _HudButtonHighlight;
				}
				yield return new WaitForSeconds(_HudButtonSheenDelay);
			}
			else
			{
				AnimateIcon(enable: true);
				yield return new WaitForSeconds(_HudButtonSheenDuration);
				AnimateIcon(enable: false);
				yield return new WaitForSeconds(_HudButtonSheenDelay);
			}
		}
	}

	protected virtual void UpdateState()
	{
		EventManager eventManager = mEventManager;
		if ((object)eventManager != null && eventManager._Type == Type.MISSION)
		{
			mRedeemReady = mEventManager.GetRewardsState();
		}
	}

	protected void AnimateIcon(bool enable)
	{
		if (!(m_IcoEvent.GetUITexture() == null))
		{
			if (m_IcoEvent.GetUITexture().material != _HudButtonIdle)
			{
				m_IcoEvent.GetUITexture().material = _HudButtonIdle;
			}
			m_IcoEvent.GetUITexture().material.SetFloat("_USpeedR", enable ? _HudButtonSheenSpeed : 0f);
			m_IcoEvent.GetUITexture().material.SetFloat("_UTileR", enable ? 1 : 0);
			m_IcoEvent.GetUITexture().material.SetFloat("_VTileR", enable ? 1 : 0);
			if (m_IcoEvent.GetUITexture().enabled)
			{
				m_IcoEvent.GetUITexture().enabled = false;
				m_IcoEvent.GetUITexture().enabled = true;
			}
		}
	}

	protected void ParticleEffect()
	{
		if (_CollectParticleEffect != null && m_IcoEvent.GetUITexture().enabled)
		{
			_CollectParticleEffect.Play();
		}
	}

	private void InitAchievementData()
	{
		mAchTaskReady = false;
		mRedeemAchReady = false;
		mHUDBtnProcessed = false;
		mEventManager?.GetAchievementTask(AchievementTaskReady);
		mEventManager?.GetRedeemableRewards(RedeemableAchievementReady);
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

	private void UpdateAchievementTask()
	{
		if (mAchTaskReady && mRedeemAchReady && !mHUDBtnProcessed)
		{
			mAchTaskReady = false;
			mRedeemAchReady = false;
			mHUDBtnProcessed = true;
			ProcessForHUDButton();
		}
		if (mHUDBtnProcessed)
		{
			UpdateItemQuantity();
		}
		if (!(m_ProgressBar == null) && m_ProgressBar.GetProgressLevel() < mEventManager.CachedBarValue)
		{
			float t = Time.deltaTime * _HudBarFillSpeed;
			m_ProgressBar.SetProgressLevel(Mathf.Lerp(m_ProgressBar.GetProgressLevel(), mEventManager.CachedBarValue, t));
		}
	}

	private void ProcessForHUDButton()
	{
		mShowHUDButton = mEventManager.EventInProgress();
		if (mEventManager.GracePeriodInProgress() && m_ProgressBar.GetVisibility())
		{
			m_ProgressBar.SetVisibility(inVisible: false);
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
			if (m_IcoEvent != null)
			{
				m_IcoEvent.GetUITexture().material = _HudButtonHighlight;
			}
			mEventManager.CachedBarValue = 1f;
			UiChatHistory.AddSystemNotification(_ClaimRewardActionText.GetLocalizedString(), this, OnSystemMessageClicked);
		}
		if ((bool)m_ClaimReward)
		{
			m_ClaimReward.SetVisibility(mRedeemReady);
		}
		mUserItemData = CommonInventoryData.pInstance.FindItem(mEventManager._ItemId);
		if (mUserItemData != null && mEventManager != null)
		{
			if (mEventManager.CachedQuantity > 0 && mEventManager.CachedQuantity != mUserItemData.Quantity && m_IcoEvent.GetUITexture().enabled)
			{
				_CollectParticleEffect?.Play();
			}
			mEventManager.CachedQuantity = mUserItemData.Quantity;
		}
	}

	private void OnSystemMessageClicked(object messageInfo)
	{
		OpenEventUI("System Message Clicked");
	}

	private void OpenEventUI(string analyticsEventParams)
	{
		if ((bool)mEventManager)
		{
			switch (mEventManager._Type)
			{
			case Type.ACHIEVEMENT_TASK:
				UiPrizeProgression.Load(_EventName, OnLoaded);
				break;
			case Type.MISSION:
				UiEventBoard.Load(_EventName);
				break;
			}
		}
	}

	private void UpdateItemQuantity()
	{
		if (mUserItemData == null)
		{
			mUserItemData = CommonInventoryData.pInstance.FindItem(mEventManager._ItemId);
		}
		if (mUserItemData != null && mEventManager.CachedQuantity != mUserItemData.Quantity)
		{
			InitAchievementData();
		}
	}

	private bool AchievementRedeemPending(UserAchievementTaskRedeemableRewards rewards)
	{
		if (rewards != null && mRedeemRewards.RedeemableRewards != null)
		{
			for (int i = 0; i < mEventManager.AchievementTaskInfoList.AchievementTaskInfo.Length; i++)
			{
				AchievementTaskInfo taskInfo = mEventManager.AchievementTaskInfoList.AchievementTaskInfo[i];
				if (mEventManager.AchievementVisible(taskInfo) && Array.Find(rewards.RedeemableRewards, (UserAchievementTaskRedeemableReward x) => x.AchievementInfoID == taskInfo.AchievementInfoID) != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void ShowProgression(UserAchievementTask achievementTask)
	{
		float cachedBarValue = 1f;
		if (achievementTask.NextLevel.HasValue)
		{
			cachedBarValue = mEventManager.GetAchievementProgress(achievementTask.NextLevel.Value, achievementTask);
		}
		mEventManager.CachedBarValue = cachedBarValue;
	}

	private void RedeemFinished(List<int> redeemFailList)
	{
		mEventManager.CachedBarValue = 0f;
		m_ProgressBar.SetProgressLevel(0f);
		mRewardRedeemed = true;
	}

	private void LootBoxCheck()
	{
		mShowHUDButton = mEventManager.EventInProgress();
		if (m_Alert != null)
		{
			m_Alert.SetVisibility(mEventManager.CanOpenMysteryBox());
		}
	}
}
