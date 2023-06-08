using System.Collections.Generic;
using UnityEngine;

public class FishingCatchState : FishingState
{
	private GameObject mUiGenericDB;

	public override void ShowTutorial()
	{
		mController.StartTutorial();
		mController.pFishingTutDB.SetPosition(mController._TutMessages[8]._Position.x, mController._TutMessages[8]._Position.y);
		mController.pFishingTutDB.SetOk("", mController._TutMessages[8]._LocaleText.GetLocalizedString());
		FishingZone._FishingZoneUi.ShowCastPointer(show: false);
		FishingZone._FishingZoneUi.RemoveFish(hideOnly: true);
	}

	protected override void HandleOkCancel()
	{
		OnCatch();
	}

	private void OnCatch()
	{
		UtDebug.Log("ENTERING : CATCH_STATE ");
		CommonInventoryData.pInstance.AddItem(mController._CurrentFish._ItemID, updateServer: false);
		List<AchievementTask> list = new List<AchievementTask>();
		list.Add(new AchievementTask(mController._CaughtFishAchievement));
		list.Add(UserProfile.pProfileData.GetGroupAchievement(mController._CaughtFishClanAchievement));
		if (mController._CurrentFish._AchievementClanTaskID > 0)
		{
			list.Add(UserProfile.pProfileData.GetGroupAchievement(mController._CurrentFish._AchievementClanTaskID));
		}
		if (mController._CurrentFish._AchievementTaskID > 0)
		{
			list.Add(new AchievementTask(mController._CurrentFish._AchievementTaskID));
		}
		UserAchievementTask.Set(list.ToArray());
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Game", "Fishing");
			ItemData.Load(mController._CurrentFish._ItemID, OnFishItemDataReady, null);
		}
		if (mController._CurrentFish._XPAchievementTaskID > 0)
		{
			mController.MakeFishModelActive(isActive: false);
			AchievementTask[] inAchievementTasks = new AchievementTask[1]
			{
				new AchievementTask(mController._CurrentFish._XPAchievementTaskID)
			};
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			WsWebService.SetUserAchievementTask(inAchievementTasks, ServiceEventHandler, null);
		}
		else
		{
			mController.CaughtFishMessage();
			mController.LoseBait();
		}
		UtDebug.Log("You've caught a " + mController._CurrentFish.pName + "!!");
		UtDebug.Log("Fish Lvl = " + mController._CurrentFish._Rank);
		UtDebug.Log("Fish Weight = " + mController._CurrentFish._Weight);
		FishingRod component = mController._CurrentFishingRod.GetComponent<FishingRod>();
		UtDebug.Log("Rod Power = " + component._RodPower);
		UtDebug.Log("Reel Max = " + component._ReelMax);
		FishingZone._FishingZoneUi.SetStateText("");
		mController.Reset();
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
						FishingZone._FishingZoneUi.UpdateAvatarMeters();
					}
				}
			}
			mController.CaughtFishMessage(list);
			mController.LoseBait();
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.LogError("ERROR While setting Achievement Task");
			mController.CaughtFishMessage();
			mController.LoseBait();
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			break;
		}
	}

	public override void Enter()
	{
		mController.mPlayerAnimState = "idle";
		if ((bool)mController._SndCaught)
		{
			SnChannel.Play(mController._SndCaught, "DEFAULT_POOL", inForce: true, null);
		}
		if (!mController.pIsTutAvailable)
		{
			OnCatch();
		}
		else
		{
			base.Enter();
		}
	}

	public void OnFishItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null && MissionManager.pInstance != null && !string.IsNullOrEmpty(dataItem.AssetName))
		{
			string[] array = dataItem.AssetName.Split('/');
			if (array.Length == 1)
			{
				MissionManager.pInstance.Collect(array[0]);
			}
			else
			{
				MissionManager.pInstance.Collect(array[2]);
			}
		}
	}

	public override void Exit()
	{
		mController._CurrentFishingRod.GetComponent<FishingRod>().LineSetVisible(visible: false);
		if (mController.pIsTutAvailable)
		{
			base.Exit();
		}
	}

	public override void Execute()
	{
		base.Execute();
	}

	public override void Initialize(FishingZone controller, int nStateId)
	{
		base.Initialize(controller, nStateId);
	}
}
