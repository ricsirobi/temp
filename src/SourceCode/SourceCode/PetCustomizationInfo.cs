using System;
using UnityEngine;

[Serializable]
public class PetCustomizationInfo
{
	public int _TypeID;

	public LocaleString _NameText;

	public Color[] _Colors;

	public PetCustomizationType _PetCustomizationType;
}
