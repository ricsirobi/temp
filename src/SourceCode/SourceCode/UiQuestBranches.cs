using System.Collections.Generic;
using UnityEngine;

public class UiQuestBranches : KAUI
{
	[SerializeField]
	private List<QuestWidgets> m_Quests = new List<QuestWidgets>();

	[SerializeField]
	private LocaleString m_QuestCompletedText = new LocaleString("Complete");

	[SerializeField]
	private LocaleString m_QuestInProgressText = new LocaleString("In Progress");

	[SerializeField]
	private LocaleString m_QuestNotStartedText = new LocaleString("Not Started");

	[SerializeField]
	private UiNPCQuestDetails m_QuestDetailsUi;

	private KAWidget mCloseBtn;

	protected override void Start()
	{
		base.Start();
		mCloseBtn = FindItem(_BackButtonName);
		Init();
	}

	public void Init()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		SetState(KAUIState.DISABLED);
		MissionManager.pInstance.LoadMissionData(((MissionManagerDO)MissionManager.pInstance)._QuestBranchMissions._GroupId, OnMissionStaticLoad);
	}

	private void OnMissionStaticLoad(List<Mission> missions)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetState(KAUIState.INTERACTIVE);
		List<int> missionIds = ((MissionManagerDO)MissionManager.pInstance)._QuestBranchMissions._MissionIds;
		if (missionIds != null && missionIds.Count >= 1 && m_Quests != null && m_Quests.Count >= 1)
		{
			for (int i = 0; i < m_Quests.Count && i <= missionIds.Count - 1; i++)
			{
				SetUpWidgetData(m_Quests[i], missionIds[i]);
			}
			RefreshUi();
			SetVisibility(inVisible: true);
		}
	}

	private void RefreshUi()
	{
		foreach (QuestWidgets quest in m_Quests)
		{
			Mission pMission = quest.pMission;
			KAToggleButton toggleWidget = quest._ToggleWidget;
			if (!(toggleWidget == null) && pMission != null)
			{
				toggleWidget.FindChildItem("TxtStatus")?.SetText(pMission.pCompleted ? m_QuestCompletedText.GetLocalizedString() : (pMission.pStarted ? m_QuestInProgressText.GetLocalizedString() : m_QuestNotStartedText.GetLocalizedString()));
				toggleWidget.FindChildItem("QuizKnotImage").SetVisibility(pMission.pCompleted);
				toggleWidget.SetState(pMission.pCompleted ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
				toggleWidget.SetChecked(!pMission.pCompleted && pMission.pStarted);
			}
		}
	}

	private void SetUpWidgetData(QuestWidgets questWidget, int missionId)
	{
		questWidget.pMission = MissionManager.pInstance.GetMission(missionId);
		if (questWidget.pMission != null && !MissionManager.pInstance.GetRootMission(questWidget.pMission).pData.Hidden)
		{
			List<Task> tasks = new List<Task>();
			MissionManager.pInstance.GetNextTask(questWidget.pMission, ref tasks);
			if (tasks.Count > 0)
			{
				questWidget.pTaskId = tasks[0].TaskID;
			}
		}
	}

	private void CloseUI()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		Object.Destroy(base.gameObject);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mCloseBtn != null && mCloseBtn == inWidget)
		{
			CloseUI();
			return;
		}
		QuestWidgets questWidgets = m_Quests.Find((QuestWidgets x) => x.pMission != null && x._ToggleWidget == inWidget);
		if (questWidgets != null)
		{
			if (questWidgets.pMission.pStarted)
			{
				MissionManagerDO.SetCurrentActiveTask(questWidgets.pTaskId);
				questWidgets._ToggleWidget.SetChecked(isChecked: true);
				CloseUI();
			}
			else if (m_QuestDetailsUi != null)
			{
				SetVisibility(inVisible: false);
				m_QuestDetailsUi.SetVisibility(inVisible: true);
				m_QuestDetailsUi.ShowTaskDetails(questWidgets.pMission.GetTask(questWidgets.pTaskId), CloseUI);
			}
		}
	}
}
