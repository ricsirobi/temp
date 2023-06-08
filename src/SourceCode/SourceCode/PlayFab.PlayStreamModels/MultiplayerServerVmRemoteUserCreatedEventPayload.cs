using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerVmRemoteUserCreatedEventPayload
{
	public DateTime? ExpirationTime;

	public string Username;

	public string VmId;
}
