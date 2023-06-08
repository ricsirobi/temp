using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot(ElementName = "I", Namespace = "", IsNullable = true)]
public class ItemData
{
	public class ItemSale
	{
		public float mModifier;

		public DateTime mEndDate = DateTime.MaxValue;

		public string mIcon = string.Empty;
	}

	public class ItemDataUserData
	{
		public int mID;

		public ItemDataEventHandler mCallback;

		public object mUserData;
	}

	[XmlElement(ElementName = "an")]
	public string AssetName;

	[XmlElement(ElementName = "at", IsNullable = true)]
	public ItemAttribute[] Attribute;

	[XmlElement(ElementName = "c")]
	public ItemDataCategory[] Category;

	[XmlElement(ElementName = "ct")]
	public int Cost;

	[XmlElement(ElementName = "ct2")]
	public int CashCost;

	[XmlElement(ElementName = "cp")]
	public int CreativePoints;

	[XmlElement(ElementName = "d")]
	public string Description;

	[XmlElement(ElementName = "icn")]
	public string IconName;

	[XmlElement(ElementName = "im")]
	public int InventoryMax;

	[XmlElement(ElementName = "id")]
	public int ItemID;

	[XmlElement(ElementName = "itn")]
	public string ItemName;

	[XmlElement(ElementName = "itnp")]
	public string ItemNamePlural;

	[XmlElement(ElementName = "l")]
	public bool Locked;

	[XmlElement(ElementName = "g", IsNullable = true)]
	public string Geometry2;

	[XmlElement(ElementName = "ro", IsNullable = true)]
	public ItemDataRollover Rollover;

	[XmlElement(ElementName = "rid", IsNullable = true)]
	public int? RankId;

	[XmlElement(ElementName = "r")]
	public ItemDataRelationship[] Relationship;

	[XmlElement(ElementName = "s")]
	public bool Stackable;

	[XmlElement(ElementName = "as")]
	public bool AllowStacking;

	[XmlElement(ElementName = "sf")]
	public int SaleFactor;

	[XmlElement(ElementName = "t")]
	public ItemDataTexture[] Texture;

	[XmlElement(ElementName = "u")]
	public int Uses;

	[XmlElement(ElementName = "av")]
	public ItemAvailability[] Availability;

	[XmlElement(ElementName = "rtid")]
	public int RewardTypeID;

	[XmlElement(ElementName = "p", IsNullable = true)]
	public int? Points;

	private static Dictionary<int, ItemData> mItemDataCache = new Dictionary<int, ItemData>();

	private static List<ItemDataUserData> mLoadingList = new List<ItemDataUserData>();

	private RsResourceLoadEvent mLoadBundleItemsState;

	private ItemDataListEventHandler mLoadBundleItemCallback;

	private List<ItemData> mBundledItems;

	[XmlIgnore]
	public bool IsNew;

	[XmlIgnore]
	public int PopularRank = -1;

	[XmlIgnore]
	public List<ItemsInStoreDataSale> SaleList;

	[XmlIgnore]
	public List<ItemsInStoreDataSale> MemberSaleList;

	[XmlIgnore]
	private bool mSetSaleCheat;

	[XmlElement(ElementName = "is")]
	public List<ItemState> ItemStates { get; set; }

	[XmlElement(ElementName = "ir", IsNullable = true)]
	public ItemRarity? ItemRarity { get; set; }

	[XmlElement(ElementName = "ipsm", IsNullable = true)]
	public ItemPossibleStatsMap PossibleStatsMap { get; set; }

	[XmlElement(ElementName = "ism", IsNullable = true)]
	public ItemStatsMap ItemStatsMap { get; set; }

	[XmlElement(ElementName = "iscs", IsNullable = true)]
	public ItemSaleConfig[] ItemSaleConfigs { get; set; }

	[XmlElement(ElementName = "bp", IsNullable = true)]
	public BluePrint BluePrint { get; set; }

	public RsResourceLoadEvent pLoadBundleItemsState => mLoadBundleItemsState;

	public List<ItemData> pBundledItems => mBundledItems;

	public static List<ItemDataUserData> pLoadingList => mLoadingList;

	[XmlIgnore]
	public bool pIsPopular => PopularRank > 0;

	[XmlIgnore]
	public int pMemberCost
	{
		get
		{
			ItemSale memberSale = GetMemberSale();
			if (memberSale != null)
			{
				return (int)Math.Round((float)Cost * memberSale.mModifier, MidpointRounding.AwayFromZero);
			}
			return Cost;
		}
	}

	[XmlIgnore]
	public int pFinalMemberCost
	{
		get
		{
			ItemSale sale = GetSale(MemberSaleList, SaleList);
			if (sale != null)
			{
				return (int)Math.Round((float)Cost * sale.mModifier, MidpointRounding.AwayFromZero);
			}
			return Cost;
		}
	}

	[XmlIgnore]
	public int pNonMemberCost => Cost;

	[XmlIgnore]
	public int pFinalNonMemberCost
	{
		get
		{
			ItemSale currentSale = GetCurrentSale();
			if (currentSale != null)
			{
				return (int)Math.Round((float)Cost * currentSale.mModifier, MidpointRounding.AwayFromZero);
			}
			return Cost;
		}
	}

	[XmlIgnore]
	public int pMemberCashCost
	{
		get
		{
			ItemSale memberSale = GetMemberSale();
			if (memberSale != null)
			{
				return (int)Math.Round((float)CashCost * memberSale.mModifier, MidpointRounding.AwayFromZero);
			}
			return CashCost;
		}
	}

	[XmlIgnore]
	public int pFinalMemberCashCost
	{
		get
		{
			ItemSale sale = GetSale(MemberSaleList, SaleList);
			if (sale != null)
			{
				return (int)Math.Round((float)CashCost * sale.mModifier, MidpointRounding.AwayFromZero);
			}
			return CashCost;
		}
	}

	[XmlIgnore]
	public int pNonMemberCashCost => CashCost;

	[XmlIgnore]
	public int pFinalNonMemberCashCost
	{
		get
		{
			ItemSale currentSale = GetCurrentSale();
			if (currentSale != null)
			{
				return (int)Math.Round((float)CashCost * currentSale.mModifier, MidpointRounding.AwayFromZero);
			}
			return CashCost;
		}
	}

	[XmlIgnore]
	public int FinalCost
	{
		get
		{
			if (SubscriptionInfo.pIsMember)
			{
				return pFinalMemberCost;
			}
			return pFinalNonMemberCost;
		}
	}

	[XmlIgnore]
	public int FinalCashCost
	{
		get
		{
			if (SubscriptionInfo.pIsMember)
			{
				return pFinalMemberCashCost;
			}
			return pFinalNonMemberCashCost;
		}
	}

	[XmlIgnore]
	public string pSaleIcon
	{
		get
		{
			ItemSale currentSale = GetCurrentSale();
			if (currentSale == null)
			{
				return string.Empty;
			}
			return currentSale.mIcon;
		}
	}

	public static void AddToCache(ItemData idata)
	{
		if (idata != null && !mItemDataCache.ContainsKey(idata.ItemID))
		{
			mItemDataCache[idata.ItemID] = idata;
		}
	}

	public static void AddToLoadingList(int itemID, ItemDataEventHandler callback, object inUserData)
	{
		ItemDataUserData itemDataUserData = new ItemDataUserData();
		itemDataUserData.mID = itemID;
		itemDataUserData.mUserData = inUserData;
		itemDataUserData.mCallback = callback;
		bool flag = false;
		if (mLoadingList.Count > 0)
		{
			foreach (ItemDataUserData mLoading in mLoadingList)
			{
				if (mLoading != null && mLoading.mID == itemID)
				{
					flag = true;
				}
			}
		}
		mLoadingList.Add(itemDataUserData);
		if (!flag)
		{
			WsWebService.GetItemData(itemID, ServiceEventHandler, itemID);
		}
	}

	public static void Load(int itemID, ItemDataEventHandler inCallback, object inUserData)
	{
		if (mItemDataCache.ContainsKey(itemID))
		{
			inCallback?.Invoke(itemID, mItemDataCache[itemID], inUserData);
		}
		else
		{
			AddToLoadingList(itemID, inCallback, inUserData);
		}
	}

	public void LoadBundledItems(ItemDataListEventHandler inCallback)
	{
		if (inCallback != null)
		{
			mLoadBundleItemCallback = (ItemDataListEventHandler)Delegate.Combine(mLoadBundleItemCallback, inCallback);
		}
		switch (mLoadBundleItemsState)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (mLoadBundleItemCallback != null)
			{
				mLoadBundleItemCallback(mBundledItems, ItemID);
				mLoadBundleItemCallback = null;
			}
			break;
		case RsResourceLoadEvent.ERROR:
			if (mLoadBundleItemCallback != null)
			{
				mLoadBundleItemCallback(null, ItemID);
				mLoadBundleItemCallback = null;
			}
			break;
		case RsResourceLoadEvent.NONE:
		{
			mLoadBundleItemsState = RsResourceLoadEvent.PROGRESS;
			List<int> bundledItems = GetBundledItems();
			if (bundledItems == null || bundledItems.Count == 0)
			{
				mLoadBundleItemsState = RsResourceLoadEvent.COMPLETE;
				if (mLoadBundleItemCallback != null)
				{
					mLoadBundleItemCallback(mBundledItems, ItemID);
					mLoadBundleItemCallback = null;
				}
				break;
			}
			{
				foreach (int item in bundledItems)
				{
					Load(item, BundledItemCallback, null);
				}
				break;
			}
		}
		case RsResourceLoadEvent.PROGRESS:
			break;
		}
	}

	private void BundledItemCallback(int inItemID, ItemData dataItem, object inUserData)
	{
		List<int> bundledItems = GetBundledItems();
		if (bundledItems == null || bundledItems.Count == 0)
		{
			mLoadBundleItemsState = RsResourceLoadEvent.COMPLETE;
			if (mLoadBundleItemCallback != null)
			{
				mLoadBundleItemCallback(mBundledItems, ItemID);
				mLoadBundleItemCallback = null;
			}
		}
		else
		{
			if (dataItem == null)
			{
				return;
			}
			if (mBundledItems == null)
			{
				mBundledItems = new List<ItemData>();
			}
			mBundledItems.Add(dataItem);
			if (mBundledItems.Count >= bundledItems.Count)
			{
				mLoadBundleItemsState = RsResourceLoadEvent.COMPLETE;
				if (mLoadBundleItemCallback != null)
				{
					mLoadBundleItemCallback(mBundledItems, ItemID);
					mLoadBundleItemCallback = null;
				}
			}
		}
	}

	private static void UpdateLoadingList(int itemID, ItemData item)
	{
		for (int num = mLoadingList.Count - 1; num >= 0; num--)
		{
			if (itemID == mLoadingList[num].mID)
			{
				ItemDataUserData itemDataUserData = mLoadingList[num];
				mLoadingList.Remove(itemDataUserData);
				if (itemDataUserData.mCallback != null)
				{
					itemDataUserData.mCallback(itemID, item, itemDataUserData.mUserData);
				}
			}
		}
		if (!mItemDataCache.ContainsKey(itemID))
		{
			mItemDataCache[itemID] = item;
		}
	}

	private void DumpData()
	{
		string text = "";
		text += "\nGET_ITEM RECEIVED:\n";
		text = text + "ItemName : " + ItemName + "\n";
		text = text + "AssetName : " + AssetName + "\n";
		text = text + "IconName : " + IconName + "\n";
		ItemDataRollover rollover = Rollover;
		if (rollover == null)
		{
			text += "NULL Rollover\n";
		}
		else
		{
			text = text + "Rollover DialogName : " + rollover.DialogName + "\n";
			text = text + "Rollover Bundle : " + rollover.Bundle + "\n";
		}
		text = text + ((Description == null) ? "NULL Description" : ("Description " + Description)) + "\n";
		text = text + ((Geometry2 == null) ? "NULL Geometry2" : ("Geometry2" + Geometry2)) + "\n";
		text = text + ((Texture == null) ? "No Texture in Item" : ("Number Of Texture in Item " + Texture.Length)) + "\n";
		if (Texture != null)
		{
			for (int i = 0; i < Texture.Length; i++)
			{
				ItemDataTexture itemDataTexture = Texture[i];
				text = text + "TextureName : " + itemDataTexture.TextureName + "\n";
				text = text + "TextureTypeName : " + itemDataTexture.TextureTypeName + "\n";
				text = text + (itemDataTexture.OffsetX.HasValue ? "NULL OffsetX" : ("OffsetX " + itemDataTexture.OffsetX.Value)) + "\n";
				text = text + (itemDataTexture.OffsetY.HasValue ? "NULL OffsetY" : ("OffsetY " + itemDataTexture.OffsetY.Value)) + "\n";
			}
		}
		text = text + ((Category == null) ? "No Category in Item" : ("Number Of Category in Item " + Category.Length)) + "\n";
		if (Category != null)
		{
			for (int j = 0; j < Category.Length; j++)
			{
				ItemDataCategory itemDataCategory = Category[j];
				text = text + "CategoryId : " + itemDataCategory.CategoryId + "\n";
				text = text + "CategoryName : " + itemDataCategory.CategoryName + "\n";
				text = text + ((itemDataCategory.IconName == null) ? "NULL IconName" : ("IconName" + itemDataCategory.IconName)) + "\n";
			}
		}
		text = text + ((Relationship == null) ? "No Relationship in Item" : ("Number Of Relationship in Item " + Relationship.Length)) + "\n";
		if (Relationship != null)
		{
			for (int k = 0; k < Relationship.Length; k++)
			{
				text = text + "Type : " + Relationship[k].Type + "\n";
				text = text + "ItemId : " + Relationship[k].ItemId + "\n";
			}
		}
		if (RankId.HasValue)
		{
			string text2 = text;
			int? rankId = RankId;
			text = text2 + "RankId : " + rankId + "\n";
		}
		text = text + "Locked : " + Locked + "\n";
		text = text + "Cost : " + Cost + "\n";
		text = text + "Uses : " + Uses + "\n";
		text = text + "InventoryMax : " + InventoryMax + "\n";
		text = text + "CreativePoints : " + CreativePoints + "\n";
		UtDebug.LogError(text);
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		int itemID = (int)inUserData;
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UpdateLoadingList(itemID, null);
			UtDebug.LogWarning("ItemData not found for ID:" + itemID);
			break;
		case WsServiceEvent.COMPLETE:
			if (inObject == null)
			{
				UtDebug.LogError("ItemData not found for ID:" + itemID);
				UpdateLoadingList(itemID, null);
			}
			else
			{
				ItemData item = (ItemData)inObject;
				UpdateLoadingList(itemID, item);
			}
			break;
		}
	}

	public ItemDataTexture FindTextureByTypeName(string typeName)
	{
		if (Texture != null)
		{
			ItemDataTexture[] texture = Texture;
			foreach (ItemDataTexture itemDataTexture in texture)
			{
				if (typeName == itemDataTexture.TextureTypeName)
				{
					return itemDataTexture;
				}
			}
		}
		return null;
	}

	public string GetTextureNameNoPath(string typeName)
	{
		ItemDataTexture itemDataTexture = FindTextureByTypeName(typeName);
		if (itemDataTexture == null)
		{
			return "";
		}
		string[] array = itemDataTexture.TextureName.Split('/');
		if (array.Length == 3)
		{
			return array[1] + "/" + array[2];
		}
		return "";
	}

	public string GetTextureName(string typeName)
	{
		ItemDataTexture itemDataTexture = FindTextureByTypeName(typeName);
		if (itemDataTexture == null)
		{
			return "";
		}
		return itemDataTexture.TextureName;
	}

	public bool IsSubPart()
	{
		if (Relationship == null)
		{
			return false;
		}
		if (Relationship.Length == 0)
		{
			return false;
		}
		return Relationship[0].Type == "GroupChild";
	}

	public bool HasCategory(int categoryID)
	{
		if (Category == null)
		{
			return false;
		}
		ItemDataCategory[] category = Category;
		for (int i = 0; i < category.Length; i++)
		{
			if (category[i].CategoryId == categoryID)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAttribute(string attribute)
	{
		if (Attribute == null)
		{
			return false;
		}
		ItemAttribute[] attribute2 = Attribute;
		for (int i = 0; i < attribute2.Length; i++)
		{
			if (attribute2[i].Key == attribute)
			{
				return true;
			}
		}
		return false;
	}

	public TYPE GetAttribute<TYPE>(string attribute, TYPE defaultValue)
	{
		if (Attribute == null)
		{
			return defaultValue;
		}
		ItemAttribute[] attribute2 = Attribute;
		foreach (ItemAttribute itemAttribute in attribute2)
		{
			if (!(itemAttribute.Key == attribute))
			{
				continue;
			}
			Type typeFromHandle = typeof(TYPE);
			if (typeFromHandle.Equals(typeof(int)))
			{
				return (TYPE)(object)int.Parse(itemAttribute.Value);
			}
			if (typeFromHandle.Equals(typeof(float)))
			{
				return (TYPE)(object)float.Parse(itemAttribute.Value);
			}
			if (typeFromHandle.Equals(typeof(bool)))
			{
				if (itemAttribute.Value.Equals("t", StringComparison.OrdinalIgnoreCase) || itemAttribute.Value.Equals("1", StringComparison.OrdinalIgnoreCase) || itemAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					return (TYPE)(object)true;
				}
				if (itemAttribute.Value.Equals("f", StringComparison.OrdinalIgnoreCase) || itemAttribute.Value.Equals("0", StringComparison.OrdinalIgnoreCase) || itemAttribute.Value.Equals("false", StringComparison.OrdinalIgnoreCase))
				{
					return (TYPE)(object)false;
				}
			}
			else
			{
				if (typeFromHandle.Equals(typeof(string)))
				{
					return (TYPE)(object)itemAttribute.Value;
				}
				if (typeFromHandle.Equals(typeof(DateTime)))
				{
					return (TYPE)(object)DateTime.Parse(itemAttribute.Value, UtUtilities.GetCultureInfo("en-US"));
				}
			}
		}
		return defaultValue;
	}

	public int GetIDFromRelationship(string inRelationship)
	{
		if (Relationship == null)
		{
			return 0;
		}
		ItemDataRelationship[] relationship = Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (itemDataRelationship.Type == inRelationship)
			{
				return itemDataRelationship.ItemId;
			}
		}
		return 0;
	}

	public ExpireData GetExpireData()
	{
		ExpireData expireData = null;
		if (HasAttribute("ExpireDate"))
		{
			expireData = new ExpireData();
			string[] array = GetAttribute("ExpireDate", "1/1/3000,0").Split(',');
			expireData.mExpireDate = DateTime.Parse(array[0], UtUtilities.GetCultureInfo("en-US"));
			expireData.mFrequency = int.Parse(array[1]);
		}
		return expireData;
	}

	private ItemSale GetMemberSale()
	{
		return GetSale(MemberSaleList);
	}

	public ItemSale GetCurrentSale()
	{
		return GetSale(SaleList);
	}

	private ItemSale GetSale(List<ItemsInStoreDataSale> inSaleList1, List<ItemsInStoreDataSale> inSaleList2)
	{
		ItemSale sale = GetSale(inSaleList1);
		ItemSale sale2 = GetSale(inSaleList2);
		if (sale == null && sale2 == null)
		{
			return null;
		}
		if (sale == null && sale2 != null)
		{
			return sale2;
		}
		if (sale != null && sale2 == null)
		{
			return sale;
		}
		sale.mModifier = Mathf.Min(1f, sale.mModifier * sale2.mModifier);
		if (DateTime.Compare(sale.mEndDate, sale2.mEndDate) > 0)
		{
			sale.mEndDate = sale2.mEndDate;
		}
		return sale;
	}

	private ItemSale GetSale(List<ItemsInStoreDataSale> inSaleList)
	{
		if (inSaleList == null || inSaleList.Count == 0)
		{
			return null;
		}
		ItemSale itemSale = null;
		foreach (ItemsInStoreDataSale inSale in inSaleList)
		{
			if (inSale == null || inSale.IsOutdated())
			{
				continue;
			}
			if (itemSale == null)
			{
				itemSale = new ItemSale();
				itemSale.mModifier = Mathf.Min(1f, 1f - inSale.Modifier);
				itemSale.mIcon = inSale.Icon;
				if (inSale.EndDate.HasValue)
				{
					itemSale.mEndDate = inSale.EndDate.Value;
				}
				continue;
			}
			float mModifier = itemSale.mModifier;
			mModifier *= 1f - inSale.Modifier;
			itemSale.mModifier = Mathf.Min(1f, mModifier);
			itemSale.mIcon = inSale.Icon;
			if (inSale.EndDate.HasValue && DateTime.Compare(itemSale.mEndDate, inSale.EndDate.Value) > 0)
			{
				itemSale.mEndDate = inSale.EndDate.Value;
			}
		}
		ItemAvailability availability = GetAvailability();
		if (itemSale != null && availability != null && availability.EndDate.HasValue && DateTime.Compare(availability.EndDate.Value, itemSale.mEndDate) < 0)
		{
			itemSale.mEndDate = availability.EndDate.Value;
		}
		return itemSale;
	}

	public void CheckNewState()
	{
		IsNew = GetAttribute("New", defaultValue: false);
	}

	public ItemAvailability GetAvailability()
	{
		if (Availability != null)
		{
			for (int i = 0; i < Availability.Length; i++)
			{
				if ((!Availability[i].StartDate.HasValue || DateTime.Compare(ServerTime.pCurrentTime, Availability[i].StartDate.Value) >= 0) && (!Availability[i].EndDate.HasValue || DateTime.Compare(ServerTime.pCurrentTime, Availability[i].EndDate.Value) < 0))
				{
					return Availability[i];
				}
			}
		}
		return null;
	}

	public bool IsOutdated()
	{
		if (Availability != null)
		{
			for (int i = 0; i < Availability.Length; i++)
			{
				bool flag = false;
				if (Availability[i].StartDate.HasValue && ServerTime.pCurrentTime < Availability[i].StartDate.Value)
				{
					flag = true;
				}
				if (Availability[i].EndDate.HasValue && ServerTime.pCurrentTime > Availability[i].EndDate.Value)
				{
					flag = true;
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public bool IsOnSale()
	{
		return GetCurrentSale() != null;
	}

	public int GetFinalCost()
	{
		if (SubscriptionInfo.pIsMember)
		{
			return GetFinalMemberCost();
		}
		return GetFinalNonMemberCost();
	}

	public int GetFinalMemberCost()
	{
		if (GetPurchaseType() == 1)
		{
			return pFinalMemberCost;
		}
		return pFinalMemberCashCost;
	}

	public int GetFinalNonMemberCost()
	{
		if (GetPurchaseType() == 1)
		{
			return pFinalNonMemberCost;
		}
		return pFinalNonMemberCashCost;
	}

	public int GetPurchaseType()
	{
		if (CashCost > 0)
		{
			return 2;
		}
		return 1;
	}

	public void AddSale(ItemsInStoreDataSale isale, bool inIsCheat = false)
	{
		if (isale == null || (mSetSaleCheat && !inIsCheat))
		{
			return;
		}
		mSetSaleCheat = inIsCheat;
		if (isale.ForMembers.HasValue && isale.ForMembers.Value)
		{
			if (MemberSaleList == null)
			{
				MemberSaleList = new List<ItemsInStoreDataSale>();
			}
			if (!inIsCheat)
			{
				foreach (ItemsInStoreDataSale memberSale in MemberSaleList)
				{
					if (memberSale != null && memberSale.PriceChangeId == isale.PriceChangeId)
					{
						return;
					}
				}
			}
			MemberSaleList.Add(isale);
			return;
		}
		if (SaleList == null)
		{
			SaleList = new List<ItemsInStoreDataSale>();
		}
		if (!inIsCheat)
		{
			foreach (ItemsInStoreDataSale sale in SaleList)
			{
				if (sale != null && sale.PriceChangeId == isale.PriceChangeId)
				{
					return;
				}
			}
		}
		SaleList.Add(isale);
	}

	public bool GroupItem()
	{
		if (Relationship == null || Relationship.Length == 0)
		{
			return false;
		}
		return Array.Find(Relationship, (ItemDataRelationship relation) => relation != null && relation.Type == "GroupParent") != null;
	}

	public bool IsBundleItem()
	{
		if (Relationship == null || Relationship.Length == 0)
		{
			return false;
		}
		ItemDataRelationship[] relationship = Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (itemDataRelationship != null && itemDataRelationship.Type == "Bundle")
			{
				return true;
			}
		}
		return false;
	}

	public List<int> GetBundledItems()
	{
		if (Relationship == null || Relationship.Length == 0)
		{
			return null;
		}
		List<int> list = null;
		ItemDataRelationship[] relationship = Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (itemDataRelationship != null && itemDataRelationship.Type == "Bundle")
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(itemDataRelationship.ItemId);
			}
		}
		return list;
	}

	public bool IsPrizeItem()
	{
		if (Relationship == null || Relationship.Length == 0)
		{
			return false;
		}
		ItemDataRelationship[] relationship = Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (itemDataRelationship != null && itemDataRelationship.Type == "Prize")
			{
				return true;
			}
		}
		return false;
	}

	public List<int> GetPrizes()
	{
		if (Relationship == null || Relationship.Length == 0)
		{
			return null;
		}
		List<int> list = null;
		ItemDataRelationship[] relationship = Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (itemDataRelationship != null && itemDataRelationship.Type == "Prize")
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(itemDataRelationship.ItemId);
			}
		}
		return list;
	}

	public List<int> GetPrizesSorted()
	{
		if (Relationship == null || Relationship.Length == 0)
		{
			return null;
		}
		List<ItemDataRelationship> list = null;
		ItemDataRelationship[] relationship = Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (itemDataRelationship != null && itemDataRelationship.Type == "Prize")
			{
				if (list == null)
				{
					list = new List<ItemDataRelationship>();
				}
				list.Add(itemDataRelationship);
			}
		}
		list.Sort(SortByWeight);
		List<int> list2 = null;
		foreach (ItemDataRelationship item in list)
		{
			if (list2 == null)
			{
				list2 = new List<int>();
			}
			list2.Add(item.ItemId);
		}
		return list2;
	}

	private static int SortByWeight(ItemDataRelationship item1, ItemDataRelationship item2)
	{
		return item1.Weight.CompareTo(item2.Weight);
	}

	public int GetRarestPrize()
	{
		if (Relationship == null || Relationship.Length == 0)
		{
			return -1;
		}
		ItemDataRelationship itemDataRelationship = null;
		ItemDataRelationship[] relationship = Relationship;
		foreach (ItemDataRelationship itemDataRelationship2 in relationship)
		{
			if (itemDataRelationship2 != null && itemDataRelationship2.Type == "Prize" && (itemDataRelationship == null || itemDataRelationship2.Weight < itemDataRelationship.Weight))
			{
				itemDataRelationship = itemDataRelationship2;
			}
		}
		return itemDataRelationship.ItemId;
	}

	public List<ItemDataRelationship> GetPrizeItemsSorted()
	{
		if (Relationship == null || Relationship.Length == 0)
		{
			return null;
		}
		List<ItemDataRelationship> list = null;
		ItemDataRelationship[] relationship = Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (itemDataRelationship != null && itemDataRelationship.Type == "Prize")
			{
				if (list == null)
				{
					list = new List<ItemDataRelationship>();
				}
				list.Add(itemDataRelationship);
			}
		}
		list.Sort(SortByWeight);
		return list;
	}

	public bool IsFree()
	{
		if (GetPurchaseType() == 1)
		{
			return FinalCost <= 0;
		}
		return FinalCashCost <= 0;
	}

	public bool IsStatAvailable()
	{
		if (ItemStatsMap != null)
		{
			return ItemStatsMap.ItemStats != null;
		}
		return false;
	}
}
