using System;
using System.Collections.Generic;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class MatchmakingQueueConfig : PlayFabBaseModel
{
	public string BuildId;

	public uint MaxMatchSize;

	public uint MinMatchSize;

	public string Name;

	public List<MatchmakingQueueRule> Rules;

	public bool ServerAllocationEnabled;

	public StatisticsVisibilityToPlayers StatisticsVisibilityToPlayers;

	public List<MatchmakingQueueTeam> Teams;
}
