using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SubManagerBtnController : MonoBehaviour
{
	public Text mSubName;

	public Text mNextInvoice;

	public Text mPaymentMethodName;

	public Text mDetailText;

	public Button mDetailBtn;

	public Button mSelfBtn;

	private XsollaManagerSubscription mSub;

	public void init(XsollaManagerSubscription pSub, XsollaTranslations pTranslation)
	{
		mSub = pSub;
		mSubName.text = mSub.GetName();
		string text = pTranslation.Get("next_charge");
		int num = 0;
		while (text.Contains("{{"))
		{
			string oldValue = text.Substring(text.IndexOf("{{", 0) + 1, text.IndexOf("}}", 0) - text.IndexOf("{{", 0));
			text = text.Replace(oldValue, num.ToString());
			num++;
		}
		if (pSub.mStatus == "active")
		{
			mNextInvoice.text = string.Format(text, mSub.mCharge.ToString(), StringHelper.DateFormat(pSub.mDateNextCharge));
		}
		else
		{
			mNextInvoice.gameObject.SetActive(value: false);
		}
		if (pSub.mPaymentMethod != "null")
		{
			mPaymentMethodName.text = pSub.mPaymentMethod + " " + pSub.mPaymentVisibleName;
		}
		else
		{
			string mStatus = pSub.mStatus;
			if (!(mStatus == "freeze"))
			{
				if (mStatus == "non_renewing")
				{
					mPaymentMethodName.text = string.Format(StringHelper.PrepareFormatString(pTranslation.Get("user_subscription_non_renewing")), StringHelper.DateFormat(pSub.mDateNextCharge));
				}
				else
				{
					mPaymentMethodName.gameObject.SetActive(value: false);
				}
			}
			else
			{
				mPaymentMethodName.text = string.Format(StringHelper.PrepareFormatString(pTranslation.Get("user_subscription_hold_to")), StringHelper.DateFormat(pSub.mHoldDates.dateTo));
			}
		}
		mDetailText.text = pTranslation.Get("user_subscription_to_details");
	}

	public void SetDetailAction(Action<XsollaManagerSubscription> pAction)
	{
		mDetailBtn.onClick.AddListener(delegate
		{
			pAction(mSub);
		});
		mSelfBtn.onClick.AddListener(delegate
		{
			pAction(mSub);
		});
	}
}
