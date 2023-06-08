using System;
using UnityEngine;

public class LoginManager
{
	private bool mTestUserPolicy;

	private string mAppName = "";

	private string mUserName = "";

	private string mPassword = "";

	private string mFacebookID = "";

	private string mFaceBookAccessToken = "";

	private string mChildUserID = "";

	private string mLocale = "";

	private int? mAge;

	private WsServiceEventHandler mCallback;

	private object mUserData;

	private bool mIsGuest;

	public bool pTestUserPolicy => mTestUserPolicy;

	public void LoginGuest(string appName, string userName, string locale, WsServiceEventHandler inCallback, object inUserData)
	{
		mIsGuest = true;
		mAppName = appName;
		mUserName = userName;
		mLocale = locale;
		mCallback = inCallback;
		mUserData = inUserData;
		WsWebService.LoginGuest(appName, userName, mAge, locale, ServiceLoginEventHandler, null);
	}

	public void LoginParent(string username, string password, string facebookID, string facebookAccessToken, string childUserID, string locale, WsServiceEventHandler inCallback, object inUserData)
	{
		mIsGuest = false;
		mUserName = username;
		mPassword = password;
		mFacebookID = facebookID;
		mFaceBookAccessToken = facebookAccessToken;
		mChildUserID = childUserID;
		mLocale = locale;
		mCallback = inCallback;
		mUserData = inUserData;
		WsWebService.LoginParent(username, password, null, "", "", locale, ServiceLoginEventHandler, null);
	}

	public void ServiceLoginEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.LOGIN_GUEST && inType != WsServiceType.LOGIN_PARENT)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			ParentLoginInfo parentLoginInfo = (ParentLoginInfo)inObject;
			if (parentLoginInfo == null)
			{
				break;
			}
			if (mTestUserPolicy || parentLoginInfo.Status == MembershipUserStatus.UserPolicyNotAccepted)
			{
				ShowUserTerms(inType, parentLoginInfo);
			}
			else
			{
				KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
				mCallback(inType, inEvent, inProgress, inObject, inUserData);
			}
			if (parentLoginInfo.Status != 0 || PlayfabManager<PlayFabManagerDO>.Instance == null)
			{
				break;
			}
			PlayfabManager<PlayFabManagerDO>.Instance.ParentToken = parentLoginInfo.ApiToken;
			bool guest = inType == WsServiceType.LOGIN_GUEST;
			PlayfabManager<PlayFabManagerDO>.Instance.LoginUser(parentLoginInfo.UserID, guest, null);
			if (inType == WsServiceType.LOGIN_GUEST)
			{
				if (PlayerPrefs.HasKey("FTUE_GUESTNAME") && PlayerPrefs.GetString("FTUE_GUESTNAME", "") != UiLogin.pParentInfo.UserID)
				{
					UnityAnalyticsAgent.pNewUser = false;
				}
				else if (!PlayerPrefs.HasKey("FTUE_GUESTNAME"))
				{
					PlayerPrefs.SetString("FTUE_GUESTNAME", UiLogin.pParentInfo.UserID);
					PlayerPrefs.Save();
				}
			}
			break;
		}
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
			mCallback(inType, inEvent, inProgress, inObject, inUserData);
			break;
		}
	}

	private void ShowUserTerms(WsServiceType wsServiceType, ParentLoginInfo parentLoginInfo)
	{
		string inAssetName = "PfUiUserAgreement";
		if (wsServiceType == WsServiceType.LOGIN_GUEST)
		{
			Guid result;
			if (PlayerPrefs.HasKey("NeedAge"))
			{
				inAssetName = "PfUiUserAgreementGuest";
			}
			else if (Guid.TryParse(parentLoginInfo.UserID, out result) && result == Guid.Empty)
			{
				PlayerPrefs.SetString("NeedAge", "");
				inAssetName = "PfUiUserAgreementGuest";
			}
		}
		KAUICursorManager.SetDefaultCursor("Arrow", showHideSystemCursor: false);
		mTestUserPolicy = false;
		UiUserAgreement component = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(inAssetName)).GetComponent<UiUserAgreement>();
		KAUI.SetExclusive(component);
		component.AddAction(LoginOnUserAcceptance);
	}

	private void LoginOnUserAcceptance(bool userOver13)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		UserPolicy userPolicy = new UserPolicy();
		userPolicy.PrivacyPolicy = true;
		userPolicy.TermsAndConditions = true;
		PlayerPrefs.DeleteKey("NeedAge");
		mAge = ((!userOver13) ? 1 : 14);
		if (mIsGuest)
		{
			WsWebService.LoginGuest(mAppName, mUserName, mAge, mLocale, ServiceLoginEventHandler, null, userPolicy);
		}
		else
		{
			WsWebService.LoginParent(mUserName, mPassword, mFacebookID, mFaceBookAccessToken, mChildUserID, mLocale, ServiceLoginEventHandler, mUserData, userPolicy);
		}
	}
}
