using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerStateChangedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerStateChangedEventPayload Payload;

	public EntityKey WriterEntity;
}
