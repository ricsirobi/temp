using UnityEngine;

public class UiEarnGems : KAUI, IAdResult
{
	public LocaleString _EarnGemsHeadingUnder18Text = new LocaleString("Earn Gems when your friends register or by watching videos!");

	public LocaleString _EarnGemsHeadingAbove18Text = new LocaleString("Earn Gems when your friends register, by watching videos or completing offers!");

	public LocaleString _GemsEarnedText = new LocaleString("You will be awarded {{GEM_COUNT}} shortly");

	public LocaleString _NoNetworkText = new LocaleString("Could not connect to internet. Please check your connection and try again.");

	public LocaleString _NoAdsAvailable = new LocaleString("Videos are not available! Please try again later.");

	public LocaleString _AdRewardSuccessText = new LocaleString("You earned %gems% gems!");

	public LocaleString _AdRewardFailedText = new LocaleString("Reward failed to load. Please try again later.");

	public UiEarnGemsResponse _GemsResponseUI;

	public int _OfferAgeLimit = 18;

	public AdEventType mAdEventType;

	private static GameObject mMessageObject;

	private KAUIGenericDB mGenericDB;

	private int mEarnedGems;

	public static UiEarnGems mCurrentInstance;

	public static void Show(GameObject inMessageObject)
	{
		if (UtPlatform.IsWSA() || UtPlatform.IsAmazon() || ((UtPlatform.IsStandAlone() || UtPlatform.IsMobile()) && !RequiredAdProvidersAvailable()))
		{
			InviteFriend.PopUpInviteFriend(inMessageObject);
		}
		else if (mCurrentInstance != null)
		{
			mMessageObject = inMessageObject;
			mCurrentInstance.SetVisibility(inVisible: true);
			mCurrentInstance.SetInteractive(interactive: true);
			KAUI.SetExclusive(mCurrentInstance);
		}
		else
		{
			mMessageObject = inMessageObject;
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = GameConfig.GetKeyData("EarnGemsAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnBundleReady, typeof(GameObject));
		}
	}

	public static bool RequiredAdProvidersAvailable()
	{
		if (AdManager.pInstance == null)
		{
			return false;
		}
		return AdManager.pInstance.AdProvidersAvailable(AdType.INTERSTITIAL);
	}

	protected override void Start()
	{
		base.Start();
		if (_GemsResponseUI != null)
		{
			_GemsResponseUI.SetVisibility(inVisible: false);
		}
		bool flag = ParentData.pInstance.IsAgeEligible(_OfferAgeLimit);
		if (UtPlatform.IsMobile())
		{
			FindItem("TxtSubHeading").SetText(flag ? _EarnGemsHeadingAbove18Text.GetLocalizedString() : _EarnGemsHeadingUnder18Text.GetLocalizedString());
			if (!flag)
			{
				FindItem("BtnCompleteOffers").SetVisibility(inVisible: false);
			}
		}
		mEarnedGems = 0;
	}

	private static void OnBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiEarnGems";
			UiEarnGems component = obj.GetComponent<UiEarnGems>();
			KAUI.SetExclusive(component);
			mCurrentInstance = component;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mMessageObject.SendMessage("OnEarnGemsClose");
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void OnAdWatched()
	{
		AdManager.pInstance.LogAdWatchedEvent(mAdEventType, "RewardGems");
		int num = int.Parse(AdManager.pInstance.GetAdReward(mAdEventType));
		if (num != -1)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			WsWebService.SetAchievementAndGetReward(num, "", ServiceEventHandler, true);
		}
	}

	public void OnAdFailed()
	{
		UtDebug.LogError("OnAdFailed for event:- " + mAdEventType);
		SetInteractive(interactive: true);
	}

	public void OnAdSkipped()
	{
		SetInteractive(interactive: true);
	}

	public void OnAdClosed()
	{
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			int num = -1;
			if (inObject != null)
			{
				AchievementReward[] array = (AchievementReward[])inObject;
				if (array != null)
				{
					GameUtilities.AddRewards(array, inUseRewardManager: false);
					AchievementReward[] array2 = array;
					foreach (AchievementReward achievementReward in array2)
					{
						int? pointTypeID = achievementReward.PointTypeID;
						if (pointTypeID.HasValue && pointTypeID.GetValueOrDefault() == 5 && inUserData != null && (bool)inUserData)
						{
							num = achievementReward.Amount.Value;
						}
					}
				}
			}
			if (inUserData != null && (bool)inUserData)
			{
				if (num > 0)
				{
					string localizedString = _AdRewardSuccessText.GetLocalizedString();
					localizedString = localizedString.Replace("%gems%", num.ToString());
					GameUtilities.DisplayOKMessage("PfKAUIGenericDBSm", localizedString, null, "");
				}
				else
				{
					GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _AdRewardFailedText.GetLocalizedString(), null, "");
				}
				AdManager.pInstance.SyncAdAvailableCount(mAdEventType, num > 0);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (inUserData != null && (bool)inUserData)
			{
				AdManager.pInstance.SyncAdAvailableCount(mAdEventType, isConsumed: false);
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _AdRewardFailedText.GetLocalizedString(), null, "");
			}
			break;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		mEarnedGems = 0;
		if (inWidget.name == "BtnInviteFriends")
		{
			SetInteractive(interactive: false);
			InviteFriend.PopUpInviteFriend(base.gameObject);
		}
		else if (inWidget.name == "BtnWatchVideos")
		{
			if (IsConnectedToInternet())
			{
				if (Screen.fullScreen)
				{
					Screen.fullScreen = false;
				}
				AudioListener.volume = 0f;
				AdType adType = AdType.REWARDED_VIDEO;
				mAdEventType = AdEventType.EARN_GEMS;
				if (AdManager.pInstance.AdAvailable(mAdEventType, adType))
				{
					AdManager.DisplayAd(mAdEventType, adType, base.gameObject);
				}
			}
		}
		else if (inWidget.name == "BtnCompleteOffers")
		{
			if (Application.isEditor)
			{
				OnOfferComplete(0);
			}
			else if (IsConnectedToInternet())
			{
				if (Screen.fullScreen)
				{
					Screen.fullScreen = false;
				}
				AudioListener.volume = 0f;
				if (UtPlatform.IsMobile())
				{
					AdManager.DisplayAd(AdType.WALL, base.gameObject, UserInfo.pInstance.UserID);
				}
			}
		}
		else if (inWidget.name == "CloseBtn")
		{
			Close();
		}
	}

	private bool IsConnectedToInternet()
	{
		if (!UtUtilities.IsConnectedToWWW())
		{
			mGenericDB = GameUtilities.DisplayOKMessage("PfKAUIGenericDBSm", _NoNetworkText._Text, base.gameObject, "OnCloseDB");
			return false;
		}
		return true;
	}

	private void OnCloseDB()
	{
		if (mGenericDB != null)
		{
			KAUI.RemoveExclusive(mGenericDB);
			mGenericDB.Destroy();
		}
	}

	public void Close()
	{
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
		mCurrentInstance = null;
		mMessageObject.SendMessage("OnEarnGemsClose");
	}

	public void OnInviteFriendLoaded()
	{
		SetVisibility(inVisible: false);
	}

	public void OnInviteFriendClosed()
	{
		SetVisibility(inVisible: true);
		SetInteractive(interactive: true);
	}

	private void OnOfferComplete(int gemCount)
	{
		mEarnedGems += gemCount;
		AudioListener.volume = 1f;
		if (mEarnedGems > 0)
		{
			string localizedString = _GemsEarnedText.GetLocalizedString();
			localizedString = localizedString.Replace("{{GEM_COUNT}}", mEarnedGems.ToString());
			if (mEarnedGems == 1)
			{
				localizedString = localizedString.Replace("Gems", "Gem");
			}
			if (_GemsResponseUI != null)
			{
				_GemsResponseUI.SetVisibility(inVisible: true);
				_GemsResponseUI.SetText(localizedString);
			}
			SetVisibility(inVisible: false);
		}
	}

	private void OnVideoAdsUnavailable()
	{
		mGenericDB = GameUtilities.DisplayOKMessage("PfKAUIGenericDBSm", _NoAdsAvailable._Text, base.gameObject, "OnCloseDB");
		AudioListener.volume = 1f;
	}
}
