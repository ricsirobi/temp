using System;
using UnityEngine;

namespace Microsoft.Applications.Events;

internal class Utils
{
	private static readonly long TICKS_AT_1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

	public static long MsFrom1970()
	{
		return (DateTime.UtcNow.Ticks - TICKS_AT_1970) / 10000;
	}

	public static long TimestampNowTicks()
	{
		return DateTime.UtcNow.Ticks;
	}

	public static string TenantID(string tenant)
	{
		try
		{
			int num = tenant.IndexOf("-", StringComparison.Ordinal);
			if (num > 0)
			{
				return tenant.Substring(0, num);
			}
		}
		catch
		{
			Debug.LogError("Failed to convert tenantId");
		}
		return string.Empty;
	}
}
