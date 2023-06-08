namespace PlayFab.PlayStreamModels;

public class MatchmakingTicketCompleteEventData : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public MatchmakingTicketCompletePayload Payload;

	public EntityKey WriterEntity;
}
