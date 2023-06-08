using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class StatusViewController : ScreenBaseConroller<XsollaStatus>
{
	public GameObject status;

	public Transform checkListTransform;

	public Button statusViewExitButton;

	public GameObject rowTitlePrefab;

	public GameObject rowStatusPrefab;

	public GameObject rowLinePrefab;

	public GameObject rowInfoElementPrefab;

	public GameObject rowInfoElementBigPrefab;

	public event Action<XsollaStatus.Group, string, XsollaStatusData.Status, Dictionary<string, object>> StatusHandler;

	public override void InitScreen(XsollaTranslations translations, XsollaStatus xsollaStatus)
	{
		ResizeToParent();
		UnityEngine.Object.FindObjectOfType<MainHeaderController>().setStateUserMenu(pState: false);
		string text = translations.Get(XsollaTranslations.STATUS_PURCHASED_DESCRIPTION);
		XsollaStatusText statusText = xsollaStatus.GetStatusText();
		XsollaStatus.Group currentStatus = xsollaStatus.GetGroup();
		if (text != null)
		{
			Regex regex = new Regex("{{.*?}}");
			if (statusText.GetPurchsaeValue() != "" && statusText.Get("sum").GetValue() != "")
			{
				text = regex.Replace(text, statusText.GetPurchsaeValue(), 1);
				text = regex.Replace(text, statusText.Get("sum").GetValue(), 1);
			}
			else
			{
				text = "";
			}
		}
		else
		{
			text = "";
		}
		PrepareStatus(currentStatus, xsollaStatus.GetStatusText().GetState(), text, xsollaStatus.GetInvoice(), xsollaStatus);
		AddTitle(statusText.GetProjectString());
		if (currentStatus == XsollaStatus.Group.DONE)
		{
			AddStatus(translations.Get(XsollaTranslations.VIRTUALSTATUS_DONE_DESCRIPTIONS));
		}
		if (statusText.GetPurchsaeValue() != "")
		{
			AddElement(statusText.GetPurchsaeValue(), statusText.Get("sum").GetValue());
		}
		if (statusText.Get("out") != null)
		{
			AddElement(statusText.Get("out").GetPref(), statusText.Get("out").GetValue());
		}
		if (statusText.Get("invoice") != null)
		{
			AddElement(statusText.Get("invoice").GetPref(), statusText.Get("invoice").GetValue());
		}
		if (statusText.Get("details") != null)
		{
			AddElement(statusText.Get("details").GetPref(), statusText.Get("details").GetValue());
		}
		if (statusText.Get("time") != null)
		{
			AddElement(statusText.Get("time").GetPref(), statusText.Get("time").GetValue());
		}
		if (statusText.Get("merchant") != null)
		{
			AddElement(statusText.Get("merchant").GetPref(), statusText.Get("merchant").GetValue());
		}
		if (statusText.Get("userWallet") != null)
		{
			AddElement(statusText.Get("userWallet").GetPref(), statusText.Get("userWallet").GetValue());
		}
		AddLine();
		AddBigElement(statusText.Get("sum").GetPref(), statusText.Get("sum").GetValue());
		statusViewExitButton.gameObject.GetComponent<Text>().text = translations.Get(XsollaTranslations.BACK_TO_STORE);
		statusViewExitButton.onClick.AddListener(delegate
		{
			OnClickExit(currentStatus, xsollaStatus.GetStatusData().GetInvoice(), xsollaStatus.GetStatusData().GetStatus(), null);
		});
	}

	public void DrawVpStatus(XsollaUtils utils, XVPStatus status)
	{
		XsollaTranslations translations = utils.GetTranslations();
		string purchase = ((status.Status.HeaderDescription != null) ? status.Status.HeaderDescription : translations.Get(XsollaTranslations.STATUS_PURCHASED_DESCRIPTION));
		XsollaStatus.Group currentStatus = status.GetGroup();
		PrepareStatus(currentStatus, status.Status.Header, purchase, "");
		AddTitle(utils.GetProject().name);
		AddStatus(status.Status.Description);
		AddElement(translations.Get("virtualstatus_check_operation"), status.OperationId);
		AddElement(translations.Get("virtualstatus_check_time"), $"{DateTime.Parse(status.OperationCreated):dd/MM/yyyy HH:mm}");
		if (status.Items.Count > 0)
		{
			AddElement(translations.Get("virtualstatus_check_virtual_items"), status.GetPurchase(0));
		}
		if (status.vCurr.Count > 0)
		{
			AddElement(translations.Get("virtualstatus_check_virtual_currency"), status.GetVCPurchase(0) + " " + utils.GetProject().virtualCurrencyName);
		}
		if (status.OperationType != "coupon")
		{
			AddLine();
			AddBigElement(translations.Get("virtualstatus_check_vc_amount"), status.VcAmount + " " + utils.GetProject().virtualCurrencyName);
		}
		statusViewExitButton.gameObject.GetComponent<Text>().text = translations.Get(XsollaTranslations.BACK_TO_STORE);
		statusViewExitButton.onClick.AddListener(delegate
		{
			OnClickBack(currentStatus, status.OperationId, XsollaStatusData.Status.DONE, status.GetPurchaseList());
		});
	}

	public void PrepareStatus(XsollaStatus.Group group, string state, string purchase, string invoice, XsollaStatus pStatus = null)
	{
		Component[] componentsInChildren = status.GetComponentsInChildren(typeof(Text), includeInactive: true);
		ColorController component = GetComponent<ColorController>();
		((Text)componentsInChildren[1]).text = state;
		if (purchase != null && purchase != "")
		{
			((Text)componentsInChildren[2]).text = purchase;
			componentsInChildren[2].gameObject.SetActive(value: true);
		}
		else
		{
			componentsInChildren[2].gameObject.SetActive(value: false);
		}
		switch (group)
		{
		case XsollaStatus.Group.DONE:
			((Text)componentsInChildren[0]).text = "\ue017";
			component.ChangeColor(1, StyleManager.BaseColor.bg_ok);
			break;
		case XsollaStatus.Group.TROUBLED:
			((Text)componentsInChildren[0]).text = "\ue00c";
			component.ChangeColor(1, StyleManager.BaseColor.bg_error);
			break;
		default:
			((Text)componentsInChildren[0]).text = "\ue01c";
			((Text)componentsInChildren[0]).gameObject.AddComponent<MyRotation>();
			component.ChangeColor(1, StyleManager.BaseColor.selected);
			StartCoroutine(UpdateStatus(invoice));
			break;
		}
	}

	private void AddTitle(string s)
	{
		GameObject obj = UnityEngine.Object.Instantiate(rowTitlePrefab);
		obj.transform.SetParent(checkListTransform);
		obj.GetComponentInChildren<Text>().text = s;
	}

	private void AddStatus(string s)
	{
		GameObject obj = UnityEngine.Object.Instantiate(rowStatusPrefab);
		obj.transform.SetParent(checkListTransform);
		obj.GetComponentInChildren<Text>().text = s;
	}

	private void AddLine()
	{
		UnityEngine.Object.Instantiate(rowLinePrefab).transform.SetParent(checkListTransform);
	}

	private void AddElement(string s, string s1)
	{
		if (!(s == "") || !(s1 == ""))
		{
			GameObject obj = UnityEngine.Object.Instantiate(rowInfoElementPrefab);
			obj.transform.SetParent(checkListTransform);
			Text[] componentsInChildren = obj.GetComponentsInChildren<Text>();
			componentsInChildren[0].text = s;
			componentsInChildren[1].text = s1;
		}
	}

	private void AddBigElement(string s, string s1)
	{
		if (!(s == "") || !(s1 == ""))
		{
			GameObject obj = UnityEngine.Object.Instantiate(rowInfoElementBigPrefab);
			obj.transform.SetParent(checkListTransform);
			Text[] componentsInChildren = obj.GetComponentsInChildren<Text>();
			componentsInChildren[0].text = s;
			componentsInChildren[1].text = s1;
		}
	}

	private IEnumerator UpdateStatus(string invoice)
	{
		yield return new WaitForSeconds(5f);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("section", "getstatus");
		dictionary.Add("action", "getstatus");
		dictionary.Add("invoice", invoice);
		base.gameObject.GetComponentInParent<XsollaPaystationController>().GetStatus(dictionary);
	}

	private void OnClickExit(XsollaStatus.Group group, string invoice, XsollaStatusData.Status status, Dictionary<string, object> pPurchase)
	{
		if (this.StatusHandler != null)
		{
			this.StatusHandler(group, invoice, status, pPurchase);
		}
		if (GetComponentInParent<XsollaPaystationController>() != null)
		{
			GetComponentInParent<XsollaPaystationController>().gameObject.GetComponentInChildren<Selfdestruction>().DestroyRoot();
		}
	}

	private void OnClickBack(XsollaStatus.Group group, string invoice, XsollaStatusData.Status status, Dictionary<string, object> pPurchase)
	{
		if (this.StatusHandler != null)
		{
			this.StatusHandler(group, invoice, status, pPurchase);
		}
		if (GetComponentInParent<XsollaPaystationController>() != null)
		{
			GetComponentInParent<XsollaPaystationController>().LoadGoodsGroups();
		}
	}

	private void OnDestroy()
	{
		MainHeaderController mainHeaderController = UnityEngine.Object.FindObjectOfType<MainHeaderController>();
		if (mainHeaderController != null)
		{
			mainHeaderController.setStateUserMenu(pState: true);
		}
	}
}
