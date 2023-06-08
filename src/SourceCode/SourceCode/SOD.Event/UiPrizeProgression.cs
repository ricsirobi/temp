using System;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.Event;

public class UiPrizeProgression : KAUI
{
	public delegate void OnLoaded(UiPrizeProgression inUiPrizeProgression);

	[Header("Event Name")]
	public string _EventName = "";

	public RewardIconMap[] _RewardIconMap;

	[SerializeField]
	private GameObject m_HelpScreen;

	[SerializeField]
	private string m_RedeemUiResourcePath = "RS_DATA/pfUi{EventName}redeem.unity3d/PfUi{EventName}Redeem";

	[SerializeField]
	private UiPrizeProgressionMenu m_UiPrizeMenu;

	[Header("Widgets")]
	[SerializeField]
	private KAWidget m_CloseBtn;

	[SerializeField]
	private KAWidget m_BtnHowToPlay;

	[SerializeField]
	private KAWidget m_BtnExchange;

	[SerializeField]
	private KAWidget m_EventEndDays;

	[SerializeField]
	private KAWidget m_RedeemEndDays;

	[SerializeField]
	private KAWidget m_ItemQuantityTxt;

	[SerializeField]
	private LocaleString m_EventEndedText = new LocaleString("Event has ended.");

	private EventManager mEventManager;

	private UiHelpScreen mUiHelpScreen;

	private UserAchievementTaskRedeemableRewards mRedeemableRewards;

	private List<AchievementTaskInfo> mRedeemableRewardsList;

	private UserAchievementTask mAchievementTask;

	private bool mRedeemReady;

	private bool mAchTaskReady;

	private bool mRewardsDisplayed;

	private AvAvatarState mLastAvatarState;

	[HideInInspector]
	public UserNotifyEvent m_UserNotifyEvent;

	public Action OnClosed;

	public UiRedeem.OnRedeemed OnRedeemed { get; set; }

	public static void Load(string eventName, OnLoaded onLoaded = null)
	{
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(EventManager.Get(eventName)._AssetName, OnBundleLoaded, typeof(GameObject), inDontDestroy: false, onLoaded);
	}

	private static void OnBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			UiPrizeProgression component = obj.GetComponent<UiPrizeProgression>();
			((OnLoaded)inUserData)?.Invoke(component);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			((OnLoaded)inUserData)?.Invoke(null);
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading Prize Progression Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected void OnDisable()
	{
		if (mUiHelpScreen != null)
		{
			mUiHelpScreen.OnClicked -= HelpUiButtonClicked;
		}
	}

	protected override void Start()
	{
		base.Start();
		mRedeemReady = false;
		mAchTaskReady = false;
		mRewardsDisplayed = false;
		SetVisibility(inVisible: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		mEventManager = EventManager.Get(_EventName);
		if (mEventManager != null)
		{
			mEventManager.GetRedeemableRewards(RedeemInfoReady);
			mEventManager.GetAchievementTask(AchievementTaskDataReady);
			if ((bool)m_BtnHowToPlay)
			{
				m_BtnHowToPlay.SetVisibility(!mEventManager.GracePeriodInProgress());
			}
		}
		if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
		{
			mLastAvatarState = AvAvatarState.PAUSED;
			AvAvatar.pState = AvAvatarState.PAUSED;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.SetUIActive(inActive: false);
			}
		}
		DisplayEndDate();
		DisplayItemQuantity();
	}

	protected override void Update()
	{
		base.Update();
		if (mAchTaskReady && mRedeemReady && !mRewardsDisplayed)
		{
			if (mAchievementTask == null)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				CloseUi();
				UtDebug.Log("Achievement Task in null");
			}
			else
			{
				mRewardsDisplayed = true;
				ProcessRewardsDisplay();
			}
		}
	}

	private void AchievementTaskDataReady(UserAchievementTask achTask)
	{
		mAchievementTask = achTask;
		mAchTaskReady = true;
	}

	private void RedeemInfoReady(UserAchievementTaskRedeemableRewards redeemableRewards)
	{
		mRedeemableRewards = redeemableRewards;
		mRedeemReady = true;
	}

	private void ProcessRewardsDisplay()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mRedeemableRewards != null && mRedeemableRewards.RedeemableRewards != null && mRedeemableRewards.RedeemableRewards.Length != 0)
		{
			mRedeemableRewardsList = new List<AchievementTaskInfo>();
			mRedeemableRewardsList = GetRedeemableRewards(mRedeemableRewards);
			if (mRedeemableRewardsList.Count > 0)
			{
				ShowRedeemUi();
				return;
			}
		}
		RedeemFinished(null);
	}

	private void DisplayEndDate()
	{
		if (!(mEventManager == null))
		{
			int days = mEventManager.GetEventRemainingTime().Days;
			m_EventEndDays.SetText(mEventManager.GracePeriodInProgress() ? m_EventEndedText.GetLocalizedString() : string.Format(m_EventEndDays.GetText(), days));
			m_RedeemEndDays.SetText(string.Format(m_RedeemEndDays.GetText(), mEventManager.GracePeriodInProgress() ? days : (days + mEventManager._GracePeriodDays)));
		}
	}

	private void DisplayItemQuantity()
	{
		if (!(mEventManager == null) && !(m_ItemQuantityTxt == null))
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(mEventManager._ItemId);
			m_ItemQuantityTxt.SetText((userItemData != null) ? userItemData.Quantity.ToString() : "0");
		}
	}

	private void CloseUi(bool invokeOnClosed = true)
	{
		if (mUiHelpScreen != null)
		{
			UnityEngine.Object.Destroy(mUiHelpScreen.gameObject);
		}
		if (mLastAvatarState == AvAvatarState.PAUSED)
		{
			AvAvatar.pState = AvAvatar.pPrevState;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.SetUIActive(inActive: true);
			}
		}
		KAUI.RemoveExclusive(this);
		if (invokeOnClosed)
		{
			OnClosed?.Invoke();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OpenHelp()
	{
		if (!(m_HelpScreen == null))
		{
			if (mUiHelpScreen == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_HelpScreen);
				mUiHelpScreen = gameObject.GetComponent<UiHelpScreen>();
				mUiHelpScreen.OnClicked += HelpUiButtonClicked;
			}
			SetVisibility(inVisible: false);
			mUiHelpScreen.SetVisibility(inVisible: true);
			KAUI.SetExclusive(mUiHelpScreen);
		}
	}

	private List<AchievementTaskInfo> GetRedeemableRewards(UserAchievementTaskRedeemableRewards rewards)
	{
		List<AchievementTaskInfo> list = new List<AchievementTaskInfo>();
		if (rewards == null)
		{
			return list;
		}
		for (int i = 0; i < mEventManager.AchievementTaskInfoList.AchievementTaskInfo.Length; i++)
		{
			AchievementTaskInfo info = mEventManager.AchievementTaskInfoList.AchievementTaskInfo[i];
			if (mEventManager.AchievementVisible(info) && Array.Find(rewards.RedeemableRewards, (UserAchievementTaskRedeemableReward x) => x.AchievementInfoID == info.AchievementInfoID) != null)
			{
				list.Add(info);
			}
		}
		return list;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == m_CloseBtn)
		{
			CloseUi();
		}
		else if (inWidget == m_BtnHowToPlay)
		{
			OpenHelp();
		}
		else if (inWidget == m_BtnExchange)
		{
			CloseUi(invokeOnClosed: false);
			ShowExchangeUi();
		}
	}

	private void ShowExchangeUi()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		m_RedeemUiResourcePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(EventManager.Get(_EventName)._ExchangeAssetName, OnUiLoaded, typeof(GameObject));
	}

	private void ShowRedeemUi()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = m_RedeemUiResourcePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUiLoaded, typeof(GameObject));
	}

	private void OnUiLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			UiRedeemMenu componentInChildren = obj.GetComponentInChildren<UiRedeemMenu>();
			UiRedeem componentInChildren2 = obj.GetComponentInChildren<UiRedeem>();
			UiExchange component = obj.GetComponent<UiExchange>();
			if ((bool)componentInChildren2 && (bool)componentInChildren)
			{
				componentInChildren2.SetRedeemData(mRedeemableRewardsList, RedeemFinished);
				componentInChildren.PopulateUi(_RewardIconMap, mRedeemableRewardsList);
			}
			else if ((bool)component && (bool)m_UserNotifyEvent)
			{
				component.m_UserNotifyEvent = m_UserNotifyEvent;
				component.OnClosed = (Action)Delegate.Combine(component.OnClosed, new Action(m_UserNotifyEvent.MarkUserNotifyDone));
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			RedeemFinished(null);
			break;
		}
	}

	private void RedeemFinished(List<int> redeemFailList)
	{
		if (mEventManager.EventInProgress())
		{
			KAUI.SetExclusive(this);
			SetVisibility(inVisible: true);
			m_UiPrizeMenu.Populate(mAchievementTask, redeemFailList);
			OnRedeemed?.Invoke(redeemFailList);
		}
		else
		{
			CloseUi();
		}
	}

	private void HelpUiButtonClicked(string buttonName)
	{
		if (!(buttonName == "Back"))
		{
			if (buttonName == "Exit")
			{
				CloseUi();
			}
		}
		else
		{
			SetVisibility(inVisible: true);
			KAUI.SetExclusive(this);
		}
	}
}
