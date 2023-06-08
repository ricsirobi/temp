using System;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class QosServer : PlayFabBaseModel
{
	public AzureRegion? Region;

	public string ServerUrl;
}
