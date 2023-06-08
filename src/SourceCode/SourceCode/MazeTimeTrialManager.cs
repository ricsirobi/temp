using UnityEngine;

public class MazeTimeTrialManager : MonoBehaviour
{
	public string _GameModuleName = "DOMazeTimeTrial";

	public int _GameID;

	public int _LevelID;

	public int _FirstTimeMissionID;

	public string _ExitLevel = "HubSchoolDO";

	public LevelEndTrigger _TimeTrialEndMarker;

	public GameObject _QuestObject;

	public GameObject _FinishObject;

	public TimeBrackets _TimeBrackets = new TimeBrackets();

	[Tooltip("Set to true if you want members to have different awards")]
	public bool _MembersHaveDifferentPayout;

	public UiMazeTimeTrial _TimeTrialUiManager;

	public UiCountDown _UiCountdown;

	public UIEventRewardAchievement _UiEventRewardDB;

	private float mTime;

	private bool mTimeTrialActive;

	public float pTime
	{
		get
		{
			return mTime;
		}
		set
		{
			mTime = value;
		}
	}

	public bool pTimeTrialActive
	{
		get
		{
			return mTimeTrialActive;
		}
		set
		{
			mTimeTrialActive = value;
		}
	}

	private void Start()
	{
		ShowGameModeUI();
	}

	private void Update()
	{
		if ((bool)_TimeTrialUiManager && mTimeTrialActive)
		{
			_TimeTrialUiManager.UpdateTime();
		}
	}

	public void ShowGameModeUI()
	{
		if (_TimeTrialUiManager != null)
		{
			_TimeTrialUiManager.Init(this);
		}
		else
		{
			UtDebug.LogError("_TimeTrialUiManager is null");
		}
	}

	private void OnLevelReady()
	{
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
		AvAvatar.pLevelState = AvAvatarLevelState.MAZETIMETRIAL;
		if ((bool)_TimeTrialUiManager)
		{
			_TimeTrialUiManager.InitChallenge();
		}
	}

	public void EndGame()
	{
		if ((bool)_TimeTrialUiManager)
		{
			_TimeTrialUiManager.EndTimeTrial();
		}
	}

	public string CalculateGrade()
	{
		try
		{
			for (int i = 0; i < _TimeBrackets._Time.Count; i++)
			{
				if (mTime < _TimeBrackets._Time[i])
				{
					return _TimeBrackets._Grade[i];
				}
			}
			return "D";
		}
		catch
		{
			return "D";
		}
	}

	public string GradeColor(string inGrade)
	{
		return inGrade switch
		{
			"A" => "Green", 
			"B" => "Yellow", 
			"C" => "Yellow", 
			"D" => "Red", 
			_ => "", 
		};
	}

	public int GradeToPayout(string inGrade)
	{
		return inGrade switch
		{
			"A" => 1, 
			"B" => 2, 
			"C" => 3, 
			"D" => 4, 
			_ => -1, 
		};
	}
}
