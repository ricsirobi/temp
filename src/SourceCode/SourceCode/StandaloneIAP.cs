using System;
using UnityEngine;
using Xsolla;

public class StandaloneIAP : MonoBehaviour, IAPProvider
{
	public string _XsollaGameName = "DW Dragon Online";

	public XsollaSDK _XsollaPlugin;

	private IAPItemData mCurrentIAPItemData;

	private XsollaPaystationController mPaystationController;

	public Action<bool> OnProductsReceived { get; set; }

	public Action<PurchaseStatus, string, Receipt> OnPurchaseDone { get; set; }

	private void Start()
	{
		IAPManager.SetProvider(this);
	}

	public void Init(string[] productIdentifiers, IAPStoreData storeData)
	{
		if (OnProductsReceived != null)
		{
			OnProductsReceived(obj: true);
		}
	}

	public void Purchase(IAPItemData item, int quantity)
	{
		InitiatePurchase(item, quantity);
	}

	public void InitiatePurchase(IAPItemData iapItemData, int quantity)
	{
		AccessTokenRequest accessTokenRequest = new AccessTokenRequest();
		accessTokenRequest.PurchaseInfo = new PurchaseInfo();
		accessTokenRequest.PurchaseInfo.Description = new Description();
		accessTokenRequest.PurchaseInfo.Description.Value = iapItemData.Description.GetLocalizedString();
		accessTokenRequest.CustomParameters = new CustomParameters();
		if (UserInfo.pInstance != null && !string.IsNullOrEmpty(UserInfo.pInstance.UserID))
		{
			accessTokenRequest.CustomParameters.ProfileUserId = UserInfo.pInstance.UserID;
		}
		accessTokenRequest.CustomParameters.ItemId = iapItemData.AppStoreID;
		accessTokenRequest.CustomParameters.ItemName = iapItemData.AppStoreID;
		accessTokenRequest.CustomParameters.GameName = _XsollaGameName;
		WsWebService.GetXsollaToken(accessTokenRequest, OnGetAccessToken, null);
		mCurrentIAPItemData = iapItemData;
	}

	private void OnGetAccessToken(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				UtDebug.Log("Xsolla access token:: " + inObject.ToString());
				GetAccessTokenResponse getAccessTokenResponse = (GetAccessTokenResponse)inObject;
				if (getAccessTokenResponse != null && getAccessTokenResponse.Success)
				{
					if (_XsollaPlugin == null)
					{
						_XsollaPlugin = GetComponent<XsollaSDK>();
					}
					if (_XsollaPlugin != null)
					{
						if (getAccessTokenResponse.AccessTokenInfo.Sandbox.HasValue)
						{
							_XsollaPlugin.SetSandbox(getAccessTokenResponse.AccessTokenInfo.Sandbox.Value);
						}
						mPaystationController = _XsollaPlugin.CreatePaymentForm(getAccessTokenResponse.AccessTokenInfo.AccessToken);
						mPaystationController.OkHandler += OnPurchaseOk;
						mPaystationController.ErrorHandler += OnPurchaseError;
						KAUICursorManager.SetDefaultCursor("", showHideSystemCursor: false);
						break;
					}
					UtDebug.LogError("Improper setup. _XsollaPlugin is missing!!!");
				}
			}
			UtDebug.LogError("Empty response on getting Xsolla Access Token!!!");
			if (OnPurchaseDone != null)
			{
				OnPurchaseDone(PurchaseStatus.FAILED, "Xsolla Token Response Empty ", null);
			}
			break;
		case WsServiceEvent.ERROR:
			UtDebug.LogError("Error on getting Xsolla Access Token!!!");
			if (OnPurchaseDone != null)
			{
				OnPurchaseDone(PurchaseStatus.FAILED, "Xsolla Token Error", null);
			}
			break;
		}
	}

	private void OnPurchaseOk(XsollaResult inResult)
	{
		mPaystationController.OkHandler -= OnPurchaseOk;
		mPaystationController.ErrorHandler -= OnPurchaseError;
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (OnPurchaseDone != null)
		{
			if (inResult != null && !string.IsNullOrEmpty(inResult.invoice) && inResult.status == XsollaStatusData.Status.DONE)
			{
				OnPurchaseDone(PurchaseStatus.SUCCESSFUL, mCurrentIAPItemData.AppStoreID, null);
			}
			else
			{
				OnPurchaseDone(PurchaseStatus.FAILED, "Empty Xsolla response", null);
			}
		}
	}

	private void OnPurchaseError(XsollaError inError)
	{
		mPaystationController.OkHandler -= OnPurchaseOk;
		mPaystationController.ErrorHandler -= OnPurchaseError;
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (OnPurchaseDone != null)
		{
			OnPurchaseDone(PurchaseStatus.FAILED, inError.ToString(), null);
		}
	}
}
