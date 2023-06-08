using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerBuildRegionUpdatedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerBuildRegionUpdatedEventPayload Payload;

	public EntityKey WriterEntity;
}
