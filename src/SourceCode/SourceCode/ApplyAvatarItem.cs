using System;
using System.Collections.Generic;
using UnityEngine;

public class ApplyAvatarItem : MonoBehaviour
{
	[Serializable]
	public class ItemGrp
	{
		public Gender _Gender;

		public ItemVariant[] _Items;
	}

	[Serializable]
	public class ItemVariant
	{
		public int[] _Variants;
	}

	public LocaleString _ItemUnavailable = new LocaleString("You don't have the required quest item(s).");

	public ItemGrp[] _ItemsGroup;

	public bool _ForceFlightSuit;

	public string _SceneName;

	public string _StartMarker;

	public GameObject _Marker;

	public float _Distance = -1f;

	private List<UserItemData> mItemData = new List<UserItemData>();

	private AvAvatarController mAvatarController;

	private bool mCheckItem = true;

	private bool mCanResetAvatar;

	private CoCommonLevel mCommonLevel;

	private Collider mTrigger;

	private void Awake()
	{
		mTrigger = base.gameObject.GetComponent<Collider>();
		RsResourceManager.LoadLevelStarted += OnLevelLoad;
		GameObject gameObject = GameObject.Find("PfCommonLevel");
		if (gameObject != null)
		{
			mCommonLevel = gameObject.GetComponent<CoCommonLevel>();
		}
	}

	private void Start()
	{
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
	}

	private void Update()
	{
		if (!mAvatarController.isActiveAndEnabled || (mCommonLevel != null && !mCommonLevel.pWaitListCompleted))
		{
			return;
		}
		if (KAUIStore.pInstance != null && !KAUIStore.pInstance._ExitMessageObjects.Contains(base.gameObject))
		{
			KAUIStore.pInstance._ExitMessageObjects.Add(base.gameObject);
		}
		if (mTrigger != null)
		{
			return;
		}
		if (_Distance > -1f)
		{
			if (IsInRange() && !UiJournal.pIsJournalActive)
			{
				mCanResetAvatar = true;
				CheckAndApplyItem();
			}
			else if (IsItemApplied())
			{
				ResetAvatar();
			}
		}
		else if (!UiJournal.pIsJournalActive)
		{
			mCanResetAvatar = true;
			CheckAndApplyItem();
		}
		else if (IsItemApplied())
		{
			ResetAvatar();
		}
	}

	private bool IsItemApplied()
	{
		for (int i = 0; i < _ItemsGroup.Length; i++)
		{
			if ((_ItemsGroup[i]._Gender == Gender.Unknown || AvatarData.GetGender() == _ItemsGroup[i]._Gender) && mItemData.Count == _ItemsGroup[i]._Items.Length)
			{
				return true;
			}
		}
		return false;
	}

	private void CheckAndApplyItem()
	{
		if (mItemData.Count != 0 || !mCheckItem)
		{
			return;
		}
		mCheckItem = false;
		ItemGrp itemGrp = null;
		if (_ItemsGroup != null && _ItemsGroup.Length != 0)
		{
			itemGrp = _ItemsGroup[0];
			for (int i = 0; i < _ItemsGroup.Length; i++)
			{
				if (_ItemsGroup[i]._Gender != 0 && AvatarData.GetGender() != _ItemsGroup[i]._Gender)
				{
					continue;
				}
				itemGrp = _ItemsGroup[i];
				for (int j = 0; j < itemGrp._Items.Length; j++)
				{
					for (int k = 0; k < itemGrp._Items[j]._Variants.Length; k++)
					{
						UserItemData userItemData = CommonInventoryData.pInstance.FindItem(itemGrp._Items[j]._Variants[k]);
						if (userItemData != null)
						{
							mItemData.Add(userItemData);
							break;
						}
					}
				}
				break;
			}
		}
		if (_ForceFlightSuit && mAvatarController != null && !AvatarData.pInstanceInfo.FlightSuitEquipped())
		{
			UserItemData lastUsedFlightSuit = mAvatarController.GetLastUsedFlightSuit();
			if (lastUsedFlightSuit != null)
			{
				AvatarData.pInstanceInfo.UpdatePartInventoryId(AvatarData.pPartSettings.AVATAR_PART_WING, lastUsedFlightSuit);
				mAvatarController.EquipFlightSuit(lastUsedFlightSuit.Item, OnFlightSuitDownloaded);
			}
		}
		if (mItemData == null || mItemData.Count <= 0)
		{
			return;
		}
		if (itemGrp != null && itemGrp._Items != null && mItemData.Count < itemGrp._Items.Length)
		{
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ItemUnavailable.GetLocalizedString(), base.gameObject, "OnDBClose");
			return;
		}
		if (mAvatarController.pIsPlayerGliding)
		{
			mAvatarController.OnGlideLanding();
		}
		mAvatarController.pAvatarCustomization.RestoreAvatar();
		for (int l = 0; l < mItemData.Count; l++)
		{
			mAvatarController.pAvatarCustomization.UpdateGroupPart(AvatarData.pInstanceInfo, mItemData[l].Item, OnAllItemsDownloaded);
		}
		mAvatarController.pCanGlide = false;
		UiToolbar.pAvatarModified = true;
	}

	private void OnFlightSuitDownloaded()
	{
		mAvatarController.pAvatarCustomization.pCustomAvatar.UpdateShaders(AvAvatar.pObject);
		UserItemData lastUsedFlightSuit = mAvatarController.GetLastUsedFlightSuit();
		if (lastUsedFlightSuit != null)
		{
			UiAvatarItemCustomization.ApplyCustomizationOnPart(AvAvatar.mTransform.gameObject, AvatarData.pPartSettings.AVATAR_PART_WING, AvatarData.pInstance, lastUsedFlightSuit.UserInventoryID);
			UiAvatarItemCustomization.SaveAvatarPartAttributes(AvatarData.pPartSettings.AVATAR_PART_WING, lastUsedFlightSuit.UserInventoryID);
		}
		mAvatarController.pAvatarCustomization.SaveCustomAvatar();
	}

	private void OnAllItemsDownloaded()
	{
		mAvatarController.pAvatarCustomization.pCustomAvatar.UpdateShaders(AvAvatar.pObject);
		mAvatarController.pAvatarCustomization.SwapAvatars();
		if (mAvatarController.pAvatarCustomization.pCustomAvatar != null)
		{
			mAvatarController.pAvatarCustomization.pCustomAvatar.ToAvatarData(AvatarData.pInstanceInfo);
		}
		if (AvatarData.pInstanceInfo != null)
		{
			AvatarData.pInstanceInfo.RemovePart();
			AvatarData.pInstanceInfo.LoadBundlesAndUpdateAvatar();
		}
	}

	private void OnDBClose()
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		if (!string.IsNullOrEmpty(_SceneName))
		{
			AvAvatar.SetActive(inActive: false);
			if (!string.IsNullOrEmpty(_StartMarker))
			{
				AvAvatar.pStartLocation = _StartMarker;
			}
			RsResourceManager.LoadLevel(_SceneName);
		}
		else if (_Marker != null)
		{
			mItemData.Clear();
			mCheckItem = true;
			AvAvatar.TeleportToObject(_Marker);
		}
	}

	private void ResetAvatar()
	{
		if (mCanResetAvatar)
		{
			mAvatarController.pAvatarCustomization.RestoreAvatar();
			UiToolbar.pAvatarModified = true;
			mCanResetAvatar = false;
			mItemData.Clear();
			mCheckItem = true;
			mAvatarController.pCanGlide = true;
		}
	}

	private bool IsInRange()
	{
		if (Vector3.Distance(base.transform.position, AvAvatar.position) <= _Distance)
		{
			return true;
		}
		return false;
	}

	private void OnDestroy()
	{
		RsResourceManager.LoadLevelStarted -= OnLevelLoad;
	}

	private void OnLevelLoad(string level)
	{
		if (IsItemApplied())
		{
			bool num = mAvatarController.isActiveAndEnabled;
			if (!num)
			{
				AvAvatar.SetActive(inActive: true);
			}
			mAvatarController.pAvatarCustomization.RestoreAvatar();
			mAvatarController.pCanGlide = true;
			UiToolbar.pAvatarModified = true;
			mItemData.Clear();
			AvAvatar.SetActive(num);
		}
	}

	public void OnStoreClosed()
	{
		if (IsItemApplied())
		{
			mCanResetAvatar = true;
			ResetAvatar();
		}
	}

	private void OnTriggerStay(Collider collision)
	{
		if (!(collision.transform.root.gameObject != AvAvatar.pObject))
		{
			if (!UiJournal.pIsJournalActive)
			{
				mCanResetAvatar = true;
				CheckAndApplyItem();
			}
			else if (IsItemApplied())
			{
				ResetAvatar();
			}
		}
	}

	private void OnTriggerExit(Collider collision)
	{
		if (collision.transform.root.gameObject == AvAvatar.pObject)
		{
			mCanResetAvatar = true;
			ResetAvatar();
		}
	}
}
