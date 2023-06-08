using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SquadTactics;

public class CharacterDatabase : MonoBehaviour
{
	private static CharacterDatabase mInstance;

	private bool mIsInitCharacterDB;

	[Header("Character Database")]
	public List<CharacterData> _Characters;

	public static CharacterDatabase pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfSTCharacterDatabase")).GetComponent<CharacterDatabase>();
			}
			return mInstance;
		}
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Init()
	{
		if (mIsInitCharacterDB)
		{
			return;
		}
		mIsInitCharacterDB = true;
		foreach (CharacterData character in _Characters)
		{
			character._WeaponData = WeaponDatabase.pInstance.GetWeaponData(character);
		}
	}

	public void ReInitAvatarWeapon()
	{
		CharacterData avatar = GetAvatar();
		if (avatar != null)
		{
			avatar._WeaponData = WeaponDatabase.pInstance.GetWeaponData(avatar);
		}
	}

	public WeaponData GetAvatarWeaponData()
	{
		return new WeaponData(GetAvatar()._WeaponData);
	}

	private void OnDestroy()
	{
		if (mInstance == this)
		{
			mInstance = null;
		}
	}

	public void DeleteInstance()
	{
		if (mInstance != null)
		{
			Object.Destroy(mInstance.gameObject);
		}
	}

	public CharacterData GetAvatar()
	{
		return _Characters.Find((CharacterData item) => item._NameID == "Avatar");
	}

	public CharacterData GetCharacter(string charName, int raisedPetID = 0, string variantName = "")
	{
		return GetCharacter(charName, RaisedPetData.GetByID(raisedPetID), variantName);
	}

	public CharacterData GetCharacter(string charName, RaisedPetData data, string variantName = "")
	{
		if (data != null && data.pStage == RaisedPetStage.TITAN)
		{
			charName += "Titan";
		}
		CharacterData characterData = _Characters.Find((CharacterData x) => x._NameID == charName);
		if (characterData == null)
		{
			return null;
		}
		if (!string.IsNullOrEmpty(variantName) && characterData._Variants.Length != 0)
		{
			CharacterVariant characterVariant = characterData._Variants.FirstOrDefault((CharacterVariant x) => x._NameID == variantName);
			if (characterVariant == null)
			{
				return characterData;
			}
			characterData._PrefabName = characterVariant._PrefabName;
			characterData._PortraitIcon = characterVariant._PortraitIcon;
			return characterData;
		}
		return characterData;
	}

	public int GetLevel(RaisedPetData raisedPetData)
	{
		if (raisedPetData != null)
		{
			UserRank userRank = PetRankData.GetUserRank(raisedPetData);
			if (userRank != null)
			{
				return userRank.RankID;
			}
		}
		return 1;
	}
}
