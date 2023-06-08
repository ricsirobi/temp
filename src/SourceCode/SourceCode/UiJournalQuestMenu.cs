using System;
using System.Collections.Generic;

public class UiJournalQuestMenu : KAUITreeListMenu
{
	private class QuestUserData : KAUITreeListItemData
	{
		public int _MissionId;

		public QuestUserData(KAUITreeListItemData inParent, string inName, LocaleString inLocaleString, bool inCollapsed, List<KAUITreeListItemData> inChildList, int missionId)
			: base(inParent, inName, inLocaleString, inCollapsed, inChildList)
		{
			_MissionId = missionId;
		}
	}

	private bool mInitialized;

	private UiJournalQuest pParentUI => (UiJournalQuest)_ParentUi;

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	protected override void Update()
	{
		if (MissionManager.pIsReady && !mInitialized)
		{
			Initialize();
		}
	}

	public void Initialize()
	{
		if (MissionManager.pIsReady)
		{
			RefreshQuestList(-1);
			mInitialized = true;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.GetUserData() is QuestUserData questUserData && pParentUI != null)
		{
			pParentUI.ShowQuestDetails(questUserData._MissionId);
		}
	}

	public void RefreshQuestList(int inGroupId)
	{
		List<Mission> activeMissions = GetActiveMissions(inGroupId);
		ClearItems();
		PopulateMissionItems(activeMissions);
	}

	private List<Mission> GetActiveMissions(int inGroupId)
	{
		List<Mission> missionsList = new List<Mission>();
		if (MissionManager.pInstance != null && MissionManager.pInstance.pActiveTasks != null)
		{
			foreach (Task pActiveTask in MissionManager.pInstance.pActiveTasks)
			{
				Mission rootMission = MissionManager.pInstance.GetRootMission(pActiveTask);
				while (rootMission._Parent != null)
				{
					rootMission = MissionManager.pInstance.GetRootMission(rootMission._Parent);
				}
				if (((Predicate<Mission>)((Mission m) => (inGroupId == -1 || m.GroupID == inGroupId) && !m.pData.Hidden && !missionsList.Exists((Mission a) => a.MissionID == m.MissionID)))(rootMission))
				{
					if (MissionManagerDO._QuestArrow != null && MissionManagerDO._QuestArrow.pCurrentActiveTask != null && MissionManagerDO._QuestArrow.pCurrentActiveTask.TaskID == pActiveTask.TaskID)
					{
						missionsList.Insert(0, rootMission);
					}
					else
					{
						missionsList.Add(rootMission);
					}
				}
			}
		}
		return missionsList;
	}

	private KAUITreeListItemData GetItem(KAUITreeListItemData inParent, Mission inMission)
	{
		string missionName = GetMissionName(inMission);
		List<Mission> missions = new List<Mission>();
		QuestUserData questUserData = inParent as QuestUserData;
		Mission mission = null;
		if (questUserData != null)
		{
			mission = MissionManager.pInstance.GetMission(questUserData._MissionId);
		}
		else if (inMission._Parent != null)
		{
			mission = inMission._Parent;
		}
		if (mission != null)
		{
			MissionManager.pInstance.GetNextMission(mission, completed: false, ref missions);
		}
		return new QuestUserData(inParent, missionName, new LocaleString(missionName), inCollapsed: true, null, inMission.MissionID);
	}

	private List<Mission> GetChildMissions(Mission mission)
	{
		List<Mission> list = new List<Mission>();
		if (mission != null && mission.MissionRule.Criteria.RuleItems != null)
		{
			foreach (RuleItem ruleItem in mission.MissionRule.Criteria.RuleItems)
			{
				if (ruleItem.Type == RuleItemType.Mission)
				{
					list.Add(MissionManager.pInstance.GetMission(ruleItem.ID));
				}
			}
		}
		return list;
	}

	private void PopulateMissionItems(List<Mission> inMissions)
	{
		if (inMissions == null || inMissions.Count <= 0)
		{
			return;
		}
		mTreeGroupData.Clear();
		PopulateMissionItems(null, inMissions);
		PopulateTreeList();
		for (int i = 0; i < mItemInfo.Count; i++)
		{
			KAWidget kAWidget = mItemInfo[i].FindChildItem("BkgIcon");
			if (!(kAWidget != null))
			{
				continue;
			}
			Mission rootMission = MissionManager.pInstance.GetRootMission(inMissions[i]);
			string text = ((rootMission != null) ? rootMission.pData.Icon : inMissions[i].pData.Icon);
			kAWidget.SetVisibility(inVisible: false);
			if (string.IsNullOrEmpty(text))
			{
				continue;
			}
			if (text.StartsWith("http://"))
			{
				kAWidget.SetTextureFromURL(text, base.gameObject);
				continue;
			}
			string[] array = text.Split('/');
			if (array.Length >= 3)
			{
				kAWidget.SetTextureFromBundle(array[0] + "/" + array[1], array[2], base.gameObject);
			}
		}
		if (!(pParentUI != null))
		{
			return;
		}
		if (inMissions != null && inMissions.Count > 0)
		{
			Task playerActiveTask = MissionManagerDO.GetPlayerActiveTask();
			if (playerActiveTask != null)
			{
				pParentUI.ShowQuestDetails(playerActiveTask);
				KAWidget itemFromTask = GetItemFromTask(playerActiveTask);
				if (itemFromTask != null)
				{
					SelectWidget(itemFromTask);
				}
			}
			else
			{
				pParentUI.ShowQuestDetails(inMissions[0]);
			}
		}
		else
		{
			pParentUI.OnNoActiveQuests();
		}
	}

	private void OnTextureLoaded(KAWidget widget)
	{
		widget.SetVisibility(inVisible: true);
	}

	private void PopulateMissionItems(KAUITreeListItemData inParent, List<Mission> inRootMissions)
	{
		if (inRootMissions == null || inRootMissions.Count <= 0)
		{
			return;
		}
		foreach (Mission inRootMission in inRootMissions)
		{
			if (inRootMission != null)
			{
				KAUITreeListItemData item = GetItem(inParent, inRootMission);
				AddItem((inParent != null) ? inParent._Name : "", item, inRefreshTree: false);
			}
		}
	}

	private KAWidget GetItemFromTask(Task inTask)
	{
		Mission rootMission = MissionManager.pInstance.GetRootMission(inTask);
		foreach (KAWidget item in mItemInfo)
		{
			QuestUserData questUserData = (QuestUserData)item.GetUserData();
			if (questUserData != null && rootMission.MissionID == questUserData._MissionId)
			{
				return item;
			}
		}
		return null;
	}

	private string GetMissionName(Mission mission)
	{
		if (mission != null && mission.pData != null && mission.pData.Title != null)
		{
			return mission.pData.Title.GetLocalizedString();
		}
		return string.Empty;
	}
}
