using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class HistoryController : MonoBehaviour
{
	public Text mTitle;

	public GameObject mHistoryContainer;

	private const string PREFAB_HISTORY_ROW = "Prefabs/SimpleView/HistoryItem";

	private int mLimit;

	private int mCountMore = 20;

	private bool isRefresh;

	private bool sortDesc = true;

	private string mVirtCurrName;

	public Button mBtnRefresh;

	public GameObject mBtnContinue;

	public bool IsRefresh()
	{
		return isRefresh;
	}

	public void InitScreen(XsollaTranslations pTranslation, XsollaHistoryList pList, string pVirtualCurrName)
	{
		mVirtCurrName = pVirtualCurrName;
		Logger.Log("Init history screen");
		mTitle.text = pTranslation.Get("balance_history_page_title");
		mBtnContinue.GetComponent<Text>().text = pTranslation.Get("balance_back_button") + " >";
		mBtnContinue.GetComponent<Button>().onClick.AddListener(delegate
		{
			Logger.Log("Destroy history");
			Object.Destroy(base.gameObject);
		});
		AddHistoryRow(pTranslation, null, pEven: false, pHeader: true);
		foreach (XsollaHistoryItem items in pList.GetItemsList())
		{
			AddHistoryRow(pTranslation, items, mLimit % 2 != 0);
			mLimit++;
		}
		isRefresh = false;
	}

	public void SortHistory()
	{
		sortDesc = !sortDesc;
		OnRefreshHistory();
	}

	private void ClearList()
	{
		Logger.Log("Clear histroy List");
		mLimit = 0;
		Resizer.DestroyChilds(mHistoryContainer.transform);
		mHistoryContainer.GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f);
		isRefresh = true;
	}

	public void OnRefreshHistory()
	{
		Logger.Log("Click refreshBtn");
		ClearList();
		LoadMore();
	}

	public void AddListRows(XsollaTranslations pTranslation, XsollaHistoryList pList)
	{
		foreach (XsollaHistoryItem items in pList.GetItemsList())
		{
			AddHistoryRow(pTranslation, items, mLimit % 2 != 0);
			mLimit++;
		}
	}

	public void AddHistoryRow(XsollaTranslations pTranslation, XsollaHistoryItem pItem, bool pEven, bool pHeader = false)
	{
		Logger.Log("AddHistoryRow");
		GameObject obj = Object.Instantiate(Resources.Load("Prefabs/SimpleView/HistoryItem")) as GameObject;
		HistoryElemController component = obj.GetComponent<HistoryElemController>();
		if (component != null)
		{
			if (pHeader)
			{
				component.Init(pTranslation, null, mVirtCurrName, pEven, SortHistory, pHeader: true, sortDesc);
			}
			else
			{
				component.Init(pTranslation, pItem, mVirtCurrName, pEven, null);
			}
		}
		obj.transform.SetParent(mHistoryContainer.transform);
	}

	public void OnScrollChange(Vector2 pVector)
	{
		Logger.Log("Scroll vector" + pVector.ToString());
		if (pVector == new Vector2(1f, 0f) || pVector == new Vector2(0f, 0f))
		{
			Logger.Log("End scroll");
			if (!isRefresh)
			{
				LoadMore();
			}
		}
	}

	private void LoadMore()
	{
		Logger.Log("Load more history. CurLimit:" + mLimit);
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("offset", mLimit);
		dictionary.Add("limit", mCountMore);
		dictionary.Add("sortDesc", sortDesc.ToString().ToLower());
		dictionary.Add("sortKey", "dateTimestamp");
		GetComponentInParent<XsollaPaystation>().LoadHistory(dictionary);
	}
}
