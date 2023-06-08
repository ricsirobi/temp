using System;

namespace CI.WSANative.IAPStore;

public class WSAUnfulfilledConsumable
{
	public string OfferId { get; set; }

	public string ProductId { get; set; }

	public Guid TransactionId { get; set; }
}
