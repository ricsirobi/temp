using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponChestRoomItem : WeaponStorageRoomItem
{
	[Serializable]
	public class Upgrade
	{
		[SerializeField]
		private int m_ItemID = 18546;

		[SerializeField]
		private int m_StoreID = 91;

		[SerializeField]
		private float m_Scale = 2.5f;

		internal int GetItemID()
		{
			return m_ItemID;
		}

		internal int GetStoreID()
		{
			return m_StoreID;
		}

		internal float GetScale()
		{
			return m_Scale;
		}
	}

	public GameObject _ChestLid;

	public Vector3 _Marker;

	[SerializeField]
	private List<Upgrade> m_Upgrades;

	[SerializeField]
	private float m_ChestUiDelay = 1f;

	[NonSerialized]
	private float mAnimationTimer;

	private bool mIsChestOpeningAnim;

	public LocaleString _PutAwayItemText = new LocaleString("[REVIEW]You must remove all items from this chest before stowing it.");

	public LocaleString _PutAwayItemTitle = new LocaleString("Warning!");

	protected override void SetRoomItemData()
	{
		if (mRoomObject?.pUserItemData != null)
		{
			PairData pairData = null;
			if (mRoomObject.pUserItemData.UserItemAttributes == null)
			{
				mRoomObject.pUserItemData.UserItemAttributes = new PairData();
				mRoomObject.pUserItemData.UserItemAttributes.Init();
				pairData = mRoomObject.pUserItemData.UserItemAttributes;
				pairData.SetValue("ChestUpgradeCount", "0");
				pairData.SetValue("StorageItemType", "WeaponChest");
				pairData.PrepareArray();
				WsWebService.SetCommonInventoryAttribute(mRoomObject.pUserItemData.UserInventoryID, pairData, OnInventorySaveCallBack, null);
			}
			else
			{
				pairData = mRoomObject.pUserItemData.UserItemAttributes;
				pairData.Init();
				pairData.SetValue("StorageItemType", "WeaponChest");
			}
		}
	}

	[ContextMenu("Reset Upgrades")]
	public void ResetUpgrades()
	{
		int num = 0;
		int num2 = 0;
		PairData userItemAttributes = mRoomObject.pUserItemData.UserItemAttributes;
		if (userItemAttributes != null)
		{
			userItemAttributes.SetValue("StorageItemSlotCount", num.ToString());
			userItemAttributes.SetValue("ChestUpgradeCount", num2.ToString());
			userItemAttributes.PrepareArray();
			WsWebService.SetCommonInventoryAttribute(mRoomObject.pUserItemData.UserInventoryID, userItemAttributes, OnInventorySaveCallBack, null);
		}
	}

	public void OnInventorySaveCallBack(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != WsServiceEvent.COMPLETE)
		{
			_ = 3;
		}
	}

	public void UpgradeChest(float scale)
	{
		base.transform.localScale = Vector3.one * scale;
	}

	public bool IsUpgradeAvailable()
	{
		if (mRoomObject.pUserItemData.UserItemAttributes.GetIntValue("ChestUpgradeCount", 0) == m_Upgrades.Count)
		{
			return false;
		}
		return true;
	}

	public Upgrade GetUpgrade()
	{
		if (!IsUpgradeAvailable())
		{
			return null;
		}
		int intValue = mRoomObject.pUserItemData.UserItemAttributes.GetIntValue("ChestUpgradeCount", 0);
		return m_Upgrades[intValue];
	}

	protected override void Update()
	{
		base.Update();
		if (mIsChestOpeningAnim)
		{
			mAnimationTimer -= Time.deltaTime;
			if (mAnimationTimer <= 0f)
			{
				mIsChestOpeningAnim = false;
				mAnimationTimer = 0f;
				UiMyRoomBuilder myRoomBuilder = MyRoomsIntMain.pInstance._UiMyRoomsInt._MyRoomBuilder;
				string[] array = _BattleItemStorageBundle.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnBattleItemStorageLoaded, typeof(GameObject));
				myRoomBuilder.pSelectedObject = base.gameObject;
			}
		}
	}

	protected override void OnContextAction(string inActionName)
	{
		if (!(inActionName == "Pack Away"))
		{
			if (inActionName == "AddWeapon")
			{
				PlayOpenAnimation();
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

	private void PlayOpenAnimation()
	{
		mIsChestOpeningAnim = true;
		Animation component = _ChestLid.GetComponent<Animation>();
		component["Open"].speed = 1f;
		if (!component.isPlaying)
		{
			component.Play("Open");
		}
		mAnimationTimer = m_ChestUiDelay;
	}

	public void PlayCloseAnimation()
	{
		Animation component = _ChestLid.GetComponent<Animation>();
		component["Open"].speed = -1f;
		if (!component.isPlaying)
		{
			component.Play("Open");
		}
	}
}
