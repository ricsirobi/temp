using System;
using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MatchmakingMatchFoundPayload
{
	public string MatchId;

	public string QueueName;

	public List<string> TicketIds;
}
