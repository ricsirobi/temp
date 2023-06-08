using System;
using System.Collections.Generic;
using UnityEngine;

public class UiJobBoard : KAUI
{
	public class MissionItemDataLoader
	{
		private KAWidget mTextureWidget;

		private KAWidget mTextWidget;

		private string mTextToAppend = string.Empty;

		private Color mTextColor = Color.black;

		public MissionItemDataLoader(int itemID, KAWidget textureWidget, KAWidget textWidget, string textToAppend, Color inColor, bool showItemName)
		{
			mTextureWidget = textureWidget;
			mTextWidget = textWidget;
			mTextToAppend = textToAppend;
			mTextColor = inColor;
			ItemData.Load(itemID, ItemDataEventHandler, showItemName);
		}

		private void ItemDataEventHandler(int itemID, ItemData dataItem, object inUserData)
		{
			if (mTextureWidget != null && !string.IsNullOrEmpty(dataItem.IconName))
			{
				string[] array = dataItem.IconName.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], IconEventHandler, typeof(Texture));
			}
			if (!(mTextWidget == null) && !string.IsNullOrEmpty(dataItem.ItemName))
			{
				UILabel label = mTextWidget.GetLabel();
				if (label != null)
				{
					label.color = mTextColor;
				}
				if ((bool)inUserData)
				{
					mTextWidget.SetText(dataItem.ItemName + " " + mTextToAppend);
				}
				else
				{
					mTextWidget.SetText(mTextToAppend);
				}
				mTextWidget.SetVisibility(inVisible: true);
			}
		}

		private void IconEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
		{
			if (inEvent == RsResourceLoadEvent.COMPLETE && inObject != null && !(mTextureWidget == null))
			{
				mTextureWidget.SetTexture((Texture)inObject);
				mTextureWidget.SetVisibility(inVisible: true);
			}
		}
	}

	public const string JOBBOARD_DATA_KEY = "JBD_";

	public AdEventType _AdEventType;

	public JobBoardCategory[] _JobCategories;

	public DefaultJobBoardCategory[] _DefaultCategoryData;

	public int _TaskToLoad = 9;

	public ParticleSystem _TaskCompleteParticle;

	public float _ParticleDuration = 3f;

	public AudioClip _TaskCompleteSfx;

	public int _InstantFillStoreID = 93;

	public int _InstantFillItemID = 8713;

	public string PlayfabEventIdentifier = "JobBoard";

	public LocaleString _InstantFillFeeText = new LocaleString("Would you like to restore task with {{GEMS}} gems?");

	public LocaleString _InstantFillPurchaseSuccessText = new LocaleString("Restore task successful.");

	public LocaleString _InstantFillPurchaseFailText = new LocaleString("Restore task failed.");

	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public LocaleString _PurchaseProcessingText = new LocaleString("Processing purchase.");

	public LocaleString _JobNotAvailableText = new LocaleString("No jobs are available right now.");

	public Color _RequiredColor = Color.red;

	public Color _RequiredCompleteColor = Color.green;

	public RewardWidget _XPRewardWidget;

	public bool _ShowRewardItemName = true;

	public bool _ShowRewardItemQuantity = true;

	protected UiJobBoardCategoryMenu mCategoryMenu;

	protected UiJobBoardMenu mMenu;

	private JobBoardCategory mCurrentJobCategory;

	private KAWidget mAcceptBtn;

	private KAWidget mTrashBtn;

	private KAUIGenericDB mUiGenericDB;

	protected bool mSaveJobBoardData;

	private bool mMissionRewardsComplete;

	private int mInstantFillCost;

	public JobBoardCategory pCurrentJobCategory => mCurrentJobCategory;

	public Task pCurrentTask { get; set; }

	public int pInstantFillCost => mInstantFillCost;

	public Task pForcedOfferedTask { get; set; }

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		mAcceptBtn = FindItem("BtnAccept");
		mTrashBtn = FindItem("BtnDecline");
		InitMenu();
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		ItemStoreDataLoader.Load(_InstantFillStoreID, OnStoreLoaded);
		if (AddJobCategories())
		{
			int defaultCategory = GetDefaultCategory();
			KAToggleButton kAToggleButton = (KAToggleButton)mCategoryMenu.GetItemAt(defaultCategory);
			if (kAToggleButton != null)
			{
				kAToggleButton.SetChecked(isChecked: true);
			}
			SetSelectedCategory(_JobCategories[defaultCategory]);
		}
		MissionManager.AddMissionEventHandler(OnMissionEvent);
	}

	protected virtual void InitMenu()
	{
		mCategoryMenu = (UiJobBoardCategoryMenu)GetMenu("UiJobBoardCategoryMenu");
		if (mCategoryMenu != null)
		{
			mCategoryMenu.Init(this);
		}
		mMenu = (UiJobBoardMenu)GetMenu("UiJobBoardMenu");
		if (mMenu != null)
		{
			mMenu.Init(this);
		}
	}

	private int GetDefaultCategory()
	{
		if (_DefaultCategoryData == null || _DefaultCategoryData.Length == 0)
		{
			return 0;
		}
		for (int i = 0; i < _DefaultCategoryData.Length; i++)
		{
			if (string.Compare(_DefaultCategoryData[i]._SceneName, RsResourceManager.pCurrentLevel, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return _DefaultCategoryData[i]._DefaultCategoryIndex;
			}
		}
		return 0;
	}

	private bool AddJobCategories()
	{
		if (mCategoryMenu == null || _JobCategories == null || _JobCategories.Length == 0)
		{
			UtDebug.LogError("NO JOB CATEGORIES DEFINED!!!");
			return false;
		}
		mCategoryMenu.SetVisibility(inVisible: true);
		JobBoardCategory[] jobCategories = _JobCategories;
		foreach (JobBoardCategory jobBoardCategory in jobCategories)
		{
			KAWidget kAWidget = mCategoryMenu.AddWidget(jobBoardCategory._Name._Text);
			if (kAWidget != null)
			{
				kAWidget.SetToolTipText(jobBoardCategory._Name.GetLocalizedString());
				kAWidget.SetVisibility(inVisible: true);
				KAWidget kAWidget2 = kAWidget.FindChildItem("Icon");
				if (kAWidget2 != null)
				{
					kAWidget2.SetTexture(jobBoardCategory._Icon);
					kAWidget2.SetVisibility(inVisible: true);
				}
				kAWidget2 = kAWidget.FindChildItem("TxtCatName");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(jobBoardCategory._Name.GetLocalizedString());
					kAWidget2.SetVisibility(inVisible: true);
				}
			}
		}
		return true;
	}

	public void SetSelectedCategory(JobBoardCategory inCategory)
	{
		if (mCurrentJobCategory != inCategory)
		{
			HideTaskDetails();
			if (mSaveJobBoardData && mCurrentJobCategory != null)
			{
				SaveJobBoardData();
			}
			mCurrentJobCategory = inCategory;
			if (mCurrentJobCategory == null)
			{
				UtDebug.LogError("JOB CATEGORY NOT FOUND!!");
				return;
			}
			JobBoardData.pInstance = null;
			SetupActiveTaskandTime();
			KAUICursorManager.SetDefaultCursor("Loading");
			LoadMission();
		}
	}

	public virtual void LoadMission()
	{
		((MissionManagerDO)MissionManager.pInstance).LoadMissionData(mCurrentJobCategory._MissionGroupID, OnMissionDataLoad);
	}

	public virtual void OnMissionDataLoad(List<Mission> missions)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		PopulateTasks(GetOfferedTasks(mCurrentJobCategory._MissionGroupID));
	}

	private void SetupActiveTaskandTime()
	{
		string value = ProductData.pPairData.GetValue("JBD_" + mCurrentJobCategory._MissionGroupID);
		if (!string.IsNullOrEmpty(value) && value != "___VALUE_NOT_FOUND___")
		{
			JobBoardData.Init(value);
		}
	}

	public JobBoardCategory GetJobCategoryFromName(string inName)
	{
		JobBoardCategory[] jobCategories = _JobCategories;
		foreach (JobBoardCategory jobBoardCategory in jobCategories)
		{
			if (jobBoardCategory._Name._Text == inName)
			{
				return jobBoardCategory;
			}
		}
		return null;
	}

	protected virtual List<Task> GetOfferedTasks(int inMissionGroupID)
	{
		List<Mission> allMissions = MissionManager.pInstance.GetAllMissions(inMissionGroupID);
		List<Task> list = new List<Task>();
		if (allMissions != null && allMissions.Count > 0)
		{
			foreach (Mission item in allMissions)
			{
				if (MissionManager.pInstance.IsLocked(item) || item.Tasks == null || item.Tasks.Count <= 0 || (!item.Repeatable && item.pCompleted))
				{
					continue;
				}
				foreach (Task task in item.Tasks)
				{
					list.Add(task);
				}
			}
		}
		else
		{
			UtDebug.LogError("NO MISSIONS FOUND FOR MISSION GROUP ID " + inMissionGroupID);
		}
		return list;
	}

	protected void PopulateTasks(List<Task> tasksToOffer)
	{
		if (tasksToOffer == null)
		{
			UtDebug.LogError("NO TASK FOUND UNDER CATEGORY:: " + mCurrentJobCategory._Name._Text);
			return;
		}
		pCurrentTask = null;
		mMenu.Reset();
		mMenu.SetVisibility(inVisible: true);
		if (JobBoardData.pInstance == null || JobBoardData.pInstance.Slots == null || JobBoardData.pInstance.Slots.Length == 0)
		{
			int num = _TaskToLoad;
			if (num > tasksToOffer.Count)
			{
				num = tasksToOffer.Count;
			}
			JobBoardData.Init(num);
			for (int i = 0; i < num; i++)
			{
				JobBoardData.AddTaskID(i, tasksToOffer[i].TaskID);
			}
			mSaveJobBoardData = true;
		}
		AddTaskFromList(tasksToOffer);
		if (pCurrentTask == null)
		{
			ShowNoJobMessage();
		}
	}

	private void AddTaskWidget(Task intask)
	{
		KAWidget kAWidget = mMenu.AddWidget(intask.Name);
		kAWidget.SetUserDataInt(intask.TaskID);
		mMenu.SetupTask(kAWidget, MarkTaskForCompletion(intask));
		AddRequiredItems(kAWidget, intask);
		if (pCurrentTask == null)
		{
			mMenu.SetSelectedItem(kAWidget);
			pCurrentTask = intask;
			HideTaskDetails();
			SetupDetails();
		}
	}

	protected void AddRequiredItems(KAWidget widget, Task task)
	{
		if (task.pData.Objectives == null)
		{
			return;
		}
		for (int i = 1; i <= task.pData.Objectives.Count; i++)
		{
			TaskObjective taskObjective = task.pData.Objectives[i - 1];
			int num = taskObjective.Get<int>("ItemID");
			if (num <= 0)
			{
				continue;
			}
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(num);
			bool flag = userItemData != null;
			int num2 = 0;
			string text = taskObjective.Get<string>("Name");
			if (flag)
			{
				num2 = userItemData.Quantity;
				text = userItemData.Item.ItemName;
			}
			int num3 = taskObjective.Get<int>("Quantity");
			string text2 = "";
			text2 = ((!_ShowRewardItemQuantity) ? num3.ToString() : (num2 + "/" + num3));
			Color color = _RequiredCompleteColor;
			if (num3 > num2)
			{
				color = _RequiredColor;
			}
			if (!(widget != null))
			{
				continue;
			}
			KAWidget kAWidget = widget.FindChildItem("TxtRequired" + i.ToString("d2"));
			if (kAWidget != null)
			{
				if (flag)
				{
					UILabel label = kAWidget.GetLabel();
					if (label != null)
					{
						label.color = color;
					}
					if (_ShowRewardItemName)
					{
						kAWidget.SetText(text + " " + text2);
					}
					else
					{
						kAWidget.SetText(text2);
					}
					kAWidget.SetVisibility(inVisible: true);
				}
				else
				{
					kAWidget.SetVisibility(inVisible: false);
				}
			}
			KAWidget kAWidget2 = widget.FindChildItem("RequiredIcon" + i.ToString("d2"));
			if (kAWidget2 != null)
			{
				if (flag)
				{
					kAWidget2.SetTextureFromBundle(userItemData.Item.IconName);
					kAWidget2.SetVisibility(inVisible: true);
				}
				else
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
			}
			if (!flag && kAWidget2 != null && kAWidget != null)
			{
				new MissionItemDataLoader(num, kAWidget2, kAWidget, text2, color, _ShowRewardItemName);
			}
		}
	}

	protected virtual void AddTaskFromList(List<Task> inTaskList)
	{
		bool flag = JobBoardData.pInstance.Slots.Length < _TaskToLoad && JobBoardData.pInstance.Slots.Length < inTaskList.Count;
		int i;
		for (i = 0; i < JobBoardData.pInstance.Slots.Length; i++)
		{
			JobBoardSlot jobBoardSlot = JobBoardData.pInstance.Slots[i];
			if (jobBoardSlot.TaskID > 0)
			{
				Task task = MissionManager.pInstance.GetTask(jobBoardSlot.TaskID);
				if (!inTaskList.Contains(task))
				{
					task = null;
				}
				if (task == null)
				{
					Task newTask = GetNewTask();
					if (newTask != null)
					{
						JobBoardData.AddTaskID(i, newTask.TaskID);
						AddTaskWidget(newTask);
						mSaveJobBoardData = true;
					}
					else
					{
						UpdateJobBoardData(i);
						i--;
					}
				}
				else
				{
					AddTaskWidget(task);
				}
			}
			else if (IsTaskWaitTimeOver(i))
			{
				Task newTask2 = GetNewTask();
				if (newTask2 != null)
				{
					JobBoardData.AddTaskID(i, newTask2.TaskID);
					AddTaskWidget(newTask2);
					mSaveJobBoardData = true;
				}
				else
				{
					UpdateJobBoardData(i);
					i--;
				}
			}
			else
			{
				KAWidget kAWidget = mMenu.AddWidget("WaitingTask");
				kAWidget.SetUserDataInt(jobBoardSlot.TaskID);
				mMenu.SetupTask(kAWidget, markForCompletion: false);
			}
		}
		if (!flag)
		{
			return;
		}
		int num = _TaskToLoad;
		if (num > inTaskList.Count)
		{
			num = inTaskList.Count;
		}
		JobBoardData pInstance = JobBoardData.pInstance;
		JobBoardData.Init(num);
		JobBoardData.pInstance.LastTrashedTaskID = pInstance.LastTrashedTaskID;
		for (int j = 0; j < pInstance.Slots.Length; j++)
		{
			JobBoardData.pInstance.Slots[j] = pInstance.Slots[j];
		}
		for (; i < num; i++)
		{
			Task newTask3 = GetNewTask();
			if (newTask3 != null)
			{
				JobBoardData.AddTaskID(i, newTask3.TaskID);
				AddTaskWidget(newTask3);
				mSaveJobBoardData = true;
			}
		}
	}

	public bool IsTaskWaitTimeOver(int index)
	{
		if (JobBoardData.pInstance.Slots[index].CompletionTime.HasValue)
		{
			int num = mCurrentJobCategory._WaitTimeForTaskInSec;
			if (JobBoardData.pInstance.Slots[index].TaskID < 0)
			{
				num = mCurrentJobCategory._TrashTimeForTaskInSec;
			}
			DateTime value = JobBoardData.pInstance.Slots[index].CompletionTime.Value;
			return (ServerTime.pCurrentTime - value).TotalSeconds > (double)num;
		}
		return true;
	}

	public Task GetNewTask()
	{
		List<Task> offeredTasks = GetOfferedTasks(mCurrentJobCategory._MissionGroupID);
		UtUtilities.Shuffle(offeredTasks);
		int num = 0;
		if (JobBoardData.pInstance.LastTrashedTaskID.HasValue)
		{
			num = JobBoardData.pInstance.LastTrashedTaskID.Value;
		}
		Task task = null;
		foreach (Task item in offeredTasks)
		{
			if (JobBoardData.GetJobBoardSlotFromTaskID(item.TaskID) != null)
			{
				continue;
			}
			if (num == item.TaskID)
			{
				task = item;
				continue;
			}
			if (JobBoardData.pInstance.LastTrashedTaskID.HasValue)
			{
				JobBoardData.pInstance.LastTrashedTaskID = null;
			}
			return item;
		}
		if (task != null)
		{
			JobBoardData.pInstance.LastTrashedTaskID = null;
		}
		return task;
	}

	private void SetupDetails()
	{
		if (mAcceptBtn != null)
		{
			mAcceptBtn.SetVisibility(MarkTaskForCompletion(pCurrentTask));
		}
		if (mTrashBtn != null)
		{
			mTrashBtn.SetVisibility(inVisible: true);
		}
		KAWidget kAWidget;
		if (pCurrentTask.pData.Title != null)
		{
			string localizedString = pCurrentTask.pData.Title.GetLocalizedString();
			kAWidget = FindItem("TxtQuestHeading");
			if (kAWidget != null)
			{
				kAWidget.SetText(localizedString);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		else
		{
			UtDebug.LogError("No title found for TaskID: " + pCurrentTask.TaskID);
		}
		if (pCurrentTask.pData.Description != null)
		{
			string localizedString2 = pCurrentTask.pData.Description.GetLocalizedString();
			kAWidget = FindItem("TxtObjectiveStatic");
			if (kAWidget != null)
			{
				kAWidget.SetText(localizedString2);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		else
		{
			UtDebug.LogError("No description found for TaskID: " + pCurrentTask.TaskID);
		}
		kAWidget = FindItem("TxtRewards");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
		}
		Mission mission = pCurrentTask._Mission;
		while (mission._Parent != null)
		{
			mission = mission._Parent;
		}
		if (_XPRewardWidget != null)
		{
			_XPRewardWidget.SetRewards(mission.Rewards.ToArray(), MissionManager.pInstance._RewardData);
		}
		kAWidget = FindItem("TxtRequired");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
		}
		if (pCurrentTask.pData.Objectives != null)
		{
			KAWidget kAWidget2 = FindItem("Required" + pCurrentTask.pData.Objectives.Count.ToString("d2"));
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: true);
			}
		}
	}

	public void SetSelectedTask(Task inTask)
	{
		pCurrentTask = inTask;
		HideTaskDetails();
		SetupDetails();
		KAWidget widget = FindItem("Required" + pCurrentTask.pData.Objectives.Count.ToString("d2"));
		AddRequiredItems(widget, inTask);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnBack")
		{
			OnClose();
		}
		else if (inWidget == mAcceptBtn)
		{
			OnAcceptMission();
		}
		else if (inWidget == mTrashBtn)
		{
			OnTrashMission();
		}
	}

	protected virtual void OnClose()
	{
		if (mSaveJobBoardData)
		{
			SaveJobBoardData();
		}
		mMenu.Reset();
		mCategoryMenu.ClearItems();
		KAUI.RemoveExclusive(this);
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
		if (base.transform.parent != null)
		{
			UnityEngine.Object.Destroy(base.transform.parent.gameObject);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public bool MarkTaskForCompletion(Task inTask)
	{
		if (inTask.pData.Type == "Delivery" && inTask.pData.Objectives.Count > 0)
		{
			foreach (TaskObjective objective in inTask.pData.Objectives)
			{
				int num = objective.Get<int>("ItemID");
				if (num > 0)
				{
					UserItemData userItemData = CommonInventoryData.pInstance.FindItem(num);
					int num2 = 0;
					if (userItemData != null)
					{
						num2 = userItemData.Quantity;
						int num3 = objective.Get<int>("Quantity");
						if (num2 < num3)
						{
							return false;
						}
						continue;
					}
					return false;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public virtual void OnAcceptMission()
	{
		SetInteractive(interactive: false);
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.pActiveTasks.Add(pCurrentTask);
		}
		pCurrentTask.pPayload.Started = true;
		pCurrentTask.Start();
		if (pCurrentTask.CheckForCompletion(pCurrentTask.pData.Type, pCurrentTask.pData.Objectives[0].Get<string>("NPC"), "", ""))
		{
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForCompletion();
			}
			mMenu.OnTaskComplete(pCurrentTask.Name, isTrashed: false);
			HideTaskDetails();
		}
	}

	public virtual void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (inEvent == MissionEvent.MISSION_REWARDS_COMPLETE)
		{
			mMissionRewardsComplete = true;
		}
	}

	private void LateUpdate()
	{
		if (mMissionRewardsComplete)
		{
			mMissionRewardsComplete = false;
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			SetInteractive(interactive: true);
		}
	}

	public void DestroyDB()
	{
		if (mUiGenericDB != null)
		{
			KAUI.RemoveExclusive(mUiGenericDB);
			UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
			mUiGenericDB = null;
		}
	}

	private void HideTaskDetails()
	{
		if (mAcceptBtn != null)
		{
			mAcceptBtn.SetVisibility(inVisible: false);
		}
		if (mTrashBtn != null)
		{
			mTrashBtn.SetVisibility(inVisible: false);
		}
		KAWidget kAWidget = FindItem("TxtQuestHeading");
		if (kAWidget != null)
		{
			kAWidget.SetText("");
			kAWidget.SetVisibility(inVisible: false);
		}
		kAWidget = FindItem("TxtObjectiveStatic");
		if (kAWidget != null)
		{
			kAWidget.SetText("");
			kAWidget.SetVisibility(inVisible: false);
		}
		kAWidget = FindItem("TxtRewards");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		kAWidget = FindItem("TxtRequired");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		kAWidget = FindItem("XPReward");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		for (int i = 1; i <= 3; i++)
		{
			KAWidget kAWidget2 = FindItem("Required" + i.ToString("d2"));
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: false);
			}
		}
	}

	public void UpdateJobBoardData(int index)
	{
		JobBoardData pInstance = JobBoardData.pInstance;
		JobBoardData.Init(pInstance.Slots.Length - 1);
		JobBoardData.pInstance.LastTrashedTaskID = pInstance.LastTrashedTaskID;
		for (int i = 0; i < pInstance.Slots.Length; i++)
		{
			if (i < index)
			{
				JobBoardData.pInstance.Slots[i] = pInstance.Slots[i];
			}
			else if (i > index)
			{
				JobBoardData.pInstance.Slots[i - 1] = pInstance.Slots[i];
			}
		}
		mSaveJobBoardData = true;
	}

	public void SaveJobBoardData()
	{
		mSaveJobBoardData = false;
		string jobBoardDataString = JobBoardData.GetJobBoardDataString();
		if (!string.IsNullOrEmpty(jobBoardDataString))
		{
			ProductData.pPairData.SetValueAndSave("JBD_" + mCurrentJobCategory._MissionGroupID, jobBoardDataString);
		}
	}

	private void OnTrashMission()
	{
		SetInteractive(interactive: false);
		KAWidget selectedItem = mMenu.GetSelectedItem();
		selectedItem.SetInteractive(isInteractive: false);
		int userDataInt = selectedItem.GetUserDataInt();
		Task task = MissionManager.pInstance.GetTask(userDataInt);
		HideTaskDetails();
		mMenu.OnTaskComplete(task.Name, isTrashed: true);
	}

	public string FormatTimeHHMMSS(int inSeconds, string inDelemeter = ":")
	{
		int num = inSeconds % 60;
		int num2 = inSeconds - num;
		int num3 = num2 / 60 % 60;
		int num4 = (num2 - num3 * 60) / 3600;
		string text = string.Empty;
		if (num4 > 0)
		{
			text += num4.ToString("d2");
			text += inDelemeter;
		}
		text += num3.ToString("d2");
		text += inDelemeter;
		return text + num.ToString("d2");
	}

	public void ShowInstantFillOption()
	{
		HideTaskDetails();
		ShowInstantFillPurchaseDB();
	}

	private void OnStoreLoaded(StoreData sd)
	{
		SetInteractive(interactive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		ItemData itemData = sd.FindItem(_InstantFillItemID);
		if (itemData != null)
		{
			mInstantFillCost = itemData.FinalCashCost;
		}
	}

	private void ShowInstantFillPurchaseDB()
	{
		LocaleString localeString = new LocaleString(_InstantFillFeeText.GetLocalizedString().Replace("{{GEMS}}", mInstantFillCost.ToString()));
		localeString._ID = 0;
		ShowKAUIDialog("PfKAUIGenericDB", "Instant Fill", "PayGemsAndInstantFill", "DestroyDB", "", "", destroyDB: true, localeString, base.gameObject);
	}

	private void PayGemsAndInstantFill()
	{
		DestroyDB();
		if (Money.pCashCurrency >= mInstantFillCost)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			PurchaseInstantFill();
		}
		else
		{
			ShowKAUIDialog("PfKAUIGenericDB", "Insufficient Gems", "BuyGemsOnline", "DestroyDB", "", "", destroyDB: true, _NotEnoughFeeText, base.gameObject);
		}
	}

	private void PurchaseInstantFill()
	{
		CommonInventoryData.pInstance.AddPurchaseItem(_InstantFillItemID, 1, ItemPurchaseSource.JOB_BOARD.ToString());
		CommonInventoryData.pInstance.DoPurchase(2, _InstantFillStoreID, InstantFillPurchaseDone);
		ShowKAUIDialog("PfKAUIGenericDB", "Purchase Progress", "", "", "", "", destroyDB: true, _PurchaseProcessingText);
	}

	public void InstantFillPurchaseDone(CommonInventoryResponse ret)
	{
		DestroyDB();
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (ret != null && ret.Success)
		{
			ShowKAUIDialog("PfKAUIGenericDB", "Purchase Success", "", "", "DestroyDB", "", destroyDB: true, _InstantFillPurchaseSuccessText, base.gameObject);
			KAWidget selectedItem = mMenu.GetSelectedItem();
			if (selectedItem != null)
			{
				mMenu.AddNewTask(selectedItem, mMenu.GetSelectedItemIndex());
			}
			else
			{
				UtDebug.LogError("No widget selected!!!");
			}
		}
		else
		{
			ShowKAUIDialog("PfKAUIGenericDB", "Purchase Falied", "", "", "DestroyDB", "", destroyDB: true, _InstantFillPurchaseFailText, base.gameObject);
		}
	}

	private void BuyGemsOnline()
	{
		DestroyDB();
		SetVisibility(inVisible: false);
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		SetVisibility(inVisible: true);
	}

	private void ShowKAUIDialog(string assetName, string dbName, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, LocaleString localeString, GameObject msgObject = null)
	{
		if (mUiGenericDB != null)
		{
			DestroyDB();
		}
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mUiGenericDB != null)
		{
			if (msgObject == null)
			{
				msgObject = base.gameObject;
			}
			mUiGenericDB.SetMessage(msgObject, yesMessage, noMessage, okMessage, closeMessage);
			mUiGenericDB.SetDestroyOnClick(destroyDB);
			mUiGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mUiGenericDB.SetTextByID(localeString._ID, localeString._Text, interactive: false);
			KAUI.SetExclusive(mUiGenericDB);
		}
	}

	public void ShowNoJobMessage()
	{
		KAWidget kAWidget = FindItem("TxtObjectiveStatic");
		if (kAWidget != null)
		{
			kAWidget.SetText(_JobNotAvailableText.GetLocalizedString());
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	private void HideIconsInRewardGroup(KAWidget inParent)
	{
		if (!(inParent == null))
		{
			KAWidget kAWidget = inParent.FindChildItem("IcoXPPlayer");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			kAWidget = inParent.FindChildItem("IcoCoins");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			kAWidget = inParent.FindChildItem("IcoGems");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			kAWidget = inParent.FindChildItem("IcoItem");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			kAWidget = inParent.FindChildItem("IcoXPDragon");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			kAWidget = inParent.FindChildItem("IcoXPFarming");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			kAWidget = inParent.FindChildItem("IcoXPFishing");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}
}
