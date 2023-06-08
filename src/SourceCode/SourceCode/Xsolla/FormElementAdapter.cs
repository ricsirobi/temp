using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class FormElementAdapter : IBaseAdapter
{
	private GameObject labelPrefab;

	private GameObject tablePrefab;

	private GameObject inputPrefab;

	private GameObject checkBoxPrefab;

	private GameObject selectPrtefab;

	private GameObject selectElementPrefab;

	private XsollaForm form;

	private List<XsollaFormElement> elements;

	private XsollaTranslations _translation;

	public object getSelectElemPrefab()
	{
		return selectElementPrefab;
	}

	public void Awake()
	{
		labelPrefab = Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ElementLabel") as GameObject;
		tablePrefab = Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ElementTable") as GameObject;
		inputPrefab = Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ElementText") as GameObject;
		checkBoxPrefab = Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ElementCheckBox") as GameObject;
		selectPrtefab = Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ElementDropDown") as GameObject;
		selectElementPrefab = Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ElementSelectElement") as GameObject;
	}

	public override int GetElementType(int id)
	{
		return 0;
	}

	public override int GetCount()
	{
		return elements.Count;
	}

	public override GameObject GetView(int position)
	{
		XsollaFormElement xsollaFormElement = elements[position];
		return xsollaFormElement.GetElementType() switch
		{
			7 => DrawLabel(xsollaFormElement), 
			1 => DrawInput(xsollaFormElement), 
			5 => DrawCheckBox(xsollaFormElement), 
			2 => DrawSelect(xsollaFormElement), 
			4 => DrawTable(xsollaFormElement), 
			_ => DrawLabel(xsollaFormElement), 
		};
	}

	private GameObject DrawTable(XsollaFormElement element)
	{
		GameObject obj = UnityEngine.Object.Instantiate(tablePrefab);
		obj.GetComponent<ElementTableController>().InitScreen(element);
		return obj;
	}

	private GameObject DrawLabel(XsollaFormElement element)
	{
		if (element.GetTitle() == "")
		{
			return null;
		}
		GameObject obj = UnityEngine.Object.Instantiate(labelPrefab);
		obj.GetComponentInChildren<Text>().text = element.GetTitle();
		return obj;
	}

	private GameObject DrawInput(XsollaFormElement element)
	{
		if (element.GetName() == "couponCode")
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ContainerPromoCode")) as GameObject;
			PromoCodeController controller = gameObject.GetComponent<PromoCodeController>();
			controller.InitScreen(_translation, element);
			controller._inputField.onEndEdit.AddListener(delegate
			{
				OnEndEdit(element, controller._inputField);
			});
			controller._promoCodeApply.onClick.AddListener(delegate
			{
				bool flag = false;
				if (form.GetCurrentCommand() == XsollaForm.CurrentCommand.CHECKOUT && form.GetSkipChekout())
				{
					string checkoutToken = form.GetCheckoutToken();
					flag = checkoutToken != null && !"".Equals(checkoutToken) && !"null".Equals(checkoutToken) && !"false".Equals(checkoutToken);
				}
				if (flag)
				{
					string url = "https://secure.xsolla.com/pages/checkout/?token=" + form.GetCheckoutToken();
					if (Application.platform != RuntimePlatform.WebGLPlayer)
					{
						Application.OpenURL(url);
					}
				}
				base.gameObject.GetComponentInParent<XsollaPaystationController>().ApplyPromoCoupone(form.GetXpsMap());
			});
			return gameObject;
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(inputPrefab);
		gameObject2.GetComponentInChildren<Text>().text = element.GetTitle();
		InputField inputField = gameObject2.GetComponentInChildren<InputField>();
		inputField.GetComponentInChildren<Text>().text = element.GetExample();
		SetupValidation(element.GetName(), inputField);
		inputField.onEndEdit.AddListener(delegate
		{
			OnEndEdit(element, inputField);
		});
		return gameObject2;
	}

	private void SetupValidation(string fieldName, InputField _inputField)
	{
		switch (fieldName)
		{
		default:
			_ = fieldName == "zip_code";
			break;
		case "card_number":
			_inputField.characterValidation = InputField.CharacterValidation.Integer;
			_inputField.characterLimit = 19;
			break;
		case "card_year":
			_inputField.characterValidation = InputField.CharacterValidation.Integer;
			break;
		case "card_month":
			_inputField.characterValidation = InputField.CharacterValidation.Integer;
			break;
		case "cvv":
			_inputField.characterValidation = InputField.CharacterValidation.Integer;
			break;
		case "card_holder":
			break;
		}
	}

	public void OnEndEdit(XsollaFormElement element, InputField _input)
	{
		form.UpdateElement(element.GetName(), _input.text);
	}

	public void OnEndEdit(string fieldName, string newValue)
	{
		form.UpdateElement(fieldName, newValue);
	}

	private GameObject DrawCheckBox(XsollaFormElement element)
	{
		GameObject obj = UnityEngine.Object.Instantiate(checkBoxPrefab);
		Toggle componentInChildren = obj.GetComponentInChildren<Toggle>();
		OnEndEdit(element.GetName(), componentInChildren.isOn ? "1" : "0");
		componentInChildren.onValueChanged.AddListener(delegate(bool b)
		{
			OnEndEdit(element.GetName(), b ? "1" : "0");
		});
		obj.GetComponentInChildren<Text>().text = element.GetTitle();
		return obj;
	}

	private GameObject DrawSelect(XsollaFormElement element)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(selectPrtefab);
		DropDownController component = gameObject.GetComponent<DropDownController>();
		gameObject.GetComponentInChildren<Text>().text = element.GetTitle();
		List<XsollaFormElement.Option> options = element.GetOptions();
		List<string> list = new List<string>(options.Count);
		foreach (XsollaFormElement.Option item in options)
		{
			list.Add(item.GetLabel());
		}
		component.SetParentForScroll(base.gameObject.transform.parent.parent);
		component.OnItemSelected = (Action<int, string>)Delegate.Combine(component.OnItemSelected, (Action<int, string>)delegate(int position, string title)
		{
			OnEndEdit(element.GetName(), options[position].GetValue());
		});
		component.SetData(list);
		return gameObject;
	}

	public override GameObject GetPrefab()
	{
		return inputPrefab;
	}

	public void SetForm(XsollaForm form, XsollaTranslations pTranslation = null)
	{
		this.form = form;
		elements = form.GetVisible();
		_translation = pTranslation;
	}

	public override GameObject GetNext()
	{
		throw new NotImplementedException();
	}
}
