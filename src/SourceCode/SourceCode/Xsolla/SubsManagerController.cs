using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SubsManagerController : MonoBehaviour
{
	public Text mTitleScreen;

	public Text mContinueText;

	public Button mContinueLink;

	public Text mLabel;

	public GameObject mSubsContainer;

	public GameObject mStatusPanel;

	public Text mStatusLabel;

	public GameObject mErrorPanel;

	public Text mErrorLabel;

	private XsollaUtils mUtils;

	private const string DOMAIN = "https://secure.xsolla.com";

	private const string mBtnPrefab = "Prefabs/Screens/SubsManager/Simple/SubManagerBtn";

	private const string mDetailPartPrefab = "Prefabs/Screens/SubsManager/Detail/SubDetailPart";

	private const string mDetailPaymentPartPrefab = "Prefabs/Screens/SubsManager/Detail/SubDetailPaymentPart";

	private const string mDetailChargePartPrefab = "Prefabs/Screens/SubsManager/Detail/SubDetailChargesPart";

	private const string mDetailNotifyPartPrefab = "Prefabs/Screens/SubsManager/Detail/SubDetailNotifyPart";

	private const string mDetailBackLinkPartPrefab = "Prefabs/Screens/SubsManager/Detail/BackLinkPart";

	private const string mDetailCancelLinkPartPrefab = "Prefabs/Screens/SubsManager/Detail/SubCancelChoose";

	private XsollaManagerSubDetails mLocalSubDetail;

	private SubManagerCancelPartController cancelSubsCtrl;

	private bool isLinkPaymentMethod = true;

	public void initScreen(XsollaUtils pUtils, XsollaManagerSubscriptions pSubsList)
	{
		mUtils = pUtils;
		mTitleScreen.text = mUtils.GetTranslations().Get("user_menu_user_subscription");
		mContinueText.text = mUtils.GetTranslations().Get("balance_back_button");
		mLabel.gameObject.SetActive(value: true);
		mLabel.text = mUtils.GetTranslations().Get("user_subscription_list_subtitle");
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in mSubsContainer.transform)
		{
			list.Add(item.gameObject);
		}
		list.ForEach(delegate(GameObject child)
		{
			UnityEngine.Object.Destroy(child);
		});
		setNotifyPanels();
		mContinueLink.onClick.RemoveAllListeners();
		mContinueLink.onClick.AddListener(OnClickBackShopAction);
		if (pSubsList.GetCount() == 0)
		{
			Logger.Log("Empty List subs");
			mLabel.text = mUtils.GetTranslations().Get("subscription_no_data");
			mLabel.alignment = TextAnchor.MiddleCenter;
		}
		foreach (XsollaManagerSubscription items in pSubsList.GetItemsList())
		{
			addSubBtn(items);
		}
	}

	private void setNotifyPanels()
	{
		mStatusPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
		mStatusPanel.GetComponentInChildren<Button>().onClick.AddListener(delegate
		{
			mStatusPanel.SetActive(value: false);
		});
		mErrorPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
		mErrorPanel.GetComponentInChildren<Button>().onClick.AddListener(delegate
		{
			mErrorPanel.SetActive(value: false);
		});
	}

	private void addSubBtn(XsollaManagerSubscription pSub)
	{
		GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Simple/SubManagerBtn")) as GameObject;
		SubManagerBtnController component = obj.GetComponent<SubManagerBtnController>();
		component.init(pSub, mUtils.GetTranslations());
		component.SetDetailAction(onDetailBtnClick);
		obj.transform.SetParent(mSubsContainer.transform);
	}

	private void onDetailBtnClick(XsollaManagerSubscription pSub)
	{
		Logger.Log("On Detail click. Id: " + pSub.GetKey());
		GetDetailSub(pSub.GetId());
	}

	private void GetDetailSub(int pSubId)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("access_token", mUtils.GetAcceessToken());
		dictionary.Add("subscription_id", pSubId);
		dictionary.Add("userInitialCurrency", mUtils.GetUser().userBalance.currency);
		getApiRequest("https://secure.xsolla.com/paystation2/api/useraccount/subscription", dictionary, callbackShowSubDetail);
	}

	private void getApiRequest(string pUrl, Dictionary<string, object> pParams, Action<JSONNode> pRecivedCallBack)
	{
		WWWForm wWWForm = new WWWForm();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, object> pParam in pParams)
		{
			string value = ((pParam.Value != null) ? pParam.Value.ToString() : "");
			stringBuilder.Append(pParam.Key).Append("=").Append(value)
				.Append("&");
			wWWForm.AddField(pParam.Key, value);
		}
		WWW pWww = new WWW(pUrl, wWWForm);
		StartCoroutine(getRequest(pWww, pRecivedCallBack));
	}

	private IEnumerator getRequest(WWW pWww, Action<JSONNode> pCallback)
	{
		yield return pWww;
		if (pWww.error == null)
		{
			JSONNode obj = JSON.Parse(pWww.text);
			pCallback(obj);
		}
		else
		{
			JSONNode jSONNode = JSON.Parse(pWww.error);
			showError(string.Format(StringHelper.PrepareFormatString(mUtils.GetTranslations().Get("error_code")), jSONNode["error"].Value));
		}
	}

	private void showStatus(string pLabel)
	{
		mStatusPanel.SetActive(value: true);
		mStatusLabel.text = pLabel;
	}

	private void showError(string pError)
	{
		mErrorPanel.SetActive(value: true);
		mErrorLabel.text = pError;
	}

	private void callbackShowSubDetail(JSONNode pNode)
	{
		mLocalSubDetail = new XsollaManagerSubDetails().Parse(pNode) as XsollaManagerSubDetails;
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in mSubsContainer.transform)
		{
			list.Add(item.gameObject);
		}
		list.ForEach(delegate(GameObject child)
		{
			UnityEngine.Object.Destroy(child);
		});
		mLabel.gameObject.SetActive(value: false);
		mTitleScreen.text = mUtils.GetTranslations().Get("user_subscription_info_page_title");
		if (mLocalSubDetail.mStatus != "non_renewing" && mLocalSubDetail.mPaymentMethodName == "null")
		{
			isLinkPaymentMethod = false;
			SubManagerNotifyPartController component = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Detail/SubDetailNotifyPart")) as GameObject).GetComponent<SubManagerNotifyPartController>();
			component.init(mUtils.GetTranslations().Get("user_subscription_payment_not_link_account_message"), mUtils.GetTranslations().Get("user_subscription_add"), mLocalSubDetail, OnLinkPaymentMethodAction);
			component.transform.SetParent(mSubsContainer.transform);
		}
		GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Detail/SubDetailPart")) as GameObject;
		SubManagerDetailPartController component2 = obj.GetComponent<SubManagerDetailPartController>();
		component2.initScreen(mLocalSubDetail, mUtils);
		component2.getHoldCancelBtn().onClick.AddListener(OnHoldCancelLinkAction);
		component2.getUnholdBtn().onClick.AddListener(OnUnHoldLinkAction);
		component2.getRenewBtn().onClick.AddListener(OnRenewBtnAction);
		obj.transform.SetParent(mSubsContainer.transform);
		if (mLocalSubDetail.mStatus != "non_renewing")
		{
			SubManagerDetailPaymentPartController component3 = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Detail/SubDetailPaymentPart")) as GameObject).GetComponent<SubManagerDetailPaymentPartController>();
			if (isLinkPaymentMethod)
			{
				component3.init(mLocalSubDetail, mUtils, OnUnlinkPaymentMethodAction);
			}
			else
			{
				component3.init(mLocalSubDetail, mUtils, OnLinkPaymentMethodAction);
			}
			component3.transform.SetParent(mSubsContainer.transform);
		}
		if (mLocalSubDetail.mCharges != null)
		{
			SubManagerDetailChargesPartController component4 = (UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Detail/SubDetailChargesPart")) as GameObject).GetComponent<SubManagerDetailChargesPartController>();
			component4.init(mLocalSubDetail, mUtils);
			component4.transform.SetParent(mSubsContainer.transform);
		}
		addBackLinkPart(mUtils.GetTranslations().Get("user_subscription_back_to_subscription_list"), OnClickBackSubsListAction);
	}

	private void ShowLocalSubDetail()
	{
		GetDetailSub(mLocalSubDetail.mId);
	}

	private void addBackLinkPart(string pLabelLink, Action pBackAction, string pConfirmBtnLabel = "", Action pConfirmBtnAction = null)
	{
		GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Detail/BackLinkPart")) as GameObject;
		obj.GetComponent<SubBackLinkPart>().init(pLabelLink, pBackAction, pConfirmBtnLabel, pConfirmBtnAction);
		obj.transform.SetParent(mSubsContainer.transform);
	}

	private void callbackUnlinkMethod(JSONNode pNode)
	{
		if (pNode["status"].Value == "saved")
		{
			ShowLocalSubDetail();
			showStatus(mUtils.GetTranslations().Get("user_subscription_message_unlink_payment_account"));
		}
	}

	private void callbackUnholdMethod(JSONNode pNode)
	{
		if (pNode["status"].Value == "saved")
		{
			ShowLocalSubDetail();
			showStatus(string.Format(mUtils.GetTranslations().Get("user_subscription_message_unhold_no_active")));
		}
	}

	private void callbackDontrenewMethod(JSONNode pNode)
	{
		if (pNode["status"].Value == "saved")
		{
			ShowLocalSubDetail();
			showStatus(string.Format(StringHelper.PrepareFormatString(mUtils.GetTranslations().Get("user_subscription_message_non_renewing")), StringHelper.DateFormat(mLocalSubDetail.mDateNextCharge)));
		}
	}

	private void callbackDeleteSubMethod(JSONNode pNode)
	{
		if (pNode["status"].Value == "saved")
		{
			OnClickBackSubsListAction();
			showStatus(string.Format(StringHelper.PrepareFormatString(mUtils.GetTranslations().Get("user_subscription_message_canceled")), mLocalSubDetail.mName, mUtils.GetProject().name));
		}
	}

	private void callbackGetSubsList(JSONNode pNode)
	{
		XsollaManagerSubscriptions pSubsList = new XsollaManagerSubscriptions().Parse(pNode["subscriptions"]) as XsollaManagerSubscriptions;
		initScreen(mUtils, pSubsList);
	}

	private void OnLinkPaymentMethodAction(XsollaManagerSubDetails pSubDetail)
	{
		Logger.Log("Link payment method");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("change_account", "1");
		dictionary.Add("id_recurrent_subscription", pSubDetail.mId);
		dictionary.Add("id_payment_account", "");
		dictionary.Add("subscription_payment_type", "charge");
		GetComponentInParent<XsollaPaystationController>().ChooseItem(dictionary);
	}

	private void OnUnlinkPaymentMethodAction(XsollaManagerSubDetails pSubDetail)
	{
		Logger.Log("Unlink payment method");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("access_token", mUtils.GetAcceessToken());
		dictionary.Add("subscription_id", pSubDetail.mId);
		dictionary.Add("userInitialCurrency", mUtils.GetUser().userBalance.currency);
		getApiRequest("https://secure.xsolla.com/paystation2/api/useraccount/unlinkpaymentaccount", dictionary, callbackUnlinkMethod);
	}

	private void OnRenewBtnAction()
	{
		Logger.Log("Link payment method");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("change_account", "0");
		dictionary.Add("id_recurrent_subscription", mLocalSubDetail.mId);
		dictionary.Add("id_payment_account", "");
		dictionary.Add("type_payment_account", "");
		GetComponentInParent<XsollaPaystationController>().ChooseItem(dictionary);
	}

	private void OnClickBackShopAction()
	{
		Logger.Log("On click back shop");
		UnityEngine.Object.FindObjectOfType<XsollaPaystation>().LoadShop();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnClickBackSubsListAction()
	{
		Logger.Log("On click back to subs");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("access_token", mUtils.GetAcceessToken());
		dictionary.Add("userInitialCurrency", mUtils.GetUser().userBalance.currency);
		getApiRequest("https://secure.xsolla.com/paystation2/api/useraccount/subscriptions", dictionary, callbackGetSubsList);
	}

	private void OnHoldCancelLinkAction()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in mSubsContainer.transform)
		{
			list.Add(item.gameObject);
		}
		list.ForEach(delegate(GameObject child)
		{
			UnityEngine.Object.Destroy(child);
		});
		GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Detail/SubCancelChoose")) as GameObject;
		cancelSubsCtrl = gameObject.GetComponent<SubManagerCancelPartController>();
		cancelSubsCtrl.init(mLocalSubDetail, mUtils);
		cancelSubsCtrl.transform.SetParent(mSubsContainer.transform);
		addBackLinkPart(mUtils.GetTranslations().Get("back_to_user_subscription_info"), ShowLocalSubDetail, mUtils.GetTranslations().Get("hold_subscription_confirm"), OnConfirmCancelSub);
	}

	private void OnConfirmCancelSub()
	{
		Logger.Log("On confirm cancel click");
		if (cancelSubsCtrl != null)
		{
			if (cancelSubsCtrl.isDeleteNow())
			{
				OnDeleteSubAction();
			}
			else
			{
				OnDontRenewAction();
			}
		}
	}

	private void OnUnHoldLinkAction()
	{
		Logger.Log("Unhold click");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("access_token", mUtils.GetAcceessToken());
		dictionary.Add("subscription_id", mLocalSubDetail.mId);
		dictionary.Add("userInitialCurrency", mUtils.GetUser().userBalance.currency);
		getApiRequest("https://secure.xsolla.com/paystation2/api/useraccount/unholdsubscription", dictionary, callbackUnholdMethod);
	}

	private void OnDontRenewAction()
	{
		Logger.Log("Don't renew");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("access_token", mUtils.GetAcceessToken());
		dictionary.Add("subscription_id", mLocalSubDetail.mId);
		dictionary.Add("userInitialCurrency", mUtils.GetUser().userBalance.currency);
		dictionary.Add("status", "non_renewing");
		getApiRequest("https://secure.xsolla.com/paystation2/api/useraccount/holdsubscription", dictionary, callbackDontrenewMethod);
	}

	private void OnDeleteSubAction()
	{
		Logger.Log("Delet now");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("access_token", mUtils.GetAcceessToken());
		dictionary.Add("subscription_id", mLocalSubDetail.mId);
		dictionary.Add("userInitialCurrency", mUtils.GetUser().userBalance.currency);
		dictionary.Add("status", "canceled");
		getApiRequest("https://secure.xsolla.com/paystation2/api/useraccount/holdsubscription", dictionary, callbackDeleteSubMethod);
	}
}
