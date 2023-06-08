using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class AllPaymentsController : MonoBehaviour
{
	public PaymentListView paymenListView;

	public GameObject dropDownContainer;

	public DropDownController dropDownController;

	public GameObject back;

	public GameObject containerBtns;

	private XsollaCountries _countries;

	public ImageLoader imageLoader;

	private XsollaUtils utilsLink;

	private List<ShopPaymentBtnController> listBtns;

	private void Start()
	{
		if (!utilsLink.GetUser().IsAllowChangeCountry())
		{
			dropDownController.gameObject.SetActive(value: false);
		}
	}

	public XsollaCountries GetCountrysList()
	{
		return _countries;
	}

	public void SetPaymentMethods(XsollaPaymentMethods paymentMethods)
	{
		paymenListView.SetPaymentMethods(paymentMethods);
		SetUpNavButtons();
	}

	public void SetPaymentMethods(List<XsollaPaymentMethod> pList)
	{
		if (listBtns == null)
		{
			listBtns = new List<ShopPaymentBtnController>();
		}
		else
		{
			ClearBtnContainer();
		}
		foreach (XsollaPaymentMethod item in pList.FindAll((XsollaPaymentMethod method) => method.isVisible))
		{
			CreatePaymentBtn(item);
		}
		SetUpNavButtons();
	}

	private void CreatePaymentBtn(XsollaPaymentMethod pMethod)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ShopPaymentBtn")) as GameObject;
		gameObject.transform.SetParent(containerBtns.transform);
		ShopPaymentBtnController controller = gameObject.GetComponent<ShopPaymentBtnController>();
		listBtns.Add(controller);
		controller.setMethod(pMethod);
		controller.setIcon(imageLoader);
		controller._btn.onClick.AddListener(delegate
		{
			OnChoosePaymentMethod(controller.getMethod().id);
		});
	}

	private void ClearBtnContainer()
	{
		listBtns.ForEach(delegate(ShopPaymentBtnController btn)
		{
			UnityEngine.Object.Destroy(btn._self);
		});
		listBtns.Clear();
	}

	public void Sort(string pInput)
	{
		listBtns.ForEach(delegate(ShopPaymentBtnController btn)
		{
			btn._self.SetActive(btn.getMethod().name.ToLower().StartsWith(pInput.ToLower()));
		});
	}

	public void OnChoosePaymentMethod(long paymentMethodId)
	{
		GetComponentInParent<PaymentListScreenController>().ChoosePaymentMethod(paymentMethodId);
	}

	public void InitScreen(XsollaUtils pUtils)
	{
		utilsLink = pUtils;
		back.GetComponent<Text>().text = utilsLink.GetTranslations().Get(XsollaTranslations.BACK_TO_LIST);
	}

	public void SetCountries(string currentCountryIso, XsollaCountries countries)
	{
		if (utilsLink.GetUser().IsAllowChangeCountry())
		{
			_countries = countries;
			dropDownController.gameObject.SetActive(value: true);
			dropDownController.SetParentForScroll(base.gameObject.transform.parent.parent.parent);
			DropDownController obj = dropDownController;
			obj.OnItemSelected = (Action<int, string>)Delegate.Combine(obj.OnItemSelected, (Action<int, string>)delegate(int position, string title)
			{
				OnChangeCountry(position);
			});
			dropDownController.SetData(countries.GetTitleList(), _countries.GetItemByKey(currentCountryIso).name);
		}
	}

	public void UpdatePaymentMethods(XsollaPaymentMethods paymentMethods)
	{
		paymenListView.SetPaymentMethods(paymentMethods);
	}

	public void OnChangeCountry(string key)
	{
		if (utilsLink.GetUser().IsAllowChangeCountry())
		{
			XsollaCountry itemByKey = _countries.GetItemByKey(key);
			dropDownContainer.SetActive(value: false);
			GetComponentInParent<PaymentListScreenController>().ChangeCountry(itemByKey.iso);
		}
	}

	public void OnChangeCountry(int position)
	{
		if (utilsLink.GetUser().IsAllowChangeCountry())
		{
			XsollaCountry xsollaCountry = _countries.GetItemsList()[position];
			dropDownContainer.SetActive(value: false);
			GetComponentInParent<PaymentListScreenController>().ChangeCountry(xsollaCountry.iso);
		}
	}

	public void SetUpNavButtons()
	{
		back.GetComponent<Button>().onClick.AddListener(delegate
		{
			GetComponentInParent<PaymentListScreenController>().OpenQuickPayments();
		});
	}
}
