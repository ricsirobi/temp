public class AvatarDisplayDataInstance
{
	public AvatarDisplayData pAvatarDisplayData;

	public static AvatarDisplayDataInstance pInstance;

	public static bool pIsReady
	{
		get
		{
			if (pInstance != null)
			{
				return pInstance.pIsDataReady;
			}
			return false;
		}
	}

	public bool pIsDataReady => pAvatarDisplayData != null;

	public string GetDisplayName()
	{
		return pAvatarDisplayData.GetDisplayName();
	}

	public bool IsMember()
	{
		return pAvatarDisplayData.IsMember();
	}

	public void WsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && (inType == WsServiceType.GET_AVATAR_DISPLAY_DATA_BY_USER_ID || inType == WsServiceType.GET_AVATAR_DISPLAY_DATA))
		{
			pAvatarDisplayData = (AvatarDisplayData)inObject;
		}
	}

	private void GetData()
	{
		WsWebService.GetAvatarDisplayData(WsEventHandler, null);
	}

	private void GetData(string uid)
	{
		WsWebService.GetAvatarDisplayDataByUserID(uid, WsEventHandler, null);
	}

	public static AvatarDisplayDataInstance Init()
	{
		if (pInstance != null)
		{
			return pInstance;
		}
		pInstance = new AvatarDisplayDataInstance();
		pInstance.GetData();
		return pInstance;
	}

	public static AvatarDisplayDataInstance Init(string userID)
	{
		if (userID == UserInfo.pInstance.UserID)
		{
			return Init();
		}
		AvatarDisplayDataInstance avatarDisplayDataInstance = new AvatarDisplayDataInstance();
		avatarDisplayDataInstance.GetData(userID);
		return avatarDisplayDataInstance;
	}
}
