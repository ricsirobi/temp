using System;
using System.Text;
using SOD.Event;
using UnityEngine;

public class GameUtilities
{
	private static StringBuilder mStringBuilder = new StringBuilder(100);

	public static KAUIGenericDB CreateKAUIGenericDB(string inAssetName, string inDBName)
	{
		if (string.IsNullOrEmpty(inAssetName))
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(inAssetName));
		if (!string.IsNullOrEmpty(inDBName))
		{
			gameObject.name = inDBName;
		}
		return gameObject.GetComponent<KAUIGenericDB>();
	}

	public static string FormatTime(float time)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(time);
		int minutes = timeSpan.Minutes;
		int seconds = timeSpan.Seconds;
		int num = timeSpan.Milliseconds / 10;
		mStringBuilder.Length = 0;
		mStringBuilder.Append(minutes.ToString("d2") + ":");
		mStringBuilder.Append(seconds.ToString("d2") + ":");
		mStringBuilder.Append(num.ToString("d2"));
		return mStringBuilder.ToString();
	}

	public static string FormatTime(TimeSpan timeSpan)
	{
		if (timeSpan.TotalSeconds > 0.0)
		{
			return $"{timeSpan.Hours:d2}:{timeSpan.Minutes:d2}:{timeSpan.Seconds:d2}";
		}
		return "0";
	}

	public static string FormatTimeHHMM(int inSeconds, string inDelemeter = ":")
	{
		int num = inSeconds % 60;
		int num2 = inSeconds - num;
		int num3 = num2 / 60 % 60;
		int num4 = (num2 - num3 * 60) / 3600;
		return string.Concat(string.Concat(string.Empty + num4.ToString("d2"), inDelemeter), num3.ToString("d2"));
	}

	public static string FormatTimeHHMMSS(int inSeconds, string inDelemeter = ":")
	{
		int num = inSeconds % 60;
		int num2 = inSeconds - num;
		int num3 = num2 / 60 % 60;
		int num4 = (num2 - num3 * 60) / 3600;
		return string.Concat(string.Concat(string.Concat(string.Concat(string.Empty + num4.ToString("d2"), inDelemeter), num3.ToString("d2")), inDelemeter), num.ToString("d2"));
	}

	public static string FormatPosition(int inPosition)
	{
		string text = inPosition.ToString();
		if (inPosition > 10 && inPosition < 21)
		{
			return text + "th";
		}
		return (inPosition % 10) switch
		{
			1 => text + "st", 
			2 => text + "nd", 
			3 => text + "rd", 
			_ => text + "th", 
		};
	}

	public static void SortByName(ref Component[] t)
	{
		int num = t.Length;
		int num2 = 0;
		int num3 = 0;
		Component component = null;
		for (num2 = 0; num2 < num; num2++)
		{
			for (num3 = num2 + 1; num3 < num; num3++)
			{
				if (string.Compare(t[num3].name, t[num2].name) < 0)
				{
					component = t[num2];
					t[num2] = t[num3];
					t[num3] = component;
				}
			}
		}
	}

	public static KAUIGenericDB DisplayOKMessage(string inPrefabName, string inText, GameObject inMsgObj, string inCallBackFunction, bool updatePriority = false)
	{
		return DisplayOKMessage(inPrefabName, inText, null, inMsgObj, inCallBackFunction, updatePriority);
	}

	public static KAUIGenericDB DisplayOKMessage(string inPrefabName, string inText, string inTitle, GameObject inMsgObj, string inCallBackFunction, bool updatePriority = false)
	{
		KAUIGenericDB kAUIGenericDB = CreateKAUIGenericDB(inPrefabName, inPrefabName);
		if (kAUIGenericDB != null)
		{
			kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			kAUIGenericDB._MessageObject = inMsgObj;
			kAUIGenericDB._OKMessage = inCallBackFunction;
			kAUIGenericDB.SetText(inText, interactive: false);
			if (inTitle != null)
			{
				kAUIGenericDB.SetTitle(inTitle);
			}
			kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
			KAUI.SetExclusive(kAUIGenericDB, updatePriority);
		}
		return kAUIGenericDB;
	}

	public static KAUIGenericDB DisplayGenericDB(string inPrefabName, string inText, string inTitle, GameObject inMsgObj, string inYesMessage, string inNoMessage, string inOkMessage, string inCloseMessage, bool inDestroyOnClick = false, bool updatePriority = false)
	{
		KAUIGenericDB kAUIGenericDB = CreateKAUIGenericDB(inPrefabName, inPrefabName);
		if (kAUIGenericDB != null)
		{
			kAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(inYesMessage), !string.IsNullOrEmpty(inNoMessage), !string.IsNullOrEmpty(inOkMessage), !string.IsNullOrEmpty(inCloseMessage));
			kAUIGenericDB.SetText(inText, interactive: false);
			if (inTitle != null)
			{
				kAUIGenericDB.SetTitle(inTitle);
			}
			kAUIGenericDB._MessageObject = inMsgObj;
			kAUIGenericDB._YesMessage = inYesMessage;
			kAUIGenericDB._NoMessage = inNoMessage;
			kAUIGenericDB._OKMessage = inOkMessage;
			kAUIGenericDB._CloseMessage = inCloseMessage;
			kAUIGenericDB.SetDestroyOnClick(inDestroyOnClick);
			KAUI.SetExclusive(kAUIGenericDB, updatePriority);
		}
		return kAUIGenericDB;
	}

	public static void ShowRegisterMsg(string msg, GameObject msgObject)
	{
		UiOfferDB component = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiOfferDB")).GetComponent<UiOfferDB>();
		component._MessageObject = msgObject;
		component._DestroyAfterUse = true;
		component.SetText(msg);
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUI.SetExclusive(component, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public static bool HasPromptFrequencyReached(string prefKey, int requiredCount)
	{
		if (requiredCount == 0)
		{
			return true;
		}
		int num = 0;
		bool flag = false;
		if (PlayerPrefs.HasKey(prefKey))
		{
			num = PlayerPrefs.GetInt(prefKey, num);
			if (num + 1 >= requiredCount)
			{
				flag = true;
			}
		}
		num++;
		if (!flag)
		{
			PlayerPrefs.SetInt(prefKey, num);
		}
		else
		{
			PlayerPrefs.SetInt(prefKey, 0);
		}
		UtDebug.Log("Frequency count for " + prefKey + " is : " + num + ", required count : " + requiredCount);
		return flag;
	}

	public static void ResetGuestAccountData()
	{
		PlayerPrefs.DeleteKey("STORE_LAUNCH_COUNT");
		PlayerPrefs.DeleteKey("APP_LAUNCH_COUNT");
		UiLogin._RewardForRegistering = 0;
		UiLogin.pShowRegistrationPage = false;
	}

	public static void LoadLoginLevel(bool showRegstration = false, bool fullReset = true)
	{
		KAInput.ShowJoystick(JoyStickPos.BOTTOM_LEFT, inShow: false);
		Logout(fullReset);
		if (PlayerPrefs.GetInt("SafeAppClose") == 2)
		{
			PlayerPrefs.SetInt("SafeAppClose", 1);
		}
		UiLogin.pShowRegistrationPage = showRegstration;
		RsResourceManager.LoadLevel(GameConfig.GetKeyData("LoginScene"));
	}

	public static void Logout(bool fullReset = true)
	{
		if (AvAvatar.pObject != null)
		{
			AvAvatar.pObject.SetActive(value: false);
		}
		if (fullReset)
		{
			PlayerPrefs.DeleteKey("REM_USER_NAME");
			PlayerPrefs.DeleteKey("REM_PASSWORD");
			UiLogin.pParentInfo = null;
			ProductConfig.pToken = string.Empty;
			UserRankData.Reset();
			UserInfo.Reset();
			ProductData.Reset();
			SubscriptionInfo.Reset();
			CommonInventoryData.Reset();
			PairData.Reset();
			RaisedPetData.Reset();
			UserProfile.Reset();
			BuddyList.Reset();
			Money.Reset();
			Group.Reset();
			UserNotifyAnnouncements.Reset();
			UserNotifyExpansion.Reset();
			DailyBonusAndPromo.Reset();
			ParentData.Reset();
			IAPManager.Reset();
			if (AdManager.pInstance != null)
			{
				AdManager.pInstance.Reset();
			}
			WsTokenMonitor.pCheckToken = false;
		}
		UiChatHistory.HideChatHistory();
		SanctuaryManager.Reset();
		UserNotifyDragonTicket.Reset();
		MainStreetMMOClient._DefaultMMOStateSet = false;
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.Reset();
			MainStreetMMOClient.pInstance.Disconnect(clearZone: true);
		}
	}

	public static void AddRewards(AchievementReward[] rewards, bool inUseRewardManager = true, bool inImmediateShow = true)
	{
		foreach (AchievementReward achievementReward in rewards)
		{
			switch (achievementReward.PointTypeID.Value)
			{
			case 1:
			case 9:
			case 10:
			case 12:
				UserRankData.AddPoints(achievementReward.PointTypeID.Value, achievementReward.Amount.Value);
				break;
			case 2:
				if (!inUseRewardManager || RewardManager.pDisabled)
				{
					Money.AddToGameCurrency(achievementReward.Amount.Value);
				}
				break;
			case 5:
				if (!inUseRewardManager || RewardManager.pDisabled)
				{
					Money.AddToCashCurrency(achievementReward.Amount.Value);
				}
				break;
			case 6:
				if (CommonInventoryData.pIsReady)
				{
					if (achievementReward.UserItem != null)
					{
						CommonInventoryData.pInstance.AddUserItem(achievementReward.UserItem);
					}
					else
					{
						CommonInventoryData.pInstance.AddItem(achievementReward.ItemID, achievementReward.Amount.Value);
					}
					CommonInventoryData.pInstance.ClearSaveCache();
				}
				break;
			case 8:
				if (SanctuaryManager.pCurPetData != null && (string.IsNullOrEmpty(achievementReward.EntityID.ToString()) || achievementReward.EntityID.Equals(SanctuaryManager.pCurPetData.EntityID)))
				{
					SanctuaryManager.pInstance.AddXP(achievementReward.Amount.Value);
				}
				else
				{
					PetRankData.AddPoints(achievementReward.EntityID, achievementReward.Amount.Value);
				}
				break;
			}
		}
		if (inUseRewardManager && !RewardManager.pDisabled)
		{
			RewardManager.SetReward(rewards, inImmediateShow);
		}
	}

	public static void PlayAt(Vector3 inPosition, string fxName)
	{
		if (!string.IsNullOrEmpty(fxName))
		{
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(fxName);
			if (!(gameObject == null))
			{
				PlayAt(inPosition, gameObject);
			}
		}
	}

	public static GameObject PlayAt(Vector3 position, GameObject particleGo)
	{
		return UnityEngine.Object.Instantiate(particleGo, position, Quaternion.identity);
	}

	public static bool CheckItemValidity(string dateRange, string eventName)
	{
		if (CheckItemDateValidity(dateRange))
		{
			return CheckItemEventValidity(eventName);
		}
		return false;
	}

	public static bool CheckItemEventValidity(string eventName)
	{
		if (string.IsNullOrEmpty(eventName))
		{
			return true;
		}
		EventManager eventManager = EventManager.Get(eventName);
		if ((bool)eventManager)
		{
			return eventManager.EventInProgress();
		}
		return false;
	}

	public static bool CheckItemDateValidity(string dateRange)
	{
		if (string.IsNullOrEmpty(dateRange))
		{
			return true;
		}
		string[] array = dateRange.Split(',');
		if (array.Length != 2)
		{
			return true;
		}
		DateTime dateTime = Convert.ToDateTime(array[0], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
		DateTime dateTime2 = Convert.ToDateTime(array[1], UtUtilities.GetCultureInfo("en-US").DateTimeFormat);
		if (ServerTime.pCurrentTime > dateTime)
		{
			return ServerTime.pCurrentTime < dateTime2;
		}
		return false;
	}
}
