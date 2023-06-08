using System;
using System.Collections.Generic;
using KA.Framework;
using UnityEngine;

public class CustomAvatarState
{
	public Color[] mColors = new Color[4];

	public bool mIsDirty;

	public static CustomAvatarState mCurrentInstance;

	private List<string> mLoadedTextures = new List<string>();

	private List<CustomInfo> mCustomParts = new List<CustomInfo>();

	private bool mHasCache;

	private Dictionary<string, string> mTextures = new Dictionary<string, string>();

	private Dictionary<string, string> mTexturesCache = new Dictionary<string, string>();

	private Dictionary<string, string> mGeometries = new Dictionary<string, string>();

	private Dictionary<string, string> mGeometriesCache = new Dictionary<string, string>();

	private Dictionary<string, int> mUiids = new Dictionary<string, int>();

	private Dictionary<string, int> mUiidsCache = new Dictionary<string, int>();

	public bool mIsMale;

	public static AvatarSettings.CustomAvatarSettings pCustomAvatarSettings => AvatarSettings.pInstance._CustomAvatarSettings;

	public void CacheState()
	{
		mTexturesCache.Clear();
		mGeometriesCache.Clear();
		mUiidsCache.Clear();
		mHasCache = true;
		foreach (KeyValuePair<string, string> mTexture in mTextures)
		{
			if (mTexturesCache.ContainsKey(mTexture.Key))
			{
				mTexturesCache[mTexture.Key] = (string)mTexture.Value.Clone();
			}
			else
			{
				mTexturesCache.Add(mTexture.Key, (string)mTexture.Value.Clone());
			}
		}
		foreach (KeyValuePair<string, string> mGeometry in mGeometries)
		{
			if (mGeometriesCache.ContainsKey(mGeometry.Key))
			{
				mGeometriesCache[mGeometry.Key] = (string)mGeometry.Value.Clone();
			}
			else
			{
				mGeometriesCache.Add(mGeometry.Key, (string)mGeometry.Value.Clone());
			}
		}
		foreach (KeyValuePair<string, int> mUiid in mUiids)
		{
			if (mUiidsCache.ContainsKey(mUiid.Key))
			{
				mUiidsCache[mUiid.Key] = mUiid.Value;
			}
			else
			{
				mUiidsCache.Add(mUiid.Key, mUiid.Value);
			}
		}
	}

	public void RestoreAll()
	{
		if (!mHasCache)
		{
			return;
		}
		mIsDirty = true;
		mTextures.Clear();
		mGeometries.Clear();
		mUiids.Clear();
		foreach (KeyValuePair<string, string> item in mTexturesCache)
		{
			if (mTextures.ContainsKey(item.Key))
			{
				mTextures[item.Key] = (string)item.Value.Clone();
			}
			else
			{
				mTextures.Add(item.Key, (string)item.Value.Clone());
			}
		}
		foreach (KeyValuePair<string, string> item2 in mGeometriesCache)
		{
			if (mGeometries.ContainsKey(item2.Key))
			{
				mGeometries[item2.Key] = (string)item2.Value.Clone();
			}
			else
			{
				mGeometries.Add(item2.Key, (string)item2.Value.Clone());
			}
		}
		foreach (KeyValuePair<string, int> item3 in mUiidsCache)
		{
			if (mUiids.ContainsKey(item3.Key))
			{
				mUiids[item3.Key] = item3.Value;
			}
			else
			{
				mUiids.Add(item3.Key, item3.Value);
			}
		}
	}

	public void SetColor(int index, Color c)
	{
		mColors[index] = c;
		mIsDirty = true;
	}

	public void SetColors(Color skinColor, Color hairColor, Color eyeColor, Color warpaintColor)
	{
		mColors[pCustomAvatarSettings.SKINCOLOR_INDEX] = skinColor;
		mColors[pCustomAvatarSettings.HAIRCOLOR_INDEX] = hairColor;
		mColors[pCustomAvatarSettings.EYECOLOR_INDEX] = eyeColor;
		mColors[pCustomAvatarSettings.WARPAINTCOLOR_INDEX] = warpaintColor;
	}

	public void SetTextureData(string part, string type, string rsPath)
	{
		string key = part + type;
		if (mTextures.ContainsKey(key))
		{
			mTextures[key] = rsPath;
		}
		else
		{
			mTextures.Add(key, rsPath);
		}
		mIsDirty = true;
	}

	public void SetData(string inSubPartOne, string inSubPartTwo, ItemData inItemData)
	{
		if (!string.IsNullOrEmpty(inItemData.AssetName))
		{
			SetGeometryData(inSubPartOne, inItemData.AssetName);
		}
		if (!string.IsNullOrEmpty(inItemData.Geometry2))
		{
			SetGeometryData(inSubPartTwo, inItemData.Geometry2);
		}
		SetTextureData(inSubPartOne, pCustomAvatarSettings.DETAIL, inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
		SetTextureData(inSubPartTwo, pCustomAvatarSettings.DETAIL, inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
	}

	public string GetData(string part, string type)
	{
		string key = part + type;
		if (mTextures.ContainsKey(key))
		{
			return mTextures[key];
		}
		return string.Empty;
	}

	public void SetGeometryData(string part, string rsPath)
	{
		if (mGeometries.ContainsKey(part))
		{
			mGeometries[part] = rsPath;
		}
		else
		{
			mGeometries.Add(part, rsPath);
		}
		mIsDirty = true;
	}

	public string GetGeometryData(string part)
	{
		if (mGeometries.ContainsKey(part))
		{
			return mGeometries[part];
		}
		return string.Empty;
	}

	public void SetInventoryId(string partType, int uiid, bool saveDefault = false)
	{
		if (mUiids.ContainsKey(partType))
		{
			if (saveDefault && !mUiids.ContainsKey("DEFAULT_" + partType))
			{
				mUiids["DEFAULT_" + partType] = mUiids[partType];
				mUiids[partType] = uiid;
			}
			else
			{
				mUiids[partType] = uiid;
			}
		}
		else
		{
			mUiids.Add(partType, uiid);
		}
		mIsDirty = true;
	}

	public int GetInventoryId(string partType)
	{
		if (mUiids.ContainsKey(partType))
		{
			return mUiids[partType];
		}
		return -1;
	}

	public void FromDefault(Gender gender)
	{
		AvatarData inData = AvatarData.CreateDefault(gender);
		FromAvatarData(inData, bVersionCheck: false);
	}

	public void FromAvatarData(AvatarData inData, bool bVersionCheck = true)
	{
		mIsDirty = true;
		mTextures.Clear();
		mGeometries.Clear();
		mUiids.Clear();
		mIsMale = inData.GenderType == Gender.Male;
		if (bVersionCheck)
		{
			for (int i = 0; i < inData.Part.Length; i++)
			{
				AvatarDataPart avatarDataPart = inData.Part[i];
				if (avatarDataPart.PartType == "Version" && avatarDataPart.GetVersion().x < AvatarData.VERSION.x)
				{
					FromDefault(inData.GenderType);
					return;
				}
			}
		}
		for (int j = 0; j < inData.Part.Length; j++)
		{
			AvatarDataPart avatarDataPart2 = inData.Part[j];
			if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_HEAD)
			{
				AvatarDataPartOffset[] offsets = avatarDataPart2.Offsets;
				if (offsets != null && offsets.Length >= 4)
				{
					mColors[pCustomAvatarSettings.SKINCOLOR_INDEX] = ColorFromOffset(avatarDataPart2.Offsets[0]);
					mColors[pCustomAvatarSettings.HAIRCOLOR_INDEX] = ColorFromOffset(avatarDataPart2.Offsets[1]);
					mColors[pCustomAvatarSettings.EYECOLOR_INDEX] = ColorFromOffset(avatarDataPart2.Offsets[2]);
					mColors[pCustomAvatarSettings.WARPAINTCOLOR_INDEX] = ColorFromOffset(avatarDataPart2.Offsets[3]);
				}
				if (avatarDataPart2.Textures.Length >= 1)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
				}
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.MASK, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.DETAILEYES, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
				if (avatarDataPart2.Textures.Length >= 4)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.EYEMASK, "RS_SHARED/" + avatarDataPart2.Textures[3]);
				}
				if (avatarDataPart2.Textures.Length >= 5)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.DECAL1, "RS_SHARED/" + avatarDataPart2.Textures[4]);
				}
				if (avatarDataPart2.Textures.Length >= 6)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.DECAL2, "RS_SHARED/" + avatarDataPart2.Textures[5]);
				}
			}
			else if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_HAIR)
			{
				if (avatarDataPart2.Textures.Length >= 1)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAIR + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
				}
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAIR + pCustomAvatarSettings.MASK, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAIR + pCustomAvatarSettings.HIGHLIGHT, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
			}
			else if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_TOP)
			{
				if (avatarDataPart2.Textures.Length >= 1)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_TOP + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
				}
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_TOP + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_TOP + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
			}
			else if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_LEGS)
			{
				if (avatarDataPart2.Textures.Length >= 1)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_LEGS + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
				}
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_LEGS + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_LEGS + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
			}
			else if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_FEET)
			{
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
				if (avatarDataPart2.Textures.Length >= 4)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[3]);
				}
				if (avatarDataPart2.Textures.Length >= 5)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[4]);
				}
				if (avatarDataPart2.Textures.Length >= 6)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[5]);
				}
			}
			else if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_HAND)
			{
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
				if (avatarDataPart2.Textures.Length >= 4)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[3]);
				}
				if (avatarDataPart2.Textures.Length >= 5)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[4]);
				}
				if (avatarDataPart2.Textures.Length >= 6)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[5]);
				}
			}
			else if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
			{
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				else if (avatarDataPart2.Textures.Length >= 1)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
				if (avatarDataPart2.Textures.Length >= 4)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[3]);
				}
				if (avatarDataPart2.Textures.Length >= 5)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[4]);
				}
				if (avatarDataPart2.Textures.Length >= 6)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[5]);
				}
			}
			else if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND)
			{
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				else if (avatarDataPart2.Textures.Length >= 1)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
				if (avatarDataPart2.Textures.Length >= 4)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[3]);
				}
				if (avatarDataPart2.Textures.Length >= 5)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[4]);
				}
				if (avatarDataPart2.Textures.Length >= 6)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[5]);
				}
			}
			else if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_HAT)
			{
				if (avatarDataPart2.Textures.Length >= 1)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAT + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
				}
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAT + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_HAT + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
			}
			else if (avatarDataPart2.PartType == AvatarData.pPartSettings.AVATAR_PART_FACEMASK)
			{
				if (avatarDataPart2.Textures.Length >= 1)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_FACEMASK + pCustomAvatarSettings.DETAIL, "RS_SHARED/" + avatarDataPart2.Textures[0]);
				}
				if (avatarDataPart2.Textures.Length >= 2)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_FACEMASK + pCustomAvatarSettings.COLOR_MASK, "RS_SHARED/" + avatarDataPart2.Textures[1]);
				}
				if (avatarDataPart2.Textures.Length >= 3)
				{
					mTextures.Add(AvatarData.pPartSettings.AVATAR_PART_FACEMASK + pCustomAvatarSettings.BUMP, "RS_SHARED/" + avatarDataPart2.Textures[2]);
				}
			}
			if (avatarDataPart2.UserInventoryId.HasValue && avatarDataPart2.UserInventoryId.Value > 0 && !mUiids.ContainsKey(avatarDataPart2.PartType))
			{
				mUiids.Add(avatarDataPart2.PartType, avatarDataPart2.UserInventoryId.Value);
			}
		}
		CacheState();
	}

	public void ToAvatarData(AvatarData.InstanceInfo info)
	{
		if (info != null)
		{
			info.SetVersion(AvatarData.VERSION);
			AvatarDataPart avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_FEET);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[2];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 3, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 4, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT + pCustomAvatarSettings.BUMP);
				SetPartTexture(avatarDataPart, 5, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT + pCustomAvatarSettings.BUMP);
				SetPartGeometry(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT);
				SetPartGeometry(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_HAND);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[2];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 3, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 4, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT + pCustomAvatarSettings.BUMP);
				SetPartTexture(avatarDataPart, 5, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT + pCustomAvatarSettings.BUMP);
				SetPartGeometry(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT);
				SetPartGeometry(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_LEGS);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[1];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_LEGS + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_LEGS + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_LEGS + pCustomAvatarSettings.BUMP);
				SetPartGeometry(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_LEGS);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_TOP);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[1];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_TOP + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_TOP + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_TOP + pCustomAvatarSettings.BUMP);
				SetPartGeometry(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_TOP);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_HAIR);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[3];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_HAIR + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_HAIR + pCustomAvatarSettings.MASK);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_HAIR + pCustomAvatarSettings.HIGHLIGHT);
				SetPartGeometry(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_HAIR);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_HEAD);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[6];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.MASK);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.DETAILEYES);
				SetPartTexture(avatarDataPart, 3, AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.EYEMASK);
				SetPartTexture(avatarDataPart, 4, AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.DECAL1);
				SetPartTexture(avatarDataPart, 5, AvatarData.pPartSettings.AVATAR_PART_HEAD + pCustomAvatarSettings.DECAL2);
				avatarDataPart.Offsets = new AvatarDataPartOffset[4];
				SetPartColor(avatarDataPart, pCustomAvatarSettings.SKINCOLOR_INDEX, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
				SetPartColor(avatarDataPart, pCustomAvatarSettings.HAIRCOLOR_INDEX, mColors[pCustomAvatarSettings.HAIRCOLOR_INDEX]);
				SetPartColor(avatarDataPart, pCustomAvatarSettings.EYECOLOR_INDEX, mColors[pCustomAvatarSettings.EYECOLOR_INDEX]);
				SetPartColor(avatarDataPart, pCustomAvatarSettings.WARPAINTCOLOR_INDEX, mColors[pCustomAvatarSettings.WARPAINTCOLOR_INDEX]);
				SetPartGeometry(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_HEAD);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_HAT);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[1];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_HAT + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_HAT + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_HAT + pCustomAvatarSettings.BUMP);
				SetPartGeometry(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_HAT);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_FACEMASK);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[1];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_FACEMASK + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_FACEMASK + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_FACEMASK + pCustomAvatarSettings.BUMP);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[2];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 3, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 4, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT + pCustomAvatarSettings.BUMP);
				SetPartTexture(avatarDataPart, 5, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT + pCustomAvatarSettings.BUMP);
				SetPartGeometry(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT);
				SetPartGeometry(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD);
			if (avatarDataPart != null)
			{
				avatarDataPart.Textures = new string[2];
				SetPartTexture(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT + pCustomAvatarSettings.DETAIL);
				SetPartTexture(avatarDataPart, 2, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 3, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT + pCustomAvatarSettings.COLOR_MASK);
				SetPartTexture(avatarDataPart, 4, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT + pCustomAvatarSettings.BUMP);
				SetPartTexture(avatarDataPart, 5, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT + pCustomAvatarSettings.BUMP);
				SetPartGeometry(avatarDataPart, 0, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT);
				SetPartGeometry(avatarDataPart, 1, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT);
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_BACK);
			if (avatarDataPart != null)
			{
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
			if (avatarDataPart != null)
			{
				SetPartUiid(avatarDataPart);
			}
			avatarDataPart = info.FindPart(AvatarData.pPartSettings.AVATAR_PART_WING);
			if (avatarDataPart != null)
			{
				SetPartUiid(avatarDataPart);
			}
		}
	}

	private void SetPartTexture(AvatarDataPart part, int index, string texName)
	{
		if (part == null || string.IsNullOrEmpty(texName) || !mTextures.ContainsKey(texName))
		{
			return;
		}
		string[] array = mTextures[texName].Split('/');
		if (array.Length <= 2)
		{
			return;
		}
		if (part.Textures == null)
		{
			part.Textures = new string[index + 1];
		}
		else if (index >= part.Textures.Length)
		{
			int num = part.Textures.Length;
			string[] array2 = part.Textures;
			Array.Resize(ref array2, index + 1);
			part.Textures = array2;
			for (int i = num; i < part.Textures.Length - 1; i++)
			{
				part.Textures[i] = "__EMPTY__";
			}
		}
		part.Textures[index] = array[1] + "/" + array[2];
	}

	private void SetPartGeometry(AvatarDataPart part, int index, string geometryName)
	{
		if (part != null && !string.IsNullOrEmpty(geometryName) && mGeometries.ContainsKey(geometryName))
		{
			string text = mGeometries[geometryName];
			string[] array = text.Split('/');
			if (array.Length > 2)
			{
				part.Geometries[index] = array[1] + "/" + array[2];
			}
			else if (string.CompareOrdinal(text, "NULL") == 0)
			{
				part.Geometries[index] = "NULL";
			}
		}
	}

	private void SetPartColor(AvatarDataPart part, int index, Color c)
	{
		if (part?.Offsets != null && part.Offsets.Length > index)
		{
			AvatarDataPartOffset avatarDataPartOffset = new AvatarDataPartOffset();
			avatarDataPartOffset.X = c.r;
			avatarDataPartOffset.Y = c.g;
			avatarDataPartOffset.Z = c.b;
			part.Offsets[index] = avatarDataPartOffset;
		}
	}

	private void SetPartUiid(AvatarDataPart part)
	{
		if (part != null && mUiids.ContainsKey(part.PartType))
		{
			part.UserInventoryId = mUiids[part.PartType];
		}
	}

	private Shader GetPartShader(string textureType)
	{
		if (textureType == null)
		{
			return null;
		}
		List<AvatarSettings.CustomAvatarSettings.PartShaderMap> partsShaderMap = AvatarSettings.pInstance._CustomAvatarSettings._PartsShaderMap;
		if (partsShaderMap == null || partsShaderMap.Count == 0)
		{
			return null;
		}
		foreach (AvatarSettings.CustomAvatarSettings.PartShaderMap item in partsShaderMap)
		{
			if (!string.IsNullOrEmpty(item._TextureType) && !string.IsNullOrEmpty(textureType) && textureType.Contains(item._TextureType))
			{
				return UtPlatform.IsMobile() ? item._MobileShader : item._Shader;
			}
		}
		return null;
	}

	private void ApplyShader(GameObject avatar, string part, string type)
	{
		string data = GetData(part, type);
		Shader partShader = GetPartShader(data);
		GameObject partObject = AvatarData.GetPartObject(avatar.transform, part, 0);
		if (partObject == null)
		{
			return;
		}
		Renderer renderer = AvatarData.FindRenderer(partObject.transform);
		if (renderer == null || renderer.materials.Length == 0)
		{
			return;
		}
		PartShaderSwapper partShaderSwapper = renderer.gameObject.GetComponent<PartShaderSwapper>();
		if (partShader != null)
		{
			if (!(partShader.name == renderer.material.shader.name))
			{
				if (partShaderSwapper == null)
				{
					partShaderSwapper = renderer.gameObject.AddComponent<PartShaderSwapper>();
				}
				partShaderSwapper.SwapShader(renderer.material, partShader);
			}
		}
		else if (partShaderSwapper != null)
		{
			partShaderSwapper.RestoreShader(renderer.material);
		}
	}

	public void UpdateShaders(GameObject avatar, AvatarData.InstanceInfo inInstance = null)
	{
		mCustomParts.Clear();
		Material partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_HAIR);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.HAIRCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAIR, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAIR, pCustomAvatarSettings.MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAIR, pCustomAvatarSettings.HIGHLIGHT);
		}
		else
		{
			UtDebug.LogError("*** UpdateShaders Fail ***");
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_HEAD);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			partMaterial.SetColor(pCustomAvatarSettings.EYE_COLOR, mColors[pCustomAvatarSettings.EYECOLOR_INDEX]);
			partMaterial.SetColor(pCustomAvatarSettings.WAR_COLOR, mColors[pCustomAvatarSettings.WARPAINTCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DETAILEYES);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.EYEMASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DECAL1);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DECAL2);
			string[] array = new string[2]
			{
				AvatarData.pGeneralSettings.BLINK_PLANE_MALE,
				AvatarData.pGeneralSettings.BLINK_PLANE_FEMALE
			};
			for (int i = 0; i < array.Length; i++)
			{
				Transform transform = UtUtilities.FindChildTransform(avatar, array[i]);
				if (!(transform != null))
				{
					continue;
				}
				Renderer component = transform.gameObject.GetComponent<MeshRenderer>();
				if (component != null && component.material != null)
				{
					component.material.SetColor(pCustomAvatarSettings.WAR_COLOR, mColors[pCustomAvatarSettings.WARPAINTCOLOR_INDEX]);
					SetTextureProperty(component.material, AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DETAIL);
					SetTextureProperty(component.material, AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DECAL1);
					SetTextureProperty(component.material, AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DECAL2);
					if (component.material.HasProperty(pCustomAvatarSettings.SKIN_COLOR))
					{
						component.material.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
					}
					else if (component.material.HasProperty(AvatarData.pShaderSettings.SHADER_PROP_COLOR))
					{
						component.material.SetColor(AvatarData.pShaderSettings.SHADER_PROP_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
					}
				}
			}
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_TOP, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_TOP);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_TOP, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_TOP, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_TOP, pCustomAvatarSettings.BUMP);
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_LEGS, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_LEGS);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_LEGS, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_LEGS, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_LEGS, pCustomAvatarSettings.BUMP);
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, pCustomAvatarSettings.BUMP);
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, pCustomAvatarSettings.BUMP);
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, pCustomAvatarSettings.BUMP);
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, pCustomAvatarSettings.BUMP);
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, pCustomAvatarSettings.DETAIL);
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, pCustomAvatarSettings.BUMP);
		}
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, pCustomAvatarSettings.BUMP);
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, pCustomAvatarSettings.DETAIL);
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, pCustomAvatarSettings.BUMP);
		}
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, pCustomAvatarSettings.BUMP);
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_HAT, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_HAT);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAT, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAT, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_HAT, pCustomAvatarSettings.BUMP);
		}
		ApplyShader(avatar, AvatarData.pPartSettings.AVATAR_PART_FACEMASK, pCustomAvatarSettings.DETAIL);
		partMaterial = GetPartMaterial(avatar, AvatarData.pPartSettings.AVATAR_PART_FACEMASK);
		if ((bool)partMaterial)
		{
			partMaterial.SetColor(pCustomAvatarSettings.SKIN_COLOR, mColors[pCustomAvatarSettings.SKINCOLOR_INDEX]);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_FACEMASK, pCustomAvatarSettings.DETAIL);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_FACEMASK, pCustomAvatarSettings.COLOR_MASK);
			SetTextureProperty(partMaterial, AvatarData.pPartSettings.AVATAR_PART_FACEMASK, pCustomAvatarSettings.BUMP);
		}
		if (AvatarSettings.pInstance == null || !AvatarSettings.pInstance._ItemCustomizationAvatarExcludeList.Contains(avatar.name))
		{
			UpdateMultiPartShader(avatar, AvatarData.pPartSettings.AVATAR_PART_WING, inInstance);
			UpdatePartShader(avatar, AvatarData.pPartSettings.AVATAR_PART_BACK, AvatarData.pPartSettings.AVATAR_PART_BACK, inInstance);
		}
	}

	public void UpdateMultiPartShader(GameObject avatar, string partName, AvatarData.InstanceInfo inInstance)
	{
		AvatarData avatarData = ((inInstance != null && inInstance.mInstance != null) ? inInstance.mInstance : AvatarData.pInstance);
		if (avatarData == null)
		{
			return;
		}
		for (int i = 0; i < avatarData.Part.Length; i++)
		{
			AvatarDataPart avatarDataPart = avatarData.Part[i];
			if (avatarDataPart.Textures == null || avatarDataPart.IsDefault())
			{
				continue;
			}
			string[] textures = avatarDataPart.Textures;
			foreach (string text in textures)
			{
				if (!string.IsNullOrEmpty(text) && AvatarData.CustomizeTextureType(text))
				{
					UpdatePartShader(avatar, AvatarData.pPartSettings.AVATAR_PART_WING, avatarDataPart.PartType, inInstance);
					break;
				}
			}
		}
	}

	public void UpdatePartShader(GameObject avatar, string groupPartName, string partName, AvatarData.InstanceInfo inInstance = null)
	{
		Material partMaterial = GetPartMaterial(avatar, partName);
		if (!partMaterial)
		{
			return;
		}
		Renderer renderer = null;
		GameObject partObject = AvatarData.GetPartObject(avatar.transform, partName, 0);
		if (partObject != null)
		{
			Renderer renderer2 = AvatarData.FindRenderer(partObject.transform);
			if (renderer2 != null && renderer2.materials.Length != 0)
			{
				renderer = renderer2;
			}
		}
		if (renderer == null)
		{
			return;
		}
		AvatarDataPart avatarDataPart = null;
		avatarDataPart = ((inInstance == null) ? AvatarData.GetPart(groupPartName) : inInstance.FindPart(groupPartName));
		if (avatarDataPart == null)
		{
			return;
		}
		ItemCustomizationData[] customizationConfig = ItemCustomizationSettings.pInstance.GetCustomizationConfig(AvatarData.GetCategoryID(groupPartName));
		if (customizationConfig == null)
		{
			return;
		}
		List<AvatarPartAttribute> list = ((avatarDataPart.Attributes != null) ? new List<AvatarPartAttribute>(avatarDataPart.Attributes) : new List<AvatarPartAttribute>());
		foreach (ItemCustomizationData data in customizationConfig)
		{
			if (!partMaterial.HasProperty(data._ShaderPropName))
			{
				continue;
			}
			AvatarPartAttribute avatarPartAttribute = list.Find((AvatarPartAttribute a) => a.Key == data._ShaderPropName);
			switch (data._Type)
			{
			case CustomizePropertyType.COLOR:
			{
				Color color = Color.white;
				if (data._UseGroupLogo)
				{
					Group group2 = null;
					if (AvAvatar.pObject == avatar)
					{
						group2 = AvatarData.pInstance._Group;
					}
					else if (inInstance != null && inInstance.mInstance != null)
					{
						group2 = inInstance.mInstance._Group;
					}
					if (group2 != null)
					{
						if (data._LogoColorSource == LogoColorSource.BACKGROUND_COLOR)
						{
							group2.GetBGColor(out color);
						}
						else
						{
							group2.GetFGColor(out color);
						}
						partMaterial.SetColor(data._ShaderPropName, color);
					}
				}
				else if (avatarPartAttribute != null)
				{
					string[] array2 = avatarPartAttribute.Value.Split('/');
					if (array2.Length == 2)
					{
						HexUtil.HexToColor(array2[1], out color);
						partMaterial.SetColor(data._ShaderPropName, color);
					}
					else if (data._ShaderPropName == "_PrimaryColor")
					{
						HexUtil.HexToColor(array2[0], out color);
						partMaterial.SetColor(data._ShaderPropName, color);
					}
				}
				break;
			}
			case CustomizePropertyType.TEXTURE:
				if (data._UseGroupLogo)
				{
					Group group = null;
					if (AvAvatar.pObject == avatar)
					{
						group = AvatarData.pInstance._Group;
					}
					else if (inInstance != null && inInstance.mInstance != null)
					{
						group = inInstance.mInstance._Group;
					}
					if (group != null)
					{
						SetTextureData(partName, data._ShaderPropName, group.Logo);
						SetTextureProperty(partMaterial, partName, data._ShaderPropName);
					}
				}
				else if (avatarPartAttribute != null)
				{
					string[] array = avatarPartAttribute.Value.Split('/');
					if (array.Length == 2)
					{
						SetTextureData(partName, data._ShaderPropName, array[1]);
						SetTextureProperty(partMaterial, partName, data._ShaderPropName);
					}
				}
				break;
			}
		}
	}

	private Color ColorFromOffset(AvatarDataPartOffset v)
	{
		Color result = default(Color);
		result.r = v.X;
		result.g = v.Y;
		result.b = v.Z;
		result.a = 1f;
		return result;
	}

	private Material GetPartMaterial(GameObject obj, string inType)
	{
		GameObject partObject = AvatarData.GetPartObject(obj.transform, inType, 0);
		if (partObject == null)
		{
			return null;
		}
		Renderer renderer = AvatarData.FindRenderer(partObject.transform);
		if (renderer != null && renderer.materials.Length != 0)
		{
			return renderer.materials[0];
		}
		return null;
	}

	private string GetActualPropertyName(Material material, string propName)
	{
		if (pCustomAvatarSettings._ShaderPropertyOverride == null || pCustomAvatarSettings._ShaderPropertyOverride.Count <= 0)
		{
			return propName;
		}
		AvatarSettings.CustomAvatarSettings.ShaderPropertyOverride shaderPropertyOverride = pCustomAvatarSettings._ShaderPropertyOverride.Find((AvatarSettings.CustomAvatarSettings.ShaderPropertyOverride p) => p._Shader == material.shader.name && p._Property == propName);
		if (shaderPropertyOverride == null)
		{
			return propName;
		}
		return shaderPropertyOverride._Override;
	}

	private void SetTextureProperty(Material inMat, string texName, string propName)
	{
		if (!mTextures.ContainsKey(texName + propName))
		{
			return;
		}
		string text = mTextures[texName + propName];
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		string[] array = text.Split('/');
		if (array.Length > 2)
		{
			CustomInfo customInfo = new CustomInfo(text, inMat, GetActualPropertyName(inMat, propName));
			mCustomParts.Add(customInfo);
			object obj = RsResourceManager.LoadAssetFromBundle(text);
			if (obj == null)
			{
				mLoadedTextures.Add(text);
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], customInfo.OnTextureLoaded, typeof(Texture2D));
			}
			else
			{
				RsResourceManager.Unload(text);
				customInfo.OnTextureLoaded(texName, RsResourceLoadEvent.COMPLETE, 0f, obj, null);
			}
		}
	}

	public void RemoveAllTextureProperty()
	{
		for (int i = 0; i < mLoadedTextures.Count; i++)
		{
			string text = mLoadedTextures[i];
			if (!string.IsNullOrEmpty(text))
			{
				RsResourceManager.Unload(text);
			}
		}
		mLoadedTextures.Clear();
	}

	public virtual void ApplyItem(ItemData inItem, string partName)
	{
		if (partName == AvatarData.pPartSettings.AVATAR_PART_SKIN)
		{
			SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SKIN, pCustomAvatarSettings.SKIN_COLOR, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_SKIN));
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_SCAR)
		{
			SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DECAL1, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_FACE_DECAL)
		{
			SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DECAL2, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND)
		{
			SetData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, inItem);
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			SetData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, inItem);
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_FEET)
		{
			SetData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, inItem);
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_HAND)
		{
			SetData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, inItem);
			return;
		}
		if (!string.IsNullOrEmpty(inItem.AssetName))
		{
			SetGeometryData(partName, inItem.AssetName);
		}
		if (partName != AvatarData.pPartSettings.AVATAR_PART_HEAD && partName != AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			SetTextureData(partName, pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.DETAILEYES, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_EYES_OPEN));
			SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, pCustomAvatarSettings.EYEMASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_HAIR)
		{
			SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, pCustomAvatarSettings.MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, pCustomAvatarSettings.HIGHLIGHT, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_HIGHLIGHT));
		}
	}

	public List<int> GetPartUiids()
	{
		List<int> result = null;
		if (mUiids == null || mUiids.Count <= 0)
		{
			return result;
		}
		result = new List<int>();
		foreach (KeyValuePair<string, int> mUiid in mUiids)
		{
			result.Add(mUiid.Value);
		}
		return result;
	}
}
