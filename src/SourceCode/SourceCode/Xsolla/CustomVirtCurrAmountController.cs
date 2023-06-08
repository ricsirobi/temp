using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class CustomVirtCurrAmountController : MonoBehaviour
{
	public class CustomAmountCalcRes : IParseble
	{
		public class Bonus : IParseble
		{
			public IParseble Parse(JSONNode pNode)
			{
				return this;
			}
		}

		public double amount;

		public string currency;

		public double vcAmount;

		public Bonus bonus;

		public IParseble Parse(JSONNode pNode)
		{
			amount = pNode["amount"].AsDouble;
			currency = pNode["currency"];
			vcAmount = pNode["vc_amount"].AsDouble;
			bonus = new Bonus().Parse(pNode["bonus"]) as Bonus;
			return this;
		}
	}

	public InputField virtCurrAmount;

	public InputField realCurrAmount;

	public Button btnPay;

	public Text totalAmountTitle;

	public Image iconVirtCurr;

	public Text iconRealCurr;

	private string mTotalTitle = "";

	private string mCustomCurrency = "";

	private bool mSetValues;

	public void initScreen(XsollaUtils pUtils, string pCustomCurrency, Action<Dictionary<string, object>> pActionCalc, Action<float> pTryPay)
	{
		if (pUtils.GetProject().isDiscrete)
		{
			virtCurrAmount.contentType = InputField.ContentType.IntegerNumber;
		}
		else
		{
			virtCurrAmount.contentType = InputField.ContentType.DecimalNumber;
		}
		btnPay.gameObject.GetComponentInChildren<Text>().text = pUtils.GetTranslations().Get("form_continue");
		mTotalTitle = pUtils.GetTranslations().Get("payment_summary_total");
		mCustomCurrency = pCustomCurrency;
		ImageLoader imageLoader = UnityEngine.Object.FindObjectOfType<ImageLoader>();
		Logger.Log("VirtIcon " + pUtils.GetProject().virtualCurrencyIconUrl);
		imageLoader.LoadImage(iconVirtCurr, "http:" + pUtils.GetProject().virtualCurrencyIconUrl);
		virtCurrAmount.onEndEdit.AddListener(delegate
		{
			if (!mSetValues)
			{
				pActionCalc(GetParamsForCalc(pVirtCurr: true));
			}
		});
		realCurrAmount.onEndEdit.AddListener(delegate
		{
			if (!mSetValues)
			{
				pActionCalc(GetParamsForCalc(pVirtCurr: false));
			}
		});
		btnPay.onClick.AddListener(delegate
		{
			pTryPay(GetOutAmount());
		});
	}

	private float GetOutAmount()
	{
		float result = 0f;
		float.TryParse(virtCurrAmount.text, out result);
		return result;
	}

	private Dictionary<string, object> GetParamsForCalc(bool pVirtCurr)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("userInitialCurrency", "");
		dictionary.Add("custom_currency", mCustomCurrency);
		if (pVirtCurr)
		{
			dictionary.Add("custom_vc_amount", virtCurrAmount.text);
		}
		else
		{
			dictionary.Add("custom_amount", realCurrAmount.text);
		}
		return dictionary;
	}

	public void setValues(CustomAmountCalcRes pValue)
	{
		mSetValues = true;
		if (pValue.amount != 0.0)
		{
			totalAmountTitle.text = mTotalTitle + " " + CurrencyFormatter.FormatPrice(pValue.currency, pValue.amount.ToString("N2"));
		}
		else
		{
			totalAmountTitle.text = "";
		}
		if (pValue.vcAmount != 0.0)
		{
			virtCurrAmount.text = pValue.vcAmount.ToString();
		}
		else
		{
			virtCurrAmount.text = "";
		}
		if (pValue.amount != 0.0)
		{
			realCurrAmount.text = pValue.amount.ToString("0.00");
		}
		else
		{
			realCurrAmount.text = "";
		}
		if (pValue.currency == "USD")
		{
			iconRealCurr.text = "$";
		}
		else if (pValue.currency == "EUR")
		{
			iconRealCurr.text = "€";
		}
		else if (pValue.currency == "RUB")
		{
			iconRealCurr.text = "\ue019";
		}
		if (pValue.vcAmount > 0.0)
		{
			btnPay.interactable = true;
		}
		else
		{
			btnPay.interactable = false;
		}
		mSetValues = false;
	}
}
