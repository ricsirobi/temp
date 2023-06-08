using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

[Obsolete("class is ScreenStatusController, please use StatusViewController instead.")]
public class ScreenStatusController : ScreenBaseConroller<XsollaStatus>
{
	public delegate void OnStatusRecived(XsollaStatusData statusData);

	public LinearLayout linerLayout;

	public StatusElementAdapter adapter;

	public event Action<XsollaStatusData> StatusHandler;

	public void Awake()
	{
		if (linerLayout == null)
		{
			linerLayout = GetComponent<LinearLayout>();
		}
	}

	public override void InitScreen(XsollaTranslations translations, XsollaStatus status)
	{
		DrawScreen(translations, status);
	}

	public void DrawScreen(XsollaTranslations translations, XsollaStatus xsollaStatus)
	{
		ResizeToParent();
		XsollaStatus.Group group = xsollaStatus.GetGroup();
		switch (group)
		{
		case XsollaStatus.Group.DONE:
			linerLayout.AddObject(GetOkStatus(xsollaStatus.GetStatusText().GetState()));
			break;
		case XsollaStatus.Group.TROUBLED:
			linerLayout.AddObject(GetErrorByString(xsollaStatus.GetStatusText().GetState()));
			break;
		default:
			linerLayout.AddObject(GetWaitingStatus(xsollaStatus.GetStatusText().GetState()));
			StartCoroutine(TryIt(xsollaStatus.GetInvoice()));
			break;
		}
		linerLayout.AddObject(GetError(null));
		linerLayout.AddObject(GetEmpty());
		adapter.SetElements(xsollaStatus.GetStatusText().GetStatusTextElements());
		GameObject list = GetList(adapter);
		list.GetComponent<ListView>().DrawList(GetComponent<RectTransform>());
		linerLayout.AddObject(list);
		linerLayout.AddObject(GetEmpty());
		if (group == XsollaStatus.Group.INVOICE || group == XsollaStatus.Group.UNKNOWN)
		{
			linerLayout.AddObject(GetButton("Start again", delegate
			{
				StartAgain();
			}));
			linerLayout.AddObject(GetEmpty());
		}
		linerLayout.Invalidate();
	}

	private IEnumerator TryIt(string invoice)
	{
		yield return new WaitForSeconds(5f);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("section", "getstatus");
		dictionary.Add("action", "getstatus");
		dictionary.Add("invoice", invoice);
		base.gameObject.GetComponentInParent<XsollaPaystationController>().GetStatus(dictionary);
	}

	private void StartAgain()
	{
		base.gameObject.GetComponentInParent<XsollaPaystationController>().DoPayment(new Dictionary<string, object>());
	}

	private void OnClickButton(XsollaStatus status)
	{
		OnStatusRecivied(status.GetStatusData());
		UnityEngine.Object.Destroy(base.gameObject.GetComponentInParent<XsollaPaystationController>().gameObject);
	}

	private void OnStatusRecivied(XsollaStatusData xsollaStatusData)
	{
		if (this.StatusHandler != null)
		{
			this.StatusHandler(xsollaStatusData);
		}
	}

	private void OnDestroy()
	{
		MonoBehaviour.print("Script was destroyed ScreenStatusController");
	}
}
