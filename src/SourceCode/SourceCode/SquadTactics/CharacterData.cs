using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SquadTactics;

[Serializable]
public class CharacterData
{
	public string _NameID;

	public bool _Available;

	public CharacterVariant[] _Variants;

	[FormerlySerializedAs("_Name")]
	public LocaleString _DisplayNameText = new LocaleString("Character name");

	public LocaleString _SpeciesText;

	[HideInInspector]
	public Character.Team _Team;

	public Stats _Stats;

	public string _PrefabName;

	public string _PortraitIcon;

	public string _WeaponName;

	public DefaultPropData[] _DefaultProps;

	[HideInInspector]
	public WeaponData _WeaponData;

	[HideInInspector]
	public Weapon _Weapon;

	public int pRaisedPetID { get; set; }

	public int pLevel { get; set; }

	public int pSpawnOrder { get; set; }

	public bool pWeaponOverridden { get; set; }

	public bool pIsAvatar()
	{
		return _NameID == "Avatar";
	}

	public CharacterData(CharacterData charData)
	{
		if (charData != null)
		{
			_NameID = charData._NameID;
			_DisplayNameText = charData._DisplayNameText;
			_SpeciesText = charData._SpeciesText;
			_Team = charData._Team;
			_Stats = new Stats(charData._Stats);
			_PortraitIcon = charData._PortraitIcon;
			_PrefabName = charData._PrefabName;
			_WeaponName = charData._WeaponName;
			_DefaultProps = charData._DefaultProps;
			_WeaponData = new WeaponData(charData._WeaponData);
			pRaisedPetID = charData.pRaisedPetID;
			pWeaponOverridden = charData.pWeaponOverridden;
		}
	}
}
