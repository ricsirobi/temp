using UnityEngine;

public class JournalLoader
{
	private static GameObject mMessageObject;

	private static AvAvatarState mPrevAvatarState;

	public static void Load(string enterSelection, string selectionWidget, bool setDefaultMenuItem, GameObject messageObject, bool resetLastSceneRef = true, UILoadOptions inLoadOption = UILoadOptions.AUTO, string message = "")
	{
		mMessageObject = messageObject;
		UiJournal.EnterSelection = enterSelection;
		UiJournal.SelectionWidget = selectionWidget;
		UiJournal.Message = message;
		AvAvatar.SetStartPositionAndRotation();
		AvAvatar.SetUIActive(inActive: false);
		mPrevAvatarState = AvAvatar.pState;
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (resetLastSceneRef)
		{
			UiJournal.pJournalExitScene = RsResourceManager.pCurrentLevel;
		}
		if (UiJournal.pInstance != null)
		{
			KAWidget kAWidget = UiJournal.pInstance.FindItem(enterSelection);
			if (kAWidget != null)
			{
				UiJournal.pInstance.OnClick(kAWidget);
			}
			JournalActivated(UiJournal.pInstance.gameObject);
			return;
		}
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("JournalScene") || UtMobileUtilities.CanLoadInCurrentScene(UiType.Journal, inLoadOption))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = GameConfig.GetKeyData("JournalAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnPDALoadingEvent, typeof(GameObject));
			return;
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
		}
		SanctuaryManager.pMountedState = false;
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.Disconnect();
		}
		RsResourceManager.LoadLevel(GameConfig.GetKeyData("JournalScene"));
	}

	private static void OnPDALoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			AvAvatar.pState = mPrevAvatarState;
			GameObject gameObject = Object.Instantiate((GameObject)inObject);
			gameObject.name = "PfUiJournal";
			KAUICursorManager.SetDefaultCursor("Arrow");
			JournalActivated(gameObject);
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = mPrevAvatarState;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private static void JournalActivated(GameObject instance)
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("JournalActivated", instance, SendMessageOptions.DontRequireReceiver);
		}
	}
}
