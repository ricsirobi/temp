using System;
using System.Collections.Generic;
using UniRateMiniJSON;

public class UniRateAppInfo
{
	public bool validAppInfo;

	public string bundleId;

	public int appStoreGenreID;

	public int appID;

	public string version;

	private const string kAppInfoResultsKey = "results";

	private const string kAppInfoBundleIdKey = "bundleId";

	private const string kAppInfoGenreIdKey = "primaryGenreId";

	private const string kAppInfoAppIdKey = "trackId";

	private const string kAppInfoVersion = "version";

	public UniRateAppInfo(string jsonResponse)
	{
		if (Json.Deserialize(jsonResponse) is Dictionary<string, object> dictionary && dictionary["results"] is List<object> list && list.Count > 0 && list[0] is Dictionary<string, object> dictionary2)
		{
			bundleId = dictionary2["bundleId"] as string;
			appStoreGenreID = Convert.ToInt32(dictionary2["primaryGenreId"]);
			appID = Convert.ToInt32(dictionary2["trackId"]);
			version = dictionary2["version"] as string;
			validAppInfo = true;
		}
	}
}
