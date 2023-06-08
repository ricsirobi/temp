using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "RatingRankInfo", IsNullable = true)]
public class RatingRankInfo
{
	[XmlElement(ElementName = "ID")]
	public int RatingRankID;

	[XmlElement(ElementName = "CID")]
	public int RatingCategoryID;

	[XmlElement(ElementName = "EID")]
	public int RatedEntityID;

	[XmlElement(ElementName = "R")]
	public int Rank;

	[XmlElement(ElementName = "RA")]
	public float RatingAverage;

	[XmlElement(ElementName = "TV")]
	public int TotalVotes;

	[XmlElement(ElementName = "UD")]
	public DateTime UpdateDate;

	public string GetDebugString()
	{
		return "Rank Data - CID:" + RatingCategoryID + "  EID" + RatedEntityID + " Rank:" + Rank + " Date: Avg:" + RatingAverage + " #Votes:" + TotalVotes + " Udate:" + UpdateDate;
	}

	public static void GetTopRanks(int category, int numRecords, TopRankGetEventHandler callback, object userdata)
	{
		TopRankGetEventData topRankGetEventData = new TopRankGetEventData();
		topRankGetEventData.mCallback = callback;
		topRankGetEventData.mUserData = userdata;
		WsWebService.GetTopRank(category, numRecords, ServiceEventHandler, topRankGetEventData);
	}

	public static void GetRank(int category, int entityID, RankGetEventHandler callback, object userdata)
	{
		RankGetEventData rankGetEventData = new RankGetEventData();
		rankGetEventData.mCallback = callback;
		rankGetEventData.mUserData = userdata;
		WsWebService.GetRank(category, entityID, ServiceEventHandler, rankGetEventData);
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			Debug.LogError("!!!" + inType.ToString() + " failed!!!!");
			break;
		case WsServiceEvent.COMPLETE:
			switch (inType)
			{
			case WsServiceType.GET_TOP_RANK:
			{
				TopRankGetEventData topRankGetEventData = (TopRankGetEventData)inUserData;
				topRankGetEventData.mCallback((RatingRankInfo[])inObject, topRankGetEventData.mUserData);
				break;
			}
			case WsServiceType.GET_RANK:
			{
				RankGetEventData rankGetEventData = (RankGetEventData)inUserData;
				rankGetEventData.mCallback((RatingRankInfo)inObject, rankGetEventData.mUserData);
				break;
			}
			}
			break;
		}
	}
}
