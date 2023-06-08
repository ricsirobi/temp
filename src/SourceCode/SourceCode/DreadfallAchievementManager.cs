using System;
using UnityEngine;

public class DreadfallAchievementManager : MonoBehaviour
{
	public delegate void AchievementInfoCallback(ArrayOfAchievementTaskInfo AchievementTaskInfoList);

	public delegate void AchievementTaskCallback(UserAchievementTask AchievementTask);

	public delegate void RedeemableAchievementCallback(UserAchievementTaskRedeemableRewards RedeemRewards);

	public static DreadfallAchievementManager mInstance;

	public bool pIsReady;

	public int _AchievementTaskId;

	public int _AchievementGroupId;

	public int _GracePeriodDays;

	public int _DreadfallItemId;

	private ArrayOfAchievementTaskInfo mAchievementTaskInfoList;

	private UserAchievementTaskRedeemableRewards mRedeemRewards;

	private UserAchievementTask mAchievementTask;

	public static DreadfallAchievementManager pInstance => mInstance;

	public ArrayOfAchievementTaskInfo AchievementTaskInfoList
	{
		get
		{
			return mAchievementTaskInfoList;
		}
		set
		{
			mAchievementTaskInfoList = value;
		}
	}

	public UserAchievementTaskRedeemableRewards RedeemRewards
	{
		get
		{
			return mRedeemRewards;
		}
		set
		{
			mRedeemRewards = value;
		}
	}

	public UserAchievementTask AchievementTask
	{
		get
		{
			return mAchievementTask;
		}
		set
		{
			mAchievementTask = value;
		}
	}

	public int CachedCandyQty { get; set; }

	public float CachedBarValue { get; set; }

	public void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			GetAchievementTaskInfo(null);
			pIsReady = false;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
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
				mRedeemRewards = (UserAchievementTaskRedeemableRewards)inObject;
				if (inUserData != null)
				{
					((RedeemableAchievementCallback)inUserData)(mRedeemRewards);
				}
				break;
			case WsServiceType.GET_ACHIEVEMENT_LIST:
			{
				ArrayOfUserAchievementTask arrayOfUserAchievementTask = (ArrayOfUserAchievementTask)inObject;
				mAchievementTask = Array.Find(arrayOfUserAchievementTask.UserAchievementTask, (UserAchievementTask x) => x.AchievementTaskGroupID == _AchievementGroupId);
				if (inUserData != null)
				{
					((AchievementTaskCallback)inUserData)(mAchievementTask);
				}
				break;
			}
			case WsServiceType.GET_ACHIEVEMENT_TASK_INFO:
				mAchievementTaskInfoList = (ArrayOfAchievementTaskInfo)inObject;
				if (inUserData != null)
				{
					((AchievementInfoCallback)inUserData)(mAchievementTaskInfoList);
				}
				pIsReady = true;
				if (mAchievementTaskInfoList == null || mAchievementTaskInfoList.AchievementTaskInfo == null || mAchievementTaskInfoList.AchievementTaskInfo.Length < 1 || (!EventInProgress() && !GracePeriodInProgress()))
				{
					UnityEngine.Object.Destroy(base.gameObject);
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
			return ServerTime.pCurrentTime <= info.VisibilityToDate.GetValueOrDefault(ServerTime.pCurrentTime).AddDays(_GracePeriodDays);
		}
		return false;
	}

	public bool EventInProgress()
	{
		int num = AchievementTaskInfoList.AchievementTaskInfo.Length - 1;
		if (AchievementTaskInfoList.AchievementTaskInfo[0].ValidityFromDate.HasValue && AchievementTaskInfoList.AchievementTaskInfo[num].ValidityToDate.HasValue)
		{
			DateTime value = AchievementTaskInfoList.AchievementTaskInfo[0].ValidityFromDate.Value;
			DateTime value2 = AchievementTaskInfoList.AchievementTaskInfo[num].ValidityToDate.Value;
			if (ServerTime.pCurrentTime >= value)
			{
				return ServerTime.pCurrentTime <= value2;
			}
			return false;
		}
		return false;
	}

	public bool GracePeriodInProgress()
	{
		if (AchievementTaskInfoList.AchievementTaskInfo[AchievementTaskInfoList.AchievementTaskInfo.Length - 1].ValidityToDate.HasValue)
		{
			DateTime value = AchievementTaskInfoList.AchievementTaskInfo[AchievementTaskInfoList.AchievementTaskInfo.Length - 1].ValidityToDate.Value;
			DateTime dateTime = value.AddDays(_GracePeriodDays);
			if (ServerTime.pCurrentTime > value)
			{
				return ServerTime.pCurrentTime <= dateTime;
			}
			return false;
		}
		return false;
	}

	public TimeSpan GetEventRemainingTime()
	{
		DateTime pCurrentTime = ServerTime.pCurrentTime;
		DateTime dateTime;
		if (AchievementTaskInfoList.AchievementTaskInfo[AchievementTaskInfoList.AchievementTaskInfo.Length - 1].ValidityToDate.HasValue)
		{
			dateTime = AchievementTaskInfoList.AchievementTaskInfo[AchievementTaskInfoList.AchievementTaskInfo.Length - 1].ValidityToDate.Value;
			if (GracePeriodInProgress())
			{
				dateTime = dateTime.AddDays(_GracePeriodDays);
			}
		}
		else
		{
			dateTime = ServerTime.pCurrentTime;
		}
		return dateTime - pCurrentTime;
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
}
