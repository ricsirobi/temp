using UnityEngine;

namespace tv.superawesome.sdk.publisher;

public class SAVersion
{
	private static string version = "7.2.19";

	private static string sdk = "unity";

	public static void setVersionInNative()
	{
		Debug.Log("Set Sdk version to " + getSdkVersion());
	}

	private static string getVersion()
	{
		return version;
	}

	private static string getSdk()
	{
		return sdk;
	}

	public static string getSdkVersion()
	{
		return getSdk() + "_" + getVersion();
	}
}
