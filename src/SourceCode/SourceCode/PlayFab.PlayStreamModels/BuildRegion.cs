using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class BuildRegion
{
	public int MaxServers;

	public AzureRegion? Region;

	public int StandbyServers;
}
