using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SubManagerDetailPaymentPartController : MonoBehaviour
{
	public Text mPartTitle;

	public GameObject mDetailsContainer;

	public GameObject mUnLinkBtn;

	private const string mLabeltextPrefab = "Prefabs/Screens/SubsManager/Simple/DetailLabelValue";

	private XsollaUtils mUtils;

	private XsollaManagerSubDetails mSubDetail;

	public Button getUnlinkBtn()
	{
		return mUnLinkBtn.GetComponent<Button>();
	}

	public void init(XsollaManagerSubDetails pSubDetail, XsollaUtils pUtils, Action<XsollaManagerSubDetails> pLinkAction)
	{
		mUtils = pUtils;
		mSubDetail = pSubDetail;
		mPartTitle.text = pUtils.GetTranslations().Get("user_subscription_payment_title");
		if (pSubDetail.mPaymentMethodType != "notify" && pSubDetail.mPaymentMethodName != "")
		{
			mUnLinkBtn.GetComponent<Text>().text = pUtils.GetTranslations().Get("user_subscription_unlink_payment_account");
		}
		else
		{
			mUnLinkBtn.GetComponent<Text>().text = pUtils.GetTranslations().Get("user_subscription_add");
		}
		getUnlinkBtn().onClick.AddListener(delegate
		{
			pLinkAction(pSubDetail);
		});
		foreach (LabelValue importDetail in getImportDetails())
		{
			GameObject obj = UnityEngine.Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Simple/DetailLabelValue")) as GameObject;
			obj.GetComponent<LabelValueController>().init(importDetail.label, importDetail.value, importDetail.actionLabel, importDetail.action);
			obj.transform.SetParent(mDetailsContainer.transform);
		}
	}

	public List<LabelValue> getImportDetails()
	{
		List<LabelValue> list = new List<LabelValue>();
		XsollaTranslations translations = mUtils.GetTranslations();
		if (mSubDetail.mPaymentMethodName != "null")
		{
			list.Add(new LabelValue(translations.Get("user_subscription_payment_method"), mSubDetail.mPaymentMethodName + " (" + mSubDetail.mPaymentMethodVisName + ")"));
		}
		list.Add(new LabelValue(translations.Get("user_subscription_next_bill_sum"), mSubDetail.mNextCharge.ToString()));
		list.Add(new LabelValue(translations.Get("user_subscription_next_bill_date"), StringHelper.DateFormat(mSubDetail.mDateNextCharge)));
		return list;
	}
}
