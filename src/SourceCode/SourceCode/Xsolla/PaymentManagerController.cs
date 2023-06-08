using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class PaymentManagerController : MonoBehaviour
{
	public Text mTitle;

	public Text mContinueLink;

	public GameObject mInfoPanel;

	public Text mInformationTitle;

	public Text mInformation;

	public GameObject mContainer;

	public GameObject mBtnGrid;

	public GameObject mBtnAddPaymentObj;

	public ImageLoader mImgLoader;

	public GameObject mReplacePanelMethod;

	public GameObject mPanelForReplacedMethods;

	public GameObject mLinkGetAnotherMethods;

	public GameObject mLinkBack;

	public GameObject mBtnContinue;

	public GameObject mDelPanelMethod;

	public GameObject mPanelForDelMethod;

	public Text mQuestionLabel;

	public GameObject mLinkCancel;

	public GameObject mLinkDelete;

	public GameObject mBtnReplace;

	public GameObject mDelStatusPanel;

	public Button mCloseNotify;

	public GameObject mWaitChangeScreen;

	public GameObject mCancelWaitBtn;

	public Text mCanceltext;

	public MyRotation mProgressBar;

	private XsollaUtils mUtilsLink;

	private Action mActionAddPayment;

	private ArrayList mListBtnsObjs;

	private Action mOnClose;

	private ArrayList mListReplacedMethods;

	private XsollaSavedPaymentMethod mSelectedMethod;

	private Action _addPaymentMethod;

	private XsollaSavedPaymentMethods _MethodsOnWaitLoop;

	public XsollaSavedPaymentMethods mListMethods { get; set; }

	public void setOnCloseMethod(Action pAction)
	{
		mOnClose = pAction;
	}

	public void initWaitScreen(XsollaUtils pUtils, Action pAddPaymentMethod)
	{
		mUtilsLink = pUtils;
		_addPaymentMethod = pAddPaymentMethod;
		mWaitChangeScreen.SetActive(value: true);
		mProgressBar.SetLoading(isLoading: true);
		_MethodsOnWaitLoop = null;
		InvokeRepeating("StartGetSavedMethodLoop", 0f, 5f);
		mCancelWaitBtn.GetComponent<Button>().onClick.AddListener(delegate
		{
			CancelWait();
		});
		mCanceltext.text = pUtils.GetTranslations().Get("cancel");
	}

	private void StartGetSavedMethodLoop()
	{
		GetSavedMethod();
	}

	private void CancelWait()
	{
		mProgressBar.SetLoading(isLoading: false);
		mWaitChangeScreen.SetActive(value: false);
		CancelInvoke("StartGetSavedMethodLoop");
		initScreen(mUtilsLink, _MethodsOnWaitLoop, _addPaymentMethod, pAddState: false);
	}

	private void GetSavedMethod(bool pAddState = false, bool pInitAfter = false)
	{
		WWWForm wWWForm = new WWWForm();
		string text = "https://secure.xsolla.com/paystation2/api/savedmethods";
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		StringBuilder stringBuilder = new StringBuilder();
		dictionary.Add("access_token", mUtilsLink.GetAcceessToken());
		foreach (KeyValuePair<string, object> item in dictionary)
		{
			string value = ((item.Value != null) ? item.Value.ToString() : "");
			stringBuilder.Append(item.Key).Append("=").Append(value)
				.Append("&");
			wWWForm.AddField(item.Key, value);
		}
		Debug.Log(text);
		Debug.Log(stringBuilder.ToString());
		WWW www = new WWW(text, wWWForm);
		StartCoroutine(GetListSavedMethod(www, pInitAfter, pAddState));
	}

	private IEnumerator GetListSavedMethod(WWW www, bool pInitAfter, bool pAddMethod)
	{
		Debug.Log("Wait saved account list");
		yield return www;
		if (www.error != null)
		{
			yield break;
		}
		Debug.Log("WWW_request -> " + www.text);
		JSONNode jSONNode = JSON.Parse(www.text);
		if ((!(jSONNode != null) || jSONNode.Count <= 2) && !(jSONNode["error"] == null))
		{
			yield break;
		}
		XsollaSavedPaymentMethods xsollaSavedPaymentMethods = new XsollaSavedPaymentMethods();
		xsollaSavedPaymentMethods.Parse(jSONNode);
		if (pInitAfter)
		{
			initScreen(mUtilsLink, xsollaSavedPaymentMethods, mActionAddPayment, pAddState: false);
			if (pAddMethod)
			{
				SetStatusAddOk();
			}
			else
			{
				SetStatusReplaceOk();
			}
		}
		else if (_MethodsOnWaitLoop == null)
		{
			_MethodsOnWaitLoop = xsollaSavedPaymentMethods;
		}
		else if (xsollaSavedPaymentMethods.Equals(_MethodsOnWaitLoop))
		{
			_MethodsOnWaitLoop = xsollaSavedPaymentMethods;
		}
		else
		{
			Logger.Log("Stop wait end show result");
			_MethodsOnWaitLoop = xsollaSavedPaymentMethods;
			SetStatusAddOk();
			CancelWait();
		}
	}

	public void initScreen(XsollaUtils pUtils, XsollaSavedPaymentMethods pMethods, Action pAddPaymentMethod, bool pAddState)
	{
		mUtilsLink = pUtils;
		mActionAddPayment = pAddPaymentMethod;
		if (pMethods != null)
		{
			mListMethods = pMethods;
			if (mListBtnsObjs == null)
			{
				mListBtnsObjs = new ArrayList();
			}
			else
			{
				mListBtnsObjs.Clear();
			}
			mTitle.text = pUtils.GetTranslations().Get("payment_account_page_title");
			mInformationTitle.text = pUtils.GetTranslations().Get("payment_account_add_title");
			mInformation.text = pUtils.GetTranslations().Get("payment_account_add_info");
			mContinueLink.text = pUtils.GetTranslations().Get("payment_account_back_button");
			mCanceltext.text = pUtils.GetTranslations().Get("cancel");
			mCloseNotify.onClick.AddListener(CloseStatus);
			Button component = mContinueLink.GetComponent<Button>();
			component.onClick.RemoveAllListeners();
			component.onClick.AddListener(delegate
			{
				UnityEngine.Object.Destroy(base.gameObject);
				mOnClose();
			});
			mBtnAddPaymentObj.GetComponentInChildren<Text>().text = pUtils.GetTranslations().Get("payment_account_add_button");
			for (int i = 0; i < mBtnGrid.transform.childCount; i++)
			{
				UnityEngine.Object.Destroy(mBtnGrid.transform.GetChild(i).gameObject);
			}
			if (mListMethods.GetCount() == 0)
			{
				mContainer.SetActive(value: false);
				mDelPanelMethod.SetActive(value: false);
				mReplacePanelMethod.SetActive(value: false);
				mInfoPanel.SetActive(value: true);
				Button component2 = mBtnAddPaymentObj.GetComponent<Button>();
				component2.onClick.RemoveAllListeners();
				component2.onClick.AddListener(delegate
				{
					CloseStatus();
					mActionAddPayment();
				});
				return;
			}
			mInfoPanel.SetActive(value: false);
			mDelPanelMethod.SetActive(value: false);
			mReplacePanelMethod.SetActive(value: false);
			mContainer.SetActive(value: true);
			foreach (XsollaSavedPaymentMethod item in mListMethods.GetItemList())
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/SimpleView/_PaymentFormElements/SavedMethodBtnNew")) as GameObject;
				gameObject.transform.SetParent(mBtnGrid.transform);
				mListBtnsObjs.Add(gameObject);
				SavedMethodBtnController controller = gameObject.GetComponent<SavedMethodBtnController>();
				controller.setDeleteBtn(pState: true);
				controller.setMethodBtn(pState: false);
				controller.setDeleteBtnName(pUtils.GetTranslations().Get("delete_payment_account_button"));
				controller.setMethod(item);
				controller.setNameMethod(item.GetName());
				controller.setNameType(item.GetPsName());
				mImgLoader.LoadImage(controller._iconMethod, item.GetImageUrl());
				controller.getBtnDelete().onClick.AddListener(delegate
				{
					CloseStatus();
					onClickDeletePaymentMethod(controller);
				});
			}
			GameObject obj = UnityEngine.Object.Instantiate(mBtnAddPaymentObj);
			obj.transform.SetParent(mBtnGrid.transform);
			Button component3 = obj.GetComponent<Button>();
			component3.onClick.RemoveAllListeners();
			component3.onClick.AddListener(delegate
			{
				CloseStatus();
				mActionAddPayment();
			});
		}
		else
		{
			GetSavedMethod(pAddState, pInitAfter: true);
		}
	}

	public void SetStatusDeleteOk()
	{
		string pStatus = mUtilsLink.GetTranslations().Get("payment_account_message_delete_account_successfully");
		ShowStatusBar(pStatus);
	}

	public void SetStatusAddOk()
	{
		string pStatus = mUtilsLink.GetTranslations().Get("payment_account_message_success");
		ShowStatusBar(pStatus);
	}

	public void SetStatusReplaceOk()
	{
		string pStatus = mUtilsLink.GetTranslations().Get("payment_account_message_success_replace");
		ShowStatusBar(pStatus);
	}

	private void ShowStatusBar(string pStatus)
	{
		mDelStatusPanel.GetComponentInChildren<Text>().text = pStatus;
		mDelStatusPanel.SetActive(value: true);
	}

	private void CloseStatus()
	{
		mDelStatusPanel.SetActive(value: false);
	}

	private void initDeleteMethodPanel(SavedMethodBtnController pMethodObj)
	{
		mContainer.SetActive(value: false);
		mReplacePanelMethod.SetActive(value: false);
		mDelPanelMethod.SetActive(value: true);
		SavedMethodBtnController controller = UnityEngine.Object.Instantiate(pMethodObj);
		controller.setMethod(pMethodObj.getMethod());
		controller.setDeleteBtn(pState: false);
		controller.setMethodBtn(pState: false);
		RectTransform component = controller.GetComponent<RectTransform>();
		for (int i = 0; i < mPanelForDelMethod.transform.childCount; i++)
		{
			Logger.Log("Destroy child on panel for edit saved payment method with ind - " + i);
			UnityEngine.Object.Destroy(mPanelForDelMethod.transform.GetChild(i).gameObject);
		}
		controller.transform.SetParent(mPanelForDelMethod.transform);
		component.anchorMin = new Vector2(0f, 0f);
		component.anchorMax = new Vector2(1f, 1f);
		component.pivot = new Vector2(0.5f, 0.5f);
		component.offsetMin = new Vector2(0f, 0f);
		component.offsetMax = new Vector2(0f, 0f);
		mTitle.text = mUtilsLink.GetTranslations().Get("delete_payment_account_page_title");
		mQuestionLabel.text = mUtilsLink.GetTranslations().Get("payment_account_delete_confirmation_question");
		mLinkCancel.GetComponent<Text>().text = mUtilsLink.GetTranslations().Get("cancel");
		mLinkCancel.GetComponent<Button>().onClick.RemoveAllListeners();
		mLinkCancel.GetComponent<Button>().onClick.AddListener(delegate
		{
			onClickCancelEditMethod();
		});
		mLinkDelete.GetComponent<Text>().text = mUtilsLink.GetTranslations().Get("delete_payment_account_button");
		mLinkDelete.GetComponent<Button>().onClick.RemoveAllListeners();
		mLinkDelete.GetComponent<Button>().onClick.AddListener(delegate
		{
			onClickConfirmDeletePaymentMethod(controller.getMethod());
		});
		mBtnReplace.GetComponentInChildren<Text>().text = mUtilsLink.GetTranslations().Get("replace_payment_account_button");
		mBtnReplace.GetComponent<Button>().onClick.RemoveAllListeners();
		mBtnReplace.GetComponent<Button>().onClick.AddListener(delegate
		{
			onClickReplacePeymentMethod(controller.getMethod());
		});
	}

	private void onClickReplacePeymentMethod(XsollaSavedPaymentMethod pMethod)
	{
		Logger.Log("Click replace method");
		mListReplacedMethods = new ArrayList();
		mInfoPanel.SetActive(value: false);
		mDelPanelMethod.SetActive(value: false);
		mContainer.SetActive(value: false);
		mReplacePanelMethod.SetActive(value: true);
		for (int i = 0; i < mPanelForReplacedMethods.transform.childCount; i++)
		{
			Logger.Log("Destroy child on panel for edit saved payment method with ind - " + i);
			UnityEngine.Object.Destroy(mPanelForReplacedMethods.transform.GetChild(i).gameObject);
		}
		foreach (GameObject mListBtnsObj in mListBtnsObjs)
		{
			SavedMethodBtnController component = mListBtnsObj.GetComponent<SavedMethodBtnController>();
			if (!(component.getMethod().GetKey() == pMethod.GetKey()))
			{
				SavedMethodBtnController savedMethodBtnController = UnityEngine.Object.Instantiate(component);
				savedMethodBtnController.setMethod(component.getMethod());
				savedMethodBtnController.setDeleteBtn(pState: false);
				savedMethodBtnController.setMethodBtn(pState: false);
				savedMethodBtnController.setToggleObj(pState: true, onToggleChange);
				savedMethodBtnController.transform.SetParent(mPanelForReplacedMethods.transform);
				RectTransform component2 = savedMethodBtnController.GetComponent<RectTransform>();
				component2.anchorMin = new Vector2(0f, 0f);
				component2.anchorMax = new Vector2(1f, 1f);
				component2.pivot = new Vector2(0.5f, 0.5f);
				component2.offsetMin = new Vector2(0f, 0f);
				component2.offsetMax = new Vector2(0f, 0f);
				mListReplacedMethods.Add(savedMethodBtnController);
			}
		}
		mLinkGetAnotherMethods.GetComponent<Text>().text = mUtilsLink.GetTranslations().Get("savedmethod_other_change_account_label");
		Button component3 = mLinkGetAnotherMethods.GetComponent<Button>();
		component3.onClick.RemoveAllListeners();
		component3.onClick.AddListener(delegate
		{
			onClickConfirmReplacedAnotherMethod(pMethod);
		});
		mLinkBack.GetComponent<Text>().text = mUtilsLink.GetTranslations().Get("back_to_paymentaccount");
		Button component4 = mLinkBack.GetComponent<Button>();
		component4.onClick.RemoveAllListeners();
		component4.onClick.AddListener(delegate
		{
			onClickCancelEditMethod();
		});
		mBtnContinue.GetComponentInChildren<Text>().text = mUtilsLink.GetTranslations().Get("form_continue");
		Button component5 = mBtnContinue.GetComponent<Button>();
		component5.onClick.RemoveAllListeners();
		component5.onClick.AddListener(delegate
		{
			onClickConfirmReplaced(pMethod);
		});
	}

	private void onToggleChange(string pMethodKey, bool pState)
	{
		Logger.Log("Method with key " + pMethodKey + " get state " + pState);
		foreach (SavedMethodBtnController mListReplacedMethod in mListReplacedMethods)
		{
			if (mListReplacedMethod.getMethod().GetKey() == pMethodKey && pState)
			{
				mSelectedMethod = mListReplacedMethod.getMethod();
			}
			else
			{
				mListReplacedMethod.setToggleState(pState: false);
			}
		}
	}

	private void onClickConfirmReplaced(XsollaSavedPaymentMethod pMethod)
	{
		Logger.Log("Raplaced existing method");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("id_payment_account", pMethod.GetKey());
		dictionary.Add("saved_method_id", mSelectedMethod.GetKey());
		dictionary.Add("pid", mSelectedMethod.GetPid());
		dictionary.Add("paymentWithSavedMethod", 1);
		dictionary.Add("paymentSid", pMethod.GetFormSid());
		dictionary.Add("type_payment_account", pMethod.GetMethodType());
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2.Add("replace_payment_account", 1);
		XsollaPaystationController componentInParent = GetComponentInParent<XsollaPaystationController>();
		componentInParent.FillPurchase(ActivePurchase.Part.PAYMENT_MANAGER_REPLACED, dictionary2);
		componentInParent.ChoosePaymentMethod(dictionary);
	}

	private void onClickConfirmReplacedAnotherMethod(XsollaSavedPaymentMethod pMethod)
	{
		Logger.Log("Raplaced existing method");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("id_payment_account", pMethod.GetKey());
		dictionary.Add("replace_payment_account", 1);
		dictionary.Add("type_payment_account", pMethod.GetMethodType());
		GetComponentInParent<XsollaPaystationController>().ChooseItem(dictionary);
	}

	private void onClickConfirmDeletePaymentMethod(XsollaSavedPaymentMethod pMethod)
	{
		Logger.Log("Delete payment method");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("id", pMethod.GetKey());
		dictionary.Add("type", pMethod.GetMethodType());
		base.gameObject.GetComponentInParent<XsollaPaystationController>().DeleteSavedPaymentMethod(dictionary);
	}

	private void onClickCancelEditMethod()
	{
		Logger.Log("Cancel edit method");
		mInfoPanel.SetActive(value: false);
		mDelPanelMethod.SetActive(value: false);
		mReplacePanelMethod.SetActive(value: false);
		mContainer.SetActive(value: true);
		mTitle.text = mUtilsLink.GetTranslations().Get("payment_account_page_title");
	}

	private void onClickDeletePaymentMethod(SavedMethodBtnController pMethodObj)
	{
		Logger.Log("Click Btn to Delete saved method");
		initDeleteMethodPanel(pMethodObj);
	}
}
