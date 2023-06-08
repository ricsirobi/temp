using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SanctuaryPetTypeInfo
{
	public string _Name;

	public LocaleString _NameText;

	public LocaleString _DescriptionText;

	public string _Settings = "Default";

	public int _TypeID;

	public RaisedPetGrowthState[] _GrowthStates;

	public float _HatchDuration = 60f;

	public int _InstantHatchTicketItemStoreID = 93;

	public int _InstantHatchTicketItemID = 8227;

	public float _BirthScale = 0.25f;

	public PetAgeData[] _AgeData;

	public LocaleString[] _SleepText;

	public AudioClip[] _SleepMsgVO;

	public string[] _GlowColors;

	public CustomSkinData[] _CustomSkinData;

	public LocaleString _WakeupText;

	public AudioClip _WakeMsgVO;

	public string _EggIconPath = string.Empty;

	public string _DragonEggAssetpath = "RS_DATA/DragonEgg/PfDragonEgg";

	public SFXMap[] _SoundMapper;

	public string _PilotAnimRes = "RS_SHARED/DragonFlyAvatar";

	public string _PetTextureRes = "RS_SHARED/PetDragonTextures";

	public SanctuaryPetAccInfo[] _AccTypeInfo;

	public string _NameSelectPrefabName = "PfUiPetNameSelect";

	public PetSpecialSkillType _SpecialSkill;

	public bool _AllowAccessToMemberOnlyFeatures;

	public float _Weight = 30f;

	public SanctuaryPetStats _Stats;

	public DragonClass _DragonClass;

	private PrimaryTypeInfo mPrimaryType;

	private SecondaryTypeInfo mSecondaryType;

	public float _MountedMinCameraDistance = 2f;

	public int _MinAgeToMount = 2;

	public int _MinAgeToFly = 3;

	public AchievementTitle[] _AchievementTitle;

	public int _AgeUpMissionID;

	public bool _IsUniquePet;

	public List<string> _TicketItemIDList;

	public bool _Flightless;

	public int _Health = 50;

	public PrimaryTypeInfo pPrimaryType
	{
		get
		{
			if (mPrimaryType == null)
			{
				mPrimaryType = SanctuaryData.GetPrimaryType(_TypeID);
			}
			return mPrimaryType;
		}
	}

	public SecondaryTypeInfo pSecondaryType
	{
		get
		{
			if (mSecondaryType == null)
			{
				mSecondaryType = SanctuaryData.GetSecondaryType(_TypeID);
			}
			return mSecondaryType;
		}
	}

	public bool HasAccessory(RaisedPetAccType at)
	{
		SanctuaryPetAccInfo[] accTypeInfo = _AccTypeInfo;
		for (int i = 0; i < accTypeInfo.Length; i++)
		{
			if (accTypeInfo[i]._Type == at)
			{
				return true;
			}
		}
		return false;
	}

	public string FindTextureTypeName(RaisedPetData rp)
	{
		SantuayPetResourceInfo santuayPetResourceInfo = FindResourceInfo(rp);
		if (santuayPetResourceInfo != null)
		{
			return santuayPetResourceInfo._TextureTypeName;
		}
		return "";
	}

	public int FindColorKitCatID(RaisedPetData rp)
	{
		return FindResourceInfo(rp)?._ColorKitCatID ?? 0;
	}

	public int FindAccCatID(RaisedPetData rp, RaisedPetAccType at)
	{
		SanctuaryPetAccInfo[] accTypeInfo = _AccTypeInfo;
		foreach (SanctuaryPetAccInfo sanctuaryPetAccInfo in accTypeInfo)
		{
			if (sanctuaryPetAccInfo._Type == at)
			{
				if (sanctuaryPetAccInfo._CatID <= 0)
				{
					break;
				}
				return sanctuaryPetAccInfo._CatID;
			}
		}
		if (at == RaisedPetAccType.Texture)
		{
			return FindColorKitCatID(rp);
		}
		Debug.LogError("Category ID not found");
		return 0;
	}

	public int FindMaterialIndex(RaisedPetData rp)
	{
		return FindResourceInfo(rp)?._MaterialIndex ?? 0;
	}

	public SantuayPetResourceInfo FindResourceInfo(RaisedPetData inData)
	{
		int ageIndex = RaisedPetData.GetAgeIndex(inData.pStage);
		SantuayPetResourceInfo[] petResList = _AgeData[ageIndex]._PetResList;
		foreach (SantuayPetResourceInfo santuayPetResourceInfo in petResList)
		{
			if (RsResourceManager.Compare(santuayPetResourceInfo._Prefab, inData.Geometry))
			{
				return santuayPetResourceInfo;
			}
		}
		return null;
	}
}
