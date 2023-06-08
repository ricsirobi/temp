using UnityEngine;

public class LastTwoPurchasedPets : MonoBehaviour
{
	private static bool mIsReady;

	private const int mCategoryID = 233;

	private static PetDataPet mLastPurchasedPet;

	private static PetDataPet mPenultimatePurchasedPet;

	public static bool pIsReady
	{
		get
		{
			return mIsReady;
		}
		set
		{
			mIsReady = value;
		}
	}

	public static PetDataPet pLastPurchasedPet => mLastPurchasedPet;

	public static PetDataPet pPenultimatePurchasedPet => mPenultimatePurchasedPet;

	private void Start()
	{
		if (!CommonInventoryData.pIsReady)
		{
			CommonInventoryData.Init();
		}
	}

	private void Update()
	{
		if (!CommonInventoryData.pIsReady || pIsReady)
		{
			return;
		}
		pIsReady = true;
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(233);
		if (items != null && items.Length != 0)
		{
			PetDataPet petDataPetFromItem = GetPetDataPetFromItem(items[^1].Item);
			if (items.Length > 1)
			{
				OnPetPurchased(GetPetDataPetFromItem(items[^2].Item));
			}
			OnPetPurchased(petDataPetFromItem);
		}
	}

	public static void OnPetPurchased(PetDataPet currentPurchase)
	{
		mPenultimatePurchasedPet = mLastPurchasedPet;
		mLastPurchasedPet = currentPurchase;
	}

	private static PetDataPet GetPetDataPetFromItem(ItemData item)
	{
		return new PetDataPet
		{
			Geometry = item.AssetName,
			Texture = item.Texture[0].TextureName,
			Type = "",
			Name = "",
			Dirtiness = 0.5f,
			AccessoryGeometry = "",
			AccessoryTexture = ""
		};
	}
}
