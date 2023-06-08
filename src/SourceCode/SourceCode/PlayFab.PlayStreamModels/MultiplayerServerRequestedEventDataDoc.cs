using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerRequestedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerRequestedEventPayload Payload;

	public EntityKey WriterEntity;
}
