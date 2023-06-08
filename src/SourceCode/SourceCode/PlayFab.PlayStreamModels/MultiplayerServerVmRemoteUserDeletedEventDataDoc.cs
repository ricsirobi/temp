using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerVmRemoteUserDeletedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerVmRemoteUserDeletedEventPayload Payload;

	public EntityKey WriterEntity;
}
