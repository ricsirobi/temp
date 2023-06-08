using System;
using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

public class GameLobbyEndedEventData : PlayStreamEventBase
{
	public string GameMode;

	public DateTime? GameStartTime;

	public string Region;

	public string ServerBuildVersion;

	public string ServerHost;

	public string ServerHostInstanceId;

	public string ServerIPV4Address;

	public string ServerIPV6Address;

	public uint ServerPort;

	public Dictionary<string, string> Tags;

	public string TitleId;
}
