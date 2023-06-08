using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace KnowledgeAdventure.ContentServer.Model.MobileStore.Huawei;

[Serializable]
[XmlRoot(ElementName = "PurchaseOrderRequest", Namespace = "", IsNullable = true)]
[DataContract(Name = "PurchaseOrderRequest", Namespace = "")]
public class PurchaseOrderRequest
{
	[DataMember(Name = "userHostAddress", IsRequired = false, EmitDefaultValue = false)]
	public string userHostAddress;

	[DataMember(Name = "environment")]
	public string environment { get; set; }

	[DataMember(Name = "itemid")]
	public string itemid { get; set; }

	[DataMember(Name = "userid")]
	public Guid userid { get; set; }

	[DataMember(Name = "huaweirequest")]
	public ProductPayRequest huaweirequest { get; set; }
}
