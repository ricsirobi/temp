using System;

namespace PlayFab.PlayStreamModels;

public class ClientFocusChangeEventData : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public ClientFocusChangePayload Payload;

	public EntityKey WriterEntity;
}
