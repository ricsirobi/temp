using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SubscriptionBtnController : MonoBehaviour
{
	private XsollaSubscription _sub;

	public Text _subName;

	public Text _bonusText;

	public GameObject _desc;

	public Text _oldPrice;

	public Text _newPrice;

	public Text _period;

	public Button _btnSub;

	public GameObject _offerText;

	public Image _offerImagePanel;

	public void InitBtn(XsollaSubscription pSub, XsollaTranslations pTranslation)
	{
		_sub = pSub;
		_subName.text = _sub.GetName();
		if (_sub.description != null)
		{
			_desc.GetComponent<Text>().text = _sub.description;
			_desc.SetActive(value: true);
		}
		else
		{
			_desc.SetActive(value: false);
		}
		if (!_sub.IsSpecial())
		{
			_newPrice.text = CurrencyFormatter.FormatPrice(_sub.chargeCurrency, _sub.chargeAmount.ToString());
		}
		else
		{
			_oldPrice.text = CurrencyFormatter.FormatPrice(_sub.chargeCurrency, _sub.chargeAmountWithoutDiscount.ToString());
			_newPrice.text = CurrencyFormatter.FormatPrice(_sub.chargeCurrency, _sub.chargeAmount.ToString());
			_offerText.GetComponent<Text>().text = pTranslation.Get("option_offer");
		}
		_offerText.SetActive(_sub.IsSpecial() ? true : false);
		_offerImagePanel.enabled = (_sub.IsSpecial() ? true : false);
		_period.text = _sub.GetPeriodString("Every");
		_btnSub.onClick.AddListener(delegate
		{
			Dictionary<string, object> items = new Dictionary<string, object> { { "id_package", _sub.id } };
			base.gameObject.GetComponentInParent<XsollaPaystationController>().ChooseItem(items, isVirtualPayment: false);
		});
	}
}
