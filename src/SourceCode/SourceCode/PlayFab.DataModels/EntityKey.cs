using System;
using PlayFab.SharedModels;

namespace PlayFab.DataModels;

[Serializable]
public class EntityKey : PlayFabBaseModel
{
	public string Id;

	public string Type;
}
