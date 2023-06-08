using System.Collections.Generic;

public class ParentData
{
	private static ParentData mInstance;

	private CommonInventoryDataInstance mInventory;

	private UserInfo mUserInfo;

	public bool pUserMessageReady;

	public bool pUserInfoReady;

	private ArrayOfMessageInfo mMessageObject = new ArrayOfMessageInfo();

	public static ParentData pInstance => mInstance;

	public CommonInventoryDataInstance pInventory => mInventory;

	public UserInfo pUserInfo => mUserInfo;

	public Dictionary<int, PairDataInstance> pPairData { get; private set; }

	public static bool pIsReady
	{
		get
		{
			if (mInstance != null && mInstance.mInventory != null && mInstance.mInventory.pIsReady && mInstance.pUserMessageReady)
			{
				return mInstance.pUserInfoReady;
			}
			return false;
		}
	}

	public static void Init()
	{
		if (mInstance != null)
		{
			return;
		}
		mInstance = new ParentData();
		mInstance.pPairData = new Dictionary<int, PairDataInstance>();
		mInstance.mInventory = CommonInventoryDataInstance.LoadByToken(ProductConfig.pToken);
		mInstance.pUserMessageReady = false;
		mInstance.pUserInfoReady = false;
		string pUserToken = WsWebService.pUserToken;
		WsWebService.SetToken(ProductConfig.pToken);
		WsWebService.GetUserMessageQueue(showOld: false, showDeleted: false, mInstance.ServiceEventHandler, null);
		if (mInstance.mUserInfo == null)
		{
			if (ProductConfig.pToken == UserInfo.pUserToken)
			{
				mInstance.mUserInfo = UserInfo.pInstance;
				mInstance.pUserInfoReady = true;
			}
			else
			{
				WsWebService.GetUserInfoByApiToken(mInstance.ServiceEventHandler, null);
			}
		}
		WsWebService.SetToken(pUserToken);
	}

	public void LoadPairData(int pairID, PairDataEventHandler inCallback = null, bool forceLoad = false, object inUserData = null)
	{
		if (mInstance != null)
		{
			PairDataInstance pairDataInstance = null;
			if (mInstance.pPairData.ContainsKey(pairID) && (!forceLoad || mInstance.pPairData[pairID].pIsLoading))
			{
				pairDataInstance = mInstance.pPairData[pairID];
			}
			else
			{
				pairDataInstance = new PairDataInstance();
				mInstance.pPairData[pairID] = pairDataInstance;
			}
			pairDataInstance.Load(pairID, inCallback, inUserData, mInstance.pUserInfo.UserID);
		}
	}

	public PairData GetPairDataByID(int pairID)
	{
		PairData result = null;
		if (mInstance != null && mInstance.pPairData != null && mInstance.pPairData.ContainsKey(pairID))
		{
			result = mInstance.pPairData[pairID]._Data;
		}
		return result;
	}

	public PairDataInstance GetPairDataInstanceByID(int pairID)
	{
		PairDataInstance result = null;
		if (mInstance != null && mInstance.pPairData != null && mInstance.pPairData.ContainsKey(pairID))
		{
			result = mInstance.pPairData[pairID];
		}
		return result;
	}

	public void UpdatePairData(int pairID, string inKey, string inValue)
	{
		GetPairDataInstanceByID(pairID)?._Data.SetValue(inKey, inValue);
	}

	public void SavePairData(int pairID, PairDataEventHandler inCallback = null, object inUserData = null)
	{
		if (mInstance != null)
		{
			if (pPairData.ContainsKey(pairID))
			{
				pPairData[pairID].Save(mInstance.pUserInfo.UserID, inCallback, inUserData);
				return;
			}
			UtDebug.LogError("PairData[" + pairID + "] does not exist.");
			inCallback?.Invoke(success: false, null, inUserData);
		}
	}

	public static void ReInitInventory()
	{
		if (mInstance != null)
		{
			mInstance.mInventory = CommonInventoryDataInstance.LoadByToken(ProductConfig.pToken);
		}
	}

	public static void Reset()
	{
		if (pIsReady)
		{
			mInstance.mInventory = null;
			mInstance.mUserInfo = null;
			mInstance.pPairData = null;
			mInstance = null;
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_USER_MESSAGE_QUEUE:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				pUserMessageReady = true;
				if (inObject != null)
				{
					mMessageObject = (ArrayOfMessageInfo)inObject;
				}
				break;
			case WsServiceEvent.ERROR:
				pUserMessageReady = true;
				break;
			}
			break;
		case WsServiceType.GET_USERINFO_BY_TOKEN:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				pUserInfoReady = true;
				mUserInfo = (UserInfo)inObject;
				break;
			case WsServiceEvent.ERROR:
				pUserInfoReady = true;
				break;
			}
			break;
		}
	}

	public bool HasItem(int inItemID)
	{
		if (mInventory.pData.FindItem(inItemID) != null)
		{
			return true;
		}
		return false;
	}

	public ArrayOfMessageInfo GetMessages()
	{
		return mMessageObject;
	}

	public void RemoveMessage(int? inUserMessageQueueID)
	{
		if (inUserMessageQueueID.HasValue)
		{
			List<MessageInfo> list = new List<MessageInfo>(mMessageObject.MessageInfo);
			list.RemoveAll((MessageInfo m) => m.UserMessageQueueID.HasValue && m.UserMessageQueueID.Value == inUserMessageQueueID.Value);
			mMessageObject.MessageInfo = list.ToArray();
		}
	}

	public bool IsAgeEligible(int eligibleAge)
	{
		return (mInstance.pUserInfo.Age.HasValue ? mInstance.pUserInfo.Age.Value : 0) >= eligibleAge;
	}
}
