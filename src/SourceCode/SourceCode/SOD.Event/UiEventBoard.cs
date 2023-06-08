using System;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.Event;

public class UiEventBoard : UiJobBoard
{
	[Serializable]
	public class MissionRewardMap
	{
		public int _MissionID;

		public LocaleString _RewardName;

		public string _RewardIcon;

		public KAWidget _Widget;
	}

	[Serializable]
	public class ItemData
	{
		public int _ItemID;

		public KAWidget _Widget;

		public UserItemData Item { get; set; }
	}

	public delegate void OnLoaded(UiEventBoard inUiEventBoard);

	[Header("Event Naem")]
	public string _EventName;

	[Header("UI")]
	public GameObject _HelpScreen;

	public LocaleString _GracePeriodText;

	[Header("Mission")]
	public List<MissionRewardMap> _RewardMap;

	[Header("Items")]
	public List<ItemData> _Items;

	[Header("Reward UI")]
	public string _RewardAssetPath;

	[Header("Particle Effect")]
	public ParticleSystem _CollectEffect;

	private EventManager mEventManager;

	private UiHelpScreen mHelpScreen;

	private KAWidget mBtnHowToPlay;

	private KAWidget mBtnExchange;

	private KAWidget mTxtTimer;

	private KAWidget mTxtObjective;

	public Action OnClosed;

	public static void Load(string eventName, OnLoaded onLoaded = null)
	{
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(EventManager.Get(eventName)._AssetName, OnBundleLoaded, typeof(GameObject), inDontDestroy: false, onLoaded);
	}

	private static void OnBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			UiEventBoard component = obj.GetComponent<UiEventBoard>();
			((OnLoaded)inUserData)?.Invoke(component);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			((OnLoaded)inUserData)?.Invoke(null);
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading Event Board Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected override void Start()
	{
		base.Start();
		mEventManager = EventManager.Get(_EventName);
		mBtnHowToPlay = FindItem("BtnHowToPlay");
		mBtnExchange = FindItem("BtnExchange");
		mTxtTimer = FindItem("TxtTimer");
		mTxtObjective = FindItem("TxtObjective");
		CheckInventory();
		DisplayEndDate();
		if (mEventManager != null)
		{
			mBtnHowToPlay?.SetVisibility(!mEventManager.GracePeriodInProgress());
		}
		if (mEventManager != null && mEventManager.GracePeriodInProgress())
		{
			mTxtObjective?.SetText(_GracePeriodText.GetLocalizedString());
		}
	}

	protected void OnDisable()
	{
		if (mHelpScreen != null)
		{
			mHelpScreen.OnClicked -= HelpUiButtonClicked;
		}
	}

	protected override void InitMenu()
	{
		mCategoryMenu = (UiJobBoardCategoryMenu)GetMenu("UiJobBoardCategoryMenu");
		if (mCategoryMenu != null)
		{
			mCategoryMenu.Init(this);
		}
		mMenu = (UiEventBoardMenu)GetMenu("UiEventBoardMenu");
		if (mMenu != null)
		{
			mMenu.Init(this);
		}
		UpdateItems();
	}

	protected override void OnClose()
	{
		OnClosed?.Invoke();
		base.OnClose();
		if (mHelpScreen != null)
		{
			UnityEngine.Object.Destroy(mHelpScreen.gameObject);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnHowToPlay)
		{
			OpenHelp();
		}
		else if (inWidget == mBtnExchange)
		{
			UiExchange.Load(_EventName);
			OnClose();
		}
		else if (inWidget.name == "BtnPrizeCollect")
		{
			mMenu.OnClick(inWidget);
		}
	}

	public void OpenHelp()
	{
		if (mHelpScreen == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_HelpScreen);
			mHelpScreen = gameObject.GetComponent<UiHelpScreen>();
			mHelpScreen.OnClicked += HelpUiButtonClicked;
		}
		SetVisibility(inVisible: false);
		mHelpScreen.SetVisibility(inVisible: true);
		KAUI.SetExclusive(mHelpScreen);
	}

	private void HelpUiButtonClicked(string buttonName)
	{
		if (!(buttonName == "Back"))
		{
			if (buttonName == "Exit")
			{
				OnClose();
			}
		}
		else
		{
			SetVisibility(inVisible: true);
			KAUI.SetExclusive(this);
			mHelpScreen.OnClicked -= HelpUiButtonClicked;
		}
	}

	protected void UpdateTask(KAWidget widget, Task task)
	{
		if (task.pData.Objectives == null)
		{
			return;
		}
		KAWidget kAWidget = widget.FindChildItem("Required" + task.pData.Objectives.Count.ToString("d2"));
		if (kAWidget == null)
		{
			return;
		}
		bool flag = MissionManager.pInstance.CanCompleteTask(task, "Delivery");
		if (!task.pCompleted)
		{
			kAWidget.SetVisibility(inVisible: true);
			AddRequiredItems(kAWidget, task);
			KAWidget kAWidget2 = widget.FindChildItem("BkgsLegendary");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(flag);
			}
		}
		SetCollectButton(widget, task, flag);
	}

	public override void OnAcceptMission()
	{
		SetInteractive(interactive: false);
		foreach (MissionRewardMap item in _RewardMap)
		{
			if (item._Widget != null)
			{
				KAWidget kAWidget = item._Widget.FindChildItem("BtnPrizeCollect");
				if (kAWidget != null)
				{
					kAWidget.SetDisabled(isDisabled: true);
				}
			}
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.pActiveTasks.Add(base.pCurrentTask);
		}
		base.pCurrentTask.pPayload.Started = true;
		base.pCurrentTask.Start();
		if (base.pCurrentTask.CheckForCompletion(base.pCurrentTask.pData.Type, base.pCurrentTask.pData.Objectives[0].Get<string>("NPC"), "", ""))
		{
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForCompletion();
			}
			mMenu.OnTaskComplete(base.pCurrentTask.Name, isTrashed: false);
		}
	}

	protected override List<Task> GetOfferedTasks(int inMissionGroupID)
	{
		List<Mission> allMissions = MissionManager.pInstance.GetAllMissions(inMissionGroupID);
		List<Task> list = new List<Task>();
		if (allMissions != null && allMissions.Count > 0)
		{
			foreach (MissionRewardMap reward in _RewardMap)
			{
				Mission mission = allMissions.Find((Mission x) => x.MissionID == reward._MissionID);
				if (MissionManager.pInstance.IsLocked(mission) || mission.Tasks == null || mission.Tasks.Count <= 0)
				{
					continue;
				}
				foreach (Task task in mission.Tasks)
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

	public override void LoadMission()
	{
		((MissionManagerDO)MissionManager.pInstance).LoadMissionData(base.pCurrentJobCategory._MissionGroupID, OnMissionDataLoad, -1, loadCompleted: true);
	}

	private void SetCollectButton(KAWidget widget, Task task, bool state)
	{
		KAWidget kAWidget = widget.FindChildItem("BtnPrizeCollect");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(!task.pCompleted);
			kAWidget.SetDisabled(!state);
			kAWidget.SetUserDataInt(task.TaskID);
		}
		KAToggleButton kAToggleButton = (KAToggleButton)widget.FindChildItem("MissionState");
		if (kAToggleButton != null)
		{
			kAToggleButton.SetVisibility(task.pCompleted);
			kAToggleButton.SetChecked(task.pCompleted);
		}
		KAWidget kAWidget2 = widget.FindChildItem("Claimed");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(task.pCompleted);
		}
	}

	protected override void AddTaskFromList(List<Task> inTaskList)
	{
		for (int i = 0; i < JobBoardData.pInstance.Slots.Length - 1; i++)
		{
			JobBoardSlot jobBoardSlot = JobBoardData.pInstance.Slots[i];
			if (jobBoardSlot.TaskID <= 0)
			{
				continue;
			}
			Task task = MissionManager.pInstance.GetTask(jobBoardSlot.TaskID);
			if (!inTaskList.Contains(task))
			{
				task = null;
			}
			if (task != null)
			{
				KAWidget kAWidget = mMenu.AddWidget(task.Name);
				kAWidget.SetUserDataInt(task.TaskID);
				if (!task.pCompleted && mMenu.GetDefaultFocusIndex() == -1)
				{
					mMenu.SetDefaultFocusIndex(i);
				}
				UpdateTask(kAWidget, task);
				if (_RewardMap[i]._Widget == null)
				{
					_RewardMap[i]._Widget = kAWidget;
				}
				SetIcon(kAWidget, _RewardMap[i]);
			}
		}
		if (_RewardMap[_RewardMap.Count - 1] != null)
		{
			KAWidget widget = _RewardMap[_RewardMap.Count - 1]._Widget;
			Task task2 = inTaskList[_RewardMap.Count - 1];
			if (task2 != null && !(widget == null))
			{
				widget.SetUserDataInt(task2.TaskID);
				SetIcon(widget, _RewardMap[_RewardMap.Count - 1]);
				UpdateTask(widget, task2);
			}
		}
	}

	protected void SetIcon(KAWidget widget, MissionRewardMap mission)
	{
		KAWidget kAWidget = widget.FindChildItem("BkgIcon");
		CoBundleItemData coBundleItemData = new CoBundleItemData(mission._RewardIcon, null);
		if (kAWidget != null)
		{
			coBundleItemData._Item = kAWidget;
			coBundleItemData.LoadResource();
		}
		KAWidget kAWidget2 = widget.FindChildItem("TxtPrizeName");
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(mission._RewardName.GetLocalizedString());
		}
	}

	public override void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		base.OnMissionEvent(inEvent, inObject);
		if (inEvent == MissionEvent.MISSION_REWARDS_COMPLETE)
		{
			Mission mission = (Mission)inObject;
			if (mission != null && _RewardMap.Find((MissionRewardMap x) => x._MissionID == mission.MissionID) != null)
			{
				RefreshUI(mission);
			}
		}
	}

	protected void UpdateItems()
	{
		foreach (ItemData item in _Items)
		{
			item.Item = CommonInventoryData.pInstance.FindItem(item._ItemID);
			if (item._Widget != null)
			{
				item._Widget.SetText((item.Item != null) ? item.Item.Quantity.ToString() : "0");
			}
		}
	}

	protected void RefreshUI(Mission completedMission)
	{
		foreach (MissionRewardMap item in _RewardMap)
		{
			if (item._Widget != null)
			{
				int userDataInt = item._Widget.GetUserDataInt();
				Task task = MissionManager.pInstance.GetTask(userDataInt);
				UpdateTask(item._Widget, task);
			}
			if (completedMission.MissionID == item._MissionID && item._Widget != null && _CollectEffect != null)
			{
				KAWidget kAWidget = item._Widget.FindChildItem("BkgIcon");
				if (!(kAWidget == null))
				{
					_CollectEffect.transform.parent = kAWidget.transform;
					_CollectEffect.transform.localPosition = Vector3.zero;
					_CollectEffect.Play();
				}
			}
		}
		UpdateItems();
	}

	private void DisplayEndDate()
	{
		int num = ((mEventManager != null) ? mEventManager.GetRemainingDays() : 0);
		string text = mTxtTimer.GetText().Replace("{#}", num.ToString());
		mTxtTimer.SetText(text);
	}

	private void CheckInventory()
	{
		if (mEventManager != null && mEventManager.CanOpenMysteryBox())
		{
			LoadRewardScreen();
		}
	}

	private void LoadRewardScreen()
	{
		if (!string.IsNullOrEmpty(_RewardAssetPath))
		{
			SetVisibility(inVisible: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = _RewardAssetPath.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadObjectEvent, typeof(GameObject));
		}
	}

	private void LoadObjectEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			UiLootBox component = UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<UiLootBox>();
			if (component != null)
			{
				component.OnClosed = OnRedeemLootBoxClosed;
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			SetVisibility(inVisible: true);
			OnClosed?.Invoke();
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.LogError("Prefab failed to load for url : " + inURL);
			break;
		}
	}

	private void OnRedeemLootBoxClosed()
	{
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this);
		UpdateItems();
	}
}
