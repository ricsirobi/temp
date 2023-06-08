using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class XsollaPaystationController : XsollaPaystation
{
	public enum ActiveScreen
	{
		SHOP,
		P_LIST,
		VP_PAYMENT,
		PAYMENT,
		STATUS,
		ERROR,
		UNKNOWN,
		FAV_ITEMS_LIST,
		REDEEM_COUPONS,
		HISTORY_LIST,
		SUBSCRIPTIONS,
		PAYMENT_MANAGER
	}

	private const string PREFAB_SCREEN_STATUS = "Prefabs/SimpleView/_ScreenStatus/ScreenStatusNew";

	private const string PREFAB_SCREEN_ERROR = "Prefabs/Screens/ScreenError";

	private const string PREFAB_SCREEN_ERROR_MAIN = "Prefabs/Screens/MainScreenError";

	private const string PREFAB_SCREEN_CHECKOUT = "Prefabs/SimpleView/_ScreenCheckout/ScreenCheckout";

	private const string PREFAB_SCREEN_VP_SUMMARY = "Prefabs/SimpleView/_ScreenVirtualPaymentSummary/ScreenVirtualPaymentSummary";

	private const string PREFAB_SCREEN_REDEEM_COUPON = "Prefabs/SimpleView/_ScreenShop/RedeemCouponView";

	private const string PREFAB_SCREEN_HISTORY_USER = "Prefabs/SimpleView/_ScreenShop/HistoryView";

	private const string PREFAB_SCREEN_SUBSCRIPTIONS = "Prefabs/SimpleView/_ScreenShop/SubscriptionsView";

	private const string PREFAB_VIEW_MENU_ITEM = "Prefabs/SimpleView/MenuItem";

	private const string PREFAB_VIEW_MENU_ITEM_ICON = "Prefabs/SimpleView/MenuItemIcon";

	private const string PREFAB_VIEW_MENU_ITEM_EMPTY = "Prefabs/SimpleView/MenuItemEmpty";

	private const string PREFAB_SCREEN_PAYMENT_MANAGER = "Prefabs/Screens/ScreenPaymentManager";

	private const string PREFAB_SCREEN_SUBSCRIPTION_MANAGER = "Prefabs/Screens/SubsManager/ScreenSubsManager";

	public GameObject mainScreen;

	public MyRotation progressBar;

	private bool isMainScreenShowed;

	public GameObject shopScreenPrefab;

	public GameObject paymentListScreenPrefab;

	public GameObject container;

	private PaymentListScreenController _paymentListScreenController;

	private ShopViewController _shopViewController;

	private RedeemCouponViewController _couponController;

	private SubscriptionsViewController _subsController;

	private RadioGroupController _radioController;

	private PaymentManagerController _SavedPaymentController;

	private SubsManagerController _SubsManagerController;

	private static ActiveScreen currentActive = ActiveScreen.UNKNOWN;

	private Transform menuTransform;

	private GameObject mainScreenContainer;

	private XVirtualPaymentSummary _summary;

	public event Action<XsollaResult> OkHandler;

	public event Action<XsollaError> ErrorHandler;

	protected override void RecieveUtils(XsollaUtils utils)
	{
		Singleton<StyleManager>.Instance.ChangeTheme(utils.GetSettings().GetTheme());
		mainScreen = UnityEngine.Object.Instantiate(mainScreen);
		mainScreen.transform.SetParent(container.transform);
		mainScreen.SetActive(value: true);
		mainScreenContainer = mainScreen.GetComponentsInChildren<ScrollRect>()[0].gameObject;
		menuTransform = mainScreen.GetComponentsInChildren<RectTransform>()[8].transform;
		Resizer.ResizeToParrent(mainScreen);
		base.RecieveUtils(utils);
		InitHeader(utils);
		InitFooter(utils);
		if (utils.GetPurchase() == null || !utils.GetPurchase().IsPurchase())
		{
			InitMenu(utils);
		}
	}

	protected override void ShowPricepoints(XsollaUtils utils, XsollaPricepointsManager pricepoints)
	{
		Logger.Log("Pricepoints recived");
		OpenPricepoints(utils, pricepoints);
		SetLoading(isLoading: false);
	}

	protected override void ShowGoodsGroups(XsollaGroupsManager groups)
	{
		Logger.Log("Goods Groups recived");
		OpenGoods(groups);
	}

	protected override void UpdateGoods(XsollaGoodsManager goods)
	{
		Logger.Log("Goods recived");
		goods.setItemVirtCurrName(Utils.GetProject().virtualCurrencyName);
		_shopViewController.UpdateGoods(goods, Utils.GetTranslations().Get(XsollaTranslations.VIRTUAL_ITEM_OPTION_BUTTON));
		SetLoading(isLoading: false);
	}

	protected override void ShowPaymentForm(XsollaUtils utils, XsollaForm form)
	{
		Logger.Log("Payment Form recived");
		DrawForm(utils, form);
		SetLoading(isLoading: false);
	}

	protected override void ShowPaymentStatus(XsollaTranslations translations, XsollaStatus status)
	{
		Logger.Log("Status recived");
		SetLoading(isLoading: false);
		DrawStatus(translations, status);
	}

	protected override void CheckUnfinishedPaymentStatus(XsollaStatus status, XsollaForm form)
	{
		Logger.Log("Check Unfinished Payment Status");
		if (status.GetGroup() != XsollaStatus.Group.DONE)
		{
			return;
		}
		Dictionary<string, object> dictionary = TransactionHelper.LoadPurchase();
		XsollaResult xsollaResult = new XsollaResult(dictionary);
		xsollaResult.invoice = status.GetStatusData().GetInvoice();
		xsollaResult.status = status.GetStatusData().GetStatus();
		Logger.Log("Ivoice ID " + xsollaResult.invoice);
		Logger.Log("Bought", dictionary);
		if (TransactionHelper.LogPurchase(xsollaResult.invoice))
		{
			if (this.OkHandler != null)
			{
				this.OkHandler(xsollaResult);
			}
		}
		else
		{
			Logger.Log("Already added");
		}
		TransactionHelper.Clear();
	}

	protected override void ShowPaymentError(XsollaError error)
	{
		Logger.Log("Show Payment Error " + error);
		SetLoading(isLoading: false);
		DrawError(error);
	}

	protected override void ShowSubs(XsollaSubscriptions pSubs)
	{
		Logger.Log("Show subscriptions");
		DrawSubscriptions(pSubs);
		SetLoading(isLoading: false);
	}

	private void DrawPaymentListScreen()
	{
		currentActive = ActiveScreen.P_LIST;
		if (_paymentListScreenController == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(paymentListScreenPrefab);
			_paymentListScreenController = gameObject.GetComponent<PaymentListScreenController>();
			_paymentListScreenController.transform.SetParent(mainScreenContainer.transform);
			_paymentListScreenController.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
			mainScreenContainer.GetComponentInParent<ScrollRect>().content = _paymentListScreenController.GetComponent<RectTransform>();
		}
	}

	protected override void ShowQuickPaymentsList(XsollaUtils utils, XsollaQuickPayments quickPayments)
	{
	}

	protected override void ShowPaymentsList(XsollaPaymentMethods paymentMethods)
	{
		DrawPaymentListScreen();
		_paymentListScreenController.InitScreen(Utils);
		_paymentListScreenController.SetPaymentsMethods(paymentMethods);
		_paymentListScreenController.OpenPayments();
		SetLoading(isLoading: false);
	}

	protected override void ShowSavedPaymentsList(XsollaSavedPaymentMethods savedPaymentsMethods)
	{
		DrawPaymentListScreen();
		_paymentListScreenController.SetSavedPaymentsMethods(savedPaymentsMethods);
	}

	protected override void ShowCountries(XsollaCountries countries)
	{
		DrawPaymentListScreen();
		_paymentListScreenController.SetCountries(_countryCurr, countries, Utils);
	}

	protected override void ShowVPSummary(XsollaUtils utils, XVirtualPaymentSummary summary)
	{
		SetLoading(isLoading: false);
		DrawVPSummary(utils, summary);
	}

	protected override void ShowVPError(XsollaUtils utils, string error)
	{
		SetLoading(isLoading: false);
		DrawVPError(utils, error);
	}

	protected override void ShowVPStatus(XsollaUtils utils, XVPStatus status)
	{
		SetLoading(isLoading: false);
		DrawVPStatus(utils, status);
	}

	protected override void ApplyPromoCouponeCode(XsollaForm pForm)
	{
		Logger.Log("Apply promo recieved");
		PromoCodeController componentInChildren = mainScreenContainer.GetComponentInChildren<PromoCodeController>();
		if (pForm.GetError() != null)
		{
			if (pForm.GetError().elementName == "couponCode")
			{
				componentInChildren.SetError(pForm.GetError());
			}
			return;
		}
		RightTowerController componentInChildren2 = mainScreenContainer.GetComponentInChildren<RightTowerController>();
		if (componentInChildren2 != null)
		{
			componentInChildren2.UpdateDiscont(Utils.GetTranslations(), pForm.GetSummary());
		}
		PaymentFormController componentInChildren3 = mainScreenContainer.GetComponentInChildren<PaymentFormController>();
		if (componentInChildren3 != null)
		{
			componentInChildren3.layout.objects[componentInChildren3.layout.objects.Count - 1].gameObject.GetComponentsInChildren<Text>()[1].text = Utils.GetTranslations().Get(XsollaTranslations.TOTAL) + " " + pForm.GetSumTotal();
		}
		componentInChildren.ApplySuccessful();
	}

	protected override void GetCouponErrorProceed(XsollaCouponProceedResult pResult)
	{
		Logger.Log(pResult.ToString());
		if (_couponController != null)
		{
			_couponController.ShowError(pResult._error);
		}
	}

	protected override void ShowHistory(XsollaHistoryList pList)
	{
		HistoryController historyController = UnityEngine.Object.FindObjectOfType<HistoryController>();
		if (historyController != null)
		{
			historyController = UnityEngine.Object.FindObjectOfType<HistoryController>();
			if (!historyController.IsRefresh())
			{
				historyController.AddListRows(Utils.GetTranslations(), pList);
			}
			else
			{
				historyController.InitScreen(Utils.GetTranslations(), pList, Utils.GetProject().virtualCurrencyName);
			}
			return;
		}
		currentActive = ActiveScreen.HISTORY_LIST;
		GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenShop/HistoryView")) as GameObject;
		historyController = obj.GetComponent<HistoryController>();
		if (historyController != null)
		{
			historyController.InitScreen(Utils.GetTranslations(), pList, Utils.GetProject().virtualCurrencyName);
		}
		obj.transform.SetParent(mainScreenContainer.transform);
		obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		Resizer.ResizeToParrent(obj);
	}

	protected override void UpdateCustomAmount(CustomVirtCurrAmountController.CustomAmountCalcRes pRes)
	{
		CustomVirtCurrAmountController customVirtCurrAmountController = UnityEngine.Object.FindObjectOfType<CustomVirtCurrAmountController>();
		if (customVirtCurrAmountController != null)
		{
			customVirtCurrAmountController.setValues(pRes);
		}
		else
		{
			Logger.Log("Custom amount controller not found");
		}
	}

	protected override void PaymentManagerRecieved(XsollaSavedPaymentMethods pResult, bool pAddState)
	{
		if (_SavedPaymentController == null)
		{
			Resizer.DestroyChilds(mainScreenContainer.transform);
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/ScreenPaymentManager")) as GameObject;
			_SavedPaymentController = gameObject.GetComponent<PaymentManagerController>();
			_SavedPaymentController.setOnCloseMethod(delegate
			{
				_radioController.SelectItem(RadioButton.RadioType.SCREEN_GOODS);
				LoadGoodsGroups();
			});
			gameObject.transform.SetParent(mainScreenContainer.transform);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
			Resizer.ResizeToParrent(gameObject);
			currentActive = ActiveScreen.PAYMENT_MANAGER;
		}
		SetLoading(isLoading: false);
		Restart();
		_SavedPaymentController.initScreen(Utils, pResult, AddPaymentAccount, pAddState);
	}

	protected override void DeleteSavedPaymentMethodRecieved()
	{
		if (currentActive == ActiveScreen.PAYMENT_MANAGER && _SavedPaymentController != null)
		{
			_SavedPaymentController.SetStatusDeleteOk();
			LoadPaymentManager();
		}
	}

	protected override void WaitChangeSavedMethod()
	{
		if (_SavedPaymentController == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/ScreenPaymentManager")) as GameObject;
			_SavedPaymentController = gameObject.GetComponent<PaymentManagerController>();
			_SavedPaymentController.setOnCloseMethod(delegate
			{
				_radioController.SelectItem(RadioButton.RadioType.SCREEN_GOODS);
				LoadGoodsGroups();
			});
			gameObject.transform.SetParent(mainScreenContainer.transform);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
			Resizer.ResizeToParrent(gameObject);
			currentActive = ActiveScreen.PAYMENT_MANAGER;
		}
		SetLoading(isLoading: false);
		Restart();
		_SavedPaymentController.initWaitScreen(Utils, AddPaymentAccount);
	}

	protected override void SubsManagerListRecived(XsollaManagerSubscriptions pSubsList)
	{
		if (_SubsManagerController == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/ScreenSubsManager")) as GameObject;
			_SubsManagerController = gameObject.GetComponent<SubsManagerController>();
			_SubsManagerController.initScreen(Utils, pSubsList);
			gameObject.transform.SetParent(mainScreenContainer.transform);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
			Resizer.ResizeToParrent(gameObject);
		}
		else
		{
			_SubsManagerController.initScreen(Utils, pSubsList);
		}
		SetLoading(isLoading: false);
	}

	private void DrawShopScreen()
	{
		currentActive = ActiveScreen.SHOP;
		if (_shopViewController == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(shopScreenPrefab);
			_shopViewController = gameObject.GetComponent<ShopViewController>();
			_shopViewController.DestroyAfter = DestroyShopScreen;
			_shopViewController.transform.SetParent(mainScreenContainer.transform);
			_shopViewController.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
			mainScreenContainer.GetComponentInParent<ScrollRect>().content = _shopViewController.GetComponent<RectTransform>();
		}
	}

	public void OpenPricepoints(XsollaUtils utils, XsollaPricepointsManager pricepoints)
	{
		DrawShopScreen();
		string title = utils.GetTranslations().Get(XsollaTranslations.PRICEPOINT_PAGE_TITLE);
		string virtualCurrencyName = utils.GetProject().virtualCurrencyName;
		string buyBtnText = utils.GetTranslations().Get(XsollaTranslations.VIRTUAL_ITEM_OPTION_BUTTON);
		if (utils.GetSettings().components.virtualCurreny.customAmount)
		{
			_shopViewController.OpenPricepoints(title, pricepoints, virtualCurrencyName, buyBtnText, pCustomHref: true, utils);
		}
		else
		{
			_shopViewController.OpenPricepoints(title, pricepoints, virtualCurrencyName, buyBtnText);
		}
	}

	public void OpenGoods(XsollaGroupsManager groups)
	{
		DrawShopScreen();
		ShowFavorityBtn();
		LoadGoods(groups.GetItemByPosition(0).id);
		_shopViewController.OpenGoods(groups);
		_radioController.SelectItem(0);
	}

	public void DestroyShopScreen()
	{
		HideFavorityBtn();
	}

	private void ShowFavorityBtn()
	{
		_radioController.radioButtons.Find((RadioButton x) => x.getType() == RadioButton.RadioType.SCREEN_FAVOURITE).visibleBtn(pState: true);
	}

	private void HideFavorityBtn()
	{
		_radioController.radioButtons.Find((RadioButton x) => x.getType() == RadioButton.RadioType.SCREEN_FAVOURITE).visibleBtn(pState: false);
	}

	private void DrawStatus(XsollaTranslations translations, XsollaStatus status)
	{
		currentActive = ActiveScreen.STATUS;
		menuTransform.gameObject.SetActive(value: false);
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenStatus/ScreenStatusNew")) as GameObject;
		gameObject.transform.SetParent(mainScreenContainer.transform);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		mainScreenContainer.GetComponentInParent<ScrollRect>().content = gameObject.GetComponent<RectTransform>();
		StatusViewController component = gameObject.GetComponent<StatusViewController>();
		component.StatusHandler += OnUserStatusExit;
		component.InitScreen(translations, status);
	}

	private void DrawSubscriptions(XsollaSubscriptions pSubs)
	{
		currentActive = ActiveScreen.SUBSCRIPTIONS;
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenShop/SubscriptionsView")) as GameObject;
		Resizer.DestroyChilds(mainScreenContainer.transform);
		gameObject.transform.SetParent(mainScreenContainer.transform);
		gameObject.transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		Resizer.ResizeToParrent(gameObject);
		mainScreenContainer.GetComponentInParent<ScrollRect>().content = gameObject.GetComponent<RectTransform>();
		_subsController = gameObject.GetComponent<SubscriptionsViewController>();
		_subsController.InitScreen(Utils.GetTranslations(), pSubs);
	}

	public void ShowRedeemCoupon()
	{
		currentActive = ActiveScreen.REDEEM_COUPONS;
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenShop/RedeemCouponView")) as GameObject;
		Resizer.DestroyChilds(mainScreenContainer.transform);
		gameObject.transform.SetParent(mainScreenContainer.transform);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		Resizer.ResizeToParrent(gameObject);
		mainScreenContainer.GetComponentInParent<ScrollRect>().content = gameObject.GetComponent<RectTransform>();
		_couponController = gameObject.GetComponent<RedeemCouponViewController>();
		_couponController.InitScreen(Utils);
		_couponController._btnApply.onClick.AddListener(delegate
		{
			CouponApplyClick(_couponController.GetCode());
		});
		SelectRadioItem(RadioButton.RadioType.SCREEN_REDEEMCOUPON);
	}

	private void CouponApplyClick(string pCode)
	{
		_couponController.HideError();
		Logger.Log("ClickApply - " + pCode);
		GetCouponProceed(pCode);
	}

	private void AddPaymentAccount()
	{
		Logger.Log("Click addAccount");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("save_payment_account_only", 1);
		FillPurchase(ActivePurchase.Part.PAYMENT_MANAGER, dictionary);
		LoadQuickPayment();
	}

	private void setCurrentScreenValue(ActiveScreen pValue)
	{
		currentActive = pValue;
	}

	private void DrawError(XsollaError error)
	{
		if (mainScreenContainer != null)
		{
			currentActive = ActiveScreen.ERROR;
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/ScreenError")) as GameObject;
			gameObject.transform.SetParent(mainScreenContainer.transform);
			gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
			mainScreenContainer.GetComponentInParent<ScrollRect>().content = gameObject.GetComponent<RectTransform>();
			ScreenErrorController component = gameObject.GetComponent<ScreenErrorController>();
			component.ErrorHandler += OnErrorReceived;
			component.DrawScreen(error);
		}
		else
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/MainScreenError")) as GameObject;
			obj.transform.SetParent(container.transform);
			Text[] componentsInChildren = obj.GetComponentsInChildren<Text>();
			componentsInChildren[1].text = "Something went wrong";
			componentsInChildren[2].text = error.errorMessage;
			componentsInChildren[3].text = error.errorCode.ToString();
			componentsInChildren[3].gameObject.SetActive(value: false);
			Resizer.ResizeToParrent(obj);
		}
	}

	private void DrawForm(XsollaUtils utils, XsollaForm form)
	{
		currentActive = ActiveScreen.PAYMENT;
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenCheckout/ScreenCheckout")) as GameObject;
		gameObject.transform.SetParent(mainScreenContainer.transform);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		mainScreenContainer.GetComponentInParent<ScrollRect>().content = gameObject.GetComponent<RectTransform>();
		gameObject.GetComponent<ScreenCheckoutController>().InitScreen(utils, form);
	}

	private void DrawVPSummary(XsollaUtils utils, XVirtualPaymentSummary summary)
	{
		_summary = summary;
		currentActive = ActiveScreen.VP_PAYMENT;
		menuTransform.gameObject.SetActive(value: true);
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenVirtualPaymentSummary/ScreenVirtualPaymentSummary")) as GameObject;
		gameObject.transform.SetParent(mainScreenContainer.transform);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		mainScreenContainer.GetComponentInParent<ScrollRect>().content = gameObject.GetComponent<RectTransform>();
		gameObject.GetComponent<ScreenVPController>().DrawScreen(utils, summary);
	}

	private void DrawVPError(XsollaUtils utils, string error)
	{
		currentActive = ActiveScreen.VP_PAYMENT;
		menuTransform.gameObject.SetActive(value: true);
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenVirtualPaymentSummary/ScreenVirtualPaymentSummary")) as GameObject;
		gameObject.transform.SetParent(mainScreenContainer.transform);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		mainScreenContainer.GetComponentInParent<ScrollRect>().content = gameObject.GetComponent<RectTransform>();
		ScreenVPController component = gameObject.GetComponent<ScreenVPController>();
		component.DrawScreen(utils, _summary);
		component.ShowError(error);
	}

	private void DrawVPStatus(XsollaUtils utils, XVPStatus status)
	{
		currentActive = ActiveScreen.STATUS;
		menuTransform.gameObject.SetActive(value: false);
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenStatus/ScreenStatusNew")) as GameObject;
		gameObject.transform.SetParent(mainScreenContainer.transform);
		gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
		mainScreenContainer.GetComponentInParent<ScrollRect>().content = gameObject.GetComponent<RectTransform>();
		StatusViewController component = gameObject.GetComponent<StatusViewController>();
		component.StatusHandler += OnUserStatusExit;
		component.DrawVpStatus(utils, status);
	}

	protected override void SetLoading(bool isLoading)
	{
		if (!isMainScreenShowed)
		{
			if (isLoading)
			{
				mainScreen.SetActive(value: false);
			}
			else
			{
				mainScreen.SetActive(value: true);
				isMainScreenShowed = true;
			}
		}
		else if (isLoading)
		{
			Resizer.DestroyChilds(mainScreenContainer.transform);
		}
		progressBar.SetLoading(isLoading);
	}

	private void InitHeader(XsollaUtils utils)
	{
		mainScreen.GetComponentInChildren<MainHeaderController>().InitScreen(utils);
	}

	private void InitMenu(XsollaUtils utils)
	{
		_radioController = menuTransform.gameObject.AddComponent<RadioGroupController>();
		GameObject original = Resources.Load("Prefabs/SimpleView/MenuItem") as GameObject;
		GameObject original2 = Resources.Load("Prefabs/SimpleView/MenuItemIcon") as GameObject;
		GameObject original3 = Resources.Load("Prefabs/SimpleView/MenuItemEmpty") as GameObject;
		Dictionary<string, XComponent> components = utils.GetProject().components;
		if (components.ContainsKey("items") && components["items"].IsEnabled)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(original);
			Text[] componentsInChildren = gameObject.GetComponentsInChildren<Text>();
			componentsInChildren[0].text = "\ue01c";
			componentsInChildren[1].text = ((components["items"].Name != "null") ? components["items"].Name : utils.GetTranslations().Get(XsollaTranslations.VIRTUALITEM_PAGE_TITLE));
			gameObject.GetComponent<RadioButton>().setType(RadioButton.RadioType.SCREEN_GOODS);
			gameObject.GetComponent<Button>().onClick.AddListener(delegate
			{
				_radioController.SelectItem(RadioButton.RadioType.SCREEN_GOODS);
				LoadGoodsGroups();
			});
			gameObject.transform.SetParent(menuTransform);
			_radioController.AddButton(gameObject.GetComponent<RadioButton>());
		}
		if (components.ContainsKey("virtual_currency") && components["virtual_currency"].IsEnabled)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(original);
			Text[] componentsInChildren2 = gameObject2.GetComponentsInChildren<Text>();
			componentsInChildren2[0].text = "\ue00e";
			componentsInChildren2[1].text = ((components["virtual_currency"].Name != "null") ? components["virtual_currency"].Name : utils.GetTranslations().Get(XsollaTranslations.PRICEPOINT_PAGE_TITLE));
			gameObject2.GetComponent<RadioButton>().setType(RadioButton.RadioType.SCREEN_PRICEPOINT);
			gameObject2.GetComponent<Button>().onClick.AddListener(delegate
			{
				_radioController.SelectItem(RadioButton.RadioType.SCREEN_PRICEPOINT);
				LoadShopPricepoints();
			});
			gameObject2.transform.SetParent(menuTransform);
			_radioController.AddButton(gameObject2.GetComponent<RadioButton>());
		}
		if (components.ContainsKey("subscriptions") && components["subscriptions"].IsEnabled)
		{
			GameObject gameObject3 = UnityEngine.Object.Instantiate(original);
			Text[] componentsInChildren3 = gameObject3.GetComponentsInChildren<Text>();
			componentsInChildren3[0].text = "\ue01f";
			componentsInChildren3[1].text = ((components["subscriptions"].Name != "null") ? components["subscriptions"].Name : utils.GetTranslations().Get(XsollaTranslations.SUBSCRIPTION_PAGE_TITLE));
			gameObject3.GetComponent<RadioButton>().setType(RadioButton.RadioType.SCREEN_SUBSCRIPTION);
			gameObject3.GetComponent<Button>().onClick.AddListener(delegate
			{
				_radioController.SelectItem(RadioButton.RadioType.SCREEN_SUBSCRIPTION);
				LoadSubscriptions();
			});
			gameObject3.transform.SetParent(menuTransform);
			_radioController.AddButton(gameObject3.GetComponent<RadioButton>());
		}
		if (components.ContainsKey("coupons") && components["coupons"].IsEnabled)
		{
			GameObject gameObject4 = UnityEngine.Object.Instantiate(original);
			Text[] componentsInChildren4 = gameObject4.GetComponentsInChildren<Text>();
			componentsInChildren4[0].text = "\ue00d";
			componentsInChildren4[1].text = ((components["coupons"].Name != "null") ? components["coupons"].Name : utils.GetTranslations().Get(XsollaTranslations.COUPON_PAGE_TITLE));
			gameObject4.GetComponent<RadioButton>().setType(RadioButton.RadioType.SCREEN_REDEEMCOUPON);
			gameObject4.GetComponent<Button>().onClick.AddListener(delegate
			{
				_radioController.SelectItem(RadioButton.RadioType.SCREEN_REDEEMCOUPON);
				ShowRedeemCoupon();
			});
			gameObject4.transform.SetParent(menuTransform);
			_radioController.AddButton(gameObject4.GetComponent<RadioButton>());
		}
		UnityEngine.Object.Instantiate(original3).transform.SetParent(menuTransform);
		GameObject gameObject5 = UnityEngine.Object.Instantiate(original2);
		gameObject5.GetComponentInChildren<Text>().text = "\ue01d";
		gameObject5.GetComponent<RadioButton>().setType(RadioButton.RadioType.SCREEN_FAVOURITE);
		gameObject5.GetComponent<Button>().onClick.AddListener(delegate
		{
			_shopViewController.SetTitle(utils.GetTranslations().Get(XsollaTranslations.VIRTUALITEMS_TITLE_FAVORITE));
			_radioController.SelectItem(RadioButton.RadioType.SCREEN_FAVOURITE);
			LoadFavorites();
		});
		gameObject5.transform.SetParent(menuTransform);
		_radioController.AddButton(gameObject5.GetComponent<RadioButton>());
	}

	public void SelectRadioItem(RadioButton.RadioType pType)
	{
		if (_radioController != null)
		{
			_radioController.SelectItem(pType);
		}
	}

	private void InitFooter(XsollaUtils utils)
	{
		if (utils != null)
		{
			Text[] componentsInChildren = mainScreen.GetComponentsInChildren<Text>();
			XsollaTranslations translations = utils.GetTranslations();
			componentsInChildren[4].text = translations.Get(XsollaTranslations.SUPPORT_CUSTOMER_SUPPORT);
			componentsInChildren[5].text = translations.Get(XsollaTranslations.SUPPORT_CONTACT_US);
			componentsInChildren[6].text = translations.Get(XsollaTranslations.XSOLLA_COPYRIGHT);
			componentsInChildren[7].text = translations.Get(XsollaTranslations.FOOTER_SECURED_CONNECTION);
			componentsInChildren[8].text = translations.Get(XsollaTranslations.FOOTER_AGREEMENT);
		}
	}

	private void OnUserStatusExit(XsollaStatus.Group group, string invoice, XsollaStatusData.Status status, Dictionary<string, object> pPurchase = null)
	{
		Logger.Log("On user exit status screen");
		switch (group)
		{
		case XsollaStatus.Group.DONE:
			Logger.Log("Status Done");
			menuTransform.gameObject.SetActive(value: true);
			if (result == null)
			{
				result = new XsollaResult();
			}
			result.invoice = invoice;
			result.status = status;
			if (pPurchase != null)
			{
				result.purchases = pPurchase;
			}
			Logger.Log("Ivoice ID " + result.invoice);
			Logger.Log("Status " + result.status);
			Logger.Log("Bought", result.purchases);
			TransactionHelper.Clear();
			if (this.OkHandler != null)
			{
				this.OkHandler(result);
			}
			else
			{
				Logger.Log("Have no OkHandler");
			}
			break;
		default:
			if (result != null)
			{
				result.invoice = invoice;
				result.status = status;
				Logger.Log("Ivoice ID " + result.invoice);
				Logger.Log("Status " + result.status);
				Logger.Log("Bought", result.purchases);
			}
			TransactionHelper.Clear();
			if (this.OkHandler != null)
			{
				this.OkHandler(result);
			}
			else
			{
				Logger.Log("Have no OkHandler");
			}
			break;
		}
	}

	private void TryAgain()
	{
		SetLoading(isLoading: true);
		menuTransform.gameObject.SetActive(value: true);
		Restart();
	}

	private void OnErrorReceived(XsollaError xsollaError)
	{
		Logger.Log("ErrorRecivied " + xsollaError.ToString());
		if (this.ErrorHandler != null)
		{
			this.ErrorHandler(xsollaError);
		}
		else
		{
			Logger.Log("Have no ErrorHandler");
		}
	}

	private void OnDestroy()
	{
		Logger.Log("User close window");
		if (currentActive == ActiveScreen.STATUS)
		{
			Logger.Log("Check payment status");
			StatusViewController componentInChildren = GetComponentInChildren<StatusViewController>();
			if (componentInChildren != null)
			{
				componentInChildren.statusViewExitButton.onClick.Invoke();
			}
		}
		Logger.Log("Handle chancel");
		if (this.ErrorHandler != null)
		{
			this.ErrorHandler(XsollaError.GetCancelError());
		}
	}
}
