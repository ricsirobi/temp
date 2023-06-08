public class AnalyticStoreItemViewedEvent
{
	public const string CURRENT_LOCATION = "CurrentLocation";

	public const string ITEM_CATEGORY = "ItemCategory";

	public const string ITEM_SUB_CATEGORY = "ItemSubCategory";

	public const string ITEM_NAME = "ItemName";

	public const string IS_MEMBER = "IsMember";

	public const string ITEM_PAGE = "ItemPage";

	public const string IS_MULTIPLAYER_MODE_ENABLED = "IsMultiPlayerModeEnabled";

	public const string ITEM_INFO_VIEWED = "ItemInfoViewed";

	public static string _CurrentLocation;

	public static string _ItemCategory;

	public static string _ItemSubCategory;

	public static string _ItemName;

	public static string _IsMember;

	public static string _ItemPage;

	public static string _IsMultiPlayerModeEnabled;

	public static void LogEvent()
	{
	}
}
