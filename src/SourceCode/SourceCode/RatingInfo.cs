using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "RatingInfo", IsNullable = true, Namespace = "http://api.jumpstart.com/")]
public class RatingInfo
{
	[XmlElement(ElementName = "ID")]
	public int RatingID;

	[XmlElement(ElementName = "UID")]
	public Guid UserID;

	[XmlElement(ElementName = "CID")]
	public int CategoryID;

	[XmlElement(ElementName = "EID")]
	public int RatedEntityID;

	[XmlElement(ElementName = "RV")]
	public int RatedValue;

	[XmlElement(ElementName = "RD")]
	public DateTime RatedDate;

	public string GetDebugString()
	{
		return "Rating Data - CID:" + CategoryID + "  EID" + RatedEntityID + " Val:" + RatedValue + " Date:" + RatedDate;
	}

	public static void SubmitRating(int category, int entityID, int ratingValue, RatingSetEventHandler callback, object userdata)
	{
		RatingSetEventData ratingSetEventData = new RatingSetEventData();
		ratingSetEventData.mCallback = callback;
		ratingSetEventData.mUserData = userdata;
		WsWebService.SubmitRating(category, entityID, ratingValue, ServiceEventHandler, ratingSetEventData);
	}

	public static void SubmitRatingForUserID(int category, string roomID, string userID, string ratedUserID, int ratingValue, RatingSetEventHandler callback, object userdata)
	{
		RatingSetEventData ratingSetEventData = new RatingSetEventData();
		ratingSetEventData.mCallback = callback;
		ratingSetEventData.mUserData = userdata;
		WsWebService.SubmitRatingForUserID(category, roomID, userID, ratedUserID, ratingValue, ServiceEventHandler, ratingSetEventData);
	}

	public static void GetAllRatings(int category, int entityID, RatingGetAllEventHandler callback, object userdata)
	{
		RatingGetAllEventData ratingGetAllEventData = new RatingGetAllEventData();
		ratingGetAllEventData.mCallback = callback;
		ratingGetAllEventData.mUserData = userdata;
		WsWebService.GetAllRatings(category, entityID, ServiceEventHandler, ratingGetAllEventData);
	}

	public static void ClearRating(int category, int entityID)
	{
		WsWebService.ClearRating(category, entityID, ServiceEventHandler, null);
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_ALL_RATINGS:
			switch (inEvent)
			{
			case WsServiceEvent.ERROR:
			{
				RatingGetAllEventData ratingGetAllEventData2 = (RatingGetAllEventData)inUserData;
				ratingGetAllEventData2.mCallback(null, ratingGetAllEventData2.mUserData);
				Debug.LogError("!!!" + inType.ToString() + " failed!!!!");
				break;
			}
			case WsServiceEvent.COMPLETE:
			{
				RatingGetAllEventData ratingGetAllEventData = (RatingGetAllEventData)inUserData;
				ratingGetAllEventData.mCallback((RatingInfo[])inObject, ratingGetAllEventData.mUserData);
				break;
			}
			}
			break;
		case WsServiceType.SUBMIT_RATING:
			switch (inEvent)
			{
			case WsServiceEvent.ERROR:
			{
				RatingSetEventData ratingSetEventData4 = (RatingSetEventData)inUserData;
				if (ratingSetEventData4.mCallback != null)
				{
					ratingSetEventData4.mCallback(null, ratingSetEventData4.mUserData);
				}
				Debug.LogError("!!!" + inType.ToString() + " failed!!!!");
				break;
			}
			case WsServiceEvent.COMPLETE:
			{
				RatingSetEventData ratingSetEventData3 = (RatingSetEventData)inUserData;
				if (ratingSetEventData3.mCallback != null)
				{
					ratingSetEventData3.mCallback((RatingInfo)inObject, ratingSetEventData3.mUserData);
				}
				break;
			}
			}
			break;
		case WsServiceType.SUBMIT_RATING_FOR_USERID:
			switch (inEvent)
			{
			case WsServiceEvent.ERROR:
			{
				RatingSetEventData ratingSetEventData2 = (RatingSetEventData)inUserData;
				if (ratingSetEventData2.mCallback != null)
				{
					ratingSetEventData2.mCallback(null, ratingSetEventData2.mUserData);
				}
				Debug.LogError("!!!" + inType.ToString() + " failed!!!!");
				break;
			}
			case WsServiceEvent.COMPLETE:
			{
				RatingSetEventData ratingSetEventData = (RatingSetEventData)inUserData;
				if (ratingSetEventData.mCallback != null)
				{
					ratingSetEventData.mCallback((RatingInfo)inObject, ratingSetEventData.mUserData);
				}
				break;
			}
			}
			break;
		}
	}
}
