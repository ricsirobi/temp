using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaTranslations : IParseble
{
	public static string API_ERROR_MESSAGE = "api_error_message";

	public static string APP_ERROR_TITLE = "app_error_title";

	public static string BACK_TO_LIST = "back_to_list";

	public static string BACK_TO_PAYMENT = "back_to_payment";

	public static string BACK_TO_PICEPOINT = "back_to_pricepoint";

	public static string BACK_TO_SAVEDMETHOD = "back_to_savedmethod";

	public static string BACK_TO_SPECIALS = "back_to_specials";

	public static string BACK_TO_SPECIALSLIST = "back_to_specialslist";

	public static string BACK_TO_STORE = "back_to_store";

	public static string BACK_TO_SUBSCRIPTION = "back_to_subscription";

	public static string BACK_TO_VIRTUALITEM = "back_to_virtualitem";

	public static string BALANCE_BACK_BUTTON = "balance_back_button";

	public static string BALANCE_HISTORY_AMOUNT = "balance_history_amount";

	public static string BALANCE_HISTORY_COMMENT = "balance_history_comment";

	public static string BALANCE_HISTORY_DATE = "balance_history_date";

	public static string BALANCE_HISTORY_NO_DATA = "balance_history_no_data";

	public static string BALANCE_HISTORY_PAGE_TITLE = "balance_history_page_title";

	public static string BALANCE_HISTORY_PAYMENT_INFO = "balance_history_payment_info";

	public static string BALANCE_HISTORY_REFRESH_BUTTON = "balance_history_refresh_button";

	public static string BALANCE_HISTORY_STATUS = "balance_history_status";

	public static string BALANCE_HISTORY_TRANSACTION_ID = "balance_history_transaction_id";

	public static string COUNTRY_CHANGE = "country_change";

	public static string FOOTER_AGREEMENT = "footer_agreement";

	public static string FOOTER_SECURED_CONNECTION = "footer_secured_connection";

	public static string FORM_CC_CARD_NUMBER = "form_cc_card_number";

	public static string FORM_CC_EULA = "form_cc_eula";

	public static string FORM_CC_EXP_DATE = "form_cc_exp_date";

	public static string FORM_CHECKOUT = "form_checkout";

	public static string FORM_CHECKOUT_INTRO = "form_checkout_intro";

	public static string FORM_CONTINUE = "form_continue";

	public static string FORM_HEADER_CHANGE = "form_header_change";

	public static string FORM_NUMBER_2PAY = "form_number_2pay";

	public static string FORM_NUMBER_XSOLLA = "form_number_xsolla";

	public static string FORM_TOTAL = "form_total";

	public static string LIST_OPTION_EXTRA = "list_option_extra";

	public static string LIST_OPTION_OFF = "list_option_off";

	public static string OFFER_BONUS = "offer_bonus";

	public static string OFFER_DISCOUNT = "offer_discount";

	public static string OFFERS_COUNTDOWN_DAYS = "offers_countdown_days";

	public static string OFFERS_COUNTDOWN_LABEL = "offers_countdown_label";

	public static string OFFERS_COUNTDOWN_METHODS = "offers_countdown_methods";

	public static string OFFERS_DEFAULT_NAME = "offers_default_name";

	public static string PAYMENT_INSTRUCTION_LABEL = "payment_instruction_label";

	public static string PAYMENT_LIST_POPULAR_TITLE = "payment_list_popular_title";

	public static string PAYMENT_LIST_QUICK_TITLE = "payment_list_quick_title";

	public static string PAYMENT_LIST_SEARCH = "payment_list_search";

	public static string PAYMENT_LIST_SEARCH_EG = "payment_list_search_eg";

	public static string PAYMENT_LIST_SEARCH_MOBILE = "payment_list_search_mobile";

	public static string PAYMENT_LIST_SHOW_MORE = "payment_list_show_more";

	public static string PAYMENT_LIST_SHOW_QUICK = "payment_list_show_quick";

	public static string PAYMENT_METHOD_NO_DATA = "payment_method_no_data";

	public static string PAYMENT_METHODS_PAGE_TITLE = "payment_methods_page_title";

	public static string PAYMENT_PAGE_TITLE = "payment_page_title";

	public static string PAYMENT_PAGE_TITLE_VIA = "payment_page_title_via";

	public static string PAYMENT_SUMMARY_BONUS = "payment_summary_bonus";

	public static string PAYMENT_SUMMARY_DISCOUNT = "payment_summary_discount";

	public static string PAYMENT_SUMMARY_FEE = "payment_summary_fee";

	public static string PAYMENT_SUMMARY_HEADER = "payment_summary_header";

	public static string PAYMENT_SUMMARY_SUBTOTAL = "payment_summary_subtotal";

	public static string PAYMENT_SUMMARY_TOTAL = "payment_summary_total";

	public static string PAYMENT_SUMMARY_VAT = "payment_summary_vat";

	public static string PAYMENT_SUMMARY_XSOLLA_CREDITS = "payment_summary_xsolla_credits";

	public static string PAYMENT_SUMMARY_XSOLLA_CREDITS_HINT = "payment_summary_xsolla_credits_hint";

	public static string PAYMENT_WAITING_BUTTON = "payment_waiting_button";

	public static string PAYMENT_WAITING_NOTICE = "payment_waiting_notice";

	public static string PERIOD_DAYS = "period_days";

	public static string PERIOD_MONTH1 = "period_month1";

	public static string PERIOD_MONTHS = "period_months";

	public static string PRICEPOINT_OPTION_BUTTON = "pricepoint_option_button";

	public static string PRICEPOINT_PAGE_TITLE = "pricepoint_page_title";

	public static string PRICEPOINT_PAGE_CUSTOM_AMOUNT_HIDE_TITLE = "custom_amount_hide_button";

	public static string PRICEPOINT_PAGE_CUSTOM_AMOUNT_SHOW_TITLE = "custom_amount_show_button";

	public static string SAVED_METHODS_TITLE = "saved_methods_title";

	public static string SAVEDMETHOD_OTHER_ACCOUNT_LABEL = "savedmethod_other_account_label";

	public static string SAVEDMETHOD_OTHER_LABEL = "savedmethod_other_label";

	public static string SAVEDMETHOD_OTHER_LABEL_MOBILE = "savedmethod_other_label_mobile";

	public static string SAVEDMETHOD_PAGE_TITLE = "savedmethod_page_title";

	public static string STATE_NAME_INDEX = "state_name_index";

	public static string STATE_NAME_LIST = "state_name_list";

	public static string STATE_NAME_PAYMENT = "state_name_payment";

	public static string STATE_NAME_PRICEPOINT = "state_name_pricepoint";

	public static string STATE_NAME_SAVEDMETHOD = "state_name_savedmethod";

	public static string STATE_NAME_SUBSCRIPTION = "state_name_subscription";

	public static string STATE_NAME_VIRTUALITEM = "state_name_virtualitem";

	public static string COUPON_PAGE_TITLE = "coupon_page_title";

	public static string COUPON_DESCRIPTION = "coupon_description";

	public static string COUPON_CODE_TITLE = "coupon_code_title";

	public static string COUPON_CODE_EXAMPLE = "coupon_code_example";

	public static string COUPON_CONTROL_APPLY = "coupon_control_apply";

	public static string STATUS_DONE_DESCRIPTION = "status_done_description";

	public static string STATUS_PAGE_TITLE = "status_page_title";

	public static string STATUS_PURCHASED_DESCRIPTION = "status_purchased_description";

	public static string SUBSCRIPTION_MOBILE_PAGE_TITLE = "subscription_mobile_page_title";

	public static string SUBSCRIPTION_PACKAGE_RATE = "subscription_package_rate";

	public static string SUBSCRIPTION_PACKAGE_RATE_MOBILE = "subscription_package_rate_mobile";

	public static string SUBSCRIPTION_PAGE_TITLE = "subscription_page_title";

	public static string SUPPORT_CONTACT_US = "support_contact_us";

	public static string SUPPORT_CUSTOMER_SUPPORT = "support_customer_support";

	public static string SUPPORT_LABEL = "support_label";

	public static string SUPPORT_NEED_HELP = "support_need_help";

	public static string SUPPORT_PHONE = "support_phone";

	public static string TOTAL = "total";

	public static string USER_BALANCE_LABEL = "user_balance_label";

	public static string VALIDATION_MESSAGE_CARD_MONTH = "validation_message_card_month";

	public static string VALIDATION_MESSAGE_CARD_YEAR = "validation_message_card_year";

	public static string VALIDATION_MESSAGE_CARDNUMBER = "validation_message_cardnumber";

	public static string VALIDATION_MESSAGE_CVV = "validation_message_cvv";

	public static string VALIDATION_MESSAGE_EMAIL = "validation_message_email";

	public static string VALIDATION_MESSAGE_MAX = "validation_message_max";

	public static string VALIDATION_MESSAGE_MAX_LENGTH = "validation_message_max_length";

	public static string VALIDATION_MESSAGE_MIN = "validation_message_min";

	public static string VALIDATION_MESSAGE_MIN_LENGTH = "validation_message_min_length";

	public static string VALIDATION_MESSAGE_PHONE = "validation_message_phone";

	public static string VALIDATION_MESSAGE_REQUIRED = "validation_message_required";

	public static string VIRTUAL_ITEM_OPTION_BUTTON = "virtual_item_option_button";

	public static string VIRTUALSTATUS_DONE_DESCRIPTIONS = "virtualstatus_done_description";

	public static string VIRTUALITEM_NO_DATA = "virtualitem_no_data";

	public static string VIRTUALITEM_PAGE_TITLE = "virtualitem_page_title";

	public static string VIRTUALITEMS_TITLE_FAVORITE = "virtualitems_title_favorite";

	public static string WHERE_IS_CPF_NAME = "where_is_cpf_name";

	public static string WHERE_IS_CPF_NUMBER = "where_is_cpf_number";

	public static string WHERE_IS_SECURITY_CODE = "where_is_security_code";

	public static string WHERE_IS_ZIP_POSTAL_CODE = "where_is_zip_postal_code";

	public static string XSOLLA_COPYRIGHT = "xsolla_copyright";

	private Dictionary<string, string> translations;

	public XsollaTranslations()
	{
		translations = new Dictionary<string, string>();
	}

	public string Get(string key)
	{
		if (translations.ContainsKey(key))
		{
			return translations[key];
		}
		return "";
	}

	public IParseble Parse(JSONNode translationsNode)
	{
		fillMap(translationsNode);
		return this;
	}

	private void fillMap(JSONNode translationsNode)
	{
		IEnumerator enumerator = translationsNode.AsObject.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, JSONNode> keyValuePair = (KeyValuePair<string, JSONNode>)enumerator.Current;
			translations.Add(keyValuePair.Key, keyValuePair.Value);
		}
	}

	public override string ToString()
	{
		return $"[XsollaTranslations]";
	}
}
