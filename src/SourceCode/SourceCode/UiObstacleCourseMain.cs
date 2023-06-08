using UnityEngine;

public class UiObstacleCourseMain : KAUI
{
	private static string mLastLevel;

	public ObstacleCourseLevelManager _LevelManager;

	public TextAsset _TutorialTextAsset;

	public string _Intro = "tnFreeFallMtnMember";

	public string _ExitMarker = "PfMarker_FlightSchoolDOExit";

	private CoIdleManager mIdleMgr;

	private KAWidget mHelpBtn;

	private KAWidget mBackBtn;

	public KAWidget pBackBtn => mBackBtn;

	protected override void Start()
	{
		base.Start();
		mBackBtn = FindItem("BackBtn");
		mHelpBtn = FindItem("HelpBtn");
		mIdleMgr = (CoIdleManager)base.gameObject.GetComponent(typeof(CoIdleManager));
		if (RsResourceManager.pCurrentLevel != RsResourceManager.pLastLevel && mLastLevel != RsResourceManager.pLastLevel)
		{
			mLastLevel = RsResourceManager.pLastLevel;
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		Input.ResetInputAxes();
		if (item == mBackBtn)
		{
			TutorialManager.StopTutorials();
			base.gameObject.SetActive(value: false);
			ObstacleCourseLevelManager.ScoreData.mCount = 0;
			ObstacleCourseLevelManager.ScoreData.mDataReturnFail = false;
			if (!string.IsNullOrEmpty(_ExitMarker))
			{
				AvAvatar.pStartLocation = _ExitMarker;
			}
			base.gameObject.SetActive(value: false);
			if (!(_LevelManager != null))
			{
				return;
			}
			_LevelManager.pFlightSchoolGameData = null;
			if (_LevelManager.pGameMode == FSGameMode.FLIGHT_MODE)
			{
				ObstacleCourseLevelManager.mMenuState = FSMenuState.FS_STATE_DRAGONSELECT;
				_LevelManager._DragonSelectionUi.pSelectedTicketID = 0;
				if (SanctuaryManager.pCurPetInstance != null)
				{
					Object.Destroy(SanctuaryManager.pCurPetInstance.gameObject);
					SanctuaryManager.pCurPetInstance = null;
				}
				SanctuaryManager.pInstance.ReloadPet();
				_LevelManager._DragonSelectionUi.Init(_LevelManager);
			}
			else if (_LevelManager.pGameMode == FSGameMode.GLIDE_MODE)
			{
				ObstacleCourseLevelManager.mMenuState = FSMenuState.FS_STATE_MODESELECT;
				_LevelManager._GameModeSelectionUi.Init(_LevelManager);
			}
			else if (_LevelManager.pGameMode == FSGameMode.FLIGHT_SUIT_MODE)
			{
				ObstacleCourseLevelManager.mMenuState = FSMenuState.FS_STATE_MODESELECT;
				_LevelManager._GameModeSelectionUi.Init(_LevelManager);
			}
		}
		else if (item == mHelpBtn)
		{
			mIdleMgr.OnIdlePlay();
		}
	}

	public void OnEnable()
	{
		if (_TutorialTextAsset != null)
		{
			TutorialManager.StartTutorial(_TutorialTextAsset, _Intro, bMarkDone: false, 12u, null);
		}
	}
}
