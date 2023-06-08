using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using KA.Framework;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "AvatarData", Namespace = "")]
public class AvatarData
{
	public class InstanceInfo
	{
		public GameObject mAvatar;

		public AvatarData mInstance;

		public RsAssetLoader mBundleLoader;

		public RsTextureLoader mTextureLoader;

		public bool mLoading;

		public bool mLoadingCountry;

		public bool mLoadingMood;

		public int mSaveCount;

		public bool mBundlesReady;

		public bool mTexturesReady;

		public bool mInitializedFromPreviousSave;

		public bool mMergeWithDefault;

		public bool _CurrentTail;

		public string mMoodTextureURL = string.Empty;

		public string mCountryTextureURL = string.Empty;

		private CustomAvatarState mCustomAvatarState;

		private int mNumItemToBeLoaded;

		private bool mSave;

		private int mParentItemID;

		private List<string> mAssetsToDownload = new List<string>();

		public bool pIsReady
		{
			get
			{
				if (mBundlesReady && !mLoading && !mLoadingCountry)
				{
					return !mLoadingMood;
				}
				return false;
			}
		}

		public bool pIsSaving => mSaveCount != 0;

		public AvatarDataPart[] GetClonedParts()
		{
			List<AvatarDataPart> list = new List<AvatarDataPart>();
			AvatarDataPart[] part = mInstance.Part;
			foreach (AvatarDataPart avatarDataPart in part)
			{
				AvatarDataPart avatarDataPart2 = new AvatarDataPart();
				avatarDataPart2.PartType = avatarDataPart.PartType;
				avatarDataPart2.Offsets = avatarDataPart.Offsets;
				avatarDataPart2.Geometries = avatarDataPart.Geometries;
				avatarDataPart2.Textures = avatarDataPart.Textures;
				avatarDataPart2.Attributes = avatarDataPart.Attributes;
				avatarDataPart2.UserInventoryId = avatarDataPart.UserInventoryId;
				list.Add(avatarDataPart2);
			}
			return list.ToArray();
		}

		public AvatarDataPart FindPart(string inType)
		{
			if (mInstance == null || mInstance.Part == null)
			{
				return null;
			}
			for (int i = 0; i < mInstance.Part.Length; i++)
			{
				AvatarDataPart avatarDataPart = mInstance.Part[i];
				if (avatarDataPart != null && avatarDataPart.PartType.Equals(inType, StringComparison.OrdinalIgnoreCase))
				{
					return avatarDataPart;
				}
			}
			return null;
		}

		public bool IsDefaultSaved()
		{
			return IsDefaultSaved(mInstance);
		}

		public static bool IsDefaultSaved(AvatarData data)
		{
			if (data?.Part == null)
			{
				return false;
			}
			for (int i = 0; i < data.Part.Length; i++)
			{
				AvatarDataPart avatarDataPart = data.Part[i];
				if (avatarDataPart != null && avatarDataPart.IsDefault())
				{
					return true;
				}
			}
			return false;
		}

		public bool IsDefaultSaved(string ptype)
		{
			return FindPart("DEFAULT_" + ptype)?.IsDefault() ?? false;
		}

		public bool GetPartVisibility(string avatarName, string partType)
		{
			if (!string.IsNullOrEmpty(avatarName) && AvatarSettings.pInstance._PartVisibilityAvatarExcludeList.Contains(avatarName))
			{
				return true;
			}
			AvatarDataPart avatarDataPart = FindPart("Hidden");
			if (avatarDataPart == null)
			{
				return true;
			}
			AvatarPartAttribute avatarPartAttribute = Array.Find(avatarDataPart.Attributes, (AvatarPartAttribute a) => a.Key == partType);
			if (avatarPartAttribute != null)
			{
				return bool.Parse(avatarPartAttribute.Value);
			}
			return true;
		}

		public void UpdatePartVisibility(string partType, bool visible)
		{
			AvatarDataPart part = GetPart("Hidden");
			if (part == null)
			{
				return;
			}
			AvatarPartAttribute avatarPartAttribute = new AvatarPartAttribute
			{
				Key = partType,
				Value = visible.ToString()
			};
			List<AvatarPartAttribute> list = new List<AvatarPartAttribute>();
			if (part.Attributes != null)
			{
				list.AddRange(part.Attributes);
			}
			if (list != null && list.Count > 0)
			{
				AvatarPartAttribute avatarPartAttribute2 = list.Find((AvatarPartAttribute p) => p.Key == partType);
				if (avatarPartAttribute2 != null)
				{
					avatarPartAttribute2.Value = avatarPartAttribute.Value;
				}
				else
				{
					list.Add(avatarPartAttribute);
				}
			}
			else
			{
				list.Add(avatarPartAttribute);
			}
			part.Attributes = list.ToArray();
		}

		public void RemovePartData(string inType)
		{
			if (mInstance == null || mInstance.Part == null)
			{
				return;
			}
			for (int i = 0; i < mInstance.Part.Length; i++)
			{
				if (mInstance.Part[i] != null && mInstance.Part[i].PartType.Equals(inType, StringComparison.OrdinalIgnoreCase))
				{
					mInstance.Part[i].PartType = "";
					mInstance.Part = Array.FindAll(mInstance.Part, (AvatarDataPart x) => x.PartType != "");
					break;
				}
			}
		}

		public void RestorePartData()
		{
			if (mInstance?.Part == null)
			{
				return;
			}
			List<AvatarDataPart> list = new List<AvatarDataPart>();
			for (int i = 0; i < mInstance.Part.Length; i++)
			{
				AvatarDataPart avatarDataPart = mInstance.Part[i];
				if (avatarDataPart.IsDefault())
				{
					string[] array = avatarDataPart.PartType.Split('_');
					AvatarDataPart avatarDataPart2 = FindPart(array[1]);
					if (avatarDataPart2 != null)
					{
						avatarDataPart2.PartType = "";
					}
					avatarDataPart.PartType = array[1];
				}
			}
			for (int i = 0; i < mInstance.Part.Length; i++)
			{
				AvatarDataPart avatarDataPart3 = mInstance.Part[i];
				if (avatarDataPart3.PartType.Length > 0 && !avatarDataPart3.IsPlaceHolder())
				{
					list.Add(avatarDataPart3);
				}
			}
			mInstance.Part = list.ToArray();
		}

		public void RestoreDefault()
		{
			if (mInstance != null && mInstance.Part != null)
			{
				RestorePartData();
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_LEGS, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_FEET, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_TOP, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_HEAD, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_HAIR, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_HAT, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_WRISTBAND, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_HAND, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_SHOULDERPAD, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_FACEMASK, ignorePartCheck: true);
				RestoreCurrentPartCheckBundle(this, pPartSettings.AVATAR_PART_BACK, ignorePartCheck: true);
			}
		}

		public void OnPartLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
		{
			string itemPartType = GetItemPartType(dataItem);
			mBundlesReady = true;
			string[] array = null;
			if (!dataItem.AssetName.Equals("NULL", StringComparison.OrdinalIgnoreCase))
			{
				array = dataItem.AssetName.Split('/');
				SetGeometrySaveDefault(this, itemPartType, array[1] + "/" + array[2], 0);
			}
			if (dataItem.Geometry2 != null && dataItem.Geometry2.Length > 0)
			{
				array = dataItem.Geometry2.Split('/');
				SetGeometrySaveDefault(this, itemPartType, array[1] + "/" + array[2], 1);
			}
			string textureNameNoPath = dataItem.GetTextureNameNoPath(pTextureSettings.TEXTURE_TYPE_STYLE);
			if (textureNameNoPath.Length > 0)
			{
				SetStyleTexture(this, itemPartType, textureNameNoPath, 0);
			}
			else
			{
				SetStyleTexture(this, itemPartType, "NULL", 0);
			}
			FindPart(itemPartType).AddBundles(mAssetsToDownload);
			mBundlesReady = false;
			mNumItemToBeLoaded--;
			if (mNumItemToBeLoaded == 0)
			{
				if (mSave)
				{
					mSave = false;
					mBundlesReady = true;
					Save();
					mBundlesReady = false;
				}
				if (mAssetsToDownload.Count > 0)
				{
					mBundleLoader = new RsAssetLoader();
					mBundleLoader.Load(mAssetsToDownload.ToArray(), "RS_SHARED", BundleLoadEventHandler, inDontDestroy: true);
				}
				else
				{
					mBundlesReady = true;
				}
			}
		}

		public void SetPartsByItemIDs(int[] ids, bool save)
		{
			mSave = save;
			mBundlesReady = false;
			mAssetsToDownload.Clear();
			mNumItemToBeLoaded = ids.Length;
			for (int i = 0; i < ids.Length; i++)
			{
				ItemData.Load(ids[i], OnPartLoadItemDataReady, null);
			}
		}

		public void OnItemDataReady(int itemID, ItemData dataItem, object inUserData)
		{
			string itemPartType = GetItemPartType(dataItem);
			mBundlesReady = true;
			if (!string.IsNullOrEmpty(dataItem.AssetName))
			{
				if (dataItem.AssetName.Equals("NULL", StringComparison.OrdinalIgnoreCase))
				{
					SetGeometrySaveDefault(this, itemPartType, "NULL", 0);
				}
				else
				{
					string[] array = dataItem.AssetName.Split('/');
					SetGeometrySaveDefault(this, itemPartType, array[1] + "/" + array[2], 0);
				}
			}
			if (!string.IsNullOrEmpty(dataItem.Geometry2))
			{
				if (dataItem.Geometry2.Equals("NULL", StringComparison.OrdinalIgnoreCase))
				{
					SetGeometrySaveDefault(this, itemPartType, "NULL", 1);
				}
				else
				{
					string[] array2 = dataItem.Geometry2.Split('/');
					SetGeometrySaveDefault(this, itemPartType, array2[1] + "/" + array2[2], 1);
				}
			}
			SetAttributes(pInstanceInfo, itemPartType, dataItem.Attribute);
			string text = null;
			text = dataItem.GetTextureNameNoPath(pTextureSettings.TEXTURE_TYPE_STYLE);
			if (text.Length > 0)
			{
				SetStyleTexture(this, itemPartType, text, 0);
			}
			else
			{
				SetStyleTexture(this, itemPartType, "NULL", 0);
			}
			FindPart(itemPartType).AddBundles(mAssetsToDownload);
			mBundlesReady = false;
			if (itemID != mParentItemID)
			{
				mNumItemToBeLoaded--;
			}
			else
			{
				mNumItemToBeLoaded = 0;
				if (dataItem.Relationship != null && dataItem.Relationship.Length != 0)
				{
					ItemDataRelationship[] array3 = Array.FindAll(dataItem.Relationship, (ItemDataRelationship r) => r.Type.Equals("GroupParent"));
					mNumItemToBeLoaded = array3.Length;
					if (mNumItemToBeLoaded > 0)
					{
						for (int i = 0; i < array3.Length; i++)
						{
							ItemData.Load(array3[i].ItemId, OnItemDataReady, null);
						}
						return;
					}
				}
			}
			if (mNumItemToBeLoaded == 0 && !mBundlesReady)
			{
				if (mSave)
				{
					mSave = false;
					mBundlesReady = true;
					Save();
					mBundlesReady = false;
				}
				if (mAssetsToDownload.Count > 0)
				{
					mBundleLoader = new RsAssetLoader();
					mBundleLoader.Load(mAssetsToDownload.ToArray(), "RS_SHARED", BundleLoadEventHandler, inDontDestroy: true);
				}
				else
				{
					mBundlesReady = true;
				}
			}
		}

		public void SetGroupPart(int itemID, bool save)
		{
			if (pIsReady)
			{
				RestoreDefault();
				mSave = save;
				mBundlesReady = false;
				mAssetsToDownload.Clear();
				mParentItemID = itemID;
				ItemData.Load(itemID, OnItemDataReady, null);
			}
		}

		public bool CheckVersion(bool majorOnly = true)
		{
			if (mInstance == null)
			{
				return false;
			}
			AvatarDataPart avatarDataPart = FindPart("Version");
			if (avatarDataPart?.Offsets == null || avatarDataPart.Offsets.Length == 0 || avatarDataPart.Offsets[0] == null)
			{
				return false;
			}
			if (avatarDataPart.Offsets[0].X == (float)(int)VERSION.x)
			{
				if (!majorOnly)
				{
					if (avatarDataPart.Offsets[0].Y == (float)(int)VERSION.y)
					{
						return avatarDataPart.Offsets[0].Z == (float)(int)VERSION.z;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public void SetVersion(Vector3 inVersion)
		{
			if (mInstance != null)
			{
				FindPart("Version")?.SetVersion(inVersion);
			}
		}

		public Vector3 GetVersion()
		{
			if (mInstance == null)
			{
				return Vector3.zero;
			}
			return FindPart("Version")?.GetVersion() ?? Vector3.zero;
		}

		public string PartType(ItemData item)
		{
			for (int i = 0; i < mInstance.Part.Length; i++)
			{
				string text = mInstance.Part[i].PartType;
				if (!item.HasCategory(228))
				{
					text = text.Replace("DEFAULT_", "");
				}
				if (item.HasCategory(GetCategoryID(text)))
				{
					return text;
				}
			}
			return null;
		}

		public void UpdatePartInventoryId(string inPartTypeName, UserItemData inUserItemData)
		{
			if (string.IsNullOrEmpty(inPartTypeName))
			{
				return;
			}
			for (int i = 0; i < pInstance.Part.Length; i++)
			{
				if (inPartTypeName.Equals(pInstance.Part[i].PartType))
				{
					pInstance.Part[i].UserInventoryId = inUserItemData?.UserInventoryID ?? (-1);
					break;
				}
			}
		}

		public void UpdatePartsInventoryIds()
		{
			if (!CommonInventoryData.pIsReady)
			{
				return;
			}
			for (int i = 0; i < mInstance.Part.Length; i++)
			{
				if (mInstance.Part[i] != null && mInstance.Part[i].Textures != null && mInstance.Part[i].Textures.Length != 0)
				{
					int categoryID = GetCategoryID(mInstance.Part[i].PartType);
					if (mInstance.Part[i].UserInventoryId.HasValue)
					{
						break;
					}
					UserItemData userItemDataFromGeometryAndTexture = CommonInventoryData.pInstance.GetUserItemDataFromGeometryAndTexture(mInstance.Part[i].Geometries, mInstance.Part[i].Textures, categoryID);
					if (userItemDataFromGeometryAndTexture != null)
					{
						mInstance.Part[i].UserInventoryId = userItemDataFromGeometryAndTexture.UserInventoryID;
					}
					else
					{
						mInstance.Part[i].UserInventoryId = -1;
					}
				}
			}
		}

		public int GetPartInventoryID(string partName)
		{
			AvatarDataPart avatarDataPart = FindPart(partName);
			if (avatarDataPart != null && avatarDataPart.UserInventoryId.HasValue)
			{
				return avatarDataPart.UserInventoryId.Value;
			}
			return -1;
		}

		public List<int> GetPartsInventoryIds()
		{
			return GetPartsInventoryIds(mInstance.Part);
		}

		public List<int> GetPartsInventoryIds(AvatarDataPart[] inPartsData)
		{
			if (inPartsData == null)
			{
				return null;
			}
			List<int> list = new List<int>();
			for (int i = 0; i < inPartsData.Length; i++)
			{
				if (inPartsData[i].UserInventoryId.HasValue && inPartsData[i].UserInventoryId.Value > 0)
				{
					list.Add(inPartsData[i].UserInventoryId.Value);
				}
			}
			return list;
		}

		public ItemStat[] GetPartsCombinedStats()
		{
			return GetPartsCombinedStats(mInstance.Part);
		}

		public ItemStat[] GetPartsCombinedStats(AvatarDataPart[] inPartsData)
		{
			List<int> partsInventoryIds = GetPartsInventoryIds(inPartsData);
			return GetPartsCombinedStats(partsInventoryIds);
		}

		public ItemStat[] GetPartsCombinedStats(List<int> partsInventory)
		{
			if (partsInventory == null)
			{
				return null;
			}
			List<ItemStat> list = new List<ItemStat>();
			for (int j = 0; j < partsInventory.Count; j++)
			{
				ItemStat[] partStats = CommonInventoryData.pInstance.GetItemStatsByUserInventoryID(partsInventory[j]);
				if (partStats == null || partStats.Length == 0)
				{
					continue;
				}
				int i;
				for (i = 0; i < partStats.Length; i++)
				{
					ItemStat itemStat = list.Find((ItemStat x) => x.ItemStatID == partStats[i].ItemStatID);
					if (itemStat != null)
					{
						int result = 0;
						int result2 = 0;
						int.TryParse(partStats[i].Value, out result);
						int.TryParse(itemStat.Value, out result2);
						itemStat.Value = (result2 + result).ToString();
					}
					else
					{
						list.Add(new ItemStat(partStats[i]));
					}
				}
			}
			return list.ToArray();
		}

		public void DumpData()
		{
			Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> Begin Dump <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
			if (mInstance?.Part != null)
			{
				AvatarDataPart[] part = mInstance.Part;
				foreach (AvatarDataPart avatarDataPart in part)
				{
					if (avatarDataPart == null)
					{
						continue;
					}
					if (avatarDataPart.Geometries != null)
					{
						string[] geometries = avatarDataPart.Geometries;
						foreach (string text in geometries)
						{
							if (text != null)
							{
								Debug.Log("================>>>>> PART GEO: " + text);
							}
						}
					}
					if (avatarDataPart.Textures != null)
					{
						string[] geometries = avatarDataPart.Textures;
						foreach (string text2 in geometries)
						{
							if (text2 != null)
							{
								Debug.Log("================>>>>> PART TEX: " + text2);
							}
						}
					}
					if (avatarDataPart.Offsets == null)
					{
						continue;
					}
					AvatarDataPartOffset[] offsets = avatarDataPart.Offsets;
					foreach (AvatarDataPartOffset avatarDataPartOffset in offsets)
					{
						if (avatarDataPartOffset != null)
						{
							Debug.Log("================>>>>> PART OFF: " + avatarDataPartOffset.X + "," + avatarDataPartOffset.Y);
						}
					}
				}
			}
			Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> End Dump <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
		}

		public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
		{
			switch (inType)
			{
			case WsServiceType.GET_AVATAR:
			case WsServiceType.GET_AVATAR_BY_USER_ID:
				switch (inEvent)
				{
				case WsServiceEvent.COMPLETE:
					mInstance = (AvatarData)inObject;
					if (mInstance == null)
					{
						mInstance = CreateDefault();
						mInitializedFromPreviousSave = false;
					}
					else
					{
						ValidateDefaultParts(mInstance);
						mInitializedFromPreviousSave = true;
					}
					if (inType == WsServiceType.GET_AVATAR && UserInfo.pInstance != null)
					{
						DataCache.Set(UserInfo.pInstance.UserID + "_AvatarData", mInstance);
					}
					OnLoadBundlesAndUpdateAvatar();
					break;
				case WsServiceEvent.ERROR:
					UtDebug.LogError("WEB SERVICE CALL GetAvatarData FAILED!!!");
					mInstance = CreateDefault();
					OnLoadBundlesAndUpdateAvatar();
					break;
				}
				break;
			case WsServiceType.SET_AVATAR:
				if (inEvent == WsServiceEvent.COMPLETE || inEvent == WsServiceEvent.ERROR)
				{
					mSaveCount--;
				}
				if (inEvent == WsServiceEvent.COMPLETE)
				{
					MainStreetMMOPlugin.UpdateAvatar();
				}
				break;
			}
		}

		public void LoadRank(string inURL)
		{
		}

		private void RankImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject)
		{
			if (inEvent == RsResourceLoadEvent.COMPLETE)
			{
				Texture inTexture = (Texture)inObject;
				if (mAvatar == AvAvatar.pObject)
				{
					AddRankToDisplayName(AvAvatar.pObject, inTexture, pDisplayYourName);
				}
				else
				{
					AddRankToDisplayName(AvAvatar.pObject, inTexture, pDisplayOtherName);
				}
			}
		}

		public void LoadCountry(string inURL)
		{
		}

		private void CountryImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject)
		{
			mCountryTextureURL = inURL;
			int num = inURL.LastIndexOf('/') + 1;
			string flagName = inURL.Substring(num, inURL.LastIndexOf(".") - num);
			if (mAvatar == AvAvatar.pObject)
			{
				AddCountryToDisplayName(mAvatar, null, pDisplayYourName, flagName);
			}
			else
			{
				AddCountryToDisplayName(mAvatar, null, pDisplayOtherName, flagName);
			}
		}

		public void LoadMood(string inURL)
		{
			if (!string.IsNullOrEmpty(inURL))
			{
				MoodImageLoadingEvent(inURL, RsResourceLoadEvent.COMPLETE, 0f, null);
			}
		}

		private void MoodImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject)
		{
			int num = inURL.LastIndexOf('/') + 1;
			string moodName = inURL.Substring(num, inURL.LastIndexOf(".") - num);
			if (mAvatar == AvAvatar.pObject)
			{
				AddMoodToDisplayName(mAvatar, null, pDisplayYourName, moodName);
			}
			else
			{
				AddMoodToDisplayName(mAvatar, null, pDisplayOtherName, moodName);
			}
		}

		public void LoadBundlesAndUpdateAvatar()
		{
			UtDebug.Assert(mAvatar != null, "LoadBundlesAndUpdateAvatar -- AVATAR REFERENCe iS NULL!");
			if (!mLoading)
			{
				mLoading = true;
				mBundlesReady = false;
				mTexturesReady = false;
				OnLoadBundlesAndUpdateAvatar();
			}
			else
			{
				UtDebug.LogError("ERROR: ATTEMPTING TO START A LOAD ON AN INSTANCE THAT IS CURRENTLY LOADING!");
			}
		}

		public void UnloadBundleData()
		{
			if (mBundleLoader != null && mAssetsToDownload != null)
			{
				mBundleLoader.Unload(mAssetsToDownload.ToArray(), "RS_SHARED");
				mAssetsToDownload.Clear();
			}
			mCustomAvatarState?.RemoveAllTextureProperty();
		}

		public void RemovePart()
		{
			AvatarData avatarData = CreateDefault();
			for (int i = 0; i < avatarData.Part.Length; i++)
			{
				AvatarDataPart avatarDataPart = avatarData.Part[i];
				bool flag = false;
				for (int j = 0; j < mInstance.Part.Length; j++)
				{
					AvatarDataPart avatarDataPart2 = mInstance.Part[j];
					if (avatarDataPart.PartType == avatarDataPart2.PartType)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					if (avatarDataPart.PartType == pPartSettings.AVATAR_PART_HAT)
					{
						RemoveCurrentPart(this, pPartSettings.AVATAR_PART_HAT);
						CheckHelmetHair();
					}
					else if (avatarDataPart.PartType == pPartSettings.AVATAR_PART_SHOULDERPAD)
					{
						RemoveCurrentPart(this, pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, 0);
						RemoveCurrentPart(this, pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, 1);
					}
					else if (avatarDataPart.PartType == pPartSettings.AVATAR_PART_FACEMASK)
					{
						RemoveCurrentPart(this, pPartSettings.AVATAR_PART_FACEMASK);
					}
					else if (avatarDataPart.PartType == pPartSettings.AVATAR_PART_HAND_PROP_RIGHT)
					{
						RemoveCurrentPart(this, pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
					}
					else if (avatarDataPart.PartType == pPartSettings.AVATAR_PART_BACK)
					{
						RemoveCurrentPart(this, pPartSettings.AVATAR_PART_BACK);
					}
				}
			}
		}

		internal void OnLoadBundlesAndUpdateAvatar()
		{
			UtDebug.Assert(mLoading, "OnLoadBundlesAndUpdateAvatar -- LOADING STATe iS FALSE!");
			UtDebug.Assert(!mBundlesReady, "OnLoadBundlesAndUpdateAvatar -- BUNDLES ARE FLAGGED AS READY BEFORE BEING CHECKED!");
			if (AvAvatar.pObject == null)
			{
				AvAvatar.SpawnAvatar();
				mInstanceInfo.mAvatar = AvAvatar.pObject;
			}
			if (mMergeWithDefault)
			{
				if (mInstance == null)
				{
					mInstance = CreateDefault();
				}
				else
				{
					Merge(mInstance, CreateDefault());
				}
			}
			if (mInstance != null)
			{
				if (!CheckVersion())
				{
					MakeDefault(mInstance);
				}
				AddBundles(mInstance, mAssetsToDownload);
				mTexturesReady = false;
				if (mAssetsToDownload.Count > 0)
				{
					mBundleLoader = new RsAssetLoader();
					mBundleLoader.Load(mAssetsToDownload.ToArray(), "RS_SHARED", BundleLoadEventHandler, inDontDestroy: true);
					return;
				}
				UtDebug.Log("WEB SERVICE CALL GetAvatarData SHOWS NO DOWNLOAD!!");
				mBundlesReady = true;
				if (mTexturesReady)
				{
					mLoading = false;
					UpdateAvatar();
				}
			}
			else
			{
				UtDebug.Log("WEB SERVICE CALL GetAvatarData RETURNED NO DATA!!!");
				mInstance = new AvatarData(VERSION, Vector3.one);
				mLoading = false;
				mBundlesReady = true;
			}
		}

		internal void BundleLoadEventHandler(RsAssetLoader inLoader, RsResourceLoadEvent inEvent, float inProgress, object inUserData)
		{
			UtDebug.Assert(inLoader == mBundleLoader);
			if (inEvent == RsResourceLoadEvent.PROGRESS || (uint)(inEvent - 2) > 1u)
			{
				return;
			}
			mBundlesReady = true;
			mTexturesReady = true;
			if (mTexturesReady)
			{
				mLoading = false;
				UpdateAvatar();
				if (mInstanceInfo == this)
				{
					SetDontDestroyOnBundles(inDontDestroy: true);
				}
			}
		}

		public GameObject DisconnectParent(string inType, int index)
		{
			GameObject partObject = GetPartObject(mAvatar.transform, inType, index);
			if (partObject != null)
			{
				partObject.transform.parent = null;
			}
			return partObject;
		}

		public void ConnectParent(GameObject obj, string inType, int index)
		{
			if (obj != null)
			{
				string parentBone = GetParentBone(inType, index);
				Transform parent = mAvatar.transform;
				if (parentBone != "")
				{
					parent = mAvatar.transform.Find(parentBone);
				}
				obj.transform.parent = parent;
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localEulerAngles = Vector3.zero;
				obj.transform.localScale = Vector3.one;
			}
			else
			{
				UtDebug.LogError("---part " + inType + " not found");
			}
		}

		private void ResetPartScale(AvatarDataPart part)
		{
			if (part.Geometries == null)
			{
				return;
			}
			for (int i = 0; i < part.Geometries.Length; i++)
			{
				GameObject partObject = GetPartObject(mAvatar.transform, part.PartType, i);
				if (partObject != null)
				{
					partObject.transform.localScale = Vector3.one;
				}
			}
		}

		internal void RemovePartScale()
		{
			for (int i = 0; i < mInstance.Part.Length; i++)
			{
				AvatarDataPart avatarDataPart = mInstance.Part[i];
				if (avatarDataPart != null)
				{
					ResetPartScale(avatarDataPart);
				}
			}
		}

		internal void UpdateAvatar()
		{
			if (mAvatar == null)
			{
				return;
			}
			mInstance.mCurSkin = null;
			AvAvatarProperties component = mAvatar.GetComponent<AvAvatarProperties>();
			if (component != null)
			{
				if (mInstance.mCurSkin != null)
				{
					component._DefaultSkin = mInstance.mCurSkin;
				}
				else
				{
					mInstance.mCurSkin = component._DefaultSkin;
				}
			}
			SetDisplayName(mAvatar, mInstance.DisplayName);
			Transform transform = mAvatar.transform.Find(pBoneSettings.TOP_PARENT_BONE);
			if (transform == null)
			{
				transform = GameObject.Find(pBoneSettings.TOP_PARENT_BONE).transform;
			}
			GameObject gameObject = transform.gameObject;
			bool activeSelf = gameObject.activeSelf;
			gameObject.SetActive(value: true);
			for (int i = 0; i < mInstance.Part.Length; i++)
			{
				int num = mInstance.Part.Length;
				AvatarDataPart avatarDataPart = mInstance.Part[i];
				if (avatarDataPart != null && !avatarDataPart.IsDefault() && !avatarDataPart.PartType.Equals(pPartSettings.AVATAR_PART_TAIL, StringComparison.OrdinalIgnoreCase) && !avatarDataPart.PartType.Equals(pPartSettings.AVATAR_PART_WING, StringComparison.OrdinalIgnoreCase))
				{
					UpdateAvatarPart(avatarDataPart, Shader.Find(GetPartShaderType(avatarDataPart.PartType)), mInstance.mCurSkin);
				}
				if (mInstance.Part.Length < num)
				{
					i--;
				}
			}
			mCustomAvatarState = new CustomAvatarState();
			mCustomAvatarState.FromAvatarData(mInstance);
			mCustomAvatarState.UpdateShaders(mAvatar, this);
			gameObject.SetActive(activeSelf);
			mAvatar.BroadcastMessage("OnUpdateAvatar", SendMessageOptions.DontRequireReceiver);
		}

		public void UpdateAvatar(AvatarDataPart inPart, Color color)
		{
			if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_HAIR, StringComparison.OrdinalIgnoreCase))
			{
				ApplyColorToObject(GetPartInstance(mAvatar, pBoneSettings.HAIR_PARENT_BONE, inPart.PartType), color);
			}
		}

		internal void UpdateAvatarPart(AvatarDataPart inPart, Shader inShader, Texture inSkinTex)
		{
			if (inPart.PartType.Equals("Version", StringComparison.OrdinalIgnoreCase) || inPart.Geometries == null || inPart.Geometries.Length == 0)
			{
				return;
			}
			string parentBone = GetParentBone(inPart.PartType, 0);
			bool inMeshSwap = IsMeshSwap(inPart.PartType);
			string partBoneAttribute = null;
			if (inPart.Attributes != null)
			{
				for (int i = 0; i < inPart.Attributes.Length; i++)
				{
					if (inPart.Attributes[i].Key.Contains("ClonePart"))
					{
						partBoneAttribute = inPart.Attributes[i].Value;
					}
				}
			}
			if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_LEGS, StringComparison.OrdinalIgnoreCase))
			{
				ApplyAvatarGeometry(mAvatar, inPart.Geometries[0], parentBone, inPart.PartType, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_FEET, StringComparison.OrdinalIgnoreCase))
			{
				if (inPart.Geometries.Length > pGeneralSettings.PART_LEFT_INDEX)
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[pGeneralSettings.PART_LEFT_INDEX], parentBone, pPartSettings.AVATAR_PART_FOOT_LEFT, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
				}
				if (inPart.Geometries.Length > pGeneralSettings.PART_RIGHT_INDEX)
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[pGeneralSettings.PART_RIGHT_INDEX], GetParentBone(inPart.PartType, 1), pPartSettings.AVATAR_PART_FOOT_RIGHT, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
				}
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_WRISTBAND, StringComparison.OrdinalIgnoreCase))
			{
				if (inPart.Geometries[0] == "NULL")
				{
					GameObject partObject = GetPartObject(mAvatar.transform, pPartSettings.AVATAR_PART_WRISTBAND_LEFT, 0);
					if (partObject != null)
					{
						SkinnedMeshRenderer componentInChildren = partObject.GetComponentInChildren<SkinnedMeshRenderer>();
						if (componentInChildren != null)
						{
							componentInChildren.sharedMesh = null;
						}
						else
						{
							Debug.LogError("mesh renderer in null!!!!!!!!!! for Wrist band");
						}
					}
					partObject = GetPartObject(mAvatar.transform, pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, 1);
					if (partObject != null)
					{
						SkinnedMeshRenderer componentInChildren2 = partObject.GetComponentInChildren<SkinnedMeshRenderer>();
						if (componentInChildren2 != null)
						{
							componentInChildren2.sharedMesh = null;
						}
						else
						{
							Debug.LogError("mesh renderer in null!!!!!!!!!! for Wrist band");
						}
					}
				}
				else
				{
					if (inPart.Geometries.Length > pGeneralSettings.PART_LEFT_INDEX)
					{
						ApplyAvatarGeometry(mAvatar, inPart.Geometries[pGeneralSettings.PART_LEFT_INDEX], parentBone, pPartSettings.AVATAR_PART_WRISTBAND_LEFT, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
					}
					if (inPart.Geometries.Length > pGeneralSettings.PART_RIGHT_INDEX)
					{
						ApplyAvatarGeometry(mAvatar, inPart.Geometries[pGeneralSettings.PART_RIGHT_INDEX], GetParentBone(inPart.PartType, 1), pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
					}
				}
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_HAND, StringComparison.OrdinalIgnoreCase))
			{
				if (inPart.Geometries.Length > pGeneralSettings.PART_LEFT_INDEX)
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[pGeneralSettings.PART_LEFT_INDEX], parentBone, pPartSettings.AVATAR_PART_HAND_LEFT, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
				}
				if (inPart.Geometries.Length > pGeneralSettings.PART_RIGHT_INDEX)
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[pGeneralSettings.PART_RIGHT_INDEX], GetParentBone(inPart.PartType, 1), pPartSettings.AVATAR_PART_HAND_RIGHT, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
				}
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD, StringComparison.OrdinalIgnoreCase))
			{
				if (inPart.Geometries[0] == "NULL")
				{
					RemovePartData(pPartSettings.AVATAR_PART_SHOULDERPAD);
					RemoveCurrentPart(this, pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, 0);
					RemoveCurrentPart(this, pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, 1);
					return;
				}
				if (inPart.Geometries.Length > pGeneralSettings.PART_LEFT_INDEX)
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[pGeneralSettings.PART_LEFT_INDEX], parentBone, pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
				}
				if (inPart.Geometries.Length > pGeneralSettings.PART_RIGHT_INDEX)
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[pGeneralSettings.PART_RIGHT_INDEX], GetParentBone(inPart.PartType, 1), pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
				}
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_TOP, StringComparison.OrdinalIgnoreCase))
			{
				ApplyAvatarGeometry(mAvatar, inPart.Geometries[0], parentBone, inPart.PartType, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_HEAD, StringComparison.OrdinalIgnoreCase))
			{
				ApplyAvatarGeometry(mAvatar, inPart.Geometries[0], parentBone, inPart.PartType, inMeshSwap, inShader, inSkinTex, inPart.PartType, partBoneAttribute);
				CheckDisableBlink();
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_HAIR, StringComparison.OrdinalIgnoreCase))
			{
				ApplyAvatarGeometry(mAvatar, inPart.Geometries[0], parentBone, inPart.PartType, inMeshSwap, inPart.PartType, partBoneAttribute);
				CheckHelmetHair();
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_HAT, StringComparison.OrdinalIgnoreCase))
			{
				if (inPart.Geometries[0] == "NULL")
				{
					RemovePartData(pPartSettings.AVATAR_PART_HAT);
					RemoveCurrentPart(this, pPartSettings.AVATAR_PART_HAT);
				}
				else if (!GetPartVisibility(mAvatar.name, inPart.PartType))
				{
					RemoveCurrentPart(this, pPartSettings.AVATAR_PART_HAT);
				}
				else
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[0], parentBone, inPart.PartType, inMeshSwap, inPart.PartType, partBoneAttribute);
				}
				CheckHelmetHair();
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_FACEMASK, StringComparison.OrdinalIgnoreCase))
			{
				if (inPart.Geometries[0] == "NULL")
				{
					RemovePartData(pPartSettings.AVATAR_PART_FACEMASK);
					RemoveCurrentPart(this, pPartSettings.AVATAR_PART_FACEMASK);
				}
				else
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[0], parentBone, inPart.PartType, inMeshSwap, inPart.PartType, partBoneAttribute);
				}
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_TAIL, StringComparison.OrdinalIgnoreCase))
			{
				if (inPart.Geometries[0] == "NULL")
				{
					if (_CurrentTail)
					{
						RestoreCurrentPart(this, pPartSettings.AVATAR_PART_LEGS);
						RemovePartData(pPartSettings.AVATAR_PART_TAIL);
					}
				}
				else
				{
					_CurrentTail = true;
					GameObject obj = DisconnectParent(pPartSettings.AVATAR_PART_TOP, 0);
					RemoveCurrentPart(this, pPartSettings.AVATAR_PART_FEET, 0);
					RemoveCurrentPart(this, pPartSettings.AVATAR_PART_FEET, 1);
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[0], parentBone, pPartSettings.AVATAR_PART_LEGS, inMeshSwap: false, inPart.PartType, partBoneAttribute);
					ConnectParent(obj, pPartSettings.AVATAR_PART_TOP, 0);
				}
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, StringComparison.OrdinalIgnoreCase))
			{
				if (inPart.Geometries[0] == "NULL")
				{
					RemovePartData(pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
					RemoveCurrentPart(this, pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
				}
				else
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[0], parentBone, inPart.PartType, inMeshSwap, inPart.PartType, partBoneAttribute);
				}
			}
			else if (inPart.PartType.Equals(pPartSettings.AVATAR_PART_BACK, StringComparison.OrdinalIgnoreCase))
			{
				if (inPart.Geometries[0] == "NULL")
				{
					RemovePartData(pPartSettings.AVATAR_PART_BACK);
					RemoveCurrentPart(this, pPartSettings.AVATAR_PART_BACK);
				}
				else
				{
					ApplyAvatarGeometry(mAvatar, inPart.Geometries[0], parentBone, inPart.PartType, inMeshSwap, inPart.PartType, partBoneAttribute);
				}
			}
		}

		public void CheckHelmetHair()
		{
			Transform partInstance = GetPartInstance(mAvatar, pBoneSettings.HAIR_PARENT_BONE, pPartSettings.AVATAR_PART_HAIR);
			AvatarDataPart avatarDataPart = (GetPartVisibility(mAvatar.name, pPartSettings.AVATAR_PART_HAT) ? FindPart(pPartSettings.AVATAR_PART_HAT) : null);
			if (partInstance != null)
			{
				Renderer renderer = FindRenderer(partInstance);
				if (renderer != null)
				{
					bool flag = true;
					if (avatarDataPart != null && avatarDataPart.Geometries.Length != 0 && GameDataConfig.pInstance.AvatarHatsData != null && GameDataConfig.pInstance.AvatarHatsData.NoHairHats != null)
					{
						for (int i = 0; i < avatarDataPart.Geometries.Length; i++)
						{
							if (!flag)
							{
								continue;
							}
							for (int j = 0; j < GameDataConfig.pInstance.AvatarHatsData.NoHairHats.Length; j++)
							{
								string value = GameDataConfig.pInstance.AvatarHatsData.NoHairHats[j];
								if (avatarDataPart.Geometries[i].Contains(value))
								{
									flag = false;
									break;
								}
							}
						}
					}
					renderer.enabled = flag;
				}
			}
			Transform transform = mAvatar.transform.Find(pBoneSettings.HAT_PARENT_BONE + "/HairScale_J");
			Vector3 localScale = Vector3.one;
			if (avatarDataPart != null && avatarDataPart.Geometries.Length != 0 && avatarDataPart.Geometries[0] != "NULL" && avatarDataPart.Geometries[0] != "PLACEHOLDER")
			{
				localScale = new Vector3(0.5f, 0.5f, 0.5f);
				if (GameDataConfig.pInstance.AvatarHatsData != null && GameDataConfig.pInstance.AvatarHatsData.NoHairHats != null)
				{
					for (int k = 0; k < GameDataConfig.pInstance.AvatarHatsData.NoHairScaleHats.Length; k++)
					{
						string value2 = GameDataConfig.pInstance.AvatarHatsData.NoHairScaleHats[k];
						if (avatarDataPart.Geometries[0].Contains(value2))
						{
							localScale = Vector3.one;
							break;
						}
					}
				}
			}
			if (transform != null)
			{
				transform.localScale = localScale;
			}
			else
			{
				UtDebug.LogError(pBoneSettings.HAT_PARENT_BONE + "/HairScale_J not found");
			}
		}

		public bool CheckDisableBlink()
		{
			AvAvatarBlink componentInChildren = mAvatar.GetComponentInChildren<AvAvatarBlink>(includeInactive: true);
			if (!componentInChildren || GameDataConfig.pInstance?.AvatarHeadData == null || GameDataConfig.pInstance?.AvatarHeadData.NoBlinkGeometries == null)
			{
				return false;
			}
			AvatarDataPart avatarDataPart = FindPart(pPartSettings.AVATAR_PART_HEAD);
			if (avatarDataPart == null)
			{
				return false;
			}
			string[] noBlinkGeometries = GameDataConfig.pInstance.AvatarHeadData.NoBlinkGeometries;
			foreach (string blinkData in noBlinkGeometries)
			{
				if (Array.Find(avatarDataPart.Geometries, (string t) => string.Equals(t, blinkData)) != null)
				{
					componentInChildren.enabled = false;
					componentInChildren.pMeshRenderer.enabled = false;
					componentInChildren.ResetBlink();
					return true;
				}
			}
			if (componentInChildren.enabled)
			{
				return false;
			}
			componentInChildren.enabled = true;
			componentInChildren.pMeshRenderer.enabled = false;
			componentInChildren.ResetBlink();
			return false;
		}

		public Renderer GetPartRenderer(Transform rootTransform, string partName)
		{
			GameObject partObject = GetPartObject(rootTransform, partName, 0);
			if (partObject == null)
			{
				return null;
			}
			return partObject.GetComponent<SkinnedMeshRenderer>();
		}

		public bool CheckAttributeSet(string partName, string key)
		{
			AvatarDataPart avatarDataPart = FindPart(partName);
			if (avatarDataPart?.Attributes == null || avatarDataPart.Attributes.Length == 0)
			{
				return false;
			}
			AvatarPartAttribute[] attributes = avatarDataPart.Attributes;
			foreach (AvatarPartAttribute avatarPartAttribute in attributes)
			{
				if (avatarPartAttribute.Key.Equals(key) && avatarPartAttribute.Value.Equals("True"))
				{
					return true;
				}
			}
			return false;
		}

		public bool FlightSuitEquipped()
		{
			bool flag = CheckAttributeSet(pPartSettings.AVATAR_PART_HAND, "FlightSuit");
			if (flag)
			{
				return true;
			}
			AvatarDataPart avatarDataPart = FindPart(pPartSettings.AVATAR_PART_HAND);
			if (avatarDataPart == null || avatarDataPart.Textures == null || avatarDataPart.Textures.Length == 0)
			{
				return false;
			}
			if (flag)
			{
				return Array.Find(avatarDataPart.Textures, (string t) => !string.IsNullOrEmpty(t) && t.Contains("WingSuit")) != null;
			}
			return false;
		}

		public bool CustomSuitEquipped()
		{
			return CheckAttributeSet(pPartSettings.AVATAR_PART_HAND, "CustomSuit");
		}

		public bool SuitEquipped()
		{
			if (!FlightSuitEquipped())
			{
				return CustomSuitEquipped();
			}
			return true;
		}

		public void Release()
		{
			if (mTextureLoader != null)
			{
				mTextureLoader.Release();
				mTextureLoader = null;
			}
			mTexturesReady = false;
			RsResourceManager.Unload(mMoodTextureURL, splitURL: false);
			RsResourceManager.Unload(mCountryTextureURL, splitURL: false);
			UnloadBundleData();
		}
	}

	public class RestoreRsAssetLoader : RsAssetLoader
	{
		public InstanceInfo mInstanceInfo;

		public string mInType;
	}

	public class Geometry
	{
		public string _Name = "";

		public Transform _Prefab;

		public Mesh pMesh
		{
			get
			{
				if (_Prefab == null)
				{
					return null;
				}
				SkinnedMeshRenderer skinnedMeshRenderer = FindRenderer(_Prefab) as SkinnedMeshRenderer;
				if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
				{
					return skinnedMeshRenderer.sharedMesh;
				}
				return null;
			}
		}

		public Material[] pMaterials
		{
			get
			{
				if (mMaterialCache.TryGetValue(_Name, out var value))
				{
					return value;
				}
				if (_Prefab == null)
				{
					return null;
				}
				SkinnedMeshRenderer skinnedMeshRenderer = FindRenderer(_Prefab) as SkinnedMeshRenderer;
				if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMaterials == null)
				{
					return null;
				}
				return skinnedMeshRenderer.sharedMaterials;
			}
		}

		public Transform Instantiate()
		{
			if (!_Prefab)
			{
				return null;
			}
			Transform transform = UnityEngine.Object.Instantiate(_Prefab);
			if (transform == null)
			{
				Debug.LogError("INSTANTIATE PREFAB FAILED!!");
			}
			return transform;
		}
	}

	public int? Id;

	public string DisplayName;

	[XmlElement(ElementName = "Part")]
	public AvatarDataPart[] Part;

	[XmlElement(ElementName = "Gender")]
	public Gender GenderType;

	[XmlElement(ElementName = "UTD", IsNullable = true)]
	public bool? SetUserNameToDisplayName;

	public static Vector3 VERSION = new Vector3(6f, 1f, 0f);

	private const string DATA_LOCATION = "RS_SHARED";

	private const string DATA_LOCATION_PREFIX = "RS_SHARED/";

	public const string INVALID_DATA = "__EMPTY__";

	public const string VERSION_PART_TYPE = "Version";

	public const string HiddenPartType = "Hidden";

	[XmlIgnore]
	public Texture mCurSkin;

	private static AvatarData mDefault = null;

	private static InstanceInfo mInstanceInfo = new InstanceInfo();

	private static bool mInitialized = false;

	private static int mControlMode = 0;

	private static bool mDisplayYourName = false;

	private static bool mDisplayOtherName = false;

	private static bool mDisplayNPCName = true;

	private static Dictionary<string, WsServiceEventHandler> mGetAvatarCache = new Dictionary<string, WsServiceEventHandler>();

	public Group _Group;

	public static List<RestoreRsAssetLoader> mBundleLoaderList = new List<RestoreRsAssetLoader>();

	public static Dictionary<string, Material[]> mMaterialCache = new Dictionary<string, Material[]>();

	[XmlElement(ElementName = "IsSuggestedAvatarName", IsNullable = true)]
	public bool? IsSuggestedAvatarName { get; set; }

	public static AvatarSettings.BoneSettings pBoneSettings => AvatarSettings.pInstance._BoneSettings;

	public static AvatarSettings.TextureSettings pTextureSettings => AvatarSettings.pInstance._TextureSettings;

	public static AvatarSettings.ShaderSettings pShaderSettings => AvatarSettings.pInstance._ShaderSettings;

	public static AvatarSettings.AvatarPartSettings pPartSettings => AvatarSettings.pInstance._AvatarPartSettings;

	public static AvatarSettings.GeneralSettings pGeneralSettings => AvatarSettings.pInstance._GeneralSettings;

	public static bool pInitialized
	{
		get
		{
			return mInitialized;
		}
		set
		{
			mInitialized = value;
		}
	}

	private static AvatarData pDefault
	{
		get
		{
			if (mDefault == null)
			{
				mDefault = CreateDefault();
			}
			return mDefault;
		}
	}

	public static InstanceInfo pInstanceInfo => mInstanceInfo;

	public static AvatarData pInstance
	{
		get
		{
			if (!pIsReady)
			{
				Init();
			}
			return mInstanceInfo.mInstance;
		}
	}

	public static bool pIsReady => mInstanceInfo.pIsReady;

	public static bool pIsSaving => mInstanceInfo.pIsSaving;

	public static bool pInitializedFromPreviousSave => mInstanceInfo.mInitializedFromPreviousSave;

	public static int pControlMode
	{
		get
		{
			return mControlMode;
		}
		set
		{
			mControlMode = value;
		}
	}

	public static bool pDisplayYourName
	{
		get
		{
			return mDisplayYourName;
		}
		set
		{
			mDisplayYourName = value;
		}
	}

	public static bool pDisplayOtherName
	{
		get
		{
			return mDisplayOtherName;
		}
		set
		{
			mDisplayOtherName = value;
		}
	}

	public static bool pDisplayNPCName
	{
		get
		{
			return mDisplayNPCName;
		}
		set
		{
			mDisplayNPCName = value;
		}
	}

	public AvatarData()
	{
	}

	public AvatarData(Vector3 inVersion, Vector3 inScale)
	{
		AvatarDataPart avatarDataPart = new AvatarDataPart("Version");
		avatarDataPart.SetVersion(inVersion);
		Part = new AvatarDataPart[1];
		Part[0] = avatarDataPart;
	}

	public static void Init()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			mControlMode = PlayerPrefs.GetInt("AvatarControlMode", 0);
			if (UserInfo.pInstance != null)
			{
				mDisplayYourName = PlayerPrefs.GetInt("DYN" + UserInfo.pInstance.UserID, 0) == 1;
				mDisplayOtherName = PlayerPrefs.GetInt("DON" + UserInfo.pInstance.UserID, 1) == 1;
				mDisplayNPCName = PlayerPrefs.GetInt("DNN" + UserInfo.pInstance.UserID, 1) == 1;
			}
			mInstanceInfo.mLoading = true;
			WsWebService.GetAvatarData(mInstanceInfo.ServiceEventHandler, null);
		}
	}

	public static void Reset()
	{
		if (mInstanceInfo != null)
		{
			mInstanceInfo.mBundlesReady = false;
			mInstanceInfo.Release();
		}
	}

	public static void Init(AvatarData data, string countryURL, string moodURL)
	{
		Reset();
		mInitialized = true;
		mControlMode = PlayerPrefs.GetInt("AvatarControlMode", 0);
		mDisplayYourName = PlayerPrefs.GetInt("DYN" + UserInfo.pInstance.UserID, 0) == 1;
		mDisplayOtherName = PlayerPrefs.GetInt("DON" + UserInfo.pInstance.UserID, 1) == 1;
		mDisplayNPCName = PlayerPrefs.GetInt("DNN" + UserInfo.pInstance.UserID, 1) == 1;
		mInstanceInfo.mLoading = true;
		mInstanceInfo.ServiceEventHandler(WsServiceType.GET_AVATAR, WsServiceEvent.COMPLETE, 1f, data, null);
	}

	public static void AddBundles(AvatarData adata, List<string> assetsToDownload)
	{
		for (int i = 0; i < adata.Part.Length; i++)
		{
			adata.Part[i]?.AddBundles(assetsToDownload);
		}
	}

	public static void Clear()
	{
		mInstanceInfo.mInstance = new AvatarData(VERSION, Vector3.one);
	}

	public static AvatarData CreateDefault(Gender GenderType = Gender.Unknown)
	{
		AvatarData obj = new AvatarData
		{
			GenderType = ((GenderType == Gender.Unknown) ? Gender.Female : GenderType)
		};
		MakeDefault(obj);
		return obj;
	}

	public static AvatarData CreateFilteredData(AvatarData inData, string[] inFilter, bool inIncludeFilter, string[] inFilterWingsuit)
	{
		AvatarData avatarData = new AvatarData();
		List<AvatarDataPart> list = new List<AvatarDataPart>();
		if (inData != null && inFilter != null && inData.Part != null)
		{
			if (inIncludeFilter)
			{
				for (int i = 0; i < inData.Part.Length; i++)
				{
					AvatarDataPart avatarDataPart = inData.Part[i];
					if (avatarDataPart != null && avatarDataPart.PartType.Equals("Version", StringComparison.OrdinalIgnoreCase))
					{
						list.Add(avatarDataPart);
					}
				}
				foreach (string value in inFilter)
				{
					for (int j = 0; j < inData.Part.Length; j++)
					{
						AvatarDataPart avatarDataPart2 = inData.Part[j];
						if (avatarDataPart2 != null && avatarDataPart2.PartType.Equals(value, StringComparison.OrdinalIgnoreCase))
						{
							list.Add(avatarDataPart2);
						}
					}
				}
			}
			else
			{
				for (int k = 0; k < inData.Part.Length; k++)
				{
					AvatarDataPart avatarDataPart3 = inData.Part[k];
					if (avatarDataPart3 == null)
					{
						continue;
					}
					bool flag = false;
					foreach (string value2 in inFilter)
					{
						if (avatarDataPart3.PartType.Equals(value2, StringComparison.OrdinalIgnoreCase))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						list.Add(avatarDataPart3);
					}
				}
			}
		}
		if (inData != null && inFilterWingsuit != null && inData.Part != null)
		{
			foreach (string text in inFilterWingsuit)
			{
				for (int n = 0; n < inData.Part.Length; n++)
				{
					AvatarDataPart avatarDataPart4 = inData.Part[n];
					if (avatarDataPart4 != null && avatarDataPart4.PartType.Equals("DEFAULT_" + text, StringComparison.OrdinalIgnoreCase))
					{
						avatarDataPart4.PartType = avatarDataPart4.PartType.Replace("DEFAULT_", "");
					}
				}
			}
		}
		avatarData.Part = list.ToArray();
		return avatarData;
	}

	public static void ApplyColorToObject(Transform inInstance, Color color)
	{
		if (inInstance == null)
		{
			return;
		}
		Renderer[] componentsInChildren = inInstance.GetComponentsInChildren<Renderer>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			if (materials == null || materials.Length == 0)
			{
				continue;
			}
			for (int j = 0; j < materials.Length; j++)
			{
				if (materials[j] != null && materials[j].HasProperty(pShaderSettings.SHADER_PROP_COLOR))
				{
					materials[j].SetColor(pShaderSettings.SHADER_PROP_COLOR, color);
				}
			}
		}
	}

	public static bool CustomizeTextureType(string texture)
	{
		if (AvatarSettings.pInstance == null || AvatarSettings.pInstance._CustomAvatarSettings == null)
		{
			return false;
		}
		return AvatarSettings.pInstance._CustomAvatarSettings._CustomizeTextureTypes.Find((string t) => texture.Contains(t)) != null;
	}

	public static void MakeDefault(AvatarData outData)
	{
		List<AvatarDataPart> list = new List<AvatarDataPart>();
		AvatarDataPart avatarDataPart = new AvatarDataPart("Version");
		avatarDataPart.SetVersion(VERSION);
		list.Add(avatarDataPart);
		AvatarIDefault defaultParts = AvatarDefault.GetDefaultParts(outData.GenderType);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_LEGS);
		avatarDataPart.Geometries = new string[1];
		avatarDataPart.Geometries[0] = defaultParts.pSettings.DEFAULT_LEGS_GEOM_RES;
		avatarDataPart.Textures = new string[1];
		avatarDataPart.Textures[0] = defaultParts.pSettings.DEFAULT_LEGS_TEX_RES;
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_FEET);
		avatarDataPart.Geometries = new string[2];
		UtDebug.Assert(avatarDataPart.Geometries.Length >= Mathf.Max(pGeneralSettings.PART_LEFT_INDEX, pGeneralSettings.PART_RIGHT_INDEX), "GEOMETRY BUFFER SIZe iNVALID!!");
		avatarDataPart.Geometries[pGeneralSettings.PART_LEFT_INDEX] = defaultParts.pSettings.DEFAULT_FOOTL_GEOM_RES;
		avatarDataPart.Geometries[pGeneralSettings.PART_RIGHT_INDEX] = defaultParts.pSettings.DEFAULT_FOOTR_GEOM_RES;
		avatarDataPart.Textures = new string[2];
		avatarDataPart.Textures[0] = defaultParts.pSettings.DEFAULT_FOOTL_TEX_RES;
		avatarDataPart.Textures[1] = defaultParts.pSettings.DEFAULT_FOOTR_TEX_RES;
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_WRISTBAND);
		avatarDataPart.Geometries = new string[2];
		UtDebug.Assert(avatarDataPart.Geometries.Length >= Mathf.Max(pGeneralSettings.PART_LEFT_INDEX, pGeneralSettings.PART_RIGHT_INDEX), "GEOMETRY BUFFER SIZe iNVALID!!");
		avatarDataPart.Geometries[pGeneralSettings.PART_LEFT_INDEX] = defaultParts.pSettings.DEFAULT_WRISTBANDL_GEOM_RES;
		avatarDataPart.Geometries[pGeneralSettings.PART_RIGHT_INDEX] = defaultParts.pSettings.DEFAULT_WRISTBANDR_GEOM_RES;
		avatarDataPart.Textures = new string[1];
		avatarDataPart.Textures[0] = defaultParts.pSettings.DEFAULT_WRISTBAND_TEX_RES;
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_HAND);
		avatarDataPart.Geometries = new string[2];
		UtDebug.Assert(avatarDataPart.Geometries.Length >= Mathf.Max(pGeneralSettings.PART_LEFT_INDEX, pGeneralSettings.PART_RIGHT_INDEX), "GEOMETRY BUFFER SIZe iNVALID!!");
		avatarDataPart.Geometries[pGeneralSettings.PART_LEFT_INDEX] = defaultParts.pSettings.DEFAULT_HANDL_GEOM_RES;
		avatarDataPart.Geometries[pGeneralSettings.PART_RIGHT_INDEX] = defaultParts.pSettings.DEFAULT_HANDR_GEOM_RES;
		avatarDataPart.Textures = new string[2];
		avatarDataPart.Textures[0] = defaultParts.pSettings.DEFAULT_HANDL_TEX_RES;
		avatarDataPart.Textures[1] = defaultParts.pSettings.DEFAULT_HANDR_TEX_RES;
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_TOP);
		avatarDataPart.Geometries = new string[1];
		avatarDataPart.Geometries[0] = defaultParts.pSettings.DEFAULT_TOP_GEOM_RES;
		avatarDataPart.Textures = new string[1];
		avatarDataPart.Textures[0] = defaultParts.pSettings.DEFAULT_TOP_TEX_RES;
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_HEAD);
		avatarDataPart.Geometries = new string[1];
		avatarDataPart.Geometries[0] = defaultParts.pSettings.DEFAULT_HEAD_GEOM_RES;
		avatarDataPart.Textures = new string[6];
		avatarDataPart.Textures[0] = defaultParts.pSettings.DEFAULT_HEAD_TEX_RES;
		avatarDataPart.Textures[1] = defaultParts.pSettings.DEFAULT_HEAD_TEX_MASK_RES;
		avatarDataPart.Textures[2] = defaultParts.pSettings.DEFAULT_EYE_TEX_RES;
		avatarDataPart.Textures[3] = defaultParts.pSettings.DEFAULT_EYE_TEX_MASK_RES;
		avatarDataPart.Textures[4] = defaultParts.pSettings.DEFAULT_HEAD_TEX_DECAL1;
		avatarDataPart.Textures[5] = defaultParts.pSettings.DEFAULT_HEAD_TEX_DECAL2;
		avatarDataPart.Offsets = new AvatarDataPartOffset[4];
		Color[] array = new Color[4]
		{
			defaultParts.pSettings.DEFAULT_SKIN_COLOR,
			defaultParts.pSettings.DEFAULT_HAIR_COLOR,
			defaultParts.pSettings.DEFAULT_EYE_COLOR,
			defaultParts.pSettings.DEFAULT_WARPAINT_COLOR
		};
		for (int i = 0; i < 4; i++)
		{
			AvatarDataPartOffset avatarDataPartOffset = new AvatarDataPartOffset();
			avatarDataPartOffset.X = array[i].r;
			avatarDataPartOffset.Y = array[i].g;
			avatarDataPartOffset.Z = array[i].b;
			avatarDataPart.Offsets[i] = avatarDataPartOffset;
		}
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_HAIR);
		avatarDataPart.Geometries = new string[1];
		avatarDataPart.Geometries[0] = defaultParts.pSettings.DEFAULT_HAIR_GEOM_RES;
		avatarDataPart.Textures = new string[3];
		avatarDataPart.Textures[0] = defaultParts.pSettings.DEFAULT_HAIR_TEX_RES;
		avatarDataPart.Textures[1] = defaultParts.pSettings.DEFAULT_HAIR_TEX_MASK_RES;
		avatarDataPart.Textures[2] = defaultParts.pSettings.DEFAULT_HAIR_TEX_HIGH_RES;
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_HAT);
		avatarDataPart.Geometries = new string[1];
		avatarDataPart.Geometries[0] = "NULL";
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_SHOULDERPAD);
		avatarDataPart.Geometries = new string[2];
		avatarDataPart.Geometries[0] = "NULL";
		avatarDataPart.Geometries[1] = "NULL";
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_FACEMASK);
		avatarDataPart.Geometries = new string[1];
		avatarDataPart.Geometries[0] = "NULL";
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_WING);
		avatarDataPart.Geometries = new string[1];
		avatarDataPart.Geometries[0] = "NULL";
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_EYES);
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_MOUTH);
		avatarDataPart.Textures = new string[2];
		UtDebug.Assert(avatarDataPart.Textures.Length >= Mathf.Max(pGeneralSettings.OPEN_INDEX, pGeneralSettings.CLOSED_INDEX), "TEXTURE BUFFER SIZe iNVALID!!");
		avatarDataPart.Textures[pGeneralSettings.OPEN_INDEX] = defaultParts.pSettings.DEFAULT_MOUTH_OPEN_TEX_RES;
		avatarDataPart.Textures[pGeneralSettings.CLOSED_INDEX] = defaultParts.pSettings.DEFAULT_MOUTH_CLOSE_TEX_RES;
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_SKIN);
		avatarDataPart.Textures = new string[1];
		avatarDataPart.Textures[0] = defaultParts.pSettings.DEFAULT_SKIN_TEX_RES;
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
		avatarDataPart.Geometries = new string[1];
		avatarDataPart.Geometries[0] = defaultParts.pSettings.DEFAULT_HAND_PROP_RIGHT_RES;
		list.Add(avatarDataPart);
		avatarDataPart = new AvatarDataPart(pPartSettings.AVATAR_PART_BACK);
		avatarDataPart.Geometries = new string[1];
		avatarDataPart.Geometries[0] = defaultParts.pSettings.DEFAULT_BACK_RES;
		list.Add(avatarDataPart);
		outData.Part = list.ToArray();
	}

	public static void Merge(AvatarData ioPrimary, AvatarData inAddFrom)
	{
		if (ioPrimary.Part == null)
		{
			ioPrimary.Part = inAddFrom.Part;
			return;
		}
		for (int i = 0; i < inAddFrom.Part.Length; i++)
		{
			AvatarDataPart avatarDataPart = inAddFrom.Part[i];
			int num = -1;
			for (int j = 0; j < ioPrimary.Part.Length; j++)
			{
				if (ioPrimary.Part[j].PartType.Equals(avatarDataPart.PartType, StringComparison.OrdinalIgnoreCase))
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				AvatarDataPart[] array = ioPrimary.Part;
				Array.Resize(ref array, array.Length + 1);
				array[^1] = avatarDataPart;
				ioPrimary.Part = array;
			}
		}
	}

	public static InstanceInfo ApplyCurrent(GameObject inAvatar)
	{
		return ApplyCurrent(inAvatar, null, inIncludeFilter: false);
	}

	public static InstanceInfo ApplyCurrent(GameObject inAvatar, string[] inFilter, bool inIncludeFilter, string[] inFilterWingsuit = null)
	{
		InstanceInfo instanceInfo = null;
		if (mInitialized)
		{
			if (mInstanceInfo.mInstance != null)
			{
				instanceInfo = new InstanceInfo();
				instanceInfo.mAvatar = inAvatar;
				instanceInfo.mInstance = ((inFilter != null) ? CreateFilteredData(mInstanceInfo.mInstance, inFilter, inIncludeFilter, inFilterWingsuit) : mInstanceInfo.mInstance);
				instanceInfo.mBundlesReady = true;
				instanceInfo.mInitializedFromPreviousSave = false;
				instanceInfo.mMergeWithDefault = true;
				instanceInfo.UpdateAvatar();
			}
			else
			{
				UtDebug.LogError("ERROR: Avatar data instance is null, can not apply current avatar to object!!");
			}
		}
		else
		{
			UtDebug.LogError("ERROR: Avatar data not initialized, can not apply current avatar to object!!");
		}
		return instanceInfo;
	}

	public static void RemovePartData(string pname)
	{
		mInstanceInfo.RemovePartData(pname);
	}

	public static void RemoveCurrentPart(string pname)
	{
		RemoveCurrentPart(mInstanceInfo, pname);
	}

	public static void RemoveCurrentPart(InstanceInfo instanceInfo, string pname)
	{
		RemoveCurrentPart(instanceInfo, pname, 0);
	}

	public static void RemoveCurrentPart(InstanceInfo instanceInfo, string pname, int idx)
	{
		if (AvatarSettings.pInstance._AvatarPartSettings.AVATAR_PART_HAND_PROP_RIGHT == pname || AvatarSettings.pInstance._AvatarPartSettings.AVATAR_PART_BACK == pname)
		{
			instanceInfo.mAvatar.BroadcastMessage("ResetPropInfo", pname, SendMessageOptions.DontRequireReceiver);
		}
		GameObject partObject = GetPartObject(instanceInfo.mAvatar.transform, pname, idx);
		if (!(partObject == null))
		{
			partObject.transform.parent = null;
			UnityEngine.Object.Destroy(partObject);
		}
	}

	private static void RestoreBundleLoadEventHandler(RsAssetLoader inLoader, RsResourceLoadEvent inEvent, float inProgress, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.PROGRESS && (uint)(inEvent - 2) <= 1u)
		{
			RestoreRsAssetLoader restoreRsAssetLoader = (RestoreRsAssetLoader)inLoader;
			RestoreCurrentPart(restoreRsAssetLoader.mInstanceInfo, restoreRsAssetLoader.mInType);
			if (CustomAvatarState.mCurrentInstance != null)
			{
				CustomAvatarState.mCurrentInstance.mIsDirty = true;
			}
			if (pInstanceInfo == restoreRsAssetLoader.mInstanceInfo)
			{
				SetDontDestroyOnBundles(inDontDestroy: true);
			}
			mBundleLoaderList.Remove(restoreRsAssetLoader);
		}
	}

	public static bool PartHasGeo(string inType)
	{
		return PartHasGeo(mInstanceInfo, inType);
	}

	public static bool PartHasGeo(InstanceInfo instanceInfo, string inType)
	{
		AvatarDataPart avatarDataPart = instanceInfo.FindPart(inType);
		if (avatarDataPart?.Geometries == null || avatarDataPart.Geometries.Length == 0)
		{
			return false;
		}
		return AvatarDataPart.IsResourceValid(avatarDataPart.Geometries[0]);
	}

	public static void RestoreCurrentPartCheckBundle(string inType)
	{
		if (mInitialized)
		{
			RestoreCurrentPartCheckBundle(mInstanceInfo, inType);
		}
		else
		{
			UtDebug.LogError("ERROR: Avatar data not initialized, can not restore current avatar part!!");
		}
	}

	public static void RestoreCurrentPartCheckBundle(InstanceInfo instanceInfo, string inType, bool ignorePartCheck = false)
	{
		if (instanceInfo.mInstance != null)
		{
			if (instanceInfo.mAvatar != null)
			{
				List<string> list = new List<string>();
				AvatarDataPart avatarDataPart = instanceInfo.FindPart(inType);
				if (ignorePartCheck || avatarDataPart != null)
				{
					avatarDataPart?.AddBundles(list);
					if (list.Count > 0)
					{
						RestoreRsAssetLoader restoreRsAssetLoader = new RestoreRsAssetLoader();
						restoreRsAssetLoader.mInstanceInfo = instanceInfo;
						restoreRsAssetLoader.mInType = inType;
						mBundleLoaderList.Add(restoreRsAssetLoader);
						restoreRsAssetLoader.Load(list.ToArray(), "RS_SHARED", RestoreBundleLoadEventHandler, inDontDestroy: true);
					}
					else
					{
						RestoreCurrentPart(instanceInfo, inType);
					}
				}
			}
			else
			{
				UtDebug.LogError("ERROR: Avatar object instance is null, can not restore current avatar part!!");
			}
		}
		else
		{
			UtDebug.LogError("ERROR: Avatar data instance is null, can not restore current avatar part!!");
		}
	}

	public static void RestoreCurrentPart(string inType)
	{
		if (mInitialized)
		{
			RestoreCurrentPart(mInstanceInfo, inType);
		}
		else
		{
			UtDebug.LogError("ERROR: Avatar data not initialized, can not restore current avatar part!!");
		}
	}

	public static void RestoreCurrentPart(InstanceInfo instanceInfo, string inType)
	{
		if (instanceInfo.mInstance != null)
		{
			if (instanceInfo.mAvatar != null)
			{
				instanceInfo.mInstance.mCurSkin = null;
				AvAvatarProperties component = instanceInfo.mAvatar.GetComponent<AvAvatarProperties>();
				if (component != null)
				{
					instanceInfo.mInstance.mCurSkin = component._DefaultSkin;
				}
				RestoreCurrentPart(instanceInfo, inType, Shader.Find(GetPartShaderType(inType)), instanceInfo.mInstance.mCurSkin);
			}
			else
			{
				UtDebug.LogError("ERROR: Avatar object instance is null, can not restore current avatar part!!");
			}
		}
		else
		{
			UtDebug.LogError("ERROR: Avatar data instance is null, can not restore current avatar part!!");
		}
	}

	public static void RestoreCurrentPart(InstanceInfo instanceInfo, string inType, Shader inShader, Texture inSkin)
	{
		if (instanceInfo.mInstance != null)
		{
			AvatarDataPart avatarDataPart = instanceInfo.FindPart(inType);
			if (avatarDataPart == null && pDefault != null)
			{
				for (int i = 0; i < pDefault.Part.Length; i++)
				{
					AvatarDataPart avatarDataPart2 = pDefault.Part[i];
					if (avatarDataPart2.PartType.Equals(inType, StringComparison.OrdinalIgnoreCase))
					{
						avatarDataPart = avatarDataPart2;
						break;
					}
				}
			}
			if (avatarDataPart != null)
			{
				instanceInfo.UpdateAvatarPart(avatarDataPart, inShader, inSkin);
			}
		}
		else
		{
			UtDebug.LogError("ERROR: Avatar data instance is null, can not restore current avatar part!!");
		}
	}

	public static void RemovePartScale()
	{
		RemovePartScale(mInstanceInfo);
	}

	public static void RemovePartScale(InstanceInfo instanceInfo)
	{
		if (instanceInfo.mInstance != null)
		{
			if (instanceInfo.mAvatar != null)
			{
				instanceInfo.RemovePartScale();
			}
			else
			{
				UtDebug.LogError("ERROR: Avatar object instance is null, can not restore current avatar part!!");
			}
		}
		else
		{
			UtDebug.LogError("ERROR: Avatar data instance is null, can not restore current avatar part!!");
		}
	}

	public static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		string text = inUserData.ToString();
		WsServiceEventHandler value = null;
		if (mGetAvatarCache.TryGetValue(text, out value))
		{
			value(inType, inEvent, inProgress, inObject, null);
			if (inEvent == WsServiceEvent.COMPLETE || inEvent == WsServiceEvent.ERROR)
			{
				mGetAvatarCache.Remove(text);
			}
			if (inEvent == WsServiceEvent.COMPLETE)
			{
				DataCache.Set(text + "_AvatarData", inObject);
			}
		}
	}

	public static void Load(InstanceInfo inInfo, string inUserID)
	{
		inInfo.mLoading = true;
		inInfo.mBundleLoader = null;
		inInfo.mBundlesReady = false;
		inInfo.mInitializedFromPreviousSave = false;
		if (DataCache.Get<AvatarData>(inUserID + "_AvatarData", out var inObject))
		{
			new WsServiceEventHandler(inInfo.ServiceEventHandler)(WsServiceType.GET_AVATAR_BY_USER_ID, WsServiceEvent.COMPLETE, 1f, inObject, null);
		}
		else if (UserInfo.pInstance != null && UserInfo.pInstance.UserID == inUserID)
		{
			DataCache.Set(inUserID + "_AvatarData", pInstance);
			new WsServiceEventHandler(inInfo.ServiceEventHandler)(WsServiceType.GET_AVATAR_BY_USER_ID, WsServiceEvent.COMPLETE, 1f, pInstance, null);
		}
		else if (mGetAvatarCache.ContainsKey(inUserID))
		{
			Dictionary<string, WsServiceEventHandler> dictionary = mGetAvatarCache;
			dictionary[inUserID] = (WsServiceEventHandler)Delegate.Combine(dictionary[inUserID], new WsServiceEventHandler(inInfo.ServiceEventHandler));
		}
		else
		{
			WsServiceEventHandler value = inInfo.ServiceEventHandler;
			mGetAvatarCache.Add(inUserID, value);
			WsWebService.GetAvatarByUserID(inUserID, ServiceEventHandler, inUserID);
		}
	}

	public static void Load(InstanceInfo inInfo, AvatarData inAvatarData)
	{
		inInfo.mLoading = true;
		inInfo.mBundleLoader = null;
		inInfo.mTextureLoader = null;
		inInfo.mTexturesReady = false;
		inInfo.mBundlesReady = false;
		inInfo.mInitializedFromPreviousSave = true;
		inInfo.mInstance = inAvatarData;
		inInfo.OnLoadBundlesAndUpdateAvatar();
	}

	public static void Save()
	{
		if (pIsReady)
		{
			ValidateDefaultParts(mInstanceInfo.mInstance);
			WsWebService.SetAvatar(mInstanceInfo.mInstance, mInstanceInfo.ServiceEventHandler, null);
			mInstanceInfo.mInitializedFromPreviousSave = true;
			mInstanceInfo.mSaveCount++;
		}
	}

	private static void ValidateDefaultParts(AvatarData avData)
	{
		if (false)
		{
			AvatarDataPart[] part = avData.Part;
			foreach (AvatarDataPart avatarDataPart in part)
			{
				if (avatarDataPart.PartType == "Hand" || avatarDataPart.PartType == "Head")
				{
					for (int j = 0; j < avatarDataPart.Textures.Length - 2; j++)
					{
						avatarDataPart.Textures[j] = "PfDWAvWingsDragonCostumeFlightmare.unity3d/DragonCostumeFlightmareWings_Tex";
					}
				}
				if (avatarDataPart.PartType == "Hand")
				{
					avatarDataPart.Geometries[0] = "PfDWAvHandFLWingSuit.unity3d/PfDWAvHandFLWingSuit";
					avatarDataPart.Geometries[1] = "PfDWAvHandFRWingSuit.unity3d/PfDWAvHandFRWingSuit";
					for (int k = 0; k < avatarDataPart.Textures.Length; k++)
					{
						avatarDataPart.Textures[k] = "PfDWAvWingsFDragonArmorValka.unity3d/ValkaFlightSuit_Wing_D";
					}
				}
			}
			return;
		}
		AvatarIDefault defaultParts = AvatarDefault.GetDefaultParts(avData.GenderType);
		bool partVisibility = pInstanceInfo.GetPartVisibility((pInstanceInfo.mAvatar != null) ? pInstanceInfo.mAvatar.name : "PfAvatar", pPartSettings.AVATAR_PART_WING);
		if (!partVisibility || (partVisibility && !pInstanceInfo.SuitEquipped()))
		{
			try
			{
				AvatarDataPart avatarDataPart2 = FindPart("Hand");
				if (avatarDataPart2 != null)
				{
					avatarDataPart2.Attributes = null;
					avatarDataPart2.Geometries[0] = defaultParts.pSettings.DEFAULT_HANDL_GEOM_RES;
					avatarDataPart2.Geometries[1] = defaultParts.pSettings.DEFAULT_HANDR_GEOM_RES;
					avatarDataPart2.Textures[0] = defaultParts.pSettings.DEFAULT_HANDL_TEX_RES;
					avatarDataPart2.Textures[1] = defaultParts.pSettings.DEFAULT_HANDR_TEX_RES;
				}
			}
			catch (Exception message)
			{
				UtDebug.LogError(message);
			}
		}
		try
		{
			AvatarDataPart[] part = avData.Part;
			foreach (AvatarDataPart avatarDataPart3 in part)
			{
				if (avatarDataPart3.Geometries == null || (avatarDataPart3.Geometries.Length == 1 && avatarDataPart3.Geometries[0] == "__EMPTY__"))
				{
					continue;
				}
				for (int l = 0; l < avatarDataPart3.Geometries.Length; l++)
				{
					if (avatarDataPart3.Geometries[l].Contains(defaultParts.pSettings.DEFAULT_HEAD_GEOM_RES))
					{
						if (!avatarDataPart3.Textures[0].Contains(defaultParts.pSettings.DEFAULT_HEAD_TEX_RES))
						{
							avatarDataPart3.Textures[0] = defaultParts.pSettings.DEFAULT_HEAD_TEX_RES;
						}
						if (!avatarDataPart3.Textures[1].Contains(defaultParts.pSettings.DEFAULT_HEAD_TEX_MASK_RES))
						{
							avatarDataPart3.Textures[1] = defaultParts.pSettings.DEFAULT_HEAD_TEX_MASK_RES;
						}
						if (!ValidEyeTexture(avatarDataPart3.Textures[2]))
						{
							avatarDataPart3.Textures[2] = defaultParts.pSettings.DEFAULT_EYE_TEX_RES;
						}
						if (!ValidEyeTexture(avatarDataPart3.Textures[3]))
						{
							avatarDataPart3.Textures[3] = defaultParts.pSettings.DEFAULT_EYE_TEX_MASK_RES;
						}
					}
					if (!avatarDataPart3.Geometries[l].Contains(defaultParts.pSettings.DEFAULT_HANDL_GEOM_RES) && !avatarDataPart3.Geometries[l].Contains(defaultParts.pSettings.DEFAULT_HANDR_GEOM_RES))
					{
						continue;
					}
					for (int m = 0; m < avatarDataPart3.Textures.Length; m++)
					{
						if (avatarDataPart3.Geometries[l].Contains(defaultParts.pSettings.DEFAULT_HANDL_GEOM_RES) && !avatarDataPart3.Textures[m].Contains(defaultParts.pSettings.DEFAULT_HANDL_TEX_RES))
						{
							avatarDataPart3.Textures[m] = defaultParts.pSettings.DEFAULT_HANDL_TEX_RES;
						}
						if (avatarDataPart3.Geometries[l].Contains(defaultParts.pSettings.DEFAULT_HANDR_GEOM_RES) && !avatarDataPart3.Textures[m].Contains(defaultParts.pSettings.DEFAULT_HANDR_TEX_RES))
						{
							avatarDataPart3.Textures[m] = defaultParts.pSettings.DEFAULT_HANDR_TEX_RES;
						}
					}
				}
			}
		}
		catch (Exception message2)
		{
			UtDebug.LogError(message2);
		}
	}

	public static bool ValidEyeTexture(string texture)
	{
		if (AvatarSettings.pInstance == null || AvatarSettings.pInstance._CustomAvatarSettings == null)
		{
			return false;
		}
		return AvatarSettings.pInstance._CustomAvatarSettings._ValidEyeTextures.Find((string t) => texture.Contains(t)) != null;
	}

	public static void SetDontDestroyOnBundles(bool inDontDestroy)
	{
		if (!pIsReady)
		{
			return;
		}
		for (int i = 0; i < mInstanceInfo.mInstance.Part.Length; i++)
		{
			AvatarDataPart avatarDataPart = mInstanceInfo.mInstance.Part[i];
			if (avatarDataPart == null)
			{
				continue;
			}
			if (avatarDataPart.Geometries != null)
			{
				for (int j = 0; j < avatarDataPart.Geometries.Length; j++)
				{
					string text = avatarDataPart.Geometries[j];
					if (AvatarDataPart.IsResourceValid(text))
					{
						string[] array = text.Split('/');
						string text2 = "RS_SHARED/" + array[0];
						if (!RsResourceManager.SetDontDestroy(text2, inDontDestroy))
						{
							UtDebug.LogError("ERROR: PART BUNDLE DESTROY FLAG NOT SET: " + text2);
						}
					}
				}
			}
			if (avatarDataPart.Textures == null)
			{
				continue;
			}
			for (int k = 0; k < avatarDataPart.Textures.Length; k++)
			{
				string text3 = avatarDataPart.Textures[k];
				if (AvatarDataPart.IsResourceValid(text3))
				{
					string[] array = text3.Split('/');
					string text2 = "RS_SHARED/" + array[0];
					if (!RsResourceManager.SetDontDestroy(text2, inDontDestroy))
					{
						UtDebug.LogError("ERROR: PART BUNDLE DESTROY FLAG NOT SET: " + text2);
					}
				}
			}
		}
	}

	public static AvatarDataPart FindPart(string inType)
	{
		return mInstanceInfo.FindPart(inType);
	}

	public static AvatarDataPart GetPart(string inType)
	{
		return GetPart(mInstanceInfo, inType);
	}

	public static AvatarDataPart GetPart(InstanceInfo inInstance, string inType)
	{
		AvatarDataPart avatarDataPart = null;
		if (!inInstance.pIsReady)
		{
			return null;
		}
		if (inInstance.mInstance.Part == null)
		{
			inInstance.mInstance.Part = new AvatarDataPart[1];
			avatarDataPart = new AvatarDataPart(inType);
			inInstance.mInstance.Part[0] = avatarDataPart;
		}
		else
		{
			avatarDataPart = inInstance.FindPart(inType);
			if (avatarDataPart != null)
			{
				return avatarDataPart;
			}
			AvatarDataPart[] array = inInstance.mInstance.Part;
			Array.Resize(ref array, array.Length + 1);
			avatarDataPart = new AvatarDataPart(inType);
			array[^1] = avatarDataPart;
			inInstance.mInstance.Part = array;
		}
		return avatarDataPart;
	}

	public static Renderer FindRenderer(Transform inRoot)
	{
		if (inRoot == null)
		{
			UtDebug.LogError("Input object is null");
			return null;
		}
		Renderer component = inRoot.gameObject.GetComponent<SkinnedMeshRenderer>();
		if (component != null)
		{
			return component;
		}
		component = inRoot.gameObject.GetComponent<MeshRenderer>();
		if (!(component != null))
		{
			return FindRendererInChildren(inRoot);
		}
		return component;
	}

	public static Renderer FindRendererInChildren(Transform inRoot)
	{
		Renderer renderer = null;
		foreach (Transform item in inRoot)
		{
			renderer = item.gameObject.GetComponent<SkinnedMeshRenderer>();
			if (renderer != null)
			{
				return renderer;
			}
			renderer = item.gameObject.GetComponent<MeshRenderer>();
			if (renderer != null)
			{
				return renderer;
			}
		}
		foreach (Transform item2 in inRoot)
		{
			renderer = FindRendererInChildren(item2);
			if (renderer != null)
			{
				return renderer;
			}
		}
		return null;
	}

	public static bool IsDefaultSaved()
	{
		return mInstanceInfo.IsDefaultSaved();
	}

	public static bool IsDefaultSaved(string ptype)
	{
		return mInstanceInfo.IsDefaultSaved(ptype);
	}

	public static void RestoreDefault()
	{
		mInstanceInfo.RestoreDefault();
	}

	public static void RestorePartData()
	{
		mInstanceInfo.RestorePartData();
	}

	public static void SetGroupPart(InstanceInfo instanceInfo, int itemID)
	{
		instanceInfo.SetGroupPart(itemID, save: false);
	}

	public static void SetGroupPart(int itemID, bool saveData)
	{
		mInstanceInfo.SetGroupPart(itemID, saveData);
	}

	public static Gender GetGender()
	{
		return mInstanceInfo.mInstance?.GenderType ?? Gender.Female;
	}

	public static void SetGender(Gender g)
	{
		pInstance.GenderType = g;
	}

	public static Color GetColor(int index)
	{
		Color white = Color.white;
		if (pInstanceInfo == null)
		{
			return white;
		}
		AvatarDataPart avatarDataPart = pInstanceInfo.FindPart(pPartSettings.AVATAR_PART_HEAD);
		if (avatarDataPart == null)
		{
			return white;
		}
		white.r = avatarDataPart.Offsets[index].X;
		white.g = avatarDataPart.Offsets[index].Y;
		white.b = avatarDataPart.Offsets[index].Z;
		white.a = 1f;
		return white;
	}

	public static void SetGeometrySaveDefault(string inType, string inGeoName)
	{
		SetGeometrySaveDefault(mInstanceInfo, inType, inGeoName, 0);
	}

	public static void SetGeometrySaveDefault(string inType, string inGeoName, int inIdx)
	{
		SetGeometrySaveDefault(mInstanceInfo, inType, inGeoName, inIdx);
	}

	public static void SetGeometrySaveDefault(InstanceInfo inInstance, string inType, string inGeoName)
	{
		SetGeometrySaveDefault(inInstance, inType, inGeoName, 0);
	}

	public static void SetGeometrySaveDefault(InstanceInfo inInstance, string inType, string inGeoName, int inIdx)
	{
		if (!inInstance.pIsReady)
		{
			return;
		}
		AvatarDataPart avatarDataPart = inInstance.FindPart("DEFAULT_" + inType);
		if (avatarDataPart == null)
		{
			avatarDataPart = inInstance.FindPart(inType);
			if (avatarDataPart == null)
			{
				avatarDataPart = GetPart(inType);
				if (avatarDataPart.Geometries != null && inIdx < avatarDataPart.Geometries.Length)
				{
					avatarDataPart.Geometries[inIdx] = "PLACEHOLDER";
				}
			}
			avatarDataPart.PartType = "DEFAULT_" + inType;
		}
		SetGeometry(inInstance, inType, inGeoName, inIdx);
	}

	public static void SetGeometry(string inType, string inGeoName)
	{
		SetGeometry(mInstanceInfo, inType, inGeoName, 0);
	}

	public static void SetGeometry(string inType, string inGeoName, int inIdx)
	{
		SetGeometry(mInstanceInfo, inType, inGeoName, inIdx);
	}

	public static void SetGeometry(InstanceInfo inInstance, string inType, string inGeoName)
	{
		SetGeometry(inInstance, inType, inGeoName, 0);
	}

	public static void SetGeometry(InstanceInfo inInstance, string inType, string inGeoName, int inIdx)
	{
		if (!inInstance.pIsReady)
		{
			return;
		}
		AvatarDataPart part = GetPart(inInstance, inType);
		if (part != null)
		{
			if (part.Geometries == null)
			{
				part.Geometries = new string[inIdx + 1];
			}
			else if (inIdx >= part.Geometries.Length)
			{
				string[] array = part.Geometries;
				Array.Resize(ref array, inIdx + 1);
				part.Geometries = array;
			}
			part.Geometries[inIdx] = inGeoName;
		}
	}

	public static void SetStyleTexture(string inType, string inTexName)
	{
		SetStyleTexture(mInstanceInfo, inType, inTexName, 0);
	}

	public static void SetStyleTexture(string inType, string inTexName, int inIdx)
	{
		SetStyleTexture(mInstanceInfo, inType, inTexName, inIdx);
	}

	public static void SetStyleTexture(InstanceInfo inInstance, string inType, string inTexName)
	{
		SetStyleTexture(inInstance, inType, inTexName, 0);
	}

	public static void SetStyleTexture(InstanceInfo inInstance, string inType, string inTexName, int inIdx)
	{
		if (!pIsReady)
		{
			return;
		}
		AvatarDataPart part = GetPart(inInstance, inType);
		if (part != null)
		{
			if (part.Textures == null)
			{
				part.Textures = new string[inIdx + 1];
			}
			else if (inIdx >= part.Textures.Length)
			{
				string[] array = part.Textures;
				Array.Resize(ref array, inIdx + 1);
				part.Textures = array;
			}
			part.Textures[inIdx] = inTexName;
		}
	}

	public static void SetAttributes(InstanceInfo inInstance, string inType, ItemAttribute[] attributes)
	{
		List<AvatarPartAttribute> list = new List<AvatarPartAttribute>();
		if (attributes != null && attributes.Length != 0)
		{
			foreach (ItemAttribute itemAttribute in attributes)
			{
				if (!(itemAttribute.Key == "Gender") && !(itemAttribute.Key == "New"))
				{
					AvatarPartAttribute avatarPartAttribute = new AvatarPartAttribute();
					avatarPartAttribute.Key = itemAttribute.Key;
					avatarPartAttribute.Value = itemAttribute.Value;
					list.Add(avatarPartAttribute);
				}
			}
		}
		SetAttributes(inInstance, inType, list);
	}

	public static void SetAttributes(string inType, List<AvatarPartAttribute> attributes)
	{
		SetAttributes(mInstanceInfo, inType, attributes);
	}

	public static void SetAttributes(InstanceInfo inInstance, string inType, List<AvatarPartAttribute> attributes)
	{
		if (inInstance.pIsReady)
		{
			AvatarDataPart avatarDataPart = inInstance.FindPart(inType);
			if (avatarDataPart != null)
			{
				avatarDataPart.Attributes = attributes?.ToArray();
			}
		}
	}

	public static void SetAttribute(string inType, AvatarPartAttribute attribute)
	{
		SetAttribute(mInstanceInfo, inType, attribute);
	}

	public static void SetAttribute(InstanceInfo inInstance, string inType, AvatarPartAttribute attribute)
	{
		if (!inInstance.pIsReady || attribute == null)
		{
			return;
		}
		AvatarDataPart avatarDataPart = inInstance.FindPart(inType);
		if (avatarDataPart == null)
		{
			return;
		}
		List<AvatarPartAttribute> list = new List<AvatarPartAttribute>();
		if (avatarDataPart.Attributes != null)
		{
			list.AddRange(avatarDataPart.Attributes);
		}
		if (list != null && list.Count > 0)
		{
			AvatarPartAttribute avatarPartAttribute = list.Find((AvatarPartAttribute f) => f.Key == attribute.Key);
			if (avatarPartAttribute != null)
			{
				avatarPartAttribute.Value = attribute.Value;
			}
			else
			{
				list.Add(attribute);
			}
		}
		else
		{
			list.Add(attribute);
		}
		avatarDataPart.Attributes = list.ToArray();
	}

	public static void UseTail(bool t)
	{
		UseTail(mInstanceInfo, t);
	}

	public static void UseTail(InstanceInfo instanceInfo, bool t)
	{
		if (PartHasGeo(instanceInfo, pPartSettings.AVATAR_PART_TAIL) && t != instanceInfo._CurrentTail)
		{
			RestoreCurrentPart(instanceInfo, t ? pPartSettings.AVATAR_PART_TAIL : pPartSettings.AVATAR_PART_LEGS);
		}
	}

	public static Transform GetGroupNameObj(GameObject inAvatar)
	{
		Transform result = null;
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if ((bool)displayNameObj)
		{
			result = displayNameObj.Find("GroupName");
		}
		return result;
	}

	public static Transform GetUDTStarsObj(GameObject inAvatar)
	{
		Transform result = null;
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if ((bool)displayNameObj)
		{
			result = displayNameObj.Find("UDTStarsIcon");
		}
		return result;
	}

	public static void SetGroupName(Group inGroup)
	{
		SetGroupName(AvAvatar.pObject, inGroup);
		MainStreetMMOPlugin.SetGroup(inGroup);
	}

	public static void SetGroupName(GameObject inAvatar, Group inGroup)
	{
		Transform groupNameObj = GetGroupNameObj(inAvatar);
		if (groupNameObj != null)
		{
			string text = ((inGroup != null) ? inGroup.Name : "");
			ObNGUI_Proxy component = groupNameObj.GetComponent<ObNGUI_Proxy>();
			component.SetText(text);
			bool visible = component.GetVisible();
			if (AvAvatar.pObject == inAvatar)
			{
				pInstance._Group = inGroup;
				if (pIsReady)
				{
					mInstanceInfo.UpdateAvatar();
				}
			}
			if (inGroup != null)
			{
				AddGroupToDisplayName(inAvatar, visible, inGroup);
			}
		}
		else
		{
			UtDebug.LogError(inAvatar.name + " doesn't have displayname object attached!!");
		}
	}

	public static Transform GetDisplayNameObj(GameObject inAvatar)
	{
		Transform transform = inAvatar.transform.Find("DWAvatar/DisplayName");
		if (transform == null)
		{
			transform = inAvatar.transform.Find("Sprite/DisplayName");
		}
		if (transform == null)
		{
			transform = inAvatar.transform.Find("DisplayName");
		}
		return transform;
	}

	public static void SetDisplayName(string inName)
	{
		mInstanceInfo.mInstance.DisplayName = inName;
		SetDisplayName(AvAvatar.pObject, inName);
		MainStreetMMOPlugin.SetDisplayName(inName);
	}

	public static void SetDisplayName(GameObject inAvatar, string inName)
	{
		if (inName == null)
		{
			return;
		}
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if (displayNameObj != null)
		{
			ObNGUI_Proxy component = displayNameObj.GetComponent<ObNGUI_Proxy>();
			component.SetText(inName);
			bool visible = component.GetVisible();
			if (AvAvatar.IsCurrentPlayer(inAvatar))
			{
				AddMemberToDisplayName(inAvatar, visible, SubscriptionInfo.pIsMember);
			}
		}
		else
		{
			UtDebug.LogError(inAvatar.name + " doesn't have displayname object attached!!");
		}
	}

	public static Vector3 GetDisplayNameLocalPosition(GameObject inAvatar)
	{
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if (!(displayNameObj != null))
		{
			return Vector3.zero;
		}
		return displayNameObj.localPosition;
	}

	public static void SetDisplayNameLocalPosition(GameObject inAvatar, Vector3 offset)
	{
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if (displayNameObj != null)
		{
			displayNameObj.localPosition = offset;
		}
	}

	public static void SetDisplayNameVisible(GameObject inAvatar, bool inVisible, bool isMember)
	{
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if (displayNameObj != null)
		{
			displayNameObj.GetComponent<ObNGUI_Proxy>().SetVisible(inVisible);
			Transform transform = displayNameObj.Find("MemberIcon");
			if (transform != null)
			{
				transform.GetComponent<ObNGUI_Proxy>().SetVisible(inVisible);
			}
			Transform transform2 = displayNameObj.Find("RankIcon");
			if (transform2 != null)
			{
				transform2.GetComponent<ObNGUI_Proxy>().SetVisible(inVisible);
			}
		}
		SetUDTStarsVisible(inAvatar, inVisible);
		SetGroupNameVisible(inAvatar, inVisible);
	}

	public static void SetUDTStarsVisible(GameObject inAvatar, bool inVisible)
	{
		Transform uDTStarsObj = GetUDTStarsObj(inAvatar);
		if (!(uDTStarsObj == null))
		{
			ObNGUI_Proxy[] componentsInChildren = uDTStarsObj.GetComponentsInChildren<ObNGUI_Proxy>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetVisible(inVisible);
			}
		}
	}

	public static void SetGroupNameVisible(GameObject inAvatar, bool inVisible)
	{
		Transform groupNameObj = GetGroupNameObj(inAvatar);
		if (!(groupNameObj == null))
		{
			ObNGUI_Proxy component = groupNameObj.GetComponent<ObNGUI_Proxy>();
			inVisible = inVisible && !string.IsNullOrEmpty(component.GetText());
			component.SetVisible(inVisible);
			ObNGUI_Proxy[] componentsInChildren = groupNameObj.GetComponentsInChildren<ObNGUI_Proxy>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetVisible(inVisible);
			}
		}
	}

	public static void AddMemberToDisplayName(GameObject inAvatar, bool inVisible, bool isMember)
	{
		if (inAvatar == null)
		{
			return;
		}
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if (!(displayNameObj == null))
		{
			Transform transform = displayNameObj.Find("MemberIcon");
			if (!(transform == null))
			{
				ObNGUI_Proxy component = transform.GetComponent<ObNGUI_Proxy>();
				component.SetVisible(inVisible);
				component.SetSpriteName(isMember ? pGeneralSettings.ICON_MEMBER : pGeneralSettings.ICON_NONMEMBER);
				Vector3 localPosition = GetTextSizeFromProxy(displayNameObj) * 0.5f;
				localPosition.x += 0.1f;
				localPosition.y = transform.localPosition.y;
				transform.localPosition = localPosition;
			}
		}
	}

	public static void AddRankToDisplayName(GameObject inAvatar, Texture inTexture, bool inVisible)
	{
		if (inAvatar == null)
		{
			return;
		}
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if (!(displayNameObj == null))
		{
			Transform transform = displayNameObj.Find("RankIcon");
			if (!(transform == null))
			{
				transform.GetComponent<ObNGUI_Proxy>().SetVisible(inVisible);
				Vector3 localPosition = GetTextSizeFromProxy(displayNameObj) * 0.5f;
				localPosition.x += 0.1f;
				localPosition.y = transform.localPosition.y;
				transform.localPosition = localPosition;
			}
		}
	}

	public static void AddCountryToDisplayName(GameObject inAvatar, Texture inTexture, bool inVisible, string FlagName)
	{
		if (inAvatar == null)
		{
			return;
		}
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if (displayNameObj == null)
		{
			return;
		}
		Transform transform = displayNameObj.Find("RankIcon");
		if (!(transform == null))
		{
			ObNGUI_Proxy component = transform.GetComponent<ObNGUI_Proxy>();
			if (FlagName != null && FlagName == string.Empty)
			{
				FlagName = "IcoCountryUnknown";
			}
			component.SetVisible(inVisible);
			if (!string.IsNullOrEmpty(FlagName))
			{
				component.SetSpriteName(FlagName);
			}
			Vector3 localPosition = -GetTextSizeFromProxy(displayNameObj) * 0.5f;
			localPosition.x -= 0.1f;
			localPosition.y = transform.localPosition.y;
			transform.localPosition = localPosition;
		}
	}

	public static void AddMoodToDisplayName(GameObject inAvatar, Texture inTexture, bool inVisible, string MoodName)
	{
		if (inAvatar == null)
		{
			return;
		}
		Transform displayNameObj = GetDisplayNameObj(inAvatar);
		if (displayNameObj == null)
		{
			return;
		}
		Transform transform = displayNameObj.Find("MemberIcon");
		if (!(transform == null))
		{
			ObNGUI_Proxy component = transform.GetComponent<ObNGUI_Proxy>();
			if (!string.IsNullOrEmpty(MoodName))
			{
				component.SetVisible(inVisible);
				component.SetSpriteName(MoodName);
			}
			else
			{
				component.SetVisible(Visible: false);
			}
			Vector3 localPosition = GetTextSizeFromProxy(displayNameObj) * 0.5f;
			localPosition.x += 0.1f;
			localPosition.y = transform.localPosition.y;
			transform.localPosition = localPosition;
		}
	}

	public static void AddGroupToDisplayName(GameObject inAvatar, bool inVisible, Group inGroup)
	{
		if (inAvatar == null)
		{
			return;
		}
		bool activeSelf = inAvatar.activeSelf;
		if (!activeSelf)
		{
			inAvatar.SetActive(value: true);
		}
		Transform groupNameObj = GetGroupNameObj(inAvatar);
		if (groupNameObj != null)
		{
			Transform transform = groupNameObj.Find("GroupIcon");
			Transform transform2 = groupNameObj.Find("GroupIcon/GroupIconBkg");
			if (transform != null)
			{
				ObNGUI_Proxy component = transform.GetComponent<ObNGUI_Proxy>();
				ObNGUI_Proxy component2 = transform2.GetComponent<ObNGUI_Proxy>();
				component.SetVisible(inVisible);
				component2.SetVisible(inVisible);
				if (inGroup != null && !string.IsNullOrEmpty(inGroup.Logo))
				{
					string[] array = inGroup.Logo.Split('/');
					if (array.Length == 3)
					{
						component.SetSpriteName(array[2]);
					}
					if (inGroup.GetFGColor(out var color))
					{
						component.SetColor(color);
					}
					if (inGroup.GetBGColor(out color))
					{
						component2.SetColor(color);
					}
				}
				Vector3 localPosition = -GetTextSizeFromProxy(groupNameObj) * 0.5f;
				localPosition.x -= 0.1f;
				localPosition.y = transform.localPosition.y;
				transform.localPosition = localPosition;
				ObNGUI_MasterProxy componentInChildren = inAvatar.GetComponentInChildren<ObNGUI_MasterProxy>();
				if (componentInChildren != null)
				{
					componentInChildren.UpdateData(bForce: true);
				}
			}
		}
		if (!activeSelf)
		{
			inAvatar.SetActive(value: false);
		}
	}

	private static Vector3 GetTextSizeFromProxy(Transform obj)
	{
		Vector3 zero = Vector3.zero;
		if (obj == null)
		{
			return zero;
		}
		ObNGUI_Proxy component = obj.GetComponent<ObNGUI_Proxy>();
		if (component == null || component._Widget == null)
		{
			return zero;
		}
		UILabel component2 = component._Widget.GetComponent<UILabel>();
		if (component2 == null || component2.bitmapFont == null)
		{
			return zero;
		}
		Vector2 printedSize = component2.printedSize;
		zero = new Vector3(printedSize.x, printedSize.y, 1f);
		return Vector3.Scale(zero, component2.cachedTransform.localScale);
	}

	public static void ApplyAvatarGeometry(GameObject inAvatar, string inGeoName, string inParent, string inName, bool inMeshSwap, string inType, string partBoneAttribute = null)
	{
		ApplyAvatarGeometry(inAvatar, inGeoName, inParent, inName, inMeshSwap, null, null, inType, partBoneAttribute);
	}

	public static void ApplyAvatarGeometry(GameObject inAvatar, string inGeoName, string inParent, string inName, bool inMeshSwap, Shader inShader, Texture inSkin, string inType, string partBoneAttribute = null)
	{
		switch (inGeoName)
		{
		case "__EMPTY__":
			return;
		case "PLACEHOLDER":
			return;
		}
		GameObject gameObject = null;
		if (!inGeoName.Equals("NULL", StringComparison.OrdinalIgnoreCase))
		{
			gameObject = (GameObject)RsResourceManager.LoadAssetFromBundle("RS_SHARED/" + inGeoName);
			RsResourceManager.Unload("RS_SHARED/" + inGeoName);
			if (gameObject == null)
			{
				UtDebug.LogError("Geometry file not found in the bundle = " + inGeoName);
				return;
			}
		}
		ApplyAvatarGeometry(inAvatar, gameObject, inParent, inName, inMeshSwap, inShader, inSkin, partBoneAttribute);
	}

	public static int GetMaterialIndex(Renderer meshRenderer, AvatarMatType t, out string texName)
	{
		texName = pShaderSettings.SHADER_PROP_PART;
		int result = -1;
		int num = 0;
		if (meshRenderer != null)
		{
			num = meshRenderer.materials.Length;
		}
		if (num == 1)
		{
			switch (t)
			{
			case AvatarMatType.EYES:
				texName = pShaderSettings.SHADER_PROP_EYES;
				result = 0;
				break;
			case AvatarMatType.MOUTH:
				texName = pShaderSettings.SHADER_PROP_MOUTH;
				result = 0;
				break;
			case AvatarMatType.SKIN:
				texName = pShaderSettings.SHADER_PROP_NON_GLOBAL_SKIN;
				result = 0;
				break;
			}
			return result;
		}
		string value = "skintex";
		switch (t)
		{
		case AvatarMatType.SKIN:
			value = "skintex";
			break;
		case AvatarMatType.RANK:
			value = "ranktex";
			break;
		case AvatarMatType.MAIN:
			value = "maintex";
			break;
		case AvatarMatType.MOUTH:
			Debug.LogError("mouth????");
			break;
		}
		for (int i = 0; i < num; i++)
		{
			if (meshRenderer.materials[i].name.IndexOf(value) >= 0)
			{
				return i;
			}
		}
		return -1;
	}

	public static void ApplyAvatarGeometry(GameObject inAvatar, GameObject inPreFab, string inParent, string inName, bool inMeshSwap, Shader inShader, Texture inSkin, string partBoneAttribute = null)
	{
		string n = ((inParent == "") ? inName : (inParent + "/" + inName));
		Transform transform = inAvatar.transform.Find(n);
		Transform transform2 = inAvatar.transform;
		if (inParent != "")
		{
			transform2 = inAvatar.transform.Find(inParent);
		}
		int num = 0;
		string texName = pShaderSettings.SHADER_PROP_SKIN;
		if ((bool)transform2)
		{
			if ((bool)inPreFab)
			{
				Geometry geometry = new Geometry();
				geometry._Name = inPreFab.name;
				geometry._Prefab = inPreFab.transform;
				bool flag = false;
				Color value = Color.white;
				Renderer renderer = null;
				if (transform != null)
				{
					renderer = FindRenderer(transform);
					num = GetMaterialIndex(renderer, AvatarMatType.RANK, out texName);
					if (num >= 0)
					{
						flag = true;
						value = renderer.materials[num].GetColor(pShaderSettings.SHADER_PROP_RANK_COLOR);
					}
					if (inMeshSwap)
					{
						SkinnedMeshRenderer componentInChildren = transform.GetComponentInChildren<SkinnedMeshRenderer>();
						if (componentInChildren == null)
						{
							return;
						}
						componentInChildren.sharedMesh = geometry.pMesh;
						if (inSkin != null)
						{
							num = GetMaterialIndex(componentInChildren, AvatarMatType.SKIN, out texName);
							if (num >= 0)
							{
								componentInChildren.materials[num].SetTexture(texName, inSkin);
							}
						}
						if (flag)
						{
							num = GetMaterialIndex(componentInChildren, AvatarMatType.RANK, out texName);
							if (num >= 0)
							{
								componentInChildren.materials[num].SetColor(pShaderSettings.SHADER_PROP_RANK_COLOR, value);
							}
						}
						return;
					}
				}
				Transform transform3 = null;
				SkinnedMeshRenderer skinnedMeshRenderer = null;
				if (!string.IsNullOrEmpty(partBoneAttribute))
				{
					SkinnedMeshRenderer skinnedMeshRenderer2 = null;
					Transform transform4 = UtUtilities.FindChildTransform(inAvatar, partBoneAttribute);
					if (transform4 != null)
					{
						skinnedMeshRenderer2 = FindRenderer(transform4) as SkinnedMeshRenderer;
					}
					if (geometry._Prefab != null)
					{
						skinnedMeshRenderer = FindRenderer(geometry._Prefab) as SkinnedMeshRenderer;
					}
					transform3 = ((!(transform4 != null) || !(skinnedMeshRenderer != null) || !(skinnedMeshRenderer2 != null)) ? geometry.Instantiate() : UnityEngine.Object.Instantiate(transform4.gameObject).transform);
				}
				else
				{
					transform3 = geometry.Instantiate();
				}
				if (transform3 != null)
				{
					GameObject gameObject = new GameObject(inName);
					gameObject.transform.parent = transform2;
					gameObject.transform.localPosition = Vector3.zero;
					gameObject.transform.localEulerAngles = Vector3.zero;
					gameObject.transform.localScale = Vector3.one;
					transform3.parent = gameObject.transform;
					transform3.localPosition = Vector3.zero;
					transform3.localEulerAngles = Vector3.zero;
					transform3.localScale = Vector3.one;
					renderer = FindRenderer(transform3);
					if (renderer != null)
					{
						if (!string.IsNullOrEmpty(partBoneAttribute) && skinnedMeshRenderer != null)
						{
							SkinnedMeshRenderer skinnedMeshRenderer3 = renderer as SkinnedMeshRenderer;
							if (skinnedMeshRenderer3 != null)
							{
								skinnedMeshRenderer3.sharedMesh = geometry.pMesh;
								skinnedMeshRenderer3.sharedMaterials = geometry.pMaterials;
							}
						}
						if (inName == pPartSettings.AVATAR_PART_HAT)
						{
							Transform transform5 = inAvatar.transform.Find(inParent + "/HairScale_J");
							if (transform5 != null)
							{
								transform3.localPosition = transform5.localPosition;
							}
						}
						if (inName == pPartSettings.AVATAR_PART_FACEMASK)
						{
							Transform transform6 = inAvatar.transform.Find(inParent + "/HairScale_J");
							transform3.localPosition = transform6.localPosition;
						}
						if (GetGender() == Gender.Male && (inName == pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT || inName == pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT))
						{
							transform3.localPosition = new Vector3(0f, 0.025f, -0.015f);
						}
						if (inName == pPartSettings.AVATAR_PART_BACK)
						{
							bool activeSelf = inAvatar.activeSelf;
							inAvatar.SetActive(value: true);
							transform3.SendMessage("ApplyViewInfo", "AvatarBack", SendMessageOptions.DontRequireReceiver);
							transform3.SendMessage("ApplyPropInfo", inAvatar.name, SendMessageOptions.DontRequireReceiver);
							inAvatar.SetActive(activeSelf);
						}
						if (inName == pPartSettings.AVATAR_PART_HAND_PROP_RIGHT)
						{
							bool activeSelf2 = inAvatar.activeSelf;
							inAvatar.SetActive(value: true);
							transform3.SendMessage("ApplyPropInfo", inAvatar.name, SendMessageOptions.DontRequireReceiver);
							inAvatar.SetActive(activeSelf2);
						}
						if (inSkin != null)
						{
							num = GetMaterialIndex(renderer, AvatarMatType.SKIN, out texName);
							if (num >= 0 && renderer.materials[num].HasProperty(texName))
							{
								renderer.materials[num].SetTexture(texName, inSkin);
							}
						}
						if (flag)
						{
							num = GetMaterialIndex(renderer, AvatarMatType.RANK, out texName);
							if (num >= 0)
							{
								renderer.materials[num].SetColor(pShaderSettings.SHADER_PROP_RANK_COLOR, value);
							}
						}
						int num2 = renderer.materials.Length;
						for (int i = 0; i < num2; i++)
						{
							UnityEngine.Object.DontDestroyOnLoad(renderer.materials[i]);
						}
					}
				}
				else
				{
					UtDebug.LogError("NEW PART CREATE FAILED!!!");
				}
			}
			if (transform == null)
			{
				return;
			}
			if (inMeshSwap)
			{
				SkinnedMeshRenderer componentInChildren2 = transform.GetComponentInChildren<SkinnedMeshRenderer>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.sharedMesh = null;
				}
			}
			else
			{
				transform.parent = null;
				UnityEngine.Object.Destroy(transform.gameObject);
			}
		}
		else
		{
			UtDebug.Log("GET PARENT BONE FAILED!!");
		}
	}

	public static GameObject GetPartObject(Transform rootTransform, string partType, int idx)
	{
		string parentBone = GetParentBone(partType, idx);
		string partInstanceName = GetPartInstanceName(partType, idx);
		parentBone = ((!(parentBone == "")) ? (parentBone + "/" + partInstanceName) : partInstanceName);
		Transform transform = rootTransform.Find(parentBone);
		if (!(transform == null))
		{
			return transform.gameObject;
		}
		return null;
	}

	public static Transform GetPartInstance(GameObject inAvatar, string inParent, string inName)
	{
		string n = ((inParent == "") ? inName : (inParent + "/" + inName));
		return inAvatar.transform.Find(n);
	}

	public static void ApplyAvatarRankStyle(GameObject inAvatar, Color rColor)
	{
		if (!(inAvatar != null))
		{
			return;
		}
		int num = 0;
		string texName = pShaderSettings.SHADER_PROP_SKIN;
		Renderer[] componentsInChildren = inAvatar.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			num = GetMaterialIndex(renderer, AvatarMatType.RANK, out texName);
			if (num >= 0 && renderer.materials[num].HasProperty(pShaderSettings.SHADER_PROP_RANK_COLOR))
			{
				renderer.materials[num].SetColor(pShaderSettings.SHADER_PROP_RANK_COLOR, rColor);
			}
		}
	}

	public static bool IsMeshSwap(string partType)
	{
		if (partType.Equals(pPartSettings.AVATAR_PART_LEGS, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FEET, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TOP, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HEAD, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAIR, StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAT, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FACEMASK, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_EYES, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_MOUTH, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SKIN, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TAIL, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WING, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		partType.Equals(pPartSettings.AVATAR_PART_BACK, StringComparison.OrdinalIgnoreCase);
		return false;
	}

	public static string GetPartType(int cID)
	{
		return cID switch
		{
			13 => pPartSettings.AVATAR_PART_LEGS, 
			14 => pPartSettings.AVATAR_PART_FEET, 
			410 => pPartSettings.AVATAR_PART_WRISTBAND, 
			464 => pPartSettings.AVATAR_PART_HAND, 
			12 => pPartSettings.AVATAR_PART_TOP, 
			15 => pPartSettings.AVATAR_PART_HEAD, 
			18 => pPartSettings.AVATAR_PART_HAIR, 
			19 => pPartSettings.AVATAR_PART_HAT, 
			434 => pPartSettings.AVATAR_PART_FACEMASK, 
			16 => pPartSettings.AVATAR_PART_EYES, 
			17 => pPartSettings.AVATAR_PART_MOUTH, 
			20 => pPartSettings.AVATAR_PART_SKIN, 
			228 => pPartSettings.AVATAR_PART_HAND, 
			234 => pPartSettings.AVATAR_PART_TAIL, 
			418 => pPartSettings.AVATAR_PART_SHOULDERPAD, 
			420 => pPartSettings.AVATAR_PART_SCAR, 
			421 => pPartSettings.AVATAR_PART_FACE_DECAL, 
			491 => pPartSettings.AVATAR_PART_BACK, 
			387 => pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, 
			_ => "", 
		};
	}

	public static string GetPartInstanceName(string partType, int index)
	{
		if (partType.Equals(pPartSettings.AVATAR_PART_FEET, StringComparison.OrdinalIgnoreCase))
		{
			if (index != 0)
			{
				return pPartSettings.AVATAR_PART_FOOT_RIGHT;
			}
			return pPartSettings.AVATAR_PART_FOOT_LEFT;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND, StringComparison.OrdinalIgnoreCase))
		{
			if (index != 0)
			{
				return pPartSettings.AVATAR_PART_WRISTBAND_RIGHT;
			}
			return pPartSettings.AVATAR_PART_WRISTBAND_LEFT;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND, StringComparison.OrdinalIgnoreCase))
		{
			if (index != 0)
			{
				return pPartSettings.AVATAR_PART_HAND_RIGHT;
			}
			return pPartSettings.AVATAR_PART_HAND_LEFT;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD, StringComparison.OrdinalIgnoreCase))
		{
			if (index != 0)
			{
				return pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT;
			}
			return pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT;
		}
		if (!partType.Equals(pPartSettings.AVATAR_PART_WING, StringComparison.OrdinalIgnoreCase))
		{
			return partType;
		}
		return pPartSettings.AVATAR_PART_HAND_LEFT;
	}

	public static int GetCategoryID(string partType)
	{
		if (partType.Equals(pPartSettings.AVATAR_PART_LEGS, StringComparison.OrdinalIgnoreCase))
		{
			return 13;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FEET, StringComparison.OrdinalIgnoreCase))
		{
			return 14;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND, StringComparison.OrdinalIgnoreCase))
		{
			return 410;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND, StringComparison.OrdinalIgnoreCase))
		{
			return 464;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TOP, StringComparison.OrdinalIgnoreCase))
		{
			return 12;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HEAD, StringComparison.OrdinalIgnoreCase))
		{
			return 15;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAIR, StringComparison.OrdinalIgnoreCase))
		{
			return 18;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAT, StringComparison.OrdinalIgnoreCase))
		{
			return 19;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FACEMASK, StringComparison.OrdinalIgnoreCase))
		{
			return 434;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_EYES, StringComparison.OrdinalIgnoreCase))
		{
			return 16;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_MOUTH, StringComparison.OrdinalIgnoreCase))
		{
			return 17;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SKIN, StringComparison.OrdinalIgnoreCase))
		{
			return 20;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WING, StringComparison.OrdinalIgnoreCase))
		{
			return 228;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TAIL, StringComparison.OrdinalIgnoreCase))
		{
			return 234;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return 387;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD, StringComparison.OrdinalIgnoreCase))
		{
			return 418;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SCAR, StringComparison.OrdinalIgnoreCase))
		{
			return 420;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FACE_DECAL, StringComparison.OrdinalIgnoreCase))
		{
			return 421;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_BACK, StringComparison.OrdinalIgnoreCase))
		{
			return 491;
		}
		return 0;
	}

	public static string GetParentPartType(string partType)
	{
		if (partType.Equals(pPartSettings.AVATAR_PART_LEGS, StringComparison.OrdinalIgnoreCase))
		{
			return "";
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FOOT_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_LEGS;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FOOT_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_LEGS;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FEET, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_LEGS;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TOP, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_LEGS;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HEAD, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAIR, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FACEMASK, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WING, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TAIL, StringComparison.OrdinalIgnoreCase))
		{
			return "";
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_BACK, StringComparison.OrdinalIgnoreCase))
		{
			return pPartSettings.AVATAR_PART_TOP;
		}
		return "";
	}

	public static string GetPartShaderType(string partType)
	{
		return pShaderSettings.SHADER_DIFFUSE;
	}

	public static string GetParentBone(string partType)
	{
		return GetParentBone(partType, 0);
	}

	public static string GetParentBone(string partType, int index)
	{
		if (partType.Equals(pPartSettings.AVATAR_PART_LEGS, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.LEGS_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FOOT_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.FOOTL_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FOOT_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.FOOTR_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FEET, StringComparison.OrdinalIgnoreCase))
		{
			if (index == 0)
			{
				return pBoneSettings.FOOTL_PARENT_BONE;
			}
			return pBoneSettings.FOOTR_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.WRISTBANDL_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.WRISTBANDR_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND, StringComparison.OrdinalIgnoreCase))
		{
			if (index == 0)
			{
				return pBoneSettings.WRISTBANDL_PARENT_BONE;
			}
			return pBoneSettings.WRISTBANDR_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WING, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.HANDLEFT_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.HANDLEFT_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.HANDRIGHT_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND, StringComparison.OrdinalIgnoreCase))
		{
			if (index != 0)
			{
				return pBoneSettings.HANDRIGHT_PARENT_BONE;
			}
			return pBoneSettings.HANDLEFT_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TOP, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.TOP_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HEAD, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.HEAD_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAIR, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.HAIR_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.HAT_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FACEMASK, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.HAT_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WING, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.WING_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TAIL, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.TAIL_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.HAND_PROP_RIGHT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD, StringComparison.OrdinalIgnoreCase))
		{
			if (index != 0)
			{
				return pBoneSettings.SHOULDERPADRIGHT_PARENT_BONE;
			}
			return pBoneSettings.SHOULDERPADLEFT_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.SHOULDERPADLEFT_PARENT_BONE;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return pBoneSettings.SHOULDERPADRIGHT_PARENT_BONE;
		}
		if (!partType.Equals(pPartSettings.AVATAR_PART_BACK, StringComparison.OrdinalIgnoreCase))
		{
			return "";
		}
		return pBoneSettings.BACK_PARENT_BONE;
	}

	public static int GetPartTypeEnumIdx(string partType)
	{
		return GetPartTypeEnumIdx(partType, 0);
	}

	public static int GetPartTypeEnumIdx(string partType, int idx)
	{
		if (partType.Equals(pPartSettings.AVATAR_PART_LEGS, StringComparison.OrdinalIgnoreCase))
		{
			return 0;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FEET, StringComparison.OrdinalIgnoreCase))
		{
			if (idx != 0)
			{
				return 2;
			}
			return 1;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FOOT_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return 1;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FOOT_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return 2;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TOP, StringComparison.OrdinalIgnoreCase))
		{
			return 3;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HEAD, StringComparison.OrdinalIgnoreCase))
		{
			return 4;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAIR, StringComparison.OrdinalIgnoreCase))
		{
			return 5;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAT, StringComparison.OrdinalIgnoreCase))
		{
			return 6;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_EYES, StringComparison.OrdinalIgnoreCase))
		{
			return 7;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_MOUTH, StringComparison.OrdinalIgnoreCase))
		{
			return 8;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SKIN, StringComparison.OrdinalIgnoreCase))
		{
			return 9;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WING, StringComparison.OrdinalIgnoreCase))
		{
			return 10;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_TAIL, StringComparison.OrdinalIgnoreCase))
		{
			return 11;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_PROP_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return 12;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND, StringComparison.OrdinalIgnoreCase))
		{
			if (idx != 0)
			{
				return 14;
			}
			return 13;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return 13;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return 14;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND, StringComparison.OrdinalIgnoreCase))
		{
			if (idx != 0)
			{
				return 16;
			}
			return 15;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return 15;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_HAND_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return 16;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD, StringComparison.OrdinalIgnoreCase))
		{
			if (idx != 0)
			{
				return 18;
			}
			return 17;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, StringComparison.OrdinalIgnoreCase))
		{
			return 17;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, StringComparison.OrdinalIgnoreCase))
		{
			return 18;
		}
		if (partType.Equals(pPartSettings.AVATAR_PART_FACEMASK, StringComparison.OrdinalIgnoreCase))
		{
			return 19;
		}
		if (!(partType == ""))
		{
			return 0;
		}
		return 20;
	}

	public static string GetItemPartType(ItemData item)
	{
		ItemDataCategory[] array = item?.Category;
		if (array == null || array.Length == 0)
		{
			return "";
		}
		for (int i = 0; i < array.Length; i++)
		{
			string partType = GetPartType(array[i].CategoryId);
			if (partType.Length > 0)
			{
				return partType;
			}
		}
		return "";
	}

	public static string GetPartName(ItemData item)
	{
		if (!item.HasCategory(228))
		{
			return GetItemPartType(item);
		}
		return pPartSettings.AVATAR_PART_WING;
	}

	public static void UseHandPropRight(InstanceInfo instanceInfo, bool restore)
	{
		if (PartHasGeo(instanceInfo, pPartSettings.AVATAR_PART_HAND_PROP_RIGHT))
		{
			if (restore)
			{
				RestoreCurrentPart(instanceInfo, pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
			}
			else
			{
				RemoveCurrentPart(instanceInfo, pPartSettings.AVATAR_PART_HAND_PROP_RIGHT);
			}
		}
	}

	public static void AttachRightHandProp(int itemID)
	{
		int[] ids = new int[1] { itemID };
		pInstanceInfo.SetPartsByItemIDs(ids, save: true);
	}

	public static bool HasItemOfCategory(int categoryID, string partType = null)
	{
		for (int i = 0; i < pInstance.Part.Length; i++)
		{
			AvatarDataPart avatarDataPart = pInstance.Part[i];
			if ((string.IsNullOrEmpty(partType) || avatarDataPart.PartType.Equals(partType)) && CommonInventoryData.pInstance.GetItemDataFromGeometry(avatarDataPart.Geometries, categoryID) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool ValidGenderItem(ItemData itemData)
	{
		string text = "U";
		switch (GetGender())
		{
		case Gender.Female:
			text = "F";
			break;
		case Gender.Male:
			text = "M";
			break;
		}
		string attribute = itemData.GetAttribute("Gender", "U");
		if (!(attribute == text) && !(attribute == "U"))
		{
			return text == "U";
		}
		return true;
	}
}
