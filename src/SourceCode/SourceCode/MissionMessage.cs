using UnityEngine;

internal class MissionMessage : GenericMessage
{
	private string mAssetBundlePath = string.Empty;

	private int mNumAssetsLoading;

	private GameObject mDialogBox;

	public MissionMessage(MessageInfo messageInfo, string inAssetBundlePath)
		: base(messageInfo, null)
	{
		mAssetBundlePath = inAssetBundlePath;
	}

	public override void Show()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		if (!string.IsNullOrEmpty(mAssetBundlePath))
		{
			string[] array = mAssetBundlePath.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], DBCallback, typeof(GameObject));
			mNumAssetsLoading++;
		}
		if (!string.IsNullOrEmpty(mMessageInfo.MemberAudioUrl))
		{
			string[] array2 = mMessageInfo.MemberAudioUrl.Split('/');
			RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], SpeechCallback, typeof(AudioClip));
			mNumAssetsLoading++;
		}
	}

	public override void Update()
	{
		if (mNumAssetsLoading != 0)
		{
			return;
		}
		mNumAssetsLoading = -1;
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (!string.IsNullOrEmpty(mMessageInfo.NonMemberLinkUrl) && !SubscriptionInfo.pIsMember)
		{
			_OKBtnVisible = false;
		}
		mUiGenericDB = Object.Instantiate(mDialogBox);
		base.Show();
		if (!string.IsNullOrEmpty(mMessageInfo.NonMemberLinkUrl) && !SubscriptionInfo.pIsMember)
		{
			KAWidget kAWidget = mUiGenericDB.GetComponent<KAUIGenericDB>().FindItem(mMessageInfo.NonMemberLinkUrl);
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: true);
			}
		}
	}

	public override void Close()
	{
		base.Close();
		if (MissionManager.pInstance != null && MissionManager.pInstance.pEventMessageObject != null)
		{
			MissionManager.pInstance.pEventMessageObject.SendMessage("OnMissionMessageClose", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void DBCallback(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mDialogBox = (GameObject)inObject;
			mNumAssetsLoading--;
			break;
		case RsResourceLoadEvent.ERROR:
			mNumAssetsLoading--;
			break;
		}
	}

	public void SpeechCallback(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mAudioClip = (AudioClip)inObject;
			mNumAssetsLoading--;
			break;
		case RsResourceLoadEvent.ERROR:
			mNumAssetsLoading--;
			break;
		}
	}
}
