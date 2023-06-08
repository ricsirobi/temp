using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class PaymentListScreenController : ScreenBaseConroller<object>
{
	public Text titleText;

	public QuickPaymentsController quickController;

	public AllPaymentsController allController;

	public SavedPayController savedPayController;

	public ImageLoader imageLoader;

	public GameObject screenHider;

	private string startCountryIso;

	private XsollaPaymentMethods _paymentMethods;

	private XsollaSavedPaymentMethods _savedPaymetnsMethods;

	private XsollaCountries _countries;

	private XsollaUtils utilsLink;

	public bool isPaymentMethodsLoaded()
	{
		return _paymentMethods != null;
	}

	public override void InitScreen(XsollaTranslations translations, object model)
	{
		throw new NotImplementedException();
	}

	public void InitScreen(XsollaUtils utils)
	{
		utilsLink = utils;
		titleText.text = utils.GetTranslations().Get(XsollaTranslations.PAYMENT_METHODS_PAGE_TITLE);
		startCountryIso = utils.GetUser().GetCountryIso();
		savedPayController.InitScreen(utilsLink);
		quickController.InitScreen(utilsLink);
		allController.InitScreen(utilsLink);
	}

	public void SetPaymentsMethods(XsollaPaymentMethods paymentMethods)
	{
		_paymentMethods = paymentMethods;
		quickController.SetQuickMethods(_paymentMethods.GetListOnType(XsollaPaymentMethod.TypePayment.QUICK));
		quickController.SetAllMethods(_paymentMethods.GetListOnType(XsollaPaymentMethod.TypePayment.REGULAR));
		allController.SetPaymentMethods(_paymentMethods.GetListOnType());
	}

	public void SetCountries(string pStartCountry, XsollaCountries countries, XsollaUtils pUtils)
	{
		_countries = countries;
		if (pUtils.GetUser().IsAllowChangeCountry())
		{
			allController.SetCountries((pStartCountry != "") ? pStartCountry : startCountryIso, _countries);
		}
	}

	public void SetSavedPaymentsMethods(XsollaSavedPaymentMethods paymentMethods)
	{
		_savedPaymetnsMethods = paymentMethods;
		savedPayController.SetSavedMethods(_savedPaymetnsMethods);
	}

	public void OpenPayments()
	{
		if (_savedPaymetnsMethods != null && _savedPaymetnsMethods.Count != 0)
		{
			OpenSavedMethod();
		}
		else
		{
			OpenQuickPayments();
		}
	}

	public void OpenQuickPayments()
	{
		savedPayController.gameObject.SetActive(value: false);
		screenHider.SetActive(value: false);
		allController.gameObject.SetActive(value: false);
		quickController.gameObject.SetActive(value: true);
	}

	public void OpenAllPayments()
	{
		if (utilsLink.GetUser().IsAllowChangeCountry() && allController.GetCountrysList() == null)
		{
			GetComponentInParent<XsollaPaystationController>().LoadCountries();
		}
		savedPayController.gameObject.SetActive(value: false);
		screenHider.SetActive(value: false);
		quickController.gameObject.SetActive(value: false);
		allController.gameObject.SetActive(value: true);
	}

	public void OpenSavedMethod()
	{
		screenHider.SetActive(value: true);
		quickController.gameObject.SetActive(value: false);
		allController.gameObject.SetActive(value: false);
		savedPayController.gameObject.SetActive(value: true);
	}

	public void ChoosePaymentMethod(long paymentMethodId)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("pid", paymentMethodId);
		dictionary.Add("hidden", "out");
		GetComponentInParent<XsollaPaystationController>().ChoosePaymentMethod(dictionary);
	}

	public void ChangeCountry(string countryIso)
	{
		GetComponentInParent<XsollaPaystationController>().UpdateCountries(countryIso);
	}

	public bool IsAllPayments()
	{
		return _paymentMethods != null;
	}

	public bool IsSavedPayments()
	{
		return _savedPaymetnsMethods != null;
	}
}
