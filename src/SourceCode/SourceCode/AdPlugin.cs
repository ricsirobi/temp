using System;
using KA.Framework;
using UnityEngine;

public class AdPlugin : MonoBehaviour
{
	public string _ProviderName = "";

	public ProductPlatform[] _SupportedPlatforms;

	protected const int LOG_MASK = 256;

	protected string mAppID = "";

	protected string mSecretKey = "";

	protected string mUserID = "";

	protected bool mIsRegisteredForAds;

	protected bool mIsConnected;

	protected bool mIsAdRequested;

	protected bool mIsShowingAd;

	protected string mCurrentArgs;

	protected AdEventType mCurrentAdEvent = AdEventType.GENERIC;

	protected AdType mCurrentAdType;

	public AdEventType pCurrentAdEvent => mCurrentAdEvent;

	public AdType pCurrentAdType => mCurrentAdType;

	public GameObject pMessageObject { get; set; }

	public event Action ConnectSucceeded;

	public event Action ConnectFailed;

	public event Action<AdPlugin> Destroying;

	public event Action<int> RewardPointsEarned;

	public event Action<AdPlugin> AdRequested;

	public event Action<AdPlugin> AdRequestCancelled;

	public event Action<AdPlugin> AdOpened;

	public event Action<AdPlugin> AdClosed;

	public event Action<AdPlugin> AdFinished;

	public event Action<AdPlugin> AdSkipped;

	public event Action<AdPlugin, string> ShowAdFailed;

	public virtual void Init()
	{
	}

	protected virtual void Start()
	{
		AdData.mOnAdDataReady = (AdData.OnAdDataReady)Delegate.Combine(AdData.mOnAdDataReady, new AdData.OnAdDataReady(CheckAndEnablePlugin));
	}

	protected virtual void StartPlugin()
	{
	}

	private void CheckAndEnablePlugin()
	{
		if (!CanEnable())
		{
			base.enabled = false;
			return;
		}
		StartPlugin();
		AdData.mOnAdDataReady = (AdData.OnAdDataReady)Delegate.Remove(AdData.mOnAdDataReady, new AdData.OnAdDataReady(CheckAndEnablePlugin));
	}

	private bool CanEnable()
	{
		bool result = true;
		AdsNetworkEnableInfo adsNetworkEnableInfo = AdData.GetAdsNetworkEnableInfo(ProductSettings.GetPlatform(), _ProviderName);
		if (adsNetworkEnableInfo != null)
		{
			if (!adsNetworkEnableInfo.Disabled)
			{
				long num = adsNetworkEnableInfo.MinimumRAM;
				if (UtMobileUtilities.GetPhysicalMemory() / 1048576 < num)
				{
					result = false;
				}
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	public virtual void SetAge(int age)
	{
	}

	protected virtual void OnDestroy()
	{
		mIsConnected = false;
		ResetCurrentAd();
		if (this.Destroying != null)
		{
			this.Destroying(this);
		}
	}

	public virtual bool ShowAd(AdParams adParam)
	{
		if (!mIsConnected)
		{
			UtDebug.LogError("AdPlugin " + _ProviderName + " not initialized.");
			return false;
		}
		if (mIsAdRequested || mIsShowingAd)
		{
			UtDebug.LogError("AdPlugin.ShowAd: Already processing another Ad.", 256);
			return false;
		}
		if (!string.IsNullOrEmpty(adParam.adProvider) && adParam.adProvider != _ProviderName)
		{
			return false;
		}
		mCurrentAdType = adParam.adType;
		mCurrentAdEvent = adParam.adEvent;
		mCurrentArgs = adParam.data;
		pMessageObject = adParam.gameObject;
		return true;
	}

	public virtual bool AdAvailable(AdType type)
	{
		return false;
	}

	public virtual void CancelAdRequest()
	{
		if (!mIsAdRequested)
		{
			UtDebug.LogError("AdPlugin.CancelAdRequest: No ads are currently srequested.", 256);
			return;
		}
		mIsAdRequested = false;
		OnCancelAdRequest();
	}

	public virtual void SetUserID(string inUserID)
	{
		mUserID = inUserID;
	}

	protected virtual void ResetCurrentAd()
	{
		mIsAdRequested = false;
		mIsShowingAd = false;
		mCurrentAdEvent = AdEventType.GENERIC;
		mCurrentArgs = null;
	}

	protected virtual void OnConnectSucceeded()
	{
		mIsConnected = true;
		if (this.ConnectSucceeded != null)
		{
			this.ConnectSucceeded();
		}
	}

	protected virtual void OnConnectFailed()
	{
		mIsConnected = false;
		if (this.ConnectFailed != null)
		{
			this.ConnectFailed();
		}
	}

	protected virtual void OnRewardPointsEarned(int inPoints)
	{
		if (this.RewardPointsEarned != null)
		{
			this.RewardPointsEarned(inPoints);
		}
		ResetCurrentAd();
	}

	protected virtual void OnAdRequested()
	{
		mIsAdRequested = true;
		if (this.AdRequested != null)
		{
			this.AdRequested(this);
		}
	}

	protected virtual void OnCancelAdRequest()
	{
		if (this.AdRequestCancelled != null)
		{
			this.AdRequestCancelled(this);
		}
		ResetCurrentAd();
	}

	protected virtual void OnAdOpened()
	{
		mIsShowingAd = true;
		if (this.AdOpened != null)
		{
			this.AdOpened(this);
		}
	}

	protected virtual void OnAdClosed()
	{
		if (this.AdClosed != null)
		{
			this.AdClosed(this);
		}
		ResetCurrentAd();
	}

	protected virtual void OnAdFinished()
	{
		if (this.AdFinished != null)
		{
			this.AdFinished(this);
		}
		ResetCurrentAd();
	}

	protected virtual void OnAdSkipped()
	{
		if (this.AdSkipped != null)
		{
			this.AdSkipped(this);
		}
		ResetCurrentAd();
	}

	protected virtual void OnShowAdFailed(string inStrError)
	{
		if (this.ShowAdFailed != null)
		{
			this.ShowAdFailed(this, inStrError);
		}
		ResetCurrentAd();
	}
}
