using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class WeaponDatabase : MonoBehaviour
{
	private static WeaponDatabase mInstance;

	[Header("Weapons Database")]
	public List<WeaponData> _WeaponsDataList;

	[Header("Weapon Sprite Names")]
	public List<WeaponSpriteName> _WeaponSpriteDB;

	[Header("Element Sprite Names")]
	public List<ElementSpriteName> _ElementSpriteDB;

	public static WeaponDatabase pInstance => mInstance;

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
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

	public WeaponData GetWeaponData(string weaponName)
	{
		WeaponData weaponData = null;
		weaponData = _WeaponsDataList.Find((WeaponData item) => item._Name == weaponName);
		if (weaponData != null)
		{
			return new WeaponData(weaponData);
		}
		return null;
	}

	public WeaponData GetWeaponData(CharacterData charData)
	{
		string weaponName = GetWeaponName(charData);
		WeaponData weaponData = null;
		if (weaponName != string.Empty)
		{
			weaponData = GetWeaponData(weaponName);
			if (weaponData != null)
			{
				return weaponData;
			}
		}
		weaponData = _WeaponsDataList.Find((WeaponData item) => item._Name == charData._WeaponName);
		if (weaponData == null)
		{
			UtDebug.LogError("Missing Weapon Data for: " + charData._NameID);
		}
		return new WeaponData(weaponData);
	}

	private string GetWeaponName(CharacterData charData)
	{
		if (charData.pIsAvatar())
		{
			AvatarDataPart avatarDataPart = AvatarData.pInstanceInfo.FindPart(AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
			if (avatarDataPart == null)
			{
				avatarDataPart = AvatarData.pInstanceInfo.FindPart("DEFAULT_" + AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
			}
			if (avatarDataPart != null && avatarDataPart.UserInventoryId.HasValue)
			{
				UserItemData userItemData = CommonInventoryData.pInstance.FindItemByUserInventoryID(avatarDataPart.UserInventoryId.Value);
				if (userItemData != null && userItemData.Item != null)
				{
					return userItemData.Item.GetAttribute("WeaponName", string.Empty);
				}
			}
		}
		return string.Empty;
	}

	public string GetElementSprite(ElementType type)
	{
		foreach (ElementSpriteName item in _ElementSpriteDB)
		{
			if (item._ElementType == type)
			{
				return item._SpriteName;
			}
		}
		UtDebug.LogError("Element sprite not found for " + type);
		return "";
	}

	public string GetWeaponTypeSprite(string type)
	{
		foreach (WeaponSpriteName item in _WeaponSpriteDB)
		{
			if (item._WeaponType == type)
			{
				return item._SpriteName;
			}
		}
		UtDebug.LogError("WeaponType sprite not found for " + type);
		return "";
	}
}
