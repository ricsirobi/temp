using System;
using System.Collections.Generic;

namespace CI.WSANative.IAPStore;

public static class WSANativeStore
{
	public static void EnableTestMode()
	{
	}

	public static void ReloadSimulator()
	{
	}

	public static void GetProductListings(Action<List<WSAProduct>> response)
	{
	}

	public static void PurchaseProduct(string id, Action<WSAPurchaseResult> response)
	{
	}

	public static void ReportConsumableProductFulfillment(string id, Guid transactionId, Action<WSAFulfillmentResult> response)
	{
	}

	public static void GetUnfulfilledConsumableProducts(Action<List<WSAUnfulfilledConsumable>> response)
	{
	}

	public static void PurchaseApp(Action<string> response)
	{
	}

	public static WSAProductLicense GetLicenseForApp()
	{
		return new WSAProductLicense();
	}

	public static WSAProductLicense GetLicenseForProduct(string id)
	{
		return new WSAProductLicense();
	}
}
