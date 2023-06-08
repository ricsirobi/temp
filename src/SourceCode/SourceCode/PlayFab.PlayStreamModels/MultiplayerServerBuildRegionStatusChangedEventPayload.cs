using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerBuildRegionStatusChangedEventPayload
{
	public string BuildId;

	public double MinutesInOldStatus;

	public string NewStatus;

	public string OldStatus;

	public AzureRegion? Region;
}
