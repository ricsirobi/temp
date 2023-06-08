using System;
using System.Collections.Generic;

namespace SOD.Event;

[Serializable]
public class RewardsMap
{
	public string _ModuleName;

	public int _GameID;

	public LocaleString _RewardDisplayText;

	public List<RewardAchievementMap> _Maps;
}
