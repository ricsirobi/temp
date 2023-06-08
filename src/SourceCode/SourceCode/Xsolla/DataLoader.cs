using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

public class DataLoader : MonoBehaviour
{
	public Action<XsollaUtils> RecieveUtils;

	public Action<XsollaForm> RecieveForm;

	public Action<XsollaPricepointsManager> RecievePricePoints;

	public Action<XsollaSubscriptions> RecieveSubscriptions;

	public Action<XsollaGoodsManager> RecieveGoods;

	public Action<XsollaGoodsManager> RecieveGoodsGrous;

	public Action<XsollaQuickPayments> RecieveQuickPayments;

	public Action<XsollaPaymentMethods> RecievePaymentsList;

	public Action<XsollaCountries> RecieveCountries;

	private void Start()
	{
	}

	public void GetUtils(Dictionary<string, object> requestParams)
	{
		BaseWWWRequest utilsRequest = RequestFactory.GetUtilsRequest(requestParams);
		utilsRequest.ObjectsRecived = (Action<int, object[]>)Delegate.Combine(utilsRequest.ObjectsRecived, new Action<int, object[]>(OnDataLoaded));
		StartCoroutine(utilsRequest.Execute());
	}

	public void OnDataLoaded(int type, object[] data)
	{
	}
}
