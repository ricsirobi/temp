using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "PetData", Namespace = "", IsNullable = false)]
public class PetData
{
	[XmlElement(ElementName = "Pet")]
	public PetDataPet[] Pet;

	public static PetData mCurPetData;

	public static PetData mPetData;

	public static PetData pCurrentPet => mCurPetData;

	public static bool IsFreePet(PetDataPet pd)
	{
		if (!(pd.Geometry == "RS_SHARED/PullToyDragon/PfPullToyDragon") && !(pd.Geometry == "RS_SHARED/PetBunny/PfPetBunnyMagic") && !(pd.Geometry == "RS_SHARED/PetCat/PfPetCatMagic") && !(pd.Geometry == "RS_SHARED/PetLamb/PfPetLambMagic"))
		{
			return pd.Geometry == "RS_SHARED/PetTurtle/PfPetTurtleMagic";
		}
		return true;
	}

	public static bool HasCurrentPet()
	{
		if (mCurPetData == null)
		{
			return false;
		}
		return true;
	}

	public static void InitCurrent(GameObject msgObj)
	{
		if (mCurPetData == null)
		{
			WsWebService.GetCurrentPetData(WsGetCurEventHandler, msgObj.name);
		}
		else if (msgObj != null)
		{
			msgObj.SendMessage("OnCurPetDataLoaded", mCurPetData.Pet[0]);
		}
	}

	public static void WsSetCurEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("-----WEB SERVICE CALL SetCurrentPet FAILED!!!");
			break;
		case WsServiceEvent.COMPLETE:
			if (inType == WsServiceType.SET_CURRENT_PET)
			{
				UtDebug.Log("------WEB SERVICE CALL SetCurrentPet RETURNED completed.");
			}
			break;
		}
	}

	public static void ChangeEmptyStrings(PetData petData, bool addSlash)
	{
		PetDataPet[] pet = petData.Pet;
		foreach (PetDataPet petDataPet in pet)
		{
			if (addSlash)
			{
				if (petDataPet.AccessoryGeometry.Length == 0)
				{
					petDataPet.AccessoryGeometry = "/";
				}
				if (petDataPet.AccessoryTexture.Length == 0)
				{
					petDataPet.AccessoryTexture = "/";
				}
			}
			else
			{
				if (petDataPet.AccessoryGeometry == "/")
				{
					petDataPet.AccessoryGeometry = "";
				}
				if (petDataPet.AccessoryTexture == "/")
				{
					petDataPet.AccessoryTexture = "";
				}
			}
		}
	}

	public static void WsGetCurEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		string text = (string)inUserData;
		GameObject gameObject = null;
		if (text != null && text.Length > 0)
		{
			gameObject = GameObject.Find(text);
			if (gameObject == null)
			{
				UtDebug.LogError("Pet loading notification object [" + text + "]doesn't exist");
			}
		}
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("-----WEB SERVICE CALL GeCurPetData FAILED!!!");
			mCurPetData = null;
			break;
		case WsServiceEvent.COMPLETE:
			if (inType != WsServiceType.GET_CURRENT_PET)
			{
				break;
			}
			if (inObject != null)
			{
				mCurPetData = (PetData)inObject;
				if (mCurPetData.Pet == null)
				{
					mCurPetData = null;
					PetDataPet petDataPet = new PetDataPet();
					petDataPet.Geometry = "";
					gameObject.SendMessage("OnCurPetDataLoaded", petDataPet);
				}
				else if (gameObject != null)
				{
					gameObject.SendMessage("OnCurPetDataLoaded", mCurPetData.Pet[0]);
				}
			}
			else
			{
				UtDebug.Log("------WEB SERVICE CALL GetCurPetData RETURNED NO DATA!!!");
				if (gameObject != null)
				{
					PetDataPet petDataPet2 = new PetDataPet();
					petDataPet2.Geometry = "";
					gameObject.SendMessage("OnCurPetDataLoaded", petDataPet2);
				}
			}
			break;
		}
	}

	public static void SaveCurrent(PetDataPet pd)
	{
		if (pd != null)
		{
			if (mCurPetData == null)
			{
				mCurPetData = new PetData();
				mCurPetData.Pet = new PetDataPet[1];
			}
			mCurPetData.Pet[0] = pd;
			WsWebService.SetCurrentPetData(mCurPetData, WsSetCurEventHandler, null);
		}
		else
		{
			if (mCurPetData != null)
			{
				mCurPetData = null;
			}
			WsWebService.DeleteCurrentPetData(WsSetCurEventHandler, null);
		}
	}

	public static void InitPets(GameObject msgObj)
	{
		if (mPetData == null)
		{
			WsWebService.GetAdoptedPetData(WsGetEventHandler, msgObj.name);
		}
		else if (msgObj != null)
		{
			msgObj.SendMessage("OnPetDataLoaded", mPetData);
		}
	}

	public static void WsSetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		GameObject gameObject = (GameObject)inUserData;
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("-----WEB SERVICE CALL SetPet FAILED!!!");
			if (gameObject != null)
			{
				gameObject.SendMessage("OnPetDataSaved", null);
			}
			break;
		case WsServiceEvent.COMPLETE:
			if (inType == WsServiceType.SET_ADOPTED_PET)
			{
				UtDebug.Log("------WEB SERVICE CALL SetPet RETURNED completed.");
				if (gameObject != null)
				{
					gameObject.SendMessage("OnPetDataSaved", null);
				}
			}
			break;
		}
	}

	public static void WsGetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		string text = (string)inUserData;
		GameObject gameObject = null;
		if (text != null && text.Length > 0)
		{
			gameObject = GameObject.Find(text);
			if (gameObject == null)
			{
				UtDebug.LogError("Pet loading notification object [" + text + "]doesn't exist");
			}
		}
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("-----WEB SERVICE CALL GeCurPetData FAILED!!!");
			mPetData = null;
			break;
		case WsServiceEvent.COMPLETE:
			if (inType != WsServiceType.GET_ADOPTED_PET)
			{
				break;
			}
			if (inObject != null)
			{
				mPetData = (PetData)inObject;
				if (gameObject != null)
				{
					gameObject.SendMessage("OnPetDataLoaded", mPetData);
				}
				break;
			}
			UtDebug.Log("------WEB SERVICE CALL GetPetData RETURNED NO DATA!!!");
			mPetData = new PetData();
			mPetData.Pet = null;
			if (gameObject != null)
			{
				gameObject.SendMessage("OnPetDataLoaded", mPetData);
			}
			break;
		}
	}

	public static void SavePets(GameObject msgObj)
	{
		PetDataPet[] pet = mPetData.Pet;
		foreach (PetDataPet petDataPet in pet)
		{
			if (petDataPet.Dirtiness > 0.99f)
			{
				petDataPet.Dirtiness = 0.99f;
			}
			if (petDataPet.Dirtiness < 0.01f)
			{
				petDataPet.Dirtiness = 0.01f;
			}
		}
		WsWebService.SetAdoptedPetData(mPetData, WsSetEventHandler, msgObj);
	}

	public PetDataPet FindMatch(string geo, string tex)
	{
		if (Pet == null)
		{
			return null;
		}
		PetDataPet[] pet = Pet;
		foreach (PetDataPet petDataPet in pet)
		{
			if (petDataPet.Geometry == geo && petDataPet.Texture == tex)
			{
				return petDataPet;
			}
		}
		UtDebug.LogError("-adopted pet " + geo + "-" + tex + " not found");
		return null;
	}
}
