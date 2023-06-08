using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiJobBoardMenu : KAUIMenu, IAdResult
{
	protected UiJobBoard mParentUi;

	private List<KAWidget> mEmptyWidgetList = new List<KAWidget>();

	private bool mUpdateTimer;

	private KAWidget mClickedAdBtn;

	public void Init(UiJobBoard inParent)
	{
		mParentUi = inParent;
	}

	private IEnumerator UpdateWaitTimer()
	{
		while (mEmptyWidgetList != null && mEmptyWidgetList.Count > 0)
		{
			yield return new WaitForSeconds(1f);
			for (int num = mEmptyWidgetList.Count - 1; num >= 0; num--)
			{
				int num2 = FindItemIndex(mEmptyWidgetList[num]);
				if (mParentUi.IsTaskWaitTimeOver(num2))
				{
					AddNewTask(mEmptyWidgetList[num], num2);
				}
				else
				{
					bool isTrash = false;
					if (mEmptyWidgetList[num].GetUserDataInt() < 0)
					{
						isTrash = true;
					}
					KAWidget kAWidget = mEmptyWidgetList[num].FindChildItem("Timer");
					if (kAWidget != null)
					{
						SetTimerValue(kAWidget, num2, isTrash);
					}
				}
			}
			if ((mEmptyWidgetList == null || mEmptyWidgetList.Count <= 0) && mUpdateTimer)
			{
				mUpdateTimer = false;
			}
		}
	}

	public virtual void SetupTask(KAWidget inWidget, bool markForCompletion)
	{
		int userDataInt = inWidget.GetUserDataInt();
		if (userDataInt <= 0)
		{
			bool flag = false;
			if (userDataInt < 0)
			{
				flag = true;
			}
			inWidget.SetVisibility(inVisible: true);
			inWidget.SetText("");
			KAWidget kAWidget = inWidget.FindChildItem("Timer");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: true);
				SetTimerValue(kAWidget, FindItemIndex(inWidget), flag);
			}
			kAWidget = (KAToggleButton)inWidget.FindChildItem("MissionStatus");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			if (!flag)
			{
				inWidget.SetInteractive(isInteractive: false);
			}
			else
			{
				inWidget.SetInteractive(isInteractive: true);
				ShowGemCost(inWidget);
			}
			HandleAdButton(inWidget);
			mEmptyWidgetList.Add(inWidget);
			if (!mUpdateTimer)
			{
				mUpdateTimer = true;
				StartCoroutine(UpdateWaitTimer());
			}
		}
		else
		{
			Task task = MissionManager.pInstance.GetTask(userDataInt);
			if (task.pData.Title != null && task.pData.Title.GetLocalizedString() != "" && inWidget.GetLabel() != null)
			{
				inWidget.SetText(task.pData.Title.GetLocalizedString());
				inWidget.SetVisibility(inVisible: true);
				inWidget.SetInteractive(isInteractive: true);
			}
			Mission mission = task._Mission;
			while (mission._Parent != null)
			{
				mission = mission._Parent;
			}
			RewardWidget componentInChildren = inWidget.GetComponentInChildren<RewardWidget>();
			if (componentInChildren != null)
			{
				componentInChildren.SetRewards(mission.Rewards.ToArray(), MissionManager.pInstance._RewardData);
			}
			KAToggleButton kAToggleButton = (KAToggleButton)inWidget.FindChildItem("MissionStatus");
			if (kAToggleButton != null)
			{
				kAToggleButton.SetVisibility(inVisible: true);
				kAToggleButton.SetChecked(markForCompletion);
			}
			KAWidget kAWidget2 = inWidget.FindChildItem("Timer");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!(mParentUi != null))
		{
			return;
		}
		if (inWidget.name == "BtnAds")
		{
			if (AdManager.pInstance.AdAvailable(mParentUi._AdEventType, AdType.REWARDED_VIDEO))
			{
				mClickedAdBtn = inWidget;
				AdManager.DisplayAd(mParentUi._AdEventType, AdType.REWARDED_VIDEO, base.gameObject);
			}
			return;
		}
		int userDataInt = inWidget.GetUserDataInt();
		if (userDataInt > 0)
		{
			Task task = MissionManager.pInstance.GetTask(inWidget.GetUserDataInt());
			mParentUi.SetSelectedTask(task);
		}
		else if (userDataInt < 0)
		{
			mParentUi.ShowInstantFillOption();
		}
	}

	public virtual void CompleteTask(string inTaskName, bool isTrash)
	{
		if (isTrash)
		{
			mParentUi.SetInteractive(interactive: true);
		}
		KAWidget kAWidget = FindItem(inTaskName);
		if (!(kAWidget != null))
		{
			return;
		}
		int num = FindItemIndex(kAWidget);
		int num2 = mParentUi.pCurrentJobCategory._WaitTimeForTaskInSec;
		if (isTrash)
		{
			num2 = mParentUi.pCurrentJobCategory._TrashTimeForTaskInSec;
		}
		if (num2 > 0)
		{
			KAWidget kAWidget2 = DuplicateWidget(_Template);
			kAWidget2.SetVisibility(inVisible: true);
			KAWidget kAWidget3 = kAWidget2.FindChildItem("MissionStatus");
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: false);
			}
			AddWidgetAt(num, kAWidget2);
			RemoveWidget(kAWidget);
			if (!isTrash)
			{
				JobBoardData.AddTaskID(num, 0);
				kAWidget2.SetUserDataInt(0);
				kAWidget2.SetInteractive(isInteractive: false);
			}
			else
			{
				JobBoardData.AddTaskID(num, -1);
				kAWidget2.SetUserDataInt(-1);
				kAWidget2.SetInteractive(isInteractive: true);
				ShowGemCost(kAWidget2);
			}
			JobBoardData.AddCompletionTime(num, ServerTime.pCurrentTime);
			kAWidget3 = kAWidget2.FindChildItem("Timer");
			if (kAWidget3 != null)
			{
				kAWidget3.SetVisibility(inVisible: true);
				SetTimerValue(kAWidget3, num, isTrash);
			}
			if (GetSlotFromWidget(kAWidget3) != null)
			{
				GetSlotFromWidget(kAWidget3).pAdWatched = false;
			}
			HandleAdButton(kAWidget2);
			mEmptyWidgetList.Add(kAWidget2);
			if (!mUpdateTimer)
			{
				mUpdateTimer = true;
				StartCoroutine(UpdateWaitTimer());
			}
			mParentUi.SaveJobBoardData();
			AutoSelectTask();
		}
		else
		{
			AddNewTask(kAWidget, num);
		}
	}

	private void SetTimerValue(KAWidget inWidget, int index, bool isTrash)
	{
		if (JobBoardData.pInstance.Slots[index].CompletionTime.HasValue)
		{
			inWidget.SetText(mParentUi.FormatTimeHHMMSS(GetTimerValue(index, isTrash)));
		}
	}

	private int GetTimerValue(int index, bool isTrash)
	{
		DateTime value = JobBoardData.pInstance.Slots[index].CompletionTime.Value;
		TimeSpan timeSpan = ServerTime.pCurrentTime - value;
		int num = mParentUi.pCurrentJobCategory._WaitTimeForTaskInSec;
		if (isTrash)
		{
			num = mParentUi.pCurrentJobCategory._TrashTimeForTaskInSec;
		}
		return num - (int)timeSpan.TotalSeconds;
	}

	public void AddNewTask(KAWidget inWidget, int widgetIndex)
	{
		Task task = null;
		if (mParentUi.pForcedOfferedTask != null)
		{
			task = mParentUi.pForcedOfferedTask;
			mParentUi.pForcedOfferedTask = null;
		}
		else
		{
			task = mParentUi.GetNewTask();
		}
		if (task == null && inWidget.GetUserDataInt() > 0)
		{
			task = MissionManager.pInstance.GetTask(inWidget.GetUserDataInt());
		}
		if (task != null)
		{
			KAWidget kAWidget = DuplicateWidget(_Template);
			kAWidget.name = task.Name;
			kAWidget.SetUserDataInt(task.TaskID);
			AddWidgetAt(widgetIndex, kAWidget);
			SetupTask(kAWidget, mParentUi.MarkTaskForCompletion(task));
			JobBoardData.AddTaskID(widgetIndex, task.TaskID);
			JobBoardData.AddCompletionTime(widgetIndex, null);
			mParentUi.SaveJobBoardData();
		}
		else
		{
			mParentUi.UpdateJobBoardData(widgetIndex);
		}
		RemoveWidget(inWidget);
		AutoSelectTask();
	}

	public virtual void OnTaskComplete(string inTaskName, bool isTrashed)
	{
		if (string.IsNullOrEmpty(inTaskName))
		{
			return;
		}
		KAWidget kAWidget = FindItem(inTaskName);
		if (!(kAWidget != null))
		{
			return;
		}
		PlayJobBoardParticle playJobBoardParticle = kAWidget.gameObject.AddComponent<PlayJobBoardParticle>();
		if (playJobBoardParticle != null)
		{
			ParticleSystem particleSystem = UnityEngine.Object.Instantiate(mParentUi._TaskCompleteParticle);
			particleSystem.transform.parent = kAWidget.transform;
			Vector3 localPosition = new Vector3(0f, 0f, -50f);
			particleSystem.transform.localPosition = localPosition;
			particleSystem.transform.localRotation = Quaternion.identity;
			playJobBoardParticle._Particle = particleSystem;
			playJobBoardParticle._Duration = mParentUi._ParticleDuration;
			playJobBoardParticle.Init(base.gameObject, isTrashed);
			playJobBoardParticle.PlayParticle(isPlay: true);
			kAWidget.SetVisibility(inVisible: false);
			SetSelectedItem(null);
			if (mParentUi._TaskCompleteSfx != null)
			{
				SnChannel.Play(mParentUi._TaskCompleteSfx, "SFX_Pool", inForce: true);
			}
		}
	}

	private void OnParticleEffectDone(GameObject inObject)
	{
		PlayJobBoardParticle component = inObject.GetComponent<PlayJobBoardParticle>();
		if (component != null)
		{
			CompleteTask(inObject.name, component.pTrashed);
		}
		else
		{
			CompleteTask(inObject.name, isTrash: false);
		}
		UpdateTaskCompletionState();
	}

	public override void RemoveWidget(KAWidget inWidget)
	{
		PlayJobBoardParticle component = inWidget.gameObject.GetComponent<PlayJobBoardParticle>();
		if (component != null)
		{
			UnityEngine.Object.Destroy(component._Particle);
			UnityEngine.Object.Destroy(component);
		}
		if (mEmptyWidgetList.Contains(inWidget))
		{
			StopCoroutine("UpdateWaitTimer");
			mUpdateTimer = false;
			mEmptyWidgetList.Remove(inWidget);
			if (mEmptyWidgetList.Count > 0)
			{
				mUpdateTimer = true;
				StartCoroutine(UpdateWaitTimer());
			}
		}
		base.RemoveWidget(inWidget);
	}

	public void Reset()
	{
		mEmptyWidgetList.Clear();
		ClearItems();
		StopCoroutine("UpdateWaitTimer");
		mUpdateTimer = false;
	}

	private void ShowGemCost(KAWidget inWidget)
	{
		RewardWidget componentInChildren = inWidget.GetComponentInChildren<RewardWidget>();
		AchievementReward[] array = new AchievementReward[1]
		{
			new AchievementReward()
		};
		array[0].Amount = mParentUi.pInstantFillCost;
		array[0].PointTypeID = 5;
		componentInChildren.SetRewards(array, MissionManager.pInstance._RewardData);
	}

	private void UpdateTaskCompletionState()
	{
		foreach (KAWidget item in mItemInfo)
		{
			KAToggleButton kAToggleButton = (KAToggleButton)item.FindChildItem("MissionStatus");
			if (!(kAToggleButton != null) || !kAToggleButton.IsChecked())
			{
				continue;
			}
			int userDataInt = item.GetUserDataInt();
			if (userDataInt > 0)
			{
				Task task = MissionManager.pInstance.GetTask(userDataInt);
				if (task != null)
				{
					kAToggleButton.SetChecked(mParentUi.MarkTaskForCompletion(task));
				}
			}
		}
	}

	private void AutoSelectTask()
	{
		foreach (KAWidget item in mItemInfo)
		{
			if (item.GetUserDataInt() > 0)
			{
				SetSelectedItem(item);
				OnClick(item);
				break;
			}
		}
	}

	private void HandleAdButton(KAWidget inWidget)
	{
		KAWidget kAWidget = inWidget.FindChildItem("BtnAds");
		if (!(kAWidget != null))
		{
			return;
		}
		if (AdManager.pInstance.AdSupported(mParentUi._AdEventType, AdType.REWARDED_VIDEO) && kAWidget != null && GetSlotFromWidget(kAWidget) != null && !GetSlotFromWidget(kAWidget).pAdWatched)
		{
			kAWidget.SetVisibility(inVisible: true);
			if (kAWidget.GetLabel() != null)
			{
				kAWidget.GetLabel().text = AdManager.pInstance.GetReductionTimeText(mParentUi._AdEventType);
			}
		}
		else
		{
			kAWidget.SetVisibility(inVisible: false);
		}
	}

	public void OnAdWatched()
	{
		AdManager.pInstance.LogAdWatchedEvent(mParentUi._AdEventType, "JobBoard");
		AdManager.pInstance.SyncAdAvailableCount(mParentUi._AdEventType, isConsumed: true);
		if (mClickedAdBtn == null)
		{
			return;
		}
		JobBoardSlot slotFromWidget = GetSlotFromWidget(mClickedAdBtn);
		if (slotFromWidget != null)
		{
			int reductionTime = AdManager.pInstance.GetReductionTime(mParentUi._AdEventType, GetTimerValue(FindItemIndex(mClickedAdBtn.GetParentItem()), isTrash: true));
			if (slotFromWidget.CompletionTime.HasValue)
			{
				slotFromWidget.CompletionTime = slotFromWidget.CompletionTime.Value.AddSeconds(-reductionTime);
			}
			slotFromWidget.pAdWatched = true;
			mParentUi.SaveJobBoardData();
		}
	}

	public void OnAdFailed()
	{
		mClickedAdBtn.SetVisibility(inVisible: false);
		mClickedAdBtn = null;
		UtDebug.LogError("OnAdFailed for event:- " + mParentUi._AdEventType);
	}

	public void OnAdSkipped()
	{
	}

	public void OnAdClosed()
	{
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}

	private JobBoardSlot GetSlotFromWidget(KAWidget inWidget)
	{
		if (inWidget != null && inWidget.GetParentItem() != null)
		{
			KAWidget parentItem = inWidget.GetParentItem();
			if (mItemInfo.Contains(parentItem) && FindItemIndex(parentItem) < JobBoardData.pInstance.Slots.Length)
			{
				return JobBoardData.pInstance.Slots[FindItemIndex(parentItem)];
			}
		}
		return null;
	}
}
