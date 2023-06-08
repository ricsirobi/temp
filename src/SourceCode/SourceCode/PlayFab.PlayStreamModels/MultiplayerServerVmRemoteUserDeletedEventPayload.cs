using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerVmRemoteUserDeletedEventPayload
{
	public string BuildId;

	public string Username;

	public string VmId;
}
