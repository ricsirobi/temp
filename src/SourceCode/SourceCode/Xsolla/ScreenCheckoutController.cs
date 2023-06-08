using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class ScreenCheckoutController : MonoBehaviour
{
	public Button back;

	public PaymentFormController paymentForm;

	public RightTowerController tower;

	private bool isPrevStepPaymentList = true;

	public void InitScreen(XsollaUtils utils, XsollaForm form)
	{
		XsollaTranslations translations = utils.GetTranslations();
		Resizer.ResizeToParrent(base.gameObject);
		bool flag = utils.GetPurchase() == null;
		if (flag || !utils.GetPurchase().IsPurchase() || !utils.GetPurchase().IsPaymentSystem())
		{
			if (!flag)
			{
				isPrevStepPaymentList = !utils.GetPurchase().IsPaymentSystem();
			}
			PaymentFormController paymentFormController = paymentForm;
			paymentFormController.OnClickBack = (PaymentFormController.BackButtonHandler)Delegate.Combine(paymentFormController.OnClickBack, (PaymentFormController.BackButtonHandler)delegate
			{
				Back();
			});
		}
		paymentForm.InitView(translations, form);
		if (form.GetSummary() != null)
		{
			tower.InitView(translations, form.GetSummary());
		}
		else
		{
			tower.gameObject.SetActive(value: false);
		}
	}

	public void Back()
	{
		if (isPrevStepPaymentList)
		{
			GetComponentInParent<XsollaPaystationController>().LoadQuickPayment();
		}
		else
		{
			GetComponentInParent<XsollaPaystationController>().LoadShop();
		}
	}
}
