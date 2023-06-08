using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class RedeemCouponViewController : MonoBehaviour
{
	public Text _title;

	public GameObject _errorPanel;

	public Text _coupounNotif;

	public InputField _inputField;

	public Text _nameInputField;

	public Text _inputFieldExample;

	public Button _btnApply;

	private XsollaUtils _utiliLink;

	public void InitScreen(XsollaUtils pUtils)
	{
		_utiliLink = pUtils;
		_title.text = _utiliLink.GetTranslations().Get(XsollaTranslations.COUPON_PAGE_TITLE);
		_coupounNotif.text = _utiliLink.GetTranslations().Get(XsollaTranslations.COUPON_DESCRIPTION);
		_nameInputField.text = _utiliLink.GetTranslations().Get(XsollaTranslations.COUPON_CODE_TITLE);
		_inputFieldExample.text = _utiliLink.GetTranslations().Get(XsollaTranslations.COUPON_CODE_EXAMPLE);
		_btnApply.GetComponentInChildren<Text>().text = _utiliLink.GetTranslations().Get(XsollaTranslations.COUPON_CONTROL_APPLY);
	}

	public void ShowError(string pErrMsg)
	{
		_errorPanel.GetComponentInChildren<Text>().text = pErrMsg;
		_errorPanel.SetActive(value: true);
	}

	public void HideError()
	{
		_errorPanel.SetActive(value: false);
	}

	public string GetCode()
	{
		return _inputField.text;
	}
}
