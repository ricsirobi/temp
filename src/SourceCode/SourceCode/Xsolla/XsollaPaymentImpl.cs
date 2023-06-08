using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;

namespace Xsolla;

public class XsollaPaymentImpl : MonoBehaviour, IXsollaPayment
{
	private string DOMAIN = "https://secure.xsolla.com";

	private const string SDK_VERSION = "1.3.6";

	private const int TRANSLATIONS = 0;

	private const int DIRECTPAYMENT_FORM = 1;

	private const int DIRECTPAYMENT_STATUS = 2;

	private const int PRICEPOINTS = 3;

	private const int GOODS = 5;

	private const int GOODS_GROUPS = 51;

	private const int GOODS_ITEMS = 52;

	private const int PAYMENT_LIST = 6;

	private const int SAVED_PAYMENT_LIST = 61;

	private const int QUICK_PAYMENT_LIST = 7;

	private const int COUNTRIES = 8;

	private const int VIRTUAL_PAYMENT_SUMMARY = 9;

	private const int VIRTUAL_PROCEED = 10;

	private const int VIRTUAL_STATUS = 11;

	private const int APPLY_PROMO_COUPONE = 12;

	private const int COUPON_PROCEED = 13;

	private const int HISTORY = 22;

	private const int PAYMENT_MANAGER_LIST = 15;

	private const int CALCULATE_CUSTOM_AMOUNT = 14;

	private const int ACTIVE_SUBS = 16;

	private const int DELETE_SAVED_METHOD = 17;

	private const int SUBSCRIPTIONS_MANAGER_LIST = 18;

	public Action<XsollaUtils> UtilsRecieved;

	public Action<XsollaTranslations> TranslationRecieved;

	public Action<XsollaHistoryList> HistoryRecieved;

	public Action<XsollaPricepointsManager> PricepointsRecieved;

	public Action<XsollaGroupsManager> GoodsGroupsRecieved;

	public Action<XsollaGoodsManager> GoodsRecieved;

	public Action<XsollaSubscriptions> SubsReceived;

	public Action<XsollaPaymentMethods> PaymentMethodsRecieved;

	public Action<XsollaSavedPaymentMethods> SavedPaymentMethodsRecieved;

	public Action<XsollaQuickPayments> QuickPaymentMethodsRecieved;

	public Action<XsollaQuickPayments> QuickPaymentMethodsRecievedNew;

	public Action<XsollaCountries> CountriesRecieved;

	public Action<XsollaForm> FormReceived;

	public Action<XsollaStatus, XsollaForm> StatusReceived;

	public Action<XsollaForm> ApplyCouponeCodeReceived;

	public Action<XsollaStatusPing> StatusChecked;

	public Action<XsollaError> ErrorReceived;

	public Action<XsollaCouponProceedResult> CouponProceedErrorRecived;

	public Action<CustomVirtCurrAmountController.CustomAmountCalcRes> CustomAmountCalcRecieved;

	public Action<XVirtualPaymentSummary> VirtualPaymentSummaryRecieved;

	public Action<string> VirtualPaymentProceedError;

	public Action<XVPStatus> VirtualPaymentStatusRecieved;

	public Action<XsollaSavedPaymentMethods, bool> PaymentManagerMethods;

	public Action DeleteSavedPaymentMethodRespond;

	public Action WaitChangeSavedMethods;

	public Action<XsollaManagerSubscriptions> SubsManagerListRecived;

	protected string _accessToken;

	protected Dictionary<string, object> baseParams;

	protected HttpTlsRequest httpreq;

	private string propertyID = "UA-62372273-1";

	private static XsollaPaymentImpl instance;

	private string bundleID = "Unity";

	private string appName;

	private string projectId = "";

	private string screenRes;

	private string clientID;

	public XsollaPaymentImpl()
	{
	}

	public XsollaPaymentImpl(string accessToken)
	{
		_accessToken = accessToken;
	}

	public void InitPaystation(XsollaWallet xsollaWallet)
	{
		_accessToken = xsollaWallet.GetToken();
		GetUtils(null);
	}

	public void InitPaystation(string accessToken)
	{
		_accessToken = accessToken;
		GetUtils(null);
	}

	public void InitPaystation(Dictionary<string, object> pararams)
	{
		baseParams = new Dictionary<string, object>(pararams);
		GetUtils(pararams);
	}

	public void StartPaymentWithoutUtils(XsollaWallet xsollaWallet)
	{
		_accessToken = xsollaWallet.GetToken();
		GetNextStep(new Dictionary<string, object>());
	}

	public void NextStep(Dictionary<string, object> xpsMap)
	{
		GetNextStep(xpsMap);
	}

	public void Status(string token, long invoice)
	{
	}

	public void SetModeSandbox(bool isSandbox)
	{
		if (!isSandbox)
		{
			DOMAIN = "https://secure.xsolla.com";
		}
		else
		{
			DOMAIN = "https://sandbox-secure.xsolla.com";
		}
	}

	public void SetDomain(string domain)
	{
		DOMAIN = domain;
	}

	public void SetToken(string token)
	{
		_accessToken = token;
	}

	private void OnUtilsRecieved(XsollaUtils utils)
	{
		if (UtilsRecieved != null)
		{
			UtilsRecieved(utils);
		}
	}

	private void OnPricepointsRecieved(XsollaPricepointsManager pricepoints)
	{
		if (PricepointsRecieved != null)
		{
			PricepointsRecieved(pricepoints);
		}
	}

	private void OnGoodsRecieved(XsollaGoodsManager goods)
	{
		if (GoodsRecieved != null)
		{
			GoodsRecieved(goods);
		}
	}

	private void OnGoodsGroupsRecieved(XsollaGroupsManager groups)
	{
		if (GoodsGroupsRecieved != null)
		{
			GoodsGroupsRecieved(groups);
		}
	}

	private void OnSubscriptionsReceived(XsollaSubscriptions pSubs)
	{
		if (SubsReceived != null)
		{
			SubsReceived(pSubs);
		}
	}

	private void OnVPSummaryRecieved(XVirtualPaymentSummary summary)
	{
		if (VirtualPaymentSummaryRecieved != null)
		{
			VirtualPaymentSummaryRecieved(summary);
		}
	}

	private void OnVPProceedError(string error)
	{
		if (VirtualPaymentProceedError != null)
		{
			VirtualPaymentProceedError(error);
		}
	}

	private void OnVPStatusRecieved(XVPStatus status)
	{
		if (VirtualPaymentStatusRecieved != null)
		{
			VirtualPaymentStatusRecieved(status);
		}
	}

	private void OnPaymentMethodsRecieved(XsollaPaymentMethods paymentMethods)
	{
		if (PaymentMethodsRecieved != null)
		{
			PaymentMethodsRecieved(paymentMethods);
		}
	}

	private void OnSavedPaymentMethodsRecieved(XsollaSavedPaymentMethods pMethods)
	{
		if (SavedPaymentMethodsRecieved != null)
		{
			SavedPaymentMethodsRecieved(pMethods);
		}
	}

	private void OnQuickPaymentMethodsRecieved(XsollaQuickPayments quickPayments)
	{
		if (QuickPaymentMethodsRecieved != null)
		{
			QuickPaymentMethodsRecieved(quickPayments);
		}
	}

	private void OnQuickPaymentMethodsRecievedNew(XsollaQuickPayments quickPayments)
	{
		if (QuickPaymentMethodsRecievedNew != null)
		{
			QuickPaymentMethodsRecievedNew(quickPayments);
		}
	}

	private void OnCountriesRecieved(XsollaCountries countries)
	{
		if (CountriesRecieved != null)
		{
			CountriesRecieved(countries);
		}
	}

	protected virtual void OnTranslationRecieved(XsollaTranslations translations)
	{
		if (TranslationRecieved != null)
		{
			TranslationRecieved(translations);
		}
	}

	protected virtual void OnHistoryRecieved(XsollaHistoryList pHistoryList)
	{
		if (HistoryRecieved != null)
		{
			HistoryRecieved(pHistoryList);
		}
	}

	protected virtual void OnFormReceived(XsollaForm form)
	{
		if (FormReceived != null)
		{
			FormReceived(form);
		}
	}

	protected virtual void OnStatusReceived(XsollaStatus status, XsollaForm form)
	{
		if (StatusReceived != null)
		{
			StatusReceived(status, form);
		}
	}

	protected virtual void OnApplyCouponeReceived(XsollaForm pForm)
	{
		if (ApplyCouponeCodeReceived != null)
		{
			ApplyCouponeCodeReceived(pForm);
		}
	}

	protected virtual void OnStatusChecked(XsollaStatusPing pStatus)
	{
		if (StatusChecked != null)
		{
			StatusChecked(pStatus);
		}
	}

	protected virtual void OnErrorReceived(XsollaError error)
	{
		if (ErrorReceived != null)
		{
			ErrorReceived(error);
		}
	}

	protected virtual void OnCouponProceedErrorRecived(XsollaCouponProceedResult pCouponObj)
	{
		if (CouponProceedErrorRecived != null)
		{
			CouponProceedErrorRecived(pCouponObj);
		}
	}

	protected virtual void OnCustomAmountResRecieved(CustomVirtCurrAmountController.CustomAmountCalcRes pRes)
	{
		if (CustomAmountCalcRecieved != null)
		{
			CustomAmountCalcRecieved(pRes);
		}
	}

	protected virtual void OnPaymentManagerMethod(XsollaSavedPaymentMethods pRes, bool pAddState)
	{
		if (PaymentManagerMethods != null)
		{
			PaymentManagerMethods(pRes, pAddState);
		}
	}

	protected virtual void OnManageSubsListrecived(XsollaManagerSubscriptions pListSubs)
	{
		if (SubsManagerListRecived != null)
		{
			SubsManagerListRecived(pListSubs);
		}
	}

	protected virtual void OnDeleteSavedPaymentMethod()
	{
		if (DeleteSavedPaymentMethodRespond != null)
		{
			DeleteSavedPaymentMethodRespond();
		}
	}

	private void OnWaitPaymentChange()
	{
		if (WaitChangeSavedMethods != null)
		{
			WaitChangeSavedMethods();
		}
	}

	public void GetVPSummary(Dictionary<string, object> pararams)
	{
		if (!pararams.ContainsKey("is_virtual_payment"))
		{
			pararams.Add("is_virtual_payment", 1);
		}
		else
		{
			pararams["is_virtual_payment"] = 1;
		}
		StartCoroutine(POST(9, GetCartSummary(), pararams));
	}

	public void ProceedVPayment(Dictionary<string, object> pararams)
	{
		StartCoroutine(POST(10, ProceedVirtualPaymentLink(), pararams));
	}

	public void VPaymentStatus(Dictionary<string, object> pararams)
	{
		StartCoroutine(POST(11, GetVirtualPaymentStatusLink(), pararams));
	}

	private void GetUtils(Dictionary<string, object> pararams)
	{
		StartCoroutine(POST(0, GetUtilsLink(), pararams));
	}

	private void GetNextStep(Dictionary<string, object> nextStepParams)
	{
		if (nextStepParams.Count == 0)
		{
			nextStepParams.Add("pid", 26);
		}
		if (!nextStepParams.ContainsKey("paymentWithSavedMethod"))
		{
			nextStepParams.Add("paymentWithSavedMethod", 0);
		}
		StartCoroutine(POST(1, GetDirectpaymentLink(), nextStepParams));
	}

	public void GetStatus(Dictionary<string, object> statusParams)
	{
		StartCoroutine(POST(2, GetStatusLink(), statusParams));
	}

	public void GetPricePoints(Dictionary<string, object> requestParams)
	{
		StartCoroutine(POST(3, GetPricepointsUrl(), requestParams));
	}

	public void GetGoods(Dictionary<string, object> requestParams)
	{
		StartCoroutine(POST(5, GetGoodsUrl(), requestParams));
	}

	public void GetItemsGroups(Dictionary<string, object> requestParams)
	{
		StartCoroutine(POST(51, GetItemsGroupsUrl(), requestParams));
	}

	public void GetItems(long groupId, Dictionary<string, object> requestParams)
	{
		requestParams.Add("group_id", groupId);
		StartCoroutine(POST(52, GetItemsUrl(), requestParams));
	}

	public void GetSubscriptions()
	{
		new Dictionary<string, object>().Add("access_token", baseParams["access_token"]);
		StartCoroutine(POST(16, GetSubsUrl(), baseParams));
	}

	public void GetFavorites(Dictionary<string, object> requestParams)
	{
		StartCoroutine(POST(52, GetFavoritsUrl(), requestParams));
	}

	public void GetCouponProceed(Dictionary<string, object> pParams)
	{
		if (!pParams.ContainsKey("access_token") && baseParams.ContainsKey("access_token"))
		{
			pParams.Add("access_token", baseParams["access_token"]);
		}
		StartCoroutine(POST(13, GetCouponProceed(), pParams));
	}

	public void SetFavorite(Dictionary<string, object> requestParams)
	{
		StartCoroutine(POST(999, SetFavoritsUrl(), requestParams));
	}

	public void GetPaymentsInfo(Dictionary<string, object> requestParams)
	{
		StartCoroutine(POST(7, GetQuickPaymentsUrl(), requestParams));
		StartCoroutine(POST(6, GetPaymentListUrl(), requestParams));
		StartCoroutine(POST(8, GetCountriesListUrl(), requestParams));
	}

	public void GetQuickPayments(string countryIso, Dictionary<string, object> requestParams)
	{
		if (countryIso != null && !"".Equals(countryIso))
		{
			requestParams["country"] = countryIso;
		}
		StartCoroutine(POST(7, GetQuickPaymentsUrl(), requestParams));
	}

	public void GetPayments(string countryIso, Dictionary<string, object> requestParams)
	{
		if (countryIso != null && !"".Equals(countryIso))
		{
			requestParams["country"] = countryIso;
		}
		StartCoroutine(POST(6, GetPaymentListUrl(), requestParams));
	}

	public void GetSavedPayments(Dictionary<string, object> requestParams)
	{
		StartCoroutine(POST(61, GetSavedPaymentListUrl(), requestParams));
	}

	public void GetSavedPaymentsForManager(Dictionary<string, object> pParams)
	{
		StartCoroutine(POST(15, GetSavedPaymentListUrl(), pParams));
	}

	public void GetSubscriptionForManager(Dictionary<string, object> pParams)
	{
		StartCoroutine(POST(18, GetSubscriptionsList(), pParams));
	}

	public void GetCountries(Dictionary<string, object> requestParams)
	{
		StartCoroutine(POST(8, GetCountriesListUrl(), requestParams));
	}

	public void GetHistory(Dictionary<string, object> pParams)
	{
		if (!pParams.ContainsKey("access_token"))
		{
			pParams.Add("access_token", baseParams["access_token"]);
		}
		StartCoroutine(POST(22, GetHistoryUrl(), pParams));
	}

	public void ApplyPromoCoupone(Dictionary<string, object> pParams)
	{
		StartCoroutine(POST(12, GetDirectpaymentLink(), pParams));
	}

	public void CalculateCustomAmount(Dictionary<string, object> pParams)
	{
		if (!pParams.ContainsKey("access_token"))
		{
			pParams.Add("access_token", baseParams["access_token"]);
		}
		StartCoroutine(POST(14, GetCalculateCustomAmountUrl(), pParams));
	}

	public void DeleteSavedMethod(Dictionary<string, object> pParam)
	{
		if (!pParam.ContainsKey("access_token"))
		{
			pParam.Add("access_token", baseParams["access_token"]);
		}
		StartCoroutine(POST(17, GetDeleteSavedPaymentMethodUrl(), pParam));
	}

	public IEnumerator POST(int type, string url, Dictionary<string, object> post)
	{
		WWWForm wWWForm = new WWWForm();
		StringBuilder stringBuilder = new StringBuilder();
		if (!post.ContainsKey("access_token") && !post.ContainsKey("project") && !post.ContainsKey("access_data") && baseParams != null)
		{
			foreach (KeyValuePair<string, object> baseParam in baseParams)
			{
				post.Add(baseParam.Key, baseParam.Value);
			}
			if (!post.ContainsKey("access_token") && !baseParams.ContainsKey("access_token") && _accessToken != "")
			{
				post.Add("access_token", _accessToken);
			}
		}
		if (type == 2)
		{
			TransactionHelper.SaveRequest(post);
		}
		if (!post.ContainsKey("alternative_platform"))
		{
			post.Add("alternative_platform", "unity/1.3.6");
		}
		foreach (KeyValuePair<string, object> item in post)
		{
			string value = ((item.Value != null) ? item.Value.ToString() : "");
			stringBuilder.Append(item.Key).Append("=").Append(value)
				.Append("&");
			wWWForm.AddField(item.Key, value);
		}
		UtDebug.Log(url);
		UtDebug.Log(stringBuilder.ToString());
		WWW www = new WWW(url, wWWForm);
		yield return StartCoroutine(WaitForRequest(type, www, post));
	}

	private IEnumerator WaitForRequest(int pType, WWW www, Dictionary<string, object> post)
	{
		Logger.Log("Start get www");
		yield return www;
		if (www.error == null)
		{
			UtDebug.Log("Type -> " + pType);
			UtDebug.Log("WWW_request -> " + www.text);
			_ = www.text;
			JSONNode jSONNode = JSON.Parse(www.text);
			if ((jSONNode != null && jSONNode.Count > 2) || jSONNode["error"] == null)
			{
				switch (pType)
				{
				case 0:
					if (jSONNode.Count > 2)
					{
						XsollaUtils xsollaUtils = new XsollaUtils().Parse(jSONNode) as XsollaUtils;
						projectId = xsollaUtils.GetProject().id.ToString();
						xsollaUtils.SetAccessToken(baseParams["access_token"].ToString());
						OnUtilsRecieved(xsollaUtils);
						OnTranslationRecieved(xsollaUtils.GetTranslations());
					}
					else
					{
						XsollaError xsollaError2 = new XsollaError();
						xsollaError2.Parse(jSONNode);
						OnErrorReceived(xsollaError2);
					}
					break;
				case 1:
					if (jSONNode.Count > 8)
					{
						XsollaForm xsollaForm3 = new XsollaForm();
						xsollaForm3.Parse(jSONNode);
						switch (xsollaForm3.GetCurrentCommand())
						{
						case XsollaForm.CurrentCommand.STATUS:
							if (post.ContainsKey("save_payment_account_only") || post.ContainsKey("replace_payment_account"))
							{
								if (!xsollaForm3.IsCardPayment() && !post.ContainsKey("replace_payment_account"))
								{
									OnWaitPaymentChange();
								}
								else
								{
									OnPaymentManagerMethod(null, (!post.ContainsKey("replace_payment_account")) ? true : false);
								}
							}
							else
							{
								GetStatus(xsollaForm3.GetXpsMap());
							}
							break;
						case XsollaForm.CurrentCommand.FORM:
						case XsollaForm.CurrentCommand.CREATE:
						case XsollaForm.CurrentCommand.CHECKOUT:
						case XsollaForm.CurrentCommand.CHECK:
						case XsollaForm.CurrentCommand.ACCOUNT:
							OnFormReceived(xsollaForm3);
							break;
						case XsollaForm.CurrentCommand.UNKNOWN:
						{
							if (jSONNode.Count > 10)
							{
								OnFormReceived(xsollaForm3);
								break;
							}
							XsollaError xsollaError = new XsollaError();
							xsollaError.Parse(jSONNode);
							OnErrorReceived(xsollaError);
							break;
						}
						}
					}
					else
					{
						XsollaStatusPing xsollaStatusPing = new XsollaStatusPing();
						xsollaStatusPing.Parse(jSONNode);
						OnStatusChecked(xsollaStatusPing);
					}
					break;
				case 2:
				{
					XsollaForm xsollaForm2 = new XsollaForm();
					xsollaForm2.Parse(jSONNode);
					XsollaStatus xsollaStatus = new XsollaStatus();
					xsollaStatus.Parse(jSONNode);
					OnStatusReceived(xsollaStatus, xsollaForm2);
					break;
				}
				case 3:
				{
					XsollaPricepointsManager xsollaPricepointsManager = new XsollaPricepointsManager();
					xsollaPricepointsManager.Parse(jSONNode);
					OnPricepointsRecieved(xsollaPricepointsManager);
					break;
				}
				case 5:
				{
					XsollaGoodsManager xsollaGoodsManager2 = new XsollaGoodsManager();
					xsollaGoodsManager2.Parse(jSONNode);
					OnGoodsRecieved(xsollaGoodsManager2);
					break;
				}
				case 51:
				{
					XsollaGroupsManager xsollaGroupsManager = new XsollaGroupsManager();
					xsollaGroupsManager.Parse(jSONNode);
					OnGoodsGroupsRecieved(xsollaGroupsManager);
					break;
				}
				case 52:
				{
					XsollaGoodsManager xsollaGoodsManager = new XsollaGoodsManager();
					xsollaGoodsManager.Parse(jSONNode);
					OnGoodsRecieved(xsollaGoodsManager);
					break;
				}
				case 6:
				{
					XsollaPaymentMethods xsollaPaymentMethods = new XsollaPaymentMethods();
					xsollaPaymentMethods.Parse(jSONNode);
					OnPaymentMethodsRecieved(xsollaPaymentMethods);
					break;
				}
				case 61:
				{
					XsollaSavedPaymentMethods xsollaSavedPaymentMethods = new XsollaSavedPaymentMethods();
					xsollaSavedPaymentMethods.Parse(jSONNode);
					OnSavedPaymentMethodsRecieved(xsollaSavedPaymentMethods);
					break;
				}
				case 7:
				{
					XsollaQuickPayments xsollaQuickPayments = new XsollaQuickPayments();
					xsollaQuickPayments.Parse(jSONNode);
					OnQuickPaymentMethodsRecieved(xsollaQuickPayments);
					break;
				}
				case 8:
				{
					XsollaCountries xsollaCountries = new XsollaCountries();
					xsollaCountries.Parse(jSONNode);
					OnCountriesRecieved(xsollaCountries);
					break;
				}
				case 9:
				{
					XVirtualPaymentSummary xVirtualPaymentSummary = new XVirtualPaymentSummary();
					xVirtualPaymentSummary.Parse(jSONNode);
					Logger.Log("VIRTUAL_PAYMENT_SUMMARY " + xVirtualPaymentSummary.ToString());
					if (xVirtualPaymentSummary.IsSkipConfirmation)
					{
						Logger.Log("IsSkipConfirmation true");
						post.Add("dont_ask_again", 0);
						ProceedVPayment(post);
					}
					else
					{
						Logger.Log("IsSkipConfirmation false");
						OnVPSummaryRecieved(xVirtualPaymentSummary);
					}
					break;
				}
				case 10:
				{
					XProceed xProceed = new XProceed();
					xProceed.Parse(jSONNode);
					Logger.Log("VIRTUAL_PROCEED " + xProceed.ToString());
					if (xProceed.IsInvoiceCreated)
					{
						Logger.Log("VIRTUAL_PROCEED 1");
						long operationId = xProceed.OperationId;
						post.Add("operation_id", operationId);
						VPaymentStatus(post);
					}
					else
					{
						Logger.Log("VIRTUAL_PROCEED 0 ");
						OnVPProceedError(xProceed.Error);
					}
					break;
				}
				case 11:
				{
					XVPStatus xVPStatus = new XVPStatus();
					xVPStatus.Parse(jSONNode);
					Logger.Log("VIRTUAL_STATUS" + xVPStatus.ToString());
					OnVPStatusRecieved(xVPStatus);
					break;
				}
				case 12:
				{
					XsollaForm xsollaForm = new XsollaForm();
					xsollaForm.Parse(jSONNode);
					OnApplyCouponeReceived(xsollaForm);
					break;
				}
				case 13:
				{
					XsollaCouponProceedResult xsollaCouponProceedResult = new XsollaCouponProceedResult();
					xsollaCouponProceedResult.Parse(jSONNode);
					if (xsollaCouponProceedResult._error != null)
					{
						Logger.Log("COUPON_PROCEED ERROR: " + xsollaCouponProceedResult._error);
						OnCouponProceedErrorRecived(xsollaCouponProceedResult);
						break;
					}
					long num = xsollaCouponProceedResult._operationId;
					if (post.ContainsKey("coupon_code"))
					{
						post.Remove("coupon_code");
					}
					post.Add("operation_id", num);
					VPaymentStatus(post);
					break;
				}
				case 22:
				{
					XsollaHistoryList pHistoryList = new XsollaHistoryList().Parse(jSONNode["operations"]) as XsollaHistoryList;
					OnHistoryRecieved(pHistoryList);
					break;
				}
				case 14:
				{
					CustomVirtCurrAmountController.CustomAmountCalcRes pRes2 = new CustomVirtCurrAmountController.CustomAmountCalcRes().Parse(jSONNode["calculation"]) as CustomVirtCurrAmountController.CustomAmountCalcRes;
					OnCustomAmountResRecieved(pRes2);
					break;
				}
				case 16:
				{
					XsollaSubscriptions xsollaSubscriptions = new XsollaSubscriptions();
					xsollaSubscriptions.Parse(jSONNode);
					OnSubscriptionsReceived(xsollaSubscriptions);
					break;
				}
				case 15:
				{
					XsollaSavedPaymentMethods pRes = new XsollaSavedPaymentMethods().Parse(jSONNode) as XsollaSavedPaymentMethods;
					OnPaymentManagerMethod(pRes, pAddState: false);
					break;
				}
				case 17:
					OnDeleteSavedPaymentMethod();
					break;
				case 18:
				{
					XsollaManagerSubscriptions pListSubs = new XsollaManagerSubscriptions().Parse(jSONNode["subscriptions"]) as XsollaManagerSubscriptions;
					OnManageSubsListrecived(pListSubs);
					break;
				}
				}
			}
			else
			{
				XsollaError xsollaError3 = new XsollaError();
				xsollaError3.Parse(jSONNode);
				OnErrorReceived(xsollaError3);
			}
		}
		else
		{
			JSONNode jSONNode2 = JSON.Parse(www.text);
			string errorMessage = jSONNode2["errors"].AsArray[0]["message"].Value + ". Support code " + jSONNode2["errors"].AsArray[0]["support_code"].Value;
			int errorCode = ((www.error.Length <= 3) ? int.Parse(www.error) : int.Parse(www.error.Substring(0, 3)));
			OnErrorReceived(new XsollaError(errorCode, errorMessage));
		}
		if (projectId != null && !"".Equals(projectId))
		{
			LogEvent("UNITY 1.3.6 REQUEST", projectId, www.url);
		}
		else
		{
			LogEvent("UNITY 1.3.6 REQUEST", "undefined", www.url);
		}
	}

	private string GetUtilsLink()
	{
		return DOMAIN + "/paystation2/api/utils";
	}

	private string GetDirectpaymentLink()
	{
		return DOMAIN + "/paystation2/api/directpayment";
	}

	private string GetStatusLink()
	{
		return DOMAIN + "/paystation2/api/directpayment";
	}

	private string GetPricepointsUrl()
	{
		return DOMAIN + "/paystation2/api/pricepoints";
	}

	private string GetGoodsUrl()
	{
		return DOMAIN + "/paystation2/api/digitalgoods";
	}

	private string GetFavoritsUrl()
	{
		return DOMAIN + "/paystation2/api/virtualitems/favorite";
	}

	private string SetFavoritsUrl()
	{
		return DOMAIN + "/paystation2/api/virtualitems/setfavorite";
	}

	private string GetItemsGroupsUrl()
	{
		return DOMAIN + "/paystation2/api/virtualitems/groups";
	}

	private string GetCouponProceed()
	{
		return DOMAIN + "/paystation2/api/coupons/proceed";
	}

	private string GetItemsUrl()
	{
		return DOMAIN + "/paystation2/api/virtualitems/items";
	}

	private string GetHistoryUrl()
	{
		return DOMAIN + "/paystation2/api/balance/history";
	}

	private string GetPaymentListUrl()
	{
		return DOMAIN + "/paystation2/api/paymentlist/payment_methods";
	}

	private string GetSavedPaymentListUrl()
	{
		return DOMAIN + "/paystation2/api/savedmethods";
	}

	private string GetQuickPaymentsUrl()
	{
		return DOMAIN + "/paystation2/api/paymentlist/quick_payments";
	}

	private string GetCountriesListUrl()
	{
		return DOMAIN + "/paystation2/api/country";
	}

	private string GetSubsUrl()
	{
		return DOMAIN + "/paystation2/api/recurring/active";
	}

	private string GetCartSummary()
	{
		return DOMAIN + "/paystation2/api/cart/summary";
	}

	private string ProceedVirtualPaymentLink()
	{
		return DOMAIN + "/paystation2/api/virtualpayment/proceed";
	}

	private string GetVirtualPaymentStatusLink()
	{
		return DOMAIN + "/paystation2/api/virtualstatus";
	}

	private string GetCalculateCustomAmountUrl()
	{
		return DOMAIN + "/paystation2/api/pricepoints/calculate";
	}

	private string GetDeleteSavedPaymentMethodUrl()
	{
		return DOMAIN + "/paystation2/api/savedmethods/delete";
	}

	private string GetSubscriptionsList()
	{
		return DOMAIN + "/paystation2/api/useraccount/subscriptions";
	}

	private void Start()
	{
		appName = GetProjectName();
		screenRes = Screen.width + "x" + Screen.height;
		clientID = SystemInfo.deviceUniqueIdentifier;
	}

	public void LogScreen(string title)
	{
		title = WWW.EscapeURL(title);
		string url = "https://www.google-analytics.com/collect?v=1&ul=en-us&t=appview&sr=" + screenRes + "&an=" + WWW.EscapeURL(appName) + "&a=448166238&tid=" + propertyID + "&aid=" + bundleID + "&cid=" + WWW.EscapeURL(clientID) + "&_u=.sB&av=1.3.6&_v=ma1b3&cd=" + title + "&qt=2500&z=185";
		StartCoroutine(Process(new WWW(url)));
	}

	public void LogEvent(string titleCat, string titleAction, string actionLable)
	{
		titleCat = WWW.EscapeURL(titleCat);
		titleAction = WWW.EscapeURL(titleAction);
		actionLable = WWW.EscapeURL(actionLable);
		string url = "https://www.google-analytics.com/collect?v=1&ul=en-us&t=event&sr=" + screenRes + "&an=" + WWW.EscapeURL(appName) + "&a=448166238&tid=" + propertyID + "&aid=" + bundleID + "&cid=" + WWW.EscapeURL(clientID) + "&_u=.sB&av=1.3.6&_v=ma1b3&ec=" + titleCat + "&ea=" + titleAction + "&el" + actionLable + "&qt=2500&z=185";
		StartCoroutine(Process(new WWW(url)));
	}

	public void LogEvent(string titleCat, string actionLable)
	{
		titleCat = WWW.EscapeURL(titleCat);
		if (projectId == null)
		{
			projectId = GetProjectName();
			projectId = WWW.EscapeURL(projectId);
		}
		actionLable = WWW.EscapeURL(actionLable);
		string url = "https://www.google-analytics.com/collect?v=1&ul=en-us&t=event&sr=" + screenRes + "&an=" + WWW.EscapeURL(appName) + "&a=448166238&tid=" + propertyID + "&aid=" + bundleID + "&cid=" + WWW.EscapeURL(clientID) + "&_u=.sB&av=1.3.6&_v=ma1b3&ec=" + titleCat + "&ea=" + projectId + "&el=" + actionLable + "&qt=2500&z=185";
		StartCoroutine(Process(new WWW(url)));
	}

	private IEnumerator Process(WWW www)
	{
		yield return www;
		if (www.error == null)
		{
			if (www.responseHeaders.ContainsKey("STATUS"))
			{
				if (!(www.responseHeaders["STATUS"] == "HTTP/1.1 200 OK"))
				{
				}
			}
			else
			{
				UtDebug.LogWarning("Event failed to send to Google");
			}
		}
		else
		{
			UtDebug.LogWarning(www.error.ToString());
		}
		www.Dispose();
	}

	public string GetProjectName()
	{
		return Application.dataPath.Split('/')[^2];
	}
}
