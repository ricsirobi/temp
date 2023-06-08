using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class PromoCodeController : MonoBehaviour
{
	public GameObject _promoBtn;

	public GameObject _promoContainerInputApplyCode;

	public Text _promoDesc;

	public Button _promoCodeApply;

	public InputField _inputField;

	public GameObject _acceptBlock;

	public Text _acceptLable;

	public GameObject _panelError;

	public Text _errorLable;

	public void InitScreen(XsollaTranslations pTranslation, XsollaFormElement pForm)
	{
		_promoBtn.GetComponent<Text>().text = pTranslation.Get("coupon_control_header");
		_promoDesc.text = pTranslation.Get("coupon_control_hint");
		_promoCodeApply.gameObject.GetComponentInChildren<Text>().text = pTranslation.Get("coupon_control_apply");
		_acceptLable.text = pTranslation.Get("coupon_control_accepted");
		_inputField.onValueChanged.AddListener(delegate
		{
			if (_panelError.activeSelf)
			{
				_panelError.SetActive(value: false);
			}
		});
		if (pForm.IsReadOnly() && !pForm.GetValue().Equals(""))
		{
			_inputField.text = pForm.GetValue();
			ApplySuccessful();
		}
	}

	public void SetError(XsollaError pError)
	{
		_errorLable.text = pError.errorMessage;
		_panelError.SetActive(value: true);
	}

	public void btnClick()
	{
		_promoContainerInputApplyCode.SetActive(!_promoContainerInputApplyCode.activeSelf);
	}

	public void ApplySuccessful()
	{
		_inputField.interactable = false;
		_promoCodeApply.gameObject.SetActive(value: false);
		_acceptBlock.SetActive(value: true);
	}
}
