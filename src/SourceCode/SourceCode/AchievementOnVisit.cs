using System;
using System.Collections.Generic;

[Serializable]
public class AchievementOnVisit
{
	public List<string> _IgnoreFromLevels;

	public int _AchievementId = -1;

	public int _AchievementInfoID;

	public string _RelatedID = string.Empty;
}
