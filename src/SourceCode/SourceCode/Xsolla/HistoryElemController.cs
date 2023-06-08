using System;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class HistoryElemController : MonoBehaviour
{
	public Text mDate;

	public Text mType;

	public Text mItem;

	public Text mBalance;

	public Text mPrice;

	public GameObject mSymbolRub;

	public GameObject mDevider;

	public string prepareTypeStr(string pStr)
	{
		string text = pStr;
		int num = 0;
		if (text.Contains("{{paymentName}}"))
		{
			text = text.Replace("{{paymentName}}", "{" + num + "}");
			num++;
		}
		if (text.Contains("{{transactionId}}"))
		{
			text = text.Replace("{{transactionId}}", "{" + num + "}");
			num++;
		}
		if (text.Contains("{{comment}}"))
		{
			text = text.Replace("{{comment}}", "{" + num + "}");
			num++;
		}
		if (text.Contains("{{code}}"))
		{
			text = text.Replace("{{code}}", "{" + num + "}");
			num++;
		}
		return text;
	}

	public void Init(XsollaTranslations pTranslation, XsollaHistoryItem pItem, string pVirtCurrName, bool pEven, Action pSortAction, bool pHeader = false, bool pDesc = true)
	{
		GetComponent<Image>().enabled = pEven;
		if (pHeader)
		{
			mDate.text = pTranslation.Get("balance_history_date") + (pDesc ? " ▼" : " ▲");
			mDate.gameObject.AddComponent<Button>().onClick.AddListener(delegate
			{
				Logger.Log("On sort btn click");
				pSortAction();
				mDate.text = pTranslation.Get("balance_history_date") + " ↓";
			});
			mType.text = pTranslation.Get("balance_history_purpose");
			mItem.text = pTranslation.Get("balance_history_item");
			mBalance.text = pTranslation.Get("balance_history_vc_amount");
			mPrice.text = pTranslation.Get("balance_history_payment_amount");
			mPrice.alignment = TextAnchor.LowerLeft;
			mDevider.SetActive(value: true);
			base.transform.GetComponent<LayoutElement>().minHeight = 30f;
			return;
		}
		mDate.text = pItem.date.ToString("MMM d, yyyy hh:mm tt");
		switch (pItem.operationType)
		{
		case "payment":
			mType.text = string.Format(prepareTypeStr(pTranslation.Get("balance_history_payment_info")), pItem.paymentName, pItem.invoiceId);
			break;
		case "cancellation":
			mType.text = string.Format(prepareTypeStr(pTranslation.Get("balance_history_payment_info_cancellation")), pItem.paymentName, pItem.invoiceId);
			break;
		case "inGamePurchase":
			mType.text = string.Format(prepareTypeStr(pTranslation.Get("balance_history_ingame_info")));
			break;
		case "internal":
			mType.text = string.Format(prepareTypeStr(pTranslation.Get("balance_history_internal_info")), pItem.comment);
			break;
		case "coupon":
			mType.text = string.Format(prepareTypeStr(pTranslation.Get("balance_history_coupon_info")), pItem.couponeCode);
			break;
		case "subscriptionRenew":
			mType.text = string.Format(prepareTypeStr(pTranslation.Get("balance_history_subscription_renew")), pItem.paymentName, pItem.invoiceId);
			break;
		case "subscriptionCreate":
			mType.text = string.Format(prepareTypeStr(pTranslation.Get("balance_history_subscription_create")), pItem.paymentName, pItem.invoiceId);
			break;
		case "subscriptionChange":
			mType.text = string.Format(prepareTypeStr(pTranslation.Get("balance_history_subscription_change")), pItem.paymentName, pItem.invoiceId);
			break;
		case "subscriptionCancellation":
			mType.text = string.Format(prepareTypeStr(pTranslation.Get("subscription_cancellation")), pItem.paymentName, pItem.invoiceId);
			break;
		default:
			mType.text = "";
			break;
		}
		if (pItem.virtualItems.items.GetCount() != 0)
		{
			mItem.text = pItem.virtualItems.items.GetItemByPosition(0).GetName();
		}
		if (pItem.vcAmount != 0f)
		{
			mBalance.text = ((pItem.vcAmount > 0f) ? "+" : "") + pItem.vcAmount + " " + pVirtCurrName + "\n(=" + pItem.userBalance + " " + pVirtCurrName + ")";
		}
		else
		{
			mBalance.text = "";
		}
		if (pItem.paymentAmount != 0f)
		{
			mPrice.text = CurrencyFormatter.FormatPrice(pItem.paymentCurrency, pItem.paymentAmount.ToString("0.00"));
			if (pItem.paymentCurrency == "RUB")
			{
				mSymbolRub.SetActive(value: true);
				return;
			}
			mSymbolRub.SetActive(value: false);
			mPrice.alignment = TextAnchor.LowerLeft;
		}
		else
		{
			mPrice.text = "";
		}
	}
}
