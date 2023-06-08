using UnityEngine;

public class UiSelectFriendItemData : KAWidgetUserData
{
	private AvPhotoManager mPhotoManager;

	private bool mIsLoading;

	public UiSelectFriendItemData(AvPhotoManager photoManager)
	{
		mPhotoManager = photoManager;
	}

	public void Load()
	{
		if (!mIsLoading)
		{
			mIsLoading = true;
			if (mPhotoManager != null)
			{
				KAWidget kAWidget = _Item.FindChildItem("IcoPlayer");
				mPhotoManager.TakePhotoUI(_Item.name, (kAWidget != null) ? ((Texture2D)kAWidget.GetTexture()) : null, BuddyPhotoCallback, null);
			}
			KAWidget kAWidget2 = _Item.FindChildItem("TxtName");
			if (kAWidget2 != null && string.IsNullOrEmpty(kAWidget2.GetText()))
			{
				WsWebService.GetDisplayNameByUserID(_Item.name, GetNameEventHandler, null);
			}
		}
	}

	public void BuddyPhotoCallback(Texture tex, object inUserData)
	{
		if (_Item != null)
		{
			KAWidget kAWidget = _Item.FindChildItem("IcoPlayer");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
				kAWidget.SetTexture(tex);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
	}

	private void GetNameEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && _Item != null)
		{
			KAWidget kAWidget = _Item.FindChildItem("TxtName");
			if (kAWidget != null)
			{
				kAWidget.SetText((string)inObject);
			}
		}
	}
}
