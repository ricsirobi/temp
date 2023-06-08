using System.Runtime.Serialization;

namespace KnowledgeAdventure.ContentServer.Model.MobileStore.Huawei;

public class HuaweiorderRequest
{
	[DataMember(Name = "applicationID")]
	public string applicationID { get; set; }

	[DataMember(Name = "merchantId")]
	public string merchantId { get; set; }

	[DataMember(Name = "productNo")]
	public string productNo { get; set; }

	[DataMember(Name = "sdkChannel")]
	public string sdkChannel { get; set; }

	[DataMember(Name = "urlver")]
	public string urlver { get; set; }
}
