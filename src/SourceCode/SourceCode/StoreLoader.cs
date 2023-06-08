using System;
using UnityEngine;

public class StoreLoader
{
	[Serializable]
	public class Selection
	{
		[Tooltip("Name of store to open.")]
		public string _Store;

		[Tooltip("Name of category to open.  Leave blank to open default category.")]
		public string _Category;
	}

	public const string STORE_LAUNCH_COUNT = "STORE_LAUNCH_COUNT";

	public const string STORE_OBJECT_NAME = "PfUiStoresDO";

	private static GameObject mExitToObject;

	private static KAUIGenericDB mGenericDB;

	private static AvAvatarState mPrevAvatarState;

	public static KAUIGenericDB pGenericDB => mGenericDB;

	public static void Load(bool setDefaultMenuItem, int enterSelectionCategoryID, int enterSelectionStoreID, GameObject exitObject, UILoadOptions inLoadOption = UILoadOptions.AUTO, string backToJournalTab = "", string storeExitMarker = null)
	{
		KAUIStore._EnterSelectionID = enterSelectionStoreID;
		KAUIStore._EnterCategoryID = enterSelectionCategoryID;
		Load(GetStoreAssetPath(), setDefaultMenuItem, "", "", exitObject, inLoadOption, backToJournalTab, storeExitMarker);
	}

	public static void Load(bool setDefaultMenuItem, string categoryEnterSelection, string enterSelection, GameObject exitObject, UILoadOptions inLoadOption = UILoadOptions.AUTO, string backToJournalTab = "", string storeExitMarker = null)
	{
		Load(GetStoreAssetPath(), setDefaultMenuItem, categoryEnterSelection, enterSelection, exitObject, inLoadOption, backToJournalTab, storeExitMarker);
	}

	private static string GetStoreAssetPath()
	{
		if (UtMobileUtilities.IsWideDisplay())
		{
			return GameConfig.GetKeyData("StoreAsset");
		}
		return GameConfig.GetKeyData("StoreAsset4x3");
	}

	public static void Load(string storeResource, bool setDefaultMenuItem, string categoryEnterSelection, string enterSelection, GameObject exitObject, UILoadOptions inLoadOption = UILoadOptions.AUTO, string backToJournalTab = "", string storeExitMarker = null)
	{
		KAUIStoreCategory._SetDefaultMenuItem = setDefaultMenuItem;
		KAUIStore._EnterSelection = enterSelection;
		KAUIStore._EnterCategoryName = categoryEnterSelection;
		KAUIStore._BackToJournalTab = backToJournalTab;
		if (UtPlatform.IsMobile() && RsResourceManager.pCurrentLevel == "FarmingDO")
		{
			storeExitMarker = "PfMarker_AvatarStart";
		}
		KAUIStore._StoreExitMarker = storeExitMarker;
		mExitToObject = exitObject;
		AvAvatar.SetUIActive(inActive: false);
		mPrevAvatarState = ((AvAvatar.pState == AvAvatarState.PAUSED) ? AvAvatar.pPrevState : AvAvatar.pState);
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (UtMobileUtilities.CanLoadInCurrentScene(UiType.Store, inLoadOption))
		{
			AvAvatar.SetUIActive(inActive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = storeResource.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnStoreLoadingEvent, typeof(GameObject));
			return;
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
		}
		SanctuaryManager.pMountedState = false;
		AvAvatar.SetStartPositionAndRotation();
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.Disconnect();
		}
		RsResourceManager.LoadLevel(GameConfig.GetKeyData("StoreScene"));
	}

	private static void OnStoreLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			AvAvatar.pState = mPrevAvatarState;
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			gameObject.name = "PfUiStoresDO";
			gameObject.GetComponentInChildren<KAUIStore>()._ExitMessageObjects.Add(mExitToObject);
			KAUICursorManager.SetDefaultCursor("Arrow");
			RsResourceManager.ReleaseBundleData(inURL);
			float safeAreaHeightRatio = UtMobileUtilities.GetSafeAreaHeightRatio();
			gameObject.transform.localScale -= new Vector3(safeAreaHeightRatio, safeAreaHeightRatio);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = mPrevAvatarState;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}
}
