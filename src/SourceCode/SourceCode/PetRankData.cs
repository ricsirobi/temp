using System;
using System.Collections.Generic;
using UnityEngine;

public class PetRankData
{
	private class PetInfoCallBackData
	{
		public Guid? _EntityID;

		public UserRankForPetReady _RankCallback;

		public UserAchievementInfoForPetReady _AchievementCallback;

		public UserAchievementInfoForAllPetsReady _AllPetsAchievementsCallback;

		public object _UserData;

		public PetInfoCallBackData(Guid? entityID, UserRankForPetReady callback, object userData)
		{
			_RankCallback = callback;
			_UserData = userData;
		}

		public PetInfoCallBackData(Guid? entityID, UserAchievementInfoForPetReady callback, object userData)
		{
			_AchievementCallback = callback;
			_UserData = userData;
		}

		public PetInfoCallBackData(UserAchievementInfoForAllPetsReady callback, object userData)
		{
			_AllPetsAchievementsCallback = callback;
			_UserData = userData;
		}
	}

	private static Dictionary<string, UserAchievementInfo> mPetAchievementInfo = new Dictionary<string, UserAchievementInfo>();

	private static PetAchievementInfoReady mCallback = null;

	private static bool mIsReady = false;

	public static bool pIsReady => mIsReady;

	public static void InitAchievementInfo(PetAchievementInfoReady callback)
	{
		mCallback = callback;
		if (UserInfo.pInstance != null)
		{
			LoadUserAchievementInfoForAllPets(UserInfo.pInstance.UserID, OnAllPetAchievementsReady, null);
		}
	}

	private static void OnAllPetAchievementsReady(List<UserAchievementInfo> achievementInfo, object UserItemData)
	{
		mIsReady = true;
		mCallback();
	}

	private static List<UserAchievementInfo> ProcessAchievementInfoArray(ArrayOfUserAchievementInfo achievementInfoArray)
	{
		if (achievementInfoArray != null && achievementInfoArray.UserAchievementInfo != null && achievementInfoArray.UserAchievementInfo.Length != 0)
		{
			List<UserAchievementInfo> list = new List<UserAchievementInfo>();
			UserAchievementInfo[] userAchievementInfo = achievementInfoArray.UserAchievementInfo;
			foreach (UserAchievementInfo achievementInfo in userAchievementInfo)
			{
				UserAchievementInfo userAchievementInfo2 = list.Find(delegate(UserAchievementInfo x)
				{
					Guid? userID = x.UserID;
					Guid? userID2 = achievementInfo.UserID;
					if (userID.HasValue != userID2.HasValue)
					{
						return false;
					}
					return !userID.HasValue || userID.GetValueOrDefault() == userID2.GetValueOrDefault();
				});
				if (userAchievementInfo2 == null)
				{
					list.Add(achievementInfo);
				}
				else if (userAchievementInfo2.PointTypeID != 8)
				{
					list.Remove(userAchievementInfo2);
					list.Add(achievementInfo);
				}
			}
			{
				foreach (UserAchievementInfo item in list)
				{
					if (item.PointTypeID != 8)
					{
						item.AchievementPointTotal = 0;
						item.PointTypeID = 8;
						item.RankID = 1;
					}
					CacheAchievementInfo(item.UserID, item);
				}
				return list;
			}
		}
		return null;
	}

	public static UserAchievementInfo GetUserAchievementInfo(RaisedPetData petData)
	{
		if (petData == null)
		{
			return null;
		}
		return GetUserAchievementInfo(petData.EntityID);
	}

	public static UserAchievementInfo GetUserAchievementInfo(Guid? petID)
	{
		if (petID.HasValue)
		{
			string key = petID.Value.ToString();
			if (mPetAchievementInfo != null && mPetAchievementInfo.ContainsKey(key))
			{
				return mPetAchievementInfo[key];
			}
		}
		return null;
	}

	public static UserRank GetUserRank(RaisedPetData petData)
	{
		if (petData == null)
		{
			return UserRankData.GetUserRankByType(8, 1);
		}
		return GetUserRank(petData.EntityID);
	}

	public static UserRank GetUserRank(Guid? petID)
	{
		if (petID.HasValue)
		{
			int rank = 1;
			UserAchievementInfo userAchievementInfo = GetUserAchievementInfo(petID);
			if (userAchievementInfo != null)
			{
				rank = userAchievementInfo.RankID;
			}
			return UserRankData.GetUserRankByType(8, rank);
		}
		return UserRankData.GetUserRankByType(8, 1);
	}

	public static void LoadUserAchievementInfoForAllPets(string userID, UserAchievementInfoForAllPetsReady callback, object inUserData)
	{
		if (!string.IsNullOrEmpty(userID))
		{
			PetInfoCallBackData inUserData2 = new PetInfoCallBackData(callback, inUserData);
			WsWebService.GetPetAchievementsByUserID(userID, MultipleAchievementsInfoHandler, inUserData2);
		}
		else
		{
			callback?.Invoke(null, inUserData);
		}
	}

	private static void MultipleAchievementsInfoHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.GET_PET_ACHIEVEMENTS_BY_USERID)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			List<UserAchievementInfo> achievementInfo = ProcessAchievementInfoArray((ArrayOfUserAchievementInfo)inObject);
			PetInfoCallBackData petInfoCallBackData2 = (PetInfoCallBackData)inUserData;
			if (petInfoCallBackData2 != null && petInfoCallBackData2._AllPetsAchievementsCallback != null)
			{
				petInfoCallBackData2._AllPetsAchievementsCallback(achievementInfo, petInfoCallBackData2._UserData);
			}
			break;
		}
		case WsServiceEvent.ERROR:
		{
			PetInfoCallBackData petInfoCallBackData = (PetInfoCallBackData)inUserData;
			if (petInfoCallBackData != null && petInfoCallBackData._AllPetsAchievementsCallback != null)
			{
				petInfoCallBackData._AllPetsAchievementsCallback(null, petInfoCallBackData._UserData);
			}
			break;
		}
		}
	}

	public static void LoadUserAchievementInfo(RaisedPetData petData, UserAchievementInfoForPetReady callback, bool forceLoad = false, object inUserData = null)
	{
		if (petData != null)
		{
			LoadUserAchievementInfo(petData.EntityID, callback, forceLoad, inUserData);
		}
		else
		{
			callback?.Invoke(null, inUserData);
		}
	}

	public static void LoadUserAchievementInfo(Guid? petID, UserAchievementInfoForPetReady callback, bool forceLoad = false, object inUserData = null)
	{
		if (petID.HasValue)
		{
			if (!forceLoad && mPetAchievementInfo.ContainsKey(petID.Value.ToString()))
			{
				if (callback != null)
				{
					UserAchievementInfo achievementInfo = mPetAchievementInfo[petID.Value.ToString()];
					callback(achievementInfo, inUserData);
				}
			}
			else
			{
				PetInfoCallBackData inUserData2 = new PetInfoCallBackData(petID, callback, inUserData);
				WsWebService.GetAchievementsByUserID(petID.Value.ToString(), LoadUserAchievementHandler, inUserData2);
			}
		}
		else
		{
			callback?.Invoke(null, inUserData);
		}
	}

	private static void LoadUserAchievementHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			UserAchievementInfo userAchievementInfo = null;
			PetInfoCallBackData petInfoCallBackData2 = (PetInfoCallBackData)inUserData;
			ArrayOfUserAchievementInfo arrayOfUserAchievementInfo = (ArrayOfUserAchievementInfo)inObject;
			if (arrayOfUserAchievementInfo != null && arrayOfUserAchievementInfo.UserAchievementInfo != null && arrayOfUserAchievementInfo.UserAchievementInfo.Length != 0)
			{
				userAchievementInfo = UserRankData.GetUserAchievementInfoByType(arrayOfUserAchievementInfo.UserAchievementInfo, 8);
				if (userAchievementInfo == null)
				{
					userAchievementInfo = new UserAchievementInfo();
					userAchievementInfo.UserID = petInfoCallBackData2._EntityID;
					userAchievementInfo.AchievementPointTotal = 0;
					userAchievementInfo.PointTypeID = 8;
					userAchievementInfo.RankID = 1;
				}
				CacheAchievementInfo(userAchievementInfo.UserID, userAchievementInfo);
			}
			if (petInfoCallBackData2 != null && petInfoCallBackData2._AchievementCallback != null)
			{
				petInfoCallBackData2._AchievementCallback(userAchievementInfo, petInfoCallBackData2._UserData);
			}
			break;
		}
		case WsServiceEvent.ERROR:
		{
			PetInfoCallBackData petInfoCallBackData = (PetInfoCallBackData)inUserData;
			if (petInfoCallBackData != null && petInfoCallBackData._AchievementCallback != null)
			{
				petInfoCallBackData._AchievementCallback(null, petInfoCallBackData._UserData);
			}
			break;
		}
		}
	}

	public static void LoadUserRank(RaisedPetData petData, UserRankForPetReady callback, bool forceLoad = false, object inUserData = null)
	{
		if (petData != null)
		{
			LoadUserRank(petData.EntityID, callback, forceLoad, inUserData);
		}
		else
		{
			callback?.Invoke(UserRankData.GetUserRankByType(8, 1), inUserData);
		}
	}

	public static void LoadUserRank(Guid? petID, UserRankForPetReady callback, bool forceLoad = false, object inUserData = null)
	{
		if (petID.HasValue)
		{
			if (!forceLoad && mPetAchievementInfo.ContainsKey(petID.Value.ToString()))
			{
				if (callback != null)
				{
					int rank = 1;
					UserAchievementInfo userAchievementInfo = mPetAchievementInfo[petID.Value.ToString()];
					if (userAchievementInfo != null)
					{
						rank = userAchievementInfo.RankID;
					}
					callback(UserRankData.GetUserRankByType(8, rank), inUserData);
				}
			}
			else
			{
				PetInfoCallBackData inUserData2 = new PetInfoCallBackData(petID, callback, inUserData);
				WsWebService.GetAchievementsByUserID(petID.Value.ToString(), LoadUserRankHandler, inUserData2);
			}
		}
		else
		{
			callback?.Invoke(UserRankData.GetUserRankByType(8, 1), inUserData);
		}
	}

	private static void LoadUserRankHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			UserAchievementInfo userAchievementInfo = null;
			PetInfoCallBackData petInfoCallBackData2 = (PetInfoCallBackData)inUserData;
			ArrayOfUserAchievementInfo arrayOfUserAchievementInfo = (ArrayOfUserAchievementInfo)inObject;
			if (arrayOfUserAchievementInfo != null && arrayOfUserAchievementInfo.UserAchievementInfo != null && arrayOfUserAchievementInfo.UserAchievementInfo.Length != 0)
			{
				userAchievementInfo = UserRankData.GetUserAchievementInfoByType(arrayOfUserAchievementInfo.UserAchievementInfo, 8);
				if (userAchievementInfo == null)
				{
					userAchievementInfo = new UserAchievementInfo();
					userAchievementInfo.UserID = petInfoCallBackData2._EntityID;
					userAchievementInfo.AchievementPointTotal = 0;
					userAchievementInfo.PointTypeID = 8;
					userAchievementInfo.RankID = 1;
				}
				CacheAchievementInfo(userAchievementInfo.UserID, userAchievementInfo);
			}
			if (petInfoCallBackData2 != null && petInfoCallBackData2._RankCallback != null)
			{
				int rank = 1;
				if (userAchievementInfo != null)
				{
					rank = userAchievementInfo.RankID;
				}
				petInfoCallBackData2._RankCallback(UserRankData.GetUserRankByType(8, rank), petInfoCallBackData2._UserData);
			}
			break;
		}
		case WsServiceEvent.ERROR:
		{
			PetInfoCallBackData petInfoCallBackData = (PetInfoCallBackData)inUserData;
			if (petInfoCallBackData != null && petInfoCallBackData._RankCallback != null)
			{
				petInfoCallBackData._RankCallback(UserRankData.GetUserRankByType(8, 1), petInfoCallBackData._UserData);
			}
			break;
		}
		}
	}

	public static void AddPoints(RaisedPetData petData, int points)
	{
		if (petData != null)
		{
			AddPoints(petData.EntityID, points);
		}
	}

	public static void AddPoints(Guid? petID, int points)
	{
		if (!petID.HasValue || !UserRankData.pIsReady)
		{
			return;
		}
		UserAchievementInfo userAchievementInfo = GetUserAchievementInfo(petID);
		if (userAchievementInfo == null)
		{
			userAchievementInfo = new UserAchievementInfo();
			userAchievementInfo.UserID = petID;
			userAchievementInfo.AchievementPointTotal = 0;
			userAchievementInfo.PointTypeID = 8;
			userAchievementInfo.RankID = 1;
			CacheAchievementInfo(petID, userAchievementInfo);
		}
		userAchievementInfo.AchievementPointTotal += points;
		UserRank userRankByTypeAndValue = UserRankData.GetUserRankByTypeAndValue(8, userAchievementInfo.AchievementPointTotal.Value);
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
			GameObject gameObject3 = GameObject.Find("PfSanctuaryManager");
			if (gameObject3 != null)
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				dictionary.Add("PetID", petID);
				dictionary.Add("Rank", userRankByTypeAndValue.RankID);
				gameObject3.SendMessage("OnNextRank", dictionary, SendMessageOptions.DontRequireReceiver);
			}
		}
		if (AvAvatar.pToolbar != null)
		{
			AvAvatar.pToolbar.SendMessage("OnUpdateRank", SendMessageOptions.DontRequireReceiver);
		}
	}

	private static void CacheAchievementInfo(Guid? petID, UserAchievementInfo info)
	{
		if (!petID.HasValue)
		{
			return;
		}
		string text = petID.Value.ToString();
		if (!string.IsNullOrEmpty(text))
		{
			if (mPetAchievementInfo.ContainsKey(text))
			{
				mPetAchievementInfo[text] = info;
			}
			else
			{
				mPetAchievementInfo.Add(text, info);
			}
		}
	}

	public static void ResetCache()
	{
		mPetAchievementInfo.Clear();
		mIsReady = false;
	}
}
