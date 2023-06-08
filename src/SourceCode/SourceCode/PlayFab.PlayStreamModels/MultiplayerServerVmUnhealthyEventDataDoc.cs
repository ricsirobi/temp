using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerVmUnhealthyEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerVmUnhealthyEventPayload Payload;

	public EntityKey WriterEntity;
}
