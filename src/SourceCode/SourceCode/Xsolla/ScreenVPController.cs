using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class ScreenVPController : ScreenBaseConroller<XVirtualPaymentSummary>
{
	public Text Title;

	public Text Confirmation;

	public GameObject ErrorContainer;

	public Text Error;

	public Text ItemName;

	public GameObject BackTo;

	public Text Total;

	public Text ProceedButtonText;

	public Text ToggleText;

	public Button ProceedButton;

	public Toggle Toggle;

	public ImageLoader ImageLoader;

	public override void InitScreen(XsollaTranslations translations, XVirtualPaymentSummary summary)
	{
	}

	public void DrawScreen(XsollaUtils utils, XVirtualPaymentSummary summary)
	{
		ResizeToParent();
		Title.text = utils.GetTranslations().Get("cart_page_title");
		Confirmation.text = utils.GetTranslations().Get("cart_confirm_your_purchase");
		ImageLoader.UploadImageToCurrentView(summary.Items[0].GetImage());
		ItemName.text = summary.Items[0].Name;
		BackTo.GetComponent<Text>().text = "< " + utils.GetTranslations().Get("back_to_virtualitem");
		BackTo.GetComponent<Button>().onClick.AddListener(delegate
		{
			onClickBack();
		});
		ToggleText.text = utils.GetTranslations().Get("cart_dont_ask_again");
		Total.text = utils.GetTranslations().Get("total") + " " + summary.Total + " " + utils.GetProject().virtualCurrencyName;
		ProceedButtonText.text = utils.GetTranslations().Get("cart_submit");
		ProceedButton.onClick.AddListener(delegate
		{
			OnClickProceed();
		});
	}

	public void ShowError(string errorText)
	{
		Error.gameObject.transform.parent.gameObject.SetActive(value: true);
		Error.text = errorText;
	}

	private void OnClickProceed()
	{
		Logger.Log("Toggle " + Toggle.isOn);
		int num = 0;
		if (Toggle.isOn)
		{
			num = 1;
		}
		Dictionary<string, object> dictionary = new Dictionary<string, object>(1);
		dictionary.Add("dont_ask_again", num);
		base.gameObject.GetComponentInParent<XsollaPaystationController>().ProceedVirtualPayment(dictionary);
	}

	private void onClickBack()
	{
		if (GetComponentInParent<XsollaPaystationController>() != null)
		{
			GetComponentInParent<XsollaPaystationController>().LoadGoodsGroups();
		}
	}
}
