using System;
using System.Collections.Generic;

public class Nicknames
{
	private class SetNicknameData
	{
		public string OtherUserID;

		public string Nickname;
	}

	private static Nicknames mInstance;

	private static bool mInitialized;

	private static List<Nickname> mList;

	private NicknamesEventHandler mEventDelegate;

	public static Nicknames pInstance
	{
		get
		{
			if (!pIsReady)
			{
				Init();
			}
			return mInstance;
		}
	}

	public static bool pIsReady => mInstance != null;

	public static Nickname[] pList => mList.ToArray();

	public NicknamesEventHandler pEventDelegate
	{
		get
		{
			return mEventDelegate;
		}
		set
		{
			mEventDelegate = value;
		}
	}

	public static void Init()
	{
		if (!mInitialized)
		{
			mInitialized = true;
			WsWebService.GetNicknames(UserInfo.pInstance.UserID, null, ServiceEventHandler, null);
		}
	}

	public static void Uninit()
	{
		mInitialized = false;
		mList = null;
		mInstance = null;
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_NICKNAMES:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				Nickname[] array = (Nickname[])inObject;
				if (array != null)
				{
					mInstance = new Nicknames();
					mList = new List<Nickname>(array);
				}
				else
				{
					UtDebug.Log("WEB SERVICE CALL GetNicknames RETURNED NO DATA!!!");
					InitDefault();
				}
				break;
			}
			case WsServiceEvent.ERROR:
				InitDefault();
				break;
			}
			break;
		case WsServiceType.SET_NICKNAME:
			if (inEvent != WsServiceEvent.COMPLETE && inEvent != WsServiceEvent.ERROR)
			{
				break;
			}
			if ((NicknameSetResult)inObject == NicknameSetResult.Success)
			{
				SetNicknameData setNicknameData = (SetNicknameData)inUserData;
				if (setNicknameData != null)
				{
					mInstance.UpdateNickname(setNicknameData.OtherUserID, setNicknameData.Nickname);
				}
			}
			if (mInstance.mEventDelegate != null)
			{
				mInstance.mEventDelegate((NicknameSetResult)inObject);
			}
			mInstance.mEventDelegate = null;
			break;
		}
	}

	public static void InitDefault()
	{
		mInstance = new Nicknames();
		mList = new List<Nickname>();
	}

	public void SetNickname(string inOtherUserID, string inNickname, NicknamesEventHandler inCallback)
	{
		if (!string.IsNullOrEmpty(inNickname) && IsNicknameAlreadyForAnyBuddy(inNickname))
		{
			inCallback?.Invoke(NicknameSetResult.Failure);
			inCallback = null;
			return;
		}
		mEventDelegate = inCallback;
		SetNicknameData setNicknameData = new SetNicknameData();
		setNicknameData.OtherUserID = inOtherUserID;
		setNicknameData.Nickname = inNickname;
		WsWebService.SetNickname(UserInfo.pInstance.UserID, inOtherUserID, inNickname, ServiceEventHandler, setNicknameData);
	}

	private bool IsNicknameAlreadyForAnyBuddy(string inNickname)
	{
		foreach (Nickname m in mList)
		{
			if (m.Name == inNickname)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateNickname(string inUserID, string inNickname)
	{
		bool flag = false;
		foreach (Nickname m in mList)
		{
			if (m.UserID.ToString() == inUserID)
			{
				m.Name = inNickname;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			AddNickname(inUserID, inNickname);
		}
	}

	private void AddNickname(string inUserID, string inNickname)
	{
		Nickname nickname = new Nickname();
		Guid userID = new Guid(inUserID);
		nickname.UserID = userID;
		nickname.Name = inNickname;
		mList.Add(nickname);
	}

	public string GetNickname(string inUserID)
	{
		foreach (Nickname m in mList)
		{
			if (m.UserID.ToString() == inUserID)
			{
				return m.Name;
			}
		}
		return "";
	}

	public void RemoveNickname(string inUserID)
	{
		foreach (Nickname m in mList)
		{
			if (m.UserID.ToString() == inUserID)
			{
				mList.Remove(m);
				break;
			}
		}
	}
}
