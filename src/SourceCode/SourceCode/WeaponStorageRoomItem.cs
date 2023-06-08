using System.Collections.Generic;
using UnityEngine;

public class WeaponStorageRoomItem : MyRoomItem
{
	public ContextSensitiveState _BuildModeContextSensitiveState;

	public string _BattleItemStorageBundle = "RS_DATA/PfUiBattleItemStorageDO.unity3d/PfUiBattleItemStorageDO";

	public List<string> _CategoryOverride = new List<string>();

	public List<int> _CategoryToExclude = new List<int>();

	protected GameObject mBattleItemStorageUI;

	public int _DefaultSlotCount = 1;

	protected MyRoomObject mRoomObject;

	private List<MyRoomObject> mEquippedItems = new List<MyRoomObject>();

	public List<MyRoomObject> pEquippedItems
	{
		get
		{
			return mEquippedItems;
		}
		set
		{
			mEquippedItems = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (mRoomObject == null)
		{
			mRoomObject = GetComponent<MyRoomObject>();
		}
		SetRoomItemData();
		SetEquippedItems();
	}

	protected virtual void SetRoomItemData()
	{
		if (mRoomObject?.pUserItemData != null)
		{
			if (mRoomObject.pUserItemData.UserItemAttributes == null)
			{
				mRoomObject.pUserItemData.UserItemAttributes = new PairData();
				mRoomObject.pUserItemData.UserItemAttributes.Init();
			}
			int defaultSlotCount = _DefaultSlotCount;
			mRoomObject.pUserItemData.UserItemAttributes.SetValue("StorageItemSlotCount", defaultSlotCount.ToString());
		}
	}

	protected virtual void SetEquippedItems()
	{
		if (!MyRoomsIntMain.pInstance)
		{
			return;
		}
		string pCurrentRoomID = MyRoomsIntMain.pInstance.pCurrentRoomID;
		int? userItemPositionID = UserItemPositionList.GetUserItemPositionID(pCurrentRoomID, base.gameObject);
		if (!userItemPositionID.HasValue || userItemPositionID.Value == 0)
		{
			return;
		}
		pEquippedItems?.Clear();
		UserItemPosition[] list = UserItemPositionList.GetList(pCurrentRoomID);
		foreach (UserItemPosition userItemPosition in list)
		{
			if (userItemPosition.ParentID == userItemPositionID)
			{
				MyRoomObject component = userItemPosition._GameObject.GetComponent<MyRoomObject>();
				pEquippedItems?.Add(component);
			}
		}
	}

	public void AddToEquppedList(MyRoomObject roomObject)
	{
		if (pEquippedItems != null)
		{
			int num = pEquippedItems.FindIndex((MyRoomObject x) => x.Equals(roomObject));
			if (num != -1)
			{
				pEquippedItems[num] = roomObject;
			}
			else
			{
				pEquippedItems.Add(roomObject);
			}
		}
	}

	public void RemoveFromEquippedList(MyRoomObject roomObject)
	{
		if (pEquippedItems != null)
		{
			int num = pEquippedItems.FindIndex((MyRoomObject x) => x.Equals(roomObject));
			if (num != -1)
			{
				pEquippedItems.RemoveAt(num);
			}
		}
	}

	public MyRoomObject GetStorageItem(KAWidget storageItem)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)storageItem.GetUserData();
		foreach (MyRoomObject pEquippedItem in pEquippedItems)
		{
			if (pEquippedItem.pUserItemData.UserInventoryID.Equals(kAUISelectItemData._UserItemData.UserInventoryID))
			{
				return pEquippedItem;
			}
		}
		return null;
	}

	protected override ContextSensitiveState[] GetSensitiveData(List<ContextSensitiveState> csData)
	{
		if (MyRoomsIntMain.pInstance != null && !MyRoomsIntMain.pInstance.pIsBuildMode)
		{
			csData.Add(_DefaultContextSensitiveState);
		}
		else if (MyRoomsIntMain.pInstance != null && MyRoomsIntMain.pInstance.pIsBuildMode)
		{
			csData.Add(_BuildModeContextSensitiveState);
		}
		return csData.ToArray();
	}

	protected virtual void OnBattleItemStorageLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mBattleItemStorageUI = Object.Instantiate((GameObject)inObject);
			mBattleItemStorageUI.name = "PfUiBattleItemStorage";
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}
}
