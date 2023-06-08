using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GameData", Namespace = "")]
public class GameData
{
	public class Difficulty
	{
		public const int Unknown = 0;

		public const int Easy = 1;

		public const int Medium = 2;

		public const int Hard = 3;
	}

	[XmlElement(ElementName = "RankID", IsNullable = true)]
	public int? RankID;

	[XmlElement(ElementName = "IsMember")]
	public bool IsMember;

	[XmlElement(ElementName = "UserName")]
	public string UserName;

	[XmlElement(ElementName = "Value")]
	public int Value;

	[XmlElement(ElementName = "DatePlayed", IsNullable = true)]
	public DateTime? DatePlayed;

	[XmlElement(ElementName = "Win")]
	public int Win;

	[XmlElement(ElementName = "Loss")]
	public int Loss;

	[XmlElement(ElementName = "UserID")]
	public Guid UserID;

	[XmlElement(ElementName = "ProductID", IsNullable = true)]
	public int? ProductID;

	[XmlElement(ElementName = "PlatformID", IsNullable = true)]
	public int? PlatformID;

	[XmlElement(ElementName = "FBIDS", IsNullable = true)]
	public long? FacebookID;

	public static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE)
		{
			GetGameDataCallbackInfo getGameDataCallbackInfo = (GetGameDataCallbackInfo)inUserData;
			getGameDataCallbackInfo.mCallback((GameDataSummary)inObject, getGameDataCallbackInfo.mUserData);
		}
	}

	public static void GetGameDataByGameForDayRange(int gameID, bool isMultiPlayer, int difficulty, int level, string key, int count, bool ascendingOrder, GetGameDataEventHandler callback, object callbackUserData)
	{
		WsWebService.GetPeriodicGameDataByGame(UserInfo.pInstance.UserID, inUserData: new GetGameDataCallbackInfo
		{
			mCallback = callback,
			mUserData = callbackUserData
		}, gameID: gameID, isMultiplayer: isMultiPlayer, difficulty: difficulty, level: level, key: key, count: count, ascendingOrder: ascendingOrder, score: null, buddyFilter: false, inCallback: ServiceEventHandler);
	}
}
