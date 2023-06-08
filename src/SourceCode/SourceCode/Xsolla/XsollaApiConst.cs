using System.Runtime.InteropServices;

namespace Xsolla;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct XsollaApiConst
{
	public const string ERROR_CODE = "error_code";

	public const string ERROR_MSG = "error";

	public const string REQUEST_PARAMS = "request_params";

	public const string COMMAND_CALCULATE = "calculate";

	public const string COMMAND_CHECK = "check";

	public const string XPS_FIX_COMMAND = "xps_fix_command";

	public const string XPS_CHANGE_ELEM = "xps_change_element";

	public const string ACCESS_TOKEN = "access_token";

	public const string PID = "pid";

	public const string OUT = "out";

	public const string SKU = "sku";

	public const string ID_PACKAGE = "id_package";

	public const string RETURN_URL = "returnUrl";

	public const string PAYMENT_WITH_SAVED_METHOD = "paymentWithSavedMethod";

	public const string XPS = "xps";

	public const string DESCRIPTION = "description";

	public const string CARD_NUMBER = "card_number";

	public const string CARD_EXP_YEAR = "card_year";

	public const string CARD_EXP_MONTH = "card_month";

	public const string CARD_CVV = "cvv";

	public const string CARD_ZIP = "zip";

	public const string CARD_HOLDER = "cardholdername";

	public const string ALLOW_SUBSCRIPTION = "allowSubscription";

	public const string COUPON_CODE = "couponCode";

	public const string PID_Card = "26";

	public const string PID_PayPal = "24";

	public const string R_TITLE = "title";

	public const string R_TEXTALL = "textAll";

	public const string R_ERRORS = "errors";

	public const string R_MESSAGES = "messages";

	public const string R_ACCOUNT = "account";

	public const string R_ACCOUNTXSOLLA = "accountXsolla";

	public const string R_ONLINEBANKING = "onlineBanking";

	public const string R_FORM = "form";

	public const string RF_DFP = "dfp";

	public const string R_CHECKOUTFORM = "checkoutForm";

	public const string R_JAVASCRIPT = "javascript";

	public const string R_REPORT = "report";

	public const string R_BACKURL = "backURL";

	public const string R_BACKVISIBLE = "backVisible";

	public const string R_FORMURL = "formURL";

	public const string R_SKIPFORM = "skipForm";

	public const string R_SKIPCHECKOUT = "skipCheckout";

	public const string R_FORMPARAMS = "formParams";

	public const string R_FORMISHIDDEN = "formIsHidden";

	public const string R_INSTRUCTION = "instruction";

	public const string R_BUYDATA = "buyData";

	public const string R_CURRENCY = "currency";

	public const string R_VIRTUALCURRENCYNAME = "virtualCurrencyName";

	public const string R_INSTANCESIZE = "instanceSize";

	public const string R_STATUS = "status";

	public const string R_MARKETPLACE = "marketplace";

	public const string R_HASFORMBEENSUBMITTED = "hasFormBeenSubmitted";

	public const string R_CURRENTCOMMAND = "currentCommand";

	public const string R_MINMAX = "minmax";

	public const string R_REQUESTID = "requestID";

	public const string R_SCHEMEREDIRECT = "schemeRedirect";

	public const string R_PID = "pid";

	public const string R_ICONURL = "iconUrl";

	public const string R_ACTION = "action";

	public const string R_INFOFIELDS = "infofields";

	public const string R_PROJECTLOGO = "projectLogo";

	public const string R_FORMCLASS = "formClass";

	public const string R_PROJECT = "project";

	public const string R_THEME = "theme";

	public const string R_REQUEST = "request";

	public const string R_COUNTRY = "country";

	public const string R_APP = "app";

	public const string R_CONTEXT = "context";

	public const string R_ISPACKETS = "isPackets";

	public const string R_ISSHORTFORM = "isShortForm";

	public const string R_ISFASTCHECKOUT = "isFastCheckout";

	public const string R_SPIKES = "spikes";

	public const string R_USER = "user";

	public const string R_PURCHASE = "purchase";

	public const string R_SETTINGS = "settings";

	public const string R_TRANSLATIONS = "translations";

	public const string R_API = "api";
}
