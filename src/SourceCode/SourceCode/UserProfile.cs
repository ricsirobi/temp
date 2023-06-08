using UnityEngine;

public class UserProfile
{
	public static ProfileQuestionData pQuestionData;

	public static UserProfileData pProfileData;

	private UserProfileData mProfileData;

	private string mUserID = "";

	private bool mIsError;

	public string UserID => mUserID;

	public AvatarDisplayData AvatarInfo => mProfileData.AvatarInfo;

	public int MythieCount => mProfileData.MythieCount;

	public int BuddyCount => mProfileData.BuddyCount.GetValueOrDefault();

	public int RoomVisitCount
	{
		get
		{
			if (!mProfileData.ActivityCount.HasValue)
			{
				return 0;
			}
			return mProfileData.ActivityCount.Value;
		}
	}

	public UserAnswerData AnswerData => mProfileData.AnswerData;

	public int GameCurrency
	{
		get
		{
			if (!mProfileData.GameCurrency.HasValue)
			{
				return 0;
			}
			return mProfileData.GameCurrency.Value;
		}
	}

	public bool pIsError => mIsError;

	public bool pIsReady
	{
		get
		{
			if (mProfileData != null)
			{
				return pQuestionData != null;
			}
			return false;
		}
	}

	public ProfileUserAnswer FindQuestionAnswer(int qid)
	{
		return mProfileData.FindQuestionAnswer(qid);
	}

	public void SetQuestionAnswer(ProfileUserAnswer sel)
	{
		mProfileData.SetQuestionAnswer(sel, WsEventHandler, null);
	}

	public Gender GetGender()
	{
		if (mProfileData == null || mProfileData.AvatarInfo == null || mProfileData.AvatarInfo.UserInfo == null)
		{
			return Gender.Unknown;
		}
		if (!mProfileData.AvatarInfo.UserInfo.GenderID.HasValue)
		{
			return Gender.Unknown;
		}
		return mProfileData.AvatarInfo.UserInfo.GenderID.Value;
	}

	public void SetGender(Gender gender, bool save)
	{
		mProfileData.AvatarInfo.UserInfo.GenderID = gender;
		WsWebService.SetUserGender(gender, WsEventHandler, null);
	}

	public string GetDisplayName()
	{
		if (mProfileData == null || mProfileData.AvatarInfo == null || mProfileData.AvatarInfo.AvatarData == null)
		{
			return "Not Ready Yet";
		}
		if (BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(mProfileData.AvatarInfo.UserInfo.UserID);
			if (buddy != null)
			{
				string displayName = buddy.DisplayName;
				if (!string.IsNullOrEmpty(displayName))
				{
					return displayName;
				}
			}
		}
		return mProfileData.AvatarInfo.AvatarData.DisplayName;
	}

	public bool IsMember()
	{
		if (mProfileData == null || mProfileData.AvatarInfo == null || mProfileData.AvatarInfo.UserSubscriptionInfo == null)
		{
			return false;
		}
		return mProfileData.AvatarInfo.UserSubscriptionInfo.SubscriptionTypeID == 1;
	}

	public void WsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			mIsError = false;
			if (inType == WsServiceType.GET_USER_PROFILE)
			{
				if (inObject == null)
				{
					UtDebug.LogError("WEB SERVICE CALL GetUserProfile RETURNED NO DATA!!!");
					UtUtilities.ShowGenericValidationError();
				}
				else
				{
					pProfileData = (UserProfileData)inObject;
				}
				if (pQuestionData != null && pProfileData != null)
				{
					DistributeData();
				}
			}
			switch (inType)
			{
			case WsServiceType.GET_USER_PROFILE_BY_USER_ID:
				if (inObject == null)
				{
					Debug.LogError("Get profile data failed");
					mIsError = true;
					break;
				}
				mProfileData = (UserProfileData)inObject;
				if (mProfileData.ID == UserInfo.pInstance.UserID)
				{
					pProfileData = mProfileData;
				}
				break;
			case WsServiceType.GET_QUESTIONS:
				if (inObject == null)
				{
					Debug.LogError("Get profile question data failed");
					mIsError = true;
				}
				else
				{
					pQuestionData = (ProfileQuestionData)inObject;
				}
				if (pQuestionData != null && pProfileData != null)
				{
					DistributeData();
				}
				break;
			}
			break;
		case WsServiceEvent.ERROR:
			if (inType == WsServiceType.GET_USER_PROFILE)
			{
				UtDebug.LogError("WEB SERVICE CALL GetUserProfile FAILED!!!");
			}
			else
			{
				mIsError = true;
			}
			break;
		}
	}

	public string GetGenderURL(GameObject inAvatar)
	{
		return mProfileData.GetGenderURL(inAvatar);
	}

	public string GetCountryURL(GameObject inAvatar)
	{
		return mProfileData.GetCountryURL(inAvatar);
	}

	public static string GetCountryURL(GameObject inAvatar, int id)
	{
		string result = ((inAvatar != null) ? inAvatar.GetComponent<AvAvatarProperties>()._DefaultCountryURL : "");
		ProfileQuestionList questionList = GetQuestionList(2);
		if (questionList != null)
		{
			ProfileQuestion question = questionList.GetQuestion(33);
			if (question != null)
			{
				ProfileAnswer answer = question.GetAnswer(id);
				if (answer != null)
				{
					result = answer.ImageURL;
				}
			}
		}
		return result;
	}

	public static string GetMoodURL(int id)
	{
		string result = "";
		ProfileQuestionList questionList = GetQuestionList(2);
		if (questionList != null)
		{
			ProfileQuestion question = questionList.GetQuestion(34);
			if (question != null)
			{
				ProfileAnswer answer = question.GetAnswer(id);
				if (answer != null)
				{
					result = answer.ImageURL;
				}
			}
		}
		return result;
	}

	public bool HasGroup()
	{
		if (mProfileData.Groups != null)
		{
			return mProfileData.Groups.Length != 0;
		}
		return false;
	}

	public UserProfileGroupData[] GetGroups()
	{
		return mProfileData.Groups;
	}

	public static void DistributeData()
	{
		UserRankData.Init(pProfileData.AvatarInfo.Achievements);
		UserInfo.Init(pProfileData.AvatarInfo.UserInfo);
		if (!SubscriptionInfo.pIsReady)
		{
			SubscriptionInfo.Init(pProfileData.AvatarInfo.UserSubscriptionInfo.GetSubscriptionInfo());
		}
		string countryURL = pProfileData.GetCountryURL(AvAvatar.pObject);
		string moodURL = pProfileData.GetMoodURL();
		AvatarData.Init(pProfileData.AvatarInfo.AvatarData, countryURL, moodURL);
		if (pProfileData.HasGroup() && pProfileData.Groups[0] != null)
		{
			Group group = Group.GetGroup(pProfileData.Groups[0].GroupID);
			if (group != null)
			{
				AvatarData.SetGroupName(group);
			}
		}
	}

	public static void Init()
	{
		UserProfile @object = new UserProfile();
		if (pQuestionData == null)
		{
			WsWebService.GetQuestions(activeOnly: true, @object.WsEventHandler, null);
		}
		WsWebService.GetUserProfile(@object.WsEventHandler, null);
	}

	public static void InitDefault()
	{
		pProfileData = new UserProfileData();
		pProfileData.GameCurrency = 0;
		pProfileData.CashCurrency = 0;
		pProfileData.AvatarInfo = new AvatarDisplayData();
		pProfileData.AvatarInfo.UserSubscriptionInfo = new UserSubscriptionInfo();
		pProfileData.AvatarInfo.UserSubscriptionInfo.SubscriptionTypeID = 2;
		pProfileData.AvatarInfo.UserInfo = new UserInfo();
		pProfileData.AvatarInfo.Achievements = new UserAchievementInfo[1];
		pProfileData.AvatarInfo.Achievements[0] = new UserAchievementInfo();
		pProfileData.AvatarInfo.Achievements[0].AchievementPointTotal = 0;
	}

	private void Init(string uid)
	{
		mUserID = uid;
		if (pQuestionData == null)
		{
			WsWebService.GetQuestions(activeOnly: true, WsEventHandler, null);
		}
		if (mUserID == pProfileData.ID)
		{
			mProfileData = pProfileData;
		}
		else
		{
			WsWebService.GetUserProfileByUserID(uid, WsEventHandler, null);
		}
	}

	public void Init(string uid, UserProfileData profile)
	{
		mUserID = uid;
		mProfileData = profile;
	}

	public static void Init(UserProfileData profile)
	{
		pProfileData = profile;
		DistributeData();
	}

	public static void Reset()
	{
		pProfileData = null;
	}

	public static UserProfile LoadUserProfile(string uid)
	{
		UserProfile userProfile = new UserProfile();
		userProfile.Init(uid);
		return userProfile;
	}

	public void AnswerQuestion(ProfileUserAnswer s)
	{
		mProfileData.SetQuestionAnswer(s, WsEventHandler, null);
	}

	public static ProfileAnswer GetAnswer(int gid, int qid, int aid)
	{
		return GetQuestionList(gid).GetQuestion(qid).GetAnswer(aid);
	}

	public static ProfileQuestionList GetQuestionList(int gID)
	{
		if (pQuestionData == null || pQuestionData.Lists == null)
		{
			return null;
		}
		ProfileQuestionList[] lists = pQuestionData.Lists;
		foreach (ProfileQuestionList profileQuestionList in lists)
		{
			if (profileQuestionList.ID == gID)
			{
				return profileQuestionList;
			}
		}
		return null;
	}
}
