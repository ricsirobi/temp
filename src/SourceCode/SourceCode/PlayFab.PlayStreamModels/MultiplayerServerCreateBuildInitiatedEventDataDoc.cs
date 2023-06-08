using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerCreateBuildInitiatedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerCreateBuildInitiatedEventPayload Payload;

	public EntityKey WriterEntity;
}
