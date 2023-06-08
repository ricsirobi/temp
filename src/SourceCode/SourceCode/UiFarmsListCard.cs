using System;
using System.Collections.Generic;
using UnityEngine;

public class UiFarmsListCard : UiCard
{
	[Serializable]
	public class FarmImageMap
	{
		public int _FarmType;

		public string _IconSprite;

		public string _BannerSprite;
	}

	public LocaleString _FarmText = new LocaleString("{{UserName}} Farm");

	public LocaleString _FriendsFarmText = new LocaleString("Friend's Farm");

	public StoreLoader.Selection _StoreInfo;

	private UiFarmsListCardMenu mUiFarmsListCardMenu;

	private KAWidget mCurrentRoomItem;

	private KAWidget mTxtMyRoom;

	private string mLastSelectedFarmID;

	private bool mShowInfoCard;

	private bool mRoomSelected;

	public List<FarmImageMap> _FarmIcon = new List<FarmImageMap>();

	public string _FarmIconForUnknown;

	public string _FarmBannerForUnknown;

	public string _FarmIconStore;

	private UiFarms mUiFarms;

	private bool mInitialized;

	private UserRoom mHeaderRoomData;

	private List<UserRoom> mFarmRoomList = new List<UserRoom>();

	public UserRoom pHeaderRoomData => mHeaderRoomData;

	public bool pIsUserFarm => mUiFarms.pIsUserFarm;

	protected override void Awake()
	{
		base.Awake();
		mUiFarms = (UiFarms)_UiCardParent;
	}

	public string GetFarmIcon(int farmType)
	{
		if (_FarmIcon != null)
		{
			foreach (FarmImageMap item in _FarmIcon)
			{
				if (item._FarmType == farmType)
				{
					return item._IconSprite;
				}
			}
		}
		return _FarmIconForUnknown;
	}

	public string GetFarmBanner(int farmType)
	{
		if (_FarmIcon != null)
		{
			foreach (FarmImageMap item in _FarmIcon)
			{
				if (item._FarmType == farmType)
				{
					return item._BannerSprite;
				}
			}
		}
		return _FarmBannerForUnknown;
	}

	public override void SetVisibility(bool inVisible)
	{
		bool visibility = GetVisibility();
		base.SetVisibility(inVisible);
		if (!visibility && inVisible)
		{
			InitializeUI();
		}
	}

	private void InitializeUI()
	{
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		mTxtMyRoom = FindItem("TxtMyRoom");
		if (UiFarms.pUserID != null)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(UiFarms.pUserID);
			string text = _FriendsFarmText.GetLocalizedString();
			if (buddy != null)
			{
				text = _FarmText.GetLocalizedString().Replace("{{UserName}}", buddy.DisplayName);
			}
			mTxtMyRoom.SetText(text);
			WsWebService.GetUserRoomList(UiFarms.pUserID, 541, GetRoomServiceEventHandler, null);
		}
		else
		{
			UserRoom.LoadRooms(541, force: false, FarmRoomDataEvent);
		}
	}

	private void FarmRoomDataEvent(bool success)
	{
		if (!success)
		{
			return;
		}
		mFarmRoomList = UserRoom.GetRooms(541);
		foreach (UserRoom mFarmRoom in mFarmRoomList)
		{
			if (mFarmRoom.RoomID == mUiFarms._FarmExpansionDefaultRoomID)
			{
				mFarmRoom.pItemID = mUiFarms._FarmExpansionDefaultItemID;
			}
		}
		if (CommonInventoryData.pIsReady)
		{
			List<UserItemData> userItems = new List<UserItemData>();
			CheckInventory(CommonInventoryData.pInstance, ref userItems);
			CheckInventory(ParentData.pInstance.pInventory.pData, ref userItems);
			if (userItems.Count > 0)
			{
				foreach (UserItemData item in userItems)
				{
					UserRoom.AddRoom(item, 541);
				}
			}
		}
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		RefreshUI();
		if (!mIsTransition && !mRoomSelected)
		{
			SelectRoom(FarmManager.pCurrentFarmID);
		}
	}

	private void CheckInventory(CommonInventoryData cid, ref List<UserItemData> userItems)
	{
		UserItemData[] items = cid.GetItems(541);
		if (items == null)
		{
			return;
		}
		UserItemData[] array = items;
		foreach (UserItemData item in array)
		{
			if (item.Item.ItemID != mUiFarms._FarmExpansionDefaultItemID)
			{
				List<UserRoom> list = mFarmRoomList.FindAll((UserRoom s) => s.InventoryID == item.UserInventoryID);
				int num = 0;
				if (list == null)
				{
					num = item.Quantity;
				}
				else if (list.Count < item.Quantity)
				{
					num = item.Quantity - list.Count;
				}
				for (int j = 0; j < num; j++)
				{
					userItems.Add(item);
				}
			}
		}
	}

	public void GetRoomServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.GET_USER_ROOM_LIST)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			mFarmRoomList.Clear();
			if (inObject != null)
			{
				UserRoomResponse userRoomResponse = (UserRoomResponse)inObject;
				if (userRoomResponse != null)
				{
					mFarmRoomList.AddRange(userRoomResponse.UserRoomList);
				}
			}
			if (mFarmRoomList.Count == 0)
			{
				UserRoom userRoom = new UserRoom();
				userRoom.pItemID = mUiFarms._FarmExpansionDefaultItemID;
				userRoom.pLocaleName = "BaseFarm";
				userRoom.CategoryID = 541;
				userRoom.RoomID = mUiFarms._FarmExpansionDefaultRoomID;
				mFarmRoomList.Add(userRoom);
			}
			RefreshUI();
			if (!mIsTransition && !mRoomSelected)
			{
				SelectDefaultRoom();
			}
			break;
		case WsServiceEvent.ERROR:
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			mFarmRoomList.Clear();
			RefreshUI();
			break;
		}
	}

	private void SelectDefaultRoom()
	{
		UserRoom userRoom = mFarmRoomList.Find((UserRoom room) => room.RoomID == FarmManager.pCurrentFarmID);
		if (userRoom == null)
		{
			userRoom = mFarmRoomList.Find((UserRoom room) => room.RoomID == mUiFarms._FarmExpansionDefaultRoomID);
		}
		if (userRoom != null)
		{
			SelectRoom(userRoom.RoomID);
		}
	}

	public void RefreshUI()
	{
		mUiFarmsListCardMenu = GetComponentInChildren<UiFarmsListCardMenu>();
		mCurrentRoomItem = FindItem("CurrentRoom");
		mHeaderRoomData = mFarmRoomList.Find((UserRoom room) => room.RoomID == FarmManager.pCurrentFarmID);
		if (mHeaderRoomData == null)
		{
			mHeaderRoomData = mFarmRoomList.Find((UserRoom room) => room.RoomID == mUiFarms._FarmExpansionDefaultRoomID);
		}
		if (mHeaderRoomData != null)
		{
			mCurrentRoomItem.SetVisibility(inVisible: true);
			SetSelectedFarmItem(mHeaderRoomData);
		}
		else
		{
			mCurrentRoomItem.SetVisibility(inVisible: false);
		}
		mUiFarmsListCardMenu.LoadFarmsList(mFarmRoomList);
		mInitialized = true;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCurrentRoomItem && mHeaderRoomData != null)
		{
			SelectRoom(mHeaderRoomData.RoomID);
		}
	}

	public void RefreshFarmInfoTips()
	{
		if (GetState() == KAUIState.INTERACTIVE)
		{
			mUiFarms.SetMessage(this, mUiFarms._SelectFarmText);
		}
	}

	public override void OnExitClicked()
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnCardExit", SendMessageOptions.DontRequireReceiver);
		}
		PopOutCard();
	}

	private void SetSelectedFarmItem(UserRoom rData)
	{
		mCurrentRoomItem.FindChildItem("TxtRoomName").SetText(rData.pLocaleName);
		KAWidget kAWidget = mCurrentRoomItem.FindChildItem("RoomIco");
		if (kAWidget != null)
		{
			UISlicedSprite componentInChildren = kAWidget.GetComponentInChildren<UISlicedSprite>();
			string farmIcon = GetFarmIcon(rData.pItemID);
			if (!string.IsNullOrEmpty(farmIcon))
			{
				componentInChildren.UpdateSprite(farmIcon);
			}
		}
	}

	public void OpenStore()
	{
		mUiFarms.Exit();
		if (_StoreInfo != null)
		{
			StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, base.gameObject);
		}
	}

	public void SelectRoom(string roomID)
	{
		if (!(mLastSelectedFarmID != roomID))
		{
			return;
		}
		mRoomSelected = true;
		mLastSelectedFarmID = roomID;
		KAWidget kAWidget = mCurrentRoomItem.FindChildItem("SelectedFrame");
		if ((bool)kAWidget)
		{
			if (roomID == mHeaderRoomData.RoomID)
			{
				kAWidget.SetVisibility(inVisible: true);
				mUiFarmsListCardMenu.ResetSelection();
			}
			else
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
		if (!mUiFarms.pUiFarmsInfoCard.GetVisibility())
		{
			Transition childTrans = new Transition(Transition.Type.SlideOut, Transition.Direction.Right, Transition.ZOrder.Bottom);
			mUiFarms.pUiFarmsInfoCard.PushCard(this, null, childTrans);
			UserRoom pFarmData = mFarmRoomList.Find((UserRoom r) => r.RoomID == roomID);
			mUiFarms.pUiFarmsInfoCard.pFarmData = pFarmData;
			mUiFarms.pUiFarmsInfoCard.RefreshUI();
		}
		else
		{
			mShowInfoCard = true;
			mUiFarms.pUiFarmsInfoCard.PopOutCard(releaseParentLink: false);
			UserRoom pFarmData2 = mFarmRoomList.Find((UserRoom r) => r.RoomID == roomID);
			mUiFarms.pUiFarmsInfoCard.pFarmData = pFarmData2;
		}
	}

	public override void TransitionDone()
	{
		base.TransitionDone();
		if (mInitialized && FarmManager.pCurrentFarmData != null && !mRoomSelected)
		{
			SelectDefaultRoom();
		}
	}

	public override void ChildReverseTransitionDone()
	{
		base.ChildReverseTransitionDone();
		if (!base.pPopOutCard && mShowInfoCard)
		{
			mShowInfoCard = false;
			mUiFarms.pUiFarmsInfoCard.RefreshUI();
			Transition childTrans = new Transition(Transition.Type.SlideOut, Transition.Direction.Right, Transition.ZOrder.Bottom);
			mUiFarms.pUiFarmsInfoCard.PushCard(this, null, childTrans);
		}
	}
}
