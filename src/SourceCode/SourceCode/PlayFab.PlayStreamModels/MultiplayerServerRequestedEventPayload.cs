using System;
using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerRequestedEventPayload
{
	public AzureRegion? AllocatedRegion;

	public int? AllocatedRegionPreferenceRanking;

	public string BuildId;

	public GenericErrorCodes? ErrorCode;

	public List<AzureRegion> PreferredRegions;

	public string ServerId;

	public string SessionId;
}
