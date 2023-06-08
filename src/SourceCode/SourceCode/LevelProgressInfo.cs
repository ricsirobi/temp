using System;

[Serializable]
public class LevelProgressInfo
{
	public int _Level;

	public LevelInfo[] _LevelInfo;

	private int mCurrentLevel = 1;

	public int pCurrentLevel
	{
		get
		{
			return mCurrentLevel;
		}
		set
		{
			mCurrentLevel = value;
		}
	}

	public bool Upgrade()
	{
		return true;
	}

	public float GetValue(string inKey)
	{
		float result = 0f;
		LevelInfo[] levelInfo = _LevelInfo;
		foreach (LevelInfo levelInfo2 in levelInfo)
		{
			if (levelInfo2._PlayerLevel == pCurrentLevel)
			{
				result = levelInfo2.GetValue(inKey);
				break;
			}
		}
		return result;
	}
}
