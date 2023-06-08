using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SubManagerDetailPartController : MonoBehaviour
{
	public Text mPartTitle;

	public Text mBtnRenewName;

	public Button mBtnRenew;

	public GameObject mDetailsContainer;

	public GameObject mLinkUnhold;

	public GameObject mLinkHoldCancel;

	public GameObject mLinkChangePlan;

	private const string mLabeltextPrefab = "Prefabs/Screens/SubsManager/Simple/DetailLabelValue";

	private XsollaUtils mUtils;

	private XsollaManagerSubDetails mSubDetail;

	public Button getUnholdBtn()
	{
		return mLinkUnhold.GetComponent<Button>();
	}

	public Button getHoldCancelBtn()
	{
		return mLinkHoldCancel.GetComponent<Button>();
	}

	public Button getChangePlanBtn()
	{
		return mLinkChangePlan.GetComponent<Button>();
	}

	public Button getRenewBtn()
	{
		return mBtnRenew;
	}

	public void initScreen(XsollaManagerSubDetails pSubDetail, XsollaUtils pUtils)
	{
		mUtils = pUtils;
		mSubDetail = pSubDetail;
		mPartTitle.text = pUtils.GetTranslations().Get("user_subscription_details_title");
		mBtnRenewName.text = pUtils.GetTranslations().Get("user_subscription_renew");
		mLinkUnhold.GetComponent<Text>().text = pUtils.GetTranslations().Get("user_subscription_unhold");
		mLinkHoldCancel.GetComponent<Text>().text = pUtils.GetTranslations().Get("hold_subscription_cancel_label");
		mLinkChangePlan.GetComponent<Text>().text = pUtils.GetTranslations().Get("user_subscription_change_plan");
		mBtnRenew.gameObject.SetActive(mSubDetail.mIsRenewPossible);
		mLinkUnhold.SetActive(mSubDetail.mStatus == "freeze");
		mLinkHoldCancel.SetActive((mSubDetail.mIsHoldPossible || mSubDetail.mStatus != "non_renewing") && mSubDetail.mStatus != "freeze");
		mLinkChangePlan.SetActive(value: false);
		foreach (LabelValue importDetail in getImportDetails())
		{
			GameObject obj = Object.Instantiate(Resources.Load("Prefabs/Screens/SubsManager/Simple/DetailLabelValue")) as GameObject;
			obj.GetComponent<LabelValueController>().init(importDetail.label, importDetail.value, importDetail.actionLabel, importDetail.action);
			obj.transform.SetParent(mDetailsContainer.transform);
		}
	}

	public List<LabelValue> getImportDetails()
	{
		List<LabelValue> list = new List<LabelValue>();
		XsollaTranslations translations = mUtils.GetTranslations();
		list.Add(new LabelValue(translations.Get("user_subscription_name"), mSubDetail.mName));
		list.Add(new LabelValue(translations.Get("user_subscription_status"), translations.Get("user_subscription_status_" + mSubDetail.mStatus)));
		list.Add(new LabelValue(translations.Get("user_subscription_charge"), mSubDetail.mCharge.ToString()));
		list.Add(new LabelValue(translations.Get("user_subscription_period"), formattedPeriod(mSubDetail.mPeriod.mValue.ToString(), mSubDetail.mPeriod.mUnit)));
		if (mSubDetail.mStatus == "non_renewing")
		{
			list.Add(new LabelValue(translations.Get("user_subscription_end_bill_date"), StringHelper.DateFormat(mSubDetail.mDateNextCharge)));
		}
		if (mSubDetail.mNextPeriodPlanChange != null)
		{
			list.Add(new LabelValue(translations.Get("user_subscription_new_plan"), string.Format(StringHelper.PrepareFormatString(translations.Get("user_subscription_next_period_plan_change")), mSubDetail.mNextPeriodPlanChange.name, StringHelper.DateFormat(mSubDetail.mNextPeriodPlanChange.date))));
		}
		if ((mSubDetail.mIsSheduledHoldExist && mSubDetail.mSheduledHoldDates != null) || (mSubDetail.mStatus == "freeze" && mSubDetail.mHoldDates != null))
		{
			string text = "";
			string text2 = "";
			if (mSubDetail.mStatus == "freeze")
			{
				if (mSubDetail.mHoldDates != null)
				{
					text = StringHelper.DateFormat(mSubDetail.mHoldDates.dateFrom);
					text2 = StringHelper.DateFormat(mSubDetail.mHoldDates.dateTo);
				}
			}
			else if (mSubDetail.mSheduledHoldDates != null)
			{
				text = StringHelper.DateFormat(mSubDetail.mSheduledHoldDates.dateFrom);
				text2 = StringHelper.DateFormat(mSubDetail.mSheduledHoldDates.dateTo);
			}
			if (mSubDetail.mIsSheduledHoldExist)
			{
				list.Add(new LabelValue(translations.Get("user_subscription_hold_dates"), text + " - " + text2, translations.Get((mSubDetail.mStatus == "freeze") ? "user_subscription_unhold" : "cancel"), cancelHoldDates));
			}
			else
			{
				list.Add(new LabelValue(translations.Get("user_subscription_hold_dates"), text + " - " + text2));
			}
		}
		return list;
	}

	private string formattedPeriod(string pValue, string pUnit)
	{
		string key = "period_" + pUnit + pValue;
		string text = mUtils.GetTranslations().Get(key);
		if (text == "")
		{
			text = mUtils.GetTranslations().Get("period_" + pUnit + "s");
		}
		return pValue + " " + text;
	}

	private void cancelHoldDates()
	{
		Logger.Log("Cancel hold dates with id - " + mSubDetail.mId);
		getUnholdBtn().onClick.Invoke();
	}
}
