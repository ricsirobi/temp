using System.Runtime.Serialization;
using KnowledgeAdventure.ContentServer.Model.MobileStore.Huawei;

public class GetCreateOrderResponse
{
	[DataMember(Name = "success")]
	public bool success { get; set; }

	[DataMember(Name = "validation")]
	public PurchaseOrderValidation validation { get; set; }

	[DataMember(Name = "payrequest")]
	public ProductPayRequest payrequest { get; set; }
}
