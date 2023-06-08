using UnityEngine;

public class KAUIStoreChoose3DDragons : KAUIStoreChoose3D
{
	private RaisedPetStage mPrevPetStage;

	private RaisedPetStage mNewPetStage;

	private ItemData mItemData;

	public override void OnPurchaseItem(bool isBundle, ItemData itemData)
	{
		base.OnPurchaseItem(isBundle, itemData);
	}

	public void OnSetAgeDone(SetRaisedPetResponse response)
	{
		if (response != null && response.RaisedPetSetResult == RaisedPetSetResult.Success)
		{
			SanctuaryManager.pCurPetInstance.SetAge(RaisedPetData.GetAgeIndex(mNewPetStage), save: false, resetSkills: true);
			SanctuaryManager.pInstance.pSetFollowAvatar = true;
			if (CommonInventoryData.pInstance.RemoveItem(mItemData.ItemID, updateServer: false) >= 0)
			{
				CommonInventoryData.pInstance.ClearSaveCache();
			}
			if (mNewPetStage == RaisedPetStage.TITAN)
			{
				UserAchievementTask.Set(SanctuaryManager.pInstance._DragonTitanAchievemetID);
			}
			if (mPet != null)
			{
				Object.Destroy(mPet.gameObject);
				LoadDragon();
				mPet.gameObject.SetActive(value: false);
			}
		}
		else
		{
			SanctuaryManager.pInstance.SetAge(SanctuaryManager.pCurPetData, RaisedPetData.GetAgeIndex(mPrevPetStage), inSave: false);
			KAUIStore.pInstance.HideCompleteStore(hide: false);
		}
	}

	public override void OnSyncUIClosed(bool isPurchaseSuccess)
	{
		base.OnSyncUIClosed(isPurchaseSuccess);
		if (isPurchaseSuccess && base.pChosenItemData != null && base.pChosenItemData._ItemData != null && base.pChosenItemData._ItemData.HasCategory(435))
		{
			if (KAUIStore.pInstance != null)
			{
				KAUIStore.pInstance.HideCompleteStore(hide: true);
			}
			UiDragonsAgeUp.Init(OnDragonAgeUpUIDone, closeOnAgeUp: false, null, isTicketPurchased: true);
		}
	}

	private void OnDragonAgeUpUIDone()
	{
		KAUIStore.pInstance.HideCompleteStore(hide: false);
	}
}
