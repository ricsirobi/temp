using System.Collections.Generic;
using UnityEngine;

public class UiMysteryBoxMenu : KAUIMenu
{
	public Vector3 _CardShufflePosition = Vector3.zero;

	public UiMysteryBox _MysteryBoxUI;

	public int _MaxShowingPrizes = 10;

	private List<ItemData> mPrizeListItems;

	private int mRarestItemID;

	private bool mCreatePrizeWidgets;

	private bool mIsAnimating;

	private int mMenuItemsProcessDone;

	private int mItemsTobeLoaded;

	public bool pIsAnimating
	{
		get
		{
			return mIsAnimating;
		}
		set
		{
			mMenuItemsProcessDone = 0;
			mIsAnimating = value;
		}
	}

	public void RearrangeForChoose()
	{
		mMenuItemsProcessDone = 0;
		foreach (KAWidget item in GetItems())
		{
			item.GetComponent<UiMysteryBoxAnimItem>().MoveToOriginalPos(_MysteryBoxUI._AnimTimes._PrizeBackToOrgPos);
		}
	}

	public void ReachedOrgPosition(UiMysteryBoxAnimItem item)
	{
		mMenuItemsProcessDone++;
		if (mMenuItemsProcessDone >= mItemsTobeLoaded)
		{
			pIsAnimating = false;
		}
	}

	public void MovePrizesToCenter()
	{
		List<KAWidget> items = GetItems();
		mMenuItemsProcessDone = 0;
		int num = 0;
		foreach (KAWidget item in items)
		{
			Vector2 localPosition = item.GetLocalPosition(new Vector2(_CardShufflePosition.x, _CardShufflePosition.y));
			item.GetComponent<UiMysteryBoxAnimItem>().CacheOriginalPos();
			ShowPrizeIcon(item, show: false);
			Vector3 localPosition2 = item.gameObject.transform.localPosition;
			item.gameObject.transform.localPosition = new Vector3(localPosition2.x, localPosition2.y, localPosition2.z - (float)num);
			item.MoveTo(localPosition, _MysteryBoxUI._AnimTimes._PrizeMoveToCenter);
			item.OnMoveToDone += CardReachedCenter;
			num += 2;
		}
	}

	public void CardReachedCenter(Object widget)
	{
		mMenuItemsProcessDone++;
		if (mMenuItemsProcessDone >= mItemsTobeLoaded)
		{
			pIsAnimating = false;
		}
	}

	public void ShuffleAnimate()
	{
		List<KAWidget> items = GetItems();
		mMenuItemsProcessDone = 0;
		foreach (KAWidget item in items)
		{
			item.GetComponent<UiMysteryBoxAnimItem>().ShuffleCard(_MysteryBoxUI._AnimTimes._PrizeShuffle);
		}
	}

	public void ShuffleCardDone(UiMysteryBoxAnimItem item)
	{
		mMenuItemsProcessDone++;
		if (mMenuItemsProcessDone >= mItemsTobeLoaded)
		{
			pIsAnimating = false;
		}
	}

	private void ShowPrizeIcon(KAWidget item, bool show)
	{
		KAWidget kAWidget = item.FindChildItem("CardBack");
		KAWidget kAWidget2 = item.FindChildItem("CardFront");
		if ((!show || !kAWidget.GetVisibility()) && (show || !kAWidget2.GetVisibility()))
		{
			TweenScale.Begin(item.gameObject, _MysteryBoxUI._AnimTimes._PrizeRotate, new Vector3(0f, 1f, 1f));
			UITweener component = item.gameObject.GetComponent<UITweener>();
			component.eventReceiver = item.gameObject;
			if (!show)
			{
				component.callWhenFinished = "RotatedToFrontHalf";
			}
			else
			{
				component.callWhenFinished = "RotatedToBackHalf";
			}
		}
	}

	public void PopulatePrizes(int itemID, ItemData dataItem, object inUserData)
	{
		mCreatePrizeWidgets = false;
		mMenuItemsProcessDone = 0;
		ClearItems();
		if (inUserData == null)
		{
			OnLoadItemDataReady(itemID, dataItem, inUserData);
			return;
		}
		mPrizeListItems = (List<ItemData>)inUserData;
		mItemsTobeLoaded = mPrizeListItems.Count;
		UtUtilities.Shuffle(mPrizeListItems);
		if (mCreatePrizeWidgets)
		{
			return;
		}
		mCreatePrizeWidgets = true;
		if (mPrizeListItems.Count == 0)
		{
			return;
		}
		int num = 0;
		foreach (ItemData mPrizeListItem in mPrizeListItems)
		{
			LoadWidget(mPrizeListItem, num);
			num++;
		}
	}

	private void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		List<int> prizes = dataItem.GetPrizes();
		mItemsTobeLoaded = prizes.Count;
		mPrizeListItems = new List<ItemData>();
		mRarestItemID = dataItem.GetRarestPrize();
		if (prizes.Count == 0)
		{
			return;
		}
		foreach (int item in prizes)
		{
			ItemData.Load(item, PrizeItemLoadEventHandler, null);
		}
	}

	private void PrizeItemLoadEventHandler(int itemID, ItemData item, object inUserData)
	{
		mPrizeListItems.Add(item);
		if (mPrizeListItems.Count >= mItemsTobeLoaded)
		{
			UtUtilities.Shuffle(mPrizeListItems);
			if (!mCreatePrizeWidgets)
			{
				mCreatePrizeWidgets = true;
				CreatePrizeWidgets();
			}
		}
	}

	private void CreatePrizeWidgets()
	{
		ClearItems();
		mItemsTobeLoaded = Mathf.Min(mPrizeListItems.Count, _MaxShowingPrizes);
		List<ItemData> list = new List<ItemData>();
		int num = 0;
		if (mItemsTobeLoaded < mPrizeListItems.Count)
		{
			foreach (ItemData mPrizeListItem in mPrizeListItems)
			{
				if ((mPrizeListItem.InventoryMax == -1 || CommonInventoryData.pInstance.GetQuantity(mPrizeListItem.ItemID) < mPrizeListItem.InventoryMax || mPrizeListItem.ItemID == _MysteryBoxUI.pFinalPrizeItemID) && (mPrizeListItem.ItemID == _MysteryBoxUI.pFinalPrizeItemID || mPrizeListItem.ItemID == mRarestItemID))
				{
					list.Add(mPrizeListItem);
					num++;
				}
			}
			foreach (ItemData mPrizeListItem2 in mPrizeListItems)
			{
				if (num >= mItemsTobeLoaded)
				{
					break;
				}
				if ((mPrizeListItem2.InventoryMax == -1 || CommonInventoryData.pInstance.GetQuantity(mPrizeListItem2.ItemID) < mPrizeListItem2.InventoryMax) && mPrizeListItem2.ItemID != _MysteryBoxUI.pFinalPrizeItemID && mPrizeListItem2.ItemID != mRarestItemID)
				{
					int num2 = Random.Range(0, num * 2);
					if (num2 >= list.Count)
					{
						list.Add(mPrizeListItem2);
					}
					else
					{
						list.Insert(num2, mPrizeListItem2);
					}
					num++;
				}
			}
		}
		else
		{
			foreach (ItemData mPrizeListItem3 in mPrizeListItems)
			{
				if (num >= mItemsTobeLoaded)
				{
					break;
				}
				list.Add(mPrizeListItem3);
				num++;
			}
		}
		mItemsTobeLoaded = list.Count;
		if (list.Count == 0)
		{
			return;
		}
		num = 0;
		foreach (ItemData item in list)
		{
			LoadWidget(item, num);
			num++;
		}
	}

	private void LoadWidget(ItemData item, int pos)
	{
		string rVo = "";
		if (item.Rollover != null)
		{
			rVo = item.Rollover.Bundle + "/" + item.Rollover.DialogName;
		}
		KAWidget kAWidget = DuplicateWidget(_Template);
		AddWidgetAt(pos, kAWidget);
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.SetInteractive(isInteractive: false);
		kAWidget.name = item.ItemName;
		kAWidget.FindChildItem("AniIconImage").SetText(item.ItemName);
		KAWidget kAWidget2 = kAWidget.FindChildItem("Icon");
		MysteryBoxPrizeItemData mysteryBoxPrizeItemData = new MysteryBoxPrizeItemData(item.IconName, rVo, item, base.gameObject);
		kAWidget2.SetUserData(mysteryBoxPrizeItemData);
		mysteryBoxPrizeItemData.ShowLoadingItem(inShow: true);
		mysteryBoxPrizeItemData.LoadResource();
	}

	private void ItemReady()
	{
		mMenuItemsProcessDone++;
	}

	public bool AllItemsReady()
	{
		return mMenuItemsProcessDone >= mItemsTobeLoaded;
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (!(item == null) && ((_MysteryBoxUI.pAnimationState == UiMysteryBox.AnimState.UserSelection && item.name == "CardBack") || item.name == "CardFront"))
		{
			KAWidget rootItem = item.GetRootItem().GetRootItem();
			rootItem.SetVisibility(inVisible: false);
			_MysteryBoxUI.PrizeSelected(rootItem);
		}
	}
}
