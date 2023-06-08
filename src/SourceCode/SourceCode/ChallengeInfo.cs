using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "ChallengeInfo", Namespace = "")]
public class ChallengeInfo
{
	[XmlElement(ElementName = "ChallengeID")]
	public int ChallengeID;

	[XmlElement(ElementName = "UserID")]
	public Guid UserID;

	[XmlElement(ElementName = "ProductGroupID")]
	public int ProductGroupID;

	[XmlElement(ElementName = "Points")]
	public int Points;

	[XmlElement(ElementName = "ExpirationDate")]
	public DateTime ExpirationDate;

	[XmlElement(ElementName = "ChallengeGameInfo")]
	public ChallengeGameInfo ChallengeGameInfo;

	[XmlElement(ElementName = "ChallengeContenders")]
	public ChallengeContenderInfo[] ChallengeContenders;

	[XmlElement(ElementName = "ExpirationDuration")]
	public int ExpirationDuration;

	private static List<ChallengeInfo> mInviteMessageList = new List<ChallengeInfo>();

	private static ChallengeInfo mActiveChallenge = null;

	private static string mRecommendedGameExitSceneName;

	private static string mRecommendedGameExitMarkerName;

	public static ChallengeGameData[] mChallengeDataMapping;

	public const int CHALLENGE_CREATE_MESSAGEID = 604;

	public const int POINTS_BASED_CHALLENGE_WON_MESSAGEID = 605;

	public const int TIME_BASED_CHALLENGE_WON_MESSAGEID = 605;

	public const int POINTS_BASED_CHALLENGE_LOST_MESSAGEID = 606;

	public const int TIME_BASED_CHALLENGE_LOST_MESSAGEID = 606;

	public const int WON_CHALLENGE_ACHIEVEMENT_ID = 182;

	public static List<ChallengeInfo> pInviteMessageList => mInviteMessageList;

	public static ChallengeInfo pActiveChallenge
	{
		get
		{
			return mActiveChallenge;
		}
		set
		{
			mActiveChallenge = value;
		}
	}

	public static string pRecommendedGameExitSceneName
	{
		get
		{
			return mRecommendedGameExitSceneName;
		}
		set
		{
			mRecommendedGameExitSceneName = value;
		}
	}

	public static string pRecommendedGameExitMarkerName
	{
		get
		{
			return mRecommendedGameExitMarkerName;
		}
		set
		{
			mRecommendedGameExitMarkerName = value;
		}
	}

	public static bool pIsReady => mChallengeDataMapping != null;

	public static void AddMessage(string fromUser, int challengeID, int gameID, int gameLevelID, int gameDiffID, int challengePoints)
	{
		ChallengeInfo challengeInfo = new ChallengeInfo();
		challengeInfo.UserID = new Guid(fromUser);
		challengeInfo.ChallengeID = challengeID;
		challengeInfo.Points = challengePoints;
		challengeInfo.ChallengeGameInfo = new ChallengeGameInfo();
		challengeInfo.ChallengeGameInfo.GameID = gameID;
		challengeInfo.ChallengeGameInfo.GameLevelID = gameLevelID;
		challengeInfo.ChallengeGameInfo.GameDifficultyID = gameDiffID;
		mInviteMessageList.Add(challengeInfo);
		GameObject gameObject = GameObject.Find("PfCheckBuddyInviteMessages");
		if (gameObject != null)
		{
			gameObject.SendMessage("ForceInviteMessageUpdate", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static ChallengeResultState CheckForChallengeCompletion(int inGameID, int inLevelID, int inDifficulty, int inPoints, bool isTimerUsedAsPoints)
	{
		if (mActiveChallenge != null && UserInfo.pIsReady && mActiveChallenge.ChallengeGameInfo.GameID == inGameID)
		{
			if (mActiveChallenge.ChallengeGameInfo.GameLevelID == inLevelID && (!mActiveChallenge.ChallengeGameInfo.GameDifficultyID.HasValue || mActiveChallenge.ChallengeGameInfo.GameDifficultyID.Value == inDifficulty))
			{
				bool flag = (!isTimerUsedAsPoints && mActiveChallenge.Points < inPoints) || (isTimerUsedAsPoints && mActiveChallenge.Points > inPoints);
				if (isTimerUsedAsPoints)
				{
					WsWebService.RespondToChallenge(mActiveChallenge.ChallengeID, flag ? 605 : 606, inPoints, (!isTimerUsedAsPoints) ? 1 : 2, null, null);
				}
				else
				{
					WsWebService.RespondToChallenge(mActiveChallenge.ChallengeID, flag ? 605 : 606, inPoints, (!isTimerUsedAsPoints) ? 1 : 2, null, null);
				}
				if (flag)
				{
					UserAchievementTask.Set(182);
					mActiveChallenge = null;
					return ChallengeResultState.WON;
				}
				return ChallengeResultState.LOST;
			}
			mActiveChallenge = null;
		}
		return ChallengeResultState.NONE;
	}

	public ChallengeState GetState()
	{
		if (ChallengeContenders != null)
		{
			ChallengeContenderInfo[] challengeContenders = ChallengeContenders;
			foreach (ChallengeContenderInfo challengeContenderInfo in challengeContenders)
			{
				if (challengeContenderInfo.UserID.ToString() == UserInfo.pInstance.UserID)
				{
					return challengeContenderInfo.ChallengeState;
				}
			}
		}
		return ChallengeState.Initiated;
	}

	public static void Init()
	{
		if (mChallengeDataMapping == null)
		{
			RsResourceManager.Load(GameConfig.GetKeyData("ChallengeDataFile"), ChallengeDataXmlLoadEventHandler);
		}
	}

	private static void ChallengeDataXmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mChallengeDataMapping = UtUtilities.DeserializeFromXml((string)inObject, typeof(ChallengeGameData[])) as ChallengeGameData[];
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load file : " + inURL);
			break;
		}
	}

	public static string GetSceneName(int inGameID)
	{
		if (mChallengeDataMapping != null)
		{
			ChallengeGameData[] array = mChallengeDataMapping;
			foreach (ChallengeGameData challengeGameData in array)
			{
				if (challengeGameData._GameID == inGameID)
				{
					return challengeGameData._SceneName;
				}
			}
		}
		return null;
	}

	public static string GetGameTitle(int inGameID)
	{
		if (mChallengeDataMapping != null)
		{
			ChallengeGameData[] array = mChallengeDataMapping;
			foreach (ChallengeGameData challengeGameData in array)
			{
				if (challengeGameData._GameID == inGameID)
				{
					return challengeGameData._GameTitle.GetLocalizedString();
				}
			}
		}
		return null;
	}

	public static ChallengePetData GetPetData(int inGameID)
	{
		if (mChallengeDataMapping != null)
		{
			ChallengeGameData[] array = mChallengeDataMapping;
			foreach (ChallengeGameData challengeGameData in array)
			{
				if (challengeGameData._GameID == inGameID)
				{
					return challengeGameData._PetData;
				}
			}
		}
		return null;
	}

	public static void ChallengeAccepted(ChallengeInfo info)
	{
		WsWebService.AcceptChallenge(info.ChallengeID, -1, null, null);
		pActiveChallenge = info;
		AvAvatar.SetActive(inActive: false);
		RsResourceManager.LoadLevel(GetSceneName(info.ChallengeGameInfo.GameID));
	}
}
