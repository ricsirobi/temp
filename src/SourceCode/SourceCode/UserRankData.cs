using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "UserRankData", Namespace = "")]
public class UserRankData
{
	[XmlElement(ElementName = "UserID")]
	public Guid UserID;

	[XmlElement(ElementName = "Points")]
	public int Points;

	[XmlElement(ElementName = "CurrentRank")]
	public UserRank CurrentRank;

	[XmlElement(ElementName = "MemberRank")]
	public UserRank MemberRank;

	[XmlElement(ElementName = "NextRank")]
	public UserRank NextRank;

	private static ArrayOfUserAchievementInfo mInstance;

	private static bool mInitialized;

	private static bool mImageDownloaded;

	private static Dictionary<int, ArrayOfUserRank> mRanks;

	private static Dictionary<int, RankAttribute[]> mRankAttributeData;

	public static UserAchievementInfo pInstance => GetUserAchievementInfoByType(1);

	public static bool pIsReady
	{
		get
		{
			if (mInstance != null && mRanks != null)
			{
				return mRankAttributeData != null;
			}
			return false;
		}
	}

	public static void Init()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			WsWebService.GetAllRanks(WsGetEventHandler, null);
			WsWebService.GetRankAttributeData(WsGetEventHandler, null);
		}
		if (mInstance == null)
		{
			WsWebService.GetUserAchievements(WsGetEventHandler, null);
		}
	}

	public static void Init(ArrayOfUserAchievementInfo userAchievementInfo)
	{
		mInstance = userAchievementInfo;
		Init();
	}

	public static void Init(UserAchievementInfo[] userAchievementInfo)
	{
		mInstance = new ArrayOfUserAchievementInfo();
		mInstance.UserAchievementInfo = userAchievementInfo;
		Init();
	}

	public static void ReInit()
	{
		mInstance = null;
		mImageDownloaded = false;
		WsWebService.GetUserAchievements(WsGetEventHandler, null);
	}

	public static void Reset()
	{
		mInstance = null;
	}

	private static void InitRanks()
	{
		mRanks = new Dictionary<int, ArrayOfUserRank>();
	}

	private static void InitRankAttributeData()
	{
		mRankAttributeData = new Dictionary<int, RankAttribute[]>();
	}

	public static int GetNumRanks()
	{
		return GetNumRanksByType(1);
	}

	public static int GetNumRanksByType(int rankType)
	{
		if (mRanks.ContainsKey(rankType))
		{
			ArrayOfUserRank arrayOfUserRank = mRanks[rankType];
			if (arrayOfUserRank == null && arrayOfUserRank.UserRank != null)
			{
				return arrayOfUserRank.UserRank.Length;
			}
		}
		return 0;
	}

	public static UserRank GetUserRank(int rank)
	{
		return GetUserRankByType(1, rank);
	}

	public static UserRank GetUserRankByType(int rankType)
	{
		if (mInstance == null)
		{
			UserAchievementInfo[] achievements = null;
			return GetUserRankByType(rankType, achievements);
		}
		return GetUserRankByType(rankType, mInstance.UserAchievementInfo);
	}

	public static UserRank GetUserRankByType(int rankType, ArrayOfUserAchievementInfo achievements)
	{
		return GetUserRankByType(rankType, achievements.UserAchievementInfo);
	}

	public static UserRank GetUserRankByType(int rankType, UserAchievementInfo[] achievements)
	{
		int rank = 1;
		UserAchievementInfo userAchievementInfoByType = GetUserAchievementInfoByType(achievements, rankType);
		if (userAchievementInfoByType != null)
		{
			rank = userAchievementInfoByType.RankID;
		}
		return GetUserRankByType(rankType, rank);
	}

	public static UserRank GetUserRankByType(int rankType, int rank)
	{
		if (mRanks != null && mRanks.ContainsKey(rankType))
		{
			ArrayOfUserRank arrayOfUserRank = mRanks[rankType];
			if (arrayOfUserRank != null)
			{
				rank--;
				if (arrayOfUserRank.UserRank != null && rank < arrayOfUserRank.UserRank.Length)
				{
					return arrayOfUserRank.UserRank[rank];
				}
			}
		}
		return new UserRank
		{
			PointTypeID = rankType,
			RankID = 1
		};
	}

	public static UserRank GetUserRankByValue(int value)
	{
		return GetUserRankByTypeAndValue(1, value);
	}

	public static UserRank GetUserRankByTypeAndValue(int rankType, int value)
	{
		if (mRanks.ContainsKey(rankType))
		{
			ArrayOfUserRank arrayOfUserRank = mRanks[rankType];
			if (arrayOfUserRank != null && arrayOfUserRank.UserRank != null)
			{
				for (int i = 1; i < arrayOfUserRank.UserRank.Length; i++)
				{
					if (value < arrayOfUserRank.UserRank[i].Value)
					{
						return arrayOfUserRank.UserRank[i - 1];
					}
				}
				return arrayOfUserRank.UserRank[arrayOfUserRank.UserRank.Length - 1];
			}
		}
		return new UserRank
		{
			PointTypeID = rankType,
			RankID = 1
		};
	}

	public static UserRank GetNextRank(int rank)
	{
		return GetNextRankByType(1, rank);
	}

	public static UserRank GetNextRankByType(int rankType, int rank)
	{
		if (mRanks.ContainsKey(rankType))
		{
			ArrayOfUserRank arrayOfUserRank = mRanks[rankType];
			if (arrayOfUserRank != null && arrayOfUserRank.UserRank != null)
			{
				if (rank < arrayOfUserRank.UserRank.Length)
				{
					return arrayOfUserRank.UserRank[rank];
				}
				return arrayOfUserRank.UserRank[arrayOfUserRank.UserRank.Length - 1];
			}
		}
		return new UserRank
		{
			PointTypeID = rankType,
			RankID = 1
		};
	}

	public static UserAchievementInfo GetUserAchievementInfoByType(int rankType)
	{
		return GetUserAchievementInfoByType(mInstance, rankType);
	}

	public static UserAchievementInfo GetUserAchievementInfoByType(ArrayOfUserAchievementInfo achievementInfo, int rankType)
	{
		if (achievementInfo != null)
		{
			return GetUserAchievementInfoByType(achievementInfo.UserAchievementInfo, rankType);
		}
		return null;
	}

	public static UserAchievementInfo GetUserAchievementInfoByType(UserAchievementInfo[] achievementInfo, int rankType)
	{
		if (achievementInfo != null)
		{
			foreach (UserAchievementInfo userAchievementInfo in achievementInfo)
			{
				if (userAchievementInfo != null && userAchievementInfo.PointTypeID == rankType)
				{
					return userAchievementInfo;
				}
			}
		}
		return null;
	}

	public static TYPE GetAttribute<TYPE>(int rank, string attribute, TYPE defaultValue, bool orLower = false)
	{
		return GetAttribute(1, rank, attribute, defaultValue, orLower);
	}

	public static TYPE GetAttribute<TYPE>(int rankType, int rank, string attribute, TYPE defaultValue, bool orLower = false)
	{
		if (mRankAttributeData == null)
		{
			return defaultValue;
		}
		UserRank userRankByType = GetUserRankByType(rankType, 1);
		if (userRankByType == null)
		{
			return defaultValue;
		}
		while (rank > 0)
		{
			int key = rank + userRankByType.GlobalRankID - 1;
			if (mRankAttributeData.ContainsKey(key))
			{
				RankAttribute[] array = mRankAttributeData[key];
				int i = 0;
				for (int num = array.Length; i < num; i++)
				{
					RankAttribute rankAttribute = array[i];
					if (!(rankAttribute.Key == attribute))
					{
						continue;
					}
					Type typeFromHandle = typeof(TYPE);
					if (typeFromHandle.Equals(typeof(int)))
					{
						return (TYPE)(object)int.Parse(rankAttribute.Value);
					}
					if (typeFromHandle.Equals(typeof(float)))
					{
						return (TYPE)(object)float.Parse(rankAttribute.Value);
					}
					if (typeFromHandle.Equals(typeof(bool)))
					{
						if (rankAttribute.Value.Equals("t", StringComparison.OrdinalIgnoreCase) || rankAttribute.Value.Equals("1", StringComparison.OrdinalIgnoreCase) || rankAttribute.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
						{
							return (TYPE)(object)true;
						}
						if (rankAttribute.Value.Equals("f", StringComparison.OrdinalIgnoreCase) || rankAttribute.Value.Equals("0", StringComparison.OrdinalIgnoreCase) || rankAttribute.Value.Equals("false", StringComparison.OrdinalIgnoreCase))
						{
							return (TYPE)(object)false;
						}
					}
					else
					{
						if (typeFromHandle.Equals(typeof(string)))
						{
							return (TYPE)(object)rankAttribute.Value;
						}
						if (typeFromHandle.Equals(typeof(Color)))
						{
							return (TYPE)(object)GetColorFromAttribute(rankAttribute.Value, (Color)(object)defaultValue);
						}
					}
				}
			}
			rank = (orLower ? (rank - 1) : 0);
		}
		return defaultValue;
	}

	public static void WsGetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_ALL_RANKS:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				InitRanks();
				ArrayOfUserRank arrayOfUserRank = (ArrayOfUserRank)inObject;
				if (arrayOfUserRank == null)
				{
					UtDebug.LogError("WEB SERVICE CALL GetAllRanks RETURNED NO DATA!!!");
				}
				else
				{
					UserRank[] userRank2 = arrayOfUserRank.UserRank;
					foreach (UserRank userRank3 in userRank2)
					{
						int pointTypeID = userRank3.PointTypeID;
						if (!mRanks.ContainsKey(pointTypeID))
						{
							mRanks.Add(pointTypeID, new ArrayOfUserRank());
						}
						ArrayOfUserRank arrayOfUserRank2 = mRanks[pointTypeID];
						int num = 0;
						if (arrayOfUserRank2.UserRank != null)
						{
							num = arrayOfUserRank2.UserRank.Length;
						}
						Array.Resize(ref arrayOfUserRank2.UserRank, num + 1);
						arrayOfUserRank2.UserRank[num] = userRank3;
						userRank3.RankID = num + 1;
					}
				}
				if (mInstance != null && !mImageDownloaded)
				{
					mImageDownloaded = true;
					UserRank userRank4 = GetUserRank(pInstance.RankID);
					if (userRank4 != null)
					{
						AvatarData.pInstanceInfo.LoadRank(userRank4.Image);
					}
				}
				break;
			}
			case WsServiceEvent.ERROR:
				InitRanks();
				UtDebug.LogError("WEB SERVICE CALL GetAllRanks FAILED!!!");
				break;
			}
			break;
		case WsServiceType.GET_USER_ACHIEVEMENTS:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mInstance = (ArrayOfUserAchievementInfo)inObject;
				if (mInstance == null)
				{
					UtDebug.LogError("WEB SERVICE CALL UserAchievementInfo RETURNED NO DATA!!!");
					break;
				}
				if (AvAvatar.pToolbar != null)
				{
					AvAvatar.pToolbar.SendMessage("OnUpdateRank", null, SendMessageOptions.DontRequireReceiver);
				}
				if (mRanks != null)
				{
					mImageDownloaded = true;
					UserRank userRank = GetUserRank(pInstance.RankID);
					if (userRank != null)
					{
						AvatarData.pInstanceInfo.LoadRank(userRank.Image);
					}
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL UserAchievementInfo FAILED!!!");
				break;
			}
			break;
		case WsServiceType.GET_RANK_ATTRIBUTE_DATA:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				InitRankAttributeData();
				ArrayOfRankAttributeData arrayOfRankAttributeData = (ArrayOfRankAttributeData)inObject;
				if (arrayOfRankAttributeData == null)
				{
					UtDebug.LogError("WEB SERVICE CALL GetRankAttibuteData RETURNED NO DATA!!!");
				}
				else
				{
					if (arrayOfRankAttributeData.RankAttributeData == null)
					{
						break;
					}
					RankAttributeData[] rankAttributeData = arrayOfRankAttributeData.RankAttributeData;
					foreach (RankAttributeData rankAttributeData2 in rankAttributeData)
					{
						if (!mRankAttributeData.ContainsKey(rankAttributeData2.RankID))
						{
							mRankAttributeData.Add(rankAttributeData2.RankID, rankAttributeData2.Attributes);
						}
					}
				}
				break;
			}
			case WsServiceEvent.ERROR:
				InitRankAttributeData();
				UtDebug.LogError("WEB SERVICE CALL GetRankAttibuteData FAILED!!!");
				break;
			}
			break;
		}
	}

	public static void AddPoints(int points)
	{
		AddPoints(1, points);
	}

	public static void AddPoints(int rankType, int points)
	{
		if (!pIsReady)
		{
			return;
		}
		if (rankType == 8)
		{
			UtDebug.LogError("Pet XP should be added using PetRankData.AddPoints()");
			return;
		}
		UserAchievementInfo userAchievementInfo = GetUserAchievementInfoByType(rankType);
		if (userAchievementInfo == null)
		{
			userAchievementInfo = new UserAchievementInfo();
			userAchievementInfo.AchievementPointTotal = 0;
			userAchievementInfo.PointTypeID = rankType;
			userAchievementInfo.RankID = 1;
			List<UserAchievementInfo> list = new List<UserAchievementInfo>(mInstance.UserAchievementInfo);
			list.Add(userAchievementInfo);
			mInstance.UserAchievementInfo = list.ToArray();
		}
		userAchievementInfo.AchievementPointTotal += points;
		UserRank userRankByTypeAndValue = GetUserRankByTypeAndValue(rankType, userAchievementInfo.AchievementPointTotal.Value);
		if (userRankByTypeAndValue != null && userRankByTypeAndValue.RankID > userAchievementInfo.RankID)
		{
			userAchievementInfo.RankID = userRankByTypeAndValue.RankID;
			GameObject gameObject = GameObject.Find("PfCheckUserMessages");
			if (gameObject != null)
			{
				gameObject.SendMessage("ForceUserMessageUpdate", SendMessageOptions.DontRequireReceiver);
			}
			GameObject gameObject2 = GameObject.Find("PfMissionManager");
			if (gameObject2 != null)
			{
				gameObject2.SendMessage("OnNextRank", userRankByTypeAndValue.RankID, SendMessageOptions.DontRequireReceiver);
			}
		}
		if (AvAvatar.pToolbar != null)
		{
			AvAvatar.pToolbar.SendMessage("OnUpdateRank", SendMessageOptions.DontRequireReceiver);
		}
	}

	public static Color GetColorFromAttribute(string attributeData, Color defaultColor)
	{
		Color result = defaultColor;
		if (!string.IsNullOrEmpty(attributeData))
		{
			string[] array = attributeData.Split(',');
			if (array.Length == 3)
			{
				result.r = UtStringUtil.Parse(array[0], 1f);
				result.g = UtStringUtil.Parse(array[1], 1f);
				result.b = UtStringUtil.Parse(array[2], 1f);
			}
		}
		return result;
	}
}
