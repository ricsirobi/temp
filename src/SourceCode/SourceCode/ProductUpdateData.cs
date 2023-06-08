using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ProductUpgradeURL", Namespace = "")]
public class ProductUpdateData
{
	public string IOSUpdateURL;

	public string AmazonUpdateURL;

	public string GoogleUpdateURL;

	public string WSAUpdateURL;

	public string HuaweiUpdateURL;
}
