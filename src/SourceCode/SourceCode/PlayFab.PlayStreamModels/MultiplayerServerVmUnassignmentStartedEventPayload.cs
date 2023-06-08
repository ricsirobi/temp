using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerVmUnassignmentStartedEventPayload
{
	public double AssignmentDurationMs;

	public string BillingAssignmentCorrelationId;

	public string BuildId;

	public AzureRegion? Region;

	public string SessionId;

	public DateTime UnassignmentEventTimestamp;

	public string VmId;

	public string VmOs;
}
