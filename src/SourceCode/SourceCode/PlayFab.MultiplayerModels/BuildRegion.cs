using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class BuildRegion : PlayFabBaseModel
{
	public CurrentServerStats CurrentServerStats;

	public int MaxServers;

	public AzureRegion? Region;

	public int StandbyServers;

	public string Status;
}
