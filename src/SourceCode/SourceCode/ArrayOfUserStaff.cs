using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ArrayOfUserStaff", Namespace = "http://api.jumpstart.com/")]
public class ArrayOfUserStaff
{
	private class UserStaffSave
	{
		public UserStaff mStaff;

		public UserStaffSaveEventHandler mCallback;
	}

	[XmlElement(ElementName = "UserStaff")]
	public UserStaff[] UserStaff;

	private static List<UserStaff> mList;

	private static ArrayOfUserStaff mInstance;

	private static int mStaffSaving;

	public static UserStaff[] pList => mList.ToArray();

	public static bool pIsReady => mInstance != null;

	public static int pStaffSaving => mStaffSaving;

	public static void Init(string inUserID)
	{
		mInstance = null;
		WsWebService.GetUserStaff(inUserID, ServiceEventHandler, null);
	}

	private static void InitDefault()
	{
		mInstance = new ArrayOfUserStaff();
		mList = new List<UserStaff>();
	}

	public static void Save(UserStaff staff, UserStaffSaveEventHandler callback)
	{
		Save(staff, UserStaffFlag.NONE, callback);
	}

	public static void Save(UserStaff staff, UserStaffFlag flag, UserStaffSaveEventHandler callback)
	{
		UserStaffSave userStaffSave = new UserStaffSave();
		userStaffSave.mStaff = staff;
		userStaffSave.mCallback = callback;
		mStaffSaving++;
		WsWebService.SetUserStaff(staff, flag, ServiceEventHandler, userStaffSave);
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_USER_STAFF_LIST:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mInstance = (ArrayOfUserStaff)inObject;
				if (mInstance != null)
				{
					if (mInstance.UserStaff != null)
					{
						mList = new List<UserStaff>(mInstance.UserStaff);
					}
					else
					{
						mList = new List<UserStaff>();
					}
				}
				else
				{
					InitDefault();
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("Get User Staff Failed!");
				if (mInstance == null)
				{
					InitDefault();
				}
				break;
			}
			break;
		case WsServiceType.SET_USER_STAFF:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				UserStaff userStaff = (UserStaff)inObject;
				UserStaffSave userStaffSave2 = (UserStaffSave)inUserData;
				mStaffSaving--;
				if (userStaff != null)
				{
					mList.Remove(userStaffSave2.mStaff);
					mList.Add(userStaff);
					if (userStaffSave2.mCallback != null)
					{
						userStaffSave2.mCallback(success: true, userStaff);
					}
				}
				else
				{
					UtDebug.LogError("Set User Staff Failed!");
					if (userStaffSave2.mCallback != null)
					{
						userStaffSave2.mCallback(success: false, null);
					}
				}
				break;
			}
			case WsServiceEvent.ERROR:
			{
				UtDebug.LogError("Set User Staff Failed!");
				mStaffSaving--;
				UserStaffSave userStaffSave = (UserStaffSave)inUserData;
				if (userStaffSave.mCallback != null)
				{
					userStaffSave.mCallback(success: false, null);
				}
				break;
			}
			}
			break;
		}
	}
}
