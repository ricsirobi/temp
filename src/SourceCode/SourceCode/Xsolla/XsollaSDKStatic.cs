using System;
using UnityEngine;

namespace Xsolla;

public static class XsollaSDKStatic
{
	public static string InitPaystation(string token)
	{
		string text = "https://secure.xsolla.com/paystation2/?access_token=" + token;
		Application.OpenURL(text);
		return text;
	}

	public static void CreatePaymentForm(string data, bool isSandbox)
	{
		XsollaPaystationController paystationController = GetPaystationController();
		paystationController.OkHandler += delegate(XsollaResult status)
		{
			Debug.Log("OkHandler 1 " + status);
		};
		paystationController.ErrorHandler += delegate(XsollaError error)
		{
			Debug.Log("ErrorHandler 2 " + error);
		};
		paystationController.OpenPaystation(data, isSandbox);
	}

	public static void CreatePaymentForm(string data, Action<XsollaResult> actionOk, Action<XsollaError> actionError, bool isSandbox)
	{
		XsollaPaystationController paystationController = GetPaystationController();
		paystationController.OkHandler += actionOk;
		paystationController.ErrorHandler += actionError;
		paystationController.OpenPaystation(data, isSandbox);
	}

	public static void CreatePaymentForm(XsollaJsonGenerator generator, Action<XsollaResult> actionOk, Action<XsollaError> actionError, bool isSandbox)
	{
		XsollaPaystationController paystationController = GetPaystationController();
		paystationController.OkHandler += actionOk;
		paystationController.ErrorHandler += actionError;
		paystationController.OpenPaystation(generator.GetPrepared(), isSandbox);
	}

	public static void DirectPayment(CCPayment payment)
	{
		payment.InitPaystation();
	}

	private static XsollaPaystationController GetPaystationController()
	{
		return (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/XsollaPaystation")) as GameObject).GetComponent<XsollaPaystationController>();
	}
}
