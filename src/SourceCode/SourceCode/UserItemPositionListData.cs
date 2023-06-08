using System;
using System.Collections.Generic;
using UnityEngine;

public class UserItemPositionListData
{
	internal class CreateListSort : IComparer<UserItemPositionSetRequest>
	{
		public int Compare(UserItemPositionSetRequest inFirst, UserItemPositionSetRequest inSecond)
		{
			int num = (inFirst.ParentIndex.HasValue ? inFirst.ParentIndex.Value : (-1));
			int num2 = (inSecond.ParentIndex.HasValue ? inSecond.ParentIndex.Value : (-1));
			if (num < num2)
			{
				return -1;
			}
			if (num2 < num)
			{
				return 1;
			}
			return 0;
		}
	}

	internal class RemoveListSort : IComparer<UserItemPosition>
	{
		public int Compare(UserItemPosition inFirst, UserItemPosition inSecond)
		{
			int num = (inFirst.ParentID.HasValue ? inFirst.ParentID.Value : (-1));
			int num2 = (inSecond.ParentID.HasValue ? inSecond.ParentID.Value : (-1));
			if (num > num2)
			{
				return -1;
			}
			if (num2 > num)
			{
				return 1;
			}
			return 0;
		}
	}

	private List<UserItemPositionSetRequest> mCreateObjectList = new List<UserItemPositionSetRequest>();

	private List<UserItemPositionSetRequest> mUpdateObjectList = new List<UserItemPositionSetRequest>();

	private List<UserItemPosition> mRemoveObjectList = new List<UserItemPosition>();

	private bool mIsSortingNeeded = true;

	private UserItemPositionEventHandler mEventDelegate;

	private List<UserItemPosition> mList;

	private string mRoomID = "";

	public UserItemPosition[] pList
	{
		get
		{
			if (mList == null)
			{
				return null;
			}
			return mList.ToArray();
		}
	}

	public UserItemPositionListData(string inroomId)
	{
		mRoomID = inroomId;
	}

	public void Init(string inUserID, UserItemPositionEventHandler inEventDelegate)
	{
		mEventDelegate = inEventDelegate;
		WsWebService.GetUserItemPositions(inUserID, mRoomID, ServiceEventHandler, null);
	}

	private void InitDefault()
	{
		mList = new List<UserItemPosition>();
	}

	public bool Save(UserItemPositionEventHandler inEventDelegate)
	{
		if (mRemoveObjectList.Count == 0 && mCreateObjectList.Count == 0 && mUpdateObjectList.Count == 0)
		{
			return false;
		}
		mEventDelegate = inEventDelegate;
		List<int> list = new List<int>();
		mRemoveObjectList.Sort(new RemoveListSort());
		foreach (UserItemPosition mRemoveObject in mRemoveObjectList)
		{
			list.Add(mRemoveObject.UserItemPositionID.Value);
		}
		List<UserItemPositionSetRequest> inSortedList = new List<UserItemPositionSetRequest>(mCreateObjectList);
		if (mIsSortingNeeded)
		{
			mIsSortingNeeded = false;
			inSortedList.Sort(new CreateListSort());
			ReLinkCorrectParentIndex(ref inSortedList, mCreateObjectList);
			ReLinkUpdateListParentIndex(inSortedList);
		}
		mCreateObjectList = inSortedList;
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		WsWebService.SetUserItemPositions(mRoomID, inSortedList.ToArray(), mUpdateObjectList.ToArray(), list.ToArray(), ServiceEventHandler, null);
		return true;
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE || inEvent == WsServiceEvent.ERROR)
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		}
		switch (inType)
		{
		case WsServiceType.GET_USER_ITEM_POSITION_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				UserItemPositionList userItemPositionList = (UserItemPositionList)inObject;
				if (userItemPositionList != null)
				{
					if (userItemPositionList.UserItemPosition != null)
					{
						mList = new List<UserItemPosition>(userItemPositionList.UserItemPosition);
					}
					else
					{
						mList = new List<UserItemPosition>();
					}
				}
				else
				{
					InitDefault();
				}
				if (mEventDelegate != null)
				{
					mEventDelegate(inType, inEvent, mRoomID);
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UtDebug.LogError("Get User Item Position Failed!");
				InitDefault();
				if (mEventDelegate != null)
				{
					mEventDelegate(inType, inEvent, mRoomID);
				}
				break;
			}
			break;
		case WsServiceType.SET_USER_ITEM_POSITION_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (mList != null)
				{
					UserItemPositionSetResponse userItemPositionSetResponse = (UserItemPositionSetResponse)inObject;
					if (userItemPositionSetResponse != null && !userItemPositionSetResponse.Success)
					{
						if (mEventDelegate != null)
						{
							mEventDelegate(inType, WsServiceEvent.ERROR, mRoomID);
						}
						UtDebug.LogError("Set User Item Position Failed!");
						break;
					}
					if (userItemPositionSetResponse != null && userItemPositionSetResponse.CreatedUserItemPositionIDs != null && mCreateObjectList.Count == userItemPositionSetResponse.CreatedUserItemPositionIDs.Length)
					{
						for (int i = 0; i < mCreateObjectList.Count; i++)
						{
							mCreateObjectList[i].UserItemPositionID = userItemPositionSetResponse.CreatedUserItemPositionIDs[i];
							int? parentIndex = mCreateObjectList[i].ParentIndex;
							if (parentIndex.HasValue)
							{
								mCreateObjectList[i].ParentID = mCreateObjectList[parentIndex.Value].UserItemPositionID;
								mCreateObjectList[i].ParentIndex = null;
							}
							if (userItemPositionSetResponse.UserItemStates == null)
							{
								continue;
							}
							UserItemState[] userItemStates = userItemPositionSetResponse.UserItemStates;
							foreach (UserItemState userItemState in userItemStates)
							{
								if (mCreateObjectList[i].UserItemPositionID == userItemState.UserItemPositionID)
								{
									mCreateObjectList[i]._GameObject.SendMessage("SetState", userItemState);
								}
							}
						}
						List<UserItemPosition> list = mList;
						UserItemPosition[] collection = mCreateObjectList.ToArray();
						list.AddRange(collection);
					}
					foreach (UserItemPositionSetRequest mUpdateObject in mUpdateObjectList)
					{
						foreach (UserItemPosition m in mList)
						{
							if (mUpdateObject.UserItemPositionID == m.UserItemPositionID)
							{
								m.PositionX = mUpdateObject.PositionX;
								m.PositionY = mUpdateObject.PositionY;
								m.PositionZ = mUpdateObject.PositionZ;
								m.RotationX = mUpdateObject.RotationX;
								m.RotationY = mUpdateObject.RotationY;
								m.RotationZ = mUpdateObject.RotationZ;
								if (mUpdateObject.ParentIndex.HasValue)
								{
									m.ParentID = mCreateObjectList[mUpdateObject.ParentIndex.Value].UserItemPositionID;
								}
							}
						}
					}
					ClearRemoveObjectList();
				}
				mIsSortingNeeded = true;
				mCreateObjectList.Clear();
				mUpdateObjectList.Clear();
				if (mEventDelegate != null)
				{
					mEventDelegate(inType, inEvent, mRoomID);
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("Set User Item Position Failed!");
				if (mEventDelegate != null)
				{
					mEventDelegate(inType, inEvent, mRoomID);
				}
				break;
			}
			break;
		}
	}

	public void ClearRemoveObjectList()
	{
		foreach (UserItemPosition mRemoveObject in mRemoveObjectList)
		{
			mList.Remove(mRemoveObject);
		}
		mRemoveObjectList.Clear();
	}

	public int? GetUserItemPositionID(GameObject inGameObject)
	{
		return GetUserItemPositionData(inGameObject)?.UserItemPositionID;
	}

	public GameObject GetGameObjectReference(int? inUserItemPositionID)
	{
		if (inUserItemPositionID.HasValue)
		{
			UserItemPosition userItemPositionData = GetUserItemPositionData(inUserItemPositionID.Value);
			if (userItemPositionData != null)
			{
				return userItemPositionData._GameObject;
			}
		}
		return null;
	}

	public UserItemPosition GetUserItemPositionData(GameObject inGameObject)
	{
		if (inGameObject != null && mList != null)
		{
			foreach (UserItemPosition m in mList)
			{
				if (m._GameObject == inGameObject)
				{
					return m;
				}
			}
		}
		return null;
	}

	private UserItemPosition GetUserItemPositionData(int inUserItemPositionID)
	{
		if (mList != null)
		{
			foreach (UserItemPosition m in mList)
			{
				if (inUserItemPositionID == m.UserItemPositionID.Value)
				{
					return m;
				}
			}
		}
		return null;
	}

	private int GetUserItemPositionIndex(UserItemPosition[] inList, GameObject inGameObject)
	{
		int num = 0;
		bool flag = false;
		if (inGameObject != null)
		{
			for (int i = 0; i < inList.Length; i++)
			{
				if (inList[i]._GameObject == inGameObject)
				{
					flag = true;
					break;
				}
				num++;
			}
		}
		if (!flag)
		{
			num = -1;
		}
		return num;
	}

	private UserItemPosition CheckItemAvailablityFromRemoveList(int itemID)
	{
		foreach (UserItemPosition mRemoveObject in mRemoveObjectList)
		{
			if (mRemoveObject.Item != null && mRemoveObject.Item.ItemID == itemID)
			{
				return mRemoveObject;
			}
		}
		return null;
	}

	private void PopulateObjectData(bool isCreateEntry, ref UserItemPositionSetRequest inItem, GameObject inGameObject, UserItemData inUserItemData, Vector3 inPosition, Vector3 inRotation, GameObject inParentObject)
	{
		inItem._GameObject = inGameObject;
		if (inUserItemData != null)
		{
			if (inItem.Item == null)
			{
				inItem.Item = inUserItemData.Item;
				inItem.ItemID = inUserItemData.ItemID;
			}
			inItem.UserInventoryCommonID = inUserItemData.UserInventoryID;
		}
		else if (!inItem.UserInventoryCommonID.HasValue)
		{
			UtDebug.LogError("ERROR!! UserInventoryCommonID required!");
		}
		inItem.PositionX = Math.Round(inPosition.x, 3);
		inItem.PositionY = Math.Round(inPosition.y, 3);
		inItem.PositionZ = Math.Round(inPosition.z, 3);
		inItem.RotationX = Math.Round(inRotation.x, 3);
		inItem.RotationY = Math.Round(inRotation.y, 3);
		inItem.RotationZ = Math.Round(inRotation.z, 3);
		inItem.ParentIndex = null;
		inItem.ParentID = null;
		if (!(inParentObject != null))
		{
			return;
		}
		int userItemPositionIndex = GetUserItemPositionIndex(pList, inParentObject);
		if (userItemPositionIndex != -1)
		{
			inItem.ParentID = pList[userItemPositionIndex].UserItemPositionID;
			return;
		}
		UserItemPosition[] inList;
		if (isCreateEntry)
		{
			inList = mCreateObjectList.ToArray();
			int userItemPositionIndex2 = GetUserItemPositionIndex(inList, inParentObject);
			if (userItemPositionIndex2 != -1)
			{
				inItem.ParentIndex = userItemPositionIndex2;
			}
			return;
		}
		inList = mUpdateObjectList.ToArray();
		int userItemPositionIndex3 = GetUserItemPositionIndex(inList, inParentObject);
		if (userItemPositionIndex3 != -1)
		{
			inItem.ParentID = mUpdateObjectList[userItemPositionIndex3].UserItemPositionID;
			return;
		}
		inList = mCreateObjectList.ToArray();
		int userItemPositionIndex4 = GetUserItemPositionIndex(inList, inParentObject);
		if (userItemPositionIndex4 != -1)
		{
			inItem.ParentIndex = userItemPositionIndex4;
		}
	}

	public void CreateObject(GameObject inGameObject, UserItemData inUserItemData, GameObject inParentObject)
	{
		CreateObject(inGameObject, inUserItemData, inGameObject.transform.position, inGameObject.transform.rotation.eulerAngles, inParentObject);
	}

	public void CreateObject(GameObject inGameObject, UserItemData inUserItemData, Vector3 inPosition, Vector3 inRotation, GameObject inParentObject)
	{
		UserItemPositionSetRequest inItem = new UserItemPositionSetRequest();
		UserItemPosition userItemPosition = CheckItemAvailablityFromRemoveList(inUserItemData.Item.ItemID);
		if (userItemPosition == null)
		{
			PopulateObjectData(isCreateEntry: true, ref inItem, inGameObject, inUserItemData, inPosition, inRotation, inParentObject);
			mCreateObjectList.Add(inItem);
			return;
		}
		PopulateObjectData(isCreateEntry: false, ref inItem, inGameObject, inUserItemData, inPosition, inRotation, inParentObject);
		inItem.UserItemPositionID = userItemPosition.UserItemPositionID;
		userItemPosition._GameObject = inGameObject;
		mUpdateObjectList.Add(inItem);
		mRemoveObjectList.Remove(userItemPosition);
	}

	public void UpdateObject(GameObject inGameObject, UserItemData inUserItemData, GameObject inParentObject)
	{
		UpdateObject(inGameObject, inUserItemData, inGameObject.transform.position, inGameObject.transform.rotation.eulerAngles, inParentObject);
	}

	public void UpdateObject(GameObject inGameObject, UserItemData inUserItemData, Vector3 inPosition, Vector3 inRotation, GameObject inParentObject)
	{
		if (!(inGameObject != null))
		{
			return;
		}
		UserItemPosition[] inList = mUpdateObjectList.ToArray();
		int userItemPositionIndex = GetUserItemPositionIndex(inList, inGameObject);
		if (userItemPositionIndex != -1)
		{
			UserItemPositionSetRequest inItem = mUpdateObjectList[userItemPositionIndex];
			PopulateObjectData(isCreateEntry: false, ref inItem, inGameObject, inUserItemData, inPosition, inRotation, inParentObject);
			return;
		}
		inList = mCreateObjectList.ToArray();
		int userItemPositionIndex2 = GetUserItemPositionIndex(inList, inGameObject);
		if (userItemPositionIndex2 != -1)
		{
			UserItemPositionSetRequest inItem2 = mCreateObjectList[userItemPositionIndex2];
			PopulateObjectData(isCreateEntry: true, ref inItem2, inGameObject, inUserItemData, inPosition, inRotation, inParentObject);
			return;
		}
		UserItemPosition userItemPositionData = GetUserItemPositionData(inGameObject);
		if (userItemPositionData != null)
		{
			UserItemPositionSetRequest inItem3 = new UserItemPositionSetRequest();
			if (inUserItemData == null)
			{
				inUserItemData = new UserItemData();
				inUserItemData.UserInventoryID = userItemPositionData.UserInventoryCommonID.Value;
			}
			else if (inUserItemData.UserInventoryID != userItemPositionData.UserInventoryCommonID)
			{
				Debug.LogError("Error!! inUserItemData must be the same ItemID as before to go in UpdateObjectList!");
			}
			PopulateObjectData(isCreateEntry: false, ref inItem3, inGameObject, inUserItemData, inPosition, inRotation, inParentObject);
			inItem3.UserItemPositionID = userItemPositionData.UserItemPositionID;
			mUpdateObjectList.Add(inItem3);
		}
		else
		{
			UserItemPositionSetRequest inItem4 = new UserItemPositionSetRequest();
			PopulateObjectData(isCreateEntry: true, ref inItem4, inGameObject, inUserItemData, inPosition, inRotation, inParentObject);
			mCreateObjectList.Add(inItem4);
		}
	}

	public void RemoveObject(GameObject inGameObject)
	{
		if (!(inGameObject != null))
		{
			return;
		}
		UserItemPosition[] inList = mUpdateObjectList.ToArray();
		int userItemPositionIndex = GetUserItemPositionIndex(inList, inGameObject);
		if (userItemPositionIndex != -1)
		{
			AddItemToRemoveList(mUpdateObjectList[userItemPositionIndex].UserItemPositionID);
			mUpdateObjectList.RemoveAt(userItemPositionIndex);
			return;
		}
		inList = mCreateObjectList.ToArray();
		int userItemPositionIndex2 = GetUserItemPositionIndex(inList, inGameObject);
		if (userItemPositionIndex2 != -1)
		{
			mCreateObjectList.RemoveAt(userItemPositionIndex2);
			return;
		}
		UserItemPosition userItemPositionData = GetUserItemPositionData(inGameObject);
		if (userItemPositionData != null)
		{
			if (!mRemoveObjectList.Contains(userItemPositionData))
			{
				mRemoveObjectList.Add(userItemPositionData);
				return;
			}
			int? userItemPositionID = userItemPositionData.UserItemPositionID;
			UtDebug.LogError("ERROR: OBJECT REMOVED MULTIPLE TIMES FOR USER ITEM POSITION ID: " + userItemPositionID);
		}
		else
		{
			UtDebug.LogError("ERROR: NULL REFERENCE RETURNED FOR USER ITEM POSITION DATA: " + inGameObject.name);
		}
	}

	public void RemoveObject(UserItemPosition inUserItemPosition)
	{
		if (inUserItemPosition == null)
		{
			return;
		}
		UserItemPosition[] inList = mUpdateObjectList.ToArray();
		int userItemPositionIndex = GetUserItemPositionIndex(inList, inUserItemPosition._GameObject);
		if (userItemPositionIndex != -1)
		{
			AddItemToRemoveList(mUpdateObjectList[userItemPositionIndex].UserItemPositionID);
			mUpdateObjectList.RemoveAt(userItemPositionIndex);
			return;
		}
		inList = mCreateObjectList.ToArray();
		int userItemPositionIndex2 = GetUserItemPositionIndex(inList, inUserItemPosition._GameObject);
		if (userItemPositionIndex2 != -1)
		{
			mCreateObjectList.RemoveAt(userItemPositionIndex2);
		}
		else
		{
			AddItemToRemoveList(inUserItemPosition.UserItemPositionID);
		}
	}

	private void AddItemToRemoveList(int? inUserItemPositionID)
	{
		if (!inUserItemPositionID.HasValue)
		{
			return;
		}
		UserItemPosition userItemPositionData = GetUserItemPositionData(inUserItemPositionID.Value);
		if (userItemPositionData != null)
		{
			if (!mRemoveObjectList.Contains(userItemPositionData))
			{
				mRemoveObjectList.Add(userItemPositionData);
				return;
			}
			int? num = inUserItemPositionID;
			UtDebug.LogError("ERROR: OBJECT REMOVED MULTIPLE TIMES FOR USER ITEM POSITION ID: " + num);
		}
		else
		{
			int? num = inUserItemPositionID;
			UtDebug.LogError("ERROR: NULL REFERENCE RETURNED FOR USER ITEM POSITION ID: " + num);
		}
	}

	private void ReLinkCorrectParentIndex(ref List<UserItemPositionSetRequest> inSortedList, List<UserItemPositionSetRequest> inParentReferenceList)
	{
		if (inParentReferenceList.Count != inSortedList.Count)
		{
			UtDebug.LogError("ERROR: ReLinkCorrectParentIndex -> SORTED LIST AND PARENT REFERENCE LIST SIZES ARE MISMATCHED!!");
			return;
		}
		for (int i = 0; i < inSortedList.Count; i++)
		{
			UserItemPositionSetRequest userItemPositionSetRequest = inSortedList[i];
			if (userItemPositionSetRequest.ParentIndex.HasValue)
			{
				UserItemPositionSetRequest parentItem = inParentReferenceList[userItemPositionSetRequest.ParentIndex.Value];
				userItemPositionSetRequest.ParentIndex = inSortedList.FindIndex((UserItemPositionSetRequest inRequest) => inRequest._GameObject == parentItem._GameObject);
				if (userItemPositionSetRequest.ParentIndex == -1)
				{
					string text = ((userItemPositionSetRequest._GameObject != null) ? userItemPositionSetRequest._GameObject.name : "NULL");
					string text2 = ((parentItem._GameObject != null) ? parentItem._GameObject.name : "NULL");
					UtDebug.LogError("ERROR: ReLinkCorrectParentIndex I:" + text + ", P:" + text2 + " -> SORTED ITEM PARENT REFERENCE NOT FOUND IN SORTED LIST!!");
					userItemPositionSetRequest.ParentIndex = null;
				}
				else if (userItemPositionSetRequest.ParentIndex >= i)
				{
					string text3 = ((userItemPositionSetRequest._GameObject != null) ? userItemPositionSetRequest._GameObject.name : "NULL");
					string text4 = ((parentItem._GameObject != null) ? parentItem._GameObject.name : "NULL");
					UtDebug.LogError("ERROR: ReLinkCorrectParentIndex I:" + text3 + ", P:" + text4 + " -> SORTED ITEM PARENT REFERENCE IS GREATER OR EQUAL TO ITEM!!");
					userItemPositionSetRequest.ParentIndex = null;
				}
			}
		}
	}

	private void ReLinkUpdateListParentIndex(List<UserItemPositionSetRequest> inSortedCreateList)
	{
		for (int i = 0; i < mUpdateObjectList.Count; i++)
		{
			UserItemPositionSetRequest userItemPositionSetRequest = mUpdateObjectList[i];
			if (userItemPositionSetRequest.ParentIndex.HasValue)
			{
				UserItemPositionSetRequest parentItem = mCreateObjectList[userItemPositionSetRequest.ParentIndex.Value];
				userItemPositionSetRequest.ParentIndex = inSortedCreateList.FindIndex((UserItemPositionSetRequest inRequest) => inRequest._GameObject == parentItem._GameObject);
				if (userItemPositionSetRequest.ParentIndex == -1)
				{
					string text = ((userItemPositionSetRequest._GameObject != null) ? userItemPositionSetRequest._GameObject.name : "NULL");
					string text2 = ((parentItem._GameObject != null) ? parentItem._GameObject.name : "NULL");
					UtDebug.LogError("ERROR: ReLinkCorrectParentIndex I:" + text + ", P:" + text2 + " -> SORTED ITEM PARENT REFERENCE NOT FOUND IN SORTED LIST!!");
					userItemPositionSetRequest.ParentIndex = null;
				}
			}
		}
	}
}
