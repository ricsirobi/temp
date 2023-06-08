using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class PaymentFormController : ScreenBaseConroller<object>
{
	public delegate void BackButtonHandler();

	public BackButtonHandler OnClickBack;

	public LinearLayout layout;

	public GameObject footer;

	private XsollaForm _form;

	private bool isMaestro;

	private List<ValidatorInputField> validators;

	private bool isDivCreated;

	private const string PrefabInfo = "Prefabs/SimpleView/_ScreenCheckout/InfoPlate";

	public override void InitScreen(XsollaTranslations translations, object model)
	{
		throw new NotImplementedException();
	}

	public void InitView(XsollaTranslations pTranslations, XsollaForm form)
	{
		_form = form;
		if (form.GetCurrentCommand() == XsollaForm.CurrentCommand.CHECKOUT && form.GetSkipChekout())
		{
			string checkoutToken = _form.GetCheckoutToken();
			bool flag = checkoutToken != null && !"".Equals(checkoutToken) && !"null".Equals(checkoutToken) && !"false".Equals(checkoutToken);
			if (flag)
			{
				OnClickPay(flag);
				return;
			}
		}
		string titleText = new Regex("{{.*?}}").Replace(pTranslations.Get(XsollaTranslations.PAYMENT_PAGE_TITLE_VIA), form.GetTitle(), 1);
		layout.AddObject(GetTitle(titleText));
		layout.AddObject(GetError(form.GetError()));
		layout.AddObject(GetInfo(form.GetMessage()));
		if (form.GetVisible().Count > 0)
		{
			GameObject formView = GetFormView(form, pTranslations);
			layout.AddObject(formView);
		}
		if (form.GetAccountXsolla() != null && !"".Equals(form.GetAccountXsolla()) && !"null".Equals(form.GetAccountXsolla()))
		{
			layout.AddObject(GetTwoTextPlate("Xsolla number", form.GetAccountXsolla()));
		}
		if (form.GetAccount() != null && !"".Equals(form.GetAccount()) && !"null".Equals(form.GetAccount()))
		{
			layout.AddObject(GetTwoTextPlate("2pay number", form.GetAccount()));
		}
		if (form.IsValidPaymentSystem())
		{
			layout.AddObject(GetTextPlate(pTranslations.Get(XsollaTranslations.FORM_CC_EULA)));
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(footer);
		Text[] componentsInChildren = gameObject.GetComponentsInChildren<Text>();
		string nextStepString = form.GetNextStepString();
		componentsInChildren[2].text = nextStepString;
		Button[] componentsInChildren2 = gameObject.GetComponentsInChildren<Button>();
		if (OnClickBack != null)
		{
			componentsInChildren2[0].onClick.AddListener(delegate
			{
				OnBack();
			});
		}
		else
		{
			componentsInChildren2[0].gameObject.SetActive(value: false);
		}
		if (form.GetCurrentCommand() == XsollaForm.CurrentCommand.ACCOUNT || form.GetCurrentCommand() == XsollaForm.CurrentCommand.CREATE || form.GetCurrentCommand() == XsollaForm.CurrentCommand.CHECKOUT)
		{
			componentsInChildren[1].text = "";
			RectTransform component = componentsInChildren2[1].GetComponent<RectTransform>();
			Vector2 anchorMin = component.anchorMin;
			anchorMin.x -= (component.anchorMax.x - anchorMin.x) / 2f;
			component.anchorMin = anchorMin;
		}
		else
		{
			componentsInChildren[1].text = pTranslations.Get(XsollaTranslations.TOTAL) + " " + form.GetSumTotal();
		}
		layout.AddObject(gameObject);
		layout.Invalidate();
		if (!"".Equals(nextStepString) && form.GetCurrentCommand() != XsollaForm.CurrentCommand.ACCOUNT)
		{
			string checkoutToken2 = _form.GetCheckoutToken();
			bool isLinkRequired = checkoutToken2 != null && !"".Equals(checkoutToken2) && !"null".Equals(checkoutToken2) && !"false".Equals(checkoutToken2);
			string link = "https://secure.xsolla.com/pages/checkout/?token=" + _form.GetCheckoutToken();
			if (isLinkRequired && Application.platform == RuntimePlatform.WebGLPlayer)
			{
				RectTransform component2 = componentsInChildren2[1].GetComponent<RectTransform>();
				int width = (int)(component2.rect.xMax - component2.rect.xMin);
				int num = (int)(component2.rect.yMax - component2.rect.yMin);
				num *= 8;
				Vector3[] array = new Vector3[4];
				component2.GetWorldCorners(array);
				int xPos = (int)array[0].x;
				int num2 = (int)array[0].y;
				num2 /= 2;
				CreateLinkButtonWebGl(xPos, num2, width, num, link, "CardPaymeentForm", "Next");
				componentsInChildren2[1].onClick.AddListener(delegate
				{
					OnClickPay(isLinkRequired: false);
				});
			}
			else
			{
				componentsInChildren2[1].onClick.AddListener(delegate
				{
					OnClickPay(isLinkRequired);
				});
			}
		}
		else
		{
			componentsInChildren2[1].gameObject.SetActive(value: false);
		}
	}

	public GameObject GetFormView(XsollaForm xsollaForm, XsollaTranslations translations)
	{
		if (xsollaForm.GetCurrentCommand() == XsollaForm.CurrentCommand.FORM && xsollaForm.IsCreditCard())
		{
			return GetCardViewWeb(xsollaForm, translations);
		}
		FormElementAdapter component = GetComponent<FormElementAdapter>();
		component.SetForm(xsollaForm, translations);
		GameObject list = GetList(component);
		list.GetComponent<ListView>().DrawList(GetComponent<RectTransform>());
		return list;
	}

	public GameObject GetCardViewWeb(XsollaForm xsollaForm, XsollaTranslations translations)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenCheckout/CardViewLayoutWeb")) as GameObject;
		InputField[] componentsInChildren = gameObject.GetComponentsInChildren<InputField>();
		validators = new List<ValidatorInputField>();
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			XsollaFormElement element = null;
			string text = "Invalid";
			InputField input = componentsInChildren[num];
			ValidatorInputField componentInChildren = input.GetComponentInChildren<ValidatorInputField>();
			switch (num)
			{
			case 5:
			{
				element = xsollaForm.GetItem("card_number");
				text = translations.Get(XsollaTranslations.VALIDATION_MESSAGE_CARDNUMBER);
				CCEditText component = input.GetComponent<CCEditText>();
				isMaestro = component.IsMaestro();
				componentInChildren.AddValidator(new ValidatorEmpty(text));
				componentInChildren.AddValidator(new ValidatorCreditCard(text));
				break;
			}
			case 4:
				element = xsollaForm.GetItem("card_month");
				text = translations.Get(XsollaTranslations.VALIDATION_MESSAGE_CARD_MONTH);
				componentInChildren.AddValidator(new ValidatorEmpty(text));
				componentInChildren.AddValidator(new ValidatorMonth(text));
				break;
			case 3:
				element = xsollaForm.GetItem("card_year");
				text = translations.Get(XsollaTranslations.VALIDATION_MESSAGE_CARD_YEAR);
				componentInChildren.AddValidator(new ValidatorEmpty(text));
				componentInChildren.AddValidator(new ValidatorYear(text));
				break;
			case 2:
				element = xsollaForm.GetItem("zip");
				text = translations.Get(XsollaTranslations.VALIDATION_MESSAGE_REQUIRED);
				componentInChildren.AddValidator(new ValidatorEmpty(text));
				break;
			case 1:
				element = xsollaForm.GetItem("cardholdername");
				text = translations.Get(XsollaTranslations.VALIDATION_MESSAGE_REQUIRED);
				componentInChildren.AddValidator(new ValidatorEmpty(text));
				break;
			case 0:
				element = xsollaForm.GetItem("cvv");
				text = translations.Get(XsollaTranslations.VALIDATION_MESSAGE_CVV);
				componentInChildren.AddValidator(new ValidatorEmpty(text));
				componentInChildren.AddValidator(new ValidatorCvv(text, isMaestro));
				break;
			}
			if (element != null)
			{
				if (element.GetName() != "cvv")
				{
					input.GetComponentInChildren<Text>().text = element.GetExample();
				}
				input.onValueChanged.AddListener(delegate
				{
					OnValueChange(input, element.GetName());
				});
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(input.transform.parent.gameObject);
			}
			if (componentInChildren != null)
			{
				validators.Add(componentInChildren);
			}
		}
		Toggle componentInChildren2 = gameObject.GetComponentInChildren<Toggle>();
		if (xsollaForm.Contains("allowSubscription"))
		{
			XsollaFormElement ToggleElement = null;
			ToggleElement = xsollaForm.GetItem("allowSubscription");
			componentInChildren2.transform.GetComponentInChildren<Text>().text = ToggleElement.GetTitle();
			OnValueChange(ToggleElement.GetName(), componentInChildren2.isOn ? "1" : "0");
			componentInChildren2.onValueChanged.AddListener(delegate(bool b)
			{
				OnValueChange(ToggleElement.GetName(), b ? "1" : "0");
			});
		}
		else
		{
			GameObject.Find(componentInChildren2.transform.parent.name).SetActive(value: false);
		}
		if (xsollaForm.Contains("couponCode") && xsollaForm.GetItem("couponCode").IsVisible())
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ContainerPromoCode")) as GameObject;
			PromoCodeController component2 = obj.GetComponent<PromoCodeController>();
			component2.InitScreen(translations, xsollaForm.GetItem("couponCode"));
			component2._inputField.onValueChanged.AddListener(delegate(string s)
			{
				OnValueChange("couponCode", s.Trim());
			});
			component2._promoCodeApply.onClick.AddListener(delegate
			{
				bool flag = false;
				if (_form.GetCurrentCommand() == XsollaForm.CurrentCommand.CHECKOUT && _form.GetSkipChekout())
				{
					string checkoutToken = _form.GetCheckoutToken();
					flag = checkoutToken != null && !"".Equals(checkoutToken) && !"null".Equals(checkoutToken) && !"false".Equals(checkoutToken);
				}
				if (flag)
				{
					string url = "https://secure.xsolla.com/pages/checkout/?token=" + _form.GetCheckoutToken();
					if (Application.platform != RuntimePlatform.WebGLPlayer)
					{
						Application.OpenURL(url);
					}
				}
				base.gameObject.GetComponentInParent<XsollaPaystationController>().ApplyPromoCoupone(_form.GetXpsMap());
			});
			obj.transform.SetParent(gameObject.transform);
		}
		return gameObject;
	}

	private void OnValueChange(InputField _input, string elemName)
	{
		_form.UpdateElement(elemName, _input.text);
	}

	private void OnValueChange(string elemName, string pValue)
	{
		_form.UpdateElement(elemName, pValue);
	}

	private void OnClickPay(bool isLinkRequired)
	{
		Logger.Log("OnClickPay");
		bool flag = true;
		if (validators != null)
		{
			foreach (ValidatorInputField validator in validators)
			{
				if (flag)
				{
					flag = validator.IsValid();
				}
			}
		}
		if (!flag)
		{
			return;
		}
		if (isLinkRequired)
		{
			string url = "https://secure.xsolla.com/pages/checkout/?token=" + _form.GetCheckoutToken();
			if (Application.platform != RuntimePlatform.WebGLPlayer)
			{
				Application.OpenURL(url);
			}
		}
		base.gameObject.GetComponentInParent<XsollaPaystationController>().DoPayment(_form.GetXpsMap());
	}

	private void Next()
	{
		Logger.Log("OnClickNext");
		if (Application.platform == RuntimePlatform.WebGLPlayer)
		{
			RemoveLinkButtonWebGL();
		}
		base.gameObject.GetComponentInParent<XsollaPaystationController>().DoPayment(_form.GetXpsMap());
	}

	public void CreateLinkButtonWebGl(int xPos, int yPos, int width, int heigth, string link, string objName, string functionToCall)
	{
		isDivCreated = true;
	}

	public void RemoveLinkButtonWebGL()
	{
		if (Application.platform == RuntimePlatform.WebGLPlayer && isDivCreated)
		{
			isDivCreated = false;
		}
	}

	private GameObject GetInfo(XsollaMessage infoText)
	{
		if (infoText != null)
		{
			GameObject @object = GetObject("Prefabs/SimpleView/_ScreenCheckout/InfoPlate");
			SetText(@object, infoText.message);
			return @object;
		}
		return null;
	}

	private void OnDestroy()
	{
		RemoveLinkButtonWebGL();
	}

	private void OnBack()
	{
		if (OnClickBack != null)
		{
			OnClickBack();
		}
	}
}
