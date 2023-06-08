using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Xsolla;

public abstract class XsollaPaystation : MonoBehaviour
{
	private string BaseParams;

	private bool IsSandbox;

	protected XsollaUtils Utils;

	private ActivePurchase currentPurchase;

	private bool isSimple;

	public string _countryCurr = "US";

	private XsollaPaymentImpl __payment;

	protected XsollaResult result;

	private XsollaPaymentImpl Payment
	{
		get
		{
			if (__payment == null)
			{
				__payment = base.gameObject.GetComponent<XsollaPaymentImpl>();
				if (__payment == null)
				{
					__payment = base.gameObject.AddComponent<XsollaPaymentImpl>();
				}
			}
			return __payment;
		}
		set
		{
			__payment = value;
		}
	}

	public bool isSandbox()
	{
		return IsSandbox;
	}

	protected abstract void ShowPricepoints(XsollaUtils utils, XsollaPricepointsManager pricepoints);

	protected abstract void ShowGoodsGroups(XsollaGroupsManager groups);

	protected abstract void UpdateGoods(XsollaGoodsManager goods);

	protected abstract void ShowQuickPaymentsList(XsollaUtils utils, XsollaQuickPayments paymentMethods);

	protected abstract void ShowPaymentsList(XsollaPaymentMethods paymentMethods);

	protected abstract void ShowSavedPaymentsList(XsollaSavedPaymentMethods savedPaymentsMethods);

	protected abstract void ShowCountries(XsollaCountries paymentMethods);

	protected abstract void ApplyPromoCouponeCode(XsollaForm pForm);

	protected abstract void ShowHistory(XsollaHistoryList pList);

	protected abstract void UpdateCustomAmount(CustomVirtCurrAmountController.CustomAmountCalcRes pRes);

	protected abstract void ShowSubs(XsollaSubscriptions pSubs);

	protected abstract void ShowPaymentForm(XsollaUtils utils, XsollaForm form);

	protected abstract void ShowPaymentStatus(XsollaTranslations translations, XsollaStatus status);

	protected abstract void CheckUnfinishedPaymentStatus(XsollaStatus status, XsollaForm form);

	protected abstract void ShowVPSummary(XsollaUtils utils, XVirtualPaymentSummary summary);

	protected abstract void ShowVPError(XsollaUtils utils, string error);

	protected abstract void ShowVPStatus(XsollaUtils utils, XVPStatus status);

	protected abstract void GetCouponErrorProceed(XsollaCouponProceedResult presult);

	protected abstract void PaymentManagerRecieved(XsollaSavedPaymentMethods pResult, bool pAddState);

	protected abstract void DeleteSavedPaymentMethodRecieved();

	protected abstract void WaitChangeSavedMethod();

	protected abstract void SubsManagerListRecived(XsollaManagerSubscriptions pSubsList);

	public void OpenPaystation(string accessToken, bool isSandbox)
	{
		SetLoading(isSandbox);
		Logger.isLogRequired = true;
		Logger.Log("Paystation initiated current mode sandbox");
		currentPurchase = new ActivePurchase();
		JSONNode jSONNode = JSON.Parse(accessToken);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (jSONNode == null)
		{
			isSimple = false;
			dictionary.Add("access_token", accessToken);
			BaseParams = "access_token=" + accessToken;
		}
		else
		{
			isSimple = true;
			dictionary.Add("access_data", accessToken);
			BaseParams = "access_data=" + accessToken;
		}
		StartPayment(dictionary, isSandbox);
	}

	public static void AddHttpRequestObj()
	{
		if (GameObject.Find(HttpTlsRequest.loaderGameObjName) == null)
		{
			(UnityEngine.Object.Instantiate(Resources.Load("Prefabs/GameLoader")) as GameObject).name = HttpTlsRequest.loaderGameObjName;
		}
	}

	private void StartPayment(Dictionary<string, object> dict, bool isSandbox)
	{
		Logger.Log("Request prepared");
		currentPurchase.Add(ActivePurchase.Part.TOKEN, dict);
		IsSandbox = isSandbox;
		if (isSimple)
		{
			CheckUnfinished();
		}
		XsollaPaymentImpl payment = Payment;
		payment.UtilsRecieved = (Action<XsollaUtils>)Delegate.Combine(payment.UtilsRecieved, new Action<XsollaUtils>(RecieveUtils));
		XsollaPaymentImpl payment2 = Payment;
		payment2.FormReceived = (Action<XsollaForm>)Delegate.Combine(payment2.FormReceived, (Action<XsollaForm>)delegate(XsollaForm form)
		{
			ShowPaymentForm(Utils, form);
		});
		XsollaPaymentImpl payment3 = Payment;
		payment3.StatusReceived = (Action<XsollaStatus, XsollaForm>)Delegate.Combine(payment3.StatusReceived, (Action<XsollaStatus, XsollaForm>)delegate(XsollaStatus status, XsollaForm form)
		{
			FillPurchase(ActivePurchase.Part.XPS, form.GetXpsMap());
			ShowPaymentStatus(Utils.GetTranslations(), status);
		});
		XsollaPaymentImpl payment4 = Payment;
		payment4.ApplyCouponeCodeReceived = (Action<XsollaForm>)Delegate.Combine(payment4.ApplyCouponeCodeReceived, (Action<XsollaForm>)delegate(XsollaForm form)
		{
			ApplyPromoCouponeCode(form);
		});
		XsollaPaymentImpl payment5 = Payment;
		payment5.StatusChecked = (Action<XsollaStatusPing>)Delegate.Combine(payment5.StatusChecked, (Action<XsollaStatusPing>)delegate(XsollaStatusPing status)
		{
			WaitingStatus(status);
		});
		XsollaPaymentImpl payment6 = Payment;
		payment6.QuickPaymentMethodsRecieved = (Action<XsollaQuickPayments>)Delegate.Combine(payment6.QuickPaymentMethodsRecieved, (Action<XsollaQuickPayments>)delegate(XsollaQuickPayments quickpayments)
		{
			ShowQuickPaymentsList(Utils, quickpayments);
		});
		XsollaPaymentImpl payment7 = Payment;
		payment7.PaymentMethodsRecieved = (Action<XsollaPaymentMethods>)Delegate.Combine(payment7.PaymentMethodsRecieved, new Action<XsollaPaymentMethods>(ShowPaymentsList));
		XsollaPaymentImpl payment8 = Payment;
		payment8.SavedPaymentMethodsRecieved = (Action<XsollaSavedPaymentMethods>)Delegate.Combine(payment8.SavedPaymentMethodsRecieved, new Action<XsollaSavedPaymentMethods>(ShowSavedPaymentsList));
		XsollaPaymentImpl payment9 = Payment;
		payment9.CountriesRecieved = (Action<XsollaCountries>)Delegate.Combine(payment9.CountriesRecieved, new Action<XsollaCountries>(ShowCountries));
		XsollaPaymentImpl payment10 = Payment;
		payment10.HistoryRecieved = (Action<XsollaHistoryList>)Delegate.Combine(payment10.HistoryRecieved, new Action<XsollaHistoryList>(ShowHistory));
		XsollaPaymentImpl payment11 = Payment;
		payment11.PricepointsRecieved = (Action<XsollaPricepointsManager>)Delegate.Combine(payment11.PricepointsRecieved, (Action<XsollaPricepointsManager>)delegate(XsollaPricepointsManager pricepoints)
		{
			ShowPricepoints(Utils, pricepoints);
		});
		XsollaPaymentImpl payment12 = Payment;
		payment12.GoodsGroupsRecieved = (Action<XsollaGroupsManager>)Delegate.Combine(payment12.GoodsGroupsRecieved, (Action<XsollaGroupsManager>)delegate(XsollaGroupsManager goods)
		{
			ShowGoodsGroups(goods);
		});
		XsollaPaymentImpl payment13 = Payment;
		payment13.GoodsRecieved = (Action<XsollaGoodsManager>)Delegate.Combine(payment13.GoodsRecieved, (Action<XsollaGoodsManager>)delegate(XsollaGoodsManager goods)
		{
			UpdateGoods(goods);
		});
		XsollaPaymentImpl payment14 = Payment;
		payment14.CustomAmountCalcRecieved = (Action<CustomVirtCurrAmountController.CustomAmountCalcRes>)Delegate.Combine(payment14.CustomAmountCalcRecieved, (Action<CustomVirtCurrAmountController.CustomAmountCalcRes>)delegate(CustomVirtCurrAmountController.CustomAmountCalcRes calcRes)
		{
			UpdateCustomAmount(calcRes);
		});
		XsollaPaymentImpl payment15 = Payment;
		payment15.SubsReceived = (Action<XsollaSubscriptions>)Delegate.Combine(payment15.SubsReceived, (Action<XsollaSubscriptions>)delegate(XsollaSubscriptions pSubs)
		{
			ShowSubs(pSubs);
		});
		XsollaPaymentImpl payment16 = Payment;
		payment16.VirtualPaymentSummaryRecieved = (Action<XVirtualPaymentSummary>)Delegate.Combine(payment16.VirtualPaymentSummaryRecieved, (Action<XVirtualPaymentSummary>)delegate(XVirtualPaymentSummary summary)
		{
			ShowVPSummary(Utils, summary);
		});
		XsollaPaymentImpl payment17 = Payment;
		payment17.VirtualPaymentProceedError = (Action<string>)Delegate.Combine(payment17.VirtualPaymentProceedError, (Action<string>)delegate(string error)
		{
			ShowVPError(Utils, error);
		});
		XsollaPaymentImpl payment18 = Payment;
		payment18.VirtualPaymentStatusRecieved = (Action<XVPStatus>)Delegate.Combine(payment18.VirtualPaymentStatusRecieved, (Action<XVPStatus>)delegate(XVPStatus status)
		{
			ShowVPStatus(Utils, status);
		});
		XsollaPaymentImpl payment19 = Payment;
		payment19.CouponProceedErrorRecived = (Action<XsollaCouponProceedResult>)Delegate.Combine(payment19.CouponProceedErrorRecived, (Action<XsollaCouponProceedResult>)delegate(XsollaCouponProceedResult proceed)
		{
			GetCouponErrorProceed(proceed);
		});
		XsollaPaymentImpl payment20 = Payment;
		payment20.PaymentManagerMethods = (Action<XsollaSavedPaymentMethods, bool>)Delegate.Combine(payment20.PaymentManagerMethods, (Action<XsollaSavedPaymentMethods, bool>)delegate(XsollaSavedPaymentMethods savedMethods, bool addState)
		{
			PaymentManagerRecieved(savedMethods, addState);
		});
		XsollaPaymentImpl payment21 = Payment;
		payment21.DeleteSavedPaymentMethodRespond = (Action)Delegate.Combine(payment21.DeleteSavedPaymentMethodRespond, (Action)delegate
		{
			DeleteSavedPaymentMethodRecieved();
		});
		XsollaPaymentImpl payment22 = Payment;
		payment22.WaitChangeSavedMethods = (Action)Delegate.Combine(payment22.WaitChangeSavedMethods, (Action)delegate
		{
			WaitChangeSavedMethod();
		});
		XsollaPaymentImpl payment23 = Payment;
		payment23.SubsManagerListRecived = (Action<XsollaManagerSubscriptions>)Delegate.Combine(payment23.SubsManagerListRecived, (Action<XsollaManagerSubscriptions>)delegate(XsollaManagerSubscriptions SubsList)
		{
			SubsManagerListRecived(SubsList);
		});
		XsollaPaymentImpl payment24 = Payment;
		payment24.ErrorReceived = (Action<XsollaError>)Delegate.Combine(payment24.ErrorReceived, new Action<XsollaError>(ShowPaymentError));
		Payment.SetModeSandbox(isSandbox);
		Payment.InitPaystation(currentPurchase.GetMergedMap());
	}

	private void CheckUnfinished()
	{
		Logger.Log("Check unfinished payments");
		if (TransactionHelper.CheckUnfinished())
		{
			Logger.Log("Have unfinished payments");
			XsollaPaymentImpl payment = Payment;
			payment.StatusReceived = (Action<XsollaStatus, XsollaForm>)Delegate.Combine(payment.StatusReceived, new Action<XsollaStatus, XsollaForm>(CheckUnfinishedPaymentStatus));
			Dictionary<string, object> dictionary = TransactionHelper.LoadRequest();
			if (dictionary != null)
			{
				Payment.GetStatus(dictionary);
				return;
			}
			TransactionHelper.Clear();
			Payment = null;
		}
	}

	protected void NextPaymentStep(Dictionary<string, object> xpsMap)
	{
		Logger.Log("Next Payment Step request");
		SetLoading(isLoading: true);
		Payment.NextStep(xpsMap);
	}

	private void SelectRadioItem(RadioButton.RadioType pType)
	{
		GetComponentInParent<XsollaPaystationController>().SelectRadioItem(pType);
	}

	public void LoadShopPricepoints()
	{
		Logger.Log("Load Pricepoints request");
		SetLoading(isLoading: true);
		Payment.GetPricePoints(currentPurchase.GetMergedMap());
		SelectRadioItem(RadioButton.RadioType.SCREEN_PRICEPOINT);
	}

	public void LoadGoodsGroups()
	{
		Logger.Log("Load Goods Groups request");
		SetLoading(isLoading: true);
		Payment.GetItemsGroups(currentPurchase.GetMergedMap());
		SelectRadioItem(RadioButton.RadioType.SCREEN_GOODS);
	}

	public void LoadGoods(long groupId)
	{
		Logger.Log("Load Goods request");
		Payment.GetItems(groupId, currentPurchase.GetMergedMap());
	}

	public void LoadSubscriptions()
	{
		Logger.Log("Load subscriptions");
		SetLoading(isLoading: true);
		Payment.GetSubscriptions();
		SelectRadioItem(RadioButton.RadioType.SCREEN_SUBSCRIPTION);
	}

	public void LoadFavorites()
	{
		Logger.Log("Load Favorites request");
		Payment.GetFavorites(currentPurchase.GetMergedMap());
		SelectRadioItem(RadioButton.RadioType.SCREEN_FAVOURITE);
	}

	public void GetCouponProceed(string pCouponCode)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("coupon_code", pCouponCode);
		Payment.GetCouponProceed(dictionary);
	}

	public void SetFavorite(Dictionary<string, object> items)
	{
		foreach (KeyValuePair<string, object> item in currentPurchase.GetPart(ActivePurchase.Part.TOKEN))
		{
			items.Add(item.Key, item.Value);
		}
		Payment.SetFavorite(items);
	}

	public void LoadQuickPayment()
	{
		Logger.Log("Load Quick Payments request");
		if (currentPurchase != null && currentPurchase.counter > 2)
		{
			currentPurchase.Remove(ActivePurchase.Part.PID);
			currentPurchase.Remove(ActivePurchase.Part.XPS);
		}
		LoadSavedPaymentMethods();
		LoadPaymentMethods();
		LoadCountries();
		SetLoading(isLoading: true);
	}

	public void LoadPaymentMethods()
	{
		Logger.Log("Load Payment Methods request");
		SetLoading(isLoading: true);
		Payment.GetPayments(_countryCurr, currentPurchase.GetMergedMap());
	}

	public void LoadSavedPaymentMethods()
	{
		Logger.Log("Load saved payments methods");
		SetLoading(isLoading: true);
		Payment.GetSavedPayments(currentPurchase.GetMergedMap());
	}

	public void LoadCountries()
	{
		Logger.Log("Load Countries request");
		SetLoading(isLoading: true);
		Payment.GetCountries(currentPurchase.GetMergedMap());
	}

	public void LoadHistory(Dictionary<string, object> pParams)
	{
		Payment.GetHistory(pParams);
	}

	public void LoadPaymentManager()
	{
		Logger.Log("Show Payment manager");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("userInitialCurrency", Utils.GetUser().userBalance.currency);
		Payment.GetSavedPaymentsForManager(dictionary);
	}

	public void LoadSubscriptionsManager()
	{
		Logger.Log("Show Subscription manager");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("userInitialCurrency", Utils.GetUser().userBalance.currency);
		Payment.GetSubscriptionForManager(dictionary);
		SetLoading(isLoading: true);
	}

	public void UpdateCountries(string countryIso)
	{
		Logger.Log("Update Countries request");
		_countryCurr = countryIso;
		Payment.GetPayments(countryIso, currentPurchase.GetMergedMap());
	}

	public void ChooseItem(Dictionary<string, object> items)
	{
		Logger.Log("Choose item request");
		if (isSimple)
		{
			TransactionHelper.SavePurchase(items);
		}
		result = new XsollaResult(new Dictionary<string, object>(items));
		currentPurchase.Remove(ActivePurchase.Part.PID);
		currentPurchase.Remove(ActivePurchase.Part.XPS);
		FillPurchase(ActivePurchase.Part.ITEM, items);
		TryPay();
	}

	public void ChooseItem(Dictionary<string, object> items, bool isVirtualPayment)
	{
		Logger.Log("Choose item request");
		if (isSimple)
		{
			TransactionHelper.SavePurchase(items);
		}
		result = new XsollaResult(new Dictionary<string, object>(items));
		currentPurchase.Remove(ActivePurchase.Part.PID);
		currentPurchase.Remove(ActivePurchase.Part.XPS);
		FillPurchase(ActivePurchase.Part.ITEM, items);
		if (!isVirtualPayment)
		{
			TryPay();
			return;
		}
		SetLoading(isLoading: true);
		Payment.GetVPSummary(currentPurchase.GetMergedMap());
	}

	public void ProceedVirtualPayment(Dictionary<string, object> items)
	{
		Logger.Log("Proceed VirtualPayment");
		FillPurchase(ActivePurchase.Part.PROCEED, items);
		SetLoading(isLoading: true);
		Payment.ProceedVPayment(currentPurchase.GetMergedMap());
	}

	public void ChoosePaymentMethod(Dictionary<string, object> items)
	{
		Logger.Log("Choose payment method request");
		items.Add("returnUrl", "https://secure.xsolla.com/paystation3/#/desktop/return/?" + BaseParams);
		FillPurchase(ActivePurchase.Part.PID, items);
		TryPay();
	}

	public void ApplyPromoCoupone(Dictionary<string, object> items)
	{
		Logger.Log("Apply promo-coupone");
		FillPurchase(ActivePurchase.Part.XPS, items);
		TryApplyCoupone();
	}

	public void DeleteSavedPaymentMethod(Dictionary<string, object> pParams)
	{
		Logger.Log("Delete method");
		Payment.DeleteSavedMethod(pParams);
	}

	public void ReplacedOnSavedMethod(Dictionary<string, object> pParams)
	{
		Logger.Log("Replaced saved method on saved method");
		FillPurchase(ActivePurchase.Part.PAYMENT_MANAGER_REPLACED, pParams);
		TryPay();
	}

	public void DoPayment(Dictionary<string, object> items)
	{
		Logger.Log("Do payment");
		currentPurchase.Remove(ActivePurchase.Part.INVOICE);
		FillPurchase(ActivePurchase.Part.XPS, items);
		TryPay();
	}

	public void GetStatus(Dictionary<string, object> items)
	{
		Logger.Log("Get Status");
		FillPurchase(ActivePurchase.Part.INVOICE, items);
		Payment.NextStep(currentPurchase.GetMergedMap());
	}

	public void CalcCustomAmount(Dictionary<string, object> pParam)
	{
		Logger.Log("Calc custom amount");
		Payment.CalculateCustomAmount(pParam);
	}

	protected void Restart()
	{
		Logger.Log("Restart payment");
		currentPurchase.RemoveAllExceptToken();
	}

	public void RetryPayment()
	{
		Logger.Log("Retry payment");
		TryPay();
	}

	public void FillPurchase(ActivePurchase.Part part, Dictionary<string, object> items)
	{
		if (currentPurchase == null)
		{
			currentPurchase = new ActivePurchase();
			currentPurchase.Add(part, new Dictionary<string, object>(items));
		}
		else
		{
			currentPurchase.Remove(part);
			currentPurchase.Add(part, new Dictionary<string, object>(items));
		}
	}

	public void RemovePurchasePart(ActivePurchase.Part pPart)
	{
		Logger.Log("Remove purchase part: " + pPart);
		if (currentPurchase != null && currentPurchase.ContainsKey(pPart))
		{
			currentPurchase.Remove(pPart);
		}
		else
		{
			Logger.Log("purchase is null or don't contain key");
		}
	}

	private void TryApplyCoupone()
	{
		Dictionary<string, object> mergedMap = currentPurchase.GetMergedMap();
		if (!mergedMap.ContainsKey("xps_fix_command"))
		{
			mergedMap.Add("xps_fix_command", "calculate");
		}
		else
		{
			mergedMap["xps_fix_command"] = "calculate";
		}
		if (!mergedMap.ContainsKey("xps_change_element"))
		{
			mergedMap.Add("xps_change_element", "couponCode");
		}
		else
		{
			mergedMap["xps_change_element"] = "couponCode";
		}
		Payment.ApplyPromoCoupone(mergedMap);
	}

	private void TryPay()
	{
		Logger.Log("Try pay");
		if (Utils.GetPurchase() != null)
		{
			if (currentPurchase.counter >= 2)
			{
				NextPaymentStep(currentPurchase.GetMergedMap());
			}
			else
			{
				LoadQuickPayment();
			}
		}
		else if (currentPurchase.counter >= 3)
		{
			NextPaymentStep(currentPurchase.GetMergedMap());
		}
		else
		{
			LoadQuickPayment();
		}
	}

	protected virtual void RecieveUtils(XsollaUtils utils)
	{
		Logger.Log("Utils recived");
		Utils = utils;
		if (isSimple)
		{
			BaseParams = BaseParams + "&access_token=" + utils.GetAcceessToken();
			currentPurchase.GetPart(ActivePurchase.Part.TOKEN).Remove("access_data");
			currentPurchase.GetPart(ActivePurchase.Part.TOKEN).Add("access_token", utils.GetAcceessToken());
		}
		XsollaPurchase purchase = utils.GetPurchase();
		if (purchase != null)
		{
			bool flag = purchase.IsPurchase();
			if (purchase.paymentSystem != null && flag)
			{
				NextPaymentStep(currentPurchase.GetMergedMap());
			}
			else if (flag)
			{
				LoadQuickPayment();
			}
			else
			{
				LoadShop(utils);
			}
		}
		else
		{
			LoadShop(utils);
		}
		SetLoading(isLoading: false);
	}

	private void LoadShop(XsollaUtils utils)
	{
		Logger.Log("Load Shop request");
		XsollaPaystation2 paystation = utils.GetSettings().paystation2;
		if (paystation.goodsAtFirst != null && paystation.goodsAtFirst.Equals("1"))
		{
			LoadGoodsGroups();
		}
		else if (paystation.pricepointsAtFirst != null && paystation.pricepointsAtFirst.Equals("1"))
		{
			LoadShopPricepoints();
		}
	}

	public void LoadShop()
	{
		Logger.Log("Load Shop request");
		if (Utils != null)
		{
			XsollaPaystation2 paystation = Utils.GetSettings().paystation2;
			if (paystation.goodsAtFirst != null && paystation.goodsAtFirst.Equals("1"))
			{
				LoadGoodsGroups();
			}
			else if (paystation.pricepointsAtFirst != null && paystation.pricepointsAtFirst.Equals("1"))
			{
				LoadShopPricepoints();
			}
		}
	}

	protected void WaitingStatus(XsollaStatusPing pStatus)
	{
		Logger.Log("Waiting payment status");
		if (XsollaStatus.Group.DONE != pStatus.GetGroup() && XsollaStatus.Group.TROUBLED != pStatus.GetGroup() && pStatus.GetElapsedTiem() < 1200)
		{
			if (pStatus.isFinal())
			{
				LoadShopPricepoints();
			}
			else
			{
				StartCoroutine(Test());
			}
		}
		else
		{
			currentPurchase.Remove(ActivePurchase.Part.INVOICE);
			TryPay();
		}
	}

	private IEnumerator Test()
	{
		yield return new WaitForSeconds(2f);
		Payment.NextStep(currentPurchase.GetMergedMap());
	}

	protected abstract void ShowPaymentError(XsollaError error);

	protected abstract void SetLoading(bool isLoading);
}
