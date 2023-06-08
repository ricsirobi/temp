using System;
using PlayFab.SharedModels;

namespace PlayFab.CloudScriptModels;

[Serializable]
public class EntityKey : PlayFabBaseModel
{
	public string Id;

	public string Type;
}
