using System;
using System.Collections.Generic;
using PlayFab.EventsModels;
using PlayFab.Internal;

namespace PlayFab;

public static class PlayFabEventsAPI
{
	static PlayFabEventsAPI()
	{
	}

	public static void ForgetAllCredentials()
	{
		PlayFabSettings.staticPlayer.ForgetAllCredentials();
	}

	public static void WriteEvents(WriteEventsRequest request, Action<WriteEventsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Event/WriteEvents", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}
}
