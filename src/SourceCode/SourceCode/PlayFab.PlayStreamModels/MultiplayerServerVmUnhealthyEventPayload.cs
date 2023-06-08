using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerVmUnhealthyEventPayload
{
	public string BuildId;

	public string HealthStatus;

	public AzureRegion? Region;

	public string VmId;
}
