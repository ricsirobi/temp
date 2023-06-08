using SimpleJSON;

namespace Xsolla;

public class XsollaPaystation2 : IParseble
{
	public string iconUrl { get; private set; }

	public string formIcopnUrl { get; private set; }

	public string pricepointsAtFirst { get; private set; }

	public string subscriptionAtFirst { get; private set; }

	public string bonusTimerShow { get; private set; }

	public string goodsAtFirst { get; private set; }

	public string countryRemove { get; private set; }

	public string statusRowExclude { get; private set; }

	public IParseble Parse(JSONNode paystation2Node)
	{
		iconUrl = paystation2Node["icon_url"];
		formIcopnUrl = paystation2Node["form_icon_url"];
		pricepointsAtFirst = paystation2Node["pricepoints_at_first"];
		subscriptionAtFirst = paystation2Node["subscriptions_at_first"];
		bonusTimerShow = paystation2Node["bonus_timer_show"];
		goodsAtFirst = paystation2Node["goods_at_first"];
		countryRemove = paystation2Node["country_remove"];
		statusRowExclude = paystation2Node["status_rows_exclude"];
		return this;
	}
}
