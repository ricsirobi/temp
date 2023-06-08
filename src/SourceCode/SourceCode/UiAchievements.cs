using System;
using System.Collections.Generic;
using UnityEngine;

public class UiAchievements : KAUI
{
	public enum AchievementType
	{
		NONE,
		MISSION,
		MISSION_TASK
	}

	[Serializable]
	public class AchievementTitleInfo
	{
		public int _ID = -1;

		public LocaleString _TitleText;

		public int _PopUpMessageID;

		public int _PopUpMessageAllDoneID;

		public bool _Visible = true;

		public string _SceneName = "";

		public string _TeleportToMarker = "";

		public KAWidget _Template;

		public int _MaxRewardCount = 4;

		public int _Multiplier = 1;

		public AchievementType _Type;

		public int _MaxItemCount = 1;

		[NonSerialized]
		public KAWidget pWidget;

		[NonSerialized]
		public int pLevel = -1;

		[NonSerialized]
		public int pCounter = -1;

		[NonSerialized]
		public int pCountRemForNext = -1;

		[NonSerialized]
		public AchievementTaskReward[] pRewards;
	}

	public LocaleString[] _PopUpMessagesText;

	public LocaleString[] _PopUpMessageAllDoneText;

	public LocaleString _NextMedalText;

	public LocaleString _TrophyText;

	protected bool mIsReady;

	public List<AchievementTitleInfo> _AchievementInfoList = new List<AchievementTitleInfo>();

	public AchievementCategory[] _AchievementCategoryList;

	public int _NumberOfRecentCategories = 5;

	public GameObject _MessageObject;

	public string _YesMessage = "";

	public string _NoMessage = "";

	public string _OKMessage = "";

	public string _CloseMessage = "";

	public string _TextMessage = "";

	public string _MovieMessage = "";

	public string _ButtonClickedMessage = "";

	public string _TrophyBkg = "";

	public string _EmptyTrophyBkg = "";

	public string _SilverAwardBkg = "";

	public string _EmptySilverAwardBkg = "";

	public string _CopperAwardBkg = "";

	public string _EmptyCopperAwardBkg = "";

	public string _BronzeAwardBkg = "";

	public string _EmptyBronzeAwardBkg = "";

	public int _MessageIdentifier = -1;

	public static int SelectedAchievementGroupID = -1;

	protected KAWidget mTrophyWidget;

	protected KAWidget mPointsWidget;

	protected UiJournalAchievementsMenu mMenu;

	protected UiJournalAchievementsCatMenu mCatMenu;

	protected bool mIsInitialized;

	protected string mAchTrophyBkgName = "Ach_";

	protected UiJournalAchievementPopup mUiAchievementPopup;

	protected string mSelectedCategoryName = "";

	protected bool mIsAchievementPointsSet;

	protected bool mLoadingAchData;

	protected bool mSetVisibleAfterLoad;

	public bool pIsReady => mIsReady;

	protected override void Start()
	{
		base.Start();
		mMenu = (UiJournalAchievementsMenu)GetMenu("UiJournalAchievementsMenu");
		mUiAchievementPopup = base.transform.parent.GetComponentInChildren<UiJournalAchievementPopup>();
		if (mMenu != null)
		{
			PopulateAchievementMenu();
		}
		mCatMenu = (UiJournalAchievementsCatMenu)GetMenu("UiJournalAchievementsCatMenu");
		KAWidget kAWidget = FindItem("TxtAchievementPoints");
		if (kAWidget != null)
		{
			mPointsWidget = kAWidget.FindChildItem("TxtAchPoints");
		}
	}

	public void Reset()
	{
		mIsInitialized = false;
	}

	protected override void Update()
	{
		base.Update();
		if (!mIsAchievementPointsSet && UserProfile.pProfileData != null)
		{
			mIsAchievementPointsSet = true;
			if (mPointsWidget != null)
			{
				mPointsWidget.SetText(UserProfile.pProfileData.AchievementCount.ToString());
			}
		}
	}

	protected Texture GetCatgoryTextureFromId(int achievementID)
	{
		Texture result = null;
		AchievementCategory categoryfromAchievementID = GetCategoryfromAchievementID(achievementID);
		if (categoryfromAchievementID != null)
		{
			result = categoryfromAchievementID._CategoryIcon;
		}
		return result;
	}

	protected AchievementCategory GetCategoryfromAchievementID(int achievementID)
	{
		AchievementCategory[] achievementCategoryList = _AchievementCategoryList;
		foreach (AchievementCategory achievementCategory in achievementCategoryList)
		{
			int[] achievementID2 = achievementCategory._AchievementID;
			for (int j = 0; j < achievementID2.Length; j++)
			{
				if (achievementID2[j] == achievementID)
				{
					return achievementCategory;
				}
			}
		}
		return null;
	}

	public override void SetVisibility(bool t)
	{
		base.SetVisibility(t);
		if (t)
		{
			if (!mIsInitialized)
			{
				GetAchievements();
			}
		}
		else if (mUiAchievementPopup != null && mUiAchievementPopup.GetVisibility())
		{
			mUiAchievementPopup.Show(visibility: false);
		}
	}

	public void Show(bool t)
	{
		base.gameObject.SetActive(t);
		if (t)
		{
			if (!mIsInitialized)
			{
				GetAchievements();
				mSetVisibleAfterLoad = true;
				base.SetVisibility(inVisible: false);
			}
			else
			{
				base.SetVisibility(inVisible: true);
			}
		}
		else if (mUiAchievementPopup != null && mUiAchievementPopup.GetVisibility())
		{
			mUiAchievementPopup.Show(visibility: false);
		}
	}

	private void GetAchievements()
	{
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		mLoadingAchData = true;
		UserAchievementTask.Get(GetAchievementOwnerID(), AchievementEventHandler, null);
		mIsInitialized = true;
	}

	public virtual string GetAchievementOwnerID()
	{
		return UserInfo.pInstance.UserID;
	}

	protected void GetMissionAchievements()
	{
		if (_AchievementInfoList == null || _AchievementInfoList.Count == 0)
		{
			return;
		}
		foreach (AchievementTitleInfo achievementInfo in _AchievementInfoList)
		{
			if (achievementInfo == null)
			{
				continue;
			}
			switch (achievementInfo._Type)
			{
			case AchievementType.MISSION:
			{
				achievementInfo.pCounter = 0;
				achievementInfo.pCountRemForNext = 1;
				Mission mission = MissionManager.pInstance.GetMission(achievementInfo._ID);
				if (mission != null && mission.pCompleted)
				{
					achievementInfo.pCounter = 1;
					achievementInfo.pLevel = 1;
					achievementInfo.pCountRemForNext = 0;
					ShowAwards(achievementInfo.pWidget, 0);
				}
				break;
			}
			case AchievementType.MISSION_TASK:
			{
				achievementInfo.pCounter = 0;
				achievementInfo.pCountRemForNext = 1;
				Task task = MissionManager.pInstance.GetTask(achievementInfo._ID);
				if (task != null && task.pCompleted)
				{
					achievementInfo.pCounter = 1;
					achievementInfo.pLevel = 1;
					achievementInfo.pCountRemForNext = 0;
					ShowAwards(achievementInfo.pWidget, 0);
				}
				break;
			}
			}
		}
	}

	public virtual void AchievementEventHandler(WsServiceEvent inEvent, ArrayOfUserAchievementTask arrayOfUserAchievementTask)
	{
		if (this == null)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			mLoadingAchData = false;
			SetInteractive(interactive: true);
			if (mSetVisibleAfterLoad)
			{
				mSetVisibleAfterLoad = false;
				base.SetVisibility(inVisible: true);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (arrayOfUserAchievementTask == null)
			{
				break;
			}
			UserAchievementTask[] userAchievementTask = arrayOfUserAchievementTask.UserAchievementTask;
			foreach (UserAchievementTask userAchievementTask2 in userAchievementTask)
			{
				int achievementIndex;
				AchievementTitleInfo achievementTitleInfo = FindAchievementInfo(userAchievementTask2.AchievementTaskGroupID, out achievementIndex);
				if (achievementTitleInfo != null && achievementTitleInfo.pWidget != null)
				{
					achievementTitleInfo.pCounter = 0;
					achievementTitleInfo.pCountRemForNext = 0;
					if (userAchievementTask2.AchievedQuantity.HasValue)
					{
						achievementTitleInfo.pCounter = userAchievementTask2.AchievedQuantity.Value * achievementTitleInfo._Multiplier;
					}
					if (userAchievementTask2.QuantityRequired.HasValue)
					{
						achievementTitleInfo.pCountRemForNext = userAchievementTask2.QuantityRequired.Value * achievementTitleInfo._Multiplier;
					}
					if (userAchievementTask2.NextLevelAchievementRewards != null)
					{
						achievementTitleInfo.pRewards = userAchievementTask2.NextLevelAchievementRewards;
					}
					KAWidget kAWidget = achievementTitleInfo.pWidget.FindChildItem("TxtCounter");
					if (kAWidget != null)
					{
						kAWidget.SetText(achievementTitleInfo.pCounter.ToString());
					}
					achievementTitleInfo.pLevel = Mathf.Min(4, achievementTitleInfo._MaxRewardCount);
					if (userAchievementTask2.NextLevel.HasValue)
					{
						achievementTitleInfo.pLevel = userAchievementTask2.NextLevel.Value - 1;
					}
					ShowAwards(achievementTitleInfo.pWidget, Mathf.Max(0, achievementTitleInfo._MaxRewardCount - achievementTitleInfo.pLevel));
				}
				mIsReady = true;
			}
			GetMissionAchievements();
			int achievementIndex2 = -1;
			AchievementTitleInfo achievementTitleInfo2 = FindAchievementInfo(SelectedAchievementGroupID, out achievementIndex2);
			if (achievementTitleInfo2 != null)
			{
				ProcessMenuWidgetClicked(achievementTitleInfo2.pWidget);
			}
			SelectedAchievementGroupID = -1;
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.LogError("ERROR: Unable to fetch achievements!");
			KAUICursorManager.SetDefaultCursor("Arrow");
			mLoadingAchData = false;
			break;
		}
	}

	public void ShowAwards(KAWidget inWidget, int inEmptyRewardCount)
	{
		if (inWidget == null)
		{
			return;
		}
		KAWidget kAWidget = inWidget.FindChildItem("AniTrophy");
		if (kAWidget != null)
		{
			if (inEmptyRewardCount >= 1)
			{
				kAWidget.SetSprite(_EmptyTrophyBkg);
			}
			else
			{
				kAWidget.SetSprite(_TrophyBkg);
			}
		}
		KAWidget kAWidget2 = inWidget.FindChildItem("AniAwardSilver");
		if (kAWidget2 != null)
		{
			if (inEmptyRewardCount >= 2)
			{
				kAWidget2.SetSprite(_EmptySilverAwardBkg);
			}
			else
			{
				kAWidget2.SetSprite(_SilverAwardBkg);
			}
		}
		KAWidget kAWidget3 = inWidget.FindChildItem("AniAwardCopper");
		if (kAWidget3 != null)
		{
			if (inEmptyRewardCount >= 3)
			{
				kAWidget3.SetSprite(_EmptyCopperAwardBkg);
			}
			else
			{
				kAWidget3.SetSprite(_CopperAwardBkg);
			}
		}
		KAWidget kAWidget4 = inWidget.FindChildItem("AniAwardBronze");
		if (kAWidget4 != null)
		{
			if (inEmptyRewardCount >= 4)
			{
				kAWidget4.SetSprite(_EmptyBronzeAwardBkg);
			}
			else
			{
				kAWidget4.SetSprite(_BronzeAwardBkg);
			}
		}
	}

	public AchievementTitleInfo FindAchievementInfo(int inAchievementTypeID, out int achievementIndex)
	{
		achievementIndex = -1;
		for (int i = 0; i < _AchievementInfoList.Count; i++)
		{
			if (_AchievementInfoList[i]._ID == inAchievementTypeID)
			{
				achievementIndex = i;
				return _AchievementInfoList[i];
			}
		}
		return null;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "CloseBtn")
		{
			SetVisibility(t: false);
		}
		else if (inWidget.name == "GeneralBtn" && !mSelectedCategoryName.Equals(inWidget.name))
		{
			mSelectedCategoryName = inWidget.name;
			mMenu.ClearItems();
			PopulateAchievementMenu();
		}
	}

	protected void PopulateAchievementMenu()
	{
		if (mMenu == null || mMenu._Template == null)
		{
			return;
		}
		for (int i = 0; i < _AchievementInfoList.Count; i++)
		{
			if (_AchievementInfoList[i]._Visible && _AchievementInfoList[i]._ID != 0)
			{
				KAWidget template = _AchievementInfoList[i]._Template;
				if (template == null)
				{
					template = mMenu._Template;
				}
				KAWidget kAWidget = DuplicateWidget(template);
				kAWidget.gameObject.name = mAchTrophyBkgName + i;
				KAWidget kAWidget2 = kAWidget.FindChildItem("TxtTrophyName");
				if (kAWidget2 != null)
				{
					kAWidget2.SetTextByID(_AchievementInfoList[i]._TitleText._ID, _AchievementInfoList[i]._TitleText._Text);
				}
				KAWidget kAWidget3 = kAWidget.FindChildItem("AniCategoryIco");
				Texture catgoryTextureFromId = GetCatgoryTextureFromId(_AchievementInfoList[i]._ID);
				kAWidget3.SetTexture(catgoryTextureFromId);
				kAWidget.SetVisibility(inVisible: true);
				mMenu.AddWidget(kAWidget);
				if (_AchievementInfoList[i].pWidget == null)
				{
					_AchievementInfoList[i].pWidget = kAWidget;
				}
			}
		}
	}

	public void ProcessCategorySelected(KAWidget inwidget)
	{
		if (inwidget == null || mSelectedCategoryName.Equals(inwidget.name))
		{
			return;
		}
		mSelectedCategoryName = inwidget.name;
		AchievementCategory categoryInfoFromWidgetName = GetCategoryInfoFromWidgetName(inwidget.name);
		mMenu.ClearItems();
		int[] achievementID = categoryInfoFromWidgetName._AchievementID;
		foreach (int inAchievementTypeID in achievementID)
		{
			int achievementIndex = 0;
			AchievementTitleInfo achievementTitleInfo = FindAchievementInfo(inAchievementTypeID, out achievementIndex);
			KAWidget kAWidget = DuplicateWidget(mMenu._Template);
			kAWidget.gameObject.name = mAchTrophyBkgName + achievementIndex;
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtTrophyName");
			if (kAWidget2 != null)
			{
				kAWidget2.SetTextByID(achievementTitleInfo._TitleText._ID, achievementTitleInfo._TitleText._Text);
			}
			KAWidget kAWidget3 = kAWidget.FindChildItem("AniCategoryIco");
			Texture catgoryTextureFromId = GetCatgoryTextureFromId(achievementTitleInfo._ID);
			kAWidget3.SetTexture(catgoryTextureFromId);
			kAWidget.SetVisibility(inVisible: true);
			mMenu.AddWidget(kAWidget);
		}
	}

	protected AchievementCategory GetCategoryInfoFromWidgetName(string widgetname)
	{
		for (int i = 0; i < _AchievementCategoryList.Length; i++)
		{
			if (_AchievementCategoryList[i]._CategoryName == widgetname)
			{
				return _AchievementCategoryList[i];
			}
		}
		return null;
	}

	public void ProcessMenuWidgetClicked(KAWidget inWidget)
	{
		if (!(inWidget != null) || !inWidget.name.Contains(mAchTrophyBkgName) || !GetVisibility())
		{
			return;
		}
		int result = -1;
		if (int.TryParse(inWidget.name.Substring(mAchTrophyBkgName.Length), out result))
		{
			AchievementTitleInfo achievementTitleInfo = _AchievementInfoList[result];
			if (achievementTitleInfo != null)
			{
				mUiAchievementPopup.Show(inWidget.FindChildItem("TxtTrophyName").GetText(), GetAchievementPopUpString(achievementTitleInfo), achievementTitleInfo);
			}
		}
	}

	public void ProcessCategoryIconClicked(KAWidget inWidget)
	{
		if (inWidget != null)
		{
			int userDataInt = inWidget.GetUserDataInt();
			mMenu.ClearItems();
			int achievementIndex = 0;
			AchievementTitleInfo achievementTitleInfo = FindAchievementInfo(userDataInt, out achievementIndex);
			KAWidget kAWidget = DuplicateWidget(mMenu._Template);
			kAWidget.gameObject.name = mAchTrophyBkgName + achievementIndex;
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtTrophyName");
			if (kAWidget2 != null)
			{
				kAWidget2.SetTextByID(achievementTitleInfo._TitleText._ID, achievementTitleInfo._TitleText._Text);
			}
			KAWidget kAWidget3 = kAWidget.FindChildItem("AniCategoryIco");
			Texture catgoryTextureFromId = GetCatgoryTextureFromId(achievementTitleInfo._ID);
			kAWidget3.SetTexture(catgoryTextureFromId);
			kAWidget.SetVisibility(inVisible: true);
			mMenu.AddWidget(kAWidget);
		}
	}

	private string GetAchievementPopUpString(AchievementTitleInfo ach)
	{
		string result = "";
		if (ach != null)
		{
			if (ach.pLevel >= ach._MaxRewardCount)
			{
				if (_PopUpMessageAllDoneText != null && ach._PopUpMessageAllDoneID < _PopUpMessageAllDoneText.Length)
				{
					result = StringTable.GetStringData(_PopUpMessageAllDoneText[ach._PopUpMessageAllDoneID]._ID, _PopUpMessageAllDoneText[ach._PopUpMessageAllDoneID]._Text);
				}
			}
			else if (_PopUpMessagesText != null && ach._PopUpMessageID < _PopUpMessagesText.Length)
			{
				result = StringTable.GetStringData(_PopUpMessagesText[ach._PopUpMessageID]._ID, _PopUpMessagesText[ach._PopUpMessageID]._Text);
				int num = ((ach.pCounter >= 0) ? ach.pCounter : 0);
				int num2 = ((ach.pCountRemForNext >= 0) ? ach.pCountRemForNext : 0);
				result = result.Replace("XX", (num + num2).ToString());
				result = ((ach.pLevel >= 3) ? result.Replace("@@@@", StringTable.GetStringData(_TrophyText._ID, _TrophyText._Text)) : result.Replace("@@@@", StringTable.GetStringData(_NextMedalText._ID, _NextMedalText._Text)));
			}
		}
		return result;
	}

	public void Destroy()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}

	protected int[] GetAchievementsFromCategory(string inName)
	{
		AchievementCategory[] achievementCategoryList = _AchievementCategoryList;
		foreach (AchievementCategory achievementCategory in achievementCategoryList)
		{
			if (achievementCategory._CategoryName == inName)
			{
				return achievementCategory._AchievementID;
			}
		}
		return null;
	}

	protected void PopulateAchievementCategories()
	{
		AchievementCategory[] achievementCategoryList = _AchievementCategoryList;
		foreach (AchievementCategory achievementCategory in achievementCategoryList)
		{
			KAWidget kAWidget = DuplicateWidget(mCatMenu._Template);
			kAWidget.SetText(achievementCategory._CategoryName);
			kAWidget.name = achievementCategory._CategoryName;
			kAWidget.SetVisibility(inVisible: true);
			mCatMenu.AddWidget(kAWidget);
		}
	}
}
