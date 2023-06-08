using System.Collections.Generic;

namespace Xsolla;

public static class RequestFactory
{
	public const int UTILS = 0;

	public const int DIRECTPAYMENT_FORM = 1;

	public const int DIRECTPAYMENT_STATUS = 2;

	public const int PRICEPOINTS = 3;

	public const int SUBSCRIPTIONS = 4;

	public const int GOODS_GROUPS = 51;

	public const int GOODS_ITEMS = 52;

	public const int PAYMENT_LIST = 6;

	public const int QUICK_PAYMENT_LIST = 7;

	public const int COUNTRIES = 8;

	public static BaseWWWRequest GetUtilsRequest(Dictionary<string, object> requestParams)
	{
		return new RequestUtils(0).Prepare(isSandbox: false, requestParams);
	}
}
