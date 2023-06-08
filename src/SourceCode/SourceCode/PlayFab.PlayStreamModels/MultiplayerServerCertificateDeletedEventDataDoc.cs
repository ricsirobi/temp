using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerCertificateDeletedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerCertificateDeletedEventPayload Payload;

	public EntityKey WriterEntity;
}
