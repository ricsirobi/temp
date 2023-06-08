using System;
using UnityEngine;

public class UiIncredibleMachinesLevelSelection : KAUI
{
	public LocaleString _LockedItemClickText = new LocaleString("You need to beat previous levels to unlock this level.");

	public UiIncredibleMachinesLevelSelectionMenu _UiLevelSelectionMenu;

	public KAUIMenu _UiPageTabMenu;

	private KAWidget mBackBtn;

	private KAWidget mAniPageCount;

	private int mPageNumber;

	private IMLevelData mLevelData;

	public void PopulateLevels(IMLevelData imLevelData, bool forceReload = false)
	{
		if (mLevelData != null && !forceReload)
		{
			RefreshLevels(mLevelData);
			return;
		}
		mLevelData = imLevelData;
		_UiLevelSelectionMenu.ClearItems();
		int num = 0;
		int num2 = 1;
		int num3 = IMLevelProgress.pInstance.GetLastLevelPlayed() + 1;
		CTLevelManager cTLevelManager = (CTLevelManager)IMGLevelManager.pInstance;
		bool flag = !UnlockManager.IsSceneUnlocked(cTLevelManager._GameModuleName, inShowUi: true);
		LevelData[] levels = imLevelData.Levels;
		foreach (LevelData levelData in levels)
		{
			if (!levelData.IsMissionLevel)
			{
				KAWidget kAWidget = _UiLevelSelectionMenu.AddWidget("TemplateTool");
				IncredibleMachineLevelUserData incredibleMachineLevelUserData = new IncredibleMachineLevelUserData();
				incredibleMachineLevelUserData._Level = num2;
				incredibleMachineLevelUserData._Index = num;
				string levelName = levelData.LevelName;
				kAWidget.name = levelName;
				KAWidget widget = kAWidget.FindChildItem("AniLevelNo");
				DisplayLevelNo(widget, num2);
				kAWidget.SetUserData(incredibleMachineLevelUserData);
				bool isLocked = (num2 > num3 || (levelData.MemberOnly && flag)) && !cTLevelManager._UnlockAllLevels;
				RefreshWidget(kAWidget, levelData, isLocked);
				num2++;
			}
			num++;
		}
		_UiPageTabMenu.ClearItems();
		int pageCount = _UiLevelSelectionMenu.GetPageCount();
		for (int j = 0; j < pageCount; j++)
		{
			_UiPageTabMenu.AddWidget(_UiPageTabMenu._Template.name).SetVisibility(inVisible: true);
		}
		mPageNumber = _UiLevelSelectionMenu.GetCurrentPage();
		OnPageChange(mPageNumber);
	}

	public void RefreshLevels(IMLevelData imLevelData)
	{
		int num = IMLevelProgress.pInstance.GetLastLevelPlayed() + 1;
		CTLevelManager cTLevelManager = (CTLevelManager)IMGLevelManager.pInstance;
		bool flag = !UnlockManager.IsSceneUnlocked(cTLevelManager._GameModuleName, inShowUi: true);
		foreach (KAWidget item in _UiLevelSelectionMenu.GetItems())
		{
			IncredibleMachineLevelUserData incredibleMachineLevelUserData = (IncredibleMachineLevelUserData)item.GetUserData();
			LevelData levelData = imLevelData.Levels[incredibleMachineLevelUserData._Index];
			bool isLocked = incredibleMachineLevelUserData._Level > num || (levelData.MemberOnly && flag && !cTLevelManager._UnlockAllLevels);
			RefreshWidget(item, levelData, isLocked);
		}
	}

	private void RefreshWidget(KAWidget widget, LevelData levelData, bool isLocked)
	{
		KAWidget kAWidget = widget.FindChildItem("StarsIcon");
		for (int i = 1; i <= IMLevelProgress.GetStarsCollected(levelData.LevelName); i++)
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("Star" + i);
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: true);
			}
		}
		widget.SetInteractive(isInteractive: true);
		if (!isLocked)
		{
			widget.FindChildItem("AniLockedLevel").SetVisibility(inVisible: false);
			widget.FindChildItem("AniLevelNo").SetVisibility(inVisible: true);
		}
	}

	private void DisplayLevelNo(KAWidget widget, int levelNumber)
	{
		int num = 0;
		int num2;
		for (num2 = levelNumber; num2 > 0; num2 /= 10)
		{
			num++;
		}
		KAWidget kAWidget = widget.FindChildItem("AniLevel_" + num);
		if (!(kAWidget != null))
		{
			return;
		}
		kAWidget.SetVisibility(inVisible: true);
		num2 = levelNumber;
		int num3 = 0;
		while (num2 > 0)
		{
			int num4 = num2 % 10;
			num2 /= 10;
			KAWidget kAWidget2 = kAWidget.FindChildItem("AniLevelPos_" + num3);
			if (kAWidget2 != null)
			{
				kAWidget2.pBackground.UpdateSprite("AniDWDragonsIMLvlNumb" + num4);
			}
			num3++;
		}
	}

	protected override void Start()
	{
		base.Start();
		mBackBtn = FindItem("BtnBack");
		mAniPageCount = FindItem("AniPageCount");
		UiIncredibleMachinesLevelSelectionMenu uiLevelSelectionMenu = _UiLevelSelectionMenu;
		uiLevelSelectionMenu.onPageChange = (KAUIMenu.OnPageChange)Delegate.Combine(uiLevelSelectionMenu.onPageChange, new KAUIMenu.OnPageChange(OnPageChange));
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBackBtn)
		{
			((CTLevelManager)IMGLevelManager.pInstance).QuitGame();
		}
		else if (item == _UiPageTabMenu.GetClickedItem())
		{
			_UiLevelSelectionMenu.GoToPage(_UiPageTabMenu.GetSelectedItemIndex() + 1);
		}
		IncredibleMachineLevelUserData incredibleMachineLevelUserData = (IncredibleMachineLevelUserData)item.GetUserData();
		if (incredibleMachineLevelUserData == null)
		{
			return;
		}
		LevelData obj = CTLevelManager.pLevelData.Levels[incredibleMachineLevelUserData._Index];
		CTLevelManager cTLevelManager = (CTLevelManager)IMGLevelManager.pInstance;
		if (!obj.MemberOnly || cTLevelManager._UnlockAllLevels || UnlockManager.IsSceneUnlocked(cTLevelManager._GameModuleName, inShowUi: false, delegate(bool success)
		{
			if (success)
			{
				OnClick(item);
			}
		}))
		{
			if (IMLevelProgress.pInstance.GetLastLevelPlayed() + 1 >= incredibleMachineLevelUserData._Level || cTLevelManager._UnlockAllLevels)
			{
				cTLevelManager.pCurrentLevelIndex = incredibleMachineLevelUserData._Index;
				cTLevelManager.SetupLevel();
				SetVisibility(inVisible: false);
			}
			else
			{
				ShowLockedItemClickDB();
			}
		}
	}

	private void ShowLockedItemClickDB()
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		kAUIGenericDB.SetText(_LockedItemClickText.GetLocalizedString(), interactive: false);
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	private void OnPageChange(int pageNumber)
	{
		mAniPageCount.SetText(pageNumber + "/" + _UiLevelSelectionMenu.GetPageCount());
		if (mPageNumber != pageNumber && mPageNumber > 0)
		{
			KAWidget itemAt = _UiPageTabMenu.GetItemAt(mPageNumber - 1);
			if (itemAt != null)
			{
				itemAt.SetDisabled(isDisabled: false);
			}
		}
		KAWidget itemAt2 = _UiPageTabMenu.GetItemAt(pageNumber - 1);
		if (itemAt2 != null)
		{
			itemAt2.SetDisabled(isDisabled: true);
		}
		mPageNumber = pageNumber;
	}
}
