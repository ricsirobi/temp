using System;
using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerCreateBuildInitiatedEventPayload
{
	public string BuildId;

	public string BuildName;

	public DateTime? CreationTime;

	public Dictionary<string, string> Metadata;
}
