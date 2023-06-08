using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerBuildRegionStatusChangedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerBuildRegionStatusChangedEventPayload Payload;

	public EntityKey WriterEntity;
}
