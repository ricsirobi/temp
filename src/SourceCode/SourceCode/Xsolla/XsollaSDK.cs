using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class XsollaSDK : MonoBehaviour
{
	public bool isSandbox;

	public CCPayment payment;

	private string token;

	public string getToken()
	{
		return token;
	}

	public string InitPaystation(string token)
	{
		string text = "https://secure.xsolla.com/paystation2/?access_token=" + token;
		Application.OpenURL(text);
		return text;
	}

	public void CreatePaymentForm()
	{
		XsollaPaystationController formController = GetPaystationController();
		formController.OkHandler += delegate(XsollaResult status)
		{
			UtDebug.Log("OkHandler 1 " + status);
		};
		formController.ErrorHandler += delegate(XsollaError error)
		{
			UtDebug.Log("ErrorHandler 2 " + error);
		};
		string prepared = new XsollaJsonGenerator("user_1", 14004L)
		{
			user = 
			{
				name = "John Smith",
				email = "support@xsolla.com",
				country = "US"
			},
			settings = 
			{
				currency = "USD",
				languge = "en"
			}
		}.GetPrepared();
		new Dictionary<string, object>().Add("data", prepared);
		StartCoroutine(XsollaJsonGenerator.FreshToken(delegate(string token)
		{
			SetToken(formController, token);
		}));
	}

	private void SetToken(XsollaPaystationController controller, string token)
	{
		if (token != null)
		{
			controller.OpenPaystation(token, isSandbox: false);
		}
	}

	public XsollaPaystationController CreatePaymentForm(InputField inputField)
	{
		XsollaPaystationController paystationController = GetPaystationController();
		string text = inputField.text;
		paystationController.OkHandler += delegate(XsollaResult status)
		{
			UtDebug.Log("OkHandler 1 " + status);
		};
		paystationController.ErrorHandler += delegate(XsollaError error)
		{
			UtDebug.Log("ErrorHandler 2 " + error);
		};
		paystationController.OpenPaystation(text, isSandbox);
		return paystationController;
	}

	public XsollaPaystationController CreatePaymentForm(string data)
	{
		XsollaPaystationController paystationController = GetPaystationController();
		paystationController.OkHandler += delegate(XsollaResult status)
		{
			UtDebug.Log("OkHandler 1 " + status);
		};
		paystationController.ErrorHandler += delegate(XsollaError error)
		{
			UtDebug.Log("ErrorHandler 2 " + error);
		};
		paystationController.OpenPaystation(data, isSandbox);
		return paystationController;
	}

	public void CreatePaymentForm(string data, Action<XsollaResult> actionOk, Action<XsollaError> actionError)
	{
		XsollaPaystationController paystationController = GetPaystationController();
		paystationController.OkHandler += actionOk;
		paystationController.ErrorHandler += actionError;
		paystationController.OpenPaystation(data, isSandbox);
	}

	public void CreatePaymentForm(XsollaJsonGenerator generator, Action<XsollaResult> actionOk, Action<XsollaError> actionError)
	{
		CreatePaymentForm(generator.GetPrepared(), actionOk, actionError);
	}

	public void DirectPayment(CCPayment payment)
	{
		payment.InitPaystation();
	}

	public void SetToken(string s)
	{
		token = s;
	}

	public void SetSandbox(bool b)
	{
		isSandbox = b;
	}

	private XsollaPaystationController GetPaystationController()
	{
		return (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/XsollaPaystation")) as GameObject).GetComponent<XsollaPaystationController>();
	}
}
