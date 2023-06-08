using System.Collections.Generic;
using UnityEngine;

public class Money : MonoBehaviour
{
	private static bool mInitialized = false;

	private static bool mIsReady = false;

	public static Money pInstance;

	public static GameObject mObject = null;

	public static bool mUpdateToolbar = true;

	private static UserGameCurrency mServerValue = null;

	private static int mDeltaGameCurrency = 0;

	private static int mSentGameCurrency = 0;

	protected static float mUpdateServerTimer = 30f;

	protected static float mUpdateServerTimerDuration = 30f;

	protected static bool mUpdateServerTimerActive = false;

	private static List<GameObject> mMessageObjectList = new List<GameObject>();

	public static bool pIsReady => mIsReady;

	public static bool pUpdateToolbar
	{
		get
		{
			return mUpdateToolbar;
		}
		set
		{
			mUpdateToolbar = value;
			if (mUpdateToolbar)
			{
				UpdateDisplay();
			}
		}
	}

	public static int pGameCurrency => mServerValue.GameCurrency.Value + mDeltaGameCurrency;

	public static int pCashCurrency => mServerValue.CashCurrency.Value;

	public static void Init()
	{
		if (mObject == null)
		{
			mObject = new GameObject("MoneyObj");
			Object.DontDestroyOnLoad(mObject);
			pInstance = mObject.AddComponent<Money>();
			mServerValue = new UserGameCurrency();
		}
		ResetTimer();
	}

	public static void ReInit()
	{
		Init();
		mInitialized = false;
	}

	public static void Reset()
	{
		if (mObject != null)
		{
			mInitialized = false;
			UpdateServer();
			Object.Destroy(mObject);
		}
	}

	public void OnApplicationQuit()
	{
		UpdateServer();
	}

	public void Update()
	{
		if (!mInitialized && UserProfile.pProfileData != null)
		{
			mInitialized = true;
			mIsReady = true;
			if (UserProfile.pProfileData.GameCurrency.HasValue)
			{
				mServerValue.GameCurrency = UserProfile.pProfileData.GameCurrency.Value;
			}
			else
			{
				mServerValue.GameCurrency = 0;
			}
			if (UserProfile.pProfileData.CashCurrency.HasValue)
			{
				mServerValue.CashCurrency = UserProfile.pProfileData.CashCurrency.Value;
			}
			mDeltaGameCurrency = 0;
			UpdateDisplay();
		}
		if (mUpdateServerTimerActive)
		{
			mUpdateServerTimer -= Time.deltaTime;
			if (mUpdateServerTimer <= 0f)
			{
				mUpdateServerTimerActive = false;
				UpdateServer();
			}
		}
	}

	private static void ResetTimer()
	{
		mUpdateServerTimerActive = false;
		mUpdateServerTimer = mUpdateServerTimerDuration;
	}

	public static void InitDefault()
	{
		mIsReady = true;
		mServerValue = new UserGameCurrency();
		mDeltaGameCurrency = 0;
		UpdateDisplay();
	}

	private static void Save()
	{
		if (pIsReady && mDeltaGameCurrency != 0)
		{
			WsWebService.SetGameCurrency(mDeltaGameCurrency, ServiceEventHandler, null);
			mSentGameCurrency = mDeltaGameCurrency;
			mDeltaGameCurrency = 0;
		}
	}

	private static void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_USER_GAME_CURRENCY:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				mIsReady = true;
				UserGameCurrency userGameCurrency = (UserGameCurrency)inObject;
				if (userGameCurrency == null)
				{
					UtDebug.Log("WEB SERVICE CALL GetUserGameCurrency FAILED!!!");
					InitDefault();
				}
				else
				{
					mDeltaGameCurrency = 0;
					mServerValue = userGameCurrency;
				}
				UpdateDisplay();
				break;
			}
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetUserGameCurrency FAILED!!!");
				InitDefault();
				break;
			}
			ResetTimer();
			break;
		case WsServiceType.SET_GAME_CURRENCY:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				int value = (int)inObject;
				mServerValue.GameCurrency = value;
				UpdateDisplay();
				ResetTimer();
				break;
			}
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL SetGameCurrency FAILED!!!");
				mDeltaGameCurrency += mSentGameCurrency;
				break;
			}
			ResetTimer();
			break;
		}
	}

	public static void AddMoney(int val, bool bForceUpdate)
	{
		if (pIsReady)
		{
			mDeltaGameCurrency += val;
			mUpdateServerTimerActive = true;
			if (bForceUpdate)
			{
				UpdateServer();
			}
			UpdateDisplay();
		}
	}

	public static void UpdateMoneyFromServer()
	{
		WsWebService.GetUserGameCurrency(UserInfo.pInstance.UserID, ServiceEventHandler, null);
	}

	public static void AddToGameCurrency(int val)
	{
		if (pIsReady)
		{
			mServerValue.GameCurrency += val;
			PlaySound();
			UpdateDisplay();
		}
	}

	public static void AddToCashCurrency(int val)
	{
		if (pIsReady)
		{
			mServerValue.CashCurrency += val;
			PlaySound();
			UpdateDisplay();
		}
	}

	public static void SetMoney(UserGameCurrency val)
	{
		if (pIsReady)
		{
			mServerValue = val;
			PlaySound();
			UpdateDisplay();
		}
	}

	public static void UpdateServer()
	{
		if (pIsReady)
		{
			ResetTimer();
			Save();
		}
	}

	public static void UpdateDisplay()
	{
		if (!pIsReady)
		{
			return;
		}
		if (mUpdateToolbar && AvAvatar.pToolbar != null)
		{
			KAUI component = AvAvatar.pToolbar.GetComponent<KAUI>();
			KAWidget kAWidget = component.FindItem("TxtCoinAmount");
			if (kAWidget != null)
			{
				kAWidget.SetText(pGameCurrency.ToString());
			}
			KAWidget kAWidget2 = component.FindItem("TxtGemAmount");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(pCashCurrency.ToString());
			}
		}
		foreach (GameObject mMessageObject in mMessageObjectList)
		{
			if (mMessageObject != null)
			{
				mMessageObject.SendMessage("OnMoneyUpdated", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public static void PlaySound()
	{
	}

	public static void AddNotificationObject(GameObject inMsgObj)
	{
		if (!mMessageObjectList.Contains(inMsgObj))
		{
			mMessageObjectList.Add(inMsgObj);
		}
		inMsgObj.SendMessage("OnMoneyUpdated", SendMessageOptions.DontRequireReceiver);
	}

	public static void RemoveNotificationObject(GameObject inMsgObj)
	{
		mMessageObjectList.Remove(inMsgObj);
	}
}
