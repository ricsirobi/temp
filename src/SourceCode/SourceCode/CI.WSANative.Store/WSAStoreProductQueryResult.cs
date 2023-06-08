using System;
using System.Collections.Generic;

namespace CI.WSANative.Store;

public class WSAStoreProductQueryResult
{
	public Dictionary<string, WSAStoreProduct> Products { get; set; }

	public Exception Error { get; set; }
}
