using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class RightTowerController : MonoBehaviour
{
	private XsollaSummary _summary;

	public GameObject orderSummaryPrefab;

	public GameObject summaryItemPrefab;

	public GameObject financeItemPrefab;

	public GameObject subTotalPrefab;

	public GameObject totalPrefab;

	public ImageLoader imageLoader;

	public LinearLayout linearLayout;

	public void InitView(XsollaTranslations translations, XsollaSummary summary)
	{
		_summary = summary;
		if (base.gameObject.GetComponent<VerticalLayoutGroup>() == null)
		{
			base.gameObject.AddComponent<VerticalLayoutGroup>();
			GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
		}
		GameObject gameObject = Object.Instantiate(orderSummaryPrefab);
		gameObject.GetComponentsInChildren<Text>()[0].text = translations.Get(XsollaTranslations.PAYMENT_SUMMARY_HEADER);
		linearLayout.AddObject(gameObject);
		foreach (IXsollaSummaryItem purchase in _summary.GetPurchases())
		{
			linearLayout.AddObject(GetSummaryItem(purchase));
		}
		XsollaFinance finance = _summary.GetFinance();
		linearLayout.AddObject(GetItem(subTotalPrefab, translations.Get(XsollaTranslations.PAYMENT_SUMMARY_SUBTOTAL), PriceFormatter.Format(finance.subTotal.amount, finance.subTotal.currency)));
		if (finance.discount != null && finance.discount.amount > 0f)
		{
			linearLayout.AddObject(GetItem(financeItemPrefab, translations.Get(XsollaTranslations.PAYMENT_SUMMARY_DISCOUNT), "- " + PriceFormatter.Format(finance.discount.amount, finance.discount.currency)));
		}
		if (finance.fee != null)
		{
			linearLayout.AddObject(GetItem(financeItemPrefab, translations.Get(XsollaTranslations.PAYMENT_SUMMARY_FEE), PriceFormatter.Format(finance.fee.amount, finance.fee.currency)));
		}
		if (finance.xsollaCredits != null && finance.xsollaCredits.amount > 0f)
		{
			linearLayout.AddObject(GetItem(financeItemPrefab, translations.Get(XsollaTranslations.PAYMENT_SUMMARY_XSOLLA_CREDITS), PriceFormatter.Format(finance.xsollaCredits.amount, finance.xsollaCredits.currency)));
		}
		linearLayout.AddObject(GetItem(totalPrefab, translations.Get(XsollaTranslations.PAYMENT_SUMMARY_TOTAL), PriceFormatter.Format(finance.total.amount, finance.total.currency)));
		if (finance.vat != null && finance.vat.amount > 0f)
		{
			linearLayout.AddObject(GetItem(financeItemPrefab, "VAT", PriceFormatter.Format(finance.vat.amount, finance.vat.currency)));
		}
		linearLayout.Invalidate();
	}

	public void UpdateDiscont(XsollaTranslations pTranslation, XsollaSummary pSummary)
	{
		linearLayout.objects.ForEach(delegate(GameObject obj)
		{
			Object.Destroy(obj);
		});
		InitView(pTranslation, pSummary);
	}

	private GameObject GetSummaryItem(IXsollaSummaryItem purchase)
	{
		GameObject obj = Object.Instantiate(summaryItemPrefab);
		Image[] componentsInChildren = obj.GetComponentsInChildren<Image>();
		string imgUrl = purchase.GetImgUrl();
		if (!"".Equals(imgUrl))
		{
			imageLoader.LoadImage(componentsInChildren[1], purchase.GetImgUrl());
		}
		else
		{
			componentsInChildren[1].gameObject.transform.parent.gameObject.SetActive(value: false);
		}
		Text[] componentsInChildren2 = obj.GetComponentsInChildren<Text>();
		componentsInChildren2[0].text = purchase.GetName();
		componentsInChildren2[1].text = purchase.GetPrice();
		componentsInChildren2[2].text = purchase.GetDescription();
		componentsInChildren2[3].text = purchase.GetBonus();
		return obj;
	}

	private GameObject GetItem(GameObject prefab, string title, object amount)
	{
		GameObject obj = Object.Instantiate(prefab);
		Text[] componentsInChildren = obj.GetComponentsInChildren<Text>();
		componentsInChildren[0].text = title;
		componentsInChildren[1].text = amount.ToString();
		return obj;
	}
}
