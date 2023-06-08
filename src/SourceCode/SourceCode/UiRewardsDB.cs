using System;
using System.Collections.Generic;
using UnityEngine;

public class UiRewardsDB : KAUIGenericDB
{
	[Serializable]
	public class CategoryMessageMap
	{
		public List<int> _CategoryIDs;

		public LocaleString _MessageText = new LocaleString("[REVIEW] sample message");
	}

	private class ItemQuantityMap
	{
		public UserItemData _Data;

		public int _Quantity;

		public ItemQuantityMap(UserItemData data, int quantity)
		{
			_Data = data;
			_Quantity = quantity;
		}
	}

	public List<CategoryMessageMap> _CategoryMessages;

	private GameObject mUserNotifyObject;

	private bool mIsNewDragonDB;

	private KAWidget mNextBtn;

	private KAWidget mPrevBtn;

	private KAWidget mImgWidget;

	private KAWidget mTextQuantity;

	private int mRewardItemIndex;

	private List<ItemQuantityMap> mItemQuantityMap;

	public void Show(string Msg, string DragonImageName, GameObject ObjToMessage)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mUserNotifyObject = ObjToMessage;
		SetText(Msg, interactive: true);
		SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		SetVisibility(inVisible: false);
		mIsNewDragonDB = true;
		mImgWidget = FindItem("Img");
		SetExclusive();
		if (mImgWidget != null)
		{
			mImgWidget.SetTextureFromURL(DragonImageName, null, OnTextureLoaded);
		}
	}

	public void Show(List<UserItemData> inRewardsListData, GameObject inObjToMessage, bool groupItems = false)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mItemQuantityMap = new List<ItemQuantityMap>();
		foreach (UserItemData data in inRewardsListData)
		{
			if (!groupItems)
			{
				mItemQuantityMap.Add(new ItemQuantityMap(data, 1));
				continue;
			}
			ItemQuantityMap itemQuantityMap = mItemQuantityMap.Find((ItemQuantityMap x) => x._Data.ItemID == data.ItemID);
			if (itemQuantityMap == null)
			{
				itemQuantityMap = new ItemQuantityMap(data, 1);
				mItemQuantityMap.Add(itemQuantityMap);
			}
			else
			{
				itemQuantityMap._Quantity++;
			}
		}
		mNextBtn = FindItem("NextBtn");
		mPrevBtn = FindItem("PrevBtn");
		mTextQuantity = FindItem("TextQuantity");
		if (inRewardsListData.Count > 1)
		{
			mNextBtn.SetVisibility(inVisible: true);
		}
		mUserNotifyObject = inObjToMessage;
		SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		SetVisibility(inVisible: false);
		mIsNewDragonDB = false;
		mImgWidget = FindItem("Img");
		mRewardItemIndex = -1;
		ShowNextReward(inNext: true);
	}

	private void OnTextureLoaded(KAWidget inWidget, bool success)
	{
		if (AvAvatar.GetUIActive())
		{
			AvAvatar.SetUIActive(inActive: false);
		}
		if (AvAvatar.pState != AvAvatarState.PAUSED)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
		}
		SetVisibility(inVisible: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void UpdateRewardImage()
	{
		SetText(mItemQuantityMap[mRewardItemIndex]._Data.Item.Description, interactive: true);
		if (mImgWidget != null)
		{
			mImgWidget.SetTextureFromBundle(mItemQuantityMap[mRewardItemIndex]._Data.Item.IconName, null, OnTextureLoaded);
		}
	}

	private void OnClickOk()
	{
		if (mIsNewDragonDB)
		{
			mUserNotifyObject.SendMessage("OnCloseNewDragonDialog");
		}
		else
		{
			mUserNotifyObject.SendMessage("OnRewardShown", true);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mNextBtn)
		{
			ShowNextReward(inNext: true);
		}
		else if (item == mPrevBtn)
		{
			ShowNextReward(inNext: false);
		}
	}

	private void ShowNextReward(bool inNext)
	{
		if (inNext)
		{
			mRewardItemIndex++;
		}
		else
		{
			mRewardItemIndex--;
		}
		mNextBtn.SetVisibility(mRewardItemIndex + 1 != mItemQuantityMap.Count);
		mPrevBtn.SetVisibility(mRewardItemIndex != 0);
		SetText(GetRewardItemDescription(), interactive: true);
		if (mImgWidget != null)
		{
			mImgWidget.SetTextureFromBundle(mItemQuantityMap[mRewardItemIndex]._Data.Item.IconName, null, OnTextureLoaded);
		}
		if (mTextQuantity != null)
		{
			if (mItemQuantityMap[mRewardItemIndex]._Quantity > 1)
			{
				mTextQuantity.SetVisibility(inVisible: true);
				mTextQuantity.SetText(mItemQuantityMap[mRewardItemIndex]._Quantity.ToString());
			}
			else
			{
				mTextQuantity.SetVisibility(inVisible: false);
			}
		}
	}

	private string GetRewardItemDescription()
	{
		foreach (CategoryMessageMap categoryMessage in _CategoryMessages)
		{
			foreach (int categoryID in categoryMessage._CategoryIDs)
			{
				if (mItemQuantityMap[mRewardItemIndex]._Data.Item.HasCategory(categoryID))
				{
					return categoryMessage._MessageText.GetLocalizedString();
				}
			}
		}
		return mItemQuantityMap[mRewardItemIndex]._Data.Item.Description;
	}
}
