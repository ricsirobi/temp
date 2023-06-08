using System;

namespace PlayFab.PlayStreamModels;

public class ClientSessionStartEventData : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime OriginalTimestamp;

	public ClientSessionStartPayload Payload;

	public EntityKey WriterEntity;
}
