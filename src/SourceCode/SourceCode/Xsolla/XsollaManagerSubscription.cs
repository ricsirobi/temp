using System;
using SimpleJSON;

namespace Xsolla;

public class XsollaManagerSubscription : IXsollaObject, IParseble
{
	public XsollaSubCharge mCharge;

	public DateTime mDateNextCharge;

	public string mDesc;

	public XsollaSubHoldDates mHoldDates;

	private int mId;

	private string mIdExternal;

	public string mName;

	public string mPaymentMethod;

	private string mPaymentType;

	public string mPaymentVisibleName;

	private XsollaSubPeriod mPeriod;

	private int mValue;

	public string mStatus;

	public IParseble Parse(JSONNode rootNode)
	{
		mCharge = new XsollaSubCharge().Parse(rootNode["charge"]) as XsollaSubCharge;
		if (rootNode["date_next_charge"].Value != "")
		{
			mDateNextCharge = DateTime.Parse(rootNode["date_next_charge"].Value);
		}
		mDesc = rootNode["description"];
		mHoldDates = new XsollaSubHoldDates().Parse(rootNode["hold_dates"]) as XsollaSubHoldDates;
		mId = rootNode["id"].AsInt;
		mIdExternal = rootNode["id_external"];
		mName = rootNode["name"];
		mPaymentMethod = rootNode["payment_method"];
		mPaymentType = rootNode["payment_type"];
		mPaymentVisibleName = rootNode["payment_visible_name"];
		mPeriod = new XsollaSubPeriod().Parse(rootNode["period"]) as XsollaSubPeriod;
		mValue = rootNode["value"].AsInt;
		mStatus = rootNode["status"];
		return this;
	}

	public string GetKey()
	{
		return mId.ToString();
	}

	public int GetId()
	{
		return mId;
	}

	public string GetName()
	{
		return mName;
	}

	public override string ToString()
	{
		return $"[XsollaManagerSubscription: mCharge={mCharge}, mDateNextCharge={mDateNextCharge}, mDesc={mDesc}, mHoldDates={mHoldDates}, mId={mId}, mIdExternal={mIdExternal}, mName={mName}, mPaymentMethod={mPaymentMethod}, mPaymentType={mPaymentType}, mPaymentVisibleName={mPaymentVisibleName}, mPeriod={mPeriod}, mValue={mValue}, mStatus={mStatus}]";
	}
}
