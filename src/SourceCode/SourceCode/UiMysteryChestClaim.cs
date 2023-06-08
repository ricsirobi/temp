using System;
using System.Collections;
using UnityEngine;

public class UiMysteryChestClaim : KAUI
{
	public int _RedeemItemFetchCount = 1;

	private UserItemData mSelectedUserItemData;

	public KAWidget _TemplateMysteryChest;

	public KAScrollBar _KAScrollBar;

	public GameObject _MysteryBoxAnimation;

	private string mRedeemedMysteryBoxAssetName;

	private GameObject mLastMysteryBoxAsset;

	public KAWidget _MysteryReward;

	public KAWidget _TxtNoChestMessage;

	public KAWidget _StoreBtn;

	public KAWidget _TxtRarityLabel;

	public KAWidget _RareBanner;

	public KAWidget _LegendaryBanner;

	private KAWidget mBkgIcon;

	private KAWidget mTxtOwned;

	private KAWidget mTxtQuantity;

	private KAWidget mBattleReadyIcon;

	private KAWidget mFlightReadyIcon;

	public ParticleSystem _DustParticles;

	private GameObject mMsgObject;

	private bool mUiToolbarActive = true;

	private AvAvatarState mCachedAvatarState;

	public UserItemData pSelectedUserItemData
	{
		get
		{
			return mSelectedUserItemData;
		}
		set
		{
			mSelectedUserItemData = value;
		}
	}

	public GameObject pMsgObject
	{
		get
		{
			return mMsgObject;
		}
		set
		{
			mMsgObject = value;
		}
	}

	protected override void Start()
	{
		KAUI.SetExclusive(this);
		mUiToolbarActive = AvAvatar.GetUIActive();
		AvAvatar.SetUIActive(inActive: false);
		mCachedAvatarState = AvAvatar.pState;
		AvAvatar.pState = AvAvatarState.PAUSED;
		BuildInventory();
		if ((bool)_MysteryReward)
		{
			_MysteryReward.gameObject.SetActive(value: false);
		}
		else
		{
			UtDebug.LogWarning("_MysteryReward not set!");
		}
		if (!_TxtNoChestMessage)
		{
			UtDebug.LogWarning("_TxtNoChesMessage not set!");
		}
		if (!_StoreBtn)
		{
			UtDebug.LogWarning("_StoreBtn not set!");
		}
		if (!_TxtRarityLabel)
		{
			UtDebug.LogWarning("_TxtRarityBanner not set!");
		}
		_TemplateMysteryChest.gameObject.SetActive(value: false);
	}

	private void RefreshInventory()
	{
		int quantity = mSelectedUserItemData.Quantity;
		KAWidget kAWidget = _MenuList[0].FindItem(mSelectedUserItemData.Item.ItemID.ToString());
		_MenuList[0].FindItemIndex(kAWidget);
		if (quantity == 0)
		{
			_MenuList[0].RemoveWidget(kAWidget);
		}
		else
		{
			kAWidget.FindChildItem("TxtQuantity").SetText(quantity.ToString());
		}
	}

	private void BuildInventory()
	{
		_MenuList[0].ClearItems();
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(462);
		if (items != null && items.Length != 0)
		{
			_TxtNoChestMessage.SetVisibility(inVisible: false);
			int sentItemPos = -1;
			UserItemData[] array = items;
			foreach (UserItemData userItemData in array)
			{
				if (userItemData.Quantity > 0)
				{
					KAWidget kAWidget = DuplicateWidget(_TemplateMysteryChest);
					kAWidget.gameObject.name = userItemData.Item.ItemID.ToString();
					kAWidget.SetUserDataInt(userItemData.Item.ItemID);
					kAWidget.FindChildItem("TxtQuantity").SetText(userItemData.Quantity.ToString());
					kAWidget.FindChildItem("TxtMysteryBoxName").SetText(userItemData.Item.ItemName);
					kAWidget.FindChildItem("ItemIcon").SetTextureFromBundle(userItemData.Item.IconName);
					_MenuList[0].AddWidget(kAWidget);
					if (mSelectedUserItemData != null && userItemData == mSelectedUserItemData)
					{
						sentItemPos = _MenuList[0].GetItemCount();
					}
				}
			}
			StartCoroutine(UpdateScrollPosition(sentItemPos));
		}
		else
		{
			_TxtNoChestMessage.SetVisibility(inVisible: true);
			_StoreBtn.SetVisibility(inVisible: true);
		}
	}

	private IEnumerator UpdateScrollPosition(int sentItemPos)
	{
		yield return new WaitForEndOfFrame();
		int itemCount = _MenuList[0].GetItemCount();
		if (itemCount <= 3)
		{
			_KAScrollBar.value = 0.5f;
		}
		else if (sentItemPos <= 2 || sentItemPos >= itemCount - 1 || itemCount == 4)
		{
			_KAScrollBar.value = ((sentItemPos > 2) ? 1 : 0);
		}
		else
		{
			_KAScrollBar.value = 1f / (float)(itemCount - 3) * (float)(sentItemPos - 2);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		switch (inWidget.name)
		{
		case "OpenBtn":
			if ((bool)_MysteryBoxAnimation)
			{
				_MysteryBoxAnimation.SetActive(value: false);
			}
			UseMysteryBoxTicket(inWidget.GetParentItem().GetUserDataInt());
			break;
		case "CloseBtn":
			Exit();
			break;
		case "StoreBtn":
			if (KAUIStore.pInstance != null)
			{
				KAUI.RemoveExclusive(this);
				mMsgObject.SendMessage("OnMysteryBoxClosed", true, SendMessageOptions.DontRequireReceiver);
				UnityEngine.Object.Destroy(base.transform.root.gameObject);
			}
			else
			{
				Exit(closeAll: true);
				StoreLoader.Load(setDefaultMenuItem: true, "Item Bundles", "Bundles", null);
			}
			break;
		}
	}

	private void UseMysteryBoxTicket(int itemID)
	{
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		mSelectedUserItemData = CommonInventoryData.pInstance.FindItem(itemID);
		CommonInventoryData.pInstance.RedeemMysteryBoxItems(mSelectedUserItemData, _RedeemItemFetchCount, OnMysteryBoxItemPurchaseDone);
	}

	public void OnMysteryBoxItemPurchaseDone(CommonInventoryResponse response)
	{
		if (response != null && response.Success)
		{
			_MysteryReward.gameObject.SetActive(value: true);
			ItemData itemData = response.PrizeItems[0].MysteryPrizeItems.Find((ItemData p) => p.ItemID == response.PrizeItems[0].PrizeItemID);
			UtDebug.Log("Received " + itemData.ItemName);
			_MysteryReward.SetText(itemData.ItemName);
			_TxtRarityLabel.SetText((itemData.ItemRarity != ItemRarity.Common) ? itemData.ItemRarity.ToString() : "");
			if (!mBkgIcon)
			{
				mBkgIcon = _MysteryReward.FindChildItem("BkgIcon");
			}
			mBkgIcon.SetTextureFromBundle(itemData.IconName);
			mBkgIcon.SetVisibility(inVisible: false);
			UpdateParticleColors(InventorySetting.pInstance.GetItemRarityColor(ItemRarity.NonBattleCommon));
			_TxtRarityLabel.SetVisibility(inVisible: false);
			_RareBanner.SetVisibility(inVisible: false);
			_LegendaryBanner.SetVisibility(inVisible: false);
			if (!mBattleReadyIcon)
			{
				mBattleReadyIcon = _MysteryReward.FindChildItem("BattleReadyIcon");
			}
			if ((bool)mBattleReadyIcon)
			{
				mBattleReadyIcon.SetVisibility(inVisible: false);
			}
			if (itemData.ItemRarity.HasValue)
			{
				UpdateParticleColors(InventorySetting.pInstance.GetItemRarityColor(itemData.ItemRarity.Value));
				if (itemData.ItemRarity != ItemRarity.Common)
				{
					Color itemRarityColor = InventorySetting.pInstance.GetItemRarityColor(itemData.ItemRarity.Value);
					itemRarityColor = new Color(itemRarityColor.r, itemRarityColor.g, itemRarityColor.b, 1f);
					_TxtRarityLabel.ColorBlendTo(Color.clear, itemRarityColor, 0f);
					_TxtRarityLabel.SetText(itemData.ItemRarity.ToString() + "!");
					_TxtRarityLabel.SetVisibility(inVisible: true);
					if (itemData.ItemRarity == ItemRarity.Rare || itemData.ItemRarity == ItemRarity.Epic)
					{
						_RareBanner.SetVisibility(inVisible: true);
						_RareBanner.ColorBlendTo(Color.clear, itemRarityColor, 0f);
					}
					else if (itemData.ItemRarity == ItemRarity.Legendary)
					{
						_LegendaryBanner.SetVisibility(inVisible: true);
						_LegendaryBanner.ColorBlendTo(Color.clear, itemRarityColor, 0f);
					}
				}
				if ((bool)mBattleReadyIcon)
				{
					mBattleReadyIcon.SetVisibility(itemData.IsStatAvailable());
				}
			}
			if (!mFlightReadyIcon)
			{
				mFlightReadyIcon = _MysteryReward.FindChildItem("FlightReadyIcon");
			}
			if ((bool)mFlightReadyIcon)
			{
				mFlightReadyIcon.SetVisibility(itemData.HasAttribute("FlightSuit"));
			}
			if (!mTxtOwned)
			{
				mTxtOwned = _MysteryReward.FindChildItem("TxtOwned");
			}
			mTxtOwned.SetVisibility(inVisible: false);
			if (!mTxtQuantity)
			{
				mTxtQuantity = _MysteryReward.FindChildItem("TxtQuantity");
			}
			mTxtQuantity.SetVisibility(inVisible: false);
			if (response.CommonInventoryIDs[0].Quantity > 1)
			{
				mTxtQuantity.SetVisibility(inVisible: true);
				mTxtQuantity.SetText(response.CommonInventoryIDs[0].Quantity.ToString());
			}
			_MysteryReward.gameObject.SetActive(value: false);
			RefreshInventory();
			mRedeemedMysteryBoxAssetName = mSelectedUserItemData.Item.AssetName;
			RsResourceManager.LoadAssetFromBundle(mRedeemedMysteryBoxAssetName, ChestLoadedCallback, null);
		}
		else
		{
			Money.pUpdateToolbar = true;
			UtDebug.LogError("Redeem failed!");
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	private void UpdateParticleColors(Color color)
	{
		ParticleSystem[] componentsInChildren = _MysteryBoxAnimation.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			try
			{
				if ((!particleSystem || !(particleSystem == _DustParticles)) && !(particleSystem.transform.parent == _DustParticles.gameObject))
				{
					ParticleSystem.MainModule main = particleSystem.main;
					main.startColor = color;
				}
			}
			catch (Exception message)
			{
				UtDebug.Log(message);
			}
		}
	}

	private void ChestLoadedCallback(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Chest load failed!");
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
			if ((bool)_MysteryBoxAnimation)
			{
				StartMysteryChestAnimation();
			}
			break;
		}
	}

	private void StartMysteryChestAnimation()
	{
		if (mLastMysteryBoxAsset != null)
		{
			UnityEngine.Object.Destroy(mLastMysteryBoxAsset.gameObject);
		}
		GameObject gameObject = null;
		try
		{
			gameObject = (mLastMysteryBoxAsset = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromBundle(mRedeemedMysteryBoxAssetName)));
		}
		catch (Exception message)
		{
			UtDebug.LogError(message);
			MeshRenderer[] componentsInChildren = _MysteryBoxAnimation.transform.GetChild(0).GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
		if (gameObject != null && (bool)_MysteryBoxAnimation)
		{
			MeshRenderer[] componentsInChildren = _MysteryBoxAnimation.transform.GetChild(0).GetComponentsInChildren<MeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			gameObject.transform.SetParent(_MysteryBoxAnimation.transform.GetChild(0));
			gameObject.transform.localEulerAngles = Vector3.zero;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.layer = LayerMask.NameToLayer("2DNGUI");
			for (int j = 0; j < gameObject.transform.childCount; j++)
			{
				gameObject.transform.GetChild(j).gameObject.layer = LayerMask.NameToLayer("2DNGUI");
			}
		}
		_MysteryBoxAnimation.SetActive(value: true);
	}

	public void RevealItemsAndRarity()
	{
		_MysteryReward.gameObject.SetActive(value: true);
		_MysteryReward.FindChildItem("BkgIcon").SetVisibility(inVisible: true);
		if (_MenuList[0].GetItemCount() == 0)
		{
			_TxtNoChestMessage.SetVisibility(!KAUIStore.pInstance);
			_StoreBtn.SetVisibility(inVisible: true);
		}
	}

	public void OnMysteryChestAnimationComplete()
	{
	}

	private void Exit(bool closeAll = false)
	{
		AvAvatar.SetUIActive(mUiToolbarActive);
		AvAvatar.pState = mCachedAvatarState;
		KAUI.RemoveExclusive(this);
		UnityEngine.Object.Destroy(base.transform.root.gameObject);
		if ((bool)mMsgObject)
		{
			mMsgObject.SetActive(value: true);
			mMsgObject.SendMessage("OnMysteryBoxClosed", closeAll, SendMessageOptions.DontRequireReceiver);
		}
	}
}
