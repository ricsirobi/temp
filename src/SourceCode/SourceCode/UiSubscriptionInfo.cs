using UnityEngine;

public class UiSubscriptionInfo : KAUI
{
	public string _TermsAndConditionsURL = "http://www.schoolofdragons.com/help/siteterms";

	public string _PrivacyPolicyURL = "http://www.schoolofdragons.com/help/privacypolicy";

	protected override void Awake()
	{
		base.Awake();
		KAUI.SetExclusive(this);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		switch (inWidget.name)
		{
		case "BtnClose":
			KAUI.RemoveExclusive(this);
			Object.Destroy(base.gameObject);
			break;
		case "BtnTermsConditions":
			if (!Application.isEditor && UtUtilities.IsConnectedToWWW())
			{
				Application.OpenURL(_TermsAndConditionsURL);
			}
			break;
		case "BtnPrivacyPolicy":
			if (!Application.isEditor && UtUtilities.IsConnectedToWWW())
			{
				Application.OpenURL(_PrivacyPolicyURL);
			}
			break;
		}
	}
}
