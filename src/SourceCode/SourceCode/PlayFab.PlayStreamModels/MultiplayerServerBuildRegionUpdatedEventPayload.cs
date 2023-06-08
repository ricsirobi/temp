using System;
using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class MultiplayerServerBuildRegionUpdatedEventPayload
{
	public string BuildId;

	public List<BuildRegion> BuildRegions;
}
