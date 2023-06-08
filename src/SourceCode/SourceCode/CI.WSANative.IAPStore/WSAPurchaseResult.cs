using System;

namespace CI.WSANative.IAPStore;

public class WSAPurchaseResult
{
	public WSAPurchaseResultStatus Status { get; set; }

	public Guid TransactionId { get; set; }

	public string ReceiptXml { get; set; }

	public string OfferId { get; set; }
}
