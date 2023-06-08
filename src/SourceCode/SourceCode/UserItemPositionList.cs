using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserItemPosition", Namespace = "http://api.jumpstart.com/")]
public class UserItemPositionList
{
	[XmlElement(ElementName = "UserItemPosition")]
	public UserItemPosition[] UserItemPosition;

	private static Dictionary<string, UserItemPositionListData> mListData = new Dictionary<string, UserItemPositionListData>();

	public static UserItemPosition[] GetList(string inRoomID)
	{
		if (!mListData.ContainsKey(inRoomID))
		{
			return null;
		}
		return mListData[inRoomID].pList;
	}

	public static UserItemPositionListData GetInstance(string inRoomID)
	{
		if (mListData.ContainsKey(inRoomID))
		{
			return mListData[inRoomID];
		}
		UserItemPositionListData userItemPositionListData = new UserItemPositionListData(inRoomID);
		mListData.Add(inRoomID, userItemPositionListData);
		return userItemPositionListData;
	}

	public static void Init(string inUserID, string inRoomID, UserItemPositionEventHandler inEventDelegate)
	{
		mListData.Remove(inRoomID);
		GetInstance(inRoomID).Init(inUserID, inEventDelegate);
	}

	public static bool Save(string inRoomID, UserItemPositionEventHandler inEventDelegate)
	{
		return GetInstance(inRoomID).Save(inEventDelegate);
	}

	public static int? GetUserItemPositionID(string inRoomID, GameObject inGameObject)
	{
		return GetInstance(inRoomID).GetUserItemPositionID(inGameObject);
	}

	public static GameObject GetGameObjectReference(string inRoomID, int? inUserItemPositionID)
	{
		return GetInstance(inRoomID).GetGameObjectReference(inUserItemPositionID);
	}

	public static void CreateObject(string inRoomID, GameObject inGameObject, UserItemData inUserItemData, GameObject inParentObject)
	{
		CreateObject(inRoomID, inGameObject, inUserItemData, inGameObject.transform.position, inGameObject.transform.rotation.eulerAngles, inParentObject);
	}

	public static void CreateObject(string inRoomID, GameObject inGameObject, UserItemData inUserItemData, Vector3 inPosition, Vector3 inRotation, GameObject inParentObject)
	{
		GetInstance(inRoomID).CreateObject(inGameObject, inUserItemData, inPosition, inRotation, inParentObject);
	}

	public static void UpdateObject(string inRoomID, GameObject inGameObject, UserItemData inUserItemData, GameObject inParentObject)
	{
		UpdateObject(inRoomID, inGameObject, inUserItemData, inGameObject.transform.position, inGameObject.transform.rotation.eulerAngles, inParentObject);
	}

	public static void UpdateObject(string inRoomID, GameObject inGameObject, UserItemData inUserItemData, Vector3 inPosition, Vector3 inRotation, GameObject inParentObject)
	{
		GetInstance(inRoomID).UpdateObject(inGameObject, inUserItemData, inPosition, inRotation, inParentObject);
	}

	public static void RemoveObject(string inRoomID, GameObject inGameObject)
	{
		GetInstance(inRoomID).RemoveObject(inGameObject);
	}

	public static void RemoveObject(string inRoomID, UserItemPosition inUserItemPosition)
	{
		GetInstance(inRoomID).RemoveObject(inUserItemPosition);
	}
}
