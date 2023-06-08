using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GiftData
{
	public string GiftName;

	public List<GiftSetup> GiftSetup;

	public List<GiftPrerequisite> Prerequisites;

	public int MessageID;

	public int AchievementID;

	public bool AccountOnly;

	[Tooltip("Gift will not be validated or awarded to anyone while this box is checked")]
	public bool InActive;

	public bool pExpired { get; private set; }

	public GiftData()
	{
		GiftSetup = new List<GiftSetup>();
		Prerequisites = new List<GiftPrerequisite>();
		InActive = false;
	}

	public bool ValidatePrerequisites()
	{
		bool flag = false;
		if (Prerequisites == null || Prerequisites.Count == 0)
		{
			return true;
		}
		foreach (GiftPrerequisite prerequisite in Prerequisites)
		{
			switch (prerequisite.Type)
			{
			case GiftPrerequisiteType.DateRange:
			{
				string value = prerequisite.Value;
				flag = ValidateDateRange(value);
				break;
			}
			case GiftPrerequisiteType.Item:
				flag = ValidateItem(prerequisite);
				break;
			case GiftPrerequisiteType.Mission:
			{
				int missionID = UtStringUtil.Parse(prerequisite.Value, -1);
				flag = ValidateMission(missionID);
				break;
			}
			case GiftPrerequisiteType.Task:
			{
				int taskID = UtStringUtil.Parse(prerequisite.Value, -1);
				flag = ValidateTask(taskID);
				break;
			}
			case GiftPrerequisiteType.Member:
				if (UtStringUtil.Parse(prerequisite.Value, inDefault: false) && SubscriptionInfo.pIsReady && SubscriptionInfo.pIsMember)
				{
					flag = true;
				}
				break;
			case GiftPrerequisiteType.Rank:
			{
				int rank = UtStringUtil.Parse(prerequisite.Value, -1);
				flag = ValidateRank(rank);
				break;
			}
			case GiftPrerequisiteType.Gender:
			{
				int gender = UtStringUtil.Parse(prerequisite.Value, 0);
				flag = ValidateGender(gender);
				break;
			}
			}
			if (!flag)
			{
				break;
			}
		}
		return flag;
	}

	public void ValidateGiftExpiry()
	{
		if (!ServerTime.pIsReady || Prerequisites == null || Prerequisites.Count == 0)
		{
			return;
		}
		GiftPrerequisite giftPrerequisite = Prerequisites.Find((GiftPrerequisite x) => x.Type == GiftPrerequisiteType.DateRange);
		if (giftPrerequisite == null)
		{
			return;
		}
		string value = giftPrerequisite.Value;
		if (!string.IsNullOrEmpty(value))
		{
			string[] array = value.Split(',');
			DateTime dateTime = Convert.ToDateTime(array[0], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
			DateTime dateTime2 = Convert.ToDateTime(array[1], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
			if (ServerTime.pCurrentTime > dateTime && ServerTime.pCurrentTime > dateTime2)
			{
				pExpired = true;
			}
		}
	}

	private bool ValidateDateRange(string dateRange)
	{
		if (!ServerTime.pIsReady)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(dateRange))
		{
			string[] array = dateRange.Split(',');
			DateTime dateTime = Convert.ToDateTime(array[0], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
			if (Convert.ToDateTime(array[1], UtUtilities.GetCultureInfo("en-US").DateTimeFormat) >= ServerTime.pCurrentTime && ServerTime.pCurrentTime >= dateTime)
			{
				return true;
			}
		}
		return false;
	}

	private bool ValidateItem(GiftPrerequisite prereq)
	{
		bool result = false;
		if (!CommonInventoryData.pIsReady || !ParentData.pIsReady)
		{
			return result;
		}
		int num = UtStringUtil.Parse(prereq.Value, -1);
		if (num != -1 && CommonInventoryData.pInstance.GetQuantity(num) + ParentData.pInstance.pInventory.GetQuantity(num) >= prereq.Quantity)
		{
			result = true;
		}
		return result;
	}

	private bool ValidateMission(int missionID)
	{
		if (!MissionManager.pIsReady)
		{
			return false;
		}
		if (missionID != -1)
		{
			Mission mission = MissionManager.pInstance.GetMission(missionID);
			if (mission != null && mission.pCompleted)
			{
				return true;
			}
		}
		return false;
	}

	private bool ValidateTask(int taskID)
	{
		if (!MissionManager.pIsReady)
		{
			return false;
		}
		if (taskID != -1)
		{
			Task task = MissionManager.pInstance.GetTask(taskID);
			if (task != null && task.pCompleted)
			{
				return true;
			}
		}
		return false;
	}

	private bool ValidateRank(int rank)
	{
		if (!UserRankData.pIsReady)
		{
			return false;
		}
		if (rank != -1 && UserRankData.pInstance.RankID >= rank)
		{
			return true;
		}
		return false;
	}

	private bool ValidateGender(int gender)
	{
		if (!AvatarData.pIsReady)
		{
			return false;
		}
		Gender result = Gender.Unknown;
		Enum.TryParse<Gender>(gender.ToString(), out result);
		if (AvatarData.GetGender() == result)
		{
			return true;
		}
		return false;
	}
}
