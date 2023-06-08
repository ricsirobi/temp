using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySetting : ScriptableObject
{
	[Serializable]
	public class TabData
	{
		public string _TabID;

		public int[] _Categories;

		public int _MaxNumSlots;

		public int _MemberSlots;

		public int _NonMemberSlots;

		public int _SlotItemID;

		public int _SlotItemStoreID;

		public bool _CheckGender;

		public bool _BattleReady;

		public bool _DisableEquipped;

		public LocaleString _PurchaseSlotDisplayText;

		public bool HasCategory(int categoryID)
		{
			return Array.Exists(_Categories, (int c) => c == categoryID);
		}

		public int GetTotalSlots()
		{
			return (SubscriptionInfo.pIsMember ? _MemberSlots : _NonMemberSlots) + CommonInventoryData.pInstance.GetQuantity(_SlotItemID);
		}

		public int GetOccupiedSlots()
		{
			UserItemData[] items = CommonInventoryData.pInstance.GetItems(_Categories);
			int num = 0;
			if (items != null)
			{
				UserItemData[] array = items;
				foreach (UserItemData userItem in array)
				{
					if (ValidateItem(userItem))
					{
						num++;
					}
				}
			}
			return num;
		}

		public void BuySlot(GameObject messageObject, int occupiedSlots = 0)
		{
			UiSlotPurchaseDB.ShowSlotPurchaseDB(_PurchaseSlotDisplayText.GetLocalizedString(), _SlotItemID, _SlotItemStoreID, (occupiedSlots > 0) ? occupiedSlots : GetOccupiedSlots(), GetTotalSlots(), messageObject);
		}

		public void BuySlot(GameObject messageObject, int quantity, int occupiedSlots = 0)
		{
			UiSlotPurchaseDB.ShowSlotPurchaseDB(_PurchaseSlotDisplayText.GetLocalizedString(), _SlotItemID, _SlotItemStoreID, (occupiedSlots > 0) ? occupiedSlots : GetOccupiedSlots(), GetTotalSlots(), quantity, messageObject);
		}

		public bool ValidateItem(UserItemData userItem)
		{
			if (_CheckGender && !ValidateGender(userItem.Item))
			{
				return false;
			}
			if ((_BattleReady && !userItem.pIsBattleReady) || (!_BattleReady && userItem.pIsBattleReady))
			{
				return false;
			}
			return true;
		}

		public bool ValidateItem(ItemData item)
		{
			if (_CheckGender && !ValidateGender(item))
			{
				return false;
			}
			if (_BattleReady)
			{
				return false;
			}
			return true;
		}

		private bool ValidateGender(ItemData item)
		{
			string text = "U";
			if (AvatarData.GetGender() == Gender.Male)
			{
				text = "M";
			}
			else if (AvatarData.GetGender() == Gender.Female)
			{
				text = "U";
			}
			string attribute = item.GetAttribute("Gender", "U");
			if (!(attribute == text) && !(attribute == "U"))
			{
				return text == "U";
			}
			return true;
		}
	}

	[Serializable]
	public class ItemRarityInfo
	{
		public ItemRarity _ItemRarity;

		public Color _Color = Color.white;

		public LocaleString _Text;
	}

	public List<TabData> _TabData = new List<TabData>();

	public int _GemsToCoinsFactor = 5;

	public ItemRarityInfo[] _ItemRarityInfo;

	private static InventorySetting mInstance;

	public static InventorySetting pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (InventorySetting)RsResourceManager.LoadAssetFromResources("InventorySetting.asset");
				if (mInstance == null)
				{
					mInstance = ScriptableObject.CreateInstance<InventorySetting>();
				}
			}
			return mInstance;
		}
	}

	public TabData GetTabData(string TabID)
	{
		if (pInstance._TabData.Count == 0)
		{
			Debug.LogError("InventorySetting: No data set. Please populate the necessary data.");
			return null;
		}
		TabData tabData = pInstance._TabData.Find((TabData key) => key._TabID == TabID);
		if (tabData != null)
		{
			return tabData;
		}
		Debug.LogError("Please setup " + TabID + " in InventorySetting");
		return null;
	}

	public Color GetItemRarityColor(ItemRarity itemRarity)
	{
		if (pInstance._ItemRarityInfo == null || pInstance._ItemRarityInfo.Length == 0)
		{
			Debug.LogError("InventorySetting: No data set. Please populate the necessary data.");
			return Color.white;
		}
		ItemRarityInfo itemRarityInfo = Array.Find(_ItemRarityInfo, (ItemRarityInfo info) => info._ItemRarity == itemRarity);
		if (itemRarityInfo != null)
		{
			return itemRarityInfo._Color;
		}
		Debug.LogError("Please setup " + itemRarity.ToString() + " in InventorySetting");
		return Color.white;
	}

	public string GetItemRarityText(ItemRarity itemRarity)
	{
		if (pInstance._ItemRarityInfo == null || pInstance._ItemRarityInfo.Length == 0)
		{
			Debug.LogError("InventorySetting: No data set. Please populate the necessary data.");
			return "";
		}
		ItemRarityInfo itemRarityInfo = Array.Find(_ItemRarityInfo, (ItemRarityInfo info) => info._ItemRarity == itemRarity);
		if (itemRarityInfo != null)
		{
			return itemRarityInfo._Text.GetLocalizedString();
		}
		Debug.LogError("Please setup " + itemRarity.ToString() + " in InventorySetting");
		return "";
	}
}
