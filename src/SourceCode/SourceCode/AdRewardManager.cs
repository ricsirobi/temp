using System;
using System.Collections.Generic;
using UnityEngine;

public class AdRewardManager : MonoBehaviour, IAdResult
{
	[Serializable]
	public class AdEventRewardMap
	{
		public AdEventType _AdEventType;

		public MysteryBoxStoreInfo[] _RewardData;
	}

	public delegate void OnAdRewardDone();

	public AdEventRewardMap[] _AdEventRewardMap;

	private bool mBusy;

	public static AdRewardManager mInstance;

	private AdEventType mEventType;

	private AvAvatarState mPreviousAvatarState;

	private MysteryBoxStoreInfo mRewardInfo;

	public bool pBusy
	{
		get
		{
			return mBusy;
		}
		set
		{
			mBusy = value;
			if (mBusy)
			{
				if (AvAvatar.pState != AvAvatarState.PAUSED)
				{
					mPreviousAvatarState = AvAvatar.pState;
					AvAvatar.pState = AvAvatarState.PAUSED;
				}
			}
			else if (mPreviousAvatarState != AvAvatarState.PAUSED)
			{
				AvAvatar.pState = mPreviousAvatarState;
			}
		}
	}

	public static AdRewardManager pInstance => mInstance;

	public event OnAdRewardDone OnAdRewardDoneCallback;

	public static void Init()
	{
		if (!(mInstance != null))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("AdRewardManagerAsset"), OnAdRewardManagerLoaded, typeof(GameObject));
		}
	}

	public void RegisterEvent(OnAdRewardDone onAdRewardDone)
	{
		OnAdRewardDoneCallback += onAdRewardDone;
	}

	public void UnRegisterEvent(OnAdRewardDone onAdRewardDone)
	{
		OnAdRewardDoneCallback -= onAdRewardDone;
	}

	public static void OnAdRewardManagerLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			mInstance = obj.GetComponent<AdRewardManager>();
			UnityEngine.Object.DontDestroyOnLoad(obj);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error loading HUD Mystery chest manager!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void ShowAdRewards(AdEventType eventType)
	{
		if (_AdEventRewardMap != null && _AdEventRewardMap.Length != 0)
		{
			AdEventRewardMap adEventRewardMap = Array.Find(_AdEventRewardMap, (AdEventRewardMap x) => x._AdEventType == eventType);
			if (adEventRewardMap?._RewardData != null && adEventRewardMap._RewardData.Length != 0)
			{
				pBusy = true;
				mRewardInfo = adEventRewardMap._RewardData[UnityEngine.Random.Range(0, adEventRewardMap._RewardData.Length)];
				mEventType = eventType;
				LoadRewardDB(mRewardInfo);
			}
		}
	}

	private void LoadRewardDB(MysteryBoxStoreInfo rewardInfo)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("HUDAdRewardAsset"), OnRewardDBLoaded, typeof(GameObject));
	}

	private void OnRewardDBLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			obj.GetComponent<UiHUDAdReward>().Init(mRewardInfo, base.gameObject);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error loading HUD Mystery reward db!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			Exit();
			break;
		}
	}

	private void OnAdRewardDBAccepted(bool accepted)
	{
		if (accepted)
		{
			DisplayAd();
		}
		else
		{
			Exit();
		}
	}

	private void DisplayAd()
	{
		if (AdManager.pInstance.AdAvailable(mEventType, AdType.REWARDED_VIDEO))
		{
			AdManager.DisplayAd(mEventType, AdType.REWARDED_VIDEO, base.gameObject);
		}
		else
		{
			Exit();
		}
	}

	public void OnAdWatched()
	{
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		CommonInventoryData.pInstance.AddPurchaseItem(mRewardInfo._ItemID, 1);
		CommonInventoryData.pInstance.DoPurchase(2, mRewardInfo._StoreID, OnPurchaseComplete);
	}

	public void OnAdFailed()
	{
		Exit();
	}

	public void OnAdSkipped()
	{
		Exit();
	}

	public void OnAdClosed()
	{
		Exit();
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}

	public void OnPurchaseComplete(CommonInventoryResponse ret)
	{
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		if (ret != null && ret.Success)
		{
			AdManager.pInstance.SyncAdAvailableCount(mEventType, isConsumed: true);
			List<UserItemData> list = new List<UserItemData>();
			CommonInventoryResponseItem[] commonInventoryIDs = ret.CommonInventoryIDs;
			foreach (CommonInventoryResponseItem commonInventoryResponseItem in commonInventoryIDs)
			{
				UserItemData userItemData = CommonInventoryData.pInstance.FindItem(commonInventoryResponseItem.ItemID);
				if (userItemData != null)
				{
					list.Add(userItemData);
				}
			}
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources("PfUiRewardsDB");
			if (!(gameObject == null))
			{
				UnityEngine.Object.Instantiate(gameObject).GetComponent<UiRewardsDB>().Show(list, base.gameObject, groupItems: true);
			}
		}
		else
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", AdManager.pInstance._AdRewardFailedText.GetLocalizedString(), base.gameObject, "OnGenericDBClosed");
			AdManager.pInstance.SyncAdAvailableCount(mEventType, isConsumed: false);
			AdManager.pInstance.ResetAdCoolDown(mEventType);
		}
	}

	private void OnGenericDBClosed()
	{
		Exit();
	}

	private void OnRewardShown()
	{
		AvAvatar.SetUIActive(inActive: true);
		Money.pUpdateToolbar = true;
		Exit();
	}

	private void Exit()
	{
		pBusy = false;
		ResetData();
		if (this.OnAdRewardDoneCallback != null)
		{
			this.OnAdRewardDoneCallback();
		}
	}

	private void ResetData()
	{
		mEventType = AdEventType.NONE;
		mRewardInfo = null;
	}
}
