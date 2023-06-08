namespace PlayFab.PlayStreamModels;

public class MatchmakingMatchFoundEventData : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public MatchmakingMatchFoundPayload Payload;

	public EntityKey WriterEntity;
}
