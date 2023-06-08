using System;

namespace CI.WSANative.IAPStore;

public class WSAProductLicense
{
	public bool IsActive { get; set; }

	public bool IsTrial { get; set; }

	public bool IsConsumable { get; set; }

	public DateTimeOffset ExpirationDate { get; set; }
}
