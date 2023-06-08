using System;
using System.Collections.Generic;
using UnityEngine;

public class UiWorldEventRewards : KAUI, IAdResult
{
	public class RewardItemdBundleData : CoBundleItemData
	{
		public KAWidget _TxtRewardWidget;

		public int _Quantity;
	}

	[Serializable]
	public class RewardData
	{
		public int PointTypeID;

		public int Value;

		public LocaleString Title;
	}

	[Serializable]
	public class RewardPositionsData
	{
		public Vector2[] _Positions;
	}

	public LocaleString _UseMysteryBoxTicketText = new LocaleString("Use Mystery box ticket. Do you wish to continue?");

	public LocaleString _AdRewardFailedText = new LocaleString("Ad rewards failed to load. Please try again later");

	public string _MysteryBoxBundlePath = "RS_DATA/PfUiClaimMysteryBoxDO.unity3d/PfUiClaimMysteryBoxDO";

	public int _RedeemItemFetchCount = 10;

	public RewardWidget _XPRewardWidget;

	public RewardWidget _ClansRewardWidget;

	public List<MissionRewardData> _RewardData = new List<MissionRewardData>();

	public RewardData[] _ClanRewards;

	public AdEventType _AdEventType;

	public UITexture _TierRewardSprite;

	private KAWidget mMysteryBoxBtn;

	private KAWidget mTierTxt;

	private KAWidget mDefeatTxt;

	private KAWidget mItemQtyTxt;

	private KAWidget mClanRewards;

	private KAWidget mEventRewards;

	private KAWidget mMysteryBoxIcon;

	private KAWidget mBtnAds;

	private UiWorldEventEndDB mUiWorldEventEndDB;

	private List<CommonInventoryResponseItem> mLastReceivedItems = new List<CommonInventoryResponseItem>();

	private UserItemData mSelectedUserItemData;

	private bool mMysteryBoxAvailable;

	public int pAdRewardAchievementID { get; set; }

	protected override void Start()
	{
		base.Start();
		mTierTxt = FindItem("TxtTierName");
		mDefeatTxt = FindItem("TxtDefeat");
		mItemQtyTxt = FindItem("TxtItemQty");
		mMysteryBoxBtn = FindItem("OpenBtn");
		mEventRewards = FindItem("EventRewards");
		mClanRewards = FindItem("ClanRewards");
		mMysteryBoxIcon = FindItem("MysteryBoxIcon");
		mBtnAds = FindItem("BtnAds");
		mUiWorldEventEndDB = GetComponentInParent<UiWorldEventEndDB>();
		if (_ClanRewards.Length == 0)
		{
			return;
		}
		AchievementReward[] array = new AchievementReward[_ClanRewards.Length];
		for (int i = 0; i < array.Length; i++)
		{
			AchievementReward achievementReward = new AchievementReward();
			achievementReward.PointTypeID = _ClanRewards[i].PointTypeID;
			if (achievementReward.PointTypeID == 6)
			{
				achievementReward.ItemID = _ClanRewards[i].Value;
				achievementReward.Amount = 1;
			}
			else
			{
				achievementReward.Amount = _ClanRewards[i].Value;
			}
			array[i] = achievementReward;
		}
		_ClansRewardWidget.SetRewards(array, _RewardData, OnRewardItemCreated);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mMysteryBoxBtn)
		{
			OpenMysteryBoxUI();
		}
		else if (inWidget == mBtnAds && AdManager.pInstance.AdAvailable(_AdEventType, AdType.REWARDED_VIDEO))
		{
			if (mUiWorldEventEndDB != null)
			{
				mUiWorldEventEndDB.SetInteractive(interactive: false);
			}
			AdManager.DisplayAd(_AdEventType, AdType.REWARDED_VIDEO, base.gameObject);
		}
	}

	public void OnAdWatched()
	{
		mBtnAds.SetVisibility(inVisible: false);
		AdManager.pInstance.LogAdWatchedEvent(_AdEventType, "DoubleRewards");
		KAUICursorManager.SetDefaultCursor("Loading");
		WsWebService.SetAchievementAndGetReward(pAdRewardAchievementID, "", ServiceEventHandler, null);
	}

	public void OnAdFailed()
	{
		if (mUiWorldEventEndDB != null)
		{
			mUiWorldEventEndDB.SetInteractive(interactive: true);
		}
	}

	public void OnAdSkipped()
	{
		if (mUiWorldEventEndDB != null)
		{
			mUiWorldEventEndDB.SetInteractive(interactive: true);
		}
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
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (mUiWorldEventEndDB != null)
			{
				mUiWorldEventEndDB.SetInteractive(interactive: true);
			}
			if (inObject != null)
			{
				AchievementReward[] array = (AchievementReward[])inObject;
				if (array != null)
				{
					ShowRewards(array, "", isAdRewards: true);
					AdManager.pInstance.SyncAdAvailableCount(_AdEventType, isConsumed: true);
					break;
				}
			}
			AdManager.pInstance.SyncAdAvailableCount(_AdEventType, isConsumed: false);
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _AdRewardFailedText.GetLocalizedString(), null, "");
			break;
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (mUiWorldEventEndDB != null)
			{
				mUiWorldEventEndDB.SetInteractive(interactive: true);
			}
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _AdRewardFailedText.GetLocalizedString(), null, "");
			AdManager.pInstance.SyncAdAvailableCount(_AdEventType, isConsumed: false);
			UtDebug.LogError("Getting AdRewards for World Event Scout Attack failed");
			break;
		}
	}

	private void OpenMysteryBoxUI()
	{
		mUiWorldEventEndDB.SetInteractive(interactive: false);
		string[] array = _MysteryBoxBundlePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MysteryBoxUiHandler, typeof(GameObject));
	}

	private void SlotUseMysteryBoxTicketConfirm()
	{
		SetInteractive(interactive: false);
		mMysteryBoxAvailable = false;
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.RedeemItem(mSelectedUserItemData, _RedeemItemFetchCount, OnMysteryBoxItemPurchaseDone);
	}

	private void PurchaseItem(int inCurrencyType, int itemID, int storeID, PurchaseEventHandler onPurchaseDoneCallback)
	{
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(itemID, 1);
		CommonInventoryData.pInstance.DoPurchase(inCurrencyType, storeID, onPurchaseDoneCallback);
	}

	public void OnMysteryBoxItemPurchaseDone(CommonInventoryResponse response)
	{
		if (response != null && response.Success)
		{
			mLastReceivedItems.Clear();
			if (response.CommonInventoryIDs != null)
			{
				CommonInventoryResponseItem[] commonInventoryIDs = response.CommonInventoryIDs;
				foreach (CommonInventoryResponseItem item in commonInventoryIDs)
				{
					mLastReceivedItems.Add(item);
				}
				string[] array = _MysteryBoxBundlePath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MysteryBoxUiHandler, typeof(GameObject), inDontDestroy: false, response.PrizeItems);
			}
			else
			{
				SetInteractive(interactive: true);
				KAUICursorManager.SetDefaultCursor("Arrow");
				UtDebug.LogWarning("No items awarded by mysteryBox !!!");
			}
		}
		else
		{
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	private void MysteryBoxUiHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			if ((bool)gameObject)
			{
				gameObject.name = "PfUiMysteryBox";
				UiMysteryChestClaim component = gameObject.GetComponent<UiMysteryChestClaim>();
				if ((bool)component)
				{
					component.pMsgObject = base.gameObject;
					if (mSelectedUserItemData != null)
					{
						component.pSelectedUserItemData = mSelectedUserItemData;
					}
				}
			}
			if ((bool)mUiWorldEventEndDB)
			{
				mUiWorldEventEndDB.SetVisibility(Visibility: false);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private void OnMysteryBoxClosed()
	{
		OnClose();
	}

	private void OnClose()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUI.RemoveExclusive(this);
		SetVisibility(inVisible: false);
		UnityEngine.Object.Destroy(base.transform.root.gameObject);
	}

	private void UpdateRewards(AchievementReward[] rewards, bool isAdRewards)
	{
		AchievementReward[] array;
		if (isAdRewards)
		{
			array = rewards;
			foreach (AchievementReward achievementReward in array)
			{
				int value = achievementReward.PointTypeID.Value;
				if ((uint)(value - 1) <= 1u || (uint)(value - 8) <= 2u || value == 12)
				{
					achievementReward.Amount *= 2;
				}
			}
		}
		List<AchievementReward> list = new List<AchievementReward>();
		array = rewards;
		foreach (AchievementReward achievementReward2 in array)
		{
			if (achievementReward2.PointTypeID.Value == 6)
			{
				mSelectedUserItemData = CommonInventoryData.pInstance.FindItem(achievementReward2.ItemID);
				if (mSelectedUserItemData != null && !string.IsNullOrEmpty(mSelectedUserItemData.Item.IconName))
				{
					CoBundleItemData coBundleItemData = new CoBundleItemData(mSelectedUserItemData.Item.IconName, "");
					mMysteryBoxIcon.SetUserData(coBundleItemData);
					coBundleItemData.LoadResource();
					mMysteryBoxAvailable = true;
					if (mItemQtyTxt != null)
					{
						mItemQtyTxt.SetText(achievementReward2.Amount.ToString());
					}
				}
			}
			else
			{
				list.Add(achievementReward2);
			}
		}
		if (mSelectedUserItemData != null && mMysteryBoxBtn != null)
		{
			mMysteryBoxBtn.SetVisibility(mSelectedUserItemData.Item.HasCategory(462));
		}
		if (mMysteryBoxBtn != null)
		{
			mMysteryBoxBtn.SetDisabled(!mMysteryBoxAvailable);
		}
		_XPRewardWidget.SetRewards(list.ToArray(), _RewardData);
	}

	public void ShowRewards(AchievementReward[] rewards, string rewardTier, bool isAdRewards = false, bool inEventWon = false)
	{
		GameUtilities.AddRewards(rewards, inUseRewardManager: false);
		UpdateRewards(rewards, isAdRewards);
		if (!inEventWon)
		{
			if (mDefeatTxt != null)
			{
				mDefeatTxt.SetText(rewardTier);
			}
			if ((bool)_TierRewardSprite)
			{
				_TierRewardSprite.mainTexture = null;
			}
		}
		else if (mTierTxt != null && !string.IsNullOrEmpty(rewardTier))
		{
			mTierTxt.SetText(rewardTier);
		}
		if (!isAdRewards && mBtnAds != null && AdManager.pInstance.AdSupported(_AdEventType, AdType.REWARDED_VIDEO))
		{
			mBtnAds.SetVisibility(inVisible: true);
		}
	}

	public void OnScoreTabChanged(UiWorldEventScoresBox.ScoreTab inTabType)
	{
		bool flag = inTabType == UiWorldEventScoresBox.ScoreTab.ClanLeaderboard;
		if (mClanRewards != null)
		{
			mClanRewards.SetVisibility(flag);
		}
		if (mEventRewards != null)
		{
			mEventRewards.SetVisibility(!flag);
		}
	}

	public void OnRewardItemCreated(KAWidget inRewardParentWidget, AchievementReward inAchievementReward)
	{
		if (_ClanRewards == null)
		{
			return;
		}
		RewardData rewardData = Array.Find(_ClanRewards, (RewardData t) => t.PointTypeID == inAchievementReward.PointTypeID.Value);
		if (rewardData != null)
		{
			KAWidget kAWidget = inRewardParentWidget.FindChildItem("TxtItemTitle");
			if (kAWidget != null)
			{
				kAWidget.SetText(rewardData.Title.GetLocalizedString());
			}
		}
	}
}
