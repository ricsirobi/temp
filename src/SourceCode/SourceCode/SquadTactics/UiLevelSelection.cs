using System.Collections;
using SOD.Event;
using UnityEngine;

namespace SquadTactics;

public class UiLevelSelection : KAUI
{
	public string _TutorialLevel;

	private UiLevelSelectionMenu mSTLevelSelectionMenu;

	private UiRealmMenu mSTRealmMenu;

	private KAWidget mTxtRealmTitle;

	private KAWidget mTxtInfoOfRealm;

	private KAWidget mTxtLevelDescription;

	private KAWidget mBtnBack;

	private KAWidget mBtnPlay;

	private KAWidget mBtnTutorial;

	private bool mUpdateRealmInfo;

	private int mRealmIndex;

	private Realm mSelectedRealm;

	private int mRealmLevelIndex = -1;

	private int mRealmLevelUnlockedIndex = -1;

	private int mMissionIndex = -1;

	protected override void Start()
	{
		base.Start();
		mTxtRealmTitle = FindItem("TxtRealmTitle");
		mTxtInfoOfRealm = FindItem("TxtInfoOfRealm");
		mTxtLevelDescription = FindItem("TxtLevelDescription");
		mBtnBack = FindItem("BtnQuitGame");
		mBtnPlay = FindItem("BtnPlay");
		mBtnTutorial = FindItem("BtnTutorial");
		ResetPlayButton();
		mBtnTutorial.SetDisabled(isDisabled: true);
		mUpdateRealmInfo = true;
		mSTLevelSelectionMenu = (UiLevelSelectionMenu)GetMenuByIndex(0);
		mSTRealmMenu = (UiRealmMenu)GetMenuByIndex(1);
		LevelManager.pInstance.Init();
	}

	public void Initialize()
	{
		RsResourceManager.DestroyLoadScreen();
		InitializeRealmsMenu();
		SelectRealm();
		InitializeLevels();
		SelectLevel(mRealmLevelIndex);
		mUpdateRealmInfo = false;
		if (RealmHolder.pInstance.pSaveLevel)
		{
			RealmHolder.pInstance.pSaveLevel = false;
			mRealmLevelIndex = RealmHolder.pInstance.pRealmLevelIndex;
			StartCoroutine(CheckAndOpenTeamSelction());
		}
	}

	private IEnumerator CheckAndOpenTeamSelction()
	{
		yield return new WaitForEndOfFrame();
		if (RealmHolder.pInstance.pShowAgeUp)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			while (SanctuaryManager.pCurPetInstance == null)
			{
				yield return null;
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		OpenTeamSelection();
	}

	private void InitializeRealmsMenu()
	{
		SetDefaultRealm();
		for (int i = 0; i < LevelManager.pInstance.pSTLevelData.RealmsData.Length; i++)
		{
			Realm realm = LevelManager.pInstance.pSTLevelData.RealmsData[i];
			if (!string.IsNullOrEmpty(realm.Event))
			{
				EventManager eventManager = EventManager.Get(realm.Event);
				if (eventManager == null || !eventManager.EventInProgress() || eventManager.GracePeriodInProgress())
				{
					if (mRealmIndex == i)
					{
						mRealmIndex++;
					}
					continue;
				}
			}
			string localizedString = realm.Name.GetLocalizedString();
			KAWidget kAWidget = mSTRealmMenu.AddWidget(localizedString);
			kAWidget.SetText(localizedString);
			kAWidget.SetVisibility(inVisible: true);
			KAWidget kAWidget2 = kAWidget.FindChildItem("RealmIcon");
			if (!string.IsNullOrEmpty(realm.RealmIcon) && kAWidget2 != null)
			{
				kAWidget2.SetTextureFromBundle(realm.RealmIcon);
			}
			KAWidgetUserData userData = new KAWidgetUserData(i);
			kAWidget.SetUserData(userData);
			if (mRealmIndex == i)
			{
				mSTRealmMenu.SetSelectedItem(kAWidget);
				SetRealm(mRealmIndex);
			}
		}
	}

	private void InitializeLevels()
	{
		Task playerActiveTask = MissionManagerDO.GetPlayerActiveTask();
		KAWidget kAWidget = null;
		for (int i = 0; i < mSelectedRealm.RealmLevelsInfo.Length; i++)
		{
			string localizedString = mSelectedRealm.RealmLevelsInfo[i].Name.GetLocalizedString();
			KAWidget kAWidget2 = mSTLevelSelectionMenu.AddWidget(localizedString);
			kAWidget2.SetText(localizedString);
			((KAToggleButton)kAWidget2).SetChecked(isChecked: false);
			KAWidget kAWidget3 = kAWidget2.FindChildItem("IcoLevel");
			if (!string.IsNullOrEmpty(mSelectedRealm.RealmLevelsInfo[i].LevelIcon))
			{
				kAWidget3.SetTextureFromBundle(mSelectedRealm.RealmLevelsInfo[i].LevelIcon);
			}
			kAWidget2.SetVisibility(inVisible: true);
			if (LevelManager.pInstance.IsTutorialFinished() && playerActiveTask != null && playerActiveTask.Match("Game", "Name", mSelectedRealm.RealmLevelsInfo[i].Scenes[0]) && playerActiveTask.Completed == 0)
			{
				kAWidget = kAWidget2;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (mSelectedRealm.RealmLevelsInfo[i].TaskID.HasValue)
			{
				flag2 = true;
				flag3 = true;
				int taskID = mSelectedRealm.RealmLevelsInfo[i].TaskID.Value;
				if (null != MissionManager.pInstance)
				{
					if (MissionManager.pInstance.pActiveTasks.Exists((Task t) => t.TaskID == taskID))
					{
						flag3 = false;
						KAWidget kAWidget4 = kAWidget2.FindChildItem("IcoMission");
						if (kAWidget4 != null)
						{
							Task task = MissionManager.pInstance.GetTask(taskID);
							Mission rootMission = MissionManager.pInstance.GetRootMission(task);
							string text = ((rootMission != null) ? rootMission.pData.Icon : task._Mission.pData.Icon);
							kAWidget4.SetVisibility(inVisible: false);
							if (!string.IsNullOrEmpty(text))
							{
								if (text.StartsWith("http://"))
								{
									kAWidget4.SetTextureFromURL(text, base.gameObject);
								}
								else
								{
									string[] array = text.Split('/');
									if (array.Length >= 3)
									{
										kAWidget4.SetTextureFromBundle(array[0] + "/" + array[1], array[2], base.gameObject);
									}
								}
							}
						}
					}
					else
					{
						Task task2 = MissionManager.pInstance.GetTask(taskID);
						if (task2 != null && task2.pCompleted)
						{
							flag3 = false;
						}
					}
				}
			}
			if (!LevelManager.pUnlockLevels)
			{
				flag = !LevelManager.pInstance.IsTutorialFinished() || ((!flag2) ? (mRealmLevelUnlockedIndex < i) : flag3);
			}
			kAWidget2.FindChildItem("IcoLock").SetVisibility(flag);
			LevelUserData userData = new LevelUserData(i, flag);
			kAWidget2.SetUserData(userData);
		}
		if (kAWidget != null)
		{
			kAWidget.PlayAnim("Flash");
			mSTLevelSelectionMenu.FocusWidget(kAWidget);
		}
	}

	private void OnTextureLoaded(KAWidget inWidget)
	{
		inWidget.SetVisibility(inVisible: true);
	}

	public void SelectLevel(int inLevelIndex)
	{
		RealmHolder.pInstance.pRealmLevelIndex = (mRealmLevelIndex = inLevelIndex);
		if (mRealmLevelIndex == -1)
		{
			return;
		}
		if (mMissionIndex != -1 && LevelManager.pInstance.IsTutorialFinished())
		{
			Task playerActiveTask = MissionManagerDO.GetPlayerActiveTask();
			for (int i = 0; i < mSTLevelSelectionMenu.GetItems().Count; i++)
			{
				KAWidget kAWidget = mSTLevelSelectionMenu.GetItems()[i];
				if (mSTLevelSelectionMenu.GetSelectedItem() != kAWidget && playerActiveTask != null && playerActiveTask.Match("Game", "Name", mSelectedRealm.RealmLevelsInfo[i].Scenes[0]) && playerActiveTask.Completed == 0)
				{
					kAWidget.PlayAnim("Flash");
				}
				else if (i != inLevelIndex)
				{
					kAWidget.PlayAnim("Normal");
				}
			}
		}
		UpdateSelectedLevelDescription(mRealmLevelIndex);
	}

	public void ResetSelectedLevelDescription()
	{
		UpdateSelectedLevelDescription(mRealmLevelIndex);
	}

	public void UpdateSelectedLevelDescription(int inLvlIndex, bool inLocked = false)
	{
		if (inLvlIndex == -1)
		{
			mTxtLevelDescription.SetText(string.Empty);
		}
		else if (mSelectedRealm.RealmLevelsInfo != null && mSelectedRealm.RealmLevelsInfo.Length > inLvlIndex)
		{
			string text = mSelectedRealm.RealmLevelsInfo[inLvlIndex].Description.GetLocalizedString();
			if (inLocked)
			{
				text = text + " " + mSelectedRealm.RealmLevelsInfo[inLvlIndex].LockedDescription.GetLocalizedString();
			}
			mTxtLevelDescription.SetText(text);
		}
		else
		{
			UtDebug.LogError("Realm Level Data is null please verify Level Data XML");
		}
	}

	private void SetDefaultRealm()
	{
		if (!IsQuestLevelActive())
		{
			if (RealmHolder.pInstance.pSaveLevel)
			{
				SetRealm(RealmHolder.pInstance.pRealmIndex);
				mRealmLevelIndex = RealmHolder.pInstance.pRealmLevelIndex;
			}
			else if (LevelManager.pInstance.pRealmIndex == -1)
			{
				SetRealm(GetRealmIndexByName(LevelManager.pRealmToSelect));
			}
			else
			{
				SetRealm(LevelManager.pInstance.pRealmIndex);
			}
		}
	}

	private void SetRealm(int index)
	{
		if (mMissionIndex != -1 && LevelManager.pInstance.IsTutorialFinished())
		{
			for (int i = 0; i < mSTRealmMenu.GetItems().Count; i++)
			{
				KAWidget kAWidget = mSTRealmMenu.GetItems()[i];
				if (kAWidget != null)
				{
					if (mMissionIndex == i && i != index)
					{
						kAWidget.PlayAnim("Flash");
					}
					else
					{
						kAWidget.PlayAnim("Normal");
					}
				}
			}
		}
		RealmHolder.pInstance.pRealmIndex = (mRealmIndex = index);
	}

	private bool IsQuestLevelActive()
	{
		int num = LevelManager.pInstance.pSTLevelData.RealmsData.Length;
		for (int i = 0; i < num; i++)
		{
			Realm realm = LevelManager.pInstance.pSTLevelData.RealmsData[i];
			for (int j = 0; j < realm.RealmLevelsInfo.Length; j++)
			{
				Task playerActiveTask = MissionManagerDO.GetPlayerActiveTask();
				if (playerActiveTask != null && playerActiveTask.Match("Game", "Name", realm.RealmLevelsInfo[j].Scenes[0]) && playerActiveTask.Completed == 0)
				{
					mMissionIndex = i;
					SetRealm(i);
					return true;
				}
			}
		}
		return false;
	}

	private void SelectRealm()
	{
		if (LevelManager.pInstance.pSTLevelData.RealmsData[mRealmIndex] != null)
		{
			mSelectedRealm = LevelManager.pInstance.pSTLevelData.RealmsData[mRealmIndex];
			mRealmLevelUnlockedIndex = LevelManager.pInstance.pRealmLevelsUnlockInfo[mRealmIndex];
			mTxtRealmTitle.SetText(mSelectedRealm.Name.GetLocalizedString());
			mTxtInfoOfRealm.SetText(mSelectedRealm.Description.GetLocalizedString());
		}
		else
		{
			UtDebug.LogError("Realm Data is null please verify Level Data XML");
		}
	}

	private int GetRealmIndexByName(string inRealmName)
	{
		if (!string.IsNullOrEmpty(inRealmName) && LevelManager.pInstance.pSTLevelData != null)
		{
			for (int i = 0; i < LevelManager.pInstance.pSTLevelData.RealmsData.Length; i++)
			{
				if (LevelManager.pInstance.pSTLevelData.RealmsData[i] != null && inRealmName == LevelManager.pInstance.pSTLevelData.RealmsData[i].Name._Text)
				{
					return i;
				}
			}
		}
		return 0;
	}

	public void SelectAndShowLevelsForRealm(int inRealmIndex)
	{
		if (LevelManager.pRefreshLevels)
		{
			LevelManager.pRefreshLevels = false;
		}
		else if (mRealmIndex == inRealmIndex)
		{
			return;
		}
		SetRealm(inRealmIndex);
		RealmHolder.pInstance.pRealmLevelIndex = (mRealmLevelIndex = -1);
		SelectRealm();
		mSTLevelSelectionMenu.ClearItems();
		InitializeLevels();
		ResetSelectedLevelDescription();
		ResetPlayButton();
	}

	private void ResetPlayButton()
	{
		mBtnPlay.SetDisabled(isDisabled: true);
		LevelManager.pInstance.pEnablePlayBtn = true;
	}

	protected override void Update()
	{
		base.Update();
		if (mUpdateRealmInfo && LevelManager.pInstance.pLevelDataReady)
		{
			Initialize();
		}
		if (LevelManager.pInstance.pEnablePlayBtn)
		{
			if (mBtnTutorial.GetState() == KAUIState.DISABLED)
			{
				mBtnTutorial.SetDisabled(isDisabled: false);
				if (!LevelManager.pInstance.IsTutorialFinished())
				{
					mBtnTutorial.PlayAnim("Flash");
				}
			}
			if (mRealmLevelIndex >= 0)
			{
				mBtnPlay.SetDisabled(isDisabled: false);
				LevelManager.pInstance.pEnablePlayBtn = false;
			}
		}
		if (LevelManager.pInstance.pLevelDataReady && LevelManager.pRefreshLevels)
		{
			SelectAndShowLevelsForRealm(mRealmIndex);
		}
	}

	public void OpenTeamSelection()
	{
		SetVisibility(inVisible: false);
		LevelManager.pInstance._TeamSelection.OpenTeamSelectionTab();
		LevelManager.pInstance.LevelPrefetch(mRealmIndex);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mBtnPlay)
		{
			OpenTeamSelection();
		}
		else if (item == mBtnBack)
		{
			LevelManager.pInstance.DeleteInstance();
			CharacterDatabase.pInstance.DeleteInstance();
			WeaponDatabase.pInstance.DeleteInstance();
			PrefetchManager.Kill();
			RsResourceManager.LoadLevel(LevelManager.pInstance.pLastLevel);
			RealmHolder.pInstance.ResetRealmValue();
			RealmHolder.pInstance.DestroyRealmObj();
		}
		else if (item == mBtnTutorial)
		{
			RsResourceManager.LoadLevel(_TutorialLevel, skipMMOLogin: true);
		}
	}

	public void LoadLevel()
	{
		LevelManager.pInstance.SetLevelPlaying(mRealmIndex, mRealmLevelIndex);
		LevelManager.pInstance.LoadLevel();
	}

	public RealmLevel GetSelectedLevel()
	{
		return mSelectedRealm.RealmLevelsInfo[mRealmLevelIndex];
	}
}
