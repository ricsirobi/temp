using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UR", Namespace = "")]
public class UserRoom
{
	[XmlElement(ElementName = "N")]
	public string Name;

	[XmlElement(ElementName = "R")]
	public string RoomID;

	[XmlElement(ElementName = "CP")]
	public double CreativePoints;

	[XmlElement(ElementName = "C")]
	public int? CategoryID;

	[XmlElement(ElementName = "IID")]
	public int? ItemID;

	[XmlElement(ElementName = "SN")]
	public string SceneName;

	private static bool mIsReady = false;

	public static Dictionary<int, List<UserRoom>> pUserRoomList = new Dictionary<int, List<UserRoom>>();

	private UserRoomSaveEventHandler mSaveCallback;

	private static UserRoomLoadEventHandler mLoadCallback;

	private UserRoomMaxCretivePointsEventHandler mMaxCretivePointsCallback;

	private int mInventoryID = -1;

	private string mLocaleName;

	private const int LOG_PRIORITY = 100;

	[XmlIgnore]
	public int InventoryID
	{
		get
		{
			if (mInventoryID == -1 && !string.IsNullOrEmpty(RoomID))
			{
				int.TryParse(RoomID, out mInventoryID);
			}
			return mInventoryID;
		}
		set
		{
			mInventoryID = value;
		}
	}

	[XmlIgnore]
	public int pItemID
	{
		get
		{
			if (!ItemID.HasValue && InventoryID != -1)
			{
				UserItemData userItemData = CommonInventoryData.pInstance.FindItemByUserInventoryID(InventoryID);
				if (userItemData != null)
				{
					ItemID = userItemData.Item.ItemID;
				}
			}
			if (ItemID.HasValue)
			{
				return ItemID.Value;
			}
			return 0;
		}
		set
		{
			ItemID = value;
		}
	}

	[XmlIgnore]
	public string pLocaleName
	{
		get
		{
			if (string.IsNullOrEmpty(Name))
			{
				if (string.IsNullOrEmpty(mLocaleName))
				{
					UserItemData userItemData = CommonInventoryData.pInstance.FindItem(pItemID);
					if (userItemData == null)
					{
						userItemData = ParentData.pInstance.pInventory.pData.FindItem(pItemID);
					}
					if (userItemData != null)
					{
						mLocaleName = userItemData.Item.ItemName;
					}
				}
			}
			else if (string.IsNullOrEmpty(mLocaleName))
			{
				mLocaleName = Name;
			}
			return mLocaleName;
		}
		set
		{
			mLocaleName = value;
		}
	}

	public static bool pIsReady
	{
		get
		{
			return mIsReady;
		}
		set
		{
			mIsReady = value;
		}
	}

	public void SetRoomName(string nameString)
	{
		if (nameString != mLocaleName)
		{
			Name = nameString;
			pLocaleName = nameString;
		}
	}

	public static void Init()
	{
		UtDebug.Log("Init UserRooms ... ", 100);
		pUserRoomList.Clear();
		mIsReady = true;
	}

	public static List<UserRoom> GetRooms(int category)
	{
		if (pUserRoomList.ContainsKey(category))
		{
			return pUserRoomList[category];
		}
		return null;
	}

	public static void LoadRooms(int category, bool force, UserRoomLoadEventHandler callback)
	{
		mLoadCallback = callback;
		if (!force && pUserRoomList.ContainsKey(category))
		{
			if (mLoadCallback != null)
			{
				mLoadCallback(success: true);
				mLoadCallback = null;
			}
		}
		else
		{
			mIsReady = false;
			WsWebService.GetUserRoomList(UserInfo.pInstance.UserID, category, GetRoomServiceEventHandler, category);
		}
	}

	public static void AddRoom(UserRoom fd, int categoryID)
	{
		if (!pUserRoomList.ContainsKey(categoryID))
		{
			pUserRoomList[categoryID] = new List<UserRoom>();
		}
		pUserRoomList[categoryID].Add(fd);
	}

	public static UserRoom AddRoom(UserItemData userItem, int categoryID)
	{
		UserRoom obj = new UserRoom
		{
			pItemID = userItem.Item.ItemID,
			InventoryID = userItem.UserInventoryID,
			pLocaleName = userItem.Item.ItemName,
			CategoryID = categoryID
		};
		obj.RoomID = obj.InventoryID.ToString();
		AddRoom(obj, categoryID);
		return obj;
	}

	public void Save(UserRoomSaveEventHandler callback)
	{
		mSaveCallback = callback;
		WsWebService.SetUserRoom(this, ServiceEventHandler, null);
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.SET_USER_ROOM)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			UserRoomSetResponse userRoomSetResponse = (UserRoomSetResponse)inObject;
			if (mSaveCallback != null)
			{
				mSaveCallback(userRoomSetResponse.Success, userRoomSetResponse);
				mSaveCallback = null;
			}
			break;
		}
		case WsServiceEvent.ERROR:
			if (mSaveCallback != null)
			{
				mSaveCallback(success: false, null);
				mSaveCallback = null;
			}
			break;
		}
	}

	public static void GetRoomServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.GET_USER_ROOM_LIST)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				int key2 = (int)inUserData;
				if (!pUserRoomList.ContainsKey(key2))
				{
					pUserRoomList[key2] = new List<UserRoom>();
				}
				List<UserRoom> list = pUserRoomList[key2];
				list.Clear();
				UserRoomResponse userRoomResponse = (UserRoomResponse)inObject;
				if (userRoomResponse != null)
				{
					list.AddRange(userRoomResponse.UserRoomList);
				}
			}
			mIsReady = true;
			if (mLoadCallback != null)
			{
				mLoadCallback(success: true);
				mLoadCallback = null;
			}
			break;
		case WsServiceEvent.ERROR:
		{
			int key = (int)inUserData;
			if (!pUserRoomList.ContainsKey(key))
			{
				pUserRoomList[key] = new List<UserRoom>();
			}
			pUserRoomList[key].Clear();
			mIsReady = true;
			if (mLoadCallback != null)
			{
				mLoadCallback(success: false);
				mLoadCallback = null;
			}
			break;
		}
		}
	}

	public static void OnRoomItemDataLoaded(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null && inUserData is UserRoom userRoom)
		{
			userRoom.pLocaleName = dataItem.ItemName;
		}
	}

	public static UserRoom GetByInventoryID(int inventoryID)
	{
		UserRoom userRoom = null;
		if (pUserRoomList != null)
		{
			foreach (List<UserRoom> value in pUserRoomList.Values)
			{
				if (value != null)
				{
					userRoom = value.Find((UserRoom room) => room.InventoryID == inventoryID);
					if (userRoom != null)
					{
						return userRoom;
					}
				}
			}
		}
		return userRoom;
	}

	public static UserRoom GetByRoomID(string id)
	{
		UserRoom userRoom = null;
		if (pUserRoomList != null)
		{
			foreach (List<UserRoom> value in pUserRoomList.Values)
			{
				if (value != null)
				{
					userRoom = value.Find((UserRoom room) => room.RoomID == id);
					if (userRoom != null)
					{
						return userRoom;
					}
				}
			}
		}
		return userRoom;
	}

	public int MaxCreativePointsLimit()
	{
		int result = 0;
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(pItemID);
		if (userItemData == null)
		{
			userItemData = ParentData.pInstance.pInventory.pData.FindItem(pItemID);
		}
		if (userItemData != null && userItemData.Item != null)
		{
			result = userItemData.Item.CreativePoints;
		}
		return result;
	}

	public void MaxCreativePointsLimit(UserRoomMaxCretivePointsEventHandler callback)
	{
		if (callback != null)
		{
			mMaxCretivePointsCallback = callback;
		}
		ItemData.Load(pItemID, ItemDataEventHandler, null);
	}

	private void ItemDataEventHandler(int itemID, ItemData dataItem, object inUserData)
	{
		int creativePoints = 0;
		if (dataItem != null)
		{
			creativePoints = dataItem.CreativePoints;
		}
		if (mMaxCretivePointsCallback != null)
		{
			mMaxCretivePointsCallback(creativePoints);
			mMaxCretivePointsCallback = null;
		}
	}
}
