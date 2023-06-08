using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiAvatarCustomizationMenu : KAUISelectAvatarMenu
{
	public Vector3 _CameraOffset = new Vector3(0f, 1.195f, 0f);

	public EquippedSlotWidget[] _EquippedSlots;

	public string _ItemColorWidget = "CellBkg";

	private List<KAWidget> mCustomizeUIlist = new List<KAWidget>();

	private UiAvatarCustomization mUiCustomization;

	private bool mDragonBtnVisibility;

	private KAUISelectItemData mSelectedItemData;

	private KAUISelectItemData mHighlightedItemData;

	private bool mInitEquipSlots;

	private UiStatCompareDB mUiStatisticsDB;

	private bool mShowStats;

	private Dictionary<string, KAUISelectItemData> mUnEquipItemsData;

	private KAWidget mSellItemWidget;

	private UiItemSellInfoDB mUiItemSellInfoDB;

	private EquippedSlotWidget[] mEquippedAvatarTabInfo;

	private List<string> mRelatedParts;

	private bool mAllWidgetsReady;

	private bool mUnEquipMemberSuit;

	private bool mIsPreLoadingItems;

	private Color mItemDefaultColor = Color.white;

	public bool pInitEquipSlots
	{
		set
		{
			mInitEquipSlots = value;
		}
	}

	public Color pItemDefaultColor => mItemDefaultColor;

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		mInitEquipSlots = true;
		mUiCustomization = parentInt as UiAvatarCustomization;
		mUiStatisticsDB = (UiStatCompareDB)_UiList[0];
		mUiItemSellInfoDB = (UiItemSellInfoDB)_UiList[1];
		mUnEquipItemsData = new Dictionary<string, KAUISelectItemData>();
		if (mUiCustomization != null && mUiCustomization._AvatarPartCategory != null && mUiCustomization._AvatarPartCategory.Count > 0)
		{
			mEquippedAvatarTabInfo = new EquippedSlotWidget[mUiCustomization._AvatarPartCategory.Count];
			for (int i = 0; i < mEquippedAvatarTabInfo.Length; i++)
			{
				mEquippedAvatarTabInfo[i] = new EquippedSlotWidget();
				mEquippedAvatarTabInfo[i]._PartType = mUiCustomization._AvatarPartCategory[i]._PartName;
			}
		}
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, _ItemColorWidget);
	}

	public override void OnScroll(KAWidget inWidget, float inScroll)
	{
		base.OnScroll(inWidget, inScroll);
		base.pViewChanged = true;
		KAWidget inWidget2 = FindItemAt(GetTopItemIdx());
		SelectItem(inWidget2);
	}

	public override void OnIAPStoreClosed()
	{
		RefreshItemLockedState();
		LoadItemsInView();
	}

	public override void LoadItem(KAWidget inWidget)
	{
		KAWidget kAWidget = inWidget.FindChildItem("LockedIcon");
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(kAUISelectItemData._Locked);
		}
	}

	private void OnEquipClicked(KAWidget selectedWidget)
	{
		OnDoubleClick(selectedWidget);
	}

	private IEnumerator PreLoadItems(KAWidget inWidget)
	{
		mSelectedItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (mSelectedItemData == null || mIsPreLoadingItems)
		{
			yield break;
		}
		mIsPreLoadingItems = true;
		if (mSelectedItemData._ItemData.Relationship != null)
		{
			ItemDataRelationship[] array = Array.FindAll(mSelectedItemData._ItemData.Relationship, (ItemDataRelationship r) => r.Type.Equals("GroupParent"));
			for (int i = 0; i < array.Length; i++)
			{
				ItemData.Load(array[i].ItemId, null, null);
			}
		}
		else
		{
			ItemData.Load(mSelectedItemData._ItemID, null, null);
		}
		KAUIState mPreEquipJournalState = KAUIState.DISABLED;
		if (UiJournal.pInstance != null)
		{
			mPreEquipJournalState = UiJournal.pInstance.GetState();
			UiJournal.pInstance.SetState(KAUIState.DISABLED);
		}
		SetUICustomizationInteractive(setActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		while (ItemData.pLoadingList.Count > 0)
		{
			yield return new WaitForEndOfFrame();
		}
		EquipSelectedItem(inWidget);
		mIsPreLoadingItems = false;
		if (UiJournal.pInstance != null)
		{
			UiJournal.pInstance.SetState(mPreEquipJournalState);
		}
		SetUICustomizationInteractive(setActive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		yield return null;
	}

	private void SetUICustomizationInteractive(bool setActive)
	{
		mUiCustomization.SetInteractive(setActive);
		if (mUiCustomization.pUiJournalCustomization != null)
		{
			mUiCustomization.pUiJournalCustomization.SetInteractive(setActive);
		}
	}

	private void EquipSelectedItem(KAWidget inWidget)
	{
		bool flag = true;
		mSelectedItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (mSelectedItemData == null || mUiCustomization == null || mSelectedItemData._ItemData == null || mSelectedItemData._ItemData.ItemID <= 0)
		{
			EquippedSlotWidget equippedSlotWidget = Array.Find(_EquippedSlots, (EquippedSlotWidget x) => x._WidgetSlot == inWidget);
			if (equippedSlotWidget == null)
			{
				return;
			}
			KAUISelectItemData value = null;
			mUnEquipItemsData.TryGetValue(equippedSlotWidget._PartType, out value);
			if (value == null)
			{
				return;
			}
			flag = false;
			mSelectedItemData = value;
		}
		string partName = AvatarData.GetPartName(mSelectedItemData._ItemData);
		if (mUiCustomization.pToggleSuit != inWidget && mSelectedItemData.pEquippedItem)
		{
			KAUISelectItemData value2 = null;
			mUnEquipItemsData.TryGetValue(partName, out value2);
			if (value2 == null)
			{
				return;
			}
			mSelectedItemData = value2;
		}
		bool flag2 = CheckIfRelationalPartExists(mSelectedItemData._ItemData);
		bool flag3 = AvatarData.pInstanceInfo.SuitEquipped();
		AvatarPartTab avatarPartTab = _CurrentTab;
		if (avatarPartTab._PrtTypeName != partName)
		{
			AvatarPartTab[] tabs = mUiCustomization._Tabs;
			foreach (AvatarPartTab avatarPartTab2 in tabs)
			{
				if (avatarPartTab2._PrtTypeName == partName)
				{
					avatarPartTab = avatarPartTab2;
					break;
				}
			}
		}
		if ((flag2 && !flag3) || mSelectedItemData._ItemData.GroupItem() || (mSelectedItemData._ItemData.AssetName.Equals("NULL") && partName.Equals(AvatarData.pPartSettings.AVATAR_PART_WING)))
		{
			mUiCustomization.pAvatarCustomization.pCustomAvatar.ToAvatarData(AvatarData.pInstanceInfo);
			mUiCustomization.pAvatarCustomization.RestoreAvatar(restoreAllTextures: false);
			KAUISelectItemData value3 = null;
			EquippedSlotWidget[] array = Array.FindAll(_EquippedSlots, (EquippedSlotWidget x) => x.pEquippedItemData != null && !x._PartType.Equals(partName) && x.pEquippedItemData.GroupItem());
			if (array != null)
			{
				EquippedSlotWidget[] array2 = array;
				foreach (EquippedSlotWidget equippedSlotWidget2 in array2)
				{
					mUnEquipItemsData.TryGetValue(equippedSlotWidget2._PartType, out value3);
					if (value3 != null)
					{
						SetCustomAvatarTexture(equippedSlotWidget2._PartType, value3._ItemData);
						avatarPartTab.ApplySelection(value3);
						UpdateEquippedSlotWidget(equippedSlotWidget2, value3);
						SetInventoryId(equippedSlotWidget2._PartType, value3._UserItemData);
					}
				}
			}
			if (mSelectedItemData._ItemData.GroupItem())
			{
				mUnEquipItemsData.TryGetValue(partName, out value3);
				EquippedSlotWidget slot = Array.Find(_EquippedSlots, (EquippedSlotWidget x) => x._PartType.Equals(partName));
				if (value3 != null)
				{
					SetCustomAvatarTexture(partName, value3._ItemData);
					avatarPartTab.ApplySelection(value3);
					UpdateEquippedSlotWidget(slot, value3);
					SetInventoryId(partName, value3._UserItemData);
				}
			}
		}
		if (partName == AvatarData.pPartSettings.AVATAR_PART_WING && mUiCustomization.pToggleSuit != null)
		{
			mUiCustomization.pToggleSuit.CloneAndSetUserData(mSelectedItemData);
		}
		bool flag4 = false;
		if (!AvatarData.IsDefaultSaved(partName))
		{
			if (mSelectedItemData._ItemData.GroupItem() && !mUiCustomization.pToggleSuit.IsChecked() && partName.Equals(AvatarData.pPartSettings.AVATAR_PART_WING))
			{
				AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component != null)
				{
					component.SetLastEquippedFlightSuit(mSelectedItemData._ItemData);
				}
				SetInventoryId(partName, mSelectedItemData._UserItemData);
				flag4 = true;
			}
			else
			{
				mUiCustomization.pAvatarCustomization.ApplyItems(mSelectedItemData._ItemData);
				SetInventoryId(partName, mSelectedItemData._UserItemData);
				SetCustomAvatarTexture(partName, mSelectedItemData._ItemData);
			}
		}
		else
		{
			SetInventoryId("DEFAULT_" + partName, mSelectedItemData._UserItemData);
			AvatarData.pInstanceInfo.UpdatePartInventoryId("DEFAULT_" + partName, mSelectedItemData._UserItemData);
			if (partName == AvatarData.pPartSettings.AVATAR_PART_FEET)
			{
				UpdatePartTexture("DEFAULT_" + partName, mSelectedItemData._ItemData);
			}
			else if (partName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND)
			{
				UpdatePartTexture("DEFAULT_" + partName, mSelectedItemData._ItemData);
			}
			else if (partName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
			{
				UpdatePartTexture("DEFAULT_" + partName, mSelectedItemData._ItemData);
			}
			else if (partName == AvatarData.pPartSettings.AVATAR_PART_HAND || partName == AvatarData.pPartSettings.AVATAR_PART_WING)
			{
				UpdatePartTexture("DEFAULT_" + AvatarData.pPartSettings.AVATAR_PART_HAND, mSelectedItemData._ItemData);
			}
			else
			{
				UpdatePartTexture("DEFAULT_" + partName, mSelectedItemData._ItemData);
			}
		}
		if (mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.BattleReadyTabIndex || mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.ClothesTabIndex)
		{
			EquippedSlotWidget equippedSlotWidget3 = Array.Find(_EquippedSlots, (EquippedSlotWidget x) => x._PartType.Equals(partName));
			if (equippedSlotWidget3 != null)
			{
				UpdateEquippedSlotWidget(equippedSlotWidget3, mSelectedItemData);
			}
		}
		else if (mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.AvatarTabIndex)
		{
			EquippedSlotWidget equippedSlotWidget4 = Array.Find(mEquippedAvatarTabInfo, (EquippedSlotWidget x) => x._PartType.Equals(partName));
			if (equippedSlotWidget4 != null)
			{
				UpdateEquippedSlotWidget(equippedSlotWidget4, mSelectedItemData);
			}
		}
		EnableAllButEquippedItems();
		UpdateEquippedSlotsDisplayInfo();
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "EquipItem", partName);
			MissionManager.pInstance.CheckForTaskCompletion("Action", "EquipItem", mSelectedItemData._ItemData.ItemName);
		}
		if (flag)
		{
			SelectItem(inWidget);
			mModified = true;
		}
		if (!flag4)
		{
			avatarPartTab.ApplySelection(mSelectedItemData, checkDefaultPart: true);
		}
		if (mSelectedItemData._ItemData.HasCategory(657))
		{
			if (!AvatarData.IsDefaultSaved(partName))
			{
				ApplyCustomization(partName, !flag4);
			}
			else
			{
				UiAvatarItemCustomization.SaveAvatarPartAttributes("DEFAULT_" + partName, mSelectedItemData._UserInventoryID);
			}
		}
	}

	private void SetCustomAvatarTexture(string partName, ItemData itemData)
	{
		if (partName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND)
		{
			ApplyCustomizationItem(GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, itemData));
			ApplyCustomizationItem(GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, itemData));
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			ApplyCustomizationItem(GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, itemData));
			ApplyCustomizationItem(GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, itemData));
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_FEET)
		{
			ApplyCustomizationItem(GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, itemData));
			if (!string.IsNullOrEmpty(itemData.Geometry2))
			{
				ApplyCustomizationItem(GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, itemData));
			}
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_HAND || partName == AvatarData.pPartSettings.AVATAR_PART_WING)
		{
			if (!itemData.AssetName.Equals("NULL", StringComparison.OrdinalIgnoreCase))
			{
				ApplyCustomizationItem(GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, itemData));
				if (!string.IsNullOrEmpty(itemData.Geometry2))
				{
					ApplyCustomizationItem(GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, itemData));
				}
			}
		}
		else
		{
			ApplyCustomizationItem(GetCustomizationItemData(partName, itemData));
		}
	}

	private void ApplyCustomization(string partName, bool applyCustomization)
	{
		Transform transform = AvAvatar.mTransform.Find(AvatarData.GetParentBone(partName)).Find(partName);
		if (transform != null)
		{
			if (applyCustomization)
			{
				UiAvatarItemCustomization.ApplyCustomization(transform.gameObject, mSelectedItemData._UserInventoryID);
			}
			UiAvatarItemCustomization.SaveAvatarPartAttributes(partName, mSelectedItemData._UserInventoryID);
		}
		else if (partName == AvatarData.pPartSettings.AVATAR_PART_WING)
		{
			if (applyCustomization)
			{
				UiAvatarItemCustomization.ApplyCustomizationOnPart(AvAvatar.mTransform.gameObject, partName, AvatarData.pInstance, mSelectedItemData._UserInventoryID);
			}
			UiAvatarItemCustomization.SaveAvatarPartAttributes(partName, mSelectedItemData._UserInventoryID);
		}
	}

	private void UpdateEquippedSlotWidget(EquippedSlotWidget slot, KAUISelectItemData widgetData)
	{
		if (slot != null && widgetData != null)
		{
			if (slot != null && widgetData._ItemData.AssetName != "NULL")
			{
				slot.pEquippedItemData = widgetData._ItemData;
				slot.pEquippedUserItemData = widgetData._UserItemData;
				slot.pPartUiid = widgetData._UserInventoryID;
			}
			else
			{
				slot.pEquippedItemData = null;
				slot.pEquippedUserItemData = null;
				slot.pPartUiid = -1;
			}
			if (mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() != mUiCustomization.AvatarTabIndex)
			{
				UpdateEquippedSlotWidgetIcon(slot, widgetData._IconTexture);
			}
		}
	}

	private void SetInventoryId(string partName, UserItemData userItemData)
	{
		if (partName != null)
		{
			mUiCustomization.pCustomAvatar?.SetInventoryId(partName, userItemData?.UserInventoryID ?? (-1));
		}
	}

	private int GetInventoryId(string partName)
	{
		if (partName == null)
		{
			return -1;
		}
		return mUiCustomization.pCustomAvatar?.GetInventoryId(partName) ?? (-1);
	}

	private void UpdateEquippedSlotsDisplayInfo()
	{
		if (mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.BattleReadyTabIndex || mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.ClothesTabIndex)
		{
			EquippedSlotWidget[] equippedSlots = _EquippedSlots;
			foreach (EquippedSlotWidget equippedSlotWidget in equippedSlots)
			{
				bool show = equippedSlotWidget.pEquippedUserItemData != null && equippedSlotWidget.pEquippedUserItemData.pIsBattleReady;
				ShowBattleReadyIcon(equippedSlotWidget._WidgetSlot, show);
				ShowFlightReadyIcon(equippedSlotWidget._WidgetSlot, equippedSlotWidget.pEquippedItemData != null && equippedSlotWidget.pEquippedItemData.HasAttribute("FlightSuit"));
				UpdateWidgetBackground(equippedSlotWidget._WidgetSlot, equippedSlotWidget.pEquippedItemData, equippedSlotWidget.pEquippedUserItemData != null && equippedSlotWidget.pEquippedUserItemData.pIsBattleReady);
			}
		}
	}

	private void EnableAllButEquippedItems()
	{
		foreach (KAWidget item in GetItems())
		{
			KAUISelectItemData widgetData = (KAUISelectItemData)item.GetUserData();
			if (widgetData == null || widgetData._ItemData == null || widgetData._ItemData.ItemID <= 0 || widgetData.pGreyMaskWidget == null)
			{
				continue;
			}
			EquippedSlotWidget equippedSlotWidget = null;
			bool pEquippedItem = false;
			if (mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.BattleReadyTabIndex || mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.ClothesTabIndex)
			{
				equippedSlotWidget = Array.Find(_EquippedSlots, (EquippedSlotWidget x) => x.pEquippedItemData == widgetData._ItemData);
			}
			else if (mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.AvatarTabIndex)
			{
				equippedSlotWidget = Array.Find(mEquippedAvatarTabInfo, (EquippedSlotWidget x) => x.pEquippedItemData == widgetData._ItemData);
			}
			if (widgetData._UserItemData != null)
			{
				if (equippedSlotWidget != null && equippedSlotWidget.pPartUiid > 0 && equippedSlotWidget.pPartUiid == widgetData._UserInventoryID)
				{
					pEquippedItem = true;
				}
			}
			else if (equippedSlotWidget != null)
			{
				pEquippedItem = true;
			}
			widgetData.pEquippedItem = pEquippedItem;
			if (mAllWidgetsReady)
			{
				UpdateEquippedItem(widgetData);
			}
		}
	}

	private void UpdateEquippedItem(KAUISelectItemData widgetData)
	{
		if (widgetData.pGreyMaskWidget != null)
		{
			UITexture uITexture = (UITexture)widgetData.GetItem().FindChildNGUIItem("Background");
			UITexture uITexture2 = (UITexture)widgetData.pGreyMaskWidget.FindChildNGUIItem("Icon");
			if (uITexture != null && uITexture.mainTexture != uITexture2.mainTexture)
			{
				uITexture2.mainTexture = uITexture.mainTexture;
			}
			widgetData.pGreyMaskWidget.SetVisibility(widgetData.pEquippedItem);
			uITexture.enabled = !widgetData.pEquippedItem;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mAllWidgetsReady && GetItems().Find((KAWidget item) => item.GetTexture() == null) == null)
		{
			mAllWidgetsReady = true;
			foreach (KAWidget item in GetItems())
			{
				UpdateEquippedItem((KAUISelectItemData)item.GetUserData());
			}
		}
		if (mUnEquipMemberSuit && AvatarData.pInstanceInfo.pIsReady)
		{
			mUiCustomization.pAvatarCustomization.RestoreAvatar(restoreAllTextures: false);
			mUnEquipMemberSuit = false;
			mModified = true;
		}
	}

	private void UpdateEquippedSlotWidgetIcon(EquippedSlotWidget slot, Texture icon)
	{
		if (slot == null)
		{
			return;
		}
		if (icon != null)
		{
			slot._WidgetSlot.SetTexture(icon);
			EnableEquippedSlots(slot, enable: true);
			if (slot._PartType == AvatarData.pPartSettings.AVATAR_PART_WING && AvatarData.pInstanceInfo.SuitEquipped())
			{
				SetSuitToggleChecked(setChecked: true);
			}
		}
		else if (slot.pEquippedUserItemData != null && !slot.pEquippedUserItemData.pIsBattleReady && slot._PartType == AvatarData.pPartSettings.AVATAR_PART_BACK)
		{
			if (slot.pEquippedItemData != null && slot.pPartUiid > 0 && slot.pEquippedItemData.HasCategory(491) && slot.pEquippedItemData.AssetName != "NULL" && !string.IsNullOrEmpty(slot.pEquippedItemData.AssetName))
			{
				UiAvatarItemCustomization.SetThumbnail(slot._WidgetSlot, slot.pPartUiid);
				EnableEquippedSlots(slot, enable: true);
			}
			else
			{
				slot._WidgetSlot.SetTexture(slot._DefaultTexture);
				EnableEquippedSlots(slot, enable: true);
			}
		}
		else if (slot.pEquippedItemData != null && slot.pEquippedItemData.IconName != null)
		{
			slot._WidgetSlot.SetTextureFromBundle(slot.pEquippedItemData.IconName);
			EnableEquippedSlots(slot, enable: true);
		}
		else
		{
			slot._WidgetSlot.SetTexture(slot._DefaultTexture);
			EnableEquippedSlots(slot, enable: false);
		}
	}

	public CustomizationItemData GetCustomizationItemData(string inPartName, ItemData inItemData)
	{
		CustomizationItemData customizationItemData = new CustomizationItemData();
		customizationItemData._PartName = inPartName;
		if (inPartName == AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			string textureName = inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_EYES_OPEN);
			if (!string.IsNullOrEmpty(textureName))
			{
				customizationItemData._EyeTexture = textureName;
			}
		}
		else if (inPartName == AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT || inPartName == AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT || inPartName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT || inPartName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT)
		{
			string textureName2 = inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLER);
			if (textureName2 == "")
			{
				textureName2 = inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE);
			}
			if (!string.IsNullOrEmpty(textureName2))
			{
				customizationItemData._MainTexture = textureName2;
			}
		}
		else if (!string.IsNullOrEmpty(inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE)))
		{
			customizationItemData._MainTexture = inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_STYLE);
		}
		string textureName3 = inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_HIGHLIGHT);
		if (!string.IsNullOrEmpty(textureName3))
		{
			customizationItemData._HighlightTexture = textureName3;
		}
		textureName3 = inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_MASK);
		if (!string.IsNullOrEmpty(textureName3))
		{
			customizationItemData._MaskTexture = textureName3;
		}
		textureName3 = inItemData.GetTextureName(AvatarData.pTextureSettings.TEXTURE_TYPE_BUMP);
		if (!string.IsNullOrEmpty(textureName3))
		{
			customizationItemData._BumpMapTexture = textureName3;
		}
		textureName3 = inItemData.GetTextureName("Decal");
		if (!string.IsNullOrEmpty(textureName3))
		{
			customizationItemData._DecalTexture = textureName3;
		}
		return customizationItemData;
	}

	public void ApplyCustomizationItem(CustomizationItemData item)
	{
		CustomAvatarState pCustomAvatar = mUiCustomization.pCustomAvatar;
		if (pCustomAvatar == null)
		{
			return;
		}
		if (item._PartName == AvatarData.pPartSettings.AVATAR_PART_SCAR)
		{
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DECAL1, item._MainTexture);
			return;
		}
		if (item._PartName == AvatarData.pPartSettings.AVATAR_PART_FACE_DECAL)
		{
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DECAL2, item._MainTexture);
			bool disabled = mUiCustomization.SetWarPaintColorBtnStatus();
			if (mUiCustomization.pColorPalette != null)
			{
				mUiCustomization.pColorPalette.SetDisabled(disabled);
			}
			return;
		}
		if (item._PartName == AvatarData.pPartSettings.AVATAR_PART_HEAD)
		{
			pCustomAvatar.SetTextureData(item._PartName, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			return;
		}
		if (item._PartName != AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			pCustomAvatar.SetTextureData(item._PartName, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			pCustomAvatar.SetTextureData(item._PartName, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(item._PartName, CustomAvatarState.pCustomAvatarSettings.BUMP, item._BumpMapTexture);
		}
		if (item._PartName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND)
		{
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, item._BumpMapTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, item._BumpMapTexture);
		}
		if (item._PartName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD)
		{
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, item._BumpMapTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, item._BumpMapTexture);
		}
		if (item._PartName == AvatarData.pPartSettings.AVATAR_PART_EYES)
		{
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.DETAILEYES, item._EyeTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HEAD, CustomAvatarState.pCustomAvatarSettings.EYEMASK, item._MaskTexture);
		}
		else if (item._PartName == AvatarData.pPartSettings.AVATAR_PART_HAIR)
		{
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, CustomAvatarState.pCustomAvatarSettings.MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAIR, CustomAvatarState.pCustomAvatarSettings.HIGHLIGHT, item._HighlightTexture);
		}
		else if (item._PartName == AvatarData.pPartSettings.AVATAR_PART_FEET)
		{
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, item._BumpMapTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, item._BumpMapTexture);
		}
		else if (item._PartName == AvatarData.pPartSettings.AVATAR_PART_HAND)
		{
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.DETAIL, item._MainTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.COLOR_MASK, item._MaskTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, CustomAvatarState.pCustomAvatarSettings.BUMP, item._BumpMapTexture);
			pCustomAvatar.SetTextureData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, CustomAvatarState.pCustomAvatarSettings.BUMP, item._BumpMapTexture);
		}
	}

	public void UpdatePartTexture(string partType, ItemData itemData)
	{
		if (string.IsNullOrEmpty(partType))
		{
			return;
		}
		AvatarDataPart avatarDataPart = AvatarData.pInstanceInfo.FindPart(partType);
		if (avatarDataPart == null)
		{
			return;
		}
		avatarDataPart.Textures = new string[2];
		if (partType.Contains(AvatarData.pPartSettings.AVATAR_PART_FEET))
		{
			CustomizationItemData customizationItemData = GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_FOOT_LEFT, mSelectedItemData._ItemData);
			CustomizationItemData customizationItemData2 = customizationItemData;
			if (!string.IsNullOrEmpty(mSelectedItemData._ItemData.Geometry2))
			{
				customizationItemData2 = GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_FOOT_RIGHT, mSelectedItemData._ItemData);
			}
			SetPartTextureByIndex(avatarDataPart, 0, customizationItemData._MainTexture);
			SetPartTextureByIndex(avatarDataPart, 1, customizationItemData2._MainTexture);
			SetPartTextureByIndex(avatarDataPart, 2, customizationItemData._MaskTexture);
			SetPartTextureByIndex(avatarDataPart, 3, customizationItemData2._MaskTexture);
			SetPartTextureByIndex(avatarDataPart, 4, customizationItemData._BumpMapTexture);
			SetPartTextureByIndex(avatarDataPart, 5, customizationItemData2._BumpMapTexture);
		}
		else if (partType.Contains(AvatarData.pPartSettings.AVATAR_PART_HAND))
		{
			CustomizationItemData customizationItemData3 = GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_HAND_LEFT, mSelectedItemData._ItemData);
			CustomizationItemData customizationItemData4 = customizationItemData3;
			if (!string.IsNullOrEmpty(mSelectedItemData._ItemData.Geometry2))
			{
				customizationItemData4 = GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_HAND_RIGHT, mSelectedItemData._ItemData);
			}
			SetPartTextureByIndex(avatarDataPart, 0, customizationItemData3._MainTexture);
			SetPartTextureByIndex(avatarDataPart, 1, customizationItemData4._MainTexture);
			SetPartTextureByIndex(avatarDataPart, 2, customizationItemData3._MaskTexture);
			SetPartTextureByIndex(avatarDataPart, 3, customizationItemData4._MaskTexture);
			SetPartTextureByIndex(avatarDataPart, 4, customizationItemData3._BumpMapTexture);
			SetPartTextureByIndex(avatarDataPart, 5, customizationItemData4._BumpMapTexture);
		}
		else if (partType.Contains(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD))
		{
			CustomizationItemData customizationItemData5 = GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_LEFT, mSelectedItemData._ItemData);
			CustomizationItemData customizationItemData6 = GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD_RIGHT, mSelectedItemData._ItemData);
			SetPartTextureByIndex(avatarDataPart, 0, customizationItemData5._MainTexture);
			SetPartTextureByIndex(avatarDataPart, 1, customizationItemData6._MainTexture);
			SetPartTextureByIndex(avatarDataPart, 2, customizationItemData5._MaskTexture);
			SetPartTextureByIndex(avatarDataPart, 3, customizationItemData6._MaskTexture);
			SetPartTextureByIndex(avatarDataPart, 4, customizationItemData5._BumpMapTexture);
			SetPartTextureByIndex(avatarDataPart, 5, customizationItemData6._BumpMapTexture);
		}
		else if (partType.Contains(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND))
		{
			CustomizationItemData customizationItemData7 = GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_LEFT, mSelectedItemData._ItemData);
			CustomizationItemData customizationItemData8 = GetCustomizationItemData(AvatarData.pPartSettings.AVATAR_PART_WRISTBAND_RIGHT, mSelectedItemData._ItemData);
			SetPartTextureByIndex(avatarDataPart, 0, customizationItemData7._MainTexture);
			SetPartTextureByIndex(avatarDataPart, 1, customizationItemData8._MainTexture);
			SetPartTextureByIndex(avatarDataPart, 2, customizationItemData7._MaskTexture);
			SetPartTextureByIndex(avatarDataPart, 3, customizationItemData8._MaskTexture);
			SetPartTextureByIndex(avatarDataPart, 4, customizationItemData7._BumpMapTexture);
			SetPartTextureByIndex(avatarDataPart, 5, customizationItemData8._BumpMapTexture);
		}
		else
		{
			CustomizationItemData customizationItemData9 = GetCustomizationItemData(partType, mSelectedItemData._ItemData);
			SetPartTextureByIndex(avatarDataPart, 0, customizationItemData9._MainTexture);
			SetPartTextureByIndex(avatarDataPart, 1, customizationItemData9._MaskTexture);
			SetPartTextureByIndex(avatarDataPart, 2, customizationItemData9._BumpMapTexture);
		}
	}

	public void SetPartTextureByIndex(AvatarDataPart part, int index, string texturePath)
	{
		string[] array = texturePath.Split('/');
		if (array.Length > 2)
		{
			if (part.Textures == null)
			{
				part.Textures = new string[index + 1];
			}
			else if (index >= part.Textures.Length)
			{
				string[] array2 = part.Textures;
				Array.Resize(ref array2, index + 1);
				part.Textures = array2;
			}
			part.Textures[index] = array[1] + "/" + array[2];
		}
	}

	public override void ChangeCategory(int c, bool forceChange)
	{
		mHighlightedItemData = null;
		if (!string.IsNullOrEmpty(mPartType))
		{
			if (mPartType == "Torso_Decal")
			{
				c = mUiCustomization._TorsoWarPaintCatID;
			}
			else if (mPartType == "Legs_Decal")
			{
				c = mUiCustomization._LegsWarPaintCatID;
			}
			else if (mPartType == "EyeLiner")
			{
				c = mUiCustomization._AvatarEyeLinerCatID;
			}
		}
		if (c == 491 && !mUiCustomization.pIsBattleReady)
		{
			mUiCustomization.SetCustomizeBtnVisiblity(CanShowCustomization(c, checkForBattleReady: true));
		}
		else
		{
			mUiCustomization.SetCustomizeBtnVisiblity(isVisible: false);
		}
		base.ChangeCategory(c, forceChange: true);
	}

	public bool CanShowCustomization(int category, bool checkForBattleReady)
	{
		if (CommonInventoryData.pInstance != null)
		{
			UserItemData[] items = CommonInventoryData.pInstance.GetItems(category);
			if (items != null)
			{
				if (!checkForBattleReady && items.Length != 0)
				{
					return true;
				}
				UserItemData[] array = items;
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].pIsBattleReady)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public override void ChangeCategory(string pType, bool forceChange)
	{
		if (pType == "All")
		{
			if (UserInfo.IsBirthdayWeek() && _BDayHats.Length != 0)
			{
				_LoadItemIdList = _BDayHats;
			}
			else
			{
				_LoadItemIdList = null;
			}
			mPartType = pType;
			int[] allCategories = mUiCustomization.GetAllCategories();
			mUiCustomization.SetCustomizeBtnVisiblity(isVisible: false);
			base.ChangeCategory(allCategories, forceChange);
		}
		else
		{
			base.ChangeCategory(pType, forceChange);
		}
	}

	public override void ChangeCategory(int[] categories, bool forceChange)
	{
		mUiCustomization.UpdateSlotsInfo();
		if (mUiCustomization.pPrevSelectedTabIndex != mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() && mUiCustomization.pKAUiSelectTabMenu.pSelectedTab != null)
		{
			string tabID = mUiCustomization.pKAUiSelectTabMenu.pSelectedTab._TabID;
			InventoryTab[] inventoryTabList = mUiCustomization._InventoryTabList;
			if (tabID == inventoryTabList[mUiCustomization.AvatarTabIndex]._TabID)
			{
				categories = new int[1] { mUiCustomization.pKAUiSelectTabMenu.pSelectedTab.pTabData._Categories[0] };
				_CurrentTab = Array.Find(mUiCustomization._Tabs, (AvatarPartTab x) => x._PrtTypeName == mUiCustomization._AvatarPartCategory[0]._PartName);
			}
			else
			{
				_CurrentTab = Array.Find(mUiCustomization._Tabs, (AvatarPartTab x) => x._PrtTypeName == mUiCustomization._ClothingPartCategory[0]._PartName);
			}
			mUiCustomization.SetCustomizeBtnVisiblity(isVisible: false);
		}
		base.ChangeCategory(categories, forceChange);
	}

	public void OnCloseCustomizeItem(KAUISelectItemData inItem)
	{
		if (_CurrentTab != null && !string.IsNullOrEmpty(_CurrentTab._PrtTypeName))
		{
			ChangeCategory(_CurrentTab._PrtTypeName, forceChange: true);
		}
		else
		{
			base.ChangeCategory(base.pCategoryID, forceChange: true);
		}
		mUiCustomization._AvatarCamera.SetActive(value: true);
		if (UiJournal.pInstance != null)
		{
			UiJournal.pInstance.SetVisibility(inVisible: true);
		}
		mModified = true;
		mUiCustomization.SetVisibility(inVisible: true);
		SetVisibility(t: true);
		if (mCustomizeUIlist != null && mCustomizeUIlist.Count > 0)
		{
			foreach (KAWidget item in mCustomizeUIlist)
			{
				item.SetVisibility(inVisible: true);
			}
		}
		mUiCustomization.pCustomAvatar.mIsDirty = true;
		if (mUiCustomization.pUiJournalCustomization != null)
		{
			mUiCustomization.pUiJournalCustomization.SetBlacksmithButtonVisibility(visible: true);
			KAWidget kAWidget = mUiCustomization.pUiJournalCustomization.FindItem("DragonBtn");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(mDragonBtnVisibility);
			}
		}
		EquippedSlotWidget equippedSlotWidget = Array.Find(_EquippedSlots, (EquippedSlotWidget x) => x._PartType.Equals(AvatarData.pPartSettings.AVATAR_PART_BACK));
		if (equippedSlotWidget != null && equippedSlotWidget.pPartUiid > 0 && equippedSlotWidget.pEquippedItemData != null && equippedSlotWidget.pEquippedItemData.AssetName != "NULL" && !string.IsNullOrEmpty(equippedSlotWidget.pEquippedItemData.AssetName) && equippedSlotWidget.pEquippedUserItemData != null && !equippedSlotWidget.pEquippedUserItemData.pIsBattleReady)
		{
			UiAvatarItemCustomization.SetThumbnail(equippedSlotWidget._WidgetSlot, equippedSlotWidget.pPartUiid);
		}
		if (mSelectedItemData != null)
		{
			ApplyCustomization(AvatarData.GetPartName(mSelectedItemData._ItemData), applyCustomization: true);
		}
	}

	public void DisableCustomizeUI()
	{
		SetVisibility(t: false);
		mUiCustomization._AvatarCamera.SetActive(value: false);
		if (UiJournal.pInstance != null)
		{
			UiJournal.pInstance.SetVisibility(inVisible: false);
		}
		mUiCustomization.SetVisibility(inVisible: false);
		if (mUiCustomization.pUiJournalCustomization != null)
		{
			KAWidget kAWidget = mUiCustomization.pUiJournalCustomization.FindItem("DragonBtn");
			if (kAWidget != null)
			{
				mDragonBtnVisibility = kAWidget.GetVisibility();
				kAWidget.SetVisibility(inVisible: false);
			}
			KAWidget kAWidget2 = mUiCustomization.pUiJournalCustomization.FindItem("BtnAmAvatarScrollLt");
			if (kAWidget2 != null)
			{
				mCustomizeUIlist.Add(kAWidget2);
			}
			KAWidget kAWidget3 = mUiCustomization.pUiJournalCustomization.FindItem("BtnAmAvatarScrollRt");
			if (kAWidget3 != null)
			{
				mCustomizeUIlist.Add(kAWidget3);
			}
		}
		if (mCustomizeUIlist == null || mCustomizeUIlist.Count <= 0)
		{
			return;
		}
		foreach (KAWidget item in mCustomizeUIlist)
		{
			item.SetVisibility(inVisible: false);
		}
	}

	public void LoadItemCustomizationUI()
	{
		KAUISelectItemData selectedItemData = mSelectedItemData;
		int customizableCategory = base.pCategoryID;
		if (mHighlightedItemData != null && mHighlightedItemData._ItemData != null)
		{
			customizableCategory = ItemCustomizationSettings.pInstance.GetCustomizableCategory(mHighlightedItemData._ItemData);
			selectedItemData = mHighlightedItemData;
		}
		UiAvatarItemCustomization.Init(CommonInventoryData.pInstance.GetItems(customizableCategory), selectedItemData, OnCloseCustomizeItem, multiItemCustomizationUI: true);
	}

	public override void UpdateWidget(KAUISelectItemData id)
	{
		if (id != null && id._ItemData.HasCategory(491) && id._ItemData.AssetName != "NULL" && !string.IsNullOrEmpty(id._ItemData.AssetName) && !id._IsBattleReady)
		{
			UiAvatarItemCustomization.SetThumbnail(id.GetItem(), id._UserInventoryID);
		}
		else
		{
			base.UpdateWidget(id);
		}
		if (id != null)
		{
			bool show = id._UserItemData != null && id._UserItemData.pIsBattleReady;
			ShowBattleReadyIcon(id.GetItem(), show);
			ShowFlightReadyIcon(id.GetItem(), id._ItemData != null && id._ItemData.HasAttribute("FlightSuit"));
			UpdateWidgetBackground(id.GetItem(), id._ItemData, id._UserItemData != null && id._UserItemData.pIsBattleReady);
			KAWidget kAWidget = id.GetItem().FindChildItem(_DisabledWidgetName);
			if (kAWidget != null)
			{
				id.pGreyMaskWidget = kAWidget;
			}
		}
	}

	private void ShowBattleReadyIcon(KAWidget widget, bool show)
	{
		if (!(widget == null))
		{
			KAWidget kAWidget = widget.FindChildItem("BattleReadyIcon");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(show);
			}
		}
	}

	private void ShowFlightReadyIcon(KAWidget widget, bool show)
	{
		if (!(widget == null))
		{
			KAWidget kAWidget = widget.FindChildItem("FlightReadyIcon");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(show);
			}
		}
	}

	private void UpdateWidgetBackground(KAWidget widget, ItemData itemData, bool isBattleReady)
	{
		if (!(widget == null))
		{
			KAWidget widget2 = widget.FindChildItem(_ItemColorWidget);
			UpdateWidgetBackgroundColor(widget2, itemData, isBattleReady);
		}
	}

	private void UpdateWidgetBackgroundColor(KAWidget widget, ItemData itemData, bool isBattleReady)
	{
		if (widget != null)
		{
			if (isBattleReady)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((itemData == null || !itemData.ItemRarity.HasValue) ? ItemRarity.Common : itemData.ItemRarity.Value, widget);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, widget);
			}
		}
	}

	public override void OnDoubleClick(KAWidget inWidget)
	{
		if (!(inWidget == null))
		{
			base.OnDoubleClick(inWidget);
			StartCoroutine(PreLoadItems(inWidget));
			mShowStats = false;
			StopCoroutine("CheckAndShowStatsDB");
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_CurrentTab._OneClickEquip)
		{
			StartCoroutine(PreLoadItems(inWidget));
		}
		else if (!mShowStats)
		{
			StartCoroutine("CheckAndShowStatsDB", inWidget);
		}
		mHighlightedItemData = (KAUISelectItemData)inWidget.GetUserData();
		mUiCustomization.SetCustomizeBtnVisiblity(CustomizeItem(mHighlightedItemData));
	}

	private bool CustomizeItem(KAUISelectItemData widgetData)
	{
		if (widgetData != null && widgetData._ItemData != null && widgetData._ItemData.HasCategory(657))
		{
			return true;
		}
		return false;
	}

	public IEnumerator CheckAndShowStatsDB(KAWidget inWidget)
	{
		mShowStats = true;
		float waitTime = 0.4f;
		float deltaTime = 0f;
		while (deltaTime < waitTime && (UtPlatform.IsTouchInput() || UICamera.hoveredObject == inWidget.gameObject))
		{
			deltaTime += Time.deltaTime;
			yield return null;
		}
		if (mShowStats)
		{
			OpenStatsDB(inWidget);
			mShowStats = false;
		}
	}

	private void OpenStatsDB(KAWidget inWidget)
	{
		EquippedSlotWidget equippedSlotWidget = null;
		if (mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.AvatarTabIndex)
		{
			return;
		}
		equippedSlotWidget = Array.Find(_EquippedSlots, (EquippedSlotWidget x) => x._WidgetSlot == inWidget);
		bool flag = false;
		bool showSellBtn = false;
		bool flag2 = false;
		string empty = string.Empty;
		if (equippedSlotWidget == null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
			if (kAUISelectItemData._ItemData == null || kAUISelectItemData._ItemData.AssetName == null)
			{
				return;
			}
			empty = AvatarData.pInstanceInfo.PartType(kAUISelectItemData._ItemData);
			showSellBtn = kAUISelectItemData._IsBattleReady;
		}
		else
		{
			empty = equippedSlotWidget._PartType;
		}
		flag = CanUnequipFromPart(empty) && equippedSlotWidget != null && equippedSlotWidget.pEquippedItemData != null;
		flag2 = equippedSlotWidget == null;
		mUiStatisticsDB.OpenStatsDB(inWidget, _EquippedSlots, base.gameObject, showDBRight: false, flag2, flag, showSellBtn, showDiscardBtn: false, empty);
	}

	private bool CanUnequipFromPart(string partName)
	{
		if (partName == AvatarData.pPartSettings.AVATAR_PART_HAT || partName == AvatarData.pPartSettings.AVATAR_PART_FACEMASK || partName == AvatarData.pPartSettings.AVATAR_PART_WRISTBAND || partName == AvatarData.pPartSettings.AVATAR_PART_SHOULDERPAD || partName == AvatarData.pPartSettings.AVATAR_PART_BACK || partName == AvatarData.pPartSettings.AVATAR_PART_WING || partName == AvatarData.pPartSettings.AVATAR_PART_HAND_PROP_RIGHT)
		{
			return true;
		}
		return false;
	}

	private ItemStat[] GetItemStatsFromUserItemData(UserItemData userItemData)
	{
		return userItemData?.ItemStats;
	}

	private ItemTier? GetItemTierFromUserItemData(UserItemData userItemData)
	{
		return userItemData?.ItemTier;
	}

	private void SetUserDataForEquippedSlots(string partType, KAUISelectItemData id)
	{
		if (!(partType == ""))
		{
			if (partType == "All")
			{
				partType = AvatarData.GetItemPartType(id._ItemData);
			}
			Array.Find(_EquippedSlots, (EquippedSlotWidget x) => x._PartType.Equals(partType))?._WidgetSlot.SetUserData(id);
			mUnEquipItemsData.Add(partType, id);
		}
	}

	public override void OnStoreLoaded(StoreData sd)
	{
		if (mInitEquipSlots)
		{
			mInitEquipSlots = false;
			mUnEquipItemsData.Clear();
			for (int i = 0; i < _EquippedSlots.Length; i++)
			{
				UpdateEquippedSlotsInfo(sd, _EquippedSlots[i]._PartType, _EquippedSlots[i]);
				if (_EquippedSlots[i].pEquippedItemData == null && AvatarData.IsDefaultSaved(_EquippedSlots[i]._PartType))
				{
					UpdateEquippedSlotsInfo(sd, "DEFAULT_" + _EquippedSlots[i]._PartType, _EquippedSlots[i]);
				}
				StoreCategoryData storeCategoryData = sd.FindCategoryData(AvatarData.GetCategoryID(_EquippedSlots[i]._PartType));
				if (storeCategoryData == null || storeCategoryData._Items == null)
				{
					continue;
				}
				foreach (ItemData item in storeCategoryData._Items)
				{
					if (item.AssetName == "NULL")
					{
						SetUserDataForEquippedSlots(_EquippedSlots[i]._PartType, new KAUISelectItemData(this, item, _WHSize, 1));
						break;
					}
				}
			}
			for (int j = 0; j < mEquippedAvatarTabInfo.Length; j++)
			{
				UpdateEquippedSlotsInfo(sd, mEquippedAvatarTabInfo[j]._PartType, mEquippedAvatarTabInfo[j]);
				if (mEquippedAvatarTabInfo[j].pEquippedItemData == null && AvatarData.IsDefaultSaved(mEquippedAvatarTabInfo[j]._PartType))
				{
					UpdateEquippedSlotsInfo(sd, "DEFAULT_" + mEquippedAvatarTabInfo[j]._PartType, mEquippedAvatarTabInfo[j]);
				}
				StoreCategoryData storeCategoryData2 = sd.FindCategoryData(AvatarData.GetCategoryID(mEquippedAvatarTabInfo[j]._PartType));
				if (storeCategoryData2 == null || storeCategoryData2._Items == null)
				{
					continue;
				}
				foreach (ItemData item2 in storeCategoryData2._Items)
				{
					if (item2.AssetName == "NULL")
					{
						SetUserDataForEquippedSlots(mEquippedAvatarTabInfo[j]._PartType, new KAUISelectItemData(this, item2, _WHSize, 1));
						break;
					}
				}
			}
		}
		base.OnStoreLoaded(sd);
		EnableAllButEquippedItems();
		UpdateEquippedSlotsDisplayInfo();
		mUiCustomization.UpdateSlotsToggleButtons();
	}

	public override void AddInvMenuItem(ItemData item, int quantity)
	{
		if (!(item.AssetName == "NULL") && (!mUiCustomization.pIsBattleReady || item.ItemStatsMap != null) && (mUiCustomization.pIsBattleReady || item.ItemStatsMap == null))
		{
			base.AddInvMenuItem(item, quantity);
		}
	}

	public override void AddInvMenuItem(UserItemData item, int quantity)
	{
		if (!(item.Item.AssetName == "NULL") && (!mUiCustomization.pIsBattleReady || item.pIsBattleReady) && (mUiCustomization.pIsBattleReady || !item.pIsBattleReady))
		{
			base.AddInvMenuItem(item, quantity);
		}
	}

	private void UpdateEquippedSlotsInfo(StoreData sd, string avatarPartName, EquippedSlotWidget equipSlotwidget)
	{
		AvatarDataPart avatarDataPart = AvatarData.FindPart(avatarPartName);
		if (avatarDataPart == null || (avatarDataPart.Textures.Length == 0 && !avatarDataPart.UserInventoryId.HasValue))
		{
			return;
		}
		int categoryID = AvatarData.GetCategoryID(equipSlotwidget._PartType);
		UserItemData userItemData = null;
		if (CommonInventoryData.pIsReady)
		{
			if (!avatarDataPart.UserInventoryId.HasValue || avatarDataPart.UserInventoryId.Value <= 0)
			{
				userItemData = CommonInventoryData.pInstance.GetUserItemDataFromGeometryAndTexture(avatarDataPart.Geometries, avatarDataPart.Textures, categoryID);
				if (userItemData != null)
				{
					avatarDataPart.UserInventoryId = userItemData.UserInventoryID;
				}
			}
			else
			{
				userItemData = CommonInventoryData.pInstance.FindItemByUserInventoryID(avatarDataPart.UserInventoryId.Value);
			}
			if (userItemData != null)
			{
				equipSlotwidget.pEquippedItemData = userItemData.Item;
				equipSlotwidget.pEquippedUserItemData = userItemData;
				equipSlotwidget.pPartUiid = userItemData.UserInventoryID;
				if (avatarPartName == AvatarData.pPartSettings.AVATAR_PART_WING && mUiCustomization.pToggleSuit != null)
				{
					mUiCustomization.pToggleSuit.SetUserData(new KAUISelectItemData(this, userItemData, _WHSize, 1));
				}
			}
		}
		if (userItemData == null)
		{
			if (avatarPartName == AvatarData.pPartSettings.AVATAR_PART_WING)
			{
				equipSlotwidget.pEquippedItemData = null;
				AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
				if (component != null && AvatarData.pInstanceInfo.FlightSuitEquipped() && mUiCustomization.pToggleSuit != null)
				{
					if (component.GetMemberFlightSuit() != null)
					{
						equipSlotwidget.pEquippedItemData = component.GetMemberFlightSuit();
						mUiCustomization.pToggleSuit.SetUserData(new KAUISelectItemData(this, component.GetMemberFlightSuit(), _WHSize, 1));
					}
					else
					{
						mUnEquipMemberSuit = true;
					}
				}
			}
			else if (avatarDataPart.Textures.Length != 0)
			{
				equipSlotwidget.pEquippedItemData = ((avatarDataPart.Textures[0] != null) ? sd.FindItem(avatarDataPart.Textures[0], categoryID) : null);
			}
			equipSlotwidget.pEquippedUserItemData = null;
			equipSlotwidget.pPartUiid = -1;
		}
		if (!(equipSlotwidget._WidgetSlot != null))
		{
			return;
		}
		if (equipSlotwidget.pEquippedItemData != null)
		{
			if (avatarPartName.Contains(AvatarData.pPartSettings.AVATAR_PART_BACK) && userItemData != null && userItemData.Item != null && userItemData.Item.HasCategory(491) && !userItemData.pIsBattleReady && userItemData.Item.AssetName != "NULL" && !string.IsNullOrEmpty(userItemData.Item.AssetName))
			{
				UiAvatarItemCustomization.SetThumbnail(equipSlotwidget._WidgetSlot, userItemData.UserInventoryID);
			}
			else
			{
				equipSlotwidget._WidgetSlot.SetTextureFromBundle(equipSlotwidget.pEquippedItemData.IconName);
			}
			EnableEquippedSlots(equipSlotwidget, enable: true);
		}
		else
		{
			equipSlotwidget._WidgetSlot.SetTexture(equipSlotwidget._DefaultTexture);
		}
	}

	private void EnableEquippedSlots(EquippedSlotWidget slot, bool enable)
	{
		slot._WidgetSlot._HoverInfo._ScaleInfo._UseScaleEffect = enable;
		slot._WidgetSlot._HoverInfo._ColorInfo._UseColorEffect = enable;
	}

	private void OnSellClicked(KAWidget widget)
	{
		if (!(widget != null))
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)widget.GetUserData();
		if (!IsTaskComplete(kAUISelectItemData))
		{
			ShowTaskItemPopUp();
			return;
		}
		mSellItemWidget = widget;
		if (kAUISelectItemData != null && kAUISelectItemData._UserItemData != null)
		{
			mUiItemSellInfoDB.Initialize(kAUISelectItemData._ItemData, base.gameObject, "OnSellItem");
		}
	}

	private void OnSellItem()
	{
		if (mSellItemWidget != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)mSellItemWidget.GetUserData();
			KAUICursorManager.SetDefaultCursor("Loading");
			CommonInventoryData.pInstance.AddSellItem(kAUISelectItemData._UserInventoryID, 1);
			CommonInventoryData.pInstance.DoSell(OnItemsSold);
		}
	}

	private void OnItemsSold(bool isSuccess, CommonInventoryResponse ret)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (isSuccess && mSellItemWidget != null)
		{
			RemoveWidget(mSellItemWidget);
		}
	}

	public override void OnDragStart(KAWidget inWidget)
	{
		if (!_AllowItemDrag)
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (kAUISelectItemData == null || kAUISelectItemData.pEquippedItem || kAUISelectItemData == null || kAUISelectItemData._ItemData == null || kAUISelectItemData._ItemData.ItemID == 0 || (!(KAUIManager.pInstance.pDragItem == null) && KAUIManager.pInstance.pDragItem.pAttachToCursor))
		{
			return;
		}
		KAWidget kAWidget = CreateDragObject(inWidget, base.pPanel.depth + 1);
		if (_DisableDragWidgetList != null)
		{
			foreach (string disableDragWidget in _DisableDragWidgetList)
			{
				KAWidget kAWidget2 = kAWidget.FindChildItem(disableDragWidget);
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
			}
		}
		kAUISelectItemData.ShowLoadingItem(inShow: false);
	}

	public override void OnDragEnd(KAWidget sourceWidget)
	{
		if (!_AllowItemDrag || KAUIManager.pInstance.pDragItem == null)
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)KAUIManager.pInstance.pDragItem.GetUserData();
		sourceWidget.SetUserData(kAUISelectItemData);
		GameObject hoveredObject = UICamera.hoveredObject;
		if (hoveredObject != null && kAUISelectItemData != null && kAUISelectItemData._ItemData != null)
		{
			KAWidget hoveredWidget = hoveredObject.GetComponent<KAWidget>();
			if (hoveredWidget != null && (mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.BattleReadyTabIndex || mUiCustomization.pKAUiSelectTabMenu.GetSelectedItemIndex() == mUiCustomization.ClothesTabIndex))
			{
				if (hoveredWidget.name == mUiCustomization._DragWidgetName && !ItemLocked(kAUISelectItemData))
				{
					OnEquipClicked(sourceWidget);
				}
				else
				{
					EquippedSlotWidget equippedSlotWidget = Array.Find(_EquippedSlots, (EquippedSlotWidget x) => x._WidgetSlot == hoveredWidget);
					if (equippedSlotWidget != null && equippedSlotWidget._PartType.Equals(AvatarData.GetPartName(kAUISelectItemData._ItemData)))
					{
						bool flag = false;
						if (equippedSlotWidget.pPartUiid >= 0 && kAUISelectItemData._UserItemData != null)
						{
							if (equippedSlotWidget.pPartUiid != kAUISelectItemData._UserItemData.UserInventoryID)
							{
								flag = true;
							}
						}
						else if (equippedSlotWidget.pEquippedItemData != kAUISelectItemData._ItemData)
						{
							flag = true;
						}
						if (flag && !ItemLocked(kAUISelectItemData))
						{
							OnEquipClicked(sourceWidget);
						}
					}
				}
			}
		}
		MoveCursorItemBack(sourceWidget);
	}

	protected override void MoveCursorItemBack(KAWidget sourceWidget)
	{
		DestroyDragItem();
	}

	public void ToggleSuit()
	{
		if (!mIsPreLoadingItems)
		{
			mModified = true;
			EquippedSlotWidget equippedSlotWidget = Array.Find(_EquippedSlots, (EquippedSlotWidget x) => x._PartType == AvatarData.pPartSettings.AVATAR_PART_WING);
			if (equippedSlotWidget == null || (equippedSlotWidget != null && equippedSlotWidget.pEquippedItemData == null))
			{
				bool partVisibility = AvatarData.pInstanceInfo.GetPartVisibility(AvatarData.pInstanceInfo.mAvatar.name, AvatarData.pPartSettings.AVATAR_PART_WING);
				AvatarData.pInstanceInfo.UpdatePartVisibility(AvatarData.pPartSettings.AVATAR_PART_WING, !partVisibility);
				SetSuitToggleChecked(!partVisibility);
			}
			else if (AvatarData.pInstanceInfo.SuitEquipped())
			{
				AvatarData.pInstanceInfo.UpdatePartVisibility(AvatarData.pPartSettings.AVATAR_PART_WING, visible: false);
				mUiCustomization.pCustomAvatar.ToAvatarData(AvatarData.pInstanceInfo);
				mUiCustomization.pAvatarCustomization.RestoreAvatar(restoreAllTextures: false);
				SetSuitToggleChecked(setChecked: false);
			}
			else if (mUiCustomization.pToggleSuit != null)
			{
				AvatarData.pInstanceInfo.UpdatePartVisibility(AvatarData.pPartSettings.AVATAR_PART_WING, visible: true);
				StartCoroutine(PreLoadItems(mUiCustomization.pToggleSuit));
				SetSuitToggleChecked(setChecked: true);
			}
		}
	}

	private void SetSuitToggleChecked(bool setChecked)
	{
		if (!mIsPreLoadingItems && mUiCustomization.pToggleSuit != null)
		{
			mUiCustomization.pToggleSuit.SetChecked(setChecked);
		}
	}

	public void EnableEquippedSlots(bool enable)
	{
		EquippedSlotWidget[] equippedSlots = _EquippedSlots;
		foreach (EquippedSlotWidget equippedSlotWidget in equippedSlots)
		{
			if (equippedSlotWidget._WidgetSlot != null)
			{
				equippedSlotWidget._WidgetSlot.SetVisibility(enable);
			}
		}
	}

	private void ListAllRelatedParts(ItemData inItemData)
	{
		if (mRelatedParts == null)
		{
			mRelatedParts = new List<string>();
		}
		else
		{
			mRelatedParts.Clear();
		}
		if (inItemData.Relationship != null && Array.Exists(inItemData.Relationship, (ItemDataRelationship r) => r.Type.Equals("GroupParent")))
		{
			ItemDataRelationship[] array = Array.FindAll(inItemData.Relationship, (ItemDataRelationship r) => r.Type.Equals("GroupParent"));
			for (int i = 0; i < array.Length; i++)
			{
				ItemData.Load(array[i].ItemId, OnItemDataReady, null);
			}
		}
		mRelatedParts.Add(AvatarData.GetItemPartType(inItemData));
	}

	protected void OnItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		string itemPartType = AvatarData.GetItemPartType(dataItem);
		if (mRelatedParts != null)
		{
			mRelatedParts.Add(itemPartType);
		}
	}

	private bool CheckIfRelationalPartExists(ItemData itemData)
	{
		ListAllRelatedParts(itemData);
		if (mRelatedParts != null && mRelatedParts.Count > 0 && mUiCustomization.pCustomAvatar != null)
		{
			foreach (string mRelatedPart in mRelatedParts)
			{
				if (AvatarData.IsDefaultSaved(mRelatedPart))
				{
					return true;
				}
			}
		}
		return false;
	}
}
