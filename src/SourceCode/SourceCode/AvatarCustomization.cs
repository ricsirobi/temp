using System;
using UnityEngine;

public class AvatarCustomization
{
	public delegate void OnAllItemsDownloaded();

	protected GameObject mPrevAvatar;

	protected GameObject mSubstanceAvatar;

	protected AvatarData mAvatarDataInstance;

	protected Vector3 mPrevPostion = Vector3.zero;

	protected CustomAvatarState mCustomAvatar;

	protected int mItemsToDownload;

	protected OnAllItemsDownloaded mOnItemsDownloaded;

	protected AvatarData.InstanceInfo mInstanceInfo;

	public Vector3 pPrevPosition
	{
		set
		{
			mPrevPostion = value;
		}
	}

	public CustomAvatarState pCustomAvatar => mCustomAvatar;

	public GameObject pPrevAvatar => mPrevAvatar;

	public virtual void RestoreAvatar(bool restoreAllTextures = true, bool isDirty = false)
	{
		AvatarData.RestoreDefault();
		mCustomAvatar.FromAvatarData(AvatarData.pInstance);
		mCustomAvatar.UpdateShaders(AvAvatar.pObject);
		if (restoreAllTextures)
		{
			mCustomAvatar.RestoreAll();
		}
		if (isDirty)
		{
			mCustomAvatar.mIsDirty = true;
		}
	}

	public virtual void ResetCustomAvatar()
	{
		mCustomAvatar = null;
	}

	public virtual void CacheTextures()
	{
		if (mCustomAvatar != null)
		{
			mCustomAvatar.CacheState();
		}
	}

	public void SetAvatarData(AvatarData data)
	{
		mAvatarDataInstance = data;
	}

	public AvatarCustomization()
	{
		mCustomAvatar = new CustomAvatarState();
		mCustomAvatar.FromAvatarData(AvatarData.pInstance);
	}

	public virtual GameObject LoadAvatarForPreview(bool isMale, string avatarName, Vector3 avatarPos, Vector3 avatarScale, ref bool inFirstTime)
	{
		if (mAvatarDataInstance != null)
		{
			mCustomAvatar = new CustomAvatarState();
			mCustomAvatar.FromAvatarData(mAvatarDataInstance);
		}
		if (mSubstanceAvatar == null)
		{
			GameObject pObject = AvAvatar.pObject;
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfAvatar"));
			gameObject.name = avatarName;
			AvAvatar.pObject = gameObject;
			AvatarData.ApplyCurrent(gameObject);
			mSubstanceAvatar = gameObject;
			AvAvatar.pObject = pObject;
			ProcessLoadedAvatar(gameObject, isMale, inFirstTime: true, avatarName, avatarPos, avatarScale);
			inFirstTime = true;
			return gameObject;
		}
		ProcessLoadedAvatar(mSubstanceAvatar, isMale, inFirstTime: false, avatarName, avatarPos, avatarScale);
		inFirstTime = false;
		return mSubstanceAvatar;
	}

	public virtual void ProcessLoadedAvatar(GameObject inObject, bool inMale, bool inFirstTime, string avatarName, Vector3 pos, Vector3 scale)
	{
		if (inObject.GetComponent<AvAvatarController>() != null)
		{
			inObject.GetComponent<AvAvatarController>().enabled = false;
		}
		if (inObject.GetComponent<AvAvatarProperties>() != null)
		{
			inObject.GetComponent<AvAvatarProperties>().enabled = false;
		}
		if (inObject.GetComponent<AvSpellCast>() != null)
		{
			inObject.GetComponent<AvSpellCast>().enabled = false;
		}
		inObject.GetComponent<Collider>().enabled = false;
		AvAvatar.CacheAvatar();
		mPrevAvatar = AvAvatar.pObject;
		mPrevAvatar.SetActive(value: false);
		inObject.name = avatarName;
		inObject.transform.localScale = scale;
		AvAvatar.pObject = inObject;
		AvAvatar.SetPosition(pos);
		AvatarData.pInstanceInfo.mAvatar = AvAvatar.pObject;
		AvAvatar.SetDisplayNameVisible(inVisible: false);
		if (mSubstanceAvatar != null)
		{
			mSubstanceAvatar.SetActive(value: true);
		}
		mCustomAvatar.mIsDirty = true;
	}

	public virtual void SwapAvatars()
	{
		if (mPrevAvatar != null && AvAvatar.pObject != null)
		{
			GameObject pObject = AvAvatar.pObject;
			mPrevAvatar.SetActive(value: true);
			AvAvatar.pObject = mPrevAvatar;
			UnityEngine.Object.Destroy(pObject);
			mPrevAvatar = null;
			AvAvatar.SetPosition(mPrevPostion);
			AvatarData.pInstanceInfo.mAvatar = AvAvatar.pObject;
			if (mSubstanceAvatar != null)
			{
				UnityEngine.Object.Destroy(mSubstanceAvatar);
			}
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.VerifyPetFollow();
			}
		}
	}

	public virtual void SaveCustomAvatar()
	{
		SwapAvatars();
		if (mCustomAvatar != null)
		{
			mCustomAvatar.ToAvatarData(AvatarData.pInstanceInfo);
		}
		if (AvatarData.pInstanceInfo != null)
		{
			AvatarData.pInstanceInfo.UpdatePartVisibility(AvatarData.pPartSettings.AVATAR_PART_WING, AvatarData.pInstanceInfo.SuitEquipped());
		}
		AvatarData.Save();
		if (AvatarData.pInstanceInfo != null)
		{
			AvatarData.pInstanceInfo.RemovePart();
			AvatarData.pInstanceInfo.LoadBundlesAndUpdateAvatar();
		}
	}

	public virtual void DestroyCustomAvatar()
	{
		if (mSubstanceAvatar != null)
		{
			UnityEngine.Object.Destroy(mSubstanceAvatar);
			mSubstanceAvatar = null;
		}
	}

	public virtual void DestroyPreviousAvatar()
	{
		if (mPrevAvatar != null)
		{
			UnityEngine.Object.Destroy(mPrevAvatar);
		}
	}

	public bool ApplyItems(ItemData inItemData, bool restoreBeforeApply = true, OnAllItemsDownloaded onItemsDownload = null)
	{
		if (inItemData.Relationship != null && Array.Exists(inItemData.Relationship, (ItemDataRelationship r) => r.Type.Equals("GroupParent")))
		{
			if (restoreBeforeApply)
			{
				mCustomAvatar.ToAvatarData(AvatarData.pInstanceInfo);
			}
			if (Array.Exists(inItemData.Category, (ItemDataCategory c) => c.CategoryId == AvatarData.GetCategoryID(AvatarData.pPartSettings.AVATAR_PART_WING)))
			{
				AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component != null)
				{
					component.SetLastEquippedFlightSuit(inItemData);
				}
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			mOnItemsDownloaded = onItemsDownload;
			ItemDataRelationship[] array = Array.FindAll(inItemData.Relationship, (ItemDataRelationship r) => r.Type.Equals("GroupParent"));
			mItemsToDownload = array.Length;
			ItemDataRelationship[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ItemData.Load(array2[i].ItemId, OnItemDataReady, null);
			}
			mCustomAvatar.mIsDirty = true;
			return true;
		}
		return false;
	}

	protected void OnItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		string itemPartType = AvatarData.GetItemPartType(dataItem);
		ApplyItem(dataItem, itemPartType);
		if (mCustomAvatar != null)
		{
			mCustomAvatar.SetInventoryId(itemPartType, -1, saveDefault: true);
		}
		mItemsToDownload--;
		if (mItemsToDownload == 0)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	public void UpdateGroupPart(AvatarData.InstanceInfo instanceInfo, ItemData itemData, OnAllItemsDownloaded onItemsDownload = null)
	{
		mInstanceInfo = instanceInfo;
		AvatarData.SetGroupPart(instanceInfo, itemData.ItemID);
		ApplyItem(itemData, onItemsDownload);
	}

	public void ApplyItem(ItemData inItemData, OnAllItemsDownloaded onItemsDownload = null)
	{
		string itemPartType = AvatarData.GetItemPartType(inItemData);
		ApplyItem(inItemData, itemPartType);
		ApplyItems(inItemData, restoreBeforeApply: false, onItemsDownload);
	}

	public virtual void ApplyItem(ItemData inItem, string partName)
	{
		if (mCustomAvatar == null)
		{
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_SCAR)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DECAL1, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_FACE_DECAL)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DECAL2, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_HEAD)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			return;
		}
		if (partName != AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			mCustomAvatar.SetTextureData(partName, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(partName, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(partName, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DETAILEYES, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.EYEMASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_HAIR)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, CustomAvatarState.pCustomAvatarSettings.MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, CustomAvatarState.pCustomAvatarSettings.HIGHLIGHT, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_HIGHLIGHT));
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_FEET)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_HAND)
		{
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
			mCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, inItem.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP));
		}
	}

	public virtual void Update()
	{
		DoUpdate();
	}

	public virtual void DoUpdate()
	{
		if ((mInstanceInfo == null || mInstanceInfo.pIsReady) && mItemsToDownload == 0)
		{
			if (mCustomAvatar != null && mCustomAvatar.mIsDirty && mSubstanceAvatar != null && (AvatarData.mBundleLoaderList == null || AvatarData.mBundleLoaderList.Count == 0))
			{
				mCustomAvatar.UpdateShaders(mSubstanceAvatar);
				mCustomAvatar.mIsDirty = false;
			}
			if (mOnItemsDownloaded != null)
			{
				mOnItemsDownloaded();
				mOnItemsDownloaded = null;
			}
		}
	}
}
