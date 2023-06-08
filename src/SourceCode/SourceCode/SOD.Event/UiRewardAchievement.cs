using System;
using UnityEngine;

namespace SOD.Event;

public class UiRewardAchievement : KAUI
{
	[Serializable]
	public class RewardAchievementMap
	{
		public string _LevelName;

		public int _AchievementID;

		public EventRewardType _RewardType;
	}

	public enum EventRewardType
	{
		None,
		GameWin,
		FirstPlace,
		SecondPlace,
		ThirdPlace,
		PlayedLevel
	}

	public RewardAchievementMap[] _RewardAchievementMap;

	public RewardWidget _RewardWidget;

	public LocaleString _RewardDisplayText = new LocaleString("[Review] You got %total% Dreadfall Candies! Play this level again in 20 hours to earn more.");

	public KAWidget _RewardInfoTxt;

	public bool _SetAchievementOnStart;

	public string _StartDate;

	public string _EndDate;

	private GameObject mMsgObject;

	private bool mIsRewardCallInProgress;

	private bool mAvatarStateUpdated;

	private bool mUiStateUpdated;

	private DateTime mEventEndDate;

	private DateTime mEventStartDate;

	protected override void Start()
	{
		base.Start();
		if (!string.IsNullOrEmpty(_StartDate))
		{
			mEventStartDate = Convert.ToDateTime(_StartDate, UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
		}
		if (!string.IsNullOrEmpty(_EndDate))
		{
			mEventEndDate = Convert.ToDateTime(_EndDate, UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
		}
		if (_SetAchievementOnStart)
		{
			if (IsEventInProgress() && _RewardAchievementMap != null && _RewardAchievementMap.Length != 0)
			{
				SetReward(_RewardAchievementMap[0]);
			}
			else
			{
				Close();
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (GetVisibility() || mIsRewardCallInProgress)
		{
			if (AvAvatar.pState != AvAvatarState.PAUSED)
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
				mAvatarStateUpdated = true;
			}
			if (AvAvatar.GetUIActive())
			{
				mUiStateUpdated = true;
				AvAvatar.SetUIActive(inActive: false);
			}
		}
	}

	public void SetReward(string LevelName, EventRewardType rewardType, GameObject msgObject)
	{
		mMsgObject = msgObject;
		if (IsEventInProgress() && _RewardAchievementMap != null && _RewardAchievementMap.Length != 0)
		{
			RewardAchievementMap rewardAchievementMap = Array.Find(_RewardAchievementMap, (RewardAchievementMap X) => string.Compare(LevelName, X._LevelName, StringComparison.OrdinalIgnoreCase) == 0 && X._RewardType == rewardType);
			if (rewardAchievementMap != null)
			{
				SetReward(rewardAchievementMap);
				return;
			}
		}
		Close();
	}

	private void SetReward(RewardAchievementMap rewardMap)
	{
		mIsRewardCallInProgress = true;
		KAUICursorManager.SetDefaultCursor("Loading");
		WsWebService.SetAchievementAndGetReward(rewardMap._AchievementID, "", ServiceEventHandler, rewardMap);
	}

	private void Close()
	{
		if (mMsgObject != null)
		{
			mMsgObject.SendMessage("AchievementRewardProcessed");
		}
		SetVisibility(inVisible: false);
		if (mAvatarStateUpdated)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
		}
		if (mUiStateUpdated)
		{
			AvAvatar.SetUIActive(inActive: true);
		}
		if (_SetAchievementOnStart)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		RewardAchievementMap rewardAchievementMap = (RewardAchievementMap)inUserData;
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mIsRewardCallInProgress = false;
			if (inObject != null)
			{
				AchievementReward[] array = (AchievementReward[])inObject;
				if (array == null)
				{
					break;
				}
				GameUtilities.AddRewards(array, inUseRewardManager: false, inImmediateShow: false);
				_RewardWidget.SetRewards(array, MissionManager.pInstance._RewardData);
				if (!GetVisibility())
				{
					SetVisibility(inVisible: true);
				}
				AchievementReward[] array2 = array;
				foreach (AchievementReward achievementReward in array2)
				{
					int? pointTypeID = achievementReward.PointTypeID;
					if (pointTypeID.HasValue && pointTypeID.GetValueOrDefault() == 6)
					{
						if (CommonInventoryData.pInstance != null)
						{
							CommonInventoryData.ReInit();
						}
						if (achievementReward.Amount > 0)
						{
							string localizedString = _RewardDisplayText.GetLocalizedString();
							localizedString = localizedString.Replace("%total%", achievementReward.Amount.ToString());
							_RewardInfoTxt.SetText(localizedString);
						}
					}
				}
			}
			else
			{
				Close();
				UtDebug.LogError("Unable to process Event Reward Achievement for :: " + rewardAchievementMap._AchievementID);
			}
			break;
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mIsRewardCallInProgress = false;
			if (rewardAchievementMap != null)
			{
				UtDebug.LogError("Unable to process Event Reward Achievement for :: " + rewardAchievementMap._AchievementID);
			}
			Close();
			break;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "OKBtn")
		{
			Close();
		}
	}

	private bool IsEventInProgress()
	{
		_ = mEventEndDate;
		if (_EndDate != null && mEventEndDate > ServerTime.pCurrentTime)
		{
			return ServerTime.pCurrentTime > mEventStartDate;
		}
		return false;
	}
}
