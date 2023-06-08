using System;

namespace CI.WSANative.Store;

public class WSAStoreProductResult
{
	public WSAStoreProduct Product { get; set; }

	public Exception Error { get; set; }
}
