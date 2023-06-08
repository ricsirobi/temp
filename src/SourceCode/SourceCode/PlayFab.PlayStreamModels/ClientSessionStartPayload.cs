using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class ClientSessionStartPayload
{
	public string ClientSessionID;

	public string DeviceModel;

	public string DeviceType;

	public string OS;

	public string UserID;
}
