using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponRackRoomItem : WeaponStorageRoomItem
{
	[Serializable]
	public class SlotInfo
	{
		public string _SlotName;

		public Transform _Marker;
	}

	public List<SlotInfo> _SlotInfo;

	public LocaleString _PutAwayItemText = new LocaleString("[REVIEW]You must remove all items from this rack before stowing it.");

	public LocaleString _PutAwayItemTitle = new LocaleString("Warning!");

	protected KAUIGenericDB mKAUIGenericDB;

	protected override void SetRoomItemData()
	{
		if (mRoomObject?.pUserItemData != null)
		{
			if (mRoomObject.pUserItemData.UserItemAttributes == null)
			{
				mRoomObject.pUserItemData.UserItemAttributes = new PairData();
				mRoomObject.pUserItemData.UserItemAttributes.Init();
			}
			mRoomObject.pUserItemData.UserItemAttributes.SetValue("StorageItemType", "WeaponRack");
		}
	}

	public SlotInfo GetSelectedSlotInfo(string slotName)
	{
		return _SlotInfo.Find((SlotInfo x) => x._SlotName.Equals(slotName));
	}

	protected override void OnContextAction(string inActionName)
	{
		UiMyRoomBuilder myRoomBuilder = MyRoomsIntMain.pInstance._UiMyRoomsInt._MyRoomBuilder;
		if (!(inActionName == "Pack Away"))
		{
			if (inActionName == "AddWeapon")
			{
				string[] array = _BattleItemStorageBundle.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnBattleItemStorageLoaded, typeof(GameObject));
				myRoomBuilder.pSelectedObject = base.gameObject;
			}
		}
		else if ((bool)GetComponentInChildren<ObAvatarPropInfo>())
		{
			KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "NotEnoughGemsDB");
			kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
			kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			kAUIGenericDB.SetText(_PutAwayItemText.GetLocalizedString(), interactive: false);
			kAUIGenericDB.SetTitle(_PutAwayItemTitle.GetLocalizedString());
			KAUI.SetExclusive(kAUIGenericDB);
			return;
		}
		base.OnContextAction(inActionName);
	}
}
