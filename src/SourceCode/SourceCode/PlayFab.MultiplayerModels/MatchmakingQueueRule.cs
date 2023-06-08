using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class MatchmakingQueueRule : PlayFabBaseModel
{
	public string Name;

	public uint? SecondsUntilOptional;

	public RuleType Type;
}
