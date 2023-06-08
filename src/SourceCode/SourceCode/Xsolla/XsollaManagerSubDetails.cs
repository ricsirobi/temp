using System;
using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaManagerSubDetails : IParseble
{
	public XsollaApi mApi;

	public List<XsollaSubDetailCharge> mCharges;

	public XsollaSubCharge mCharge;

	public DateTime mDateCreate;

	public DateTime mDateNextCharge;

	public string mDesc;

	public XsollaSubHoldDates mHoldDates;

	public int mId;

	public string mIdExternal;

	public bool mIsCancelPossible;

	public bool mIsChangePlanAllowed;

	public bool mIsHoldPossible;

	public bool mIsRenewPossible;

	public bool mIsSheduledHoldExist;

	public XsollaSubLimitHoldPeriod mLimitHoldPeriod;

	public string mName;

	public XsollaSubCharge mNextCharge;

	public XsollaSubNextPeriodPlanChange mNextPeriodPlanChange;

	public XsollaSubDetailPaymentAcc mPaymentAccount;

	public string mPaymentIcoSrc;

	public string mPaymentMethodName;

	public string mPaymentMethodType;

	public string mPaymentMethodVisName;

	public XsollaSubDetailPeriod mPeriod;

	public XsollaSubHoldDates mSheduledHoldDates;

	public string mStatus;

	public IParseble Parse(JSONNode rootNode)
	{
		mApi = new XsollaApi().Parse(rootNode["api"]) as XsollaApi;
		mCharges = new List<XsollaSubDetailCharge>();
		IEnumerator<JSONNode> enumerator = rootNode["charges"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			mCharges.Add(new XsollaSubDetailCharge().Parse(enumerator.Current) as XsollaSubDetailCharge);
		}
		JSONNode jSONNode = rootNode["subscription"];
		mCharge = new XsollaSubCharge().Parse(jSONNode["charge"]) as XsollaSubCharge;
		mDateCreate = DateTime.Parse(jSONNode["date_create"].Value);
		mDateNextCharge = DateTime.Parse(jSONNode["date_next_charge"].Value);
		mDesc = jSONNode["description"].Value;
		mHoldDates = new XsollaSubHoldDates().Parse(jSONNode["hold_dates"]) as XsollaSubHoldDates;
		mId = jSONNode["id"].AsInt;
		mIdExternal = jSONNode["id_external"].Value;
		mIsCancelPossible = jSONNode["is_cancel_possible"].AsBool;
		mIsChangePlanAllowed = jSONNode["is_change_plan_allowed"].AsBool;
		mIsHoldPossible = jSONNode["is_hold_possible"].AsBool;
		mIsRenewPossible = jSONNode["is_renew_possible"].AsBool;
		mIsSheduledHoldExist = jSONNode["is_scheduled_hold_exist"].AsBool;
		mLimitHoldPeriod = new XsollaSubLimitHoldPeriod().Parse(jSONNode["limit_hold_period"]) as XsollaSubLimitHoldPeriod;
		mName = jSONNode["name"].Value;
		mNextCharge = new XsollaSubCharge().Parse(jSONNode["next_charge"]) as XsollaSubCharge;
		mNextPeriodPlanChange = new XsollaSubNextPeriodPlanChange().Parse(jSONNode["next_period_plan_change"]) as XsollaSubNextPeriodPlanChange;
		mPaymentAccount = new XsollaSubDetailPaymentAcc().Parse(jSONNode["payment_account"]) as XsollaSubDetailPaymentAcc;
		mPaymentIcoSrc = jSONNode["payment_icon_src"].Value;
		mPaymentMethodName = jSONNode["payment_method"].Value;
		mPaymentMethodType = jSONNode["payment_type"].Value;
		mPaymentMethodVisName = jSONNode["payment_visible_name"].Value;
		mPeriod = new XsollaSubDetailPeriod().Parse(jSONNode["period"]) as XsollaSubDetailPeriod;
		mSheduledHoldDates = new XsollaSubHoldDates().Parse(jSONNode["scheduled_hold_dates"]) as XsollaSubHoldDates;
		mStatus = jSONNode["status"].Value;
		return this;
	}
}
