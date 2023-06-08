using System.Collections.Generic;

namespace Xsolla;

public class XsollaResult
{
	public Dictionary<string, object> purchases;

	public string invoice { get; set; }

	public XsollaStatusData.Status status { get; set; }

	public XsollaResult()
	{
	}

	public XsollaResult(Dictionary<string, object> purchases)
	{
		this.purchases = purchases;
	}
}
