namespace PlayFab.PlayStreamModels;

public class MatchmakingUserTicketInviteEventData : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public MatchmakingUserTicketInvitePayload Payload;

	public EntityKey WriterEntity;
}
