using System;
using System.Collections.Generic;
using UnityEngine;

public class UiDailyQuests : KAUI
{
	public static int pMissionGroup;

	public UiDaysMenu _UiDaysMenu;

	public UiDailyMissionsMenu _UiDailyMissionsMenu;

	public UiDailyMissionProgressMenu _UiDailyMissionProgressMenu;

	public LocaleString _CurrentDayRewardText = new LocaleString("Today's Reward");

	public LocaleString _OtherDayRewardText = new LocaleString("Day [X] Reward");

	public Action pOnUiClosed;

	private KAWidget mCloseBtn;

	private KAWidget mBtnLastDay;

	private KAWidget mQuestTimer;

	private bool mIsReady;

	private KAWidget mSelectedDayHeader;

	private KAWidget mSelectedDayRewards;

	private KAWidget mInactiveDay;

	protected override void Start()
	{
		base.Start();
		mCloseBtn = FindItem("BtnClose");
		mQuestTimer = FindItem("QuestTimer");
		mBtnLastDay = FindItem("BtnLastDay");
		mSelectedDayHeader = FindItem("BkgSelectedDayHeader");
		mSelectedDayRewards = FindItem("SelectedDayRewards");
		mInactiveDay = FindItem("InActiveDay");
		KAUI.SetExclusive(this);
		if (!MissionManager.pInstance.RefreshDailyMission())
		{
			Init();
		}
	}

	private void Init()
	{
		mIsReady = true;
		MissionGroup missionGroup = MissionManager.pInstance.pDailyMissionStateResult.MissionGroup.Find((MissionGroup item) => item.MissionGroupID == pMissionGroup);
		List<UserTimedAchievement> list = MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement.FindAll((UserTimedAchievement item) => item.GroupID == pMissionGroup);
		if (list != null && list.Count > 0)
		{
			_UiDaysMenu.AddData(mBtnLastDay, list[list.Count - 1], missionGroup.Day);
			list.RemoveAt(list.Count - 1);
			_UiDaysMenu.Populate(list, missionGroup.Day);
			_UiDailyMissionsMenu.Populate();
			_UiDailyMissionProgressMenu.Populate(missionGroup.CompletionCount);
		}
		else
		{
			UtDebug.LogError("DailyQuest Data is missing for missionGroup " + pMissionGroup, 0);
		}
		if (!GetVisibility())
		{
			SetVisibility(inVisible: true);
		}
		if (GetState() == KAUIState.DISABLED)
		{
			SetState(KAUIState.INTERACTIVE);
		}
		if (!UICursorManager.GetCursorName().Equals("Arrow"))
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn)
		{
			if (pOnUiClosed != null)
			{
				pOnUiClosed();
				pOnUiClosed = null;
			}
			KAUI.RemoveExclusive(this);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else if (inWidget == mBtnLastDay)
		{
			_UiDaysMenu.ProcessClick(inWidget);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (MissionManager.pIsReady && !mIsReady)
		{
			Init();
		}
		if (IsActive() && mQuestTimer != null)
		{
			TimeSpan timeSpan = MissionManager.pInstance.pDailyMissionResetTime.Subtract(UtUtilities.GetPSTTimeFromUTC(ServerTime.pCurrentTime));
			if (timeSpan.TotalSeconds > 0.0)
			{
				mQuestTimer.SetText(GameUtilities.FormatTime(timeSpan));
				return;
			}
			mIsReady = false;
			SetState(KAUIState.DISABLED);
			KAUICursorManager.SetDefaultCursor("Loading");
			MissionManager.pInstance.RefreshDailyMission();
		}
	}

	public void OnDayItemClicked(UiDaysWidgetData inWidgetData, string rewardName, bool isCurrentDay)
	{
		if (mSelectedDayHeader != null)
		{
			string text = (isCurrentDay ? _CurrentDayRewardText.GetLocalizedString() : _OtherDayRewardText.GetLocalizedString());
			text = text.Replace("[X]", inWidgetData._Day.ToString());
			mSelectedDayHeader.SetText(text);
		}
		if (mSelectedDayRewards != null)
		{
			mSelectedDayRewards.SetText(rewardName);
		}
		if (mInactiveDay != null)
		{
			mInactiveDay.SetVisibility(!isCurrentDay);
		}
	}

	public void RegisterCloseEvent()
	{
		pOnUiClosed = OnMissionBoardClosed;
	}

	private void OnMissionBoardClosed()
	{
		AvAvatar.SetOnlyAvatarActive(active: true);
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
	}
}
