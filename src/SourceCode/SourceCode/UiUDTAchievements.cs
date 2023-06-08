using System;
using UnityEngine;

public class UiUDTAchievements : UiAchievements
{
	public enum RedeemStatus
	{
		NotAvailable,
		Available,
		Redeemed
	}

	[Serializable]
	public class AchievementTaskInfoItem
	{
		public int _TaskTargetValue;

		public int _AchievementInfoID;
	}

	[Serializable]
	public class AchievementTaskInfoMap
	{
		public int _AchievementID;

		public AchievementTaskInfoItem[] _AchievementTaskInfoItem;
	}

	public class RedeemInfo
	{
		public int _AchievementInfoID;

		public bool _IsRedeemAvailable;

		public AchievementTaskReward[] _AchievementRewards;
	}

	public class KAUIAchievementUserData : KAWidgetUserData
	{
		public UserAchievementTask _UserAchievementTask;

		public int _AchievementInfoID = -1;

		public KAUIAchievementUserData(UserAchievementTask task)
		{
			_UserAchievementTask = task;
		}
	}

	public UiUDTAchievementsMenu _Menu;

	public static Color _AchievementCompleteColor = Color.red;

	public string _RewardConfirmationDB = "RS_DATA/PfUiUDTRewardPopUp.unity3d/PfUiUDTRewardPopUp";

	public AchievementTaskInfoMap[] _AchievementTaskInfoMap;

	private UserAchievementTaskRedeemableRewards mRedeemableRewards;

	private AchievementReward[] mLastAcheivedRewards;

	protected override void Start()
	{
		base.Start();
		WsWebService.GetUserAchievementTaskRedeemableRewards(ServiceEventHandler, null);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			mRedeemableRewards = (UserAchievementTaskRedeemableRewards)inObject;
		}
	}

	public override void AchievementEventHandler(WsServiceEvent inEvent, ArrayOfUserAchievementTask arrayOfUserAchievementTask)
	{
		if (this == null)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			mLoadingAchData = false;
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (arrayOfUserAchievementTask == null)
			{
				break;
			}
			int num = 0;
			int num2 = 0;
			UserAchievementTask[] userAchievementTask = arrayOfUserAchievementTask.UserAchievementTask;
			foreach (UserAchievementTask userAchievementTask2 in userAchievementTask)
			{
				int achievementIndex;
				AchievementTitleInfo achievementTitleInfo = FindAchievementInfo(userAchievementTask2.AchievementTaskGroupID, out achievementIndex);
				if (achievementTitleInfo != null)
				{
					KAWidget kAWidget = DuplicateWidget(_Menu._Template);
					KAUIAchievementUserData userData = new KAUIAchievementUserData(userAchievementTask2);
					kAWidget.SetUserData(userData);
					RefreshItem(kAWidget, userAchievementTask2);
					RedeemStatus redeemStatus = GetRedeemStatus(userAchievementTask2);
					if (userAchievementTask2.NextLevel.HasValue)
					{
						achievementTitleInfo.pLevel = userAchievementTask2.NextLevel.Value - 1;
					}
					switch (redeemStatus)
					{
					case RedeemStatus.Redeemed:
						_Menu.AddWidgetAt(num2, kAWidget);
						break;
					case RedeemStatus.Available:
						_Menu.AddWidgetAt(0, kAWidget);
						num++;
						break;
					default:
						_Menu.AddWidgetAt(num, kAWidget);
						break;
					}
					num2++;
				}
			}
			mIsReady = true;
			SetVisibility(t: true);
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.LogError("ERROR: Unable to fetch achievements!");
			KAUICursorManager.SetDefaultCursor("Arrow");
			mLoadingAchData = false;
			break;
		}
	}

	public void RefreshItem(KAWidget achievement, UserAchievementTask task)
	{
		int achievementIndex;
		AchievementTitleInfo achievementTitleInfo = FindAchievementInfo(task.AchievementTaskGroupID, out achievementIndex);
		if (achievementTitleInfo == null)
		{
			return;
		}
		achievement.SetVisibility(inVisible: true);
		string text = "";
		text = achievementTitleInfo._TitleText.GetLocalizedString();
		KAWidget kAWidget = achievement.FindChildItem("TxtTrophyName");
		if (kAWidget != null)
		{
			kAWidget.SetText(text);
		}
		achievementTitleInfo.pCounter = 0;
		achievementTitleInfo.pCountRemForNext = 0;
		if (task.AchievedQuantity.HasValue)
		{
			achievementTitleInfo.pCounter = task.AchievedQuantity.Value * achievementTitleInfo._Multiplier;
		}
		if (task.QuantityRequired.HasValue)
		{
			achievementTitleInfo.pCountRemForNext = task.QuantityRequired.Value * achievementTitleInfo._Multiplier;
		}
		if (task.NextLevelAchievementRewards != null)
		{
			achievementTitleInfo.pRewards = task.NextLevelAchievementRewards;
		}
		achievementTitleInfo.pLevel = Mathf.Min(3, achievementTitleInfo._MaxRewardCount);
		if (task.NextLevel.HasValue)
		{
			achievementTitleInfo.pLevel = task.NextLevel.Value - 1;
		}
		RedeemInfo[] redeemInfo = GetRedeemInfo(achievementIndex);
		bool flag = achievementTitleInfo.pLevel == achievementTitleInfo._MaxRewardCount;
		int num = 0;
		bool flag2 = false;
		if (redeemInfo != null)
		{
			RedeemInfo[] array = redeemInfo;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i]._IsRedeemAvailable)
				{
					flag2 = true;
					break;
				}
				num++;
			}
		}
		flag = flag && !flag2;
		num = Mathf.Min(achievementTitleInfo.pLevel, num);
		float achievementProgress = GetAchievementProgress(achievementIndex, num, task);
		text = ((!flag) ? StringTable.GetStringData(_PopUpMessagesText[achievementTitleInfo._PopUpMessageID]._ID, _PopUpMessagesText[achievementTitleInfo._PopUpMessageID]._Text) : StringTable.GetStringData(_PopUpMessageAllDoneText[achievementTitleInfo._PopUpMessageAllDoneID]._ID, _PopUpMessageAllDoneText[achievementTitleInfo._PopUpMessageAllDoneID]._Text));
		kAWidget = achievement.FindChildItem("TxtRewardMessage");
		if (kAWidget != null)
		{
			kAWidget.SetText(text);
		}
		kAWidget = achievement.FindChildItem("BmpProgress");
		if (flag)
		{
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			kAWidget = achievement.FindChildItem("TxtProgress");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		else
		{
			if (kAWidget != null)
			{
				kAWidget.SetProgressLevel(achievementProgress);
				int a = (int)(achievementProgress * 100f);
				kAWidget.SetText(Mathf.Min(a, 100) + "%");
			}
			kAWidget = achievement.FindChildItem("TxtAchievementComplete");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		kAWidget = achievement.FindChildItem("BtnClaimReward");
		if (kAWidget != null && flag)
		{
			kAWidget.SetVisibility(inVisible: false);
			KAWidget kAWidget2 = achievement.FindChildItem("TxtAchievementComplete");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: true);
				kAWidget2.GetLabel().color = _AchievementCompleteColor;
			}
		}
		else if (achievementProgress < 1f)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		int num2 = Mathf.Min(3, achievementTitleInfo._MaxRewardCount);
		if (num2 == 1)
		{
			kAWidget = achievement.FindChildItem("IcoAchieveUDTFrame02");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		else
		{
			for (int j = 1; j <= num2; j++)
			{
				kAWidget = achievement.FindChildItem("IcoAchieveUDTFrame0" + j);
				if (kAWidget != null)
				{
					kAWidget.SetVisibility(inVisible: true);
				}
			}
		}
		switch (num)
		{
		case 1:
			kAWidget = ((num2 != 1) ? achievement.FindChildItem("IcoAchieveUDT01") : achievement.FindChildItem("IcoAchieveUDT02"));
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: true);
			}
			break;
		case 2:
		{
			for (int l = 1; l < 3; l++)
			{
				kAWidget = achievement.FindChildItem("IcoAchieveUDT0" + l);
				if (kAWidget != null)
				{
					kAWidget.SetVisibility(inVisible: true);
				}
			}
			break;
		}
		case 3:
		{
			for (int k = 1; k < 4; k++)
			{
				kAWidget = achievement.FindChildItem("IcoAchieveUDT0" + k);
				if (kAWidget != null)
				{
					kAWidget.SetVisibility(inVisible: true);
				}
			}
			break;
		}
		}
		RewardWidget component = achievement.FindChildItem("XPReward").GetComponent<RewardWidget>();
		if (!flag2)
		{
			component.SetRewards(achievementTitleInfo.pRewards, MissionManager.pInstance._RewardData);
		}
		else
		{
			component.SetRewards(redeemInfo[num]._AchievementRewards, MissionManager.pInstance._RewardData);
		}
	}

	public RedeemStatus GetRedeemStatus(UserAchievementTask task)
	{
		RedeemStatus result = RedeemStatus.NotAvailable;
		int achievementIndex;
		AchievementTitleInfo achievementTitleInfo = FindAchievementInfo(task.AchievementTaskGroupID, out achievementIndex);
		int num = Mathf.Min(3, achievementTitleInfo._MaxRewardCount);
		if (task.NextLevel.HasValue)
		{
			num = task.NextLevel.Value - 1;
		}
		RedeemInfo[] redeemInfo = GetRedeemInfo(achievementIndex);
		bool flag = num >= achievementTitleInfo._MaxRewardCount;
		if (redeemInfo != null)
		{
			RedeemInfo[] array = redeemInfo;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i]._IsRedeemAvailable)
				{
					result = RedeemStatus.Available;
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			result = RedeemStatus.Redeemed;
		}
		return result;
	}

	private float GetAchievementProgress(int achId, int taskIndex, UserAchievementTask task)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 1f;
		AchievementTaskInfoMap achievementTaskInfoMap = null;
		if (achId < _AchievementTaskInfoMap.Length)
		{
			achievementTaskInfoMap = _AchievementTaskInfoMap[achId];
		}
		if (achievementTaskInfoMap != null)
		{
			int num4 = taskIndex - 1;
			if (num4 >= 0)
			{
				num = achievementTaskInfoMap._AchievementTaskInfoItem[num4]._TaskTargetValue;
			}
			if (taskIndex < achievementTaskInfoMap._AchievementTaskInfoItem.Length)
			{
				num3 = achievementTaskInfoMap._AchievementTaskInfoItem[taskIndex]._TaskTargetValue;
			}
			num2 = num;
			if (task.AchievedQuantity.HasValue)
			{
				num2 = task.AchievedQuantity.Value;
			}
		}
		return (num2 - num) / (num3 - num);
	}

	private RedeemInfo[] GetRedeemInfo(int achId)
	{
		RedeemInfo[] array = new RedeemInfo[_AchievementTaskInfoMap[achId]._AchievementTaskInfoItem.Length];
		for (int i = 0; i < _AchievementTaskInfoMap[achId]._AchievementTaskInfoItem.Length; i++)
		{
			array[i] = new RedeemInfo();
			array[i]._AchievementInfoID = _AchievementTaskInfoMap[achId]._AchievementTaskInfoItem[i]._AchievementInfoID;
			array[i]._IsRedeemAvailable = false;
			if (mRedeemableRewards == null || mRedeemableRewards.RedeemableRewards == null)
			{
				continue;
			}
			for (int j = 0; j < mRedeemableRewards.RedeemableRewards.Length; j++)
			{
				if (mRedeemableRewards.RedeemableRewards[j] != null && mRedeemableRewards.RedeemableRewards[j].AchievementInfoID == _AchievementTaskInfoMap[achId]._AchievementTaskInfoItem[i]._AchievementInfoID)
				{
					array[i]._IsRedeemAvailable = true;
					array[i]._AchievementRewards = mRedeemableRewards.RedeemableRewards[j].AchievementTaskRewards;
					break;
				}
			}
		}
		return array;
	}

	private void RedeemDone(int achievementInfoID)
	{
		if (mRedeemableRewards == null || mRedeemableRewards.RedeemableRewards == null)
		{
			return;
		}
		for (int i = 0; i < mRedeemableRewards.RedeemableRewards.Length; i++)
		{
			if (mRedeemableRewards.RedeemableRewards[i] != null && mRedeemableRewards.RedeemableRewards[i].AchievementInfoID == achievementInfoID)
			{
				mRedeemableRewards.RedeemableRewards[i] = null;
				break;
			}
		}
	}

	public void RedeemReward(KAWidget achievement)
	{
		KAUIAchievementUserData kAUIAchievementUserData = (KAUIAchievementUserData)achievement.GetUserData();
		UserAchievementTask userAchievementTask = kAUIAchievementUserData._UserAchievementTask;
		FindAchievementInfo(userAchievementTask.AchievementTaskGroupID, out var achievementIndex);
		RedeemInfo[] redeemInfo = GetRedeemInfo(achievementIndex);
		if (redeemInfo == null)
		{
			return;
		}
		int num = 0;
		RedeemInfo[] array = redeemInfo;
		foreach (RedeemInfo redeemInfo2 in array)
		{
			if (redeemInfo2._IsRedeemAvailable)
			{
				achievement.SetInteractive(isInteractive: false);
				kAUIAchievementUserData._AchievementInfoID = redeemInfo2._AchievementInfoID;
				WsWebService.RedeemUserAchievementTaskReward(redeemInfo2._AchievementInfoID, RedeemServiceEventHandler, achievement);
				break;
			}
			num++;
		}
	}

	private void RedeemServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != WsServiceEvent.COMPLETE || inObject == null)
		{
			return;
		}
		RedeemUserAchievementTaskResponse redeemUserAchievementTaskResponse = (RedeemUserAchievementTaskResponse)inObject;
		if (redeemUserAchievementTaskResponse == null)
		{
			return;
		}
		AchievementReward[] achievementRewards = redeemUserAchievementTaskResponse.AchievementRewards;
		if (achievementRewards != null)
		{
			mLastAcheivedRewards = achievementRewards;
			GameUtilities.AddRewards(achievementRewards);
			if (!string.IsNullOrEmpty(_RewardConfirmationDB))
			{
				SetInteractive(interactive: false);
				string[] array = _RewardConfirmationDB.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], RewardDBLoadEvent, typeof(GameObject));
			}
		}
		KAWidget kAWidget = (KAWidget)inUserData;
		KAUIAchievementUserData kAUIAchievementUserData = (KAUIAchievementUserData)kAWidget.GetUserData();
		RedeemDone(kAUIAchievementUserData._AchievementInfoID);
		RefreshItem(kAWidget, kAUIAchievementUserData._UserAchievementTask);
		kAWidget.SetInteractive(isInteractive: true);
	}

	private void RewardDBLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			UiUDTRewardPopUp component = obj.GetComponent<UiUDTRewardPopUp>();
			if (component != null)
			{
				if (GetVisibility())
				{
					component.SetRewards(mLastAcheivedRewards, MissionManager.pInstance._RewardData);
					component._MessageObject = base.gameObject;
					KAUI.SetExclusive(component);
				}
				else
				{
					KAUICursorManager.SetDefaultCursor("Arrow");
					SetInteractive(interactive: true);
				}
			}
			else
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				SetInteractive(interactive: true);
				Debug.LogError("No UiMissionRewardDB found! MissionRewardDB will not function properly! " + inURL);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
			Debug.LogError("Error loading MissionRewardDB! " + inURL);
			break;
		}
	}

	public void OnRewardClose()
	{
		SetInteractive(interactive: true);
	}

	public void Exit()
	{
	}

	public void Clear()
	{
	}

	public void ProcessClose()
	{
	}

	public void ActivateUI(int uiIndex, bool isActive = true)
	{
	}

	public bool IsBusy()
	{
		return mLoadingAchData;
	}
}
