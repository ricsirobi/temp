using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class BuildRegionParams : PlayFabBaseModel
{
	public int MaxServers;

	public AzureRegion Region;

	public int StandbyServers;
}
