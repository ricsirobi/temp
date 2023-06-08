using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MatchmakingUserTicketCompletePayload
{
	public string CancellationReason;

	public string MatchId;

	public string QueueName;

	public string Result;

	public string TicketId;
}
