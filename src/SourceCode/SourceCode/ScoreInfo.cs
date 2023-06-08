using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "ScoreInfo", IsNullable = true, Namespace = "")]
public class ScoreInfo
{
	[XmlElement(ElementName = "ID")]
	public int ScoreID;

	[XmlElement(ElementName = "UID")]
	public Guid UserID;

	[XmlElement(ElementName = "CID")]
	public int CategoryID;

	[XmlElement(ElementName = "EID")]
	public int ScoredEntityID;

	[XmlElement(ElementName = "SV")]
	public int Score;

	[XmlElement(ElementName = "ScoredDate")]
	public DateTime ScoredDate;

	public string GetDebugString()
	{
		return "Score Data - CID:" + CategoryID + "  EID" + ScoredEntityID + " Val:" + Score + " Date:" + ScoredDate;
	}

	public static void SubmitScore(int category, int entityID, int scoreValue, SubmitScoreEventHandler callback)
	{
		WsWebService.SubmitScore(category, entityID, scoreValue, ServiceEventHandler, callback);
	}

	public static void ClearScore(int category, int entityID)
	{
		WsWebService.ClearScore(category, entityID, ServiceEventHandler, null);
	}

	public static void GetTopScores(int category, int entityID, int numRecords, TopScoreGetEventHandler callback, object userdata)
	{
		TopScoreGetEventData topScoreGetEventData = new TopScoreGetEventData();
		topScoreGetEventData.mCallback = callback;
		topScoreGetEventData.mUserData = userdata;
		WsWebService.GetTopScore(category, entityID, numRecords, ServiceEventHandler, topScoreGetEventData);
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_TOP_SCORE:
			switch (inEvent)
			{
			case WsServiceEvent.ERROR:
				Debug.LogError("!!!" + inType.ToString() + " failed!!!!");
				break;
			case WsServiceEvent.COMPLETE:
			{
				TopScoreGetEventData topScoreGetEventData = (TopScoreGetEventData)inUserData;
				topScoreGetEventData.mCallback((ScoreInfo[])inObject, topScoreGetEventData.mUserData);
				break;
			}
			}
			break;
		case WsServiceType.SUBMIT_SCORE:
			if (inEvent == WsServiceEvent.COMPLETE || inEvent == WsServiceEvent.ERROR)
			{
				((SubmitScoreEventHandler)inUserData)?.Invoke((ScoreInfo)inObject);
			}
			break;
		}
	}
}
