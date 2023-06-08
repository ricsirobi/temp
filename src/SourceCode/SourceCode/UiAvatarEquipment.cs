using UnityEngine;

public class UiAvatarEquipment : UiEquipment
{
	public Transform _AvatarStartMarker;

	private KAUIAvatarPartChangerData mAvPartChanger;

	private bool mModified;

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "DragonEquipment" && SanctuaryManager.pCurPetInstance != null)
		{
			RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("DragonEquipmentAsset"), OnDragonEquipmentLoaded, typeof(GameObject));
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		mEquipmentOwner = AvAvatar.pObject;
		if (AvAvatar.pAvatarCam != null)
		{
			AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>().SetLookAt(null, null, 0f);
		}
		InitMenuItems(mMenu);
		AvAvatar.SetActive(inActive: true);
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.SetDisplayNameVisible(inVisible: false);
		AvAvatar.pState = AvAvatarState.NONE;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		if (_AvatarStartMarker != null)
		{
			AvAvatar.SetPosition(_AvatarStartMarker);
		}
		AvatarData.SetDontDestroyOnBundles(inDontDestroy: false);
		PlayTutorial();
		if (mIdleManager != null)
		{
			mIdleManager.StartIdles();
		}
		AvAvatar.PlayAnim("Idle", WrapMode.Loop);
	}

	public override void SetItem(KAWidget widget)
	{
		base.SetItem(widget);
		ApplySelection(widget, mCurrentSlot._SlotItemName);
	}

	public override void OnClose()
	{
		AvatarData.SetDontDestroyOnBundles(inDontDestroy: true);
		if (mModified)
		{
			UiToolbar.pAvatarModified = true;
		}
		Object.Destroy(base.gameObject);
		if (mModified)
		{
			AvatarData.Save();
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.SetBusy(busy: false);
		}
		base.OnClose();
	}

	private void ApplySelection(KAWidget widget, string inPartName)
	{
		if ((KAUISelectItemData)widget.GetUserData() != null && !string.IsNullOrEmpty(inPartName))
		{
			if (mAvPartChanger == null)
			{
				mAvPartChanger = new KAUIAvatarPartChangerData();
			}
			mModified = true;
			mAvPartChanger._PrtTypeName = inPartName;
			mAvPartChanger.ApplySelection(widget);
		}
	}

	public void OnDragonEquipmentLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			Object.Destroy(base.transform.root.gameObject);
			AvAvatar.SetActive(inActive: false);
			GameObject obj = Object.Instantiate((GameObject)inObject);
			obj.transform.position = new Vector3(0f, 0f, 0f);
			UiDragonEquipment componentInChildren = obj.GetComponentInChildren<UiDragonEquipment>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = true;
				componentInChildren._CloseMsgObject = _CloseMsgObject;
				componentInChildren.OnOpen();
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to load dragon equipment ....");
			break;
		}
	}
}
