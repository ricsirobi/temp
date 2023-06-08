using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerStateChangedEventPayload
{
	public string BuildId;

	public string NewState;

	public string OldState;

	public AzureRegion? Region;

	public string ServerId;

	public string SessionId;

	public string VmId;
}
