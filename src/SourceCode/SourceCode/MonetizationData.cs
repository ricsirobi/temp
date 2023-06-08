using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "MonetizationData", Namespace = "")]
public class MonetizationData
{
	[XmlElement(ElementName = "RegisterMsg")]
	public RegisterMsg[] mGuestUserMsgList;

	[XmlElement(ElementName = "UserMsg")]
	public UserMsg[] mUserMsgs;

	[XmlElement(ElementName = "GrowthMsg")]
	public GrowthMsg[] mGrowthMsg;

	[XmlElement(ElementName = "OtherGames")]
	public OtherGames[] mOtherGames;

	[XmlElement(ElementName = "ProfileSlotsData")]
	public ProfileSlotsData mProfileSlotsData;

	private static MonetizationData mMonetizationSettings;

	private AssetBundle mAssetBundle;

	private static bool mIsReady;

	public static bool pIsReady => mIsReady;

	public static void Init()
	{
		if (mMonetizationSettings == null)
		{
			RsResourceManager.Load(GameConfig.GetKeyData("MonetizationDataFile"), XmlLoadEventHandler);
		}
	}

	public static bool IsAssetBundleLoaded()
	{
		if (mMonetizationSettings == null)
		{
			return false;
		}
		return mMonetizationSettings.mAssetBundle != null;
	}

	public static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			if (inObject == null)
			{
				break;
			}
			UtDebug.Log("Monetization \n" + (string)inObject);
			using (StringReader textReader = new StringReader((string)inObject))
			{
				mMonetizationSettings = new XmlSerializer(typeof(MonetizationData)).Deserialize(textReader) as MonetizationData;
				if (mMonetizationSettings.mOtherGames != null && mMonetizationSettings.mOtherGames.Length != 0)
				{
					Array.Sort(mMonetizationSettings.mOtherGames[0].mURLs, (OtherGamesURLs og1, OtherGamesURLs og2) => og1.mPriority.CompareTo(og2.mPriority));
				}
				mMonetizationSettings.LoadIconBundle();
			}
			mIsReady = true;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mMonetizationSettings = null;
			UtDebug.LogError("Monetization settings missing!!!");
			SetDefault();
			mIsReady = true;
			break;
		}
	}

	public static RegisterMsg GetRegisterMsg(string category)
	{
		if (mMonetizationSettings == null || mMonetizationSettings.mGuestUserMsgList == null)
		{
			SetDefaultRegisterMessages();
		}
		RegisterMsg[] array = mMonetizationSettings.mGuestUserMsgList;
		foreach (RegisterMsg registerMsg in array)
		{
			if (category.Equals(registerMsg.mCategory, StringComparison.OrdinalIgnoreCase))
			{
				return registerMsg;
			}
		}
		UtDebug.Log("Category " + category + " not found.");
		return null;
	}

	public static UserMsg GetUserMsg(string category)
	{
		if (mMonetizationSettings == null || mMonetizationSettings.mUserMsgs == null)
		{
			_ = mMonetizationSettings;
			return null;
		}
		UserMsg[] array = mMonetizationSettings.mUserMsgs;
		foreach (UserMsg userMsg in array)
		{
			if (category.Equals(userMsg.mCategory, StringComparison.OrdinalIgnoreCase))
			{
				return userMsg;
			}
		}
		UtDebug.Log("Category " + category + " not found.");
		return null;
	}

	public static string GetGrowthMsg(int petTypeID, string age)
	{
		if (mMonetizationSettings == null || mMonetizationSettings.mGrowthMsg == null)
		{
			_ = mMonetizationSettings;
			return null;
		}
		GrowthMsg[] array = mMonetizationSettings.mGrowthMsg;
		foreach (GrowthMsg growthMsg in array)
		{
			if (growthMsg.mPetTypeID != petTypeID)
			{
				continue;
			}
			AgeMsg[] mAgeMsgs = growthMsg.mAgeMsgs;
			foreach (AgeMsg ageMsg in mAgeMsgs)
			{
				if (age.Equals(ageMsg.mAge, StringComparison.OrdinalIgnoreCase))
				{
					return ageMsg.mMessage;
				}
			}
		}
		UtDebug.Log("Data not found for petTypeID : " + petTypeID + " & age : " + age);
		return "";
	}

	public static OtherGames[] GetOtherGameData()
	{
		if (mMonetizationSettings == null)
		{
			return null;
		}
		return mMonetizationSettings.mOtherGames;
	}

	public static string GetDefaultGameURL()
	{
		if (mMonetizationSettings == null || mMonetizationSettings.mOtherGames == null || mMonetizationSettings.mOtherGames.Length == 0)
		{
			return null;
		}
		return mMonetizationSettings.mOtherGames[0].mDefaultURL;
	}

	private void LoadIconBundle()
	{
		if (mMonetizationSettings != null && mMonetizationSettings.mOtherGames != null && mMonetizationSettings.mOtherGames.Length != 0)
		{
			string mIcon = mMonetizationSettings.mOtherGames[0].mIcon;
			if (string.IsNullOrEmpty(mIcon))
			{
				UtDebug.LogError("Bundle name not specified in the xml file.");
			}
			else
			{
				RsResourceManager.Load(mIcon, OnIconBundleLoaded, RsResourceType.NONE, inDontDestroy: true);
			}
		}
	}

	public void OnIconBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		if (inLoadEvent == RsResourceLoadEvent.COMPLETE)
		{
			mAssetBundle = (AssetBundle)inObject;
		}
		else
		{
			_ = 3;
		}
	}

	public static Texture2D GetIcon(bool isHighlight)
	{
		if (mMonetizationSettings == null || mMonetizationSettings.mAssetBundle == null || mMonetizationSettings.mOtherGames == null || mMonetizationSettings.mOtherGames.Length == 0)
		{
			return null;
		}
		string name = mMonetizationSettings.mOtherGames[0].mIconNormal;
		if (isHighlight)
		{
			name = mMonetizationSettings.mOtherGames[0].mIconHighlight;
		}
		return mMonetizationSettings.mAssetBundle.LoadAsset(name, typeof(Texture2D)) as Texture2D;
	}

	public static ProfileSlotsData GetProfileSlotData()
	{
		if (mMonetizationSettings == null || mMonetizationSettings.mProfileSlotsData == null || mMonetizationSettings.mProfileSlotsData.mFreeSlotCount == -1)
		{
			return null;
		}
		return mMonetizationSettings.mProfileSlotsData;
	}

	public static void Dump()
	{
		UtDebug.Log("mMonetizationSettings is null? : " + (mMonetizationSettings == null));
		if (mMonetizationSettings == null)
		{
			return;
		}
		UtDebug.Log("mMonetizationSettings.mGuestUserMsgList is null? : " + (mMonetizationSettings.mGuestUserMsgList == null));
		UtDebug.Log("mMonetizationSettings.mUserMsgs is null? : " + (mMonetizationSettings.mUserMsgs == null));
		UtDebug.Log("----Dumping  Monetization data started-------");
		if (mMonetizationSettings.mGuestUserMsgList != null)
		{
			RegisterMsg[] array = mMonetizationSettings.mGuestUserMsgList;
			foreach (RegisterMsg registerMsg in array)
			{
				if (registerMsg != null)
				{
					UtDebug.Log("Category : " + registerMsg.mCategory + ", Msg :" + registerMsg.mMessage?.ToString() + ", Credits : " + registerMsg.mCredits + ", Frequency : " + registerMsg.mFrequency);
				}
			}
		}
		if (mMonetizationSettings.mUserMsgs != null)
		{
			UserMsg[] array2 = mMonetizationSettings.mUserMsgs;
			foreach (UserMsg userMsg in array2)
			{
				if (userMsg != null)
				{
					UtDebug.Log("Category : " + userMsg.mCategory + ", Guest :" + userMsg.mGuest + ", Registered : " + userMsg.mRegistered);
				}
			}
		}
		if (mMonetizationSettings.mGrowthMsg != null)
		{
			GrowthMsg[] array3 = mMonetizationSettings.mGrowthMsg;
			foreach (GrowthMsg growthMsg in array3)
			{
				UtDebug.Log("Pet Type ID : " + growthMsg.mPetTypeID);
				if (growthMsg != null)
				{
					AgeMsg[] mAgeMsgs = growthMsg.mAgeMsgs;
					foreach (AgeMsg ageMsg in mAgeMsgs)
					{
						UtDebug.Log("Age : " + ageMsg.mAge + ", Message :" + ageMsg.mMessage);
					}
				}
			}
		}
		if (mMonetizationSettings.mOtherGames != null)
		{
			OtherGames[] array4 = mMonetizationSettings.mOtherGames;
			foreach (OtherGames otherGames in array4)
			{
				UtDebug.LogError("Game Name : " + otherGames.mName);
				UtDebug.LogError("Default URL : " + otherGames.mDefaultURL);
				UtDebug.LogError("Num URL`s : " + otherGames.mURLs.Length);
				OtherGamesURLs[] mURLs = otherGames.mURLs;
				foreach (OtherGamesURLs otherGamesURLs in mURLs)
				{
					UtDebug.LogError("Game URL : " + otherGamesURLs.mURL);
					UtDebug.LogError("Game Priority : " + otherGamesURLs.mPriority);
				}
				UtDebug.LogError("Game ICO : " + otherGames.mIcon);
			}
		}
		UtDebug.Log("----Dumping  Monetization data done-------");
	}

	private static void SetDefault()
	{
		UtDebug.Log("Using Default Monetization data.");
		mMonetizationSettings = new MonetizationData();
		SetDefaultRegisterMessages();
		mMonetizationSettings.mUserMsgs = new UserMsg[5];
		mMonetizationSettings.mUserMsgs[0] = new UserMsg();
		mMonetizationSettings.mUserMsgs[0].mCategory = "MyProfile";
		mMonetizationSettings.mUserMsgs[0].mGuest = "Share fun facts about yourself with your friends.";
		mMonetizationSettings.mUserMsgs[0].mRegistered = "Share fun facts about yourself with your friends.";
		mMonetizationSettings.mUserMsgs[1] = new UserMsg();
		mMonetizationSettings.mUserMsgs[1].mCategory = "OthersProfile";
		mMonetizationSettings.mUserMsgs[1].mGuest = "Check out this page to learn more about this Blaster.";
		mMonetizationSettings.mUserMsgs[1].mRegistered = "Check out this page to learn more about this Blaster.";
		mMonetizationSettings.mUserMsgs[2] = new UserMsg();
		mMonetizationSettings.mUserMsgs[2].mCategory = "FriendFinder";
		mMonetizationSettings.mUserMsgs[2].mGuest = "Tap the \"+\" in Random friends to connect with new friends.";
		mMonetizationSettings.mUserMsgs[2].mRegistered = "Tap the \"+\" in Random friends to connect with new friends.";
		mMonetizationSettings.mUserMsgs[3] = new UserMsg();
		mMonetizationSettings.mUserMsgs[3].mCategory = "FriendFinderNoFriends";
		mMonetizationSettings.mUserMsgs[3].mGuest = "Tap the Random button and then tap the \"+\" to connect with new friends.";
		mMonetizationSettings.mUserMsgs[3].mRegistered = "Tap the Random button and then tap the \"+\" to connect with new friends.";
		mMonetizationSettings.mUserMsgs[4] = new UserMsg();
		mMonetizationSettings.mUserMsgs[4].mCategory = "FriendFinderRandom";
		mMonetizationSettings.mUserMsgs[4].mGuest = "Tap the \"+\" to connect with new friends.";
		mMonetizationSettings.mUserMsgs[4].mRegistered = "Tap the \"+\" to connect with new friends.";
	}

	private static void SetDefaultRegisterMessages()
	{
		UtDebug.Log("Using Default Monetization data for Guest User offer wall.");
		if (mMonetizationSettings == null)
		{
			mMonetizationSettings = new MonetizationData();
		}
		mMonetizationSettings.mGuestUserMsgList = new RegisterMsg[4];
		mMonetizationSettings.mGuestUserMsgList[0] = new RegisterMsg();
		mMonetizationSettings.mGuestUserMsgList[0].mCategory = "Store";
		mMonetizationSettings.mGuestUserMsgList[0].mMessage = new LocaleString("Need some money to spend in the store? Register now and get {$} Cerdits to spend!.");
		mMonetizationSettings.mGuestUserMsgList[0].mCredits = 1000;
		mMonetizationSettings.mGuestUserMsgList[0].mFrequency = 2;
		mMonetizationSettings.mGuestUserMsgList[1] = new RegisterMsg();
		mMonetizationSettings.mGuestUserMsgList[1].mCategory = "MiniGames";
		mMonetizationSettings.mGuestUserMsgList[1].mMessage = new LocaleString("Register now and earn {$} free Credits!.");
		mMonetizationSettings.mGuestUserMsgList[1].mCredits = 500;
		mMonetizationSettings.mGuestUserMsgList[1].mFrequency = 2;
		mMonetizationSettings.mGuestUserMsgList[2] = new RegisterMsg();
		mMonetizationSettings.mGuestUserMsgList[2].mCategory = "MuttPod";
		mMonetizationSettings.mGuestUserMsgList[2].mMessage = new LocaleString("Register now and get {$} credits to spend on your mutt!.");
		mMonetizationSettings.mGuestUserMsgList[2].mCredits = 1000;
		mMonetizationSettings.mGuestUserMsgList[2].mFrequency = 2;
		mMonetizationSettings.mGuestUserMsgList[3] = new RegisterMsg();
		mMonetizationSettings.mGuestUserMsgList[3].mCategory = "GrowPrompt";
		mMonetizationSettings.mGuestUserMsgList[3].mMessage = new LocaleString("Register now and get {$} credits for your growing mutt!.");
		mMonetizationSettings.mGuestUserMsgList[3].mCredits = 500;
		mMonetizationSettings.mGuestUserMsgList[3].mFrequency = 2;
	}
}
