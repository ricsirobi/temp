using JSGames;

namespace DragonsPushNotifcations;

public class DragonsOneSignal : Singleton<DragonsOneSignal>
{
	private readonly string mOneSignalAppID = "1e54afd9-adb1-468f-9e54-64cbf3b7cda1";

	public string _AssetPath;

	public bool _IgnoreAlreadyPrompted;
}
