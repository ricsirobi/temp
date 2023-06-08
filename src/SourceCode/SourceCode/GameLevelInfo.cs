using System;

[Serializable]
public class GameLevelInfo
{
	public string _Key;

	public int _Count;

	public bool _IsMultiPlayer;

	public HighScoresDifficulty _Difficulty;

	public int _Level;

	public string _Title;

	public int _TitleTextID;

	public bool _AscendingOrder;

	private GameDataSummary mGameDataSummary;

	private GameDataSummary mMyScoreSummary;

	private GameDataSummary mMyBuddyScoreSummary;

	public void SetGameDataSummary(GameDataSummary inDataSummary, HighScoreDisplayPage currPage)
	{
		if (mGameDataSummary == null && currPage == HighScoreDisplayPage.ALLSCORES)
		{
			mGameDataSummary = inDataSummary;
		}
		else if (mMyBuddyScoreSummary == null && currPage == HighScoreDisplayPage.MYBUDDYSCORES)
		{
			mMyBuddyScoreSummary = inDataSummary;
		}
		else if (mMyScoreSummary == null && currPage == HighScoreDisplayPage.MYSCORES)
		{
			mMyScoreSummary = inDataSummary;
		}
	}

	public GameDataSummary GetGameDataSummary(HighScoreDisplayPage currPage)
	{
		return currPage switch
		{
			HighScoreDisplayPage.ALLSCORES => mGameDataSummary, 
			HighScoreDisplayPage.MYBUDDYSCORES => mMyBuddyScoreSummary, 
			_ => mMyScoreSummary, 
		};
	}
}
