using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SavedPayController : MonoBehaviour
{
	public ImageLoader imageLoader;

	public GameObject title;

	public GameObject methodsGrid;

	public GameObject showQuickPaymentMethods;

	public GameObject back;

	public GameObject self;

	private XsollaUtils utilsLink;

	private List<SavedMethodBtnController> listBtns;

	public void InitScreen(XsollaUtils utils)
	{
		utilsLink = utils;
		title.GetComponent<Text>().text = utilsLink.GetTranslations().Get(XsollaTranslations.SAVEDMETHOD_PAGE_TITLE);
		showQuickPaymentMethods.GetComponent<Text>().text = utilsLink.GetTranslations().Get(XsollaTranslations.PAYMENT_LIST_SHOW_QUICK);
		back.GetComponent<Text>().text = utilsLink.GetTranslations().Get(XsollaTranslations.BACK_TO_SPECIALS);
	}

	public void SetSavedMethods(XsollaSavedPaymentMethods pMethods)
	{
		if (pMethods != null)
		{
			if (listBtns == null)
			{
				listBtns = new List<SavedMethodBtnController>();
			}
			else
			{
				ClearBtnsContainer();
			}
			foreach (XsollaSavedPaymentMethod item in pMethods.GetItemList())
			{
				CreateMethodBtn(item);
			}
			self.gameObject.SetActive(value: true);
			SetUpNavButtons();
		}
		else
		{
			self.gameObject.SetActive(value: false);
		}
	}

	private void ClearBtnsContainer()
	{
		listBtns.ForEach(delegate(SavedMethodBtnController btn)
		{
			Object.Destroy(btn._self);
		});
		listBtns.Clear();
	}

	private void CreateMethodBtn(XsollaSavedPaymentMethod pMethod)
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/SimpleView/_PaymentFormElements/SavedMethodBtn")) as GameObject;
		gameObject.transform.SetParent(methodsGrid.transform);
		SavedMethodBtnController controller = gameObject.GetComponent<SavedMethodBtnController>();
		listBtns.Add(controller);
		controller.setMethod(pMethod);
		controller.setNameMethod(pMethod.GetName());
		controller.setNameType(pMethod.GetPsName());
		imageLoader.LoadImage(controller._iconMethod, pMethod.GetImageUrl());
		controller._btnMethod.onClick.AddListener(delegate
		{
			onMethodClick(controller.getMethod());
		});
	}

	private void onMethodClick(XsollaSavedPaymentMethod pMethod)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("saved_method_id", pMethod.GetKey());
		dictionary.Add("pid", pMethod.GetPid());
		dictionary.Add("paymentWithSavedMethod", 1);
		dictionary.Add("paymentSid", pMethod.GetFormSid());
		dictionary.Add("userInitialCurrency", pMethod.GetCurrency());
		GetComponentInParent<XsollaPaystationController>().ChoosePaymentMethod(dictionary);
	}

	public void SetUpNavButtons()
	{
		showQuickPaymentMethods.GetComponent<Button>().onClick.AddListener(delegate
		{
			GetComponentInParent<PaymentListScreenController>().OpenQuickPayments();
		});
		back.GetComponent<Button>().onClick.AddListener(delegate
		{
			GetComponentInParent<XsollaPaystationController>().LoadShop();
		});
	}
}
