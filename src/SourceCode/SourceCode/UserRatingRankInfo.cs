using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "URRI", IsNullable = true)]
public class UserRatingRankInfo
{
	[XmlElement(ElementName = "RI")]
	public RatingRankInfo RankInfo { get; set; }

	[XmlElement(ElementName = "RUID")]
	public Guid RatedUserID { get; set; }

	[XmlElement(ElementName = "RUN")]
	public string UserName { get; set; }

	[XmlElement(ElementName = "FBUID", IsNullable = true)]
	public string FacebookUserID { get; set; }

	public static void GetTopRankWithUserID(string userID, int categoryID, int numRecords, RequestType typeID, List<long> facebookFriends, GetTopRatedRanksEventHandler callback, object inUserData)
	{
		GetTopRanksEventData getTopRanksEventData = new GetTopRanksEventData();
		getTopRanksEventData.mCallback = callback;
		getTopRanksEventData.mUserData = inUserData;
		WsWebService.GetTopRanksWithUserIDs(userID, categoryID, numRecords, typeID, facebookFriends, ServiceEventHandler, getTopRanksEventData);
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.GET_TOP_RANKS_WITH_USERID)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
		{
			GetTopRanksEventData getTopRanksEventData2 = (GetTopRanksEventData)inUserData;
			if (getTopRanksEventData2.mCallback != null)
			{
				getTopRanksEventData2.mCallback(null, getTopRanksEventData2.mUserData);
			}
			Debug.LogError("!!!" + inType.ToString() + " failed!!!!");
			break;
		}
		case WsServiceEvent.COMPLETE:
		{
			GetTopRanksEventData getTopRanksEventData = (GetTopRanksEventData)inUserData;
			if (getTopRanksEventData.mCallback != null)
			{
				getTopRanksEventData.mCallback((ArrayOfUserRatingRankInfo)inObject, getTopRanksEventData.mUserData);
			}
			break;
		}
		}
	}
}
