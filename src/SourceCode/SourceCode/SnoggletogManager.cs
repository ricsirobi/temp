using System;
using System.Collections.Generic;
using UnityEngine;

public class SnoggletogManager : MonoBehaviour
{
	public static SnoggletogManager mInstance;

	public int _GracePeriodDays = 7;

	public int _MissionGroupID = 57;

	[Header("Mystery Box")]
	public int[] _MysteryBoxCategoryID;

	public int _MysteryBoxTriggerCount = 10;

	public int _MysteryBoxRedeemCount = 10;

	private List<Mission> mMissions;

	private DateTime mEventEndDate;

	private DateTime mEventStartDate;

	private List<Task> mTasksToOffer;

	public static SnoggletogManager pInstance => mInstance;

	public bool IsReady { get; set; }

	public void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			IsReady = false;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected void Update()
	{
		if (MissionManager.pInstance != null && MissionManager.pIsReady && !IsReady)
		{
			LoadMissions();
		}
	}

	private void LoadMissions()
	{
		mMissions = MissionManager.pInstance.GetAllMissions(_MissionGroupID);
		if (mMissions != null && mMissions.Count > 0 && mMissions[0].MissionRule != null)
		{
			string prerequisite = mMissions[0].MissionRule.GetPrerequisite<string>(PrerequisiteRequiredType.DateRange);
			if (!string.IsNullOrEmpty(prerequisite))
			{
				string[] array = prerequisite.Split(',');
				mEventStartDate = Convert.ToDateTime(array[0], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
				mEventEndDate = Convert.ToDateTime(array[1], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
			}
			mTasksToOffer = new List<Task>();
			foreach (Mission mMission in mMissions)
			{
				if (!MissionManager.pInstance.IsLocked(mMission) && mMission.Tasks != null && mMission.Tasks.Count > 0 && !mMission.pCompleted)
				{
					mTasksToOffer.AddRange(mMission.Tasks);
				}
			}
		}
		IsReady = true;
	}

	public bool IsEventInProgress()
	{
		if (mEventEndDate > ServerTime.pCurrentTime)
		{
			return ServerTime.pCurrentTime > mEventStartDate;
		}
		return false;
	}

	public bool IsGracePeriodInProgress()
	{
		if (IsEventInProgress())
		{
			return (mEventEndDate - ServerTime.pCurrentTime).TotalDays <= (double)(float)_GracePeriodDays;
		}
		return false;
	}

	public TimeSpan GetEventRemainingTime()
	{
		if (!IsGracePeriodInProgress())
		{
			return mEventEndDate.AddDays(-_GracePeriodDays) - ServerTime.pCurrentTime;
		}
		return mEventEndDate - ServerTime.pCurrentTime;
	}

	public int GetRemainingDays()
	{
		if (GetEventRemainingTime().Days <= 0)
		{
			return 0;
		}
		return GetEventRemainingTime().Days;
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
}
