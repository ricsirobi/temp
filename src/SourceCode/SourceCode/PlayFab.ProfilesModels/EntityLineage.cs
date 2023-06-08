using System;
using PlayFab.SharedModels;

namespace PlayFab.ProfilesModels;

[Serializable]
public class EntityLineage : PlayFabBaseModel
{
	public string CharacterId;

	public string GroupId;

	public string MasterPlayerAccountId;

	public string NamespaceId;

	public string TitleId;

	public string TitlePlayerAccountId;
}
