using System;
using System.Collections.Generic;

namespace CI.WSANative.Store;

public class WSAStoreAppLicense
{
	public Dictionary<string, WSAStoreLicense> AddOnLicenses { get; set; }

	public DateTimeOffset ExpirationDate { get; set; }

	public bool IsActive { get; set; }

	public bool IsTrial { get; set; }

	public string StoreId { get; set; }

	public TimeSpan TrialTimeRemaining { get; set; }

	public string TrialUniqueId { get; set; }
}
