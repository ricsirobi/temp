using UnityEngine;

public class UiSocialCrate : KAUI
{
	public enum SocialBoxUiType
	{
		CRATE_INFO,
		SIGNATURE_LIST,
		REWARD_GROUP,
		SIGNATURE_NOTIFICATION
	}

	public UiSocialCrateInfo _SocialCrateInfo;

	public UiSignatureList _SignatureList;

	public UiSocialCrateReward _RewardGroup;

	public UiSignatureNotification _SignatureNotification;

	public static UiSocialCrate pInstance;

	private static SocialBoxUiType mCurrentUi;

	protected override void Start()
	{
		base.Start();
		switch (mCurrentUi)
		{
		case SocialBoxUiType.CRATE_INFO:
			if (_SocialCrateInfo != null)
			{
				_SocialCrateInfo.gameObject.SetActive(value: true);
				_SocialCrateInfo.SetMessageObject(base.gameObject);
				KAUI.SetExclusive(_SocialCrateInfo);
			}
			break;
		case SocialBoxUiType.REWARD_GROUP:
			if (_RewardGroup != null)
			{
				_RewardGroup.gameObject.SetActive(value: true);
				_RewardGroup.SetMessageObject(base.gameObject);
				KAUI.SetExclusive(_RewardGroup);
			}
			break;
		case SocialBoxUiType.SIGNATURE_LIST:
			if (_SignatureList != null)
			{
				_SignatureList.gameObject.SetActive(value: true);
				_SignatureList.SetMessageObject(base.gameObject);
				KAUI.SetExclusive(_SignatureList);
			}
			break;
		case SocialBoxUiType.SIGNATURE_NOTIFICATION:
			if (_SignatureNotification != null)
			{
				_SignatureNotification.gameObject.SetActive(value: true);
				_SignatureNotification.SetMessageObject(base.gameObject);
				KAUI.SetExclusive(_SignatureNotification);
			}
			break;
		}
	}

	public static void Show(SocialBoxUiType uiType)
	{
		mCurrentUi = uiType;
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = GameConfig.GetKeyData("SocialCrateAsset").Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnBundleReady, typeof(GameObject));
	}

	private static void OnBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			pInstance = Object.Instantiate((GameObject)inObject).GetComponent<UiSocialCrate>();
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	public void OnExit()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		Object.Destroy(base.gameObject);
		pInstance = null;
	}
}
