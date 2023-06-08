using UnityEngine;

public class UiDragonEquipment : UiEquipment
{
	public Camera _Camera;

	public Transform _PetMarker;

	private SanctuaryPet mPet;

	private bool mLoadingData;

	private bool mChanged;

	private Camera mOldCam;

	private string mOldAnimName;

	private GameObject mObject;

	private Texture2D mTexture;

	private ItemPrefabResData mPrefabResourceData = new ItemPrefabResData();

	private ItemTextureResData mTextureResourceData = new ItemTextureResData();

	private KAWidget mSelectedItem;

	private void SetPet(SanctuaryPet pet)
	{
		mPet = pet;
		mOldAnimName = mPet._IdleAnimName;
		mPet._IdleAnimName = mPet._AnimNameIdle;
		mEquipmentOwner = mPet.gameObject;
		InitSetup();
		FindItem("DragonName").SetText(mPet.GetName());
	}

	public override void OnOpen()
	{
		base.OnOpen();
		mChanged = false;
		SetPet(SanctuaryManager.pCurPetInstance);
	}

	private void InitSetup()
	{
		if (_PetMarker != null)
		{
			mPet.transform.position = _PetMarker.position;
			mPet.transform.rotation = _PetMarker.rotation;
			mPet.SetFollowAvatar(follow: false);
		}
		mOldCam = mPet.GetCamera();
		mPet.SetCamera(_Camera);
		mPet.SetState(Character_State.idle);
		mPet.SetPlayScale();
		mPet.StopLookAtObject();
		if (SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.DetachFromToolbar();
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetBusy(busy: true);
		}
		SetVisibility(inVisible: true);
		AvAvatar.SetActive(inActive: false);
		SelectMenu(mMenu);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "AvatarEquipment")
		{
			RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("AvatarEquipmentAsset"), OnAvatarEquipmentLoaded, typeof(GameObject));
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		if (mSelectedItem != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)mSelectedItem.GetUserData();
			if (kAUISelectItemData._UserItemData != null)
			{
				SanctuaryManager.pCurPetInstance.pData.SetAccessory(RaisedPetAccType.Hat, kAUISelectItemData._PrefResName, kAUISelectItemData._TextResName, kAUISelectItemData._UserItemData);
			}
			else
			{
				SanctuaryManager.pCurPetInstance.pData.SetAccessory(RaisedPetAccType.Hat, kAUISelectItemData._PrefResName, kAUISelectItemData._TextResName, kAUISelectItemData._ItemID, kAUISelectItemData._UserInventoryID);
			}
			SanctuaryManager.pCurPetInstance.pData.SaveData();
		}
		if (SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.AttachToToolbar();
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetBusy(busy: false);
			if (mChanged)
			{
				MainStreetMMOClient.pInstance.SetRaisedPet(mPet.pData, -1);
			}
		}
		mPet._IdleAnimName = mOldAnimName;
		mPet.SetCamera(mOldCam);
		mPet.SetFollowAvatar(follow: true);
		mPet.SetAvatar(AvAvatar.mTransform);
		mPet.MoveToAvatar();
		mPet._Move2D = true;
		mPet._ActionDoneMessageObject = null;
		mPet.RestoreScale();
		Object.Destroy(base.gameObject);
	}

	public override void RemoveItem(CategorySlotData sd)
	{
		base.RemoveItem(sd);
		Object.Destroy(SanctuaryManager.pCurPetInstance.GetAccessoryObject(RaisedPetAccType.Hat));
	}

	public override void SetItem(KAWidget widget)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)widget.GetUserData();
		CategorySlotData[] slotItems = _SlotItems;
		foreach (CategorySlotData categorySlotData in slotItems)
		{
			if (kAUISelectItemData._ItemData.HasCategory(categorySlotData._Category))
			{
				mCurrentSlot = categorySlotData;
				if (FindItem(categorySlotData._SlotItemName).GetNumChildren() == 2)
				{
					Object.Destroy(SanctuaryManager.pCurPetInstance.GetAccessoryObject(RaisedPetAccType.Hat));
					AddItem(categorySlotData, widget);
				}
				else
				{
					RemoveItem(categorySlotData);
					AddItem(categorySlotData, widget);
				}
				mSelectedItem = widget;
				mPrefabResourceData.Init(kAUISelectItemData._PrefResName);
				mTextureResourceData.Init(kAUISelectItemData._TextResName);
				mPrefabResourceData.LoadData();
				mTextureResourceData.LoadData();
				mLoadingData = true;
				break;
			}
		}
	}

	public void RestoreUI()
	{
		if (SanctuaryManager.pInstance.pPetMeter != null)
		{
			SanctuaryManager.pInstance.pPetMeter.gameObject.SetActive(value: true);
		}
		mMenu.ReloadMenu();
		_Camera.gameObject.SetActive(value: true);
		AvAvatar.SetActive(inActive: false);
		SetVisibility(inVisible: true);
	}

	private void OnDataReady()
	{
		mTexture = (Texture2D)mTextureResourceData._Texture;
		if (mPrefabResourceData._Prefab != null && mPrefabResourceData._Prefab.gameObject != null)
		{
			mChanged = true;
			mObject = Object.Instantiate(mPrefabResourceData._Prefab.gameObject);
			if (mTextureResourceData._Texture != null)
			{
				UtUtilities.SetObjectTexture(mObject, 0, mTextureResourceData._Texture);
			}
			SanctuaryManager.pCurPetInstance.SetAccessory(RaisedPetAccType.Hat, mObject, mTexture);
		}
	}

	public void OnAvatarEquipmentLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			Object.Destroy(base.transform.root.gameObject);
			UiAvatarEquipment componentInChildren = Object.Instantiate((GameObject)inObject).GetComponentInChildren<UiAvatarEquipment>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = true;
				componentInChildren._CloseMsgObject = _CloseMsgObject;
				componentInChildren.OnOpen();
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to load back pack ....");
			break;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mLoadingData && mPrefabResourceData.IsDataLoaded() && mTextureResourceData.IsDataLoaded())
		{
			mLoadingData = false;
			OnDataReady();
		}
	}
}
