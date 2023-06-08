using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

public class PlayerDeviceInfoEventData : PlayStreamEventBase
{
	public Dictionary<string, object> DeviceInfo;

	public string TitleId;
}
