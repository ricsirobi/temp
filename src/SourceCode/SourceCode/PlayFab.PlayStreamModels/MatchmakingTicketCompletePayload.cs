using System;
using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MatchmakingTicketCompletePayload
{
	public string CancellationReason;

	public DateTime CompletionTime;

	public DateTime CreationTime;

	public string QueueName;

	public string Result;

	public DateTime? SubmissionTime;

	public List<EntityKey> TicketEntities;

	public string TicketId;
}
