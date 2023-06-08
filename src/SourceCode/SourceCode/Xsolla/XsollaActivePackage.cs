using System;
using SimpleJSON;

namespace Xsolla;

public class XsollaActivePackage : IParseble
{
	public DateTime _dateNextCharge;

	public DateTime _datePlanChange;

	public string _id;

	public bool _isPossibleRenew;

	public IParseble Parse(JSONNode pNode)
	{
		DateTime.TryParse(pNode["date_next_charge"], out _dateNextCharge);
		DateTime.TryParse(pNode["date_plan_change"], out _datePlanChange);
		_id = pNode["id"];
		_isPossibleRenew = pNode["is_possible_renew"].AsBool;
		return this;
	}
}
