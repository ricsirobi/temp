using System;

namespace CI.WSANative.Store;

public class WSAStorePurchaseResult
{
	public WSAStorePurchaseStatus Status { get; set; }

	public Exception Error { get; set; }
}
