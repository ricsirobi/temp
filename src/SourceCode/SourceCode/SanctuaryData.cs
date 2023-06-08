using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SanctuaryData : MonoBehaviour
{
	public static SanctuaryData pInstance;

	public static SanctuaryPetTypeInfo[] pPetTypes;

	public PetAgeLevel[] _PetLevels;

	public SanctuaryPetTypeInfo[] _PetTypes;

	public SanctuaryPetTypeSettings[] _Settings;

	public SanctuaryPetStatSettings[] _StatSettings;

	public DragonClassInfo[] _DragonClassInfo;

	public PrimaryTypeInfo[] _PrimaryTypeInfo;

	public SecondaryTypeInfo[] _SecondaryTypeInfo;

	public HeroPetData[] _HeroDragonData;

	public float _MemberEnergyMultiplier = 2f;

	public PetActionAchievement[] _PetActionAchievement;

	public PetAgeDisplayText[] _PetAgeDisplayTextMap;

	public CustomSkinSceneMap[] _CustomSkinSceneMap;

	public PetCustomizationInfo[] _DragonCustomizationInfo;

	public CommonHatchTicket _CommonHatchTicketInfo;

	[SerializeField]
	private List<TitanStatModifier> m_TitanStatsModifierPercentage;

	public void Awake()
	{
		pInstance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		pPetTypes = _PetTypes;
		SanctuaryPetTypeInfo[] array = pPetTypes;
		foreach (SanctuaryPetTypeInfo sanctuaryPetTypeInfo in array)
		{
			if (sanctuaryPetTypeInfo._GrowthStates != null)
			{
				RaisedPetData.SetGrowthState(sanctuaryPetTypeInfo._TypeID, sanctuaryPetTypeInfo._GrowthStates);
			}
		}
	}

	public static SanctuaryPetTypeInfo FindSanctuaryPetTypeInfo(int typeID)
	{
		if (pInstance != null)
		{
			SanctuaryPetTypeInfo[] petTypes = pInstance._PetTypes;
			foreach (SanctuaryPetTypeInfo sanctuaryPetTypeInfo in petTypes)
			{
				if (sanctuaryPetTypeInfo._TypeID == typeID)
				{
					return sanctuaryPetTypeInfo;
				}
			}
		}
		return null;
	}

	public static SanctuaryPetTypeInfo FindSanctuaryPetTypeInfo(string petTypeName)
	{
		SanctuaryPetTypeInfo[] petTypes = pInstance._PetTypes;
		foreach (SanctuaryPetTypeInfo sanctuaryPetTypeInfo in petTypes)
		{
			if (sanctuaryPetTypeInfo._Name == petTypeName)
			{
				return sanctuaryPetTypeInfo;
			}
		}
		return null;
	}

	public static PetCustomizationType GetPetCustomizationType(int petTypeID)
	{
		if (pInstance != null)
		{
			PetCustomizationInfo[] dragonCustomizationInfo = pInstance._DragonCustomizationInfo;
			foreach (PetCustomizationInfo petCustomizationInfo in dragonCustomizationInfo)
			{
				if (petCustomizationInfo._TypeID == petTypeID)
				{
					return petCustomizationInfo._PetCustomizationType;
				}
			}
		}
		return PetCustomizationType.Default;
	}

	public static PetCustomizationType GetPetCustomizationType(RaisedPetData raisedPetData)
	{
		if (raisedPetData != null)
		{
			return GetPetCustomizationType(raisedPetData.PetTypeID);
		}
		return PetCustomizationType.Default;
	}

	public static string GetLocalizedPetName(RaisedPetData raisedPetData)
	{
		string petDefaultName = raisedPetData.Name;
		PetCustomizationType petCustomizationType = GetPetCustomizationType(raisedPetData.PetTypeID);
		if (petCustomizationType == PetCustomizationType.None || petCustomizationType == PetCustomizationType.ColorOnly)
		{
			petDefaultName = GetPetDefaultName(raisedPetData.PetTypeID);
		}
		return petDefaultName;
	}

	public static string GetPetDefaultName(int petTypeID)
	{
		if (pInstance != null)
		{
			PetCustomizationInfo[] dragonCustomizationInfo = pInstance._DragonCustomizationInfo;
			foreach (PetCustomizationInfo petCustomizationInfo in dragonCustomizationInfo)
			{
				if (petCustomizationInfo._TypeID == petTypeID)
				{
					return petCustomizationInfo._NameText.GetLocalizedString();
				}
			}
		}
		return string.Empty;
	}

	public static string GetPetDefaultName(RaisedPetData raisedPetData)
	{
		if (raisedPetData != null)
		{
			return GetPetDefaultName(raisedPetData.PetTypeID);
		}
		return string.Empty;
	}

	public static Color[] GetPetDefaultColors(int petTypeID)
	{
		if (pInstance != null)
		{
			PetCustomizationInfo[] dragonCustomizationInfo = pInstance._DragonCustomizationInfo;
			foreach (PetCustomizationInfo petCustomizationInfo in dragonCustomizationInfo)
			{
				if (petCustomizationInfo._TypeID == petTypeID)
				{
					return petCustomizationInfo._Colors;
				}
			}
		}
		return null;
	}

	public static Color[] GetPetDefaultColors(RaisedPetData raisedPetData)
	{
		if (raisedPetData != null)
		{
			return GetPetDefaultColors(raisedPetData.PetTypeID);
		}
		return null;
	}

	public static bool IsNameChangeAllowed(RaisedPetData raisedPetData)
	{
		PetCustomizationType petCustomizationType = GetPetCustomizationType(raisedPetData);
		if (petCustomizationType != 0)
		{
			return petCustomizationType == PetCustomizationType.NameOnly;
		}
		return true;
	}

	public static bool IsColorChangeAllowed(RaisedPetData raisedPetData)
	{
		PetCustomizationType petCustomizationType = GetPetCustomizationType(raisedPetData);
		if (petCustomizationType != 0)
		{
			return petCustomizationType == PetCustomizationType.ColorOnly;
		}
		return true;
	}

	public static DragonClassInfo GetDragonClassInfo(DragonClass inDragonClass)
	{
		DragonClassInfo result = null;
		DragonClassInfo[] dragonClassInfo = pInstance._DragonClassInfo;
		foreach (DragonClassInfo dragonClassInfo2 in dragonClassInfo)
		{
			if (dragonClassInfo2._Class == inDragonClass)
			{
				result = dragonClassInfo2;
				break;
			}
		}
		return result;
	}

	public static PrimaryTypeInfo GetPrimaryType(int typeID)
	{
		PrimaryTypeInfo[] primaryTypeInfo = pInstance._PrimaryTypeInfo;
		foreach (PrimaryTypeInfo primaryTypeInfo2 in primaryTypeInfo)
		{
			if (primaryTypeInfo2._PetTypeIDs == null || primaryTypeInfo2._PetTypeIDs.Length == 0)
			{
				continue;
			}
			for (int j = 0; j < primaryTypeInfo2._PetTypeIDs.Length; j++)
			{
				if (primaryTypeInfo2._PetTypeIDs[j] == typeID)
				{
					return primaryTypeInfo2;
				}
			}
		}
		return null;
	}

	public static SecondaryTypeInfo GetSecondaryType(int typeID)
	{
		SecondaryTypeInfo[] secondaryTypeInfo = pInstance._SecondaryTypeInfo;
		foreach (SecondaryTypeInfo secondaryTypeInfo2 in secondaryTypeInfo)
		{
			if (secondaryTypeInfo2._PetTypeIDs == null || secondaryTypeInfo2._PetTypeIDs.Length == 0)
			{
				continue;
			}
			for (int j = 0; j < secondaryTypeInfo2._PetTypeIDs.Length; j++)
			{
				if (secondaryTypeInfo2._PetTypeIDs[j] == typeID)
				{
					return secondaryTypeInfo2;
				}
			}
		}
		return null;
	}

	public static float GetMaxMeter(SanctuaryPetMeterType inType, RaisedPetData inRaisedPetData)
	{
		int inRankID = PetRankData.GetUserRank(inRaisedPetData)?.RankID ?? 1;
		float maxMeterByRank = GetMaxMeterByRank(inType, inRankID);
		return GetMaxMeterByPetData(inType, maxMeterByRank, inRaisedPetData);
	}

	private static float GetMaxMeterByRank(SanctuaryPetMeterType inType, int inRankID)
	{
		switch (inType)
		{
		case SanctuaryPetMeterType.ENERGY:
		{
			float num = UserRankData.GetAttribute(8, inRankID, "ENERGY", 1f, orLower: true);
			if (SubscriptionInfo.pIsMember)
			{
				num *= pInstance._MemberEnergyMultiplier;
			}
			return num;
		}
		case SanctuaryPetMeterType.HAPPINESS:
			return UserRankData.GetAttribute(8, inRankID, "HAPPINESS", 1f, orLower: true);
		case SanctuaryPetMeterType.RACING_ENERGY:
			return UserRankData.GetAttribute(8, inRankID, "RACING_ENERGY", 1f, orLower: true);
		case SanctuaryPetMeterType.RACING_FIRE:
			return UserRankData.GetAttribute(8, inRankID, "RACING_FIRE", 1f, orLower: true);
		default:
			return 0f;
		}
	}

	private static float GetMaxMeterByPetData(SanctuaryPetMeterType inType, float meterValue, RaisedPetData inRaisedPetData)
	{
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = FindSanctuaryPetTypeInfo(inRaisedPetData.PetTypeID);
		switch (inType)
		{
		case SanctuaryPetMeterType.HEALTH:
		{
			float num2 = meterValue;
			if (sanctuaryPetTypeInfo != null)
			{
				num2 += (float)sanctuaryPetTypeInfo._Health;
				SanctuaryPetMeterModifier healthModifier = GetSanctuaryPetSettings(sanctuaryPetTypeInfo._Settings)._HealthModifier;
				if (healthModifier != null)
				{
					num2 = healthModifier.Modify(num2, inRaisedPetData);
				}
				num2 = ModifyMeterWithAccessories(inRaisedPetData, num2, "HealthMultiplier");
			}
			return num2;
		}
		case SanctuaryPetMeterType.ENERGY:
		{
			float num = meterValue;
			if (sanctuaryPetTypeInfo != null)
			{
				SanctuaryPetMeterModifier energyModifier = GetSanctuaryPetSettings(sanctuaryPetTypeInfo._Settings)._EnergyModifier;
				if (energyModifier != null)
				{
					num = energyModifier.Modify(num, inRaisedPetData);
				}
				num = ModifyMeterWithAccessories(inRaisedPetData, num, "EnergyMultiplier");
			}
			return num;
		}
		default:
			return meterValue;
		}
	}

	private static float ModifyMeterWithAccessories(RaisedPetData inPetData, float inValue, string attributeName)
	{
		float num = inValue;
		if (inPetData != null && inPetData.Accessories != null)
		{
			for (int i = 0; i < inPetData.Accessories.Length; i++)
			{
				int accessoryItemID = inPetData.GetAccessoryItemID(inPetData.Accessories[i]);
				if (CommonInventoryData.pIsReady)
				{
					UserItemData userItemData = CommonInventoryData.pInstance.FindItem(accessoryItemID);
					if (userItemData != null)
					{
						num += inValue * (float)userItemData.Item.GetAttribute(attributeName, 0);
					}
				}
			}
		}
		return num;
	}

	public static void Reset()
	{
		if (pInstance != null)
		{
			UnityEngine.Object.Destroy(pInstance.gameObject);
			pInstance = null;
		}
	}

	public static SanctuaryPetTypeSettings GetSanctuaryPetSettings(string inSettingsName)
	{
		SanctuaryPetTypeSettings[] settings = pInstance._Settings;
		foreach (SanctuaryPetTypeSettings sanctuaryPetTypeSettings in settings)
		{
			if (sanctuaryPetTypeSettings._Name == inSettingsName)
			{
				return sanctuaryPetTypeSettings;
			}
		}
		return null;
	}

	public static HeroPetData GetHeroDragonFromID(int inItemID)
	{
		HeroPetData[] heroDragonData = pInstance._HeroDragonData;
		foreach (HeroPetData heroPetData in heroDragonData)
		{
			if (heroPetData._ItemID == inItemID)
			{
				return heroPetData;
			}
		}
		return null;
	}

	public static HeroPetData GetHeroDragonFromName(string inName)
	{
		if (string.IsNullOrEmpty(inName))
		{
			return null;
		}
		HeroPetData[] heroDragonData = pInstance._HeroDragonData;
		foreach (HeroPetData heroPetData in heroDragonData)
		{
			if (heroPetData._Name == inName)
			{
				return heroPetData;
			}
		}
		return null;
	}

	public static HeroPetData GetHeroDragonFromTypeID(int inTypeID)
	{
		HeroPetData[] heroDragonData = pInstance._HeroDragonData;
		foreach (HeroPetData heroPetData in heroDragonData)
		{
			if (heroPetData._TypeID == inTypeID)
			{
				return heroPetData;
			}
		}
		return null;
	}

	public static List<string> GetUniquePetTicketItemsList(int petType)
	{
		SanctuaryPetTypeInfo[] array = pPetTypes;
		foreach (SanctuaryPetTypeInfo sanctuaryPetTypeInfo in array)
		{
			if (sanctuaryPetTypeInfo._TypeID == petType && sanctuaryPetTypeInfo._IsUniquePet)
			{
				return sanctuaryPetTypeInfo._TicketItemIDList;
			}
		}
		return null;
	}

	public static string GetDisplayTextFromPetAge(RaisedPetStage stage)
	{
		if (pInstance._PetAgeDisplayTextMap == null || pInstance._PetAgeDisplayTextMap.Length == 0)
		{
			return null;
		}
		for (int i = 0; i < pInstance._PetAgeDisplayTextMap.Length; i++)
		{
			if (pInstance._PetAgeDisplayTextMap[i]._Age == stage)
			{
				return pInstance._PetAgeDisplayTextMap[i]._DisplayText.GetLocalizedString();
			}
		}
		return null;
	}

	public static string GetDisplayTextFromPetStat(PetStatType statType)
	{
		SanctuaryPetStatSettings[] statSettings = pInstance._StatSettings;
		foreach (SanctuaryPetStatSettings sanctuaryPetStatSettings in statSettings)
		{
			if (sanctuaryPetStatSettings._StatType == statType)
			{
				return sanctuaryPetStatSettings._StatText.GetLocalizedString();
			}
		}
		return null;
	}

	public void UpdateActionMeterData(RaisedPetData inRaisedPetData, PetActions petAction, SanctuaryPetMeterType petMeterType, float percent)
	{
		SanctuaryPetTypeSettings sanctuaryPetSettings = GetSanctuaryPetSettings(FindSanctuaryPetTypeInfo(inRaisedPetData.PetTypeID)._Settings);
		float delta = sanctuaryPetSettings._ActionMeterData.ToList().Find((PetMeterActionData i) => i._ID == petAction && i._MeterType == petMeterType)._Delta;
		RaisedPetState raisedPetState = inRaisedPetData.States.ToList().Find((RaisedPetState i) => i.Key.Equals(petMeterType.ToString()));
		if (raisedPetState == null)
		{
			return;
		}
		float num = raisedPetState.Value + delta * percent;
		if (num >= 0f)
		{
			if (UserRankData.pIsReady)
			{
				raisedPetState.Value = Mathf.Clamp(num, sanctuaryPetSettings._MinPetMeterValue, GetMaxMeter(petMeterType, inRaisedPetData));
			}
			else
			{
				raisedPetState.Value = num;
			}
			inRaisedPetData.SaveDataReal();
		}
	}

	public string GetSceneCustomSkin()
	{
		if (_CustomSkinSceneMap == null && _CustomSkinSceneMap.Length == 0)
		{
			return "";
		}
		CustomSkinSceneMap customSkinSceneMap = Array.Find(_CustomSkinSceneMap, (CustomSkinSceneMap data) => data._SceneName.Equals(RsResourceManager.pCurrentLevel));
		if (customSkinSceneMap == null)
		{
			return "";
		}
		return customSkinSceneMap._SkinName;
	}

	public float GetFinalStatsValue(RaisedPetStage stage, PetStatType type, float value)
	{
		if (stage == RaisedPetStage.TITAN)
		{
			TitanStatModifier titanStatModifier = m_TitanStatsModifierPercentage.Find((TitanStatModifier a) => a._StatType == type);
			if (titanStatModifier != null)
			{
				value += value * (titanStatModifier._Value / 100f);
			}
		}
		return value;
	}

	public int GetLevelFromPetStage(RaisedPetStage pStage)
	{
		int result = -1;
		for (int i = 0; i < _PetLevels.Length; i++)
		{
			if (_PetLevels[i]._Age == pStage)
			{
				result = _PetLevels[i]._Level;
				break;
			}
		}
		return result;
	}
}
