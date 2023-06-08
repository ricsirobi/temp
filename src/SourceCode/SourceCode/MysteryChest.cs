using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class MysteryChest : KAMonoBase, IAdResult
{
	public enum ChestState
	{
		OPENED,
		CLOSED
	}

	public GameObject _ClosedFx;

	public GameObject _OpenFx;

	public Action<MysteryChest> OnChestOpened;

	private MysteryChestCSM mCsm;

	private int mMysteryBoxId = -1;

	private int mStoreId = -1;

	private MysteryChestManager mManager;

	private GameObject mClosedParticleFx;

	private bool mDestroyWhenFinished;

	private ItemData mItemData;

	private ChestState mCurrentState;

	private ChestType mMysteryBoxType;

	public ItemData pItemData => mItemData;

	public ChestState pCurrentState => mCurrentState;

	public ChestType pMysteryBoxType => mMysteryBoxType;

	public void Init(MysteryChestManager manager, MysteryBoxStoreInfo storeInfo, ChestType chestType, Action<MysteryChest> chestOpenedCallback = null, bool destroyWhenFinished = false)
	{
		mManager = manager;
		mMysteryBoxId = storeInfo._ItemID;
		mStoreId = storeInfo._StoreID;
		mMysteryBoxType = chestType;
		mCsm = GetComponent<MysteryChestCSM>();
		ItemStoreDataLoader.Load(mStoreId, OnStoreLoaded);
		mCurrentState = ChestState.CLOSED;
		if (_ClosedFx != null)
		{
			mClosedParticleFx = UnityEngine.Object.Instantiate(_ClosedFx, base.transform.position, base.transform.rotation, base.transform);
			if (UtPlatform.IsWSA())
			{
				UtUtilities.ReAssignShader(mClosedParticleFx);
			}
		}
		OnChestOpened = chestOpenedCallback;
		mDestroyWhenFinished = destroyWhenFinished;
	}

	private void OnStoreLoaded(StoreData sd)
	{
		if (sd != null)
		{
			mItemData = sd.FindItem(mMysteryBoxId);
			mCsm._MysteryChest = this;
		}
	}

	public int[] GetContents()
	{
		return mItemData.GetPrizesSorted().ToArray();
	}

	public void Purchase()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		switch (mItemData.GetPurchaseType())
		{
		case 1:
			if (mItemData.GetFinalCost() > Money.pGameCurrency)
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", mManager._NotEnoughGoldText.GetLocalizedString(), base.gameObject, "OnDBClose");
				return;
			}
			break;
		case 2:
			if (mItemData.GetFinalCost() > Money.pCashCurrency)
			{
				GameUtilities.DisplayGenericDB("PfKAUIGenericDB", mManager._NotEnoughGemsText.GetLocalizedString(), null, base.gameObject, "OnPurchaseGems", "OnDBClose", null, null, inDestroyOnClick: true);
				return;
			}
			break;
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(mMysteryBoxId, 1, ItemPurchaseSource.MYSTERY_CHEST.ToString());
		CommonInventoryData.pInstance.DoPurchase(mItemData.GetPurchaseType(), mStoreId, OnMysteryChestPurchaseDone);
	}

	private void OnDBClose()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
	}

	private void OnPurchaseGems()
	{
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void OnIAPStoreClosed()
	{
		OnDBClose();
	}

	public void OnMysteryChestPurchaseDone(CommonInventoryResponse response)
	{
		if (response != null && response.Success)
		{
			ItemData.Load(response.CommonInventoryIDs[0].ItemID, OnItemLoaded, response);
		}
		else
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", mManager._PurchaseFailed.GetLocalizedString(), base.gameObject, "OnDBClose");
		}
	}

	public void OnAdPurchaseMysteryChestDone(CommonInventoryResponse response)
	{
		if (response != null && response.Success)
		{
			AdManager.pInstance.SyncAdAvailableCount(AdEventType.WORLD_MYSTERY_CHEST, isConsumed: true);
			ItemData.Load(response.CommonInventoryIDs[0].ItemID, OnItemLoaded, response);
		}
		else
		{
			AdManager.pInstance.SyncAdAvailableCount(AdEventType.WORLD_MYSTERY_CHEST, isConsumed: false);
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", AdManager.pInstance._AdRewardFailedText.GetLocalizedString(), base.gameObject, "OnDBClose");
		}
	}

	private void OnItemLoaded(int itemID, ItemData dataItem, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		PlayOpenAnimation();
		mCurrentState = ChestState.OPENED;
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		if (dataItem != null)
		{
			CommonInventoryResponse commonInventoryResponse = (CommonInventoryResponse)inUserData;
			string text3;
			if (Regex.IsMatch(dataItem.ItemName, "^\\d"))
			{
				string text = dataItem.ItemName;
				if (commonInventoryResponse.CommonInventoryIDs.Length > 1)
				{
					string text2 = Regex.Match(text, "^\\d+").ToString();
					text = int.Parse(text2) * commonInventoryResponse.CommonInventoryIDs.Length + dataItem.ItemName.Substring(text2.Length);
				}
				text3 = string.Format(mManager._RewardText.GetLocalizedString(), "", text);
			}
			else
			{
				text3 = string.Format(mManager._RewardText.GetLocalizedString(), commonInventoryResponse.CommonInventoryIDs.Length, dataItem.ItemName);
			}
			GameObject obj = UnityEngine.Object.Instantiate(mManager._RewardTextTemplate, base.transform.position, base.transform.rotation);
			obj.SetActive(value: true);
			obj.GetComponentInChildren<TextMesh>().text = text3;
			OnChestOpened?.Invoke(this);
			OnChestOpened = null;
			if (!string.IsNullOrEmpty(dataItem.AssetName))
			{
				string[] array = dataItem.AssetName.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnRewardAssetLoaded, typeof(GameObject));
			}
			else if (mDestroyWhenFinished)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		AnalyticMysteryChestEvent.LogPurchasedEvent(RsResourceManager.pCurrentLevel, base.gameObject.name, mItemData.ItemID);
	}

	private void PlayOpenAnimation()
	{
		UnityEngine.Object.Destroy(mClosedParticleFx);
		GetComponentInChildren<Animation>().Play();
		if (!(_OpenFx == null))
		{
			GameObject obj = UnityEngine.Object.Instantiate(_OpenFx, base.transform.position, base.transform.rotation, base.transform);
			if (UtPlatform.IsWSA())
			{
				UtUtilities.ReAssignShader(obj);
			}
		}
	}

	private void OnRewardAssetLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE)
		{
			_ = 3;
			return;
		}
		if (inObject != null)
		{
			GameObject gameObject = (GameObject)inObject;
			MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>();
			if (componentsInChildren.Length != 0)
			{
				MonoBehaviour[] array = componentsInChildren;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = false;
				}
			}
			ObMysteryChestRewardEmitter component = GetComponent<ObMysteryChestRewardEmitter>();
			if (component != null)
			{
				component._RewardItem = gameObject;
				component._Coin = mManager._RewardTemplate;
				component._CoinsToEmit._Min = (component._CoinsToEmit._Max = 1);
				component.GenerateCoins();
			}
		}
		if (mDestroyWhenFinished)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void ShowMysteryChestAd()
	{
		if (AdManager.pInstance.AdAvailable(AdEventType.WORLD_MYSTERY_CHEST, AdType.REWARDED_VIDEO))
		{
			AdManager.DisplayAd(AdEventType.WORLD_MYSTERY_CHEST, AdType.REWARDED_VIDEO, base.gameObject);
		}
	}

	public void OnAdWatched()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(mMysteryBoxId, 1);
		CommonInventoryData.pInstance.DoPurchase(mItemData.GetPurchaseType(), mStoreId, OnAdPurchaseMysteryChestDone);
	}

	public void OnAdFailed()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public void OnAdSkipped()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		KAUICursorManager.SetDefaultCursor("Arrow");
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
}
