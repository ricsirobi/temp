using System;
using UnityEngine;
using UnityEngine.Events;

public class UiUserAgreement : KAUI
{
	public string _TermsAndConditionsURL = "http://www.schoolofdragons.com/Help/Siteterms";

	public string _PrivacyPolicyURL = "https://policy-portal.truste.com/core/privacy-policy/Knowledge-Adventure/4158c489-ef0b-439f-b09f-827a1e18af03";

	private KAToggleButton mBtnAgreeTerms;

	private KAToggleButton mOver13Btn;

	private KAWidget mOkBtn;

	private UnityAction<bool> mActionOnSubmit;

	private bool mUserOver13;

	protected override void Start()
	{
		base.Start();
		mBtnAgreeTerms = FindItem("BtnAgreeTerms") as KAToggleButton;
		mOkBtn = FindItem("OKBtn");
		mOver13Btn = FindItem("BtnOver13") as KAToggleButton;
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mOkBtn)
		{
			mUserOver13 = false;
			if (mOver13Btn != null)
			{
				mUserOver13 = mOver13Btn.IsChecked();
			}
			mActionOnSubmit(mUserOver13);
			mActionOnSubmit = null;
			KAUI.RemoveExclusive(this);
			UnityEngine.Object.Destroy(base.gameObject);
			AnalyticAgent.LogFTUEEvent(FTUEEvent.USER_AGREEMENT_ACCEPTED);
			return;
		}
		if (item == mBtnAgreeTerms)
		{
			mOkBtn.SetState((!mBtnAgreeTerms.IsChecked()) ? KAUIState.DISABLED : KAUIState.INTERACTIVE);
			return;
		}
		string text = item.name;
		if (!(text == "AniTermsConditions"))
		{
			if (text == "BtnPrivacyPolicy" && !Application.isEditor && UiLogin.pInstance.CheckForConnection(ConnectivityErrorLocation.PARENT_LOGIN))
			{
				Application.OpenURL(_PrivacyPolicyURL);
			}
		}
		else if (!Application.isEditor && UiLogin.pInstance.CheckForConnection(ConnectivityErrorLocation.PARENT_LOGIN))
		{
			Application.OpenURL(_TermsAndConditionsURL);
		}
	}

	public void AddAction(UnityAction<bool> action)
	{
		mActionOnSubmit = (UnityAction<bool>)Delegate.Combine(mActionOnSubmit, action);
	}
}
