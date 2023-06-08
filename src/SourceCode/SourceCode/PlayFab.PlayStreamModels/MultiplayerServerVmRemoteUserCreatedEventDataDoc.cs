using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerVmRemoteUserCreatedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerVmRemoteUserCreatedEventPayload Payload;

	public EntityKey WriterEntity;
}
