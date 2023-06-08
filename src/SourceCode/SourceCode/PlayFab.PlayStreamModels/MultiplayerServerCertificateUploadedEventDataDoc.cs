using System;

namespace PlayFab.PlayStreamModels;

public class MultiplayerServerCertificateUploadedEventDataDoc : PlayStreamEventBase
{
	public EntityLineage EntityLineage;

	public string OriginalEventId;

	public DateTime? OriginalTimestamp;

	public MultiplayerServerCertificateUploadedEventPayload Payload;

	public EntityKey WriterEntity;
}
