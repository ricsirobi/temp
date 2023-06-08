using System;

namespace KnowledgeAdventure.ContentServer.Model.MobileStore.Huawei;

[Serializable]
public class ProductPayRequest
{
	public string merchantId;

	public string merchantName;

	public string applicationID;

	public string requestId;

	public string urlVer;

	public int sdkChannel;

	public string extReserved;

	public string serviceCatalog;

	public string url;

	public string sign;

	public string productNo;

	public int validTime;

	public string expireTime;
}
