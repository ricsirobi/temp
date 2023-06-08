using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExpansionMissionBoard;

public class UiExpansionMissionBoard : KAUI
{
	public List<ExpansionMission> _ExpansionMissions;

	public List<UITemplate> _UIExpansionMissionTemplates;

	public LocaleString _PreviewText;

	public LocaleString _MemberText;

	public LocaleString _PurchasedText;

	public LocaleString _BuyNowText;

	public LocaleString _ContinueMissionTitleText;

	public LocaleString _ContinueMissionText;

	public LocaleString _MissionStartText;

	public int _MissionGroupID = -1;

	public KAWidget _PurchaseWidget;

	private Action OnClosed;

	private UiNPCQuestDetails mQuestDetailsUi;

	private KAUIMenu mUiMenu;

	private List<StoreData> mStoreDatas = new List<StoreData>();

	private AvAvatarState mLastAvatarState;

	private KAUIGenericDB mKAUIGenericDB;

	private int mSelectedMissionIndex = -1;

	private Mission mSelectedMission;

	private bool mMissionLoaded;

	private bool mStoreLoaded;

	protected override void Start()
	{
		base.Start();
		Init(_MissionGroupID, null);
	}

	public void Init(int groupID, Action onClosedAction)
	{
		mLastAvatarState = AvAvatar.pState;
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		OnClosed = onClosedAction;
		if (_MenuList.Length != 0)
		{
			mUiMenu = _MenuList[0];
			mUiMenu.SetInteractive(interactive: false);
		}
		List<int> list = new List<int>();
		foreach (ExpansionMission expansionMission in _ExpansionMissions)
		{
			list.Add(expansionMission._StoreID);
		}
		list = list.Distinct().ToList();
		KAUICursorManager.SetDefaultCursor("Loading");
		MissionManager.pInstance.LoadMissionData(groupID, OnMissionStaticLoad);
		ItemStoreDataLoader.Load(list.ToArray(), OnStoreLoaded, null);
	}

	private void OnMissionStaticLoad(List<Mission> missions)
	{
		mMissionLoaded = true;
		CheckMissionAndStoreLoaded();
	}

	private void OnStoreLoaded(List<StoreData> inStoreData, object inUserData)
	{
		mStoreDatas = inStoreData;
		mStoreLoaded = true;
		foreach (ExpansionMission expansionMission in _ExpansionMissions)
		{
			expansionMission.pTicketID = GetTicketID(expansionMission._TicketID);
		}
		CheckMissionAndStoreLoaded();
	}

	private void CheckMissionAndStoreLoaded()
	{
		if (mMissionLoaded && mStoreLoaded)
		{
			CreateMenuItems();
		}
	}

	private void CreateMenuItems()
	{
		foreach (ExpansionMission expansionMission in _ExpansionMissions)
		{
			if (expansionMission._Missions.Count == 0)
			{
				continue;
			}
			expansionMission.pTargetMission = expansionMission._Missions[0];
			Mission mission = null;
			for (int i = 0; i < expansionMission._Missions.Count; i++)
			{
				mission = MissionManager.pInstance.GetMission(expansionMission._Missions[i]._MissionID);
				if (!mission.pCompleted || i == expansionMission._Missions.Count - 1)
				{
					if (mission.pCompleted && i == expansionMission._Missions.Count - 1)
					{
						expansionMission.pState = State.Completed;
					}
					else if (!mission.pCompleted)
					{
						expansionMission.pTargetMission = expansionMission._Missions[i];
						break;
					}
				}
			}
			if (!mission.pStarted && expansionMission.pState != State.Completed)
			{
				if (expansionMission.pTargetMission._MissionID == expansionMission._Missions[0]._MissionID)
				{
					expansionMission.pState = State.New;
				}
				else
				{
					expansionMission.pState = State.Between;
				}
			}
			else if (expansionMission.pState != State.Completed)
			{
				List<Task> tasks = new List<Task>();
				MissionManager.pInstance.GetNextTask(mission, ref tasks);
				if (tasks.Count > 0)
				{
					if (expansionMission._PaywallTaskIDs.Contains(tasks[0].TaskID) && !ExpansionUnlock.pInstance.IsUnlocked(expansionMission.pTicketID))
					{
						expansionMission.pState = State.Paywall;
					}
					else
					{
						expansionMission.pState = State.InProgress;
					}
				}
			}
			if (MissionManager.pInstance.IsLocked(mission))
			{
				expansionMission.pState = State.Locked;
				continue;
			}
			UITemplate uITemplate = null;
			UiExpansionMissionTemplate uiExpansionMissionTemplate = null;
			if (_UIExpansionMissionTemplates.Count > 0)
			{
				uITemplate = _UIExpansionMissionTemplates.Find((UITemplate t) => t._CurrentState == expansionMission.pState);
			}
			if ((bool)uITemplate?._Template)
			{
				uiExpansionMissionTemplate = (UiExpansionMissionTemplate)DuplicateWidget(uITemplate._Template);
				bool flag = ExpansionUnlock.pInstance.IsUnlocked(expansionMission.pTicketID);
				bool flag2 = SubscriptionInfo.pIsMember && !SubscriptionInfo.pIsTrialMember;
				string text = "";
				if (flag && !flag2)
				{
					text = _PurchasedText.GetLocalizedString();
				}
				else if (flag2)
				{
					text = _MemberText.GetLocalizedString();
				}
				else if (expansionMission.pState == State.Paywall)
				{
					text = _BuyNowText.GetLocalizedString();
				}
				else if (!flag && !flag2)
				{
					text = _PreviewText.GetLocalizedString();
				}
				if (expansionMission.pState != State.Completed && (bool)_PurchaseWidget && !string.IsNullOrEmpty(text))
				{
					KAWidget kAWidget = DuplicateWidget(_PurchaseWidget);
					uiExpansionMissionTemplate.AddChild(kAWidget);
					kAWidget.gameObject.SetActive(value: true);
					kAWidget.SetText(text);
				}
				if (uiExpansionMissionTemplate.Init(expansionMission, _MissionStartText.GetLocalizedString()))
				{
					uiExpansionMissionTemplate.name = expansionMission._NameText.GetLocalizedString();
					uiExpansionMissionTemplate.SetVisibility(inVisible: true);
					mUiMenu?.AddWidget(uiExpansionMissionTemplate);
				}
				else
				{
					RemoveWidget(uiExpansionMissionTemplate);
				}
			}
		}
		_ExpansionMissions.RemoveAll((ExpansionMission t) => t.pState == State.Locked);
		KAUICursorManager.SetDefaultCursor("Arrow");
		mUiMenu?.SetInteractive(interactive: true);
	}

	private int GetTicketID(int itemTicketID)
	{
		ItemData itemData = null;
		foreach (StoreData mStoreData in mStoreDatas)
		{
			itemData = mStoreData.FindItem(itemTicketID);
		}
		if (itemData == null)
		{
			return -1;
		}
		if (itemData.Relationship == null)
		{
			return itemTicketID;
		}
		ItemDataRelationship[] relationship = itemData.Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (itemDataRelationship.Type == "Override")
			{
				int ticketID = GetTicketID(itemDataRelationship.ItemId);
				if (ticketID != -1)
				{
					return ticketID;
				}
			}
			else if (itemDataRelationship.Type == "Prereq")
			{
				if (CommonInventoryData.pInstance.FindItem(itemDataRelationship.ItemId) != null)
				{
					return itemTicketID;
				}
				return -1;
			}
		}
		return itemTicketID;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _BackButtonName)
		{
			CloseUI();
			return;
		}
		mSelectedMissionIndex = mUiMenu.GetSelectedItemIndex();
		mSelectedMission = MissionManager.pInstance.GetMission(_ExpansionMissions[mSelectedMissionIndex].pTargetMission._MissionID);
		NPCAvatar._Engaged = null;
		switch (_ExpansionMissions[mSelectedMissionIndex].pState)
		{
		case State.New:
			ShowQuestDetails(GetNextTask());
			break;
		case State.InProgress:
			ShowGenericDB("ContinueMissionDB", string.Format(_ContinueMissionText.GetLocalizedString(), _ExpansionMissions[mSelectedMissionIndex]._NameText.GetLocalizedString()), _ContinueMissionTitleText.GetLocalizedString(), "OnAcceptContinueMission", "OnDeclineContinueMission");
			break;
		case State.Paywall:
		{
			ExpansionUnlock.ExpansionInfo inExpansionInfo = null;
			ExpansionUnlock.ExpansionInfo[] expansionInfo = ExpansionUnlock.pInstance.GetExpansionInfo(_ExpansionMissions[mSelectedMissionIndex].pTicketID);
			if (expansionInfo != null)
			{
				for (int i = 0; i < expansionInfo.Length; i++)
				{
					if (expansionInfo[i].IsUnlocked())
					{
						return;
					}
					inExpansionInfo = expansionInfo[i];
				}
			}
			ExpansionUnlock.pInstance.ShowExpansionUpsell(inExpansionInfo, OnPurchaseExpansion);
			break;
		}
		case State.Between:
			ShowGenericDB("PfMissionPromptDB", _ExpansionMissions[mSelectedMissionIndex].pTargetMission._MissionPromptText.GetLocalizedString(), "", "OnAcceptMissionStartPrompt", "OnDeclineContinueMission");
			break;
		}
	}

	private void OnAcceptMissionStartPrompt()
	{
		AvAvatar.pStartLocation = _ExpansionMissions[mSelectedMissionIndex].pTargetMission._NPC;
		GameObject gameObject;
		if (RsResourceManager.pCurrentLevel != _ExpansionMissions[mSelectedMissionIndex].pTargetMission._Scene)
		{
			RsResourceManager.LoadLevel(_ExpansionMissions[mSelectedMissionIndex].pTargetMission._Scene);
		}
		else if ((gameObject = GameObject.Find(AvAvatar.pStartLocation)) != null)
		{
			AvAvatar.TeleportTo(gameObject.transform.position, -gameObject.transform.forward, 1f);
		}
		CloseUI();
		DestroyDB();
	}

	private void OnPurchaseExpansion(bool success)
	{
		if (success)
		{
			mUiMenu.ClearItems();
			CreateMenuItems();
		}
	}

	private Task GetNextTask()
	{
		List<Task> tasks = new List<Task>();
		MissionManager.pInstance.GetNextTask(mSelectedMission, ref tasks);
		return tasks[tasks.Count - 1];
	}

	private void OnAcceptContinueMission()
	{
		CloseUI();
		MissionManagerDO.SetCurrentActiveTask(GetNextTask().TaskID);
		DestroyDB();
	}

	private void OnDeclineContinueMission()
	{
		mSelectedMission = null;
		mSelectedMissionIndex = -1;
		DestroyDB();
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
				SetVisibility(inVisible: false);
				mQuestDetailsUi.SetVisibility(inVisible: true);
				mQuestDetailsUi.ShowTaskDetails(inTask, inOnCloseDelegate, CloseUI);
			}
			else
			{
				Debug.LogError("No UiNPCQuestDetails component found!");
			}
		}
	}

	public void OnQuestDetailsClose()
	{
		SetVisibility(inVisible: true);
		mQuestDetailsUi?.SetVisibility(inVisible: false);
		NPCAvatar._Engaged = base.gameObject;
	}

	private void CloseUI()
	{
		AvAvatar.pState = mLastAvatarState;
		AvAvatar.SetUIActive(inActive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void ShowGenericDB(string inDBName, string inText, string inTitle, string inYesMessage = "", string inNoMessage = "")
	{
		if (mKAUIGenericDB != null)
		{
			DestroyDB();
		}
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", inDBName);
		mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(inYesMessage), !string.IsNullOrEmpty(inNoMessage), inOKBtn: false, inCloseBtn: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._YesMessage = inYesMessage;
		mKAUIGenericDB._NoMessage = inNoMessage;
		mKAUIGenericDB.SetText(inText, interactive: false);
		if (inTitle != null)
		{
			mKAUIGenericDB.SetTitle(inTitle);
		}
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void DestroyDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}
}
