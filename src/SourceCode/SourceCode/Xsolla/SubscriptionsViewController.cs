using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class SubscriptionsViewController : MonoBehaviour
{
	private XsollaSubscriptions _listSubs;

	public Text _titlePage;

	public GameObject _listSubsView;

	public Text _timeAlert;

	private const string PREFAB_SPEC_SUBS = "Prefabs/SimpleView/_ScreenShop/ShopItemSubscriptionSpecial";

	public void InitScreen(XsollaTranslations pTranslation, XsollaSubscriptions pSubs)
	{
		_titlePage.text = pTranslation.Get(XsollaTranslations.SUBSCRIPTION_PAGE_TITLE);
		_listSubs = pSubs;
		foreach (XsollaSubscription items in _listSubs.GetItemsList())
		{
			AddSubs(items, pTranslation);
		}
	}

	private void AddSubs(XsollaSubscription pSub, XsollaTranslations pTranslation)
	{
		GameObject obj = Object.Instantiate(Resources.Load("Prefabs/SimpleView/_ScreenShop/ShopItemSubscriptionSpecial")) as GameObject;
		obj.transform.SetParent(_listSubsView.transform);
		obj.GetComponent<SubscriptionBtnController>().InitBtn(pSub, pTranslation);
	}
}
