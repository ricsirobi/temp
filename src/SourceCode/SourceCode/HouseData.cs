using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "HouseData", Namespace = "", IsNullable = false)]
public class HouseData
{
	[XmlElement(ElementName = "Part")]
	public HouseDataPart[] Part;

	public static HouseData mHouseData;

	public static HouseData pInstance => mHouseData;

	public static void Init(GameObject obj)
	{
		if (mHouseData == null)
		{
			WsWebService.GetHouseData(WsGetEventHandler, obj.name);
		}
		else
		{
			obj.SendMessage("OnHouseDataReady", mHouseData);
		}
	}

	public static void Init(GameObject obj, string inUserID)
	{
		if (mHouseData == null)
		{
			WsWebService.GetHouseDataByUserID(inUserID, WsGetEventHandler, obj.name);
		}
		else
		{
			obj.SendMessage("OnHouseDataReady", mHouseData);
		}
	}

	public string FindPart(string partTypeName, string defaultVal)
	{
		if (Part != null)
		{
			HouseDataPart[] part = Part;
			foreach (HouseDataPart houseDataPart in part)
			{
				if (houseDataPart.PartTypes == partTypeName)
				{
					return houseDataPart.PartNames;
				}
			}
		}
		return defaultVal;
	}

	public static void WsSetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("-----WEB SERVICE CALL SetHouseData FAILED!!!");
			break;
		case WsServiceEvent.COMPLETE:
			_ = 47;
			break;
		}
	}

	public static void WsGetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		GameObject gameObject = GameObject.Find((string)inUserData);
		if (gameObject == null)
		{
			UtDebug.LogError("-----MyHouse ndoesn't exist in this scene!!!");
		}
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("-----WEB SERVICE CALL GetHouseData FAILED!!!");
			mHouseData = new HouseData();
			if ((bool)gameObject)
			{
				gameObject.SendMessage("OnHouseDataReady", mHouseData);
			}
			break;
		case WsServiceEvent.COMPLETE:
			if (inType != WsServiceType.GET_HOUSE)
			{
				break;
			}
			if (inObject != null)
			{
				mHouseData = (HouseData)inObject;
				if ((bool)gameObject)
				{
					gameObject.SendMessage("OnHouseDataReady", mHouseData);
				}
			}
			else
			{
				mHouseData = new HouseData();
				if ((bool)gameObject)
				{
					gameObject.SendMessage("OnHouseDataReady", mHouseData);
				}
			}
			break;
		}
	}

	public static void ResetData()
	{
		mHouseData.Part = new HouseDataPart[1];
		mHouseData.Part[0] = new HouseDataPart();
		mHouseData.Part[0].PartNames = "N/A";
		mHouseData.Part[0].PartTypes = "N/A";
		Save();
	}

	public static void Save()
	{
		WsWebService.SetHouseData(mHouseData, WsSetEventHandler, null);
	}
}
