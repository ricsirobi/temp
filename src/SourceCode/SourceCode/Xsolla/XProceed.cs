using SimpleJSON;

namespace Xsolla;

public class XProceed : IParseble
{
	public bool IsInvoiceCreated { get; private set; }

	public long OperationId { get; private set; }

	public string Error { get; private set; }

	public IParseble Parse(JSONNode rootNode)
	{
		IsInvoiceCreated = rootNode["invoice_created"].AsBool;
		OperationId = rootNode["operation_id"].AsInt;
		Error = rootNode["errors"].AsArray[0]["message"];
		return this;
	}

	public override string ToString()
	{
		return $"[XProceed: IsInvoiceCreated={IsInvoiceCreated}, OperationId={OperationId}, Error={Error}]";
	}
}
