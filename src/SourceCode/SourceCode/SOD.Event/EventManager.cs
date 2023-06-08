using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SOD.Event;

public class EventManager : MonoBehaviour
{
	public delegate void AchievementInfoCallback(ArrayOfAchievementTaskInfo AchievementTaskInfoList);

	public delegate void AchievementTaskCallback(UserAchievementTask AchievementTask);

	public delegate void RedeemableAchievementCallback(UserAchievementTaskRedeemableRewards RedeemRewards);

	private static Dictionary<string, EventManager> mEventManagers;

	public static bool pIsReady = true;

	public string _EventName = "";

	public Type _Type;

	public string _AssetName;

	public string _ExchangeAssetName;

	public string _IntroAssetName;

	public string _HelpAssetName;

	public Texture _StableQuestIcon;

	public int _GracePeriodDays;

	[Header("Achievement Task")]
	public int _AchievementTaskId;

	public int _AchievementGroupId;

	public int _ItemId;

	[Header("Mission")]
	public int _MissionGroupID = 57;

	public int[] _MysteryBoxCategoryID;

	public int _MysteryBoxTriggerCount = 10;

	public int _MysteryBoxRedeemCount = 10;

	public List<string> _UnlockedScenes;

	public RewardsMap[] _RewardMaps;

	public List<int> _GameIDsRejectedDuringGracePeriod;

	private List<Task> mTasksToOffer;

	private DateTime mEndDate;

	private DateTime mStartDate;

	public ArrayOfAchievementTaskInfo AchievementTaskInfoList { get; set; }

	public UserAchievementTaskRedeemableRewards RedeemRewards { get; set; }

	public UserAchievementTask AchievementTask { get; set; }

	public int CachedQuantity { get; set; }

	public float CachedBarValue { get; set; }

	public DateTime pStartDate => mStartDate;

	public DateTime pEndDate => mEndDate;

	public static EventManager Get(string inEventName)
	{
		if (mEventManagers == null || !mEventManagers.ContainsKey(inEventName))
		{
			return null;
		}
		return mEventManagers[inEventName];
	}

	public void Awake()
	{
		if (mEventManagers != null && mEventManagers.ContainsKey(_EventName))
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		pIsReady = false;
		if (mEventManagers == null)
		{
			mEventManagers = new Dictionary<string, EventManager>();
		}
		mEventManagers.Add(_EventName, this);
		if (_Type == Type.ACHIEVEMENT_TASK)
		{
			GetAchievementTaskInfo(null);
		}
	}

	protected void Update()
	{
		if (_Type != Type.MISSION || !MissionManager.pIsReady || pIsReady)
		{
			return;
		}
		List<Mission> allMissions = MissionManager.pInstance.GetAllMissions(_MissionGroupID);
		if (allMissions != null && allMissions.Count > 0 && allMissions[0].MissionRule != null)
		{
			string prerequisite = allMissions[0].MissionRule.GetPrerequisite<string>(PrerequisiteRequiredType.DateRange);
			if (!string.IsNullOrEmpty(prerequisite))
			{
				string[] array = prerequisite.Split(',');
				mStartDate = Convert.ToDateTime(array[0], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
				mEndDate = Convert.ToDateTime(array[1], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
			}
			mTasksToOffer = new List<Task>();
			foreach (Mission item in allMissions)
			{
				if (!MissionManager.pInstance.IsLocked(item) && item.Tasks != null && item.Tasks.Count > 0 && !item.pCompleted)
				{
					mTasksToOffer.AddRange(item.Tasks);
				}
			}
		}
		pIsReady = true;
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inObject == null)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			switch (inType)
			{
			case WsServiceType.GET_USER_ACHIEVEMENTS_TASK_REDEEMABLE_REWARDS:
				RedeemRewards = (UserAchievementTaskRedeemableRewards)inObject;
				if (inUserData != null)
				{
					((RedeemableAchievementCallback)inUserData)(RedeemRewards);
				}
				break;
			case WsServiceType.GET_ACHIEVEMENT_LIST:
			{
				ArrayOfUserAchievementTask arrayOfUserAchievementTask = (ArrayOfUserAchievementTask)inObject;
				AchievementTask = Array.Find(arrayOfUserAchievementTask.UserAchievementTask, (UserAchievementTask x) => x.AchievementTaskGroupID == _AchievementGroupId);
				if (inUserData != null)
				{
					((AchievementTaskCallback)inUserData)(AchievementTask);
				}
				break;
			}
			case WsServiceType.GET_ACHIEVEMENT_TASK_INFO:
				AchievementTaskInfoList = (ArrayOfAchievementTaskInfo)inObject;
				pIsReady = true;
				if (AchievementTaskInfoList == null || AchievementTaskInfoList.AchievementTaskInfo == null || AchievementTaskInfoList.AchievementTaskInfo.Length < 1)
				{
					UnityEngine.Object.Destroy(base.gameObject);
					break;
				}
				if (AchievementTaskInfoList.AchievementTaskInfo[0].ValidityFromDate.HasValue && AchievementTaskInfoList.AchievementTaskInfo[AchievementTaskInfoList.AchievementTaskInfo.Length - 1].ValidityToDate.HasValue)
				{
					mStartDate = AchievementTaskInfoList.AchievementTaskInfo[0].ValidityFromDate.Value;
					mEndDate = AchievementTaskInfoList.AchievementTaskInfo[AchievementTaskInfoList.AchievementTaskInfo.Length - 1].ValidityToDate.Value;
				}
				if (!EventInProgress())
				{
					mEventManagers.Remove(_EventName);
					UnityEngine.Object.Destroy(base.gameObject);
				}
				else if (inUserData != null)
				{
					((AchievementInfoCallback)inUserData)(AchievementTaskInfoList);
				}
				break;
			}
			break;
		case WsServiceEvent.ERROR:
			UtDebug.LogError("ERROR: Unable to fetch achievements!");
			if (inUserData != null)
			{
				((RedeemableAchievementCallback)inUserData)(null);
			}
			if (inType == WsServiceType.GET_ACHIEVEMENT_TASK_INFO)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			break;
		}
	}

	public RewardsMap GetRewardMaps(int gameID)
	{
		RewardsMap result = null;
		if (_RewardMaps != null && _RewardMaps.Length != 0)
		{
			RewardsMap[] rewardMaps = _RewardMaps;
			foreach (RewardsMap rewardsMap in rewardMaps)
			{
				if (rewardsMap._GameID == gameID)
				{
					result = rewardsMap;
				}
			}
		}
		return result;
	}

	public void GetAchievementTaskInfo(AchievementInfoCallback callback)
	{
		WsWebService.GetAchievementTaskInfo(new int[1] { _AchievementTaskId }, ServiceEventHandler, callback);
	}

	public void GetAchievementTask(AchievementTaskCallback callback)
	{
		WsWebService.GetUserAchievementTask(UserInfo.pInstance.UserID, ServiceEventHandler, callback);
	}

	public void GetRedeemableRewards(RedeemableAchievementCallback callback)
	{
		WsWebService.GetUserAchievementTaskRedeemableRewards(ServiceEventHandler, callback);
	}

	public bool AchievementVisible(AchievementTaskInfo info)
	{
		if (ServerTime.pCurrentTime >= info.VisibilityFromDate.GetValueOrDefault(ServerTime.pCurrentTime))
		{
			return ServerTime.pCurrentTime <= info.VisibilityToDate.GetValueOrDefault(ServerTime.pCurrentTime);
		}
		return false;
	}

	public bool EventInProgress()
	{
		if (mEndDate >= ServerTime.pCurrentTime)
		{
			return ServerTime.pCurrentTime >= mStartDate;
		}
		return false;
	}

	public static bool EventActiveAndInProgress()
	{
		if ((bool)GetActiveEvent())
		{
			return GetActiveEvent().EventInProgress();
		}
		return false;
	}

	public bool GracePeriodInProgress()
	{
		if (EventInProgress())
		{
			return (mEndDate - ServerTime.pCurrentTime).TotalDays <= (double)(float)_GracePeriodDays;
		}
		return false;
	}

	public TimeSpan GetEventRemainingTime()
	{
		DateTime dateTime = mEndDate;
		if (!GracePeriodInProgress())
		{
			dateTime = dateTime.AddDays(-_GracePeriodDays);
		}
		return dateTime - ServerTime.pCurrentTime;
	}

	public int GetRemainingDays()
	{
		if (GetEventRemainingTime().Days <= 0)
		{
			return 0;
		}
		return GetEventRemainingTime().Days;
	}

	public float GetAchievementProgress(int currentLevel, UserAchievementTask task)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 1f;
		if (task.AchievedQuantity.HasValue)
		{
			num2 = task.AchievedQuantity.Value;
		}
		if (currentLevel - 1 > 0)
		{
			AchievementTaskInfo achievementTaskInfo = Array.Find(AchievementTaskInfoList.AchievementTaskInfo, (AchievementTaskInfo x) => x.Level == currentLevel - 1);
			if (achievementTaskInfo != null)
			{
				num = achievementTaskInfo.PointValue;
			}
		}
		if (task.AchievedQuantity.HasValue && task.QuantityRequired.HasValue)
		{
			num3 = (float)task.AchievedQuantity.Value + (float)task.QuantityRequired.Value;
		}
		return (num2 - num) / (num3 - num);
	}

	public bool CanOpenMysteryBox()
	{
		if (CommonInventoryData.pIsReady)
		{
			UserItemData[] items = CommonInventoryData.pInstance.GetItems(_MysteryBoxCategoryID);
			if (items != null)
			{
				return items.Length != 0;
			}
			return false;
		}
		return false;
	}

	public bool CanForceOpenMysteryBox()
	{
		if (CommonInventoryData.pIsReady)
		{
			UserItemData[] items = CommonInventoryData.pInstance.GetItems(_MysteryBoxCategoryID);
			int num = 0;
			if (items != null)
			{
				UserItemData[] array = items;
				foreach (UserItemData userItemData in array)
				{
					num += userItemData.Quantity;
				}
			}
			return num >= _MysteryBoxTriggerCount;
		}
		return false;
	}

	public string GetDisplayString(int gameID)
	{
		RewardsMap rewardsMap = _RewardMaps.FirstOrDefault((RewardsMap x) => x._GameID == gameID);
		if (rewardsMap == null)
		{
			return string.Empty;
		}
		return rewardsMap._RewardDisplayText.GetLocalizedString();
	}

	public static EventManager GetActiveEvent()
	{
		if (mEventManagers != null)
		{
			foreach (EventManager value in mEventManagers.Values)
			{
				if ((bool)value)
				{
					return value;
				}
			}
		}
		return null;
	}

	public static List<EventManager> GetInProgress()
	{
		List<EventManager> list = new List<EventManager>();
		if (mEventManagers != null)
		{
			foreach (EventManager value in mEventManagers.Values)
			{
				if (value != null && value.EventInProgress())
				{
					list.Add(value);
				}
			}
		}
		return list;
	}

	public bool GetRewardsState()
	{
		if (mTasksToOffer != null && MissionManager.pIsReady)
		{
			foreach (Task item in mTasksToOffer)
			{
				if (MissionManager.pInstance.CanCompleteTask(item, "Delivery"))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsSceneUnlocked(string inSceneName)
	{
		if (mEventManagers != null)
		{
			foreach (KeyValuePair<string, EventManager> mEventManager in mEventManagers)
			{
				if (Get(mEventManager.Key)._UnlockedScenes.Contains(inSceneName))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsChallengeValid(ChallengeInfo inChallengeInfo)
	{
		if (mEventManagers != null)
		{
			foreach (KeyValuePair<string, EventManager> mEventManager in mEventManagers)
			{
				if (Get(mEventManager.Key)._GameIDsRejectedDuringGracePeriod.Contains(inChallengeInfo.ChallengeGameInfo.GameID) && Get(mEventManager.Key).GracePeriodInProgress())
				{
					return false;
				}
			}
		}
		return true;
	}
}
