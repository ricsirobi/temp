public class CommonInventoryDataInstance
{
	public CommonInventoryData pData;

	public string pUserID = "";

	private string mUserToken = "";

	private bool mIsReady;

	public string pUserToken => mUserToken;

	public bool pIsReady => mIsReady;

	public static CommonInventoryDataInstance LoadByUserID(string userID)
	{
		CommonInventoryDataInstance commonInventoryDataInstance = new CommonInventoryDataInstance();
		commonInventoryDataInstance.pUserID = userID;
		commonInventoryDataInstance.mIsReady = false;
		if (UserInfo.pIsReady && UserInfo.pInstance.UserID == userID)
		{
			commonInventoryDataInstance.pData = CommonInventoryData.pInstance;
			commonInventoryDataInstance.mIsReady = true;
		}
		else
		{
			WsWebService.GetCommonInventoryDataByUserID(userID, 1, commonInventoryDataInstance.ServiceEventHandler, null);
		}
		return commonInventoryDataInstance;
	}

	public static CommonInventoryDataInstance LoadByToken(string inToken)
	{
		CommonInventoryDataInstance commonInventoryDataInstance = new CommonInventoryDataInstance();
		commonInventoryDataInstance.mUserToken = inToken;
		commonInventoryDataInstance.mIsReady = false;
		WsWebService.GetCommonInventoryData(inToken, 1, commonInventoryDataInstance.ServiceEventHandler, null);
		return commonInventoryDataInstance;
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		CommonInventoryData.ProcessEvent(ref pData, inType, inEvent, inProgress, inObject, inUserData);
		if (pData != null)
		{
			pData.mContainerId = 1;
			mIsReady = true;
		}
	}

	public UserItemData[] GetItems(int category)
	{
		if (!pIsReady)
		{
			return null;
		}
		return pData.GetItems(category);
	}

	public UserItemData[] GetItems(int[] categories)
	{
		if (!pIsReady)
		{
			return null;
		}
		return pData.GetItems(categories);
	}

	public int GetQuantity(int itemID)
	{
		if (!pIsReady)
		{
			return 0;
		}
		return pData.GetQuantity(itemID);
	}

	public void AddPurchaseItem(int itemID, int quantity, string purchaseSourceName = null)
	{
		if (pIsReady)
		{
			pData.AddPurchaseItem(itemID, quantity, purchaseSourceName);
		}
	}

	public void DoPurchase(int currencyType, int storeID, PurchaseEventHandler callback)
	{
		if (pIsReady)
		{
			pData.DoPurchase(this, currencyType, storeID, callback);
		}
	}

	public void DoPurchase(int currencyType, int storeID, bool isMysteryChest, PurchaseEventHandler callback)
	{
		if (pIsReady)
		{
			pData.DoPurchase(this, currencyType, storeID, isMysteryChest, callback);
		}
	}
}
