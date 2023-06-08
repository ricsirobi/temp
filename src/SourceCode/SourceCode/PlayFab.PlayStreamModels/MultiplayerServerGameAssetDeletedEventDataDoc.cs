using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerGameAssetDeletedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerGameAssetDeletedEventPayload Payload;

	public EntityKey WriterEntity;
}
