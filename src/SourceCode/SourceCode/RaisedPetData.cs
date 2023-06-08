using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "RPD", Namespace = "")]
public class RaisedPetData
{
	[XmlElement(ElementName = "id")]
	public int RaisedPetID;

	[XmlElement(ElementName = "eid", IsNullable = true)]
	public Guid? EntityID;

	[XmlElement(ElementName = "uid")]
	public Guid UserID;

	[XmlElement(ElementName = "n")]
	public string Name;

	[XmlElement(ElementName = "ptid")]
	public int PetTypeID;

	[XmlElement(ElementName = "gs")]
	public RaisedPetGrowthState GrowthState;

	[XmlElement(ElementName = "ip", IsNullable = true)]
	public int? ImagePosition;

	[XmlElement(ElementName = "g")]
	public string Geometry;

	[XmlElement(ElementName = "t")]
	public string Texture;

	[XmlElement(ElementName = "gd")]
	public Gender Gender;

	[XmlElement(ElementName = "ac")]
	public RaisedPetAccessory[] Accessories;

	[XmlElement(ElementName = "at")]
	public RaisedPetAttribute[] Attributes;

	[XmlElement(ElementName = "c")]
	public RaisedPetColor[] Colors;

	[XmlElement(ElementName = "sk")]
	public RaisedPetSkill[] Skills;

	[XmlElement(ElementName = "st")]
	public RaisedPetState[] States;

	[XmlElement(ElementName = "is")]
	public bool IsSelected;

	[XmlElement(ElementName = "ir")]
	public bool IsReleased;

	[XmlElement(ElementName = "cdt")]
	public DateTime CreateDate;

	[XmlElement(ElementName = "updt")]
	public DateTime UpdateDate;

	public const int TerribleTerror = 12;

	public const int Gronkle = 13;

	public const int Nadder = 14;

	public const int Nightmare = 15;

	public const int Zippleback = 16;

	public const int NightFury = 17;

	public const int TimberJack = 18;

	public const int ThunderDrum = 19;

	public const int WhisperingDeath = 20;

	public const string EQUPPED_SKILL = "ES";

	public static KeyValuePair<int, RaisedPetSkill> INVALID_ACTIVE_SKILL = new KeyValuePair<int, RaisedPetSkill>(-1, null);

	[XmlIgnore]
	public RaisedPetStage pStage;

	[XmlIgnore]
	public Texture2D pTexture;

	[XmlIgnore]
	public GrBitmap pTextureBMP;

	[XmlIgnore]
	public DateTime pHatchEndTime;

	[XmlIgnore]
	public bool pIsSleeping;

	[XmlIgnore]
	public bool pTextureUpdated;

	[XmlIgnore]
	public bool pNoSave;

	[XmlIgnore]
	public bool pIsActive;

	[XmlIgnore]
	public GameObject pObject;

	[XmlIgnore]
	public bool mIsDirty;

	[XmlIgnore]
	public bool pIsNameCustomized;

	[XmlIgnore]
	public int pPriority;

	[XmlIgnore]
	public Dictionary<int, DateTime> pFoodEffect = new Dictionary<int, DateTime>();

	[XmlIgnore]
	public int pRank = 1;

	[XmlIgnore]
	public int pIncubatorID = -1;

	[XmlIgnore]
	public bool pAbortCreation;

	[XmlIgnore]
	private GlowEffect mGlowEffect;

	public static Dictionary<int, RaisedPetData[]> pReleasedPets = new Dictionary<int, RaisedPetData[]>();

	public static Dictionary<int, RaisedPetData[]> pActivePets = new Dictionary<int, RaisedPetData[]>();

	public static Dictionary<int, RaisedPetGrowthState[]> pGrowStates = new Dictionary<int, RaisedPetGrowthState[]>();

	public static RaisedPetSelectEventHandler mSelectCallback = null;

	public static RaisedPetReleaseEventHandler mReleaseCallback = null;

	private RaisedPetSaveEventHandler mRaisedPetSaveEventHandler;

	private float mHatchDuration = 720f;

	private List<string> mChangedAttributeNames = new List<string>();

	[XmlElement(ElementName = "ispetcreated")]
	public bool IsPetCreated { get; set; }

	[XmlElement(ElementName = "validationmessage")]
	public string ValidationMessage { get; set; }

	public GlowEffect pGlowEffect
	{
		get
		{
			if (mGlowEffect == null)
			{
				RaisedPetAttribute raisedPetAttribute = FindAttrData("GlowEffect");
				if (raisedPetAttribute != null)
				{
					mGlowEffect = UtUtilities.DeserializeFromXml<GlowEffect>(raisedPetAttribute.Value);
				}
			}
			return mGlowEffect;
		}
	}

	[XmlIgnore]
	public float pHatchDuration
	{
		get
		{
			return mHatchDuration;
		}
		set
		{
			mHatchDuration = value;
		}
	}

	public RaisedPetData()
	{
	}

	public RaisedPetData(RaisedPetData inData)
	{
		RaisedPetID = inData.RaisedPetID;
		EntityID = inData.EntityID;
		UserID = inData.UserID;
		Name = inData.Name;
		PetTypeID = inData.PetTypeID;
		ImagePosition = inData.ImagePosition;
		Geometry = inData.Geometry;
		Texture = inData.Texture;
		Gender = inData.Gender;
		IsSelected = inData.IsSelected;
		IsReleased = inData.IsReleased;
		CreateDate = inData.CreateDate;
		UpdateDate = inData.CreateDate;
		pStage = inData.pStage;
		GrowthState = inData.GrowthState;
		Accessories = inData.Accessories;
		Attributes = inData.Attributes;
		Colors = inData.Colors;
		Skills = inData.Skills;
		States = inData.States;
	}

	public static void Reset()
	{
		pReleasedPets.Clear();
		pActivePets.Clear();
	}

	public static void SetSelectedPet(int petID, bool unselectOtherPets, RaisedPetSelectEventHandler callback, object inUserData)
	{
		mSelectCallback = callback;
		WsWebService.SetSelectedPet(petID, unselectOtherPets, ServiceEventHandler, inUserData);
	}

	public static RaisedPetData GetCurrentInstance(int ptype)
	{
		if (!pActivePets.ContainsKey(ptype))
		{
			return null;
		}
		RaisedPetData[] array = pActivePets[ptype];
		if (array == null)
		{
			return null;
		}
		if (array.Length == 0)
		{
			return null;
		}
		RaisedPetData raisedPetData = array[0];
		int num = raisedPetData.pPriority;
		RaisedPetData[] array2 = array;
		foreach (RaisedPetData raisedPetData2 in array2)
		{
			if (raisedPetData2.pPriority >= num && raisedPetData2.IsSelected)
			{
				raisedPetData = raisedPetData2;
				num = raisedPetData2.pPriority;
			}
		}
		return raisedPetData;
	}

	public static RaisedPetData GetByID(int raisedPetID)
	{
		if (raisedPetID <= 0)
		{
			return null;
		}
		foreach (RaisedPetData[] value in pActivePets.Values)
		{
			if (value == null || value.Length == 0)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				if (raisedPetData.RaisedPetID == raisedPetID)
				{
					return raisedPetData;
				}
			}
		}
		return null;
	}

	public static RaisedPetData GetByEntityID(Guid? entityID)
	{
		if (!entityID.HasValue)
		{
			return null;
		}
		foreach (RaisedPetData[] value2 in pActivePets.Values)
		{
			if (value2 == null || value2.Length == 0)
			{
				continue;
			}
			RaisedPetData[] array = value2;
			foreach (RaisedPetData raisedPetData in array)
			{
				if (raisedPetData.EntityID.HasValue)
				{
					Guid value = raisedPetData.EntityID.Value;
					Guid? guid = entityID;
					if (value == guid)
					{
						return raisedPetData;
					}
				}
			}
		}
		return null;
	}

	public UserRank GetPetUserRank()
	{
		return PetRankData.GetUserRank(EntityID);
	}

	public static void DumpActivePets(int ptype)
	{
		if (!pActivePets.ContainsKey(ptype))
		{
			return;
		}
		if (pActivePets[ptype] == null)
		{
			Debug.LogError("Active pet not ready yet");
			return;
		}
		UtDebug.Log("dump active pets starts , ptype = " + ptype);
		RaisedPetData[] array = pActivePets[ptype];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].DumpData();
		}
		UtDebug.Log("dump end");
	}

	public static void ResetActivePets(int ptype)
	{
		if (pActivePets[ptype] != null)
		{
			pActivePets[ptype] = null;
			UtDebug.Log("Active pet reset");
		}
	}

	public static void UpdateActivePet(int petTypeID, RaisedPetData data)
	{
		RaisedPetData[] value = new RaisedPetData[1] { data };
		pActivePets[petTypeID] = value;
	}

	public static void GetActivePets(int ptype, RaisedPetGetEventHandler callback, object inUserData)
	{
		if (!pActivePets.ContainsKey(ptype))
		{
			Debug.LogError("Calling GetActivePets again before the first call returns");
			return;
		}
		if (pActivePets[ptype] != null)
		{
			callback?.Invoke(ptype, pActivePets[ptype], inUserData);
			return;
		}
		RaisedPetGetData raisedPetGetData = new RaisedPetGetData();
		raisedPetGetData.mIsMMOUser = false;
		raisedPetGetData.mCallback = callback;
		raisedPetGetData.mUserData = inUserData;
		raisedPetGetData.mType = ptype;
		WsWebService.GetActivePet(UserInfo.pInstance.UserID, ptype, ServiceEventHandler, raisedPetGetData);
	}

	public static void GetUnselectedPetsByTypes(int[] ptypes, RaisedPetGetEventHandler callback, object inUserData)
	{
		if (ptypes != null)
		{
			RaisedPetGetData raisedPetGetData = new RaisedPetGetData();
			raisedPetGetData.mIsMMOUser = false;
			raisedPetGetData.mUserData = inUserData;
			raisedPetGetData.mFlushCache = false;
			raisedPetGetData.mCallback = callback;
			WsWebService.GetUnselectedPetsByTypes(UserInfo.pInstance.UserID, ptypes, ServiceEventHandler, raisedPetGetData);
		}
	}

	public static void GetSelectedRaisedPet(bool selected, RaisedPetGetEventHandler callback, object inUserData)
	{
		RaisedPetGetData raisedPetGetData = new RaisedPetGetData();
		raisedPetGetData.mIsMMOUser = false;
		raisedPetGetData.mCallback = callback;
		raisedPetGetData.mUserData = inUserData;
		WsWebService.GetSelectedRaisedPet(UserInfo.pInstance.UserID, selected, ServiceEventHandler, raisedPetGetData);
	}

	public static void GetAllActivePets(bool active, RaisedPetGetEventHandler callback, object inUserData)
	{
		RaisedPetGetData raisedPetGetData = new RaisedPetGetData();
		raisedPetGetData.mIsMMOUser = false;
		raisedPetGetData.mCallback = callback;
		raisedPetGetData.mUserData = inUserData;
		WsWebService.GetAllActivePetsByuserId(UserInfo.pInstance.UserID, active, ServiceEventHandler, raisedPetGetData);
	}

	public static void GetReleasedPets(int ptype, RaisedPetGetEventHandler callback, object inUserData)
	{
		if (!pReleasedPets.ContainsKey(ptype))
		{
			Debug.LogError("Calling GetReleasedPets again before the first call returns");
			return;
		}
		if (pReleasedPets[ptype] != null)
		{
			callback?.Invoke(ptype, pReleasedPets[ptype], inUserData);
			return;
		}
		RaisedPetGetData raisedPetGetData = new RaisedPetGetData();
		raisedPetGetData.mIsMMOUser = false;
		raisedPetGetData.mCallback = callback;
		raisedPetGetData.mUserData = inUserData;
		raisedPetGetData.mType = ptype;
		WsWebService.GetReleasedPet(UserInfo.pInstance.UserID, ptype, ServiceEventHandler, raisedPetGetData);
	}

	public void Invalidate(bool save)
	{
		pPriority = -999;
		if (save)
		{
			SaveData();
		}
	}

	public bool IsValid()
	{
		return pPriority != -999;
	}

	public static void GetUserPetData(int ptype, string userID, bool isActive, RaisedPetGetEventHandler callback, object inUserData)
	{
		RaisedPetGetData raisedPetGetData = new RaisedPetGetData();
		raisedPetGetData.mIsMMOUser = true;
		raisedPetGetData.mCallback = callback;
		raisedPetGetData.mUserData = inUserData;
		raisedPetGetData.mType = ptype;
		if (isActive)
		{
			WsWebService.GetActivePet(userID, ptype, ServiceEventHandler, raisedPetGetData);
		}
		else
		{
			WsWebService.GetReleasedPet(userID, ptype, ServiceEventHandler, raisedPetGetData);
		}
	}

	public static void CreateNewPet(int ptype, bool setAsSelectedPet, bool unselectOtherPets, RaisedPetCreateEventHandler callback, object inUserData)
	{
		UtDebug.Log("New Pet Creating type = " + ptype);
		RaisedPetCreateData raisedPetCreateData = new RaisedPetCreateData();
		raisedPetCreateData.mCallback = callback;
		raisedPetCreateData.mUserData = inUserData;
		raisedPetCreateData.mType = ptype;
		WsWebService.CreateRaisedPet(ptype, setAsSelectedPet, unselectOtherPets, ServiceEventHandler, raisedPetCreateData);
	}

	public static void CreateNewPet(int ptype, bool setAsSelectedPet, bool unselectOtherPets, RaisedPetData inData, CommonInventoryRequest[] inInventoryRequest, RaisedPetCreateEventHandler callback, object inUserData)
	{
		UtDebug.Log("New Pet Creating type = " + ptype);
		RaisedPetCreateData raisedPetCreateData = new RaisedPetCreateData();
		raisedPetCreateData.mCallback = callback;
		raisedPetCreateData.mUserData = inUserData;
		raisedPetCreateData.mType = ptype;
		WsWebService.CreatePet(ptype, setAsSelectedPet, unselectOtherPets, inData, inInventoryRequest, ServiceEventHandler, raisedPetCreateData);
	}

	public static RaisedPetData InitDefault(int ptype, RaisedPetStage stage, string resName, Gender gender, bool addToActivePets = true)
	{
		RaisedPetData raisedPetData = CreateCustomizedPetData(ptype, stage, resName, gender, null, noColorMap: true);
		if (addToActivePets)
		{
			pActivePets[ptype] = new RaisedPetData[1];
			pActivePets[ptype][0] = raisedPetData;
		}
		return raisedPetData;
	}

	public static int GetAgeIndex(RaisedPetStage rs)
	{
		int result = 0;
		switch (rs)
		{
		case RaisedPetStage.BABY:
			result = 0;
			break;
		case RaisedPetStage.CHILD:
			result = 1;
			break;
		case RaisedPetStage.TEEN:
			result = 2;
			break;
		case RaisedPetStage.ADULT:
			result = 3;
			break;
		case RaisedPetStage.TITAN:
			result = 4;
			break;
		}
		return result;
	}

	public static RaisedPetStage GetGrowthStage(int age)
	{
		return age switch
		{
			0 => RaisedPetStage.BABY, 
			1 => RaisedPetStage.CHILD, 
			2 => RaisedPetStage.TEEN, 
			3 => RaisedPetStage.ADULT, 
			4 => RaisedPetStage.TITAN, 
			_ => RaisedPetStage.NONE, 
		};
	}

	public static RaisedPetStage GetNextGrowthStage(RaisedPetStage rs)
	{
		RaisedPetStage result = rs;
		switch (rs)
		{
		case RaisedPetStage.BABY:
			result = RaisedPetStage.TEEN;
			break;
		case RaisedPetStage.CHILD:
			result = RaisedPetStage.TEEN;
			break;
		case RaisedPetStage.TEEN:
			result = RaisedPetStage.ADULT;
			break;
		case RaisedPetStage.ADULT:
			result = RaisedPetStage.TITAN;
			break;
		}
		return result;
	}

	public static bool IsActivePetDataReady(int ptype)
	{
		if (pActivePets.ContainsKey(ptype))
		{
			return pActivePets[ptype] != null;
		}
		return false;
	}

	public static bool IsReleasedPetDataReady(int ptype)
	{
		if (pReleasedPets.ContainsKey(ptype))
		{
			return pReleasedPets[ptype] != null;
		}
		return false;
	}

	public bool IsReleasePetDataOK()
	{
		if (PetTypeID >= 12 && PetTypeID <= 17)
		{
			if (Geometry == null || Geometry.Length == 0)
			{
				return false;
			}
			if (pStage != RaisedPetStage.ADULT && pStage != RaisedPetStage.TEEN)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void ReleasePet(RaisedPetReleaseEventHandler callback)
	{
		if (mReleaseCallback != null)
		{
			UtDebug.LogError("Calling Releasing Pet twice");
			return;
		}
		mReleaseCallback = callback;
		WsWebService.SetRaisedPetInactive(RaisedPetID, ServiceEventHandler, this);
	}

	public void SetHatchEndTime(DateTime hatchTime)
	{
		pHatchEndTime = hatchTime;
	}

	public void StartHatching(float hatchDuration, int incubatorID = -1)
	{
		pIncubatorID = incubatorID;
		SetHatchEndTime(ServerTime.pCurrentTime.AddMinutes(hatchDuration));
		SetState(RaisedPetStage.HATCHING, savedata: false);
		UtDebug.Log("Egg hatching started :" + PetTypeID);
		SaveDataReal();
	}

	public static RaisedPetData GetHatchingPet(int incubatorID)
	{
		if (pActivePets != null)
		{
			foreach (RaisedPetData[] value in pActivePets.Values)
			{
				if (value == null)
				{
					continue;
				}
				RaisedPetData[] array = value;
				foreach (RaisedPetData raisedPetData in array)
				{
					if (raisedPetData != null && raisedPetData.pIncubatorID == incubatorID)
					{
						return raisedPetData;
					}
				}
			}
		}
		return null;
	}

	public void HatchPet(string resName, Color[] colorMap, Gender gender)
	{
		UtDebug.Log("Pet Hatched");
		Name = "";
		Geometry = resName;
		Texture = "";
		SetHatchEndTime(ServerTime.pCurrentTime);
		SetState(RaisedPetStage.BABY, savedata: false);
		SetColors(colorMap);
		Gender = gender;
		States = null;
		Skills = null;
		pTexture = null;
		pTextureBMP = null;
	}

	public static RaisedPetAccType GetAccessoryType(int inCategory)
	{
		return inCategory switch
		{
			380 => RaisedPetAccType.Saddle, 
			424 => RaisedPetAccType.Materials, 
			_ => RaisedPetAccType.None, 
		};
	}

	public static RaisedPetAccType GetAccessoryType(ItemData inItemData)
	{
		if (inItemData.HasCategory(380))
		{
			return RaisedPetAccType.Saddle;
		}
		if (inItemData.HasCategory(424))
		{
			return RaisedPetAccType.Materials;
		}
		return RaisedPetAccType.None;
	}

	public static RaisedPetAccType GetAccessoryType(string aname)
	{
		switch (aname)
		{
		case "Hat":
			return RaisedPetAccType.Hat;
		case "Texture":
			return RaisedPetAccType.Texture;
		case "Saddle":
			return RaisedPetAccType.Saddle;
		case "Horn":
			return RaisedPetAccType.Horn;
		case "BumpMap":
			return RaisedPetAccType.BumpMap;
		case "Materials":
			return RaisedPetAccType.Materials;
		default:
			Debug.LogError("Accessory type not found");
			return RaisedPetAccType.None;
		}
	}

	public bool HasAccessory(int inItemID)
	{
		foreach (RaisedPetAccType value in Enum.GetValues(typeof(RaisedPetAccType)))
		{
			int accessoryItemID = GetAccessoryItemID(value);
			if (inItemID == accessoryItemID)
			{
				return true;
			}
		}
		return false;
	}

	public RaisedPetAccessory GetAccessory(RaisedPetAccType atype)
	{
		return GetAccessory(atype.ToString());
	}

	public RaisedPetAccessory GetAccessory(string atype)
	{
		if (Accessories == null)
		{
			return null;
		}
		RaisedPetAccessory[] accessories = Accessories;
		foreach (RaisedPetAccessory raisedPetAccessory in accessories)
		{
			if (raisedPetAccessory.Type == atype)
			{
				return raisedPetAccessory;
			}
		}
		return null;
	}

	public static string GetAccessoryGeometry(RaisedPetAccessory ac)
	{
		if (ac == null || ac.Geometry == null)
		{
			return "";
		}
		return ac.Geometry.Split('&')[0];
	}

	public string GetAccessoryGeometry(RaisedPetAccType atype)
	{
		return GetAccessoryGeometry(GetAccessory(atype));
	}

	public string GetAccessoryGeometry(string atype)
	{
		return GetAccessoryGeometry(GetAccessory(atype));
	}

	public string GetAccessoryTexture(RaisedPetAccType atype)
	{
		return GetAccessoryTexture(GetAccessory(atype));
	}

	public string GetAccessoryTexture(string atype)
	{
		return GetAccessoryTexture(GetAccessory(atype));
	}

	public static string GetAccessoryTexture(RaisedPetAccessory ac)
	{
		if (ac == null || ac.Texture == null)
		{
			return "";
		}
		return ac.Texture;
	}

	public int GetAccessoryItemID(RaisedPetAccessory ac)
	{
		if (ac == null || string.IsNullOrEmpty(ac.Geometry))
		{
			return 0;
		}
		return UtStringUtil.Parse(ac.Geometry.Split('&')[1], 0);
	}

	public int GetAccessoryItemID(RaisedPetAccType atype)
	{
		RaisedPetAccessory accessory = GetAccessory(atype.ToString());
		return GetAccessoryItemID(accessory);
	}

	public void RemoveAccessory(RaisedPetAccType atype)
	{
		RemoveAccessory(atype.ToString());
	}

	public void RemoveAccessory(string atype)
	{
		RaisedPetAccessory accessory = GetAccessory(atype);
		if (accessory != null)
		{
			accessory.Geometry = "";
			accessory.Texture = "";
			accessory.UserInventoryCommonID = null;
			accessory.UserItemData = null;
		}
	}

	public void SetAccessory(RaisedPetAccType atype, string geo, string tex, int itemID, int userInventoryId = -1)
	{
		SetAccessory(atype.ToString(), geo, tex, itemID, userInventoryId);
	}

	public void SetAccessory(RaisedPetAccType atype, string geo, string tex, UserItemData userItem)
	{
		SetAccessory(atype.ToString(), geo, tex, userItem.ItemID, userItem.UserInventoryID, userItem);
	}

	private void SetAccessory(string atype, string geo, string tex, int itemID, int userInventoryId = -1, UserItemData userItem = null)
	{
		RaisedPetAccessory raisedPetAccessory = GetAccessory(atype);
		if (raisedPetAccessory == null)
		{
			if (Accessories == null)
			{
				Accessories = new RaisedPetAccessory[1];
			}
			else
			{
				Array.Resize(ref Accessories, Accessories.Length + 1);
			}
			raisedPetAccessory = new RaisedPetAccessory();
			raisedPetAccessory.Type = atype;
			Accessories[Accessories.Length - 1] = raisedPetAccessory;
		}
		raisedPetAccessory.Geometry = geo + "&" + itemID;
		raisedPetAccessory.Texture = tex;
	}

	public void SetSleepMode(bool issleep, bool savedata)
	{
		if (pIsSleeping != issleep)
		{
			pIsSleeping = issleep;
			if (savedata)
			{
				SaveData();
			}
		}
	}

	public void SetState(RaisedPetStage st)
	{
		SetState(st, savedata: false);
	}

	public void SetState(RaisedPetStage st, bool savedata)
	{
		if (pStage != st)
		{
			UtDebug.Log("Raised Pet state changed from [" + pStage.ToString() + "] to [" + st.ToString() + "]", 100);
			pStage = st;
			if (savedata)
			{
				SaveData();
			}
		}
	}

	public RaisedPetSkill GetSkillData(string skillName)
	{
		if (Skills == null || Skills.Length == 0)
		{
			return null;
		}
		RaisedPetSkill[] skills = Skills;
		foreach (RaisedPetSkill raisedPetSkill in skills)
		{
			if (raisedPetSkill.Key == skillName)
			{
				return raisedPetSkill;
			}
		}
		return null;
	}

	public void RemoveSkillData(string skillName, bool save)
	{
		UtDebug.Log("Skill [" + skillName + "] is removing");
		if (Skills == null || Skills.Length == 0)
		{
			return;
		}
		List<RaisedPetSkill> list = new List<RaisedPetSkill>();
		bool flag = false;
		RaisedPetSkill[] skills = Skills;
		foreach (RaisedPetSkill raisedPetSkill in skills)
		{
			if (raisedPetSkill != null && raisedPetSkill.Key != skillName)
			{
				flag = true;
				list.Add(raisedPetSkill);
			}
		}
		if (flag)
		{
			Skills = list.ToArray();
		}
	}

	public void UpdateSkillData(string skillName, float newval, bool save)
	{
		UtDebug.Log("Skill [" + skillName + "] updated to " + newval);
		if (Skills == null || Skills.Length == 0)
		{
			Skills = new RaisedPetSkill[1];
			Skills[0] = new RaisedPetSkill();
			Skills[0].Key = skillName;
			SetSkill(Skills[0], newval);
			if (save)
			{
				SaveData();
			}
			return;
		}
		RaisedPetSkill[] skills = Skills;
		foreach (RaisedPetSkill raisedPetSkill in skills)
		{
			if (raisedPetSkill.Key == skillName)
			{
				SetSkill(raisedPetSkill, newval);
				if (save)
				{
					SaveData();
				}
				return;
			}
		}
		int num = Skills.Length;
		Array.Resize(ref Skills, num + 1);
		Skills[num] = new RaisedPetSkill();
		Skills[num].Key = skillName;
		SetSkill(Skills[num], newval);
		if (save)
		{
			SaveData();
		}
	}

	public float GetSkillLevel(string skill)
	{
		return GetSkillData(skill)?.Value ?? 0f;
	}

	public void DumpDataX()
	{
		Debug.LogWarning(GetDebugString());
	}

	public void DumpData()
	{
		UtDebug.LogWarning(GetDebugString());
	}

	public string GetDebugString()
	{
		string text = "Raised Pet Data \n";
		text = text + "\tTypeID\t\t\t" + PetTypeID + "\n";
		string text2 = text;
		Guid userID = UserID;
		text = text2 + "\tUserID\t\t\t" + userID.ToString() + "\n";
		text = text + "\tName\t\t\t" + Name + "\n";
		text = (ImagePosition.HasValue ? (text + "\tImagePosition\t\t\t" + ImagePosition.Value + "\n") : (text + "\tImagePosition\t\t\tNULL\n"));
		if (GrowthState != null)
		{
			text = text + "\t\tGrowthState  ID\t\t\t" + GrowthState.GrowthStateID + "\n";
			text = text + "\t\tGrowthState  Name\t\t" + GrowthState.Name + "\n";
			text = text + "\t\tGrowthState  PetTypeID\t" + GrowthState.PetTypeID + "\n";
			text = text + "\t\tGrowthState  Order\t\t" + GrowthState.Order + "\n";
		}
		text = text + "\tGeometry\t\t" + Geometry + "\n";
		text = text + "\tTexture\t\t\t" + Texture + "\n";
		text = text + "\tGender\t\t\t" + Gender.ToString() + "\n";
		text += " Accessories----------------------------\n";
		if (Accessories != null)
		{
			RaisedPetAccessory[] accessories = Accessories;
			foreach (RaisedPetAccessory raisedPetAccessory in accessories)
			{
				text = text + "\t\t" + raisedPetAccessory.Type + " Geometry : " + raisedPetAccessory.Geometry + "\n                     Texture : " + raisedPetAccessory.Texture + "\n";
			}
		}
		text += " Attributes----------------------------\n";
		if (Attributes != null)
		{
			RaisedPetAttribute[] attributes = Attributes;
			foreach (RaisedPetAttribute raisedPetAttribute in attributes)
			{
				text = text + "\t\t" + raisedPetAttribute.Key + " = " + raisedPetAttribute.Value + "\n";
			}
		}
		text += " Skills----------------------------\n";
		if (Skills != null)
		{
			RaisedPetSkill[] skills = Skills;
			foreach (RaisedPetSkill raisedPetSkill in skills)
			{
				text = text + "\t\t" + raisedPetSkill.Key + " = " + raisedPetSkill.Value + " Date :" + raisedPetSkill.UpdateDate.ToString(UtUtilities.GetCultureInfo("en-US")) + "\n";
			}
		}
		text += " States----------------------------\n";
		if (States != null)
		{
			RaisedPetState[] states = States;
			foreach (RaisedPetState raisedPetState in states)
			{
				text = text + "\t\t" + raisedPetState.Key + " = " + raisedPetState.Value + "\n";
			}
		}
		text += " Colors----------------------------\n";
		if (Colors != null)
		{
			RaisedPetColor[] colors = Colors;
			foreach (RaisedPetColor raisedPetColor in colors)
			{
				text = text + "\t#   " + raisedPetColor.Order + " (" + raisedPetColor.Red + " ," + raisedPetColor.Green + " ," + raisedPetColor.Blue + ")\n";
			}
		}
		return text + "   Create Date :" + CreateDate.ToString(UtUtilities.GetCultureInfo("en-US")) + "\n";
	}

	public void SaveData()
	{
		if (pNoSave)
		{
			UtDebug.Log("Pet save disabled");
		}
		else
		{
			mIsDirty = true;
		}
	}

	public void SaveGlowData()
	{
		if (mGlowEffect != null)
		{
			SetAttrData("GlowEffect", pGlowEffect.Save(), DataType.STRING);
		}
	}

	public void SaveGlowData(string duration, string color)
	{
		if (mGlowEffect == null)
		{
			mGlowEffect = new GlowEffect();
		}
		SetAttrData("GlowEffect", pGlowEffect.Save(duration, color), DataType.STRING);
	}

	public bool IsGlowAvailable()
	{
		if (pGlowEffect != null)
		{
			return pGlowEffect.Duration > 0.0;
		}
		return false;
	}

	public bool IsGlowRunning()
	{
		if (pGlowEffect != null)
		{
			return pGlowEffect.EndTime > ServerTime.pCurrentTime;
		}
		return false;
	}

	public void RemoveGlowEffect()
	{
		SaveGlowData("-1", null);
	}

	public void SetupSaveData()
	{
		string cval = pHatchEndTime.ToString(UtUtilities.GetCultureInfo("en-US"));
		SetAttrData("HatchTime", cval, DataType.STRING);
		SetAttrData("Priority", pPriority.ToString(), DataType.INT);
		SetAttrData("FoodEffect", GetFoodEffectAsString(pFoodEffect, "|", "="), DataType.STRING);
		SetAttrData("IncubatorID", pIncubatorID.ToString(), DataType.INT);
		SetAttrData("NameCustomized", pIsNameCustomized.ToString(), DataType.BOOL);
		if (Attributes != null && Attributes.Length != 0)
		{
			List<RaisedPetAttribute> list = new List<RaisedPetAttribute>();
			RaisedPetAttribute[] attributes = Attributes;
			foreach (RaisedPetAttribute raisedPetAttribute in attributes)
			{
				if (mChangedAttributeNames.Contains(raisedPetAttribute.Key))
				{
					list.Add(raisedPetAttribute);
				}
			}
			Attributes = list.ToArray();
		}
		SyncState();
		DumpData();
	}

	public void SaveDataReal(RaisedPetSaveEventHandler callback = null, CommonInventoryRequest[] inInventoryRequest = null, bool savePetMeterAlone = false)
	{
		mIsDirty = false;
		if (pNoSave)
		{
			UtDebug.Log("Pet save disabled");
			SetRaisedPetResponse setRaisedPetResponse = new SetRaisedPetResponse();
			setRaisedPetResponse.RaisedPetSetResult = RaisedPetSetResult.Success;
			callback?.Invoke(setRaisedPetResponse);
		}
		else
		{
			if (mRaisedPetSaveEventHandler != null)
			{
				return;
			}
			RaisedPetColor[] colors = Colors;
			RaisedPetSkill[] skills = Skills;
			RaisedPetAttribute[] attributes = Attributes;
			if (!savePetMeterAlone)
			{
				string cval = pHatchEndTime.ToString(UtUtilities.GetCultureInfo("en-US"));
				SetAttrData("HatchTime", cval, DataType.STRING);
				SetAttrData("Priority", pPriority.ToString(), DataType.INT);
				SetAttrData("FoodEffect", GetFoodEffectAsString(pFoodEffect, "|", "="), DataType.STRING);
				SetAttrData("IncubatorID", pIncubatorID.ToString(), DataType.INT);
				SetAttrData("NameCustomized", pIsNameCustomized.ToString(), DataType.BOOL);
				if (Attributes != null && Attributes.Length != 0)
				{
					List<RaisedPetAttribute> list = new List<RaisedPetAttribute>();
					RaisedPetAttribute[] attributes2 = Attributes;
					foreach (RaisedPetAttribute raisedPetAttribute in attributes2)
					{
						if (mChangedAttributeNames.Contains(raisedPetAttribute.Key))
						{
							list.Add(raisedPetAttribute);
						}
					}
					Attributes = list.ToArray();
				}
			}
			else
			{
				Colors = null;
				Skills = null;
				Attributes = null;
			}
			SyncState();
			DumpData();
			mRaisedPetSaveEventHandler = callback;
			WsWebService.SetRaisedPet(this, inInventoryRequest, SetPetEventHandler, savePetMeterAlone);
			if (pTextureUpdated)
			{
				pTextureUpdated = false;
				int slotIdx = (ImagePosition.HasValue ? ImagePosition.Value : 0);
				ImageData.Save("EggColor", slotIdx, pTexture);
				ImageData.UpdateImages("EggColor");
			}
			if (!savePetMeterAlone)
			{
				if (attributes != null)
				{
					Attributes = attributes;
				}
			}
			else
			{
				Colors = colors;
				Skills = skills;
				Attributes = attributes;
			}
		}
	}

	public string SaveToResStringEx(string userdata)
	{
		string text = "";
		if (userdata != null && userdata.Length > 0)
		{
			text = text + "U$" + userdata + "*";
		}
		if (PetTypeID != 1)
		{
			text = text + "I$" + PetTypeID + "*";
		}
		if (Geometry != null && Geometry.Length > 0)
		{
			text = text + "G$" + Geometry + "*";
		}
		if (Texture != null && Texture.Length > 0)
		{
			text = text + "T$" + Texture + "*";
		}
		text += "A$";
		string text2 = "N";
		switch (pStage)
		{
		case RaisedPetStage.EGGINHAND:
			text2 = "E";
			break;
		case RaisedPetStage.BABY:
			text2 = "B";
			break;
		case RaisedPetStage.CHILD:
			text2 = "C";
			break;
		case RaisedPetStage.TEEN:
			text2 = "T";
			break;
		case RaisedPetStage.ADULT:
			text2 = "A";
			break;
		case RaisedPetStage.TITAN:
			text2 = "Ti";
			break;
		}
		text += text2;
		text += "*";
		text = ((!pIsSleeping) ? (text + "P$F*") : (text + "P$T*"));
		if (Colors != null && Colors.Length != 0)
		{
			text += "C$";
			RaisedPetColor[] colors = Colors;
			foreach (RaisedPetColor raisedPetColor in colors)
			{
				text = text + (int)(raisedPetColor.Red * 99f) + "$";
				text = text + (int)(raisedPetColor.Green * 99f) + "$";
				text = text + (int)(raisedPetColor.Blue * 99f) + "$";
			}
			text += "*";
		}
		if (Accessories != null)
		{
			text += "S$";
			RaisedPetAccessory[] accessories = Accessories;
			foreach (RaisedPetAccessory raisedPetAccessory in accessories)
			{
				text = text + raisedPetAccessory.Type + "$";
				text = text + raisedPetAccessory.Geometry + "$";
				text = text + raisedPetAccessory.Texture + "$";
			}
			text += "*";
		}
		if (Skills != null && Skills.Length != 0)
		{
			text += "SK$";
			RaisedPetSkill[] skills = Skills;
			foreach (RaisedPetSkill raisedPetSkill in skills)
			{
				text = text + raisedPetSkill.Key + "$";
				text = text + raisedPetSkill.Value + "$";
			}
			text += "*";
		}
		if (!string.IsNullOrEmpty(Name))
		{
			string text3 = "NM$" + Name + "*";
			text += text3;
		}
		if (CreateDate != DateTime.MinValue)
		{
			int num = UtUtilities.DateToInt(CreateDate);
			string text4 = text;
			int i = num;
			text = text4 + "CD$" + i + "*";
		}
		if (pRank > 0)
		{
			text = text + "RK$" + pRank + "*";
		}
		if (ImagePosition.HasValue)
		{
			text = text + "IP$" + ImagePosition.Value + "*";
		}
		if (IsGlowRunning())
		{
			text = text + "GC$" + pGlowEffect.GlowColor + "*";
		}
		return text;
	}

	public string ParseResStringEx(string s)
	{
		string result = "";
		PetTypeID = 1;
		Geometry = "";
		Texture = "";
		pStage = RaisedPetStage.ADULT;
		string[] array = s.Split('*');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split('$');
			if (array2[0] == "U")
			{
				result = array2[1];
			}
			else if (array2[0] == "I")
			{
				PetTypeID = UtStringUtil.Parse(array2[1], 1);
			}
			else if (array2[0] == "G")
			{
				Geometry = array2[1];
			}
			else if (array2[0] == "T")
			{
				Texture = array2[1];
			}
			else if (array2[0] == "C")
			{
				int num = (array2.Length - 1) / 3;
				Colors = new RaisedPetColor[num];
				int num2 = 1;
				for (int j = 0; j < num; j++)
				{
					Colors[j] = new RaisedPetColor();
					Colors[j].Order = j;
					Colors[j].Red = (float)UtStringUtil.Parse(array2[num2], 0) / 99f;
					num2++;
					Colors[j].Green = (float)UtStringUtil.Parse(array2[num2], 0) / 99f;
					num2++;
					Colors[j].Blue = (float)UtStringUtil.Parse(array2[num2], 0) / 99f;
					num2++;
				}
			}
			else if (array2[0] == "A")
			{
				if (array2[1] == "E")
				{
					pStage = RaisedPetStage.EGGINHAND;
				}
				else if (array2[1] == "B")
				{
					pStage = RaisedPetStage.BABY;
				}
				else if (array2[1] == "C")
				{
					pStage = RaisedPetStage.CHILD;
				}
				else if (array2[1] == "T")
				{
					pStage = RaisedPetStage.TEEN;
				}
				else if (array2[1] == "A")
				{
					pStage = RaisedPetStage.ADULT;
				}
				else if (array2[1] == "Ti")
				{
					pStage = RaisedPetStage.TITAN;
				}
			}
			else if (array2[0] == "S")
			{
				int num3 = (array2.Length - 1) / 3;
				Accessories = new RaisedPetAccessory[num3];
				int num4 = 1;
				for (int k = 0; k < num3; k++)
				{
					Accessories[k] = new RaisedPetAccessory();
					Accessories[k].Type = array2[num4];
					num4++;
					Accessories[k].Geometry = array2[num4];
					num4++;
					Accessories[k].Texture = array2[num4];
					num4++;
				}
			}
			else if (array2[0] == "P")
			{
				pIsSleeping = array2[1] == "T";
			}
			else if (array2[0] == "SK")
			{
				int num5 = (array2.Length - 1) / 2;
				Skills = new RaisedPetSkill[num5];
				int num6 = 1;
				for (int l = 0; l < num5; l++)
				{
					Skills[l] = new RaisedPetSkill();
					Skills[l].Key = array2[num6];
					num6++;
					Skills[l].Value = UtStringUtil.Parse(array2[num6], 0f);
					num6++;
				}
			}
			else if (array2[0] == "NM")
			{
				Name = array2[1];
			}
			else if (array2[0] == "RK")
			{
				pRank = UtStringUtil.Parse(array2[1], 1);
			}
			else if (array2[0] == "CD")
			{
				CreateDate = UtUtilities.IntToDate(UtStringUtil.Parse(array2[1], 1));
			}
			else if (array2[0] == "IP")
			{
				ImagePosition = UtStringUtil.Parse(array2[1], 1);
			}
			else if (array2[0] == "GC")
			{
				mGlowEffect = new GlowEffect();
				mGlowEffect.GlowColor = array2[1];
			}
		}
		return result;
	}

	public static bool IsMounted(string petData)
	{
		return petData.StartsWith("U$1");
	}

	public static RaisedPetData LoadFromResString(string s, out string userdata)
	{
		RaisedPetData raisedPetData = new RaisedPetData();
		string[] array = s.Split('*');
		if (array[0].Split('$').Length > 1)
		{
			userdata = raisedPetData.ParseResStringEx(s);
			return raisedPetData;
		}
		int num = array.Length;
		int num2 = 0;
		userdata = array[num2];
		num2++;
		raisedPetData.PetTypeID = UtStringUtil.Parse(array[num2], 1);
		num2++;
		raisedPetData.Geometry = array[num2];
		num2++;
		int num3 = (num - num2) / 3;
		raisedPetData.Colors = new RaisedPetColor[num3];
		for (int i = 0; i < num3; i++)
		{
			raisedPetData.Colors[i] = new RaisedPetColor();
			raisedPetData.Colors[i].Order = i;
			raisedPetData.Colors[i].Red = (float)UtStringUtil.Parse(array[num2], 0) / 99f;
			num2++;
			raisedPetData.Colors[i].Green = (float)UtStringUtil.Parse(array[num2], 0) / 99f;
			num2++;
			raisedPetData.Colors[i].Blue = (float)UtStringUtil.Parse(array[num2], 0) / 99f;
			num2++;
		}
		raisedPetData.pStage = RaisedPetStage.ADULT;
		return raisedPetData;
	}

	public RaisedPetAttribute FindAttrData(string sName)
	{
		if (Attributes == null)
		{
			return null;
		}
		RaisedPetAttribute[] attributes = Attributes;
		foreach (RaisedPetAttribute raisedPetAttribute in attributes)
		{
			if (raisedPetAttribute != null && raisedPetAttribute.Key == sName)
			{
				return raisedPetAttribute;
			}
		}
		return null;
	}

	public RaisedPetAttribute SetAttrData(string sName, string cval, DataType dt)
	{
		RaisedPetAttribute raisedPetAttribute = FindAttrData(sName);
		bool flag = false;
		if (raisedPetAttribute == null)
		{
			if (Attributes == null)
			{
				Attributes = new RaisedPetAttribute[1];
			}
			else
			{
				Array.Resize(ref Attributes, Attributes.Length + 1);
			}
			raisedPetAttribute = new RaisedPetAttribute();
			raisedPetAttribute.Key = sName;
			Attributes[Attributes.Length - 1] = raisedPetAttribute;
			flag = true;
		}
		if (flag || raisedPetAttribute.Value != cval)
		{
			mChangedAttributeNames.Add(sName);
		}
		raisedPetAttribute.Type = dt;
		raisedPetAttribute.Value = cval;
		return raisedPetAttribute;
	}

	public bool RemoveAttrData(string sName)
	{
		RaisedPetAttribute raisedPetAttribute = FindAttrData(sName);
		if (raisedPetAttribute == null)
		{
			return false;
		}
		RaisedPetAttribute[] array = new RaisedPetAttribute[Attributes.Length - 1];
		int num = 0;
		RaisedPetAttribute[] attributes = Attributes;
		foreach (RaisedPetAttribute raisedPetAttribute2 in attributes)
		{
			if (raisedPetAttribute2 != raisedPetAttribute)
			{
				array[num] = raisedPetAttribute2;
				num++;
			}
		}
		Attributes = array;
		return true;
	}

	public RaisedPetState FindStateData(string sName)
	{
		if (States == null)
		{
			return null;
		}
		RaisedPetState[] states = States;
		foreach (RaisedPetState raisedPetState in states)
		{
			if (raisedPetState != null && raisedPetState.Key == sName)
			{
				return raisedPetState;
			}
		}
		return null;
	}

	public RaisedPetState SetStateData(string sName, float cval)
	{
		RaisedPetState raisedPetState = FindStateData(sName);
		if (raisedPetState == null)
		{
			if (States == null)
			{
				States = new RaisedPetState[1];
			}
			else
			{
				Array.Resize(ref States, States.Length + 1);
			}
			raisedPetState = new RaisedPetState();
			raisedPetState.Key = sName;
			States[States.Length - 1] = raisedPetState;
		}
		raisedPetState.Value = cval;
		return raisedPetState;
	}

	public void SetColors(Color[] colorMap)
	{
		if (colorMap != null)
		{
			int num = colorMap.Length;
			Colors = new RaisedPetColor[num];
			for (int i = 0; i < num; i++)
			{
				Colors[i] = new RaisedPetColor();
				Colors[i].Order = i;
				Colors[i].Red = colorMap[i].r;
				Colors[i].Green = colorMap[i].g;
				Colors[i].Blue = colorMap[i].b;
			}
		}
	}

	public Color GetColor(int id)
	{
		Color result = Color.white;
		if (Colors != null && id < Colors.Length)
		{
			result = new Color(Colors[id].Red, Colors[id].Green, Colors[id].Blue, 1f);
		}
		return result;
	}

	private void SetSkill(RaisedPetSkill sd, float newval)
	{
		sd.Value = newval;
		sd.UpdateDate = ServerTime.pCurrentTime;
	}

	private static void AppendToPetArray(ref Dictionary<int, RaisedPetData[]> petArray, RaisedPetData[] pDataArray)
	{
		if (pDataArray != null)
		{
			for (int i = 0; i < pDataArray.Length; i++)
			{
				pDataArray[i].AppendToPetArray(ref petArray);
			}
		}
	}

	private bool AppendToActivePet()
	{
		return AppendToPetArray(ref pActivePets);
	}

	private bool AppendToInactivePet()
	{
		return AppendToPetArray(ref pReleasedPets);
	}

	private bool AppendToPetArray(ref Dictionary<int, RaisedPetData[]> petArray)
	{
		if (!petArray.ContainsKey(PetTypeID))
		{
			petArray.Add(PetTypeID, null);
		}
		pIsActive = petArray[PetTypeID] == pActivePets[PetTypeID];
		if (petArray[PetTypeID] == null)
		{
			petArray[PetTypeID] = new RaisedPetData[1];
			petArray[PetTypeID][0] = this;
			return true;
		}
		RaisedPetData[] array = petArray[PetTypeID];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].RaisedPetID == RaisedPetID)
			{
				return false;
			}
		}
		RaisedPetData[] array2 = petArray[PetTypeID];
		int num = array2.Length;
		Array.Resize(ref array2, num + 1);
		array2[num] = this;
		petArray[PetTypeID] = array2;
		return true;
	}

	private bool RemoveFromActivePet()
	{
		return RemoveFromPetArray(pActivePets);
	}

	private bool RemoveFromInactivePet()
	{
		return RemoveFromPetArray(pReleasedPets);
	}

	private bool RemoveFromPetArray(Dictionary<int, RaisedPetData[]> petArray)
	{
		if (!petArray.ContainsKey(PetTypeID))
		{
			return false;
		}
		if (petArray[PetTypeID] == null)
		{
			return false;
		}
		int num = Array.IndexOf(petArray[PetTypeID], this);
		if (num < 0)
		{
			return false;
		}
		int num2 = petArray[PetTypeID].Length;
		if (num2 == 1)
		{
			petArray[PetTypeID] = null;
			return true;
		}
		RaisedPetData[] array = new RaisedPetData[num2 - 1];
		int num3 = 0;
		for (int i = 0; i < num2; i++)
		{
			if (i != num)
			{
				array[num3] = petArray[PetTypeID][i];
				num3++;
			}
		}
		petArray[PetTypeID] = array;
		return true;
	}

	private void ResolveLoadedData(bool isactive)
	{
		pIsActive = isactive;
		RaisedPetAttribute raisedPetAttribute = FindAttrData("HatchTime");
		if (raisedPetAttribute != null)
		{
			SetHatchEndTime(DateTime.Parse(raisedPetAttribute.Value, UtUtilities.GetCultureInfo("en-US")));
		}
		else
		{
			SetHatchEndTime(ServerTime.pCurrentTime);
		}
		raisedPetAttribute = FindAttrData("Priority");
		if (raisedPetAttribute != null)
		{
			pPriority = UtStringUtil.Parse(raisedPetAttribute.Value, 0);
		}
		else
		{
			pPriority = 0;
		}
		raisedPetAttribute = FindAttrData("FoodEffect");
		if (raisedPetAttribute != null)
		{
			SplitFoodEffect(raisedPetAttribute.Value, '|', '=');
		}
		pIsSleeping = false;
		raisedPetAttribute = FindAttrData("IncubatorID");
		if (raisedPetAttribute != null)
		{
			pIncubatorID = UtStringUtil.Parse(raisedPetAttribute.Value, 0);
		}
		else
		{
			pIncubatorID = -1;
		}
		pRank = 1;
		raisedPetAttribute = FindAttrData("NameCustomized");
		if (raisedPetAttribute != null)
		{
			pIsNameCustomized = UtStringUtil.Parse(raisedPetAttribute.Value, inDefault: false);
		}
		else
		{
			pIsNameCustomized = false;
		}
		if (GrowthState != null)
		{
			SetState(FindStateByName(GrowthState.Name));
			if (pStage == RaisedPetStage.NONE)
			{
				UtDebug.LogError("pet state is bad = " + GrowthState.Name);
			}
		}
		else
		{
			UtDebug.LogError("pet state is bad, GrowthState is null");
		}
	}

	private RaisedPetStage FindStateByName(string sname)
	{
		string[] names = Enum.GetNames(typeof(RaisedPetStage));
		for (int i = 0; i < names.Length; i++)
		{
			if (string.Compare(sname, names[i], StringComparison.OrdinalIgnoreCase) == 0)
			{
				return (RaisedPetStage)i;
			}
		}
		return RaisedPetStage.NONE;
	}

	private void SyncState()
	{
		if (!pGrowStates.ContainsKey(PetTypeID) || pGrowStates[PetTypeID] == null)
		{
			return;
		}
		RaisedPetGrowthState[] array = pGrowStates[PetTypeID];
		foreach (RaisedPetGrowthState raisedPetGrowthState in array)
		{
			if (string.Compare(raisedPetGrowthState.Name, pStage.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
			{
				GrowthState = raisedPetGrowthState;
			}
		}
	}

	public static void SetAllPetUnselected()
	{
		if (pActivePets == null)
		{
			return;
		}
		foreach (RaisedPetData[] value in pActivePets.Values)
		{
			if (value == null)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				if (raisedPetData != null)
				{
					raisedPetData.IsSelected = false;
				}
			}
		}
	}

	public static void ResolvePetArray(RaisedPetData[] parray, bool isactive)
	{
		if (parray != null && parray.Length != 0)
		{
			for (int i = 0; i < parray.Length; i++)
			{
				parray[i].ResolveLoadedData(isactive);
			}
		}
	}

	private void SetPetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			if (mRaisedPetSaveEventHandler != null)
			{
				RaisedPetSaveEventHandler raisedPetSaveEventHandler2 = mRaisedPetSaveEventHandler;
				mRaisedPetSaveEventHandler = null;
				raisedPetSaveEventHandler2(new SetRaisedPetResponse
				{
					RaisedPetSetResult = RaisedPetSetResult.Failure
				});
			}
			Debug.LogError("Raised Pet data set failed");
			break;
		case WsServiceEvent.COMPLETE:
		{
			SetRaisedPetResponse setRaisedPetResponse = (SetRaisedPetResponse)inObject;
			if (setRaisedPetResponse != null && setRaisedPetResponse.RaisedPetSetResult == RaisedPetSetResult.Success)
			{
				if (inUserData != null && !(bool)inUserData)
				{
					mChangedAttributeNames.Clear();
				}
			}
			else
			{
				Debug.LogError("Raised Pet data set failed");
			}
			if (mRaisedPetSaveEventHandler != null)
			{
				RaisedPetSaveEventHandler raisedPetSaveEventHandler = mRaisedPetSaveEventHandler;
				mRaisedPetSaveEventHandler = null;
				raisedPetSaveEventHandler(setRaisedPetResponse);
			}
			else if (setRaisedPetResponse != null && setRaisedPetResponse.RaisedPetSetResult == RaisedPetSetResult.InvalidPetName)
			{
				Name = null;
			}
			break;
		}
		}
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			Debug.LogError("!!!" + inType.ToString() + " failed!!!!");
			break;
		case WsServiceEvent.COMPLETE:
		{
			int num = 0;
			switch (inType)
			{
			case WsServiceType.SET_RAISED_PET_INACTIVE:
			{
				RaisedPetData raisedPetData4 = (RaisedPetData)inUserData;
				bool flag2 = (bool)inObject;
				if (flag2)
				{
					UtDebug.Log("Release Pet successful");
					raisedPetData4.RemoveFromActivePet();
					if (IsReleasedPetDataReady(raisedPetData4.PetTypeID))
					{
						raisedPetData4.AppendToInactivePet();
					}
				}
				else
				{
					Debug.LogError("Release Pet failed");
				}
				if (mReleaseCallback != null)
				{
					mReleaseCallback(flag2);
					mReleaseCallback = null;
				}
				break;
			}
			case WsServiceType.GET_ACTIVE_RAISED_PET:
			{
				RaisedPetGetData raisedPetGetData5 = (RaisedPetGetData)inUserData;
				num = raisedPetGetData5.mType;
				RaisedPetData[] array6 = (RaisedPetData[])inObject;
				ResolvePetArray(array6, isactive: true);
				if (raisedPetGetData5.mIsMMOUser)
				{
					if (raisedPetGetData5.mCallback != null)
					{
						raisedPetGetData5.mCallback(raisedPetGetData5.mType, array6, raisedPetGetData5.mUserData);
					}
					break;
				}
				pActivePets[num] = array6;
				if (array6 == null)
				{
					Debug.LogError("No pet data  for type:" + num);
				}
				if (raisedPetGetData5.mCallback != null)
				{
					raisedPetGetData5.mCallback(raisedPetGetData5.mType, array6, raisedPetGetData5.mUserData);
				}
				break;
			}
			case WsServiceType.GET_RELEASED_RAISED_PET:
			{
				RaisedPetGetData raisedPetGetData3 = (RaisedPetGetData)inUserData;
				num = raisedPetGetData3.mType;
				RaisedPetData[] array3 = (RaisedPetData[])inObject;
				ResolvePetArray(array3, isactive: false);
				if (!raisedPetGetData3.mIsMMOUser)
				{
					pReleasedPets[num] = array3;
				}
				if (raisedPetGetData3.mCallback != null)
				{
					raisedPetGetData3.mCallback(raisedPetGetData3.mType, array3, raisedPetGetData3.mUserData);
				}
				break;
			}
			case WsServiceType.GET_SELECTED_RAISED_PET:
			{
				RaisedPetGetData raisedPetGetData4 = (RaisedPetGetData)inUserData;
				num = -1;
				RaisedPetData[] array4 = (RaisedPetData[])inObject;
				ResolvePetArray(array4, isactive: true);
				if (array4 != null && array4.Length != 0)
				{
					num = array4[0].PetTypeID;
					if (raisedPetGetData4.mIsMMOUser)
					{
						if (raisedPetGetData4.mCallback != null)
						{
							raisedPetGetData4.mCallback(num, array4, raisedPetGetData4.mUserData);
						}
						break;
					}
					RaisedPetData[] array5 = array4;
					for (int i = 0; i < array5.Length; i++)
					{
						array5[i].AppendToActivePet();
					}
				}
				if (raisedPetGetData4.mCallback != null)
				{
					raisedPetGetData4.mCallback(num, array4, raisedPetGetData4.mUserData);
				}
				break;
			}
			case WsServiceType.GET_ALL_ACTIVE_PETS_BY_USER_ID:
			{
				RaisedPetGetData raisedPetGetData6 = (RaisedPetGetData)inUserData;
				num = -1;
				RaisedPetData[] array7 = (RaisedPetData[])inObject;
				ResolvePetArray(array7, isactive: true);
				if (raisedPetGetData6.mFlushCache)
				{
					pActivePets.Clear();
				}
				if (array7 != null && array7.Length != 0)
				{
					RaisedPetData[] array5 = array7;
					foreach (RaisedPetData raisedPetData3 in array5)
					{
						if (raisedPetData3.IsSelected && raisedPetData3.PetTypeID != -1)
						{
							num = raisedPetData3.PetTypeID;
							if (raisedPetGetData6.mIsMMOUser)
							{
								if (raisedPetGetData6.mCallback != null)
								{
									raisedPetGetData6.mCallback(num, array7, raisedPetGetData6.mUserData);
								}
								return;
							}
						}
						raisedPetData3.AppendToActivePet();
					}
				}
				if (raisedPetGetData6.mCallback != null)
				{
					raisedPetGetData6.mCallback(num, array7, raisedPetGetData6.mUserData);
				}
				break;
			}
			case WsServiceType.SET_SELECTED_PET:
			{
				bool flag = (bool)inObject;
				if (flag)
				{
					SetAllPetUnselected();
					RaisedPetData raisedPetData2 = (RaisedPetData)inUserData;
					if (raisedPetData2 != null)
					{
						raisedPetData2.IsSelected = true;
					}
				}
				else
				{
					Debug.LogError("Select Pet failed");
				}
				if (mSelectCallback != null)
				{
					mSelectCallback(flag);
					mSelectCallback = null;
				}
				break;
			}
			case WsServiceType.GET_SELECTED_RAISED_PET_BY_TYPE:
			{
				RaisedPetGetData raisedPetGetData = (RaisedPetGetData)inUserData;
				num = raisedPetGetData.mType;
				RaisedPetData[] array = (RaisedPetData[])inObject;
				ResolvePetArray(array, isactive: true);
				if (raisedPetGetData.mIsMMOUser)
				{
					if (raisedPetGetData.mCallback != null)
					{
						raisedPetGetData.mCallback(raisedPetGetData.mType, array, raisedPetGetData.mUserData);
					}
					break;
				}
				pActivePets[num] = array;
				if (array == null)
				{
					Debug.LogError("No selected pet data  for type:" + num);
				}
				if (raisedPetGetData.mCallback != null)
				{
					raisedPetGetData.mCallback(raisedPetGetData.mType, array, raisedPetGetData.mUserData);
				}
				break;
			}
			case WsServiceType.GET_UNSELECTED_PETS_BY_TYPES:
			{
				RaisedPetGetData raisedPetGetData2 = (RaisedPetGetData)inUserData;
				num = raisedPetGetData2.mType;
				RaisedPetData[] array2 = (RaisedPetData[])inObject;
				ResolvePetArray(array2, isactive: true);
				if (raisedPetGetData2.mIsMMOUser)
				{
					if (raisedPetGetData2.mCallback != null)
					{
						raisedPetGetData2.mCallback(raisedPetGetData2.mType, array2, raisedPetGetData2.mUserData);
					}
					break;
				}
				if (raisedPetGetData2.mFlushCache)
				{
					pActivePets.Clear();
				}
				AppendToPetArray(ref pActivePets, array2);
				if (raisedPetGetData2.mCallback != null)
				{
					raisedPetGetData2.mCallback(raisedPetGetData2.mType, array2, raisedPetGetData2.mUserData);
				}
				break;
			}
			case WsServiceType.CREATE_RAISED_PET:
			{
				RaisedPetCreateData raisedPetCreateData = (RaisedPetCreateData)inUserData;
				num = raisedPetCreateData.mType;
				RaisedPetData raisedPetData = null;
				if (inObject is CreatePetResponse)
				{
					if (inObject is CreatePetResponse createPetResponse)
					{
						raisedPetData = createPetResponse.RaisedPetData;
					}
				}
				else
				{
					raisedPetData = inObject as RaisedPetData;
				}
				if (raisedPetData == null)
				{
					Debug.LogError("Failed creating pet data for type:" + num);
				}
				else
				{
					UtDebug.Log("New Created Pet Data is ready");
					raisedPetData.ResolveLoadedData(isactive: true);
					raisedPetData.DumpData();
					raisedPetData.AppendToActivePet();
					if (pGrowStates.ContainsKey(num) && pGrowStates[num] == null)
					{
						Debug.LogError("Should not be here!!!");
					}
				}
				if (raisedPetCreateData.mCallback != null)
				{
					raisedPetCreateData.mCallback(raisedPetCreateData.mType, raisedPetData, raisedPetCreateData.mUserData);
				}
				break;
			}
			case WsServiceType.GET_INACTIVE_RAISED_PET:
			case WsServiceType.GET_ACTIVE_RAISED_PETS_BY_TYPEIDS:
			case WsServiceType.SET_ACTIVE_RAISED_PET:
				break;
			}
			break;
		}
		}
	}

	public static void SetGrowthState(int inTypeID, RaisedPetGrowthState[] inGrowthState)
	{
		pGrowStates[inTypeID] = inGrowthState;
	}

	public static RaisedPetData CreateCustomizedPetData(int ptype, RaisedPetStage stage, string resName, Gender gender, Color[] colorMap, bool noColorMap)
	{
		RaisedPetData raisedPetData = new RaisedPetData();
		raisedPetData.PetTypeID = ptype;
		raisedPetData.pStage = stage;
		raisedPetData.Texture = "";
		raisedPetData.Gender = gender;
		if (resName == null || resName.Length == 0)
		{
			raisedPetData.Geometry = "RS_SHARED/DragonBoyBashFul/PfDragonBoyBashFul";
		}
		else
		{
			raisedPetData.Geometry = resName;
		}
		if (colorMap != null)
		{
			raisedPetData.SetColors(colorMap);
		}
		else if (!noColorMap)
		{
			raisedPetData.SetColors(new Color[3]
			{
				Color.red,
				Color.green,
				Color.black
			});
		}
		raisedPetData.Skills = null;
		return raisedPetData;
	}

	public static void FlushSaveCache(int typeID)
	{
		if (!pActivePets.ContainsKey(typeID))
		{
			return;
		}
		RaisedPetData[] array;
		if (pActivePets[typeID] != null)
		{
			array = pActivePets[typeID];
			foreach (RaisedPetData raisedPetData in array)
			{
				if (raisedPetData != null && raisedPetData.mIsDirty)
				{
					raisedPetData.SaveDataReal();
					return;
				}
			}
		}
		if (!pReleasedPets.ContainsKey(typeID) || pReleasedPets[typeID] == null || pReleasedPets[typeID] == null)
		{
			return;
		}
		array = pReleasedPets[typeID];
		foreach (RaisedPetData raisedPetData2 in array)
		{
			if (raisedPetData2 != null && raisedPetData2.mIsDirty)
			{
				raisedPetData2.SaveDataReal();
				break;
			}
		}
	}

	private void SplitFoodEffect(string dateTimeString, char seperator, char keySeperator)
	{
		string[] array = dateTimeString.Split(seperator);
		if (array.Length == 0)
		{
			return;
		}
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = array2[i].Split(keySeperator);
			if (array3.Length != 0)
			{
				int result = -1;
				if (int.TryParse(array3[0], out result))
				{
					pFoodEffect[result] = DateTime.Parse(array3[1], UtUtilities.GetCultureInfo("en-US"));
				}
			}
		}
	}

	private string GetFoodEffectString()
	{
		return GetFoodEffectAsString(pFoodEffect, "|", "=");
	}

	private string GetFoodEffectAsString(Dictionary<int, DateTime> inFoodEffect, string seperator, string keySeperator)
	{
		string text = string.Empty;
		foreach (KeyValuePair<int, DateTime> item in inFoodEffect)
		{
			text = ((!string.IsNullOrEmpty(text)) ? (text + seperator + item.Key + keySeperator + item.Value.ToString(UtUtilities.GetCultureInfo("en-US"))) : (item.Key + keySeperator + item.Value.ToString(UtUtilities.GetCultureInfo("en-US"))));
		}
		return text;
	}

	public static List<RaisedPetData> GetActiveDragons()
	{
		List<RaisedPetData> list = new List<RaisedPetData>();
		if (pActivePets == null)
		{
			return list;
		}
		foreach (RaisedPetData[] value in pActivePets.Values)
		{
			if (value != null)
			{
				RaisedPetData[] array = value;
				foreach (RaisedPetData item in array)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public bool IsPetCustomized()
	{
		RaisedPetAttribute raisedPetAttribute = FindAttrData("_LastCustomizedStage");
		RaisedPetAttribute raisedPetAttribute2 = FindAttrData("NameCustomized");
		if (raisedPetAttribute == null || string.IsNullOrEmpty(raisedPetAttribute.Value))
		{
			if (raisedPetAttribute2 != null)
			{
				return UtStringUtil.Parse(raisedPetAttribute2.Value, inDefault: false);
			}
			return false;
		}
		return true;
	}

	public ItemStat[] GetAccessoriesCombinedStats()
	{
		if (Accessories == null)
		{
			return null;
		}
		List<ItemStat> list = new List<ItemStat>();
		for (int j = 0; j < Accessories.Length; j++)
		{
			if (Accessories[j].UserItemData == null)
			{
				continue;
			}
			ItemStat[] accessoryStats = Accessories[j].UserItemData.ItemStats;
			if (accessoryStats == null || accessoryStats.Length == 0)
			{
				continue;
			}
			int i;
			for (i = 0; i < accessoryStats.Length; i++)
			{
				ItemStat itemStat = list.Find((ItemStat x) => x.ItemStatID == accessoryStats[i].ItemStatID);
				if (itemStat != null)
				{
					int result = 0;
					int result2 = 0;
					int.TryParse(accessoryStats[i].Value, out result);
					int.TryParse(itemStat.Value, out result2);
					itemStat.Value = (result2 + result).ToString();
				}
				else
				{
					list.Add(accessoryStats[i]);
				}
			}
		}
		return list.ToArray();
	}
}
