using System;
using System.Collections.Generic;
using UnityEngine;

public class UiNPCMissionBoard : KAUITreeListMenu
{
	private class QuestUserData : KAUITreeListItemData
	{
		public int _TaskID;

		public OnTaskSelected _TaskHandler;

		public QuestUserData(KAUITreeListItemData inParent, string inName, LocaleString inLocaleString, bool inCollapsed, List<KAUITreeListItemData> inChildList, int typeId, OnTaskSelected inTaskHandler)
			: base(inParent, inName, inLocaleString, inCollapsed, inChildList)
		{
			_TaskID = typeId;
			_TaskHandler = inTaskHandler;
		}
	}

	public delegate void OnClose();

	public delegate bool OnTaskSelected(int inTaskId);

	public KAButton _BtnClose;

	public KAWidget _TxtTitle;

	public KAUI _ExtendedUI;

	private OnClose mOnCloseDelegate;

	private UiNPCQuestDetails mQuestDetailsUi;

	private KAWidget mBackground;

	protected override void Start()
	{
		if (_BackgroundObject != null)
		{
			mBackground = _BackgroundObject.GetComponent<KAWidget>();
		}
		base.Start();
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "NPCMissionBoardLoaded");
		}
		if (_ExtendedUI != null)
		{
			_ExtendedUI.pEvents.OnClick += OnExtendedUiClick;
		}
	}

	public void OnExtendedUiClick(KAWidget inWidget)
	{
		if (_BtnClose != null && inWidget.name == _BtnClose.name)
		{
			if (mOnCloseDelegate != null)
			{
				mOnCloseDelegate();
			}
			else
			{
				SetVisibility(inVisibility: false);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!(inWidget.GetUserData() is QuestUserData questUserData))
		{
			return;
		}
		if (questUserData._TaskHandler == null || questUserData._TaskHandler(questUserData._TaskID))
		{
			ShowQuestDetails(questUserData._TaskID);
			if (InteractiveTutManager._CurrentActiveTutorialObject != null)
			{
				InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "MissionBoardTaskClicked");
			}
		}
		else
		{
			SetVisibility(inVisibility: false);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (UtPlatform.IsAndroid() && KAUI._GlobalExclusiveUI != this && GetVisibility() && GetState() == KAUIState.INTERACTIVE && Input.GetKeyUp(KeyCode.Escape) && _BtnClose != null)
		{
			OnExtendedUiClick(_BtnClose);
		}
	}

	public override void SetVisibility(bool inVisibility)
	{
		base.SetVisibility(inVisibility);
		if (_BtnClose != null)
		{
			_BtnClose.SetVisibility(inVisibility);
		}
		if (_TxtTitle != null)
		{
			_TxtTitle.SetVisibility(inVisibility);
		}
		if (mBackground != null)
		{
			mBackground.SetVisibility(inVisibility);
		}
	}

	public void AddTask(Task inTask, OnTaskSelected inTaskHandler)
	{
		KAUITreeListItemData item = GetItem(null, inTask, inTaskHandler);
		AddItem("", item, inRefreshTree: false);
	}

	private KAUITreeListItemData GetItem(KAUITreeListItemData inParent, Task inTask, OnTaskSelected inTaskHandler)
	{
		string taskName = GetTaskName(inTask);
		return new QuestUserData(inParent, taskName, new LocaleString(taskName), inCollapsed: true, null, inTask.TaskID, inTaskHandler);
	}

	private string GetTaskName(Task inTask)
	{
		string localizedString = inTask.Name;
		Mission mission = null;
		for (Mission mission2 = inTask._Mission; mission2 != null; mission2 = mission2._Parent)
		{
			mission = mission2;
		}
		if (mission != null && mission.pData != null && mission.pData.Title != null)
		{
			localizedString = mission.pData.Title.GetLocalizedString();
		}
		else if (!inTask._Mission.MissionRule.Criteria.Ordered && inTask._Mission.pData != null && inTask._Mission.pData.Title != null)
		{
			localizedString = inTask._Mission.pData.Title.GetLocalizedString();
		}
		else if (inTask.pData != null && inTask.pData.Title != null)
		{
			localizedString = inTask.pData.Title.GetLocalizedString();
		}
		return MissionManager.pInstance.FormatText(0, localizedString);
	}

	public void ShowQuestDetails(int inTaskId)
	{
		Task task = MissionManager.pInstance.GetTask(inTaskId);
		if (task != null)
		{
			ShowQuestDetails(task);
		}
	}

	public void ShowQuestDetails(Task inTask)
	{
		ShowQuestDetails(inTask, OnQuestDetailsClose);
	}

	public void ShowQuestDetails(Task inTask, UiNPCQuestDetails.OnClose inOnCloseDelegate)
	{
		if (inTask != null)
		{
			if (mQuestDetailsUi == null)
			{
				mQuestDetailsUi = base.transform.root.GetComponentInChildren<UiNPCQuestDetails>();
			}
			if (mQuestDetailsUi != null)
			{
				SetVisibility(inVisibility: false);
				mQuestDetailsUi.SetVisibility(inVisible: true);
				mQuestDetailsUi.ShowTaskDetails(inTask, inOnCloseDelegate);
			}
		}
	}

	public void OnQuestDetailsClose()
	{
		if (mQuestDetailsUi != null)
		{
			mQuestDetailsUi.SetVisibility(inVisible: false);
			if (mOnCloseDelegate != null)
			{
				mOnCloseDelegate();
			}
			else
			{
				SetVisibility(inVisibility: false);
			}
		}
	}

	public void AddCloseButtonHandler(OnClose inHandler)
	{
		mOnCloseDelegate = (OnClose)Delegate.Combine(mOnCloseDelegate, inHandler);
	}

	public void RemoveCloseButtonHandler(OnClose inHandler)
	{
		mOnCloseDelegate = (OnClose)Delegate.Remove(mOnCloseDelegate, inHandler);
	}

	public static UiNPCMissionBoard CreateNPCMissionBoard(string inAssetName)
	{
		GameObject obj = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(inAssetName));
		obj.name = inAssetName;
		return obj.GetComponentInChildren<UiNPCMissionBoard>();
	}

	public static void DestroyNPCMissionBoard(UiNPCMissionBoard inMissionBoard)
	{
		if (inMissionBoard != null)
		{
			GameObject gameObject = inMissionBoard.transform.root.gameObject;
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			RsResourceManager.UnloadUnusedAssets();
		}
	}

	public virtual void PopulateItems(List<Task> tasksToOffer, OnClose inHandler)
	{
		if (tasksToOffer == null)
		{
			return;
		}
		foreach (Task item in tasksToOffer)
		{
			AddTask(item, null);
		}
		PopulateTreeList();
		for (int i = 0; i < mItemInfo.Count; i++)
		{
			KAWidget kAWidget = mItemInfo[i];
			if (tasksToOffer[i].pPayload.Started)
			{
				kAWidget.SetInteractive(isInteractive: false);
				KAToggleButton kAToggleButton = (KAToggleButton)kAWidget.FindChildItem("MissionStatus");
				if (kAToggleButton != null)
				{
					kAToggleButton.SetVisibility(inVisible: false);
				}
			}
			KAWidget kAWidget2 = kAWidget.FindChildItem("BkgIcon");
			if (!(kAWidget2 != null))
			{
				continue;
			}
			Mission rootMission = MissionManager.pInstance.GetRootMission(tasksToOffer[i]);
			string text = ((rootMission != null) ? rootMission.pData.Icon : tasksToOffer[i]._Mission.pData.Icon);
			kAWidget2.SetVisibility(inVisible: false);
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			if (text.StartsWith("http://"))
			{
				kAWidget2.SetTextureFromURL(text, base.gameObject);
				continue;
			}
			string[] array = text.Split('/');
			if (array.Length >= 3)
			{
				kAWidget2.SetTextureFromBundle(array[0] + "/" + array[1], array[2], base.gameObject);
			}
		}
		AddCloseButtonHandler(inHandler);
		SetVisibility(inVisibility: true);
	}

	private void OnTextureLoaded(KAWidget widget)
	{
		widget.SetVisibility(inVisible: true);
	}
}
