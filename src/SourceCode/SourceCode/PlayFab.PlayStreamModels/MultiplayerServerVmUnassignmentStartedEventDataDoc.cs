using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerVmUnassignmentStartedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerVmUnassignmentStartedEventPayload Payload;

	public EntityKey WriterEntity;
}
