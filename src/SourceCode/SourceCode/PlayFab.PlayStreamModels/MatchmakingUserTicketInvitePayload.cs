using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MatchmakingUserTicketInvitePayload
{
	public EntityKey CreatorEntity;

	public string QueueName;

	public string TicketId;
}
