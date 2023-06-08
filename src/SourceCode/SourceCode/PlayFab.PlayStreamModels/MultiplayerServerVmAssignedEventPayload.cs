using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerVmAssignedEventPayload
{
	public DateTime AssignmentEventTimestamp;

	public string BillingAssignmentCorrelationId;

	public string BuildId;

	public AzureRegion? Region;

	public string SessionId;

	public string VmId;
}
