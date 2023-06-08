using System.Collections.Generic;
using UnityEngine;

public class UiFishTrapRewards : UiGameResults
{
	public KAWidget[] _RewardWidgets;

	public int _CaughtFishAchievement = 174;

	public int _CaughtFishClanAchievement = 198;

	public List<Fish> _Fish;

	private List<AchievementTask> mFishingAchievements;

	private List<AchievementTask> mFishingXPAchievements;

	public void DisplayRewards(AchievementReward[] inRewards, FarmManager inFarmManager)
	{
		KAWidget[] rewardWidgets = _RewardWidgets;
		for (int i = 0; i < rewardWidgets.Length; i++)
		{
			rewardWidgets[i].SetVisibility(inVisible: false);
		}
		ResetAchievements();
		if (inRewards != null && inRewards.Length != 0)
		{
			for (int j = 0; j < inRewards.Length; j++)
			{
				ItemData itemData = inFarmManager.GetItemData(inRewards[j].ItemID);
				string[] array = itemData.IconName.Split('/');
				_RewardWidgets[j].SetVisibility(inVisible: true);
				_RewardWidgets[j].SetSprite(array[2]);
				KAWidget obj = _RewardWidgets[j];
				int? amount = inRewards[j].Amount;
				obj.SetText(amount + "  " + itemData.ItemName);
				AddFishAchievement(inRewards[j]);
			}
			SetFishingAchievents();
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
		}
	}

	private void ResetAchievements()
	{
		if (mFishingAchievements == null)
		{
			mFishingAchievements = new List<AchievementTask>();
		}
		else
		{
			mFishingAchievements.Clear();
		}
		if (mFishingXPAchievements == null)
		{
			mFishingXPAchievements = new List<AchievementTask>();
		}
		else
		{
			mFishingXPAchievements.Clear();
		}
	}

	private void AddFishAchievement(AchievementReward inReward)
	{
		if (mFishingAchievements == null || !inReward.PointTypeID.HasValue || inReward.PointTypeID.Value != 6)
		{
			return;
		}
		Fish fish = _Fish.Find((Fish f) => f._ItemID == inReward.ItemID);
		if (fish == null)
		{
			return;
		}
		for (int i = 0; i < inReward.Amount; i++)
		{
			mFishingAchievements.Add(new AchievementTask(_CaughtFishAchievement));
			mFishingAchievements.Add(UserProfile.pProfileData.GetGroupAchievement(_CaughtFishClanAchievement));
			if (fish._AchievementClanTaskID > 0)
			{
				mFishingAchievements.Add(UserProfile.pProfileData.GetGroupAchievement(fish._AchievementClanTaskID));
			}
			if (fish._AchievementTaskID > 0)
			{
				AchievementTask item = new AchievementTask(fish._AchievementTaskID);
				mFishingAchievements.Add(item);
			}
			if (fish._XPAchievementTaskID > 0)
			{
				AchievementTask item2 = new AchievementTask(fish._XPAchievementTaskID);
				mFishingXPAchievements.Add(item2);
			}
		}
	}

	private void SetFishingAchievents()
	{
		if (mFishingAchievements != null && mFishingAchievements.Count > 0)
		{
			UserAchievementTask.Set(mFishingAchievements.ToArray());
		}
		if (mFishingXPAchievements != null && mFishingXPAchievements.Count > 0)
		{
			SetInteractive(interactive: false);
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			WsWebService.SetUserAchievementTask(mFishingXPAchievements.ToArray(), ServiceEventHandler, null);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			List<AchievementReward> list = new List<AchievementReward>();
			if (inObject == null)
			{
				UtDebug.LogError("ERROR WHILE setting Achievement Task");
			}
			else
			{
				ArrayOfAchievementTaskSetResponse arrayOfAchievementTaskSetResponse = (ArrayOfAchievementTaskSetResponse)inObject;
				for (int i = 0; i < arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse.Length; i++)
				{
					if (arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse[i].UserMessage)
					{
						GameObject gameObject = GameObject.Find("PfCheckUserMessages");
						if (gameObject != null)
						{
							gameObject.SendMessage("ForceUserMessageUpdate", SendMessageOptions.DontRequireReceiver);
						}
					}
					if (arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse[i].AchievementRewards != null)
					{
						list.AddRange(arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse[i].AchievementRewards);
						GameUtilities.AddRewards(arrayOfAchievementTaskSetResponse.AchievementTaskSetResponse[i].AchievementRewards, inUseRewardManager: true, inImmediateShow: false);
					}
				}
			}
			SetInteractive(interactive: true);
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.LogError("ERROR While setting Achievement Task");
			SetInteractive(interactive: true);
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			break;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
	}
}
