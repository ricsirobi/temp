using UnityEngine;

public class BackyardPetSpawner : MonoBehaviour
{
	private static bool mPetLoadInitiated;

	private static GameObject mBackyardPet;

	private static Transform mBackyardPetMarker;

	private static BackyardPetSpawner mInstance;

	private void Start()
	{
		mInstance = this;
		mBackyardPetMarker = base.transform.Find("PfMarker_BackyardPet");
		if (mBackyardPetMarker == null)
		{
			UtDebug.LogError("Unable to find PfMarker_BackyardPet. Will not spawn backyard pet.");
		}
	}

	private void Update()
	{
		if (LastTwoPurchasedPets.pIsReady && UserInfo.pIsReady && !mPetLoadInitiated)
		{
			mPetLoadInitiated = true;
			LoadPet();
		}
	}

	public void LoadPet()
	{
		WsWebService.GetCurrentPetByUserID(UserInfo.pInstance.UserID, GetCurrentPetEventHandler, null);
	}

	public void GetCurrentPetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		GetCurrentPetStatus status = GetCurrentPetStatus.FAILED;
		PetDataPet pdp = null;
		switch (inEvent)
		{
		case WsServiceEvent.PROGRESS:
			return;
		case WsServiceEvent.COMPLETE:
		{
			if (inObject == null)
			{
				break;
			}
			PetData petData = (PetData)inObject;
			if (petData != null)
			{
				if (petData.Pet == null)
				{
					status = GetCurrentPetStatus.NOPET;
					break;
				}
				status = GetCurrentPetStatus.SUCCESS;
				pdp = petData.Pet[0];
			}
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.LogError("Failed loading Current Pet Data.");
			break;
		}
		CurrentPetDataLoaded(pdp, status);
	}

	private void CurrentPetDataLoaded(PetDataPet pdp, GetCurrentPetStatus status)
	{
		switch (status)
		{
		case GetCurrentPetStatus.FAILED:
		case GetCurrentPetStatus.NOPET:
			SpawnLastPurchasedPet();
			break;
		case GetCurrentPetStatus.SUCCESS:
			CompareWithLastPurchasedAndSpawnPet(pdp);
			break;
		}
	}

	private void SpawnLastPurchasedPet()
	{
		SpawnPet(LastTwoPurchasedPets.pLastPurchasedPet);
	}

	private void SpawnPenultimatePurchasedPet()
	{
		SpawnPet(LastTwoPurchasedPets.pPenultimatePurchasedPet);
	}

	private void SpawnPet(PetDataPet pdp)
	{
		if (mBackyardPetMarker != null)
		{
			TeleportFx.PlayAt(mBackyardPetMarker.transform.position);
		}
		DestroyCurrentBackyardPet();
		if (pdp != null)
		{
			new PetBundleLoader().LoadPetFromPetDataPet(pdp, PetLoadedCallback, null);
		}
	}

	public static void OnPetReturned()
	{
		if (mInstance != null)
		{
			mInstance.CompareWithLastPurchasedAndSpawnPet(null);
		}
	}

	public void OnPetSelected(PetDataPet pdp)
	{
		CompareWithLastPurchasedAndSpawnPet(pdp);
	}

	public static void OnPetPurchased(PetDataPet pdp)
	{
		if (mInstance != null)
		{
			mInstance.OnPetSelected(pdp);
		}
	}

	private void CompareWithLastPurchasedAndSpawnPet(PetDataPet pdp)
	{
		if (AreSamePets(pdp, LastTwoPurchasedPets.pLastPurchasedPet))
		{
			SpawnPenultimatePurchasedPet();
		}
		else
		{
			SpawnLastPurchasedPet();
		}
	}

	private void DestroyCurrentBackyardPet()
	{
		if (mBackyardPet != null)
		{
			Object.Destroy(mBackyardPet);
			mBackyardPet = null;
		}
	}

	public bool AreSamePets(PetDataPet pdp1, PetDataPet pdp2)
	{
		if (pdp1 == null && pdp2 != null)
		{
			return false;
		}
		if (pdp1 != null && pdp2 == null)
		{
			return false;
		}
		if (pdp1.Geometry == pdp2.Geometry && pdp1.Texture == pdp2.Texture)
		{
			return true;
		}
		return false;
	}

	public void PetLoadedCallback(PetBundleLoader bundleLoader, bool loadSucceeded, object callbackData)
	{
		if (!(mBackyardPetMarker == null) && loadSucceeded)
		{
			mBackyardPet = bundleLoader.pPetGeometry;
			mBackyardPet.transform.position = mBackyardPetMarker.position;
			mBackyardPet.transform.rotation = mBackyardPetMarker.rotation;
			mBackyardPet.SetActive(value: true);
		}
	}
}
