using UnityEngine;

public class UiAvatarItem
{
	private string mUserID;

	private AvPhotoManager mPhotoManager;

	private KAWidget mItem;

	private UiMessageBoard mMessageBoard;

	public UiAvatarItem(string userID, AvPhotoManager photoManager, KAWidget inWidget, UiMessageBoard board)
	{
		mUserID = userID;
		mPhotoManager = photoManager;
		mItem = inWidget;
		mMessageBoard = board;
	}

	public void PhotoCallback(Texture tex, object inUserData)
	{
		if (mPhotoManager == null || mMessageBoard == null)
		{
			return;
		}
		if (mPhotoManager._LiveShot)
		{
			if (mMessageBoard._PhotoTextureObj != null)
			{
				UtUtilities.SetObjectTexture(mMessageBoard._PhotoTextureObj, 0, tex);
			}
		}
		else if (mItem != null)
		{
			mItem.FindChildItem("Picture").SetTexture(tex);
			if (DataCache.Get<AvatarData>(mUserID + "_AvatarData", out var inObject) && inObject != null)
			{
				KAWidget kAWidget = mItem.FindChildItem("TxtName");
				kAWidget.SetText(inObject.DisplayName);
				kAWidget.SetInteractive(isInteractive: true);
			}
		}
	}

	public void TakePhoto()
	{
		if ((bool)mPhotoManager)
		{
			if (mPhotoManager._LiveShot)
			{
				mPhotoManager.TakePhoto(mUserID, null, PhotoCallback, null);
			}
			else
			{
				mPhotoManager.TakePhotoUI(mUserID, (Texture2D)mItem.FindChildItem("Picture").GetTexture(), PhotoCallback, null);
			}
		}
	}
}
