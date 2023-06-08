using UnityEngine;

public class AvAvatarGenerator : MonoBehaviour
{
	public AvatarPartChanger[] _PartTabs;

	public int[] _PartStoreIDs;

	public bool pIsReady;

	public StoreData[] mStoreData;

	private int mNumStoresLoaded;

	private Color[] mCustomColors = new Color[4];

	private bool mCustomColorsReady;

	public void SetCustomColors(Color skinColor, Color hairColor, Color eyeColor, Color warpaintColor)
	{
		mCustomColors[CustomAvatarState.pCustomAvatarSettings.SKINCOLOR_INDEX] = skinColor;
		mCustomColors[CustomAvatarState.pCustomAvatarSettings.HAIRCOLOR_INDEX] = hairColor;
		mCustomColors[CustomAvatarState.pCustomAvatarSettings.EYECOLOR_INDEX] = eyeColor;
		mCustomColors[CustomAvatarState.pCustomAvatarSettings.WARPAINTCOLOR_INDEX] = warpaintColor;
		mCustomColorsReady = true;
	}

	private void SetPartOffset(AvatarDataPart part, int index, Color c)
	{
		if (part != null && part.Offsets != null && part.Offsets.Length > index)
		{
			AvatarDataPartOffset avatarDataPartOffset = new AvatarDataPartOffset();
			avatarDataPartOffset.X = c.r;
			avatarDataPartOffset.Y = c.g;
			avatarDataPartOffset.Z = c.b;
			part.Offsets[index] = avatarDataPartOffset;
		}
	}

	public AvatarData.InstanceInfo GenerateRandomAvatar(GameObject obj, Gender genderType = Gender.Unknown, bool useCustomColors = false)
	{
		if (mStoreData == null)
		{
			return null;
		}
		string text = "M";
		if (genderType == Gender.Unknown)
		{
			genderType = Gender.Male;
			if (Random.Range(0, 2) == 0)
			{
				genderType = Gender.Female;
			}
		}
		if (genderType == Gender.Female)
		{
			text = "F";
		}
		AvatarData mInstance = AvatarData.CreateDefault(genderType);
		AvatarData.InstanceInfo instanceInfo = new AvatarData.InstanceInfo();
		instanceInfo.mAvatar = obj;
		instanceInfo.mInstance = mInstance;
		instanceInfo.mBundlesReady = true;
		instanceInfo.mLoading = false;
		instanceInfo.mInitializedFromPreviousSave = false;
		instanceInfo.mMergeWithDefault = false;
		CustomAvatarState customAvatarState = new CustomAvatarState();
		customAvatarState.FromAvatarData(instanceInfo.mInstance);
		int num = 0;
		AvatarPartChanger[] partTabs = _PartTabs;
		foreach (AvatarPartChanger avatarPartChanger in partTabs)
		{
			int categoryID = AvatarData.GetCategoryID(avatarPartChanger._PrtTypeName);
			int num2 = Random.Range(0, _PartStoreIDs.Length);
			if (avatarPartChanger._DefaultStoreIDs.Length != 0)
			{
				int num3 = avatarPartChanger._DefaultStoreIDs[Random.Range(0, avatarPartChanger._DefaultStoreIDs.Length)];
				for (num = 0; num < _PartStoreIDs.Length; num++)
				{
					if (_PartStoreIDs[num] == num3)
					{
						num2 = num;
						break;
					}
				}
			}
			StoreData storeData = mStoreData[num2];
			if (storeData == null)
			{
				Debug.LogWarning("Store data is null!!");
				continue;
			}
			StoreCategoryData storeCategoryData = storeData.FindCategoryData(categoryID);
			if (storeCategoryData == null || storeCategoryData._Items == null)
			{
				Debug.LogWarning("Store " + _PartStoreIDs[num2] + " doesnt have item in category : " + categoryID);
			}
			else
			{
				if (storeCategoryData._Items.Count <= 0)
				{
					continue;
				}
				int num4 = Random.Range(0, storeCategoryData._Items.Count);
				for (num = 0; num < storeCategoryData._Items.Count; num++)
				{
					string attribute = storeCategoryData._Items[num4].GetAttribute("Gender", "U");
					if (!storeCategoryData._Items[num4].Locked && (attribute == text || attribute == "U") && PrereqCheck(storeCategoryData._Items[num4]))
					{
						break;
					}
					num4++;
					if (num4 == storeCategoryData._Items.Count)
					{
						num4 = 0;
					}
				}
				customAvatarState.ApplyItem(storeCategoryData._Items[num4], avatarPartChanger._PrtTypeName);
			}
		}
		customAvatarState.ToAvatarData(instanceInfo);
		if (useCustomColors && mCustomColorsReady)
		{
			AvatarDataPart avatarDataPart = instanceInfo.FindPart(AvatarData.pPartSettings.AVATAR_PART_HEAD);
			if (avatarDataPart != null)
			{
				avatarDataPart.Offsets = new AvatarDataPartOffset[4];
				SetPartOffset(avatarDataPart, CustomAvatarState.pCustomAvatarSettings.SKINCOLOR_INDEX, mCustomColors[CustomAvatarState.pCustomAvatarSettings.SKINCOLOR_INDEX]);
				SetPartOffset(avatarDataPart, CustomAvatarState.pCustomAvatarSettings.HAIRCOLOR_INDEX, mCustomColors[CustomAvatarState.pCustomAvatarSettings.HAIRCOLOR_INDEX]);
				SetPartOffset(avatarDataPart, CustomAvatarState.pCustomAvatarSettings.EYECOLOR_INDEX, mCustomColors[CustomAvatarState.pCustomAvatarSettings.EYECOLOR_INDEX]);
				SetPartOffset(avatarDataPart, CustomAvatarState.pCustomAvatarSettings.WARPAINTCOLOR_INDEX, mCustomColors[CustomAvatarState.pCustomAvatarSettings.WARPAINTCOLOR_INDEX]);
			}
			mCustomColorsReady = false;
		}
		instanceInfo.LoadBundlesAndUpdateAvatar();
		return instanceInfo;
	}

	private bool PrereqCheck(ItemData item)
	{
		bool result = true;
		if (!item.IsSubPart() && item.Relationship != null)
		{
			ItemDataRelationship[] relationship = item.Relationship;
			foreach (ItemDataRelationship itemDataRelationship in relationship)
			{
				if (itemDataRelationship.Type == "Prereq")
				{
					result = false;
					if ((CommonInventoryData.pIsReady && CommonInventoryData.pInstance.FindItem(itemDataRelationship.ItemId) != null) || (ParentData.pIsReady && ParentData.pInstance.HasItem(itemDataRelationship.ItemId)))
					{
						result = true;
					}
					break;
				}
			}
		}
		return result;
	}

	private void OnStoreLoaded(StoreData sd)
	{
		mStoreData[mNumStoresLoaded] = sd;
		mNumStoresLoaded++;
		if (mNumStoresLoaded == _PartStoreIDs.Length)
		{
			pIsReady = true;
		}
	}

	private void Start()
	{
		mNumStoresLoaded = 0;
		mStoreData = new StoreData[_PartStoreIDs.Length];
		int[] partStoreIDs = _PartStoreIDs;
		for (int i = 0; i < partStoreIDs.Length; i++)
		{
			ItemStoreDataLoader.Load(partStoreIDs[i], OnStoreLoaded);
		}
	}
}
