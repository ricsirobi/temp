using UnityEngine;

public class GmGameManager : MonoBehaviour
{
	public int _InitScore;

	public int _InitGoalNum;

	public int _InitHealthNum;

	public int _InitLives;

	public string _GoalText = "";

	public string _GoalDesc = "";

	public GameObject _HUDObject;

	public bool _Paused = true;

	public bool _GoalReached;

	public int _GameModuleID;

	public string _GameModuleName;

	public string _ChallengeFriendGameModuleName;

	public int _AchievementID;

	public GmGameManagerTimeBonus[] _TimeBonus;

	protected int _Lives;

	public virtual void ResetGame()
	{
		_Paused = true;
		_Lives = _InitLives;
		if (_HUDObject != null)
		{
			_HUDObject.SendMessage("SetScore", _InitScore);
			_HUDObject.SendMessage("SetGoalText", _GoalText);
			_HUDObject.SendMessage("SetGoal", _InitGoalNum);
			_HUDObject.SendMessage("SetMaxHealth", _InitHealthNum);
			_HUDObject.SendMessage("SetGoalDesc", _GoalDesc);
			_HUDObject.SendMessage("SetLives", _InitLives);
		}
		_GoalReached = false;
	}

	public virtual void PauseGame(bool t)
	{
		_Paused = t;
		if (_HUDObject != null)
		{
			_HUDObject.SendMessage("PauseTimer", t);
		}
	}

	public virtual void OnCollect(GameObject collectObject)
	{
	}

	public virtual void OnLevelReady()
	{
		if (_HUDObject == null)
		{
			_HUDObject = AvAvatar.pToolbar;
		}
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnHelp()
	{
	}

	public virtual void OnHelpExit()
	{
	}

	public virtual void OnStartGame()
	{
		PauseGame(t: false);
	}

	public virtual void OnTimeUp()
	{
		PauseGame(t: true);
	}

	public virtual void OnTimeMarker(float t)
	{
	}

	public virtual void OnGoalUpdate(int gcount)
	{
		if (gcount == 0)
		{
			_GoalReached = true;
		}
	}

	public virtual void OnHealthUpdate(int hcount)
	{
	}

	public virtual int GetTimeBonus(float time)
	{
		if (_TimeBonus != null)
		{
			return GetTimeBonus(_TimeBonus, time);
		}
		return 0;
	}

	public virtual int GetTimeBonus(GmGameManagerTimeBonus[] timeBonus, float time)
	{
		if (timeBonus != null)
		{
			for (int i = 0; i < timeBonus.Length; i++)
			{
				if (timeBonus[i].IsInRange(time))
				{
					return timeBonus[i]._Points;
				}
			}
		}
		return 0;
	}
}
