namespace PlayFab.PlayStreamModels;

public class MatchmakingUserTicketCompleteEventData : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public MatchmakingUserTicketCompletePayload Payload;

	public EntityKey WriterEntity;
}
