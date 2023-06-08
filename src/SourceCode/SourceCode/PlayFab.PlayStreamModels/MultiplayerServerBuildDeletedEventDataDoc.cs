using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerBuildDeletedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerBuildDeletedEventPayload Payload;

	public EntityKey WriterEntity;
}
