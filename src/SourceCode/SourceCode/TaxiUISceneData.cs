using System;
using UnityEngine;

[Serializable]
public class TaxiUISceneData
{
	public string _LoadLevel;

	public string _StartMarker;

	public LocaleString _DisplayName;

	public bool _IsMemberOnly;

	public int _RequiredPlayerLevel;

	public bool _HideIfNotReachedPlayerLevel;

	public Texture _Icon;

	public bool IsPlayerLevelSatisfied(int inCurrentPlayerLevel)
	{
		return inCurrentPlayerLevel >= _RequiredPlayerLevel;
	}
}
