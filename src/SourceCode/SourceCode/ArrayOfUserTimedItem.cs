using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserTimedItem", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfUserTimedItem
{
	private class UserTimedItemUserData
	{
		public UserTimedItem mItem;

		public UserTimedItemEventHandler mCallback;
	}

	[XmlElement(ElementName = "UserTimedItem")]
	public UserTimedItem[] UserTimedItem;

	private static List<UserTimedItem> mList;

	private static ArrayOfUserTimedItem mInstance;

	public static UserTimedItem[] pList => mList.ToArray();

	public static bool pIsReady => mInstance != null;

	public static void Init(string inUserID)
	{
		mInstance = null;
		WsWebService.GetUserTimedItems(inUserID, ServiceEventHandler, null);
	}

	private static void InitDefault()
	{
		mInstance = new ArrayOfUserTimedItem();
		mList = new List<UserTimedItem>();
	}

	public static void Save(UserTimedItem item, UserTimedItemEventHandler callback)
	{
		Save(item, UserTimedItemFlag.NONE, callback);
	}

	public static void Save(UserTimedItem item, UserTimedItemFlag flag, UserTimedItemEventHandler callback)
	{
		UserTimedItemUserData userTimedItemUserData = new UserTimedItemUserData();
		userTimedItemUserData.mItem = item;
		userTimedItemUserData.mCallback = callback;
		WsWebService.SetUserTimedItem(item, flag, ServiceEventHandler, userTimedItemUserData);
	}

	public static void Delete(UserTimedItem item, UserTimedItemEventHandler callback)
	{
		UserTimedItemUserData userTimedItemUserData = new UserTimedItemUserData();
		userTimedItemUserData.mItem = item;
		userTimedItemUserData.mCallback = callback;
		WsWebService.DeleteUserTimedItem(item.UserTimedItemID.Value, ServiceEventHandler, userTimedItemUserData);
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_USER_TIMED_ITEM_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mInstance = (ArrayOfUserTimedItem)inObject;
				if (mInstance != null)
				{
					if (mInstance.UserTimedItem != null)
					{
						mList = new List<UserTimedItem>(mInstance.UserTimedItem);
					}
					else
					{
						mList = new List<UserTimedItem>();
					}
				}
				else
				{
					InitDefault();
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("Get User Timed Item Failed!");
				if (mInstance == null)
				{
					InitDefault();
				}
				break;
			}
			break;
		case WsServiceType.SET_USER_TIMED_ITEM:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				UserTimedItem userTimedItem = (UserTimedItem)inObject;
				UserTimedItemUserData userTimedItemUserData4 = (UserTimedItemUserData)inUserData;
				if (userTimedItem != null)
				{
					mList.Remove(userTimedItemUserData4.mItem);
					mList.Add(userTimedItem);
					if (userTimedItemUserData4.mCallback != null)
					{
						userTimedItemUserData4.mCallback(success: true, userTimedItem);
					}
				}
				else
				{
					UtDebug.LogError("Set User Timed Item Failed!");
					if (userTimedItemUserData4.mCallback != null)
					{
						userTimedItemUserData4.mCallback(success: false, null);
					}
				}
				break;
			}
			case WsServiceEvent.ERROR:
			{
				UtDebug.LogError("Set User Timed Item Failed!");
				UserTimedItemUserData userTimedItemUserData3 = (UserTimedItemUserData)inUserData;
				if (userTimedItemUserData3.mCallback != null)
				{
					userTimedItemUserData3.mCallback(success: false, null);
				}
				break;
			}
			}
			break;
		case WsServiceType.DELETE_USER_TIMED_ITEM:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				bool num = (bool)inObject;
				UserTimedItemUserData userTimedItemUserData2 = (UserTimedItemUserData)inUserData;
				if (num)
				{
					mList.Remove(userTimedItemUserData2.mItem);
					if (userTimedItemUserData2.mCallback != null)
					{
						userTimedItemUserData2.mCallback(success: true, null);
					}
				}
				else
				{
					UtDebug.LogError("Delete User Timed Item Failed!");
					if (userTimedItemUserData2.mCallback != null)
					{
						userTimedItemUserData2.mCallback(success: false, null);
					}
				}
				break;
			}
			case WsServiceEvent.ERROR:
			{
				UtDebug.LogError("Set User Timed Item Failed!");
				UserTimedItemUserData userTimedItemUserData = (UserTimedItemUserData)inUserData;
				if (userTimedItemUserData.mCallback != null)
				{
					userTimedItemUserData.mCallback(success: false, null);
				}
				break;
			}
			}
			break;
		}
	}
}
