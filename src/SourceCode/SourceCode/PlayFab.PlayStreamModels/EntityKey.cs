using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class EntityKey
{
	public string Id;

	public string Type;

	[Obsolete("Use 'Type' instead", true)]
	public string TypeString;
}
