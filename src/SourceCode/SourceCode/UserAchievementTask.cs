using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "UAT", Namespace = "")]
public class UserAchievementTask
{
	[XmlElement(ElementName = "gid")]
	public int AchievementTaskGroupID;

	[XmlElement(ElementName = "aq", IsNullable = true)]
	public int? AchievedQuantity;

	[XmlElement(ElementName = "nl", IsNullable = true)]
	public int? NextLevel;

	[XmlElement(ElementName = "qr", IsNullable = true)]
	public int? QuantityRequired;

	[XmlElement(ElementName = "ntr", IsNullable = true)]
	public AchievementTaskReward[] NextLevelAchievementRewards;

	public static ArrayUserAchievementTaskEventHandler mEventDelegate;

	public static AchievementTaskSetResponse _LatestAchievement;

	public static bool BlockAchievement { get; set; }

	public static void Set(params AchievementTask[] achTask)
	{
		List<AchievementTask> list = new List<AchievementTask>();
		for (int i = 0; i < achTask.Length; i++)
		{
			if (achTask[i] != null)
			{
				list.Add(achTask[i]);
			}
		}
		if (list.Count > 0)
		{
			achTask = list.ToArray();
			Set(achTask, displayRewards: false, null, null);
		}
	}

	public static void Set(int taskID, string relatedID = "", bool displayRewards = false, int achievementInfoID = 0, SetAchievementTaskEventHandler callback = null, object userdata = null)
	{
		Set(UserInfo.pInstance.UserID, taskID, 1, relatedID, displayRewards, achievementInfoID, callback, userdata);
	}

	public static void Set(int taskID, int ownerType, string relatedID = "", bool displayRewards = false, int achievementInfoID = 0, SetAchievementTaskEventHandler callback = null, object userdata = null)
	{
		Set(UserInfo.pInstance.UserID, taskID, ownerType, relatedID, displayRewards, achievementInfoID, callback, userdata);
	}

	public static void Set(string ownerID, int taskID, int ownerType = 1, string relatedID = "", bool displayRewards = false, int achievementInfoID = 0, SetAchievementTaskEventHandler callback = null, object userdata = null)
	{
		Set(new AchievementTask[1]
		{
			new AchievementTask(ownerID, taskID, relatedID, achievementInfoID, 0, ownerType)
		}, displayRewards, callback, userdata);
	}

	public static void Set(AchievementTask[] achTask, bool displayRewards, SetAchievementTaskEventHandler callback, object userdata)
	{
		if (!BlockAchievement)
		{
			SetAchievementTaskEventData[] array = new SetAchievementTaskEventData[achTask.Length];
			for (int i = 0; i < achTask.Length; i++)
			{
				array[i] = new SetAchievementTaskEventData();
				array[i].mAchievementTypeID = achTask[i].TaskID;
				array[i].mCallback = callback;
				array[i].mDisplayRewards = displayRewards;
				array[i].mUserData = userdata;
			}
			WsWebService.SetUserAchievementTask(achTask, ServiceEventHandler, array);
		}
	}

	public static void Get(string userID, ArrayUserAchievementTaskEventHandler inEventDelegate, object inUserData)
	{
		mEventDelegate = inEventDelegate;
		WsWebService.GetUserAchievementTask(userID, ServiceEventHandler, inUserData);
	}

	public static void SetByUserID(string userID, int taskID, string relatedID, int achievementInfoID = 0)
	{
		WsWebService.SetAchievementTaskByUserID(userID, taskID, achievementInfoID, relatedID, ServiceEventHandler, null);
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.SET_ACHIEVEMENT_TASK:
		{
			if (inEvent != WsServiceEvent.COMPLETE)
			{
				break;
			}
			if (inObject == null)
			{
				UtDebug.LogError("ERROR WHILE setting Achievement Task");
				break;
			}
			ArrayOfAchievementTaskSetResponse arrayOfAchievementTaskSetResponse = (ArrayOfAchievementTaskSetResponse)inObject;
			SetAchievementTaskEventData[] array = new SetAchievementTaskEventData[arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse.Length];
			if (inUserData != null)
			{
				array = (SetAchievementTaskEventData[])inUserData;
			}
			for (int i = 0; i < arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse.Length; i++)
			{
				if (arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse[i].UserMessage)
				{
					_LatestAchievement = arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse[i];
					GameObject gameObject = GameObject.Find("PfCheckUserMessages");
					if (gameObject != null)
					{
						gameObject.SendMessage("ForceUserMessageUpdate", SendMessageOptions.DontRequireReceiver);
					}
				}
				if (i < array.Length)
				{
					if (array[i] != null)
					{
						if (array[i].mCallback != null)
						{
							array[i].mCallback(inEvent, array[i].mAchievementTypeID, arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse[i].Success, array[i].mUserData);
						}
						if (array[i].mDisplayRewards)
						{
							DisplayAchievementRewards(arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse[i].AchievementRewards);
						}
					}
				}
				else
				{
					UtDebug.LogError("ERROR : Achievement task length not matching with response");
				}
			}
			break;
		}
		case WsServiceType.GET_ACHIEVEMENT_LIST:
			if (inEvent == WsServiceEvent.COMPLETE && mEventDelegate != null)
			{
				mEventDelegate(inEvent, (ArrayOfUserAchievementTask)inObject);
			}
			break;
		}
	}

	public static void DisplayAchievementRewards(AchievementReward[] rewards)
	{
		if (rewards != null)
		{
			RewardManager.SetReward(rewards, inImmediateShow: true);
		}
	}
}
