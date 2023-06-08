using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class QuickPaymentsController : MonoBehaviour
{
	public ImageLoader imageLoader;

	public GameObject[] popular;

	public GameObject quickPanel;

	public GameObject recPanel;

	public GameObject showMore;

	public GameObject back;

	public GameObject title;

	private XsollaUtils utilsLink;

	private int countQuickBtn = 3;

	private int countPopBtn = 6;

	private List<QuickPaymentBtnController> listQuickBtns;

	private List<ShopPaymentBtnController> listPopularBtns;

	public void DrawLayout(XsollaPaymentMethods paymentMethods)
	{
	}

	public void InitScreen(XsollaUtils utils)
	{
		utilsLink = utils;
		showMore.GetComponent<Text>().text = utilsLink.GetTranslations().Get(XsollaTranslations.PAYMENT_LIST_SHOW_MORE);
		back.GetComponent<Text>().enabled = false;
		back.GetComponent<Button>().enabled = false;
	}

	public void SetQuickMethods(List<XsollaPaymentMethod> pQuickPayments)
	{
		if (pQuickPayments == null)
		{
			return;
		}
		if (listQuickBtns == null)
		{
			listQuickBtns = new List<QuickPaymentBtnController>();
		}
		else
		{
			ClearBtnQuickContainer();
		}
		for (int i = 0; i < countQuickBtn; i++)
		{
			if (i < pQuickPayments.Count)
			{
				CreateQuickBtn(pQuickPayments[i]);
			}
			else
			{
				CreateQuickBtn(null);
			}
		}
	}

	private void ClearBtnQuickContainer()
	{
		listQuickBtns.ForEach(delegate(QuickPaymentBtnController btn)
		{
			Object.Destroy(btn._self);
		});
		listQuickBtns.Clear();
	}

	private void CreateQuickBtn(XsollaPaymentMethod pMethod)
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/SimpleView/_PaymentFormElements/QuickPaymentBtn")) as GameObject;
		gameObject.transform.SetParent(quickPanel.transform);
		QuickPaymentBtnController controller = gameObject.GetComponent<QuickPaymentBtnController>();
		listQuickBtns.Add(controller);
		if (pMethod == null)
		{
			controller.Hide();
			return;
		}
		controller.setMethod(pMethod);
		controller.setLable(pMethod.GetName());
		controller.setIcon(pMethod.id, imageLoader);
		controller._btnMethod.onClick.AddListener(delegate
		{
			OnChoosePaymentMethod(controller.getMethod().id);
		});
	}

	public void SetAllMethods(List<XsollaPaymentMethod> paymentMethods)
	{
		if (paymentMethods != null)
		{
			if (listPopularBtns == null)
			{
				listPopularBtns = new List<ShopPaymentBtnController>();
			}
			else
			{
				ClearBtnPopularConatiner();
			}
			for (int i = 0; i < countPopBtn; i++)
			{
				if (i < paymentMethods.Count)
				{
					CreatePopularBtn(paymentMethods[i]);
				}
			}
		}
		SetUpNavButtons();
	}

	private void CreatePopularBtn(XsollaPaymentMethod pMethod)
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ShopPaymentBtn")) as GameObject;
		gameObject.transform.SetParent(recPanel.transform);
		ShopPaymentBtnController controller = gameObject.GetComponent<ShopPaymentBtnController>();
		if (!pMethod.isVisible)
		{
			controller.Hide();
			return;
		}
		listPopularBtns.Add(controller);
		controller.setMethod(pMethod);
		controller.setIcon(imageLoader);
		controller._btn.onClick.AddListener(delegate
		{
			OnChoosePaymentMethod(controller.getMethod().id);
		});
	}

	private void ClearBtnPopularConatiner()
	{
		listPopularBtns.ForEach(delegate(ShopPaymentBtnController btn)
		{
			Object.Destroy(btn._self);
		});
		listPopularBtns.Clear();
	}

	public void SetUpNavButtons()
	{
		showMore.GetComponent<Button>().onClick.AddListener(delegate
		{
			GetComponentInParent<PaymentListScreenController>().OpenAllPayments();
		});
	}

	public void OnChoosePaymentMethod(long paymentMethodId)
	{
		GetComponentInParent<PaymentListScreenController>().ChoosePaymentMethod(paymentMethodId);
	}

	public void ShowMorePaymentMethod()
	{
	}

	public void Back()
	{
	}
}
