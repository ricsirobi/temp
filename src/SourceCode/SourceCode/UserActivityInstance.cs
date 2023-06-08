using System.Collections.Generic;

public class UserActivityInstance
{
	private class UserActivitySave
	{
		public UserActivity mActivity;

		public UserActivityEventHandler mCallback;
	}

	public InitUserActivityEventHandler mCallback;

	private ArrayOfUserActivity mInstance;

	private List<UserActivity> mList;

	public bool pIsReady => mInstance != null;

	public ArrayOfUserActivity pInstance => mInstance;

	public List<UserActivity> pList
	{
		get
		{
			return mList;
		}
		set
		{
			mList = value;
		}
	}

	private void InitDefault()
	{
		mInstance = new ArrayOfUserActivity();
		mList = new List<UserActivity>();
	}

	public static UserActivityInstance Init(string inUserID, InitUserActivityEventHandler callback)
	{
		UserActivityInstance userActivityInstance = new UserActivityInstance();
		userActivityInstance.mCallback = callback;
		WsWebService.GetUserActivity(inUserID, userActivityInstance.ServiceEventHandler, userActivityInstance);
		return userActivityInstance;
	}

	public void Save(UserActivity inUserActivity, UserActivityEventHandler callback)
	{
		UserActivitySave userActivitySave = new UserActivitySave();
		userActivitySave.mActivity = inUserActivity;
		userActivitySave.mCallback = callback;
		WsWebService.SetUserActivity(inUserActivity, ServiceEventHandler, userActivitySave);
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_USER_ACTIVITY_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				mInstance = (ArrayOfUserActivity)inObject;
				UserActivityInstance userActivityInstance2 = (UserActivityInstance)inUserData;
				if (mInstance != null)
				{
					if (mInstance.UserActivity != null)
					{
						mList = new List<UserActivity>(mInstance.UserActivity);
					}
					else
					{
						mList = new List<UserActivity>();
					}
				}
				else
				{
					InitDefault();
				}
				if (userActivityInstance2.mCallback != null)
				{
					userActivityInstance2.mCallback(success: true);
				}
				break;
			}
			case WsServiceEvent.ERROR:
			{
				UtDebug.LogError("Get User Activity Failed!");
				UserActivityInstance userActivityInstance = (UserActivityInstance)inUserData;
				if (userActivityInstance.mCallback != null)
				{
					userActivityInstance.mCallback(success: false);
				}
				break;
			}
			}
			break;
		case WsServiceType.SET_USER_ACTIVITY:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				UserActivity userActivity = (UserActivity)inObject;
				UserActivitySave userActivitySave2 = (UserActivitySave)inUserData;
				if (userActivity != null && mInstance != null)
				{
					mList.Remove(userActivitySave2.mActivity);
					mList.Add(userActivity);
					if (userActivitySave2.mCallback != null)
					{
						userActivitySave2.mCallback(success: true, userActivity);
					}
				}
				else
				{
					UtDebug.LogError("Set User Activity Failed!");
					if (userActivitySave2.mCallback != null)
					{
						userActivitySave2.mCallback(success: false, null);
					}
				}
				break;
			}
			case WsServiceEvent.ERROR:
			{
				UtDebug.LogError("Set User Activity Failed!");
				UserActivitySave userActivitySave = (UserActivitySave)inUserData;
				if (userActivitySave.mCallback != null)
				{
					userActivitySave.mCallback(success: false, null);
				}
				break;
			}
			}
			break;
		}
	}
}
