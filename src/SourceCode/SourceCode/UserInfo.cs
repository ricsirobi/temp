using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(Namespace = "http://api.jumpstart.com/", IsNullable = true)]
public class UserInfo
{
	[XmlElement(ElementName = "MembershipID", IsNullable = true)]
	public string MembershipID;

	[XmlElement(ElementName = "UserID", IsNullable = true)]
	public string UserID;

	[XmlElement(ElementName = "ParentUserID", IsNullable = true)]
	public string ParentUserID;

	[XmlElement(ElementName = "Username")]
	public string Username;

	[XmlElement(ElementName = "FirstName")]
	public string FirstName;

	[XmlElement(ElementName = "MultiplayerEnabled")]
	public bool MultiplayerEnabled;

	[XmlElement(ElementName = "BirthDate", IsNullable = true)]
	public DateTime? BirthDate;

	[XmlElement(ElementName = "GenderID", IsNullable = true)]
	public Gender? GenderID;

	[XmlElement(ElementName = "Age", IsNullable = true)]
	public int? Age;

	[XmlElement(ElementName = "Partner", IsNullable = true)]
	public string Partner;

	[XmlElement(ElementName = "lid", IsNullable = true)]
	public string Locale;

	[XmlElement(ElementName = "oce")]
	public bool OpenChatEnabled;

	[XmlElement(ElementName = "IsApproved")]
	public bool IsApproved;

	[XmlElement(ElementName = "RegistrationDate", IsNullable = true)]
	public DateTime? RegistrationDate;

	[XmlElement(ElementName = "FBID")]
	public long? FacebookUserID;

	[XmlElement(ElementName = "CreationDate", IsNullable = true)]
	public DateTime? CreationDate;

	public static bool pBirthdayCheat = false;

	private static UserInfo mInstance = null;

	private static bool mInitialized = false;

	private static string mBuddyCode = "";

	public static BuddyCodeEventHandler mBuddyCodeCallback = null;

	private static string mUserToken = "";

	public static UserInfo pInstance => mInstance;

	public static bool pIsReady => mInstance != null;

	public static string pBuddyCode => mBuddyCode;

	public static string pUserToken => mUserToken;

	public static void Init()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			WsWebService.GetUserInfoByApiToken(WsGetEventHandler, null);
		}
	}

	public static void Init(UserInfo userInfo)
	{
		mInitialized = true;
		mInstance = userInfo;
		UserRankData.Init();
		UtDebug.LogWarning("User ID is " + mInstance.UserID + " MMO enabled: " + mInstance.MultiplayerEnabled + " Chat enabled: " + mInstance.OpenChatEnabled + " IsApproved: " + mInstance.IsApproved);
	}

	public static void Reset()
	{
		mBuddyCode = "";
		mBuddyCodeCallback = null;
		mInitialized = false;
		mInstance = null;
	}

	public static void GetBuddyCode(BuddyCodeEventHandler callback)
	{
		mBuddyCodeCallback = callback;
		if (mBuddyCode.Length > 0)
		{
			if (mBuddyCodeCallback != null)
			{
				mBuddyCodeCallback(mBuddyCode);
			}
		}
		else
		{
			WsWebService.GetFriendCode(mInstance.UserID, WsGetEventHandler, null);
		}
	}

	public static bool IsBirthdayWeek()
	{
		if (!pIsReady || !pInstance.BirthDate.HasValue)
		{
			return false;
		}
		if (pBirthdayCheat)
		{
			return true;
		}
		DateTime value = pInstance.BirthDate.Value;
		DateTime dateTime = DateTime.Today;
		if (ServerTime.pIsReady)
		{
			dateTime = ServerTime.pCurrentTime.ToLocalTime();
		}
		value = value.AddYears(dateTime.Year - value.Year);
		TimeSpan timeSpan = dateTime - value;
		if (timeSpan.TotalDays > 3.0 || timeSpan.TotalDays < -3.0)
		{
			return false;
		}
		return true;
	}

	public static int GetAge()
	{
		if (mInstance != null && mInstance.Age.HasValue)
		{
			return mInstance.Age.Value;
		}
		return 0;
	}

	public static Gender GetGender()
	{
		if (mInstance != null && mInstance.GenderID.HasValue)
		{
			return mInstance.GenderID.Value;
		}
		return Gender.Unknown;
	}

	public static string GetPartner()
	{
		if (mInstance != null && !string.IsNullOrEmpty(mInstance.Partner))
		{
			return mInstance.Partner;
		}
		return "";
	}

	public static void WsGetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_USERINFO_BY_TOKEN:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mInstance = (UserInfo)inObject;
				if (mInstance == null)
				{
					UtDebug.LogError("WEB SERVICE CALL GetUserInfo RETURNED NO DATA!!!");
					UtUtilities.ShowGenericValidationError();
					break;
				}
				UtDebug.LogWarning("User ID is " + mInstance.UserID + " MMO enabled: " + mInstance.MultiplayerEnabled + " Chat enabled: " + mInstance.OpenChatEnabled);
				mUserToken = WsWebService.pUserToken;
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetUserInfo FAILED!!!");
				break;
			}
			break;
		case WsServiceType.GET_FRIEND_CODE:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject == null)
				{
					mBuddyCode = "?EMPTY?";
					UtDebug.LogError("WEB SERVICE CALL GetFriendCode RETURNED NO DATA!!!");
				}
				else
				{
					mBuddyCode = (string)inObject;
				}
				if (mBuddyCodeCallback != null)
				{
					mBuddyCodeCallback(mBuddyCode);
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetFriendCode FAILED!!!");
				mBuddyCode = "?ERROR?";
				if (mBuddyCodeCallback != null)
				{
					mBuddyCodeCallback(mBuddyCode);
				}
				break;
			}
			break;
		}
	}
}
