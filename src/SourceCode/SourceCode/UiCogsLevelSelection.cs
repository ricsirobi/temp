using System;
using UnityEngine;

public class UiCogsLevelSelection : KAUI
{
	public LocaleString _LockedItemClickText = new LocaleString("You need to beat previous levels to unlock this level.");

	public UiCogsLevelSelectionMenu _UiCogsLevelSelectionMenu;

	public KAUIMenu _UiPageTabMenu;

	private KAWidget mBackBtn;

	private KAWidget mAniPageCount;

	private int mPageNumber;

	private CogsLevelData mLevelData;

	public void PopulateLevels(CogsLevelData cogsLevelData, bool forceReload = false)
	{
		if (mLevelData != null && !forceReload)
		{
			RefreshLevels(mLevelData);
			return;
		}
		mLevelData = cogsLevelData;
		_UiCogsLevelSelectionMenu.ClearItems();
		int num = 0;
		int num2 = 1;
		int num3 = CogsLevelProgress.pInstance.GetLastLevelPlayed() + 1;
		bool flag = !UnlockManager.IsSceneUnlocked(CogsGameManager.pInstance._GameModuleName, inShowUi: true);
		CogsLevelDetails[] levels = cogsLevelData.Levels;
		foreach (CogsLevelDetails cogsLevelDetails in levels)
		{
			if (!cogsLevelDetails.IsMissionLevel)
			{
				KAWidget kAWidget = _UiCogsLevelSelectionMenu.AddWidget("TemplateTool");
				CogsLevelUserData userData = new CogsLevelUserData(num, num2);
				string levelName = cogsLevelDetails.LevelName;
				kAWidget.name = levelName;
				bool isLocked = (num2 > num3 || (cogsLevelDetails.MemberOnly && flag)) && !CogsLevelManager.pInstance._UnlockAllLevels;
				KAWidget widget = kAWidget.FindChildItem("AniLevelNo");
				DisplayLevelNo(widget, num2);
				kAWidget.SetUserData(userData);
				RefreshWidget(kAWidget, cogsLevelDetails, isLocked);
				num2++;
			}
			num++;
		}
		_UiPageTabMenu.ClearItems();
		int pageCount = _UiCogsLevelSelectionMenu.GetPageCount();
		for (int j = 0; j < pageCount; j++)
		{
			_UiPageTabMenu.AddWidget(_UiPageTabMenu._Template.name).SetVisibility(inVisible: true);
		}
		mPageNumber = _UiCogsLevelSelectionMenu.GetCurrentPage();
		OnPageChange(mPageNumber);
	}

	public void RefreshLevels(CogsLevelData imLevelData)
	{
		int num = CogsLevelProgress.pInstance.GetLastLevelPlayed() + 1;
		bool flag = !UnlockManager.IsSceneUnlocked(CogsGameManager.pInstance._GameModuleName, inShowUi: true);
		foreach (KAWidget item in _UiCogsLevelSelectionMenu.GetItems())
		{
			CogsLevelUserData cogsLevelUserData = (CogsLevelUserData)item.GetUserData();
			CogsLevelDetails cogsLevelDetails = imLevelData.Levels[cogsLevelUserData._Index];
			bool isLocked = (cogsLevelUserData._Level > num || (cogsLevelDetails.MemberOnly && flag)) && !CogsLevelManager.pInstance._UnlockAllLevels;
			RefreshWidget(item, cogsLevelDetails, isLocked);
		}
	}

	private void RefreshWidget(KAWidget widget, CogsLevelDetails levelData, bool isLocked)
	{
		KAWidget kAWidget = widget.FindChildItem("StarsIcon");
		for (int i = 1; i <= CogsLevelProgress.pInstance.GetStarsCollected(levelData.LevelName); i++)
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
		UiCogsLevelSelectionMenu uiCogsLevelSelectionMenu = _UiCogsLevelSelectionMenu;
		uiCogsLevelSelectionMenu.onPageChange = (KAUIMenu.OnPageChange)Delegate.Combine(uiCogsLevelSelectionMenu.onPageChange, new KAUIMenu.OnPageChange(OnPageChange));
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBackBtn)
		{
			CogsLevelManager.pInstance.QuitGame();
		}
		else if (item == _UiPageTabMenu.GetClickedItem())
		{
			_UiCogsLevelSelectionMenu.GoToPage(_UiPageTabMenu.GetSelectedItemIndex() + 1);
		}
		CogsLevelUserData cogsLevelUserData = (CogsLevelUserData)item.GetUserData();
		if (cogsLevelUserData != null && (!CogsLevelManager.pLevelData.Levels[cogsLevelUserData._Index].MemberOnly || CogsLevelManager.pInstance._UnlockAllLevels || UnlockManager.IsSceneUnlocked(CogsGameManager.pInstance._GameModuleName, inShowUi: false, delegate(bool success)
		{
			if (success)
			{
				OnClick(item);
			}
		})))
		{
			if (CogsLevelProgress.pInstance.GetLastLevelPlayed() + 1 >= cogsLevelUserData._Level || CogsLevelManager.pInstance._UnlockAllLevels)
			{
				SetVisibility(inVisible: false);
				CogsLevelManager.pInstance.LoadLevel(cogsLevelUserData._Index);
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
		mAniPageCount.SetText(pageNumber + "/" + _UiCogsLevelSelectionMenu.GetPageCount());
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
