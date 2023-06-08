using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class MainHeaderController : MonoBehaviour
{
	public Text _titleProj;

	public GameObject _btnDropDownObj;

	public Text _userName;

	public Button _pMenuBtnComponent;

	public Text _pDropDownMenuBtn;

	private const string PREFAB_VIEW_MENU_ITEM_EMPTY = "Prefabs/SimpleView/ProfileBtn";

	public void InitScreen(XsollaUtils pUtils)
	{
		_titleProj.text = pUtils.GetProject().name;
		_userName.text = string.Empty;
		_pMenuBtnComponent.enabled = false;
		_pDropDownMenuBtn.enabled = false;
	}

	private void ShowPaymentManager()
	{
		GetComponentInParent<XsollaPaystation>().LoadPaymentManager();
	}

	private void ShowHistory()
	{
		Logger.Log("Show user history");
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("offset", 0);
		dictionary.Add("limit", 20);
		dictionary.Add("sortDesc", true);
		dictionary.Add("sortKey", "dateTimestamp");
		GetComponentInParent<XsollaPaystation>().LoadHistory(dictionary);
	}

	private void ShowSubscriptionManager()
	{
		GetComponentInParent<XsollaPaystation>().LoadSubscriptionsManager();
	}

	public void setStateUserMenu(bool pState)
	{
		Logger.Log("Set user menu to state " + pState);
		_pMenuBtnComponent.gameObject.SetActive(pState);
	}
}
