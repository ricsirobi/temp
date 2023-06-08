using UnityEngine;

public class UserNotifyActivationReminder : UserNotify
{
	public string _AuthorizationEmailBundlePath;

	private UiAuthorizationReminder mAuthorizationEmailUI;

	public override void OnWaitBeginImpl()
	{
		bool flag = true;
		if (UiLogin.pParentInfo != null)
		{
			bool num = UiLogin.pParentInfo.UnAuthorized.HasValue && UiLogin.pParentInfo.UnAuthorized.Value;
			bool flag2 = UiLogin.pParentInfo.SendActivationReminder.HasValue && UiLogin.pParentInfo.SendActivationReminder.Value;
			if (num && flag2)
			{
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.SetUIActive(inActive: false);
				KAUICursorManager.SetDefaultCursor("Loading");
				string[] array = _AuthorizationEmailBundlePath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], AuthorizationUiLoadHandler, typeof(GameObject));
				flag = false;
			}
		}
		if (flag)
		{
			base.OnWaitEnd();
		}
	}

	public void OnDBClose()
	{
		OnWaitEnd();
	}

	protected override void OnWaitEnd()
	{
		if (mAuthorizationEmailUI != null)
		{
			Object.Destroy(mAuthorizationEmailUI.gameObject);
		}
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		base.OnWaitEnd();
	}

	private void AuthorizationUiLoadHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if ((uint)(inEvent - 2) > 1u)
		{
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (inObject == null || inEvent == RsResourceLoadEvent.ERROR)
		{
			UtDebug.LogError("Failed to load Autorization Email bundle", 100);
			OnWaitEnd();
			return;
		}
		GameObject gameObject = Object.Instantiate((GameObject)inObject);
		if (gameObject != null)
		{
			mAuthorizationEmailUI = gameObject.GetComponent<UiAuthorizationReminder>();
			mAuthorizationEmailUI.pCallbackObject = base.gameObject;
			mAuthorizationEmailUI.SetVisibility(inVisible: true);
		}
		else
		{
			OnWaitEnd();
		}
	}
}
