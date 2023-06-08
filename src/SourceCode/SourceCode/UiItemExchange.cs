using System;
using System.Collections.Generic;
using UnityEngine;

public class UiItemExchange : KAUI
{
	public class ItemExchangeUserData : KAWidgetUserData
	{
		public long _ExchangeGroupID;

		public int _FromItemUserInventoryID;

		public ItemExchange _FromItem;

		public ItemExchange _ToItem;

		public int _Repeat = 1;

		public ItemExchangeUserData(long exchangeGroupId, ItemExchange fromItem, ItemExchange toItem, int repeat = 1)
		{
			_ExchangeGroupID = exchangeGroupId;
			_FromItem = fromItem;
			_ToItem = toItem;
			_Repeat = repeat;
		}
	}

	public LocaleString _ExchangeItemNotOwnedText = new LocaleString("[REVIEW] You do not own this item");

	public LocaleString _ExchangeItemEquippedText = new LocaleString("[REVIEW] Equipped item cannot be exchanged");

	public LocaleString _ExchangeItemDragonEquippedText = new LocaleString("[REVIEW] Equipped item on dragon cannot be exchanged");

	public LocaleString _ExchangeItemFailedText = new LocaleString("[REVIEW] Failed to exchange item");

	public LocaleString _ExchangeItemMissingText = new LocaleString("[REVIEW] You do not have required item in inventory");

	public LocaleString _ExchangeItemInsufficientAmountText = new LocaleString("[REVIEW] Required quantity [COUNT]");

	public LocaleString _ExchangeItemUnavailableText = new LocaleString("[REVIEW] You cannot exchange items now");

	private List<int> mEquippedItems;

	protected ItemExchange[] mItemExchange;

	[HideInInspector]
	public UserNotifyEvent m_UserNotifyEvent;

	public Action OnClosed;

	protected override void Start()
	{
		if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.SetUIActive(inActive: false);
			}
		}
		if (AvatarData.pInstanceInfo != null)
		{
			mEquippedItems = AvatarData.pInstanceInfo.GetPartsInventoryIds();
		}
		SetVisibility(inVisible: false);
		base.Start();
	}

	protected void ProcessItemExchange(ItemExchangeUserData itemExchangeUserData)
	{
		UserItemData userItemData = null;
		if (CommonInventoryData.pIsReady)
		{
			userItemData = CommonInventoryData.pInstance.FindItem(itemExchangeUserData._FromItem.Item.ItemID);
		}
		if (userItemData == null)
		{
			return;
		}
		if (mEquippedItems.Contains(userItemData.UserInventoryID))
		{
			List<UserItemData> list = CommonInventoryData.pInstance.FindItems(userItemData.Item.ItemID);
			UserItemData userItemData2 = null;
			foreach (int id in mEquippedItems)
			{
				userItemData2 = list.Find((UserItemData x) => x.UserInventoryID == id);
				if (userItemData2 != null)
				{
					list.Remove(userItemData2);
					break;
				}
			}
			if (list.Count > 0)
			{
				userItemData = list[0];
			}
		}
		itemExchangeUserData._FromItemUserInventoryID = userItemData.UserInventoryID;
		List<int> list2 = new List<int>();
		list2.Add(userItemData.UserInventoryID);
		ProcessExchangeItemRequest itemRequest = new ProcessExchangeItemRequest
		{
			ExchangeGroupID = itemExchangeUserData._ExchangeGroupID,
			Repeat = itemExchangeUserData._Repeat,
			InventoryIDArr = list2.ToArray()
		};
		SetInteractive(interactive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		WsWebService.ItemExchange(itemRequest, ItemExchangeEvent, itemExchangeUserData);
	}

	protected int GetInventoryItemCount(UserItemData userItemData)
	{
		if (userItemData.Item.Uses == 1 && CommonInventoryData.pIsReady)
		{
			return CommonInventoryData.pInstance.FindItems(userItemData.Item.ItemID).Count;
		}
		return userItemData.Quantity;
	}

	protected bool ExchangeAllowed(ItemExchangeUserData itemExchangeUserData)
	{
		if (itemExchangeUserData == null)
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemNotOwnedText.GetLocalizedString(), null, "");
			return false;
		}
		UserItemData userItemData = null;
		if (CommonInventoryData.pIsReady)
		{
			userItemData = CommonInventoryData.pInstance.FindItem(itemExchangeUserData._FromItem.Item.ItemID);
		}
		if (userItemData == null)
		{
			DisplayExchangeStatus(UseInventoryStatus.UserInventoryItemNotFound);
			return false;
		}
		if (mEquippedItems != null && userItemData.UserInventoryID > 0 && mEquippedItems.Contains(userItemData.UserInventoryID) && GetInventoryItemCount(userItemData) == 1)
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemEquippedText.GetLocalizedString(), null, "");
			return false;
		}
		if (RaisedPetData.pActivePets != null)
		{
			foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
			{
				if (value == null)
				{
					continue;
				}
				RaisedPetData[] array = value;
				foreach (RaisedPetData raisedPetData in array)
				{
					if (raisedPetData == null || raisedPetData.Accessories == null || raisedPetData.Accessories.Length == 0)
					{
						continue;
					}
					RaisedPetAccessory[] accessories = raisedPetData.Accessories;
					foreach (RaisedPetAccessory ac in accessories)
					{
						if (raisedPetData.GetAccessoryItemID(ac) == itemExchangeUserData._FromItem.Item.ItemID)
						{
							GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemDragonEquippedText.GetLocalizedString(), null, "");
							return false;
						}
					}
				}
			}
		}
		if (userItemData.Quantity < itemExchangeUserData._FromItem.Quantity)
		{
			DisplayExchangeStatus(UseInventoryStatus.InsufficientQuantity, itemExchangeUserData._FromItem.Quantity);
			return false;
		}
		return true;
	}

	public virtual void GetExchangeItems()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		WsWebService.GetAllExchangeItems(GetExchangeItemsEventHandler, null);
	}

	public void GetExchangeItemsEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			ArrayOfItemExchange arrayOfItemExchange = (ArrayOfItemExchange)inObject;
			if (arrayOfItemExchange.ItemExchangeArray != null && arrayOfItemExchange.ItemExchangeArray.Length != 0)
			{
				mItemExchange = arrayOfItemExchange.ItemExchangeArray;
				CacheItemData(mItemExchange);
				SetVisibility(inVisible: true);
				ExchangeItemsLoaded();
			}
			else
			{
				Debug.LogError("Empty Dreadfall Exchange Items!");
				CloseUi();
			}
			break;
		}
		case WsServiceEvent.ERROR:
			Debug.LogError("Error Getting Dreadfall Exchange Items!");
			CloseUi();
			break;
		}
	}

	private void CacheItemData(ItemExchange[] exchangeList)
	{
		foreach (ItemExchange itemExchange in exchangeList)
		{
			if (itemExchange.Item != null)
			{
				ItemData.AddToCache(itemExchange.Item);
			}
		}
	}

	private void ItemExchangeEvent(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object userData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
			ItemExchangeUserData itemExchangeUserData = userData as ItemExchangeUserData;
			if (inObject != null)
			{
				ExchangeItemResponse exchangeItemResponse = (ExchangeItemResponse)inObject;
				if (exchangeItemResponse.Status == UseInventoryStatus.Success)
				{
					if (itemExchangeUserData != null && CommonInventoryData.pIsReady)
					{
						CommonInventoryData.pInstance.RemoveItemByUserInventoryID(itemExchangeUserData._FromItem.Item.ItemID, itemExchangeUserData._FromItemUserInventoryID, updateServer: false, itemExchangeUserData._FromItem.Quantity * itemExchangeUserData._Repeat);
						CommonInventoryData.pInstance.AddItem(itemExchangeUserData._ToItem.Item.ItemID, updateServer: false, OnItemDataLoaded, userData, itemExchangeUserData._ToItem.Quantity * itemExchangeUserData._Repeat);
						CommonInventoryData.pInstance.ClearSaveCache();
					}
				}
				else
				{
					ItemExchangeFailed(itemExchangeUserData);
					DisplayExchangeStatus(exchangeItemResponse.Status, itemExchangeUserData._FromItem.Quantity * itemExchangeUserData._Repeat);
				}
			}
			else
			{
				ItemExchangeFailed(itemExchangeUserData);
				DisplayExchangeStatus(UseInventoryStatus.UnknownError);
			}
			break;
		}
		case WsServiceEvent.ERROR:
		{
			ItemExchangeUserData userData2 = userData as ItemExchangeUserData;
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetInteractive(interactive: true);
			ItemExchangeFailed(userData2);
			DisplayExchangeStatus(UseInventoryStatus.UnknownError);
			break;
		}
		}
	}

	private void DisplayExchangeStatus(UseInventoryStatus status, int exchangeQuantity = 0)
	{
		switch (status)
		{
		case UseInventoryStatus.UserInventoryItemNotFound:
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemMissingText.GetLocalizedString(), null, "");
			break;
		case UseInventoryStatus.InsufficientQuantity:
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemInsufficientAmountText.GetLocalizedString().Replace("[COUNT]", exchangeQuantity.ToString()), null, "");
			break;
		default:
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemFailedText.GetLocalizedString(), null, "");
			break;
		}
	}

	private void OnItemDataLoaded(UserItemData itemData, object userData)
	{
		ItemExchangeDone(userData as ItemExchangeUserData);
	}

	protected virtual void ItemExchangeDone(ItemExchangeUserData userData)
	{
	}

	protected virtual void ItemExchangeFailed(ItemExchangeUserData userData)
	{
	}

	protected virtual void ExchangeItemsLoaded()
	{
	}

	protected virtual void CloseUi(bool invokeOnClosed = true)
	{
		if (AvAvatar.pState == AvAvatarState.PAUSED)
		{
			AvAvatar.pState = AvAvatar.pPrevState;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.SetUIActive(inActive: true);
			}
		}
		if (invokeOnClosed)
		{
			OnClosed?.Invoke();
		}
		if ((bool)m_UserNotifyEvent)
		{
			OnClosed = (Action)Delegate.Remove(OnClosed, new Action(m_UserNotifyEvent.MarkUserNotifyDone));
		}
		KAUI.RemoveExclusive(this);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public override void OnClick(KAWidget widget)
	{
		base.OnClick(widget);
		if (widget.name == _BackButtonName)
		{
			CloseUi();
		}
	}
}
