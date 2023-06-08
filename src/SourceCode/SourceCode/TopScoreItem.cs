using System;

[Serializable]
public class TopScoreItem
{
	public string _Title = "RED ALERT\nTOP\nSCORES";

	public int _TitleID;

	public int _GameID = 19;

	public bool _Multiplayer;

	public HighScoresDifficulty _Difficulty;

	public int _Level = 1;

	public string _Key = "highscore";

	public int _Count = 10;

	public bool _Refresh;

	public TopScore[] mScores;

	[NonSerialized]
	public float mUpdateTimer;

	public void CheckTitle()
	{
		if (_TitleID > 0)
		{
			_Title = StringTable.GetStringData(_TitleID, _Title);
			_TitleID = 0;
		}
	}

	public void UpdateScore(float t)
	{
		mUpdateTimer = t;
	}
}
