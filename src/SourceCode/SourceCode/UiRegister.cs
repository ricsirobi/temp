using System;
using System.Collections.Generic;
using KA.Framework;
using UnityEngine;

public class UiRegister : KAUI
{
	public string _TermsAndConditionsURL = "http://www.schoolofdragons.com/Help/Siteterms";

	public string _PrivacyPolicyURL = "https://policy-portal.truste.com/core/privacy-policy/Knowledge-Adventure/4158c489-ef0b-439f-b09f-827a1e18af03";

	public LocaleString _EmailSentToParentText = new LocaleString("An email has been sent to your parent. Play now while your parent approves this account!");

	public LocaleString _EmailSentText = new LocaleString("An email has been sent to your acount. Please activate your account for complete access to the site.");

	public LocaleString _RegistrationTitleText = new LocaleString("Registration");

	public LocaleString _GuestAccountConvertedText = new LocaleString("Your guest account has been synched to the new account.");

	public LocaleString _PlayersEmailText = new LocaleString("Player's Email");

	public LocaleString _ParentEmailText = new LocaleString("Parent Email");

	public LocaleString _EmailMatchSuccessText = new LocaleString("Emails match!");

	public LocaleString _EmailMatchFailText = new LocaleString("Email does not match!");

	public LocaleString _PasswordMatchFailText = new LocaleString("Password does not match!");

	public LocaleString _PasswordMatchSuccessText = new LocaleString("Passwords match!");

	private KAEditBox mTxtEmail;

	private KAEditBox mTxtEmailConfirm;

	private KAWidget mTxtEmailMatch;

	private KAEditBox mTxtPassword;

	private KAEditBox mTxtPasswordConfirm;

	private KAWidget mTxtPasswordMatch;

	private KAEditBox mTxtPlayerFirstName;

	private KAWidget mLabelParentsEmail;

	private KAWidget mEmailHighlight;

	private KAToggleButton mBtnAgreeTerms;

	private KAToggleButton mBtnOptIn;

	private KAWidget mBtnRegister;

	private KAUIGenericDB mKAUIGenericDB;

	private KAToggleButton mAgeToggle;

	private UniWebView mUniWebView;

	public static bool pGuestChatPromptOpened;

	protected override void Start()
	{
		base.Start();
		mTxtEmail = (KAEditBox)FindItem("TxtEmail");
		mTxtEmailConfirm = (KAEditBox)FindItem("TxtEmailConfirm");
		mTxtEmailMatch = FindItem("TxtEmailMatch");
		mTxtPassword = (KAEditBox)FindItem("TxtPassword");
		mTxtPasswordConfirm = (KAEditBox)FindItem("TxtPasswordConfirm");
		mTxtPasswordMatch = FindItem("TxtPasswordMatch");
		mTxtPlayerFirstName = (KAEditBox)FindItem("TxtPlayerFirstName");
		mLabelParentsEmail = FindItem("ParentsEmail");
		mEmailHighlight = FindItem("AniEmailHighlight");
		mBtnRegister = FindItem("BtnRegister");
		mBtnAgreeTerms = FindItem("BtnAgreeTerms") as KAToggleButton;
		mBtnOptIn = FindItem("BtnAgreeOptIn") as KAToggleButton;
		mAgeToggle = FindItem("BtnAgeToggle") as KAToggleButton;
		if (GameDataConfig.pInstance != null && !GameDataConfig.pInstance.GuestEnabled)
		{
			DisableGuest();
		}
		if (mBtnRegister != null)
		{
			mBtnRegister.SetState(KAUIState.DISABLED);
		}
		Reset();
	}

	public void OnAgeToggled(bool isParent)
	{
		mLabelParentsEmail.SetText(isParent ? _ParentEmailText.GetLocalizedString() : _PlayersEmailText.GetLocalizedString());
	}

	public void Reset()
	{
		if (mTxtEmail != null)
		{
			mTxtEmail.SetText("");
		}
		if (mTxtPassword != null)
		{
			mTxtPassword.SetText("");
		}
		if (mTxtEmailConfirm != null)
		{
			mTxtEmailConfirm.SetText("");
		}
		if (mTxtPasswordConfirm != null)
		{
			mTxtPasswordConfirm.SetText("");
		}
		if (mTxtPlayerFirstName != null)
		{
			mTxtPlayerFirstName.SetText("");
		}
		if (mLabelParentsEmail != null)
		{
			mLabelParentsEmail.SetVisibility(inVisible: true);
		}
		if (mBtnAgreeTerms != null)
		{
			mBtnAgreeTerms.SetChecked(isChecked: false);
		}
		if (mBtnOptIn != null)
		{
			mBtnOptIn.SetChecked(isChecked: false);
		}
		if (mAgeToggle != null)
		{
			mAgeToggle.SetChecked(isChecked: false);
			OnAgeToggled(mAgeToggle.IsChecked());
		}
	}

	protected override void Update()
	{
		if (mTxtEmail == null || mTxtEmailConfirm == null || mTxtPlayerFirstName == null || mTxtPassword == null || mTxtPasswordConfirm == null || mBtnAgreeTerms == null || mBtnRegister == null)
		{
			base.Update();
			return;
		}
		string text = "";
		string text2 = "";
		string text3 = "";
		string text4 = "";
		if ((bool)mTxtEmail && (bool)mTxtEmailConfirm && (bool)mTxtPassword && (bool)mTxtPasswordConfirm)
		{
			text = mTxtEmail.GetText().Trim();
			text2 = mTxtEmailConfirm.GetText().Trim();
			text3 = mTxtPassword.GetText();
			text4 = mTxtPasswordConfirm.GetText();
		}
		if (string.IsNullOrEmpty(mTxtPlayerFirstName.GetText()) || mTxtPlayerFirstName.GetText() == mTxtPlayerFirstName._DefaultText.GetLocalizedString() || string.IsNullOrEmpty(mTxtPassword.GetText()) || mTxtPassword.GetText() == mTxtPassword._DefaultText.GetLocalizedString() || string.IsNullOrEmpty(mTxtPasswordConfirm.GetText()) || mTxtPasswordConfirm.GetText() == mTxtPasswordConfirm._DefaultText.GetLocalizedString() || string.IsNullOrEmpty(mTxtEmail.GetText()) || mTxtEmail.GetText() == mTxtEmail._DefaultText.GetLocalizedString() || string.IsNullOrEmpty(mTxtEmailConfirm.GetText()) || mTxtEmailConfirm.GetText() == mTxtEmailConfirm._DefaultText.GetLocalizedString() || !mBtnAgreeTerms.IsChecked() || text != text2 || text3 != text4)
		{
			if (mBtnRegister.GetState() != KAUIState.DISABLED)
			{
				mBtnRegister.SetState(KAUIState.DISABLED);
			}
		}
		else if (mBtnRegister.GetState() != 0)
		{
			mBtnRegister.SetState(KAUIState.INTERACTIVE);
		}
		if (!string.IsNullOrEmpty(mTxtEmailConfirm.GetText()) && mTxtEmailMatch != null)
		{
			if (text != text2)
			{
				mTxtEmailMatch.SetText(_EmailMatchFailText._Text);
				mTxtEmailMatch.ColorBlendTo(Color.red, Color.red, 0f);
			}
			else
			{
				mTxtEmailMatch.SetText(_EmailMatchSuccessText._Text);
				mTxtEmailMatch.ColorBlendTo(Color.green, Color.green, 0f);
			}
		}
		else if (mTxtEmailMatch != null)
		{
			mTxtEmailMatch.SetText("");
		}
		if (!string.IsNullOrEmpty(mTxtPasswordConfirm.GetText()) && mTxtPasswordMatch != null)
		{
			if (text3 != text4)
			{
				mTxtPasswordMatch.SetText(_PasswordMatchFailText._Text);
				mTxtPasswordMatch.ColorBlendTo(Color.red, Color.red, 0f);
			}
			else
			{
				mTxtPasswordMatch.SetText(_PasswordMatchSuccessText._Text);
				mTxtPasswordMatch.ColorBlendTo(Color.green, Color.green, 0f);
			}
		}
		else if (mTxtPasswordMatch != null)
		{
			mTxtPasswordMatch.SetText("");
		}
		base.Update();
	}

	public void UpdateEmailLabel()
	{
		mLabelParentsEmail.SetVisibility(inVisible: true);
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		switch (inItem.name)
		{
		case "BtnClose":
			CloseDB();
			break;
		case "BtnAgeToggle":
			mAgeToggle.SetChecked(mAgeToggle.IsChecked());
			OnAgeToggled(!mAgeToggle.IsChecked());
			break;
		case "BtnRegister":
			if (UiLogin.pInstance.CheckForConnection(ConnectivityErrorLocation.LOGIN_SCENE))
			{
				SetState(KAUIState.DISABLED);
				string email = mTxtEmail.GetText().Trim();
				mTxtEmailConfirm.GetText().Trim();
				string text = mTxtPassword.GetText();
				mTxtPasswordConfirm.GetText();
				string text2 = mTxtPlayerFirstName.GetText().Trim();
				if (string.IsNullOrEmpty(text2) || text2 == mTxtPlayerFirstName._DefaultText.GetLocalizedString())
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidPlayerNameText.GetLocalizedString(), base.gameObject);
					break;
				}
				if (!mTxtEmail.IsValidText())
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidEmailText.GetLocalizedString(), base.gameObject);
					break;
				}
				if (text2.Contains(" "))
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._SpaceInPlayerNameText.GetLocalizedString(), base.gameObject);
					break;
				}
				if (string.IsNullOrEmpty(text) || text.Length < 6 || !mTxtPassword.IsValidText() || text == mTxtPlayerFirstName._DefaultText.GetLocalizedString())
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidPasswordText.GetLocalizedString(), base.gameObject);
					break;
				}
				if (!mBtnAgreeTerms.IsChecked())
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._TermsAndConditionsText.GetLocalizedString(), base.gameObject);
					break;
				}
				ShowInvalidHighlight(isVisible: false);
				KAUICursorManager.SetDefaultCursor("Loading");
				UserPolicy userPolicy = new UserPolicy();
				userPolicy.PrivacyPolicy = mBtnAgreeTerms.IsChecked();
				userPolicy.TermsAndConditions = mBtnAgreeTerms.IsChecked();
				EmailNotification emailNotification = (mBtnOptIn.IsChecked() ? EmailNotification.Optin : EmailNotification.Optout);
				if (PlayerPrefs.HasKey("GUEST_ACC_CREATED"))
				{
					WsWebService.RegisterGuest(ProductConfig.pProductName, UiLogin.GetValidGuestUserName(), (!mAgeToggle.IsChecked()) ? 1 : 14, email, text, text2, UiLogin.pLocale, userPolicy, emailNotification, OnRegisterDone, null);
				}
				else
				{
					ParentRegistrationData parentRegistrationData = new ParentRegistrationData();
					parentRegistrationData.Email = email;
					parentRegistrationData.Locale = UiLogin.pLocale;
					parentRegistrationData.Password = text;
					parentRegistrationData.ReceivesEmail = false;
					parentRegistrationData.SubscriptionID = ProductSettings.pInstance._SubscriptionID;
					parentRegistrationData.UserPolicy = userPolicy;
					parentRegistrationData.EmailNotification = emailNotification;
					parentRegistrationData.ChildList = new ChildRegistrationData[1]
					{
						new ChildRegistrationData
						{
							Age = ((!mAgeToggle.IsChecked()) ? 1 : 14),
							ChildName = text2,
							BirthDate = DateTime.MinValue,
							Gender = "",
							Password = text
						}
					};
					WsWebService.RegisterParent(ProductConfig.pProductName, parentRegistrationData, OnRegisterDone, null);
				}
			}
			AnalyticAgent.LogEvent(PlayerPrefs.HasKey("GUEST_ACC_CREATED") ? AnalyticEvent.REGISTER_GUEST : AnalyticEvent.REGISTER_USER, new Dictionary<string, object> { { "name", "clicked" } });
			break;
		case "BtnInfo":
			GameUtilities.DisplayOKMessage("PfKAUIGenericDBLg", UiLogin.GetStrings()._WhyRegisterText.GetLocalizedString(), UiLogin.GetStrings()._WhyRegisterTitleText.GetLocalizedString(), base.gameObject, "OnDBClose");
			break;
		case "AniTermsConditions":
			if (Application.isEditor || !UiLogin.pInstance.CheckForConnection(ConnectivityErrorLocation.REGISTER_SCENE))
			{
				break;
			}
			if (UtPlatform.IsMobile())
			{
				if (mUniWebView == null)
				{
					mUniWebView = base.gameObject.AddComponent<UniWebView>();
				}
				mUniWebView.SetShowToolbar(show: true);
				mUniWebView.Frame = new Rect(0f, 0f, Screen.width, Screen.height);
				mUniWebView.Load(_TermsAndConditionsURL);
				mUniWebView.Show();
			}
			else
			{
				Application.OpenURL(_TermsAndConditionsURL);
			}
			break;
		case "BtnPrivacyPolicy":
			if (Application.isEditor || !UiLogin.pInstance.CheckForConnection(ConnectivityErrorLocation.REGISTER_SCENE))
			{
				break;
			}
			if (UtPlatform.IsMobile())
			{
				if (mUniWebView == null)
				{
					mUniWebView = base.gameObject.AddComponent<UniWebView>();
				}
				mUniWebView.SetShowToolbar(show: true);
				mUniWebView.Frame = new Rect(0f, 0f, Screen.width, Screen.height);
				mUniWebView.Load(_PrivacyPolicyURL);
				mUniWebView.Show();
			}
			else
			{
				Application.OpenURL(_PrivacyPolicyURL);
			}
			break;
		}
	}

	public void OnRegisterDone(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		bool flag = false;
		if (ProductConfig.pInstance.EnableErrorLog.HasValue)
		{
			flag = ProductConfig.pInstance.EnableErrorLog.Value;
		}
		string text = "";
		if (inType != WsServiceType.REGISTER_PARENT)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (inObject != null)
			{
				RegistrationResult registrationResult = (RegistrationResult)inObject;
				if (registrationResult.Status == MembershipUserStatus.Success)
				{
					PlayerPrefs.DeleteKey("REM_USER_NAME");
					PlayerPrefs.DeleteKey("REM_PASSWORD");
					UiLogin.pInstance.pTxtEmail.SetText("");
					UiLogin.pInstance.pTxtPassword.SetText("");
					PlayerPrefs.SetInt("USER_REGISTERED_ON_DEVICE", 1);
					mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
					mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
					mKAUIGenericDB._MessageObject = base.gameObject;
					if (PlayerPrefs.HasKey("FTUE_NEWUSER"))
					{
						PlayerPrefs.SetString("FTUE_NEWUSER", mTxtPlayerFirstName.GetText().Trim());
					}
					if (PlayerPrefs.HasKey("GUEST_ACC_CREATED"))
					{
						if (pGuestChatPromptOpened)
						{
							AnalyticAgent.LogEvent(AnalyticEvent.GUEST_CHAT, new Dictionary<string, object> { { "name", "registered" } });
							pGuestChatPromptOpened = false;
						}
						PlayerPrefs.DeleteKey("GUEST_ACC_CREATED");
						UiLogin.pHasUserJustRegistered = false;
						mKAUIGenericDB.SetText(_GuestAccountConvertedText.GetLocalizedString(), interactive: false);
						mKAUIGenericDB._OKMessage = "OnGuestAccountSynched";
						AnalyticAgent.LogEvent(AnalyticEvent.REGISTER_GUEST, new Dictionary<string, object> { { "name", "success" } });
					}
					else
					{
						UiLogin.pHasUserJustRegistered = true;
						mKAUIGenericDB._OKMessage = "OnFinishAndPlay";
						mKAUIGenericDB.SetText((!mAgeToggle.IsChecked()) ? _EmailSentToParentText.GetLocalizedString() : _EmailSentText.GetLocalizedString(), interactive: false);
						AnalyticAgent.LogEvent(AnalyticEvent.REGISTER_USER, new Dictionary<string, object> { { "name", "success" } });
					}
					mKAUIGenericDB.SetTitle(_RegistrationTitleText.GetLocalizedString());
					KAUI.SetExclusive(mKAUIGenericDB);
					break;
				}
				switch (registrationResult.Status)
				{
				case MembershipUserStatus.InvalidUserName:
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidPlayerNameText.GetLocalizedString(), base.gameObject);
					break;
				case MembershipUserStatus.InvalidPassword:
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidPasswordText.GetLocalizedString(), base.gameObject);
					break;
				case MembershipUserStatus.InvalidEmail:
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidEmailText.GetLocalizedString(), base.gameObject);
					break;
				case MembershipUserStatus.InvalidDOB:
					UiLogin.ShowDB(UiLogin.GetStrings()._InvalidDOBText.GetLocalizedString(), base.gameObject);
					break;
				case MembershipUserStatus.DuplicateUserName:
					UiLogin.ShowDB(UiLogin.GetStrings()._DuplicateUserNameText.GetLocalizedString(), base.gameObject);
					break;
				case MembershipUserStatus.DuplicateEmail:
					UiLogin.pShowRegistrationPage = false;
					UiLogin._RewardForRegistering = 0;
					if (UiLogin.pInstance.CheckForConnection(ConnectivityErrorLocation.LOGIN_SCENE))
					{
						WsWebService.AuthenticateUser(mTxtPlayerFirstName.GetText(), mTxtPassword.GetText(), ServiceLoginEventHandler, null);
					}
					break;
				default:
					if (flag)
					{
						string[] array = new string[6];
						int num = (int)inType;
						array[0] = num.ToString();
						array[1] = ":";
						num = (int)inEvent;
						array[2] = num.ToString();
						array[3] = ":";
						num = (int)registrationResult.Status;
						array[4] = num.ToString();
						array[5] = " ";
						text = string.Concat(array);
					}
					Debug.Log("ERROR while Registering");
					UiLogin.ShowDB(UiLogin.GetStrings()._ErrorText.GetLocalizedString() + " " + text, base.gameObject);
					break;
				}
			}
			else
			{
				SetState(KAUIState.INTERACTIVE);
				Debug.LogError("WEB SERVICE CALL Parent Registration returned empty!!!");
			}
			break;
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			SetState(KAUIState.INTERACTIVE);
			Debug.LogError("WEB SERVICE CALL RegisterParent FAILED!!!");
			break;
		}
	}

	public void OnFinishAndPlay()
	{
		KAUI.RemoveExclusive(mKAUIGenericDB);
		mKAUIGenericDB.Destroy();
		UiLogin.pInstance.LoginParent(mTxtPlayerFirstName.GetText(), mTxtPassword.GetText(), isPostRegistrationLogin: true);
		SetVisibility(inVisible: false);
	}

	public void OnGuestAccountSynched()
	{
		mKAUIGenericDB._OKMessage = "OnFinishAndPlay";
		if (!mAgeToggle.IsChecked())
		{
			mKAUIGenericDB.SetText(_EmailSentToParentText.GetLocalizedString(), interactive: false);
		}
		else
		{
			mKAUIGenericDB.SetText(_EmailSentText.GetLocalizedString(), interactive: false);
		}
	}

	public void OnDBClose()
	{
		SetState(KAUIState.INTERACTIVE);
	}

	public void OnEnable()
	{
		SetState(KAUIState.INTERACTIVE);
		Reset();
	}

	public void ServiceLoginEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.AUTHENTICATE_USER)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				if (!(bool)inObject || UiLogin.pInstance == null)
				{
					UiLogin.ShowDB(UiLogin.GetStrings()._DuplicateEmailText.GetLocalizedString(), base.gameObject);
					break;
				}
				UiLogin.pUserName = mTxtPlayerFirstName.GetText();
				UiLogin.pPassword = mTxtPassword.GetText();
				UiLogin.pInstance.LoginParent(UiLogin.pUserName, UiLogin.pPassword);
			}
			break;
		case WsServiceEvent.ERROR:
			SetState(KAUIState.INTERACTIVE);
			Debug.LogError(" User Authenticated failed ====");
			break;
		}
	}

	public void OnParentLogIn()
	{
		if (KAUI._GlobalExclusiveUI != null)
		{
			KAUI.SetExclusive(null, null);
		}
		SetState(KAUIState.INTERACTIVE);
		SetVisibility(inVisible: false);
	}

	private void ShowInvalidHighlight(bool isVisible)
	{
		mEmailHighlight.SetVisibility(isVisible);
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		Reset();
	}

	public void DisableGuest()
	{
		KAWidget kAWidget = FindItem("BtnGuest");
		if (kAWidget != null)
		{
			kAWidget.SetState(KAUIState.DISABLED);
			kAWidget.SetVisibility(inVisible: false);
		}
	}

	public void CloseDB()
	{
		SetVisibility(inVisible: false);
		pGuestChatPromptOpened = false;
		UiLogin.pShowRegistrationPage = false;
		UiLogin.pInstance.OnDBClose();
	}
}
