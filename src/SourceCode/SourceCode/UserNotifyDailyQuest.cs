using System;
using System.Collections.Generic;

public class UserNotifyDailyQuest : UserNotify
{
	[Serializable]
	public class DailyQuestData
	{
		public int _GroupID;

		public string _SceneName;

		public string _TeleportToMarker;
	}

	public LocaleString _NewRewardsAvailableText = new LocaleString("New daily quest rewards are available now.");

	public LocaleString _ActionText = new LocaleString("check them now");

	public List<DailyQuestData> _DailyQuestData;

	private static bool mDoneOnce;

	public const string DQ_COMPLETED_DAY = "DQ_COMPLETED_DAY";

	public const string DQ_STARTED_DAY = "DQ_STARTED_DAY";

	public override void OnWaitBeginImpl()
	{
		if (CanPlayDailyQuests())
		{
			MissionManager.AddMissionEventHandler(OnMissionEvent);
			if (!mDoneOnce)
			{
				mDoneOnce = true;
				int num = -1;
				foreach (DailyQuestData dailyQuestDatum in _DailyQuestData)
				{
					string text = null;
					num++;
					if (IsNewQuestsAvailable(dailyQuestDatum._GroupID))
					{
						text = _NewRewardsAvailableText.GetLocalizedString();
						if (!string.IsNullOrEmpty(text))
						{
							MessageInfo messageInfo = new MessageInfo();
							messageInfo.MessageID = num;
							UiChatHistory.AddSystemNotification(text, messageInfo, OnSystemNotificationClicked, ignoreDuplicateMessage: false, _ActionText._Text);
						}
					}
				}
			}
		}
		OnWaitEnd();
	}

	private void OnSystemNotificationClicked(object messageObject)
	{
		MessageInfo messageInfo = (MessageInfo)messageObject;
		if (messageInfo == null || !messageInfo.MessageID.HasValue)
		{
			return;
		}
		DailyQuestData dailyQuestData = _DailyQuestData[messageInfo.MessageID.Value];
		if (!string.IsNullOrEmpty(dailyQuestData._SceneName) && !string.IsNullOrEmpty(dailyQuestData._TeleportToMarker))
		{
			if (dailyQuestData._SceneName.Equals(RsResourceManager.pCurrentLevel))
			{
				AvAvatar.TeleportToObject(dailyQuestData._TeleportToMarker);
				UiChatHistory.SystemMessageAccepted(messageObject);
			}
			else
			{
				AvAvatar.pStartLocation = dailyQuestData._TeleportToMarker;
				RsResourceManager.LoadLevel(dailyQuestData._SceneName);
			}
		}
	}

	private bool CanPlayDailyQuests()
	{
		if (!FUEManager.pIsFUERunning && _DailyQuestData != null && _DailyQuestData.Count != 0 && MissionManager.pInstance.pDailyMissionStateResult != null && MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement != null)
		{
			return MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement.Count != 0;
		}
		return false;
	}

	private bool IsNewQuestsAvailable(int groupId)
	{
		MissionGroup missionGroup = MissionManager.pInstance.pDailyMissionStateResult.MissionGroup.Find((MissionGroup missiongroup) => missiongroup.MissionGroupID == groupId);
		if (missionGroup == null)
		{
			return false;
		}
		bool flag = MissionManager.pInstance.IsCurrentDayCompleted(missionGroup.CompletionCount, groupId);
		bool flag2 = false;
		if (ProductData.pPairData.GetIntValue("DQ_COMPLETED_DAY_" + groupId, 0) > 0)
		{
			if (MissionManager.pInstance.CheckDailyMissionsReset())
			{
				flag2 = (missionGroup.Day == missionGroup.RewardCycle && flag) || !flag;
			}
			else if (missionGroup.Day == 1 && !flag)
			{
				flag2 = true;
			}
		}
		if (!flag2)
		{
			return false;
		}
		ProductData.pPairData.SetValueAndSave("DQ_COMPLETED_DAY_" + groupId, 0.ToString() ?? "");
		ProductData.pPairData.SetValueAndSave("DQ_STARTED_DAY_" + groupId, 0.ToString() ?? "");
		return true;
	}

	private void OnDestroy()
	{
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent == MissionEvent.MISSION_COMPLETE && MissionManager.pInstance.pDailyMissionStateResult != null)
		{
			Mission inMission = inObject as Mission;
			if (inMission == null)
			{
				return;
			}
			DailyQuestData dqData = _DailyQuestData.Find((DailyQuestData data) => data._GroupID == inMission.GroupID);
			if (dqData != null)
			{
				MissionGroup missionGroup = MissionManager.pInstance.pDailyMissionStateResult.MissionGroup.Find((MissionGroup missiongroup) => missiongroup.MissionGroupID == dqData._GroupID);
				UserTimedAchievement userTimedAchievement = MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement.Find((UserTimedAchievement uta) => uta.Sequence == missionGroup.Day && uta.GroupID == missionGroup.MissionGroupID);
				if (!MissionManager.pInstance.CheckDailyMissionsReset() && ProductData.pPairData.GetIntValue("DQ_COMPLETED_DAY_" + dqData._GroupID, 0) < missionGroup.Day && MissionManager.pInstance.IsCurrentDayCompleted(missionGroup.CompletionCount, dqData._GroupID))
				{
					ProductData.pPairData.SetValueAndSave("DQ_COMPLETED_DAY_" + dqData._GroupID, missionGroup.Day.ToString());
					userTimedAchievement.StatusID = 2;
				}
				int effectiveMissionDay = GetEffectiveMissionDay(missionGroup);
				if (ProductData.pPairData.GetIntValue("DQ_STARTED_DAY_" + dqData._GroupID, 0) != effectiveMissionDay)
				{
					ProductData.pPairData.SetValueAndSave("DQ_STARTED_DAY_" + dqData._GroupID, missionGroup.Day.ToString());
				}
				if (NotificationData.IsNotificationAvailable(UserInfo.pInstance.UserID + "_DailyQuest"))
				{
					MissionManager.pInstance.UpdateDailyMissionNotificationData();
				}
			}
		}
		else
		{
			if (inEvent != 0)
			{
				return;
			}
			Task newTask = inObject as Task;
			if (newTask == null || MissionManager.pInstance.CheckDailyMissionsReset())
			{
				return;
			}
			DailyQuestData dqData2 = _DailyQuestData.Find((DailyQuestData data) => data._GroupID == newTask._Mission.GroupID);
			if (dqData2 == null)
			{
				return;
			}
			MissionGroup missionGroup2 = MissionManager.pInstance.pDailyMissionStateResult.MissionGroup.Find((MissionGroup missiongroup) => missiongroup.MissionGroupID == dqData2._GroupID);
			if (ProductData.pPairData.GetIntValue("DQ_STARTED_DAY_" + dqData2._GroupID, 0) != missionGroup2.Day)
			{
				MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement.Find((UserTimedAchievement uta) => uta.Sequence == missionGroup2.Day && uta.GroupID == missionGroup2.MissionGroupID);
				ProductData.pPairData.SetValueAndSave("DQ_STARTED_DAY_" + dqData2._GroupID, missionGroup2.Day.ToString());
			}
			if (!NotificationData.IsNotificationAvailable(UserInfo.pInstance.UserID + "_DailyQuest"))
			{
				MissionManager.pInstance.UpdateDailyMissionNotificationData();
			}
		}
	}

	public int GetEffectiveMissionDay(MissionGroup missionGroup)
	{
		int num = missionGroup.Day;
		if (MissionManager.pInstance.CheckDailyMissionsReset())
		{
			num = ((missionGroup.Day >= missionGroup.RewardCycle || ProductData.pPairData.GetIntValue("DQ_COMPLETED_DAY_" + missionGroup.MissionGroupID, 0) != missionGroup.Day) ? 1 : (++num));
		}
		return num;
	}
}
