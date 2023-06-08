using System;
using System.Linq;
using SOD.Event;
using UnityEngine;

public class UIEventRewardAchievement : KAUI
{
	public RewardWidget _RewardWidget;

	public KAWidget _RewardInfoTxt;

	public bool _SetAchievementOnStart;

	private GameObject mMsgObject;

	private bool mIsRewardCallInProgress;

	private bool mAvatarStateUpdated;

	private bool mUiStateUpdated;

	private EventManager mEventManager;

	private int mCurrentGameID;

	public int pCurrentGameID
	{
		get
		{
			return mCurrentGameID;
		}
		set
		{
			mCurrentGameID = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		mEventManager = EventManager.GetActiveEvent();
		if (!(mEventManager == null) && _SetAchievementOnStart && mEventManager.EventInProgress() && !mEventManager.GracePeriodInProgress())
		{
			RewardAchievementMap rewardAchievementMap = mEventManager.GetRewardMaps(pCurrentGameID)?._Maps[0];
			if (rewardAchievementMap != null)
			{
				SetReward(rewardAchievementMap);
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

	public void SetReward(int gameID, string LevelName, EventRewardType rewardType, GameObject msgObject)
	{
		mMsgObject = msgObject;
		if (mEventManager != null && mEventManager.EventInProgress() && !mEventManager.GracePeriodInProgress())
		{
			RewardsMap rewardMaps = mEventManager.GetRewardMaps(gameID);
			if (rewardMaps != null)
			{
				RewardAchievementMap rewardAchievementMap = rewardMaps._Maps.FirstOrDefault((RewardAchievementMap x) => string.Compare(LevelName, x._LevelName, StringComparison.OrdinalIgnoreCase) == 0 && x._RewardType == rewardType);
				if (rewardAchievementMap != null)
				{
					mCurrentGameID = gameID;
					SetReward(rewardAchievementMap);
					return;
				}
			}
		}
		Close();
	}

	private void SetReward(RewardAchievementMap rewardAchievementMap)
	{
		if (rewardAchievementMap != null)
		{
			mIsRewardCallInProgress = true;
			KAUICursorManager.SetDefaultCursor("Loading");
			WsWebService.SetAchievementAndGetReward(rewardAchievementMap._AchievementID, "", ServiceEventHandler, rewardAchievementMap);
		}
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
					if (!pointTypeID.HasValue || pointTypeID.GetValueOrDefault() != 6)
					{
						continue;
					}
					if (CommonInventoryData.pInstance != null)
					{
						CommonInventoryData.ReInit();
					}
					if (achievementReward.Amount > 0)
					{
						string displayString = mEventManager.GetDisplayString(mCurrentGameID);
						if (!string.IsNullOrEmpty(displayString))
						{
							displayString = string.Format(displayString, achievementReward.Amount.ToString());
							_RewardInfoTxt.SetText(displayString);
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
}
