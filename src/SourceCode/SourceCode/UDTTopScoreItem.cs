using System;

[Serializable]
public class UDTTopScoreItem
{
	public string _Title = "ULTIMATE\nDRAGON\nTRAINER";

	public int _TitleID;

	public RequestType _PageType = RequestType.All;

	public ModeType _ModeType = ModeType.AllTime;

	public int _Count = 10;

	public bool _Refresh;

	public UDTTopScore[] mScores;

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
