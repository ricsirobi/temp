using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerVmAssignedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerVmAssignedEventPayload Payload;

	public EntityKey WriterEntity;
}
