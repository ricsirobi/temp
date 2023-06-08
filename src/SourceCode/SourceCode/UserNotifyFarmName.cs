using UnityEngine;

public class UserNotifyFarmName : UserNotify
{
	private GameObject mUiFarmNameObject;

	private UiFarmName mFarmNameUi;

	public override void OnWaitBeginImpl()
	{
		CheckAndLoadFarmNameEdit();
	}

	private void CheckAndLoadFarmNameEdit()
	{
		if (!MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt() && FarmManager.pCurrentFarmData != null && string.IsNullOrEmpty(FarmManager.pCurrentFarmData.Name))
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = GameConfig.GetKeyData("FarmNameAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], FarmNameUiLoadHandler, typeof(GameObject));
		}
		else
		{
			OnWaitEnd();
		}
	}

	private void FarmNameUiLoadHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			mUiFarmNameObject = Object.Instantiate((GameObject)inObject);
			mUiFarmNameObject.name = "PfUiFarmName";
			mFarmNameUi = mUiFarmNameObject.GetComponent<UiFarmName>();
			if (mFarmNameUi != null)
			{
				mFarmNameUi.OnNameChanged += CallOnWaitEnd;
				break;
			}
			UtDebug.LogError("Couldn't find avatar name customization script", 100);
			OnWaitEnd();
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load farm name customization bundle", 100);
			OnWaitEnd();
			break;
		}
	}

	private void CallOnWaitEnd(bool nameChanged)
	{
		if (mFarmNameUi != null)
		{
			mFarmNameUi.OnNameChanged -= CallOnWaitEnd;
		}
		OnWaitEnd();
	}

	protected override void OnWaitEnd()
	{
		if (mUiFarmNameObject != null)
		{
			Object.Destroy(mUiFarmNameObject);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		base.OnWaitEnd();
	}
}
