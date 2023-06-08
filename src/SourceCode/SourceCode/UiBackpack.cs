using System;
using UnityEngine;

public class UiBackpack : KAUISelect
{
	private static UiBackpack mInstance;

	public LocaleString _UseRewardMultiplierText = new LocaleString("Use DoubleXP. Do you wish to continue?");

	public LocaleString _ReplaceRewardMultiplierText = new LocaleString("Do you like to replace the current experience modifier?");

	public LocaleString _UseMysteryBoxTicketText = new LocaleString("Use Mystery box ticket. Do you wish to continue?");

	public LocaleString _UseBundleTicketText = new LocaleString("Congratulations, you have successfully redeemed your ticket! Check your backpack and journal for rewards");

	public LocaleString _HatchPrimaryDragonText = new LocaleString("Go to the Hatchery to hatch your first dragon, then you will be able to hatch your egg in the stable");

	public int _RewardMultiplierCategoryID = 383;

	public int _MysteryBoxCategoryID = 462;

	public int _BundlesCategoryID = 463;

	public int _DragonSaddlesCategoryID = 380;

	public int _DragonEggsCategoryID = 456;

	public int _GeneralStoreID = 93;

	public string _MysteryBoxBundlePath = "RS_DATA/PfUiClaimMysteryBoxDO.unity3d/PfUiClaimMysteryBoxDO";

	public string _ItemTrashDBBundlePath = "RS_DATA/PfUiBackpackItemTrashDB.unity3d/PfUiBackpackItemTrashDB";

	public int _RedeemItemFetchCount = 10;

	private KAWidget mBackpackItemIcon;

	private UserItemData mSelectedUserItemData;

	private UiBackpackItemActionDB mItemActionDB;

	public AudioClip _ItemTrashSfx;

	public GameObject _ItemTrashParticle;

	private KAWidget mSelectedItemWidget;

	public static UiBackpack pInstance => mInstance;

	public static void Init(int inDefaultTabIdx = -1)
	{
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("BackpackAsset"), OnBackpackLoaded, typeof(GameObject), inDontDestroy: false, inDefaultTabIdx);
	}

	protected override void Start()
	{
		base.Start();
		if (mInstance == null)
		{
			mInstance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		KAUI.SetExclusive(this);
		if (UtPlatform.IsMobile())
		{
			AdManager.DisplayAd(AdEventType.BACKPACK_ENTERED, AdOption.FULL_SCREEN);
		}
	}

	private static void OnBackpackLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			KAUISelect component = obj.GetComponent<KAUISelect>();
			if (component != null)
			{
				component.pDefaultTabIndex = (int)inUserData;
			}
			obj.SetActive(value: true);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.pToolbar.GetComponent<UiToolbar>().SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			Debug.LogError("Failed to load back pack ....");
			break;
		}
	}

	public override void ChangeCategory(KAWidget item)
	{
		if (mBackpackItemIcon != null)
		{
			ClearItemTextureData();
		}
		base.ChangeCategory(item);
	}

	public override void OnOpen()
	{
	}

	public override void OnClose()
	{
		if (mBackpackItemIcon != null)
		{
			ClearItemTextureData();
		}
		mCurrentTabItem = null;
		AvAvatar.pToolbar.GetComponent<UiToolbar>().SetInteractive(interactive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUI.RemoveExclusive(this);
		base.OnClose();
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "BackPackClosed");
		}
		mInstance = null;
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void SelectItem(KAWidget item)
	{
		base.SelectItem(item);
		if (mBackpackItemIcon != null)
		{
			ClearItemTextureData();
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._SlotLocked)
		{
			mKAUiSelectTabMenu.pSelectedTab.pTabData.BuySlot(base.gameObject);
		}
		else
		{
			if (kAUISelectItemData._ItemID <= 0)
			{
				return;
			}
			mSelectedItemWidget = item;
			mSelectedUserItemData = CommonInventoryData.pInstance.FindItem(kAUISelectItemData._ItemID);
			ItemData item2 = mSelectedUserItemData.Item;
			if (mKAUiSelectTabMenu.pSelectedTab._AllowTrash)
			{
				if (!string.IsNullOrEmpty(_ItemTrashDBBundlePath))
				{
					string[] array = _ItemTrashDBBundlePath.Split('/');
					if (array.Length > 2)
					{
						SetInteractive(interactive: false);
						KAUICursorManager.SetDefaultCursor("Loading");
						RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnItemActionDBPopupLoadEvent, typeof(GameObject));
					}
				}
				return;
			}
			if (item2.Attribute != null)
			{
				string attribute = item2.GetAttribute("ActionDB", "");
				if (!string.IsNullOrEmpty(attribute))
				{
					string[] array2 = attribute.Split('/');
					if (array2.Length > 2)
					{
						SetInteractive(interactive: false);
						KAUICursorManager.SetDefaultCursor("Loading");
						RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], OnItemActionDBPopupLoadEvent, typeof(GameObject));
						return;
					}
				}
				if (item2.HasAttribute("Load Compass"))
				{
					UiCompass.ToggleCompass();
					return;
				}
			}
			if (mKAUiSelectTabMenu.pSelectedTab._Type == InventoryTabType.ICON)
			{
				CreateItemTexture(item2, kAUISelectItemData);
			}
			if (item2.HasCategory(_RewardMultiplierCategoryID))
			{
				if (!IsRewardMultiplierAttributesExist(item2))
				{
					return;
				}
				int attribute2 = item2.GetAttribute("MultiplierRewardType", 0);
				RewardMultiplier rewardMultiplier = UserProfile.pProfileData.AvatarInfo.GetRewardMultiplier(attribute2);
				if (rewardMultiplier == null)
				{
					ShowKAUIDialog("PfKAUIGenericDBSm", "SlotUseRewardsMultiplierDB", "SlotUseRewardsMultiplierConfirm", "DestroyKAUIDB", "", "", destroyDB: true, _UseRewardMultiplierText);
					return;
				}
				int multiplierFactor = rewardMultiplier.MultiplierFactor;
				if (item2.GetAttribute("MultiplierFactor", 0) == multiplierFactor)
				{
					ShowKAUIDialog("PfKAUIGenericDBSm", "RewardsMultiplierAddDB", "SlotUseRewardsMultiplierConfirm", "DestroyKAUIDB", "", "", destroyDB: true, _UseRewardMultiplierText);
				}
				else
				{
					ShowKAUIDialog("PfKAUIGenericDBSm", "RewardsMultiplierReplaceDB", "SlotUseRewardsMultiplierConfirm", "DestroyKAUIDB", "", "", destroyDB: true, _ReplaceRewardMultiplierText);
				}
			}
			else if (item2.HasCategory(_MysteryBoxCategoryID))
			{
				if (mSelectedUserItemData.Item.Relationship != null && mSelectedUserItemData.Item.Relationship.Length != 0)
				{
					ShowMysteryBoxUI();
				}
				else
				{
					UtDebug.LogError("Error : Missing Relationship data for the Item ID" + mSelectedUserItemData.Item.ItemID);
				}
			}
			else if (item2.HasCategory(_BundlesCategoryID))
			{
				ShowKAUIDialog("PfKAUIGenericDB", "SlotUseBundleTicketDB", "SlotUseBundleTicketConfirm", "DestroyKAUIDB", "", "", destroyDB: true, _UseBundleTicketText);
			}
			else if (item2.HasCategory(_DragonSaddlesCategoryID))
			{
				ApplyOnPet(kAUISelectItemData._UserItemData);
			}
			else if (item2.HasCategory(_DragonEggsCategoryID) && SanctuaryManager.pCurPetInstance == null)
			{
				ShowKAUIDialog("PfKAUIGenericDB", "Egg Locked", "", "", "DestroyKAUIDB", "", destroyDB: true, _HatchPrimaryDragonText);
			}
			else if (item2.HasCategory(435) && SanctuaryManager.pCurPetInstance != null)
			{
				OnClose();
				UiDragonsAgeUp.Init();
			}
		}
	}

	private void OnDragonAgeUpDone()
	{
		SetInteractive(interactive: true);
	}

	private void OnItemActionDBPopupLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			mItemActionDB = gameObject.GetComponent<UiBackpackItemActionDB>();
			mItemActionDB.pMessageObject = base.gameObject;
			mItemActionDB.pItemData = mSelectedUserItemData.Item;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Error loading Popup!" + inURL);
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private void CreateItemTexture(ItemData inDragdata, KAUISelectItemData inItemData)
	{
		if (mBackpackItemIcon != null)
		{
			ClearItemTextureData();
		}
		if (mBackpackItemIcon == null)
		{
			mBackpackItemIcon = FindItem("WallpaperTemplate");
		}
		if (!(mBackpackItemIcon == null))
		{
			Texture inTexture = (Texture)RsResourceManager.LoadAssetFromBundle(inDragdata.IconName);
			mBackpackItemIcon.SetTexture(inTexture, inPixelPerfect: true);
			mBackpackItemIcon.SetUserData(inItemData);
			mBackpackItemIcon.SetVisibility(inVisible: true);
			mBackpackItemIcon.AttachToCursor(10f, 10f);
		}
	}

	private void ClearItemTextureData()
	{
		mBackpackItemIcon.SetTexture(null);
		mBackpackItemIcon.DetachFromCursor();
		mBackpackItemIcon.SetUserData(null);
		mBackpackItemIcon.SetVisibility(inVisible: false);
		mBackpackItemIcon = null;
	}

	public void SlotUseRewardsMultiplierConfirm()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		RewardMultiplierManager.pInstance?.UseRewardMultiplier(mSelectedUserItemData, OnRewardMultiplierUse);
	}

	private void OnRewardMultiplierUse(bool success, object inUserData)
	{
		if (success && inUserData != null && AvAvatar.pToolbar != null)
		{
			AvAvatar.pToolbar.GetComponent<UiToolbar>().FetchRewardMultiplier();
		}
		SetInteractive(interactive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		ChangeCategory(mPreviousTabWidget);
	}

	private void ShowMysteryBoxUI()
	{
		Money.pUpdateToolbar = false;
		SetInteractive(interactive: false);
		try
		{
			string[] array = _MysteryBoxBundlePath.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MysteryBoxUiHandler, typeof(GameObject));
		}
		catch (Exception message)
		{
			UtDebug.LogError(message);
			OnClose();
		}
	}

	private void MysteryBoxUiHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiMysteryBox";
			UiMysteryChestClaim component = obj.GetComponent<UiMysteryChestClaim>();
			component.pMsgObject = base.gameObject;
			component.pSelectedUserItemData = mSelectedUserItemData;
			base.gameObject.SetActive(value: false);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Money.pUpdateToolbar = true;
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void OnMysteryBoxClosed(bool closeAll)
	{
		if (closeAll)
		{
			OnClose();
			return;
		}
		Money.pUpdateToolbar = true;
		SetInteractive(interactive: true);
		ChangeCategory(mPreviousTabWidget);
	}

	private void SlotUseBundleTicketConfirm()
	{
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.RedeemItem(mSelectedUserItemData, _RedeemItemFetchCount, OnBundleTicketPurchaseDone);
	}

	public void OnBundleTicketPurchaseDone(CommonInventoryResponse response)
	{
		SetInteractive(interactive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (SanctuaryManager.pCurPetData != null && response.CommonInventoryIDs != null)
		{
			CommonInventoryResponseItem[] commonInventoryIDs = response.CommonInventoryIDs;
			foreach (CommonInventoryResponseItem commonInventoryResponseItem in commonInventoryIDs)
			{
				UserItemData userItemData = CommonInventoryData.pInstance.FindItem(commonInventoryResponseItem.ItemID);
				if (userItemData != null && ApplyOnPet(userItemData))
				{
					break;
				}
			}
		}
		ChangeCategory(mPreviousTabWidget);
	}

	private bool ApplyOnPet(UserItemData inItemData)
	{
		int attribute = inItemData.Item.GetAttribute("PetTypeID", -1);
		if (attribute <= 0 || SanctuaryManager.pCurPetData == null || SanctuaryManager.pCurPetData.PetTypeID != attribute)
		{
			return false;
		}
		string tex = "";
		if (inItemData.Item.Texture != null)
		{
			tex = inItemData.Item.Texture[0].TextureName;
		}
		RaisedPetAccType accessoryType = RaisedPetData.GetAccessoryType(inItemData.Item);
		SanctuaryManager.pCurPetInstance.pData.SetAccessory(accessoryType, inItemData.Item.AssetName, tex, inItemData);
		SanctuaryManager.pCurPetInstance.pData.SaveData();
		SanctuaryManager.pCurPetInstance.UpdateData(SanctuaryManager.pCurPetInstance.pData, noHat: false);
		SanctuaryManager.pPendingMMOPetCheck = true;
		return true;
	}

	private UserItemData GetUserItemData(KAWidget inWidget)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (kAUISelectItemData == null)
		{
			return null;
		}
		return CommonInventoryData.pInstance.FindItem(kAUISelectItemData._ItemID) ?? null;
	}

	private bool IsRewardMultiplierAttributesExist(ItemData inItemData)
	{
		if (inItemData.HasAttribute("MultiplierEffectTime") && inItemData.HasAttribute("MultiplierFactor"))
		{
			return inItemData.HasAttribute("MultiplierRewardType");
		}
		return false;
	}

	private void UpdateRewardMultiplier(ItemData inItemData, RewardMultiplier inRewardMultiplier, DateTime srcTime)
	{
		int attribute = inItemData.GetAttribute("MultiplierFactor", 0);
		inRewardMultiplier.MultiplierFactor = attribute;
		int attribute2 = inItemData.GetAttribute("MultiplierEffectTime", 0);
		TimeSpan value = new TimeSpan(0, 0, attribute2);
		inRewardMultiplier.MultiplierEffectTime = srcTime.Add(value);
	}

	private void TrashItem(int count)
	{
		RemoveItemFromInventory(mSelectedUserItemData.Item.ItemID, count);
		ShowParticleEffect();
	}

	protected void RemoveItemFromInventory(int itemID, int inCount = 1)
	{
		CommonInventoryData.pInstance.RemoveItem(itemID, updateServer: true, inCount);
		ChangeCategory(mPreviousTabWidget);
	}

	protected void ShowParticleEffect()
	{
		GameObject itemTrashParticle = _ItemTrashParticle;
		AudioClip itemTrashSfx = _ItemTrashSfx;
		if (itemTrashParticle != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(itemTrashParticle);
			obj.transform.localPosition = mSelectedItemWidget.transform.position;
			obj.transform.localRotation = Quaternion.identity;
		}
		if (itemTrashSfx != null)
		{
			SnChannel.Play(itemTrashSfx, "SFX_Pool", inForce: true);
		}
	}
}
