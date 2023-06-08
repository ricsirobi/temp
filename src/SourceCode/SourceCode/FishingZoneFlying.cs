using System;
using System.Collections.Generic;
using UnityEngine;

public class FishingZoneFlying : FishingZone
{
	[Serializable]
	public class FishingDragonType
	{
		public int _TypeID = 13;

		public string _TypeName = "Gronckle";
	}

	public List<FishingDragonType> _AllowedDragonTypes;

	public string _DragonTailBone = "Tail01_J";

	public Vector3 _BaitBucketOffset = Vector3.zero;

	public Vector3 _BaitMenuOffset = new Vector3(76f, -242f, 0f);

	private AvAvatarController mAvController;

	private bool mCheckForHalt;

	public override void Start()
	{
		mIsTutAvailable = false;
		base.Start();
		mAvController = AvAvatar.pObject.GetComponent<AvAvatarController>();
	}

	private bool DragonTypeAllowed(SanctuaryPet pet = null)
	{
		if (pet == null)
		{
			pet = SanctuaryManager.pCurPetInstance;
		}
		if (pet != null)
		{
			if (_AllowedDragonTypes.Find((FishingDragonType type) => type._TypeID == pet.pTypeInfo._TypeID) != null)
			{
				return true;
			}
			if (_AllowedDragonTypes.Find((FishingDragonType type) => type._TypeName == pet.pTypeInfo._Name) != null)
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnTriggerStay(Collider other)
	{
		if (RsResourceManager.pLevelLoadingScreen || !FishingData.pIsReady || !other.gameObject.CompareTag("Player"))
		{
			return;
		}
		bool flag = true;
		BoxCollider boxCollider = collider as BoxCollider;
		if (boxCollider != null)
		{
			Vector3 size = boxCollider.size - new Vector3(_TriggerThreshold, 0f, _TriggerThreshold);
			flag = !new Bounds(boxCollider.transform.position + boxCollider.center, size).Contains(other.transform.position);
		}
		if (mIsActive)
		{
			if (FishingZone._FishingZoneUi.GetVisibility() && !flag && !AvAvatarController.mForceBraking)
			{
				ExitZone(other);
			}
		}
		else if (flag && PlayerOnRide() && DragonTypeAllowed() && CanActivate())
		{
			OnAbilityZoneEntered(null);
			if (FishingZone._FishingZoneUi == null)
			{
				GameObject obj = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiFishing"));
				obj.name = "PfUiFishing";
				FishingZone._FishingZoneUi = obj.GetComponent<UiFishing>();
			}
			if (FishingZone._FishingZoneUi != null)
			{
				FishingZone._FishingZoneUi.SetFishingZone(this);
				FishingZone._FishingZoneUi.SetVisibility(inVisible: true);
			}
		}
	}

	protected override void OnTriggerExit(Collider other)
	{
		if (!AvAvatarController.mForceBraking)
		{
			ExitZone(other);
		}
	}

	public override void EquipFishingRod()
	{
		if (mAvController != null)
		{
			FishingZone._FishingZoneUi.ShowStopFishingButton(show: false);
			AvAvatarController.mForceBraking = true;
			mCheckForHalt = true;
		}
	}

	private void EquipFishingRodReal()
	{
		SetupFishingCam();
		SanctuaryManager.pCurPetInstance.pFlyCollider.gameObject.SetActive(value: false);
		FishingZone._FishingZoneUi.ShowStopFishingButton(show: true);
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		if (!string.IsNullOrEmpty(_AmbientMusicPool))
		{
			SnChannel.PausePool(_AmbientMusicPool);
		}
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(_RodCategoryID);
		if (items != null)
		{
			ItemData itemData = null;
			int num = -1;
			UserItemData[] array = items;
			foreach (UserItemData userItemData in array)
			{
				int value = userItemData.Item.RankId.Value;
				if (value >= num)
				{
					num = value;
					itemData = userItemData.Item;
				}
			}
			mFishingRodBundle = itemData.AssetName;
		}
		else
		{
			mFishingRodBundle = _DefaultRod;
		}
		if (FishingZone._CheatPoleID != -1)
		{
			UserItemData userItemData2 = CommonInventoryData.pInstance.FindItem(FishingZone._CheatPoleID);
			if (userItemData2 != null)
			{
				mFishingRodBundle = userItemData2.Item.AssetName;
			}
		}
		string[] array2 = mFishingRodBundle.Split('/');
		RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], base.OnRodLoadingEvent, typeof(GameObject));
	}

	public override void SetupBaitBucket(GameObject baitBucket, BaitMenu.OnShowHandler handler)
	{
		Transform transform = UtUtilities.FindChildTransform(SanctuaryManager.pCurPetInstance.gameObject, _DragonTailBone);
		baitBucket.transform.parent = transform.transform;
		Vector3 localScale = baitBucket.transform.localScale;
		baitBucket.transform.localScale = Vector3.one;
		baitBucket.transform.localPosition = _BaitBucketOffset;
		baitBucket.transform.localScale = localScale;
		baitBucket.transform.localRotation = Quaternion.identity;
		BaitMenu componentInChildren = baitBucket.GetComponentInChildren<BaitMenu>();
		componentInChildren.transform.parent = base.transform;
		componentInChildren.transform.localRotation = Quaternion.identity;
		componentInChildren.transform.localScale = Vector3.one;
		componentInChildren.transform.position = _BaitMenuOffset;
		componentInChildren._FishingZone = this;
		componentInChildren.SetNoBaitText(_NoBaitText);
		componentInChildren.OnShow += handler;
	}

	public override void DestroyBaitBucket(GameObject baitBucket, BaitMenu.OnShowHandler handler)
	{
		BaitMenu componentInChildren = base.gameObject.GetComponentInChildren<BaitMenu>();
		componentInChildren.OnShow -= handler;
		UnityEngine.Object.Destroy(componentInChildren.gameObject);
		if (null != baitBucket)
		{
			UnityEngine.Object.Destroy(baitBucket);
		}
	}

	private void SetupFishingCam()
	{
		mAvatarCam = AvAvatar.pAvatarCam;
		if (null != mAvatarCam)
		{
			mCaCam = mAvatarCam.GetComponent<CaAvatarCam>();
		}
		if (mCaCam != null)
		{
			mCaCam._IgnoreCollision = true;
		}
	}

	public override void StopFishing()
	{
		base.StopFishing();
		if (mCaCam != null)
		{
			mCaCam._IgnoreCollision = false;
		}
		SanctuaryManager.pCurPetInstance.pFlyCollider.gameObject.SetActive(value: true);
		if (mAvController != null)
		{
			AvAvatarController.mForceBraking = false;
		}
	}

	public override void Update()
	{
		if (mIsActive)
		{
			if ((bool)SanctuaryManager.pCurPetInstance && !SanctuaryManager.pCurPetInstance.pIsMounted)
			{
				ExitZone(null);
			}
			else
			{
				mCurrState.Execute();
			}
		}
		if (mCheckForHalt && mAvController != null && mAvController.pFlightSpeed == 0f)
		{
			mCheckForHalt = false;
			EquipFishingRodReal();
		}
		if (Application.isEditor && Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.Z))
		{
			AvAvatar.TeleportTo(new Vector3(-28f, 3f, 48f));
		}
		mFalseStrikeTimer -= Time.deltaTime;
	}
}
