using System;
using System.Collections;
using System.Collections.Generic;
using CommonInventory.V4;
using UnityEngine;

namespace SOD.Event;

public class UiLootBox : KAUI
{
	[Serializable]
	public class EffectsData
	{
		public GameObject _FX;

		public float _StartDelay;

		public float _Duration;

		public Vector3 _Offset;

		public void Play(Vector3 position)
		{
			if (_FX != null)
			{
				_FX.SetActive(value: true);
				_FX.transform.position = position + _Offset;
			}
		}

		public void Stop()
		{
			if (_FX != null)
			{
				_FX.SetActive(value: false);
			}
		}
	}

	public EffectsData _FXOpenLootBox;

	public Vector3 _LootBoxOpenWidgetScale = Vector3.one;

	public float _LootBoxOpenWidgetScaleDuration = 0.25f;

	public float _NextLootBoxOpenDelay = 1f;

	public float _LootBoxIdleDuration = 1f;

	public int _LootBoxRedeemLimit = 10;

	public LocaleString _RedeemItemFail = new LocaleString("Failed to open Cookie Jar");

	public Action OnClosed;

	private const string mSortOrder = "SortOrder";

	private KAUIMenu mLootBoxMenu;

	private KAUIMenu mCollectItemMenu;

	private KAUI mUiOpenLootBox;

	private KAUI mUiCollectItem;

	private EffectsData mEffectsData;

	private Coroutine mEffectsCouroutine;

	private int mRedeemBoxCount;

	private Dictionary<int, List<PrizeItem>> mCollectedItems = new Dictionary<int, List<PrizeItem>>();

	private AvAvatarState mLastAvatarState;

	private Animation mAnimation;

	private KAWidget mOpeningBoxWidget;

	private int mBundleCount;

	private const string LootBoxHolder = "LootBox";

	private const string LootOpenAnimation = "In";

	private void PlayFX(EffectsData data, Vector3 position)
	{
		StopFX();
		if (data != null)
		{
			mEffectsData = data;
			mEffectsCouroutine = StartCoroutine(ProcessFX(position));
		}
	}

	private void StopFX()
	{
		if (mEffectsCouroutine != null)
		{
			StopCoroutine(mEffectsCouroutine);
			mEffectsCouroutine = null;
		}
		if (mEffectsData != null)
		{
			mEffectsData.Stop();
			mEffectsData = null;
		}
	}

	private IEnumerator ProcessFX(Vector3 offset)
	{
		if (mEffectsData != null)
		{
			if (mEffectsData._StartDelay > 0f)
			{
				yield return new WaitForSeconds(mEffectsData._StartDelay);
			}
			mEffectsData.Play(offset);
			if (mEffectsData._Duration > 0f)
			{
				yield return new WaitForSeconds(mEffectsData._Duration);
				mEffectsData.Stop();
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
		{
			mLastAvatarState = AvAvatarState.PAUSED;
			AvAvatar.pState = AvAvatarState.PAUSED;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.SetUIActive(inActive: false);
			}
		}
		mLootBoxMenu = _MenuList[0];
		mCollectItemMenu = _MenuList[1];
		mUiOpenLootBox = _UiList[0];
		mUiCollectItem = _UiList[1];
		if (mUiCollectItem != null)
		{
			mUiCollectItem.SetVisibility(inVisible: false);
			mUiCollectItem.pEvents.OnClick += OnClick;
		}
		if (mUiOpenLootBox != null)
		{
			mUiOpenLootBox.SetVisibility(inVisible: true);
			mUiOpenLootBox.pEvents.OnClick += OnClick;
		}
		SetVisibility(inVisible: false);
		DisplayLootBoxes();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnOpenAll")
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			mCollectedItems.Clear();
			mRedeemBoxCount = 0;
			if (mUiOpenLootBox != null)
			{
				mUiOpenLootBox.SetVisibility(inVisible: false);
			}
			List<KAWidget> items = mLootBoxMenu.GetItems();
			List<CommonInventory.V4.RedeemRequest> list = new List<CommonInventory.V4.RedeemRequest>();
			int num = 0;
			if (items == null || items.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < items.Count; i++)
			{
				items[i].gameObject.SetActive(value: false);
				if (items[i].GetUserData() == null)
				{
					continue;
				}
				UserItemData userItemData = CommonInventoryData.pInstance.FindItem(items[i].GetUserData()._Index);
				if (userItemData == null)
				{
					continue;
				}
				int num2;
				for (num2 = userItemData.Quantity / _LootBoxRedeemLimit; num2 > 0; num2--)
				{
					CommonInventory.V4.RedeemRequest[] array = new CommonInventory.V4.RedeemRequest[1]
					{
						new CommonInventory.V4.RedeemRequest()
					};
					array[0].ItemID = userItemData.Item.ItemID;
					array[0].RedeemItemCount = _LootBoxRedeemLimit;
					mRedeemBoxCount++;
					CommonInventoryData.pInstance.RedeemItems(array, OnRedeemDone);
				}
				num2 = userItemData.Quantity % _LootBoxRedeemLimit;
				if (num2 > 0)
				{
					CommonInventory.V4.RedeemRequest redeemRequest = new CommonInventory.V4.RedeemRequest();
					redeemRequest.ItemID = userItemData.Item.ItemID;
					if (num2 <= _LootBoxRedeemLimit - num)
					{
						redeemRequest.RedeemItemCount = num2;
						num += num2;
						list.Add(redeemRequest);
					}
					else
					{
						redeemRequest.RedeemItemCount = _LootBoxRedeemLimit - num;
						num += redeemRequest.RedeemItemCount;
						list.Add(redeemRequest);
						if (num >= _LootBoxRedeemLimit)
						{
							RedeemItemList(list);
							redeemRequest.RedeemItemCount = num2 - redeemRequest.RedeemItemCount;
							num += redeemRequest.RedeemItemCount;
							list.Add(redeemRequest);
						}
					}
				}
				if (num >= _LootBoxRedeemLimit)
				{
					RedeemItemList(list);
					num = 0;
				}
			}
			if (list.Count > 0)
			{
				RedeemItemList(list);
				num = 0;
			}
		}
		else if (inWidget.name == "BtnCollectAll")
		{
			CloseUi();
		}
	}

	private void RedeemItemList(List<CommonInventory.V4.RedeemRequest> requests)
	{
		mRedeemBoxCount++;
		CommonInventoryData.pInstance.RedeemItems(requests.ToArray(), OnRedeemDone);
		requests.Clear();
	}

	private void OnRedeemDone(CommonInventoryGroupResponse response)
	{
		mRedeemBoxCount--;
		if (response != null && response.Success && response.PrizeItems != null && response.PrizeItems.Count > 0)
		{
			foreach (MultiplePrizeItemResponse prizeItem2 in response.PrizeItems)
			{
				if (prizeItem2.MysteryPrizeItems == null || prizeItem2.MysteryPrizeItems.Count <= 0)
				{
					continue;
				}
				if (!mCollectedItems.ContainsKey(prizeItem2.ItemID))
				{
					mCollectedItems[prizeItem2.ItemID] = new List<PrizeItem>();
				}
				foreach (PrizeItem item in prizeItem2.MysteryPrizeItems)
				{
					PrizeItem prizeItem = mCollectedItems[prizeItem2.ItemID].Find((PrizeItem p) => p.Item != null && p.Item.ItemID == item.Item.ItemID);
					if (prizeItem != null)
					{
						prizeItem.ItemQuantity += item.ItemQuantity;
					}
					else
					{
						mCollectedItems[prizeItem2.ItemID].Add(item);
					}
				}
			}
		}
		if (mRedeemBoxCount > 0)
		{
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mCollectedItems.Count == 0)
		{
			SetVisibility(inVisible: false);
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _RedeemItemFail.GetLocalizedString(), base.gameObject, "OnCloseGenericDB");
			return;
		}
		if (mUiCollectItem != null)
		{
			mUiCollectItem.SetVisibility(inVisible: true);
		}
		ShowNextBox();
	}

	private void OnCloseGenericDB()
	{
		CloseUi();
	}

	private void DisplayLootBoxes()
	{
		mLootBoxMenu.ClearItems();
		if (SnoggletogManager.pInstance != null && CommonInventoryData.pIsReady)
		{
			UserItemData[] items = CommonInventoryData.pInstance.GetItems(SnoggletogManager.pInstance._MysteryBoxCategoryID);
			if (items != null)
			{
				Array.Sort(items, (UserItemData x, UserItemData y) => x.Item.GetAttribute("SortOrder", 1).CompareTo(y.Item.GetAttribute("SortOrder", 1)));
				UserItemData[] array = items;
				foreach (UserItemData userItemData in array)
				{
					ShowItem(mLootBoxMenu, "LootBox_", userItemData.Item, userItemData.Quantity);
				}
				if (mBundleCount <= 0)
				{
					SetVisibility(inVisible: true);
				}
				else
				{
					KAUICursorManager.SetDefaultCursor("Loading");
				}
				return;
			}
		}
		CloseUi();
	}

	private void ShowItem(KAUIMenu menu, string itemName, ItemData itemData, int quantity)
	{
		KAWidget kAWidget = menu.GetItems().Find((KAWidget w) => w.GetUserData()._Index == itemData.ItemID);
		if (kAWidget == null)
		{
			kAWidget = menu.AddWidget(itemName + itemData.ItemID, new KAWidgetUserData(itemData.ItemID));
		}
		if (kAWidget.transform.Find("LootBox") != null && !string.IsNullOrEmpty(itemData.AssetName))
		{
			mBundleCount++;
			RsResourceManager.LoadAssetFromBundle(itemData.AssetName, OnLootBoxLoaded, typeof(GameObject), inDontDestroy: false, kAWidget);
		}
		else
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem("BkgIcon");
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: true);
				kAWidget2.SetTexture(null);
				kAWidget2.SetTextureFromBundle(itemData.IconName);
			}
		}
		KAWidget kAWidget3 = kAWidget.FindChildItem("TxtCount");
		if (kAWidget3 != null)
		{
			int.TryParse(kAWidget3.GetText(), out var result);
			kAWidget3.SetText((result + quantity).ToString());
		}
	}

	private void OnLootBoxLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE && inEvent != RsResourceLoadEvent.ERROR)
		{
			return;
		}
		mBundleCount--;
		if (inObject != null)
		{
			SetUpLootBox((KAWidget)inUserData, inObject);
		}
		if (mBundleCount > 0)
		{
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		foreach (KAWidget item in mLootBoxMenu.GetItems())
		{
			item.gameObject.SetActive(value: true);
		}
		SetVisibility(inVisible: true);
	}

	private void SetUpLootBox(KAWidget widget, object asset)
	{
		Transform transform = widget.transform.Find("LootBox");
		if (transform != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)asset);
			obj.transform.parent = transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.layer = transform.gameObject.layer;
			obj.transform.SetChildLayer(transform.gameObject.layer);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!(mOpeningBoxWidget != null) || mOpeningBoxWidget.GetUserData() == null || !(mAnimation != null) || mAnimation.IsPlaying("In"))
		{
			return;
		}
		if (mCollectedItems.ContainsKey(mOpeningBoxWidget.GetUserData()._Index))
		{
			foreach (PrizeItem item in mCollectedItems[mOpeningBoxWidget.GetUserData()._Index])
			{
				ShowItem(mCollectItemMenu, "CollectedItem_", item.Item, item.ItemQuantity);
			}
		}
		mAnimation = null;
		Invoke("ShowNextBox", _NextLootBoxOpenDelay);
	}

	private void ShowNextBox()
	{
		StopFX();
		if (mOpeningBoxWidget != null)
		{
			UnityEngine.Object.Destroy(mOpeningBoxWidget.gameObject);
			mOpeningBoxWidget = null;
		}
		if (mLootBoxMenu != null && mLootBoxMenu.GetItems() != null && mLootBoxMenu.GetItems().Count > 0)
		{
			mOpeningBoxWidget = mLootBoxMenu.GetItems()[0];
			mLootBoxMenu.GetItems().Remove(mOpeningBoxWidget);
			InitOpenLootBox(mOpeningBoxWidget);
		}
		else if (mUiCollectItem != null)
		{
			KAWidget kAWidget = mUiCollectItem.FindItem("BtnCollectAll");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: true);
			}
			mCollectedItems.Clear();
		}
	}

	private void InitOpenLootBox(KAWidget widget)
	{
		widget.gameObject.SetActive(value: true);
		widget.SetVisibility(inVisible: false);
		if (mUiOpenLootBox != null)
		{
			KAWidget kAWidget = mUiOpenLootBox.FindItem("BoxOpenMarker");
			if (kAWidget != null)
			{
				widget.transform.position = kAWidget.transform.position;
			}
		}
		widget.pUI = this;
		widget.ScaleTo(Vector3.zero, _LootBoxOpenWidgetScale, _LootBoxOpenWidgetScaleDuration);
	}

	public override void EndScaleTo(KAWidget widget)
	{
		base.EndScaleTo(widget);
		if (mOpeningBoxWidget == widget)
		{
			Invoke("OpenLootBox", _LootBoxIdleDuration);
		}
	}

	private void OpenLootBox()
	{
		if (mOpeningBoxWidget != null)
		{
			PlayFX(_FXOpenLootBox, mOpeningBoxWidget.transform.position);
			PlayLootBoxAnim(mOpeningBoxWidget, "In");
		}
	}

	private void PlayLootBoxAnim(KAWidget widget, string anim)
	{
		Transform transform = widget.transform.Find("LootBox");
		if (transform != null)
		{
			mAnimation = transform.GetComponentInChildren<Animation>();
			if (mAnimation != null && !mAnimation.IsPlaying(anim))
			{
				mAnimation.Play(anim);
			}
		}
	}

	private void CloseUi()
	{
		KAUI.RemoveExclusive(this);
		if (mUiCollectItem != null)
		{
			mUiCollectItem.pEvents.OnClick -= OnClick;
		}
		if (mUiOpenLootBox != null)
		{
			mUiOpenLootBox.pEvents.OnClick -= OnClick;
		}
		if (mLastAvatarState == AvAvatarState.PAUSED)
		{
			AvAvatar.pState = AvAvatar.pPrevState;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.SetUIActive(inActive: true);
			}
		}
		OnClosed?.Invoke();
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
