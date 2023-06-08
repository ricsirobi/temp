using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using CommonInventory.V4;

[Serializable]
[XmlRoot(ElementName = "CI", Namespace = "")]
public class CommonInventoryData
{
	private class InvItemDataUserData
	{
		public int mID;

		public InventoryItemDataEventHandler mCallback;

		public object mUserData;

		public int mQuantity = 1;

		public int mUserInvID = -1;

		public CommonInventorySaveInstance mSaveInstance;
	}

	[XmlElement(ElementName = "uid")]
	public Guid UserID;

	[XmlElement(ElementName = "i")]
	public UserItemData[] Item;

	[XmlIgnore]
	private PurchaseEventHandler mPurchaseCallback;

	[XmlIgnore]
	public static InventoryEventHandler PurchaseSuccessCallback;

	[XmlIgnore]
	private RedeemItemsEventHandler mRedeemItemsCallback;

	[XmlIgnore]
	private SellEventHandler mSellCallback;

	[XmlIgnore]
	private Dictionary<int, List<UserItemData>> mInventory = new Dictionary<int, List<UserItemData>>();

	[XmlIgnore]
	private List<CommonInventoryRequest> mUpdateList;

	[XmlIgnore]
	private Dictionary<int, CommonInventoryRequest> mItemPurchase = new Dictionary<int, CommonInventoryRequest>();

	[XmlIgnore]
	public int mContainerId;

	private static bool mInitialized;

	private static CommonInventoryData mInstance;

	public static bool pShowItemID;

	[XmlIgnore]
	public InventorySaveEventHandler mSaveCallback;

	[XmlIgnore]
	public InventoryUpdateEventHandler mUpdateCallback;

	[XmlIgnore]
	public InventorySetAttributeEventHandler mSetAttributeCallback;

	[XmlIgnore]
	public InventoryItemsEventHandler mGetInventoryItemsCallback;

	public object mSaveUserData;

	private int mUseItemNumUsed;

	public static CommonInventoryData pInstance
	{
		get
		{
			if (!pIsReady)
			{
				Init();
			}
			return mInstance;
		}
	}

	public static bool pIsReady => mInstance != null;

	public static void Init()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			WsWebService.GetCommonInventoryData(1, loadItemStats: true, ServiceEventHandler, null);
		}
	}

	public static void ReInit()
	{
		mInstance = null;
		mInitialized = true;
		WsWebService.GetCommonInventoryData(1, loadItemStats: true, ServiceEventHandler, null);
	}

	public static void Reset()
	{
		mInstance = null;
		mInitialized = false;
	}

	public void InitDefault()
	{
		Item = new UserItemData[0];
	}

	public void ClearSaveCache()
	{
		mUpdateList = null;
	}

	public void Save()
	{
		Save(null, null);
	}

	public void Save(InventorySaveEventHandler inCallback, object inUserData, CommonInventoryDataInstance instance = null)
	{
		if (mUpdateList != null && mUpdateList.Count > 0)
		{
			CommonInventorySaveInstance inUserData2 = new CommonInventorySaveInstance(inCallback, inUserData);
			for (int num = mUpdateList.Count - 1; num >= 0; num--)
			{
				CommonInventoryRequest commonInventoryRequest = mUpdateList[num];
				if (commonInventoryRequest.Quantity > 0 || commonInventoryRequest.CommonInventoryID == -1)
				{
					commonInventoryRequest.CommonInventoryID = null;
				}
			}
			CommonInventoryRequest[] inRequest = mUpdateList.ToArray();
			if (instance != null)
			{
				WsWebService.SetCommonInventoryData(inRequest, mContainerId, instance.ServiceEventHandler, inUserData2);
			}
			else
			{
				WsWebService.SetCommonInventoryData(inRequest, mContainerId, ServiceEventHandler, inUserData2);
			}
			mUpdateList = null;
		}
		else
		{
			inCallback?.Invoke(success: true, inUserData);
		}
	}

	public List<UserItemData> AddToCategories(UserItemData item)
	{
		List<UserItemData> list = null;
		if (item != null && item.Item != null)
		{
			if (item.Item.Category == null)
			{
				if (mInventory.ContainsKey(-1))
				{
					list = mInventory[-1];
				}
				else
				{
					list = new List<UserItemData>();
					mInventory[-1] = list;
				}
				list.Add(item);
			}
			else
			{
				ItemDataCategory[] category = item.Item.Category;
				foreach (ItemDataCategory itemDataCategory in category)
				{
					if (mInventory.ContainsKey(itemDataCategory.CategoryId))
					{
						list = mInventory[itemDataCategory.CategoryId];
					}
					else
					{
						list = new List<UserItemData>();
						mInventory[itemDataCategory.CategoryId] = list;
					}
					list.Add(item);
				}
			}
		}
		return list;
	}

	private bool ProcessCommonInventoryResponse(CommonInventoryResponse res, bool updateQuantity, CommonInventorySaveInstance saveInstance = null)
	{
		bool result = false;
		if (res != null && res.Success && res.CommonInventoryIDs != null)
		{
			if (saveInstance != null)
			{
				saveInstance.mLoadItemCount = res.CommonInventoryIDs.Length;
			}
			CommonInventoryResponseItem[] commonInventoryIDs = res.CommonInventoryIDs;
			foreach (CommonInventoryResponseItem commonInventoryResponseItem in commonInventoryIDs)
			{
				UserItemData userItemData = FindItemByUserInventoryID(commonInventoryResponseItem.CommonInventoryID);
				int num = commonInventoryResponseItem.Quantity;
				if (num == 0)
				{
					num++;
				}
				if (userItemData != null)
				{
					if (updateQuantity)
					{
						userItemData.Quantity += num;
					}
					userItemData.Uses = userItemData.Item.Uses;
					userItemData.UserInventoryID = commonInventoryResponseItem.CommonInventoryID;
					if (saveInstance != null && saveInstance.mSaveCallback != null)
					{
						saveInstance.mLoadItemCount--;
						if (saveInstance.mLoadItemCount <= 0)
						{
							saveInstance.mSaveCallback(success: true, saveInstance.mSaveUserData);
							saveInstance.mSaveCallback = null;
						}
					}
				}
				else
				{
					userItemData = new UserItemData();
					InvItemDataUserData invItemDataUserData = new InvItemDataUserData();
					invItemDataUserData.mID = commonInventoryResponseItem.ItemID;
					invItemDataUserData.mUserData = null;
					invItemDataUserData.mCallback = null;
					invItemDataUserData.mSaveInstance = saveInstance;
					if (updateQuantity)
					{
						invItemDataUserData.mQuantity = num;
					}
					else
					{
						invItemDataUserData.mQuantity = 0;
					}
					invItemDataUserData.mUserInvID = commonInventoryResponseItem.CommonInventoryID;
					ItemData.Load(commonInventoryResponseItem.ItemID, InvItemDataEventHandler, invItemDataUserData);
					result = true;
				}
			}
		}
		return result;
	}

	private void ProcessCommonInventoryGroupResponse(CommonInventoryGroupResponse response)
	{
		if (response == null || !response.Success)
		{
			return;
		}
		if (response.PrizeItems != null && response.PrizeItems.Count > 0)
		{
			foreach (MultiplePrizeItemResponse prizeItem in response.PrizeItems)
			{
				if (prizeItem.MysteryPrizeItems != null && prizeItem.MysteryPrizeItems.Count > 0)
				{
					foreach (PrizeItem mysteryPrizeItem in prizeItem.MysteryPrizeItems)
					{
						if (mysteryPrizeItem.CommonInventoryID > 0 && mysteryPrizeItem.ItemQuantity > 0)
						{
							UserItemData userItemData = FindItemByUserInventoryID(mysteryPrizeItem.CommonInventoryID);
							if (userItemData != null)
							{
								userItemData.Quantity += mysteryPrizeItem.ItemQuantity;
							}
							else
							{
								InvItemDataUserData invItemDataUserData = new InvItemDataUserData();
								invItemDataUserData.mID = mysteryPrizeItem.Item.ItemID;
								invItemDataUserData.mUserData = null;
								invItemDataUserData.mCallback = null;
								invItemDataUserData.mQuantity = mysteryPrizeItem.ItemQuantity;
								invItemDataUserData.mUserInvID = mysteryPrizeItem.CommonInventoryID;
								ItemData.Load(mysteryPrizeItem.Item.ItemID, InvItemDataEventHandler, invItemDataUserData);
							}
						}
					}
				}
				else
				{
					UtDebug.LogError("Redeem Multiple items : Empty prize items for Box Item " + prizeItem.ItemID);
				}
			}
			return;
		}
		UtDebug.LogError("Redeem Multiple items : Empty multiple prize items");
	}

	public static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		ProcessEvent(ref mInstance, inType, inEvent, inProgress, inObject, inUserData);
		if (mInstance != null)
		{
			mInstance.mContainerId = 1;
		}
	}

	public static void ProcessEvent(ref CommonInventoryData instance, WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_USER_GAME_CURRENCY:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inUserData != null)
				{
					if (instance.mPurchaseCallback != null)
					{
						instance.mPurchaseCallback((CommonInventoryResponse)inUserData);
					}
				}
				else if (instance.mSellCallback != null)
				{
					instance.mSellCallback(success: true, null);
				}
				if (inObject is UserGameCurrency money)
				{
					Money.SetMoney(money);
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetUserGameCurrency FAILED!!!");
				if (inUserData != null)
				{
					if (instance.mPurchaseCallback != null)
					{
						instance.mPurchaseCallback((CommonInventoryResponse)inUserData);
					}
				}
				else if (instance.mSellCallback != null)
				{
					instance.mSellCallback(success: true, null);
				}
				break;
			}
			break;
		case WsServiceType.PURCHASE_ITEMS:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				CommonInventoryResponse commonInventoryResponse4 = (CommonInventoryResponse)inObject;
				if (inObject != null && commonInventoryResponse4.Success)
				{
					if (commonInventoryResponse4.PrizeItems != null && commonInventoryResponse4.PrizeItems.Count > 0)
					{
						foreach (PrizeItemResponse prizeItem in commonInventoryResponse4.PrizeItems)
						{
							foreach (ItemData mysteryPrizeItem in prizeItem.MysteryPrizeItems)
							{
								ItemData.AddToCache(mysteryPrizeItem);
							}
						}
					}
					instance.ProcessCommonInventoryResponse(commonInventoryResponse4, updateQuantity: true);
					if (commonInventoryResponse4.UserGameCurrency != null)
					{
						if (commonInventoryResponse4.UserGameCurrency.UserID.ToString() != UserInfo.pInstance.UserID)
						{
							commonInventoryResponse4.UserGameCurrency.GameCurrency = Money.pGameCurrency;
						}
						Money.SetMoney(commonInventoryResponse4.UserGameCurrency);
					}
					if (instance.mUpdateCallback != null)
					{
						instance.mUpdateCallback();
					}
					if (PurchaseSuccessCallback != null)
					{
						PurchaseSuccessCallback(inType, (Dictionary<int, CommonInventoryRequest>)inUserData);
					}
				}
				if (instance.mPurchaseCallback != null)
				{
					instance.mPurchaseCallback(commonInventoryResponse4);
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL PURCHASE_ITEMS FAILED!!!");
				if (instance.mPurchaseCallback != null)
				{
					instance.mPurchaseCallback(null);
				}
				break;
			}
			break;
		case WsServiceType.REDEEM_ITEMS:
		case WsServiceType.REDEEM_MYSTERY_BOX_ITEMS:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				CommonInventoryResponse commonInventoryResponse3 = (CommonInventoryResponse)inObject;
				if (inObject != null && commonInventoryResponse3.Success)
				{
					instance.RemoveItem((UserItemData)inUserData, 1);
					if (commonInventoryResponse3.PrizeItems != null && commonInventoryResponse3.PrizeItems.Count > 0)
					{
						foreach (PrizeItemResponse prizeItem2 in commonInventoryResponse3.PrizeItems)
						{
							foreach (ItemData mysteryPrizeItem2 in prizeItem2.MysteryPrizeItems)
							{
								ItemData.AddToCache(mysteryPrizeItem2);
							}
						}
					}
					instance.ProcessCommonInventoryResponse(commonInventoryResponse3, updateQuantity: true);
					if (commonInventoryResponse3.UserGameCurrency != null)
					{
						Money.SetMoney(commonInventoryResponse3.UserGameCurrency);
					}
					if (instance.mUpdateCallback != null)
					{
						instance.mUpdateCallback();
					}
				}
				if (instance.mPurchaseCallback != null)
				{
					instance.mPurchaseCallback(commonInventoryResponse3);
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL PURCHASE_ITEMS FAILED!!!");
				if (instance.mPurchaseCallback != null)
				{
					instance.mPurchaseCallback(null);
				}
				break;
			}
			break;
		case WsServiceType.REDEEM_MULTIPLE_ITEMS:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				CommonInventoryGroupResponse commonInventoryGroupResponse = inObject as CommonInventoryGroupResponse;
				if (inObject != null)
				{
					if (commonInventoryGroupResponse.Success)
					{
						CommonInventory.V4.RedeemRequest[] array = (CommonInventory.V4.RedeemRequest[])inUserData;
						foreach (CommonInventory.V4.RedeemRequest redeemRequest in array)
						{
							instance.RemoveItem(redeemRequest.ItemID, updateServer: false, redeemRequest.RedeemItemCount);
							instance.ClearSaveCache();
						}
						if (commonInventoryGroupResponse.PrizeItems != null && commonInventoryGroupResponse.PrizeItems.Count > 0)
						{
							foreach (MultiplePrizeItemResponse prizeItem3 in commonInventoryGroupResponse.PrizeItems)
							{
								if (prizeItem3.MysteryPrizeItems == null || prizeItem3.MysteryPrizeItems.Count <= 0)
								{
									continue;
								}
								foreach (PrizeItem mysteryPrizeItem3 in prizeItem3.MysteryPrizeItems)
								{
									ItemData.AddToCache(mysteryPrizeItem3.Item);
								}
							}
						}
						instance.ProcessCommonInventoryGroupResponse(commonInventoryGroupResponse);
						if (commonInventoryGroupResponse.UserGameCurrency != null)
						{
							Money.SetMoney(commonInventoryGroupResponse.UserGameCurrency);
						}
						if (instance.mUpdateCallback != null)
						{
							instance.mUpdateCallback();
						}
					}
					else
					{
						UtDebug.LogError("Redeem Multiple items : Response Failed");
						if (commonInventoryGroupResponse.ValidationResponse != null && commonInventoryGroupResponse.ValidationResponse.Count > 0)
						{
							foreach (ValidationStatusResponse item3 in commonInventoryGroupResponse.ValidationResponse)
							{
								UtDebug.LogError("Redeem Multiple items Fail Status " + item3.ValidationStatus.ToString() + " ItemID " + item3.ItemID + " Error message " + item3.ErrorMessage + " Status ");
							}
						}
					}
					if (instance.mRedeemItemsCallback != null)
					{
						instance.mRedeemItemsCallback(commonInventoryGroupResponse);
					}
				}
				else
				{
					UtDebug.LogError("Redeem Multiple items : Response NULL");
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL RedeemItems multiple FAILED!!!");
				if (instance.mRedeemItemsCallback != null)
				{
					instance.mRedeemItemsCallback(null);
				}
				break;
			}
			break;
		case WsServiceType.SELL_ITEMS:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				CommonInventoryResponse commonInventoryResponse2 = (CommonInventoryResponse)inObject;
				if (commonInventoryResponse2 != null && commonInventoryResponse2.Success)
				{
					foreach (KeyValuePair<int, CommonInventoryRequest> item4 in instance.mItemPurchase)
					{
						UserItemData item2 = instance.FindItemByUserInventoryID(item4.Key);
						instance.RemoveItem(item2, item4.Value.Quantity);
					}
					instance.ProcessCommonInventoryResponse(commonInventoryResponse2, updateQuantity: true);
					if (commonInventoryResponse2.UserGameCurrency != null)
					{
						Money.SetMoney(commonInventoryResponse2.UserGameCurrency);
					}
					if (instance.mUpdateCallback != null)
					{
						instance.mUpdateCallback();
					}
				}
				instance.mItemPurchase.Clear();
				if (instance.mSellCallback != null)
				{
					instance.mSellCallback(commonInventoryResponse2?.Success ?? false, commonInventoryResponse2);
				}
				break;
			}
			case WsServiceEvent.ERROR:
				instance.mItemPurchase.Clear();
				UtDebug.LogError("WEB SERVICE CALL SELL_ITEMS FAILED!!!");
				if (instance.mSellCallback != null)
				{
					instance.mSellCallback(success: false, null);
				}
				break;
			}
			break;
		case WsServiceType.SET_COMMON_INVENTORY:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				CommonInventorySaveInstance commonInventorySaveInstance2 = (CommonInventorySaveInstance)inUserData;
				CommonInventoryResponse commonInventoryResponse = (CommonInventoryResponse)inObject;
				if (inObject != null && commonInventoryResponse.Success)
				{
					bool flag2 = instance.ProcessCommonInventoryResponse(commonInventoryResponse, updateQuantity: false, commonInventorySaveInstance2);
					if (commonInventorySaveInstance2.mSaveCallback != null && (!flag2 || ItemData.pLoadingList.Count <= 0))
					{
						commonInventorySaveInstance2.mSaveCallback(success: true, commonInventorySaveInstance2.mSaveUserData);
						commonInventorySaveInstance2.mSaveCallback = null;
					}
					if (instance.mUpdateCallback != null)
					{
						instance.mUpdateCallback();
					}
				}
				else if (commonInventorySaveInstance2.mSaveCallback != null)
				{
					commonInventorySaveInstance2.mSaveCallback(success: false, commonInventorySaveInstance2.mSaveUserData);
					commonInventorySaveInstance2.mSaveCallback = null;
				}
				break;
			}
			case WsServiceEvent.ERROR:
			{
				UtDebug.LogError("WEB SERVICE CALL SET_INVENTORY FAILED!!!");
				CommonInventorySaveInstance commonInventorySaveInstance = (CommonInventorySaveInstance)inUserData;
				if (commonInventorySaveInstance.mSaveCallback != null)
				{
					commonInventorySaveInstance.mSaveCallback(success: false, commonInventorySaveInstance.mSaveUserData);
					commonInventorySaveInstance.mSaveCallback = null;
				}
				break;
			}
			}
			break;
		case WsServiceType.SET_COMMON_INVENTORY_ATTRIBUTE:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					bool success = (bool)inObject;
					if (instance.mSetAttributeCallback != null)
					{
						instance.mSetAttributeCallback(success);
						instance.mSetAttributeCallback = null;
					}
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL SetCommonInventoryAttribute FAILED!!!");
				break;
			}
			break;
		case WsServiceType.GET_COMMON_INVENTORY:
		case WsServiceType.GET_COMMON_INVENTORY_BY_USER_ID:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				instance = (CommonInventoryData)inObject;
				if (instance != null)
				{
					if (instance.Item != null)
					{
						UserItemData[] item = instance.Item;
						foreach (UserItemData userItemData2 in item)
						{
							instance.AddToCategories(userItemData2);
							ItemData.AddToCache(userItemData2.Item);
						}
					}
					else
					{
						instance.InitDefault();
					}
				}
				else
				{
					instance = new CommonInventoryData();
					instance.InitDefault();
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetCommonInventoryData FAILED!!!");
				instance = new CommonInventoryData();
				instance.InitDefault();
				break;
			}
			break;
		case WsServiceType.USE_INVENTORY:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				bool flag = (bool)inObject;
				if (flag)
				{
					UserItemData userItemData = (UserItemData)inUserData;
					userItemData.Uses -= instance.mUseItemNumUsed;
					if (userItemData.Uses <= 0)
					{
						instance.RemoveItem(userItemData, 1);
					}
					if (instance.mUpdateCallback != null)
					{
						instance.mUpdateCallback();
					}
				}
				if (instance.mSaveCallback != null)
				{
					instance.mSaveCallback(flag, instance.mSaveUserData);
					instance.mSaveCallback = null;
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL UseCommonInventoryData FAILED!!!");
				if (instance.mSaveCallback != null)
				{
					instance.mSaveCallback(success: false, instance.mSaveUserData);
					instance.mSaveCallback = null;
				}
				break;
			}
			break;
		}
	}

	public UserItemData[] GetItems(int category)
	{
		if (mInventory.ContainsKey(category) && mInventory[category] != null)
		{
			return mInventory[category].ToArray();
		}
		return null;
	}

	public UserItemData[] GetItems(int[] categoryList)
	{
		if (categoryList == null || categoryList.Length == 0)
		{
			return null;
		}
		List<UserItemData> list = new List<UserItemData>();
		foreach (int key in categoryList)
		{
			if (mInventory.ContainsKey(key))
			{
				if (list == null)
				{
					list = new List<UserItemData>(mInventory[key]);
				}
				else
				{
					list.AddRange(mInventory[key]);
				}
			}
		}
		return list.ToArray();
	}

	public int GetQuantity(int itemID)
	{
		return FindItem(itemID)?.Quantity ?? 0;
	}

	public bool HasCategory(int itemID, int categoryID)
	{
		return FindItem(itemID)?.Item.HasCategory(categoryID) ?? false;
	}

	public ItemDataCategory[] GetCategory(int itemID)
	{
		return FindItem(itemID)?.Item.Category;
	}

	public string GetIcon(int itemID)
	{
		UserItemData userItemData = FindItem(itemID);
		if (userItemData != null)
		{
			return userItemData.Item.IconName;
		}
		return "";
	}

	public void AddPurchaseItem(int itemID, int amount, string purchaseSourceName = null)
	{
		if (mItemPurchase.ContainsKey(itemID))
		{
			mItemPurchase[itemID].Quantity += amount;
			return;
		}
		CommonInventoryRequest commonInventoryRequest = new CommonInventoryRequest();
		commonInventoryRequest.ItemID = itemID;
		commonInventoryRequest.Quantity = amount;
		commonInventoryRequest.SourceName = purchaseSourceName;
		mItemPurchase[itemID] = commonInventoryRequest;
	}

	public void DoPurchase(int currencyType, int storeID, PurchaseEventHandler callback)
	{
		if (mItemPurchase.Count == 0)
		{
			return;
		}
		mPurchaseCallback = callback;
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, CommonInventoryRequest> item in mItemPurchase)
		{
			for (int i = 0; i < item.Value.Quantity; i++)
			{
				list.Add(item.Value.ItemID.Value);
			}
		}
		WsWebService.PurchaseItems(list.ToArray(), currencyType, mContainerId, storeID, ServiceEventHandler, new Dictionary<int, CommonInventoryRequest>(mItemPurchase));
		mItemPurchase.Clear();
	}

	public void RedeemItem(UserItemData item, int itemCount, PurchaseEventHandler callback)
	{
		RedeemRequest redeemRequest = new RedeemRequest
		{
			ItemID = item.Item.ItemID,
			RedeemItemFetchCount = itemCount
		};
		mPurchaseCallback = callback;
		WsWebService.RedeemItem(redeemRequest, ServiceEventHandler, item);
	}

	public void RedeemItems(CommonInventory.V4.RedeemRequest[] redeemRequests, RedeemItemsEventHandler callback)
	{
		WsWebService.RedeemItems(redeemRequests, ServiceEventHandler, redeemRequests);
		mRedeemItemsCallback = callback;
	}

	public void RedeemMysteryBoxItems(UserItemData item, int itemCount, PurchaseEventHandler callback)
	{
		RedeemRequest redeemRequest = new RedeemRequest
		{
			ItemID = item.Item.ItemID,
			RedeemItemFetchCount = itemCount
		};
		mPurchaseCallback = callback;
		WsWebService.RedeemMysteryBoxItems(redeemRequest, ServiceEventHandler, item);
	}

	public void DoPurchase(CommonInventoryDataInstance inInstance, int currencyType, int storeID, PurchaseEventHandler callback)
	{
		if (mItemPurchase.Count == 0)
		{
			return;
		}
		mPurchaseCallback = callback;
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, CommonInventoryRequest> item in mItemPurchase)
		{
			for (int i = 0; i < item.Value.Quantity; i++)
			{
				list.Add(item.Value.ItemID.Value);
			}
		}
		WsWebService.PurchaseItems(inInstance.pUserToken, list.ToArray(), currencyType, mContainerId, storeID, inInstance.ServiceEventHandler, inInstance);
		mItemPurchase.Clear();
	}

	public void DoPurchase(CommonInventoryDataInstance inInstance, int currencyType, int storeID, bool addMysteryBoxToInventory, PurchaseEventHandler callback)
	{
		if (mItemPurchase.Count == 0)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, CommonInventoryRequest> item in mItemPurchase)
		{
			for (int i = 0; i < item.Value.Quantity; i++)
			{
				list.Add(item.Value.ItemID.Value);
			}
		}
		PurchaseStoreItemRequest purchaseItemRequest = new PurchaseStoreItemRequest
		{
			ContainerId = mContainerId,
			StoreID = storeID,
			Items = list.ToArray(),
			CurrencyType = currencyType,
			AddMysteryBoxToInventory = addMysteryBoxToInventory
		};
		inInstance.pData.mPurchaseCallback = callback;
		WsWebService.PurchaseItems(purchaseItemRequest, inInstance.ServiceEventHandler, inInstance);
		mItemPurchase.Clear();
	}

	public void AddSellItem(int userInventoryID, int amount)
	{
		AddPurchaseItem(userInventoryID, amount);
	}

	public void DoSell(SellEventHandler callback)
	{
		if (mItemPurchase.Count == 0)
		{
			return;
		}
		mSellCallback = callback;
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, CommonInventoryRequest> item in mItemPurchase)
		{
			for (int i = 0; i < item.Value.Quantity; i++)
			{
				list.Add(item.Value.ItemID.Value);
			}
		}
		WsWebService.SellItems(new SellItemsRequest
		{
			UserInventoryCommonIDs = list.ToArray(),
			ContainerID = mContainerId
		}, ServiceEventHandler, null);
	}

	public int AddItem(int itemID)
	{
		return AddItemAt(itemID, 0, -1, updateServer: false, null, null, 1);
	}

	public int AddItem(int itemID, int inQuantity)
	{
		return AddItemAt(itemID, 0, -1, updateServer: false, null, null, inQuantity);
	}

	public int AddItem(int itemID, bool updateServer)
	{
		return AddItemAt(itemID, 0, -1, updateServer, null, null, 1);
	}

	public int AddItem(int itemID, bool updateServer, InventoryItemDataEventHandler inCallback)
	{
		return AddItemAt(itemID, 0, -1, updateServer, inCallback, null, 1);
	}

	public int AddItem(int itemID, bool updateServer, InventoryItemDataEventHandler inCallback, object inUserData)
	{
		return AddItemAt(itemID, 0, -1, updateServer, inCallback, inUserData, 1);
	}

	public int AddItem(int itemID, bool updateServer, InventoryItemDataEventHandler inCallback, object inUserData, int inQuantity)
	{
		return AddItemAt(itemID, 0, -1, updateServer, inCallback, inUserData, inQuantity);
	}

	public int AddItem(int inItemID, bool updateServer, int inCommonInventoryID, int inQuantity)
	{
		return AddItemAt(inItemID, 0, -1, updateServer, null, null, inQuantity, inCommonInventoryID);
	}

	public CommonInventoryRequest FindInUpdateList(int itemID)
	{
		if (mUpdateList == null)
		{
			return null;
		}
		foreach (CommonInventoryRequest mUpdate in mUpdateList)
		{
			if (mUpdate.ItemID == itemID)
			{
				return mUpdate;
			}
		}
		return null;
	}

	public CommonInventoryRequest FindInUpdateListByUserInventoryID(int userInventoryID)
	{
		if (mUpdateList == null)
		{
			return null;
		}
		foreach (CommonInventoryRequest mUpdate in mUpdateList)
		{
			if (mUpdate.CommonInventoryID == userInventoryID)
			{
				return mUpdate;
			}
		}
		return null;
	}

	private UserItemData CreateUserItemData(ItemData inItemData, InvItemDataUserData inUserData)
	{
		UserItemData userItemData = new UserItemData();
		userItemData.Item = inItemData;
		if (inItemData != null)
		{
			userItemData.ItemID = inItemData.ItemID;
			userItemData.Uses = inItemData.Uses;
		}
		if (inUserData != null)
		{
			userItemData.UserInventoryID = inUserData.mUserInvID;
			userItemData.Quantity = inUserData.mQuantity;
		}
		return userItemData;
	}

	private void InvItemDataEventHandler(int itemID, ItemData dataItem, object inUserData)
	{
		if (itemID < 0)
		{
			return;
		}
		InvItemDataUserData invItemDataUserData = (InvItemDataUserData)inUserData;
		UserItemData userItemData = null;
		if (invItemDataUserData.mUserInvID != -1)
		{
			userItemData = FindItemByUserInventoryID(invItemDataUserData.mUserInvID);
		}
		if (userItemData == null)
		{
			userItemData = FindItem(invItemDataUserData.mID);
		}
		if (userItemData != null)
		{
			if (userItemData.UserInventoryID != -1 && userItemData.UserInventoryID == invItemDataUserData.mUserInvID)
			{
				userItemData.Quantity += invItemDataUserData.mQuantity;
				userItemData.Uses = userItemData.Item.Uses;
			}
			else if (userItemData.UserInventoryID == -1 && invItemDataUserData.mUserInvID != -1)
			{
				userItemData.UserInventoryID = invItemDataUserData.mUserInvID;
				userItemData.Quantity += invItemDataUserData.mQuantity;
				userItemData.Uses = userItemData.Item.Uses;
				if (dataItem != null && dataItem.IsStatAvailable())
				{
					userItemData.ItemStats = dataItem.ItemStatsMap.ItemStats;
					userItemData.ItemTier = ItemTier.Tier1;
				}
			}
			else if (userItemData.Uses > 0)
			{
				userItemData = CreateUserItemData(dataItem, invItemDataUserData);
				AddToCategories(userItemData);
				if (dataItem != null && dataItem.IsStatAvailable())
				{
					userItemData.ItemStats = dataItem.ItemStatsMap.ItemStats;
					userItemData.ItemTier = ItemTier.Tier1;
				}
			}
			else
			{
				userItemData.Quantity += invItemDataUserData.mQuantity;
				userItemData.Uses = userItemData.Item.Uses;
			}
		}
		else
		{
			userItemData = CreateUserItemData(dataItem, invItemDataUserData);
			AddToCategories(userItemData);
			if (dataItem != null && dataItem.IsStatAvailable())
			{
				userItemData.ItemStats = dataItem.ItemStatsMap.ItemStats;
				userItemData.ItemTier = ItemTier.Tier1;
			}
		}
		if (invItemDataUserData.mCallback != null)
		{
			invItemDataUserData.mCallback(userItemData, invItemDataUserData.mUserData);
		}
		if (invItemDataUserData.mSaveInstance != null && invItemDataUserData.mSaveInstance.mSaveCallback != null)
		{
			invItemDataUserData.mSaveInstance.mLoadItemCount--;
			if (invItemDataUserData.mSaveInstance.mLoadItemCount <= 0)
			{
				invItemDataUserData.mSaveInstance.mSaveCallback(success: true, invItemDataUserData.mSaveInstance.mSaveUserData);
				invItemDataUserData.mSaveInstance.mSaveCallback = null;
			}
		}
	}

	public int AddItemAt(int itemID, int idx)
	{
		return AddItemAt(itemID, idx, -1, updateServer: false, null, null, 1);
	}

	public int AddItemAt(int itemID, int idx, int category)
	{
		return AddItemAt(itemID, idx, category, updateServer: false, null, null, 1);
	}

	public int AddItemAt(int itemID, int idx, int category, bool updateServer, InventoryItemDataEventHandler inCallback)
	{
		return AddItemAt(itemID, idx, category, updateServer, inCallback, null, 1);
	}

	public int AddItemAt(int itemID, int idx, int category, bool updateServer, InventoryItemDataEventHandler inCallback, object inUserData)
	{
		return AddItemAt(itemID, idx, category, updateServer, inCallback, inUserData, 1);
	}

	public int AddItemAt(int itemID, int idx, int categoryID, bool updateServer, InventoryItemDataEventHandler inCallback, object inUserData, int inQuantity, int inCommonInventoryID = -1)
	{
		CommonInventoryRequest commonInventoryRequest = FindInUpdateList(itemID);
		if (commonInventoryRequest != null)
		{
			commonInventoryRequest.Quantity += inQuantity;
			if (commonInventoryRequest.Quantity == 0)
			{
				mUpdateList.Remove(commonInventoryRequest);
			}
		}
		else
		{
			commonInventoryRequest = new CommonInventoryRequest();
			commonInventoryRequest.ItemID = itemID;
			commonInventoryRequest.Quantity = inQuantity;
			if (mUpdateList == null)
			{
				mUpdateList = new List<CommonInventoryRequest>();
			}
			mUpdateList.Add(commonInventoryRequest);
		}
		commonInventoryRequest.CommonInventoryID = inCommonInventoryID;
		int num = FindItem(itemID, categoryID, out var list);
		if (num != -1)
		{
			UserItemData userItemData = list[num];
			if (userItemData.Item.Uses < 0)
			{
				list[num].Quantity += inQuantity;
				commonInventoryRequest.CommonInventoryID = list[num].UserInventoryID;
				if (num != idx)
				{
					list.RemoveAt(num);
					list.Insert(idx, userItemData);
				}
				if (updateServer)
				{
					Save();
				}
				inCallback?.Invoke(list[idx], inUserData);
				return idx;
			}
			InvItemDataUserData invItemDataUserData = new InvItemDataUserData();
			invItemDataUserData.mID = itemID;
			invItemDataUserData.mUserData = inUserData;
			invItemDataUserData.mCallback = inCallback;
			invItemDataUserData.mQuantity = inQuantity;
			ItemData.Load(itemID, InvItemDataEventHandler, invItemDataUserData);
		}
		else
		{
			if (Item != null)
			{
				UserItemData[] item = Item;
				foreach (UserItemData userItemData2 in item)
				{
					if (userItemData2.Item.ItemID == itemID && userItemData2.Item.Uses < 0)
					{
						userItemData2.Quantity = inQuantity;
						commonInventoryRequest.CommonInventoryID = userItemData2.UserInventoryID;
						AddToCategories(userItemData2);
						num = FindItem(itemID, categoryID, out list);
						if (num != idx && idx < list.Count)
						{
							UserItemData item2 = list[num];
							list.RemoveAt(num);
							list.Insert(idx, item2);
						}
						if (updateServer)
						{
							Save();
						}
						inCallback?.Invoke(userItemData2, inUserData);
						return idx;
					}
				}
			}
			InvItemDataUserData invItemDataUserData2 = new InvItemDataUserData();
			invItemDataUserData2.mID = itemID;
			invItemDataUserData2.mUserData = inUserData;
			invItemDataUserData2.mCallback = inCallback;
			invItemDataUserData2.mQuantity = inQuantity;
			invItemDataUserData2.mUserInvID = inCommonInventoryID;
			ItemData.Load(itemID, InvItemDataEventHandler, invItemDataUserData2);
		}
		if (updateServer)
		{
			Save();
		}
		return -1;
	}

	public void AddUserItem(UserItemData userItem)
	{
		UserItemData userItemData = null;
		if (userItem.UserInventoryID != -1)
		{
			userItemData = FindItemByUserInventoryID(userItem.UserInventoryID);
		}
		if (userItemData != null)
		{
			userItemData.Quantity = userItem.Quantity;
		}
		else
		{
			AddToCategories(userItem);
		}
	}

	public void AddItem(UserItemData inItem, bool updateServer, int inQuantity)
	{
		if (inItem == null || inItem.Item == null)
		{
			return;
		}
		int itemID = inItem.Item.ItemID;
		CommonInventoryRequest commonInventoryRequest = FindInUpdateList(itemID);
		if (commonInventoryRequest != null)
		{
			commonInventoryRequest.Quantity += inQuantity;
			if (commonInventoryRequest.Quantity == 0)
			{
				mUpdateList.Remove(commonInventoryRequest);
			}
		}
		else
		{
			commonInventoryRequest = new CommonInventoryRequest();
			commonInventoryRequest.ItemID = itemID;
			commonInventoryRequest.Quantity = inQuantity;
			if (mUpdateList == null)
			{
				mUpdateList = new List<CommonInventoryRequest>();
			}
			mUpdateList.Add(commonInventoryRequest);
		}
		commonInventoryRequest.CommonInventoryID = null;
		UserItemData userItemData = FindItem(inItem.Item.ItemID);
		if (userItemData != null)
		{
			userItemData.Quantity += inQuantity;
			commonInventoryRequest.CommonInventoryID = userItemData.UserInventoryID;
		}
		else
		{
			userItemData = new UserItemData();
			userItemData.Item = inItem.Item;
			userItemData.UserInventoryID = inItem.UserInventoryID;
			userItemData.Quantity = inQuantity;
			AddToCategories(userItemData);
		}
		if (updateServer)
		{
			Save();
		}
	}

	public void UpdateItemAttributes(int inventoryID, PairData pairData, InventorySetAttributeEventHandler inCallback = null)
	{
		List<UserItemData> list;
		int num = FindItemByUserInventoryID(inventoryID, out list);
		if (num != -1 && pairData != null)
		{
			WsWebService.SetCommonInventoryAttribute(inventoryID, pairData, ServiceEventHandler, null);
			mSetAttributeCallback = inCallback;
			list[num].UserItemAttributes = pairData;
		}
	}

	public int RemoveItem(int itemID, bool updateServer, int inQuantity = 1, InventorySaveEventHandler inCallback = null)
	{
		int num = -1;
		UserItemData userItemData = FindItem(itemID);
		if (userItemData != null)
		{
			userItemData.Quantity -= inQuantity;
			num = userItemData.Quantity;
			if (userItemData.Quantity <= 0 && userItemData.Item.Category != null)
			{
				ItemDataCategory[] category = userItemData.Item.Category;
				foreach (ItemDataCategory itemDataCategory in category)
				{
					mInventory[itemDataCategory.CategoryId].Remove(userItemData);
				}
			}
			CommonInventoryRequest commonInventoryRequest = FindInUpdateList(itemID);
			if (commonInventoryRequest != null)
			{
				commonInventoryRequest.Quantity -= inQuantity;
				if (commonInventoryRequest.Quantity == 0)
				{
					mUpdateList.Remove(commonInventoryRequest);
				}
			}
			else
			{
				commonInventoryRequest = new CommonInventoryRequest();
				commonInventoryRequest.ItemID = itemID;
				commonInventoryRequest.Quantity = -inQuantity;
				if (mUpdateList == null)
				{
					mUpdateList = new List<CommonInventoryRequest>();
				}
				mUpdateList.Add(commonInventoryRequest);
			}
			commonInventoryRequest.CommonInventoryID = userItemData.UserInventoryID;
			if (updateServer)
			{
				Save(inCallback, null);
			}
			return num;
		}
		UtDebug.LogError("Removing item from inventory failed!! Item ID = " + itemID);
		return -1;
	}

	public int RemoveItemByUserInventoryID(int itemID, int userInventoryID, bool updateServer, int inQuantity = 1, InventorySaveEventHandler inCallback = null)
	{
		int num = -1;
		UserItemData userItemData = FindItemByUserInventoryID(userInventoryID);
		if (userItemData != null)
		{
			userItemData.Quantity -= inQuantity;
			num = userItemData.Quantity;
			if (userItemData.Quantity <= 0 && userItemData.Item.Category != null)
			{
				ItemDataCategory[] category = userItemData.Item.Category;
				foreach (ItemDataCategory itemDataCategory in category)
				{
					mInventory[itemDataCategory.CategoryId].Remove(userItemData);
				}
			}
			CommonInventoryRequest commonInventoryRequest = FindInUpdateListByUserInventoryID(userInventoryID);
			if (commonInventoryRequest != null)
			{
				commonInventoryRequest.Quantity -= inQuantity;
				if (commonInventoryRequest.Quantity == 0)
				{
					mUpdateList.Remove(commonInventoryRequest);
				}
			}
			else
			{
				commonInventoryRequest = new CommonInventoryRequest();
				commonInventoryRequest.ItemID = itemID;
				commonInventoryRequest.Quantity = -inQuantity;
				if (mUpdateList == null)
				{
					mUpdateList = new List<CommonInventoryRequest>();
				}
				mUpdateList.Add(commonInventoryRequest);
			}
			commonInventoryRequest.CommonInventoryID = userItemData.UserInventoryID;
			if (updateServer)
			{
				Save(inCallback, null);
			}
			return num;
		}
		UtDebug.LogError("Removing item from inventory failed!! Item ID = " + itemID);
		return -1;
	}

	public int RemoveItem(UserItemData item, int inQuantity, bool checkCategory = true)
	{
		int num = -1;
		item.Quantity -= inQuantity;
		num = item.Quantity;
		if (item.Quantity <= 0)
		{
			if (checkCategory)
			{
				if (item.Item.Category != null)
				{
					ItemDataCategory[] category = item.Item.Category;
					foreach (ItemDataCategory itemDataCategory in category)
					{
						mInventory[itemDataCategory.CategoryId].Remove(item);
					}
				}
			}
			else
			{
				mInventory[-1].Remove(item);
			}
		}
		return num;
	}

	public void UseItem(UserItemData item, int inUse, InventorySaveEventHandler inCallback, object inUserData)
	{
		mSaveCallback = (InventorySaveEventHandler)Delegate.Combine(mSaveCallback, inCallback);
		mSaveUserData = inUserData;
		mUseItemNumUsed = inUse;
		WsWebService.UseCommonInventoryData(item.UserInventoryID, inUse, ServiceEventHandler, item);
	}

	public int FindItemIndex(int itemID)
	{
		return FindItemIndex(itemID, -1);
	}

	public int FindItemIndex(int itemID, int categoryID)
	{
		List<UserItemData> list;
		if (categoryID == -1)
		{
			return FindItem(itemID, out list);
		}
		if (mInventory.ContainsKey(categoryID))
		{
			list = mInventory[categoryID];
			int num = 0;
			foreach (UserItemData item in list)
			{
				if (item.Item.ItemID == itemID)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	public UserItemData FindItem(int itemID)
	{
		List<UserItemData> list;
		int num = FindItem(itemID, out list);
		if (num != -1)
		{
			return list[num];
		}
		return null;
	}

	public int FindItem(int itemID, out List<UserItemData> list)
	{
		return FindItem(itemID, -1, out list);
	}

	public int FindItem(int itemID, int categoryID, out List<UserItemData> list)
	{
		list = null;
		if (categoryID == -1)
		{
			foreach (KeyValuePair<int, List<UserItemData>> item in mInventory)
			{
				int i = 0;
				for (int count = item.Value.Count; i < count; i++)
				{
					if (item.Value[i].Item.ItemID == itemID)
					{
						list = item.Value;
						return i;
					}
				}
			}
		}
		else if (mInventory.ContainsKey(categoryID))
		{
			list = mInventory[categoryID];
			int j = 0;
			for (int count2 = list.Count; j < count2; j++)
			{
				if (list[j].Item.ItemID == itemID)
				{
					return j;
				}
			}
		}
		return -1;
	}

	public List<UserItemData> FindItems(int itemID)
	{
		List<UserItemData> list = new List<UserItemData>();
		foreach (List<UserItemData> value in mInventory.Values)
		{
			List<UserItemData> list2 = value.FindAll((UserItemData x) => x.Item.ItemID == itemID);
			if (list2 == null)
			{
				continue;
			}
			foreach (UserItemData item in list2)
			{
				if (list.Find((UserItemData x) => x.UserInventoryID == item.UserInventoryID) == null)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public UserItemData FindItemByUserInventoryID(int userInventoryID)
	{
		List<UserItemData> list;
		int num = FindItemByUserInventoryID(userInventoryID, out list);
		if (num != -1)
		{
			return list[num];
		}
		return null;
	}

	public int FindItemByUserInventoryID(int userInventoryID, out List<UserItemData> list)
	{
		list = null;
		foreach (KeyValuePair<int, List<UserItemData>> item in mInventory)
		{
			int i = 0;
			for (int count = item.Value.Count; i < count; i++)
			{
				if (item.Value[i].UserInventoryID == userInventoryID)
				{
					list = item.Value;
					return i;
				}
			}
		}
		return -1;
	}

	public ItemData GetItemDataFromGeometry(string[] geometries, int categoryID = -1)
	{
		if (geometries == null || geometries.Length == 0 || !AvatarDataPart.IsResourceValid(geometries[0]))
		{
			return null;
		}
		UserItemData[] items = GetItems(categoryID);
		if (items != null)
		{
			UserItemData[] array = items;
			for (int i = 0; i < array.Length; i++)
			{
				ItemData item = array[i].Item;
				if (item.AssetName.Contains(geometries[0]) || (item.Geometry2 != null && item.Geometry2.Contains(geometries[0])))
				{
					return item;
				}
			}
		}
		return null;
	}

	public UserItemData GetUserItemDataFromGeometryAndTexture(string[] geometries, string[] textures, int categoryID = -1)
	{
		if (geometries == null || geometries.Length == 0 || !AvatarDataPart.IsResourceValid(geometries[0]) || textures == null || textures.Length == 0)
		{
			return null;
		}
		UserItemData[] items = GetItems(categoryID);
		if (items != null)
		{
			UserItemData[] array = items;
			foreach (UserItemData userItemData in array)
			{
				if (!userItemData.Item.AssetName.Contains(geometries[0]) && (userItemData.Item.Geometry2 == null || !userItemData.Item.Geometry2.Contains(geometries[0])))
				{
					continue;
				}
				try
				{
					if ((userItemData.Item.Texture != null && Array.Find(userItemData.Item.Texture, (ItemDataTexture t) => t.TextureName.Contains(textures[0])) != null) || (userItemData.Item.Texture == null && textures[0] == "__EMPTY__"))
					{
						return userItemData;
					}
				}
				catch (Exception ex)
				{
					UtDebug.LogError("Throws exception " + ex.Message + "  for item " + userItemData.Item.ItemID);
					return null;
				}
			}
		}
		return null;
	}

	public ItemStat[] GetItemStatsByUserInventoryID(int itemUiid)
	{
		ItemStat[] result = null;
		if (pIsReady && itemUiid > 0)
		{
			UserItemData userItemData = FindItemByUserInventoryID(itemUiid);
			if (userItemData != null)
			{
				result = userItemData.ItemStats;
			}
		}
		return result;
	}

	public ItemTier? GetItemTierByUserInventoryID(int itemUiid)
	{
		ItemTier? result = null;
		if (pIsReady && itemUiid > 0)
		{
			UserItemData userItemData = FindItemByUserInventoryID(itemUiid);
			if (userItemData != null)
			{
				return userItemData.ItemTier;
			}
		}
		return result;
	}

	public void GetBundledItemsInInventory(ItemData itemData, InventoryItemsEventHandler callback)
	{
		if (itemData.pBundledItems == null)
		{
			mGetInventoryItemsCallback = callback;
			itemData.LoadBundledItems(BundleItemLoaded);
		}
		else if (itemData.pBundledItems != null)
		{
			callback(GetItemsCommonInInventory(itemData.pBundledItems));
		}
	}

	private List<UserItemData> GetItemsCommonInInventory(List<ItemData> items)
	{
		List<UserItemData> list = new List<UserItemData>();
		if (items != null)
		{
			foreach (ItemData item in items)
			{
				if (item.HasAttribute("Parent"))
				{
					UserItemData userItemData = ParentData.pInstance.pInventory.pData.FindItem(item.ItemID);
					if (userItemData != null && userItemData.Quantity >= item.InventoryMax)
					{
						list.Add(userItemData);
					}
					continue;
				}
				UserItemData userItemData2 = ParentData.pInstance.pInventory.pData.FindItem(item.ItemID);
				UserItemData userItemData3 = FindItem(item.ItemID);
				int num = userItemData2?.Quantity ?? 0;
				int num2 = userItemData3?.Quantity ?? 0;
				if (num + num2 >= item.InventoryMax)
				{
					if (userItemData2 != null)
					{
						list.Add(userItemData2);
					}
					if (userItemData3 != null)
					{
						list.Add(userItemData3);
					}
				}
			}
		}
		return list;
	}

	private void BundleItemLoaded(List<ItemData> items, int itemId)
	{
		if (mGetInventoryItemsCallback != null)
		{
			mGetInventoryItemsCallback(GetItemsCommonInInventory(items));
			mGetInventoryItemsCallback = null;
		}
	}

	public void Clear()
	{
		mInventory = new Dictionary<int, List<UserItemData>>();
	}
}
