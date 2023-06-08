using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class ClientFocusChangePayload
{
	public string ClientSessionID;

	public DateTime? EventTimestamp;

	public string FocusID;

	public bool FocusState;

	public double FocusStateDuration;
}
