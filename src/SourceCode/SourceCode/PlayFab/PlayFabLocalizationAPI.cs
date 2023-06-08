using System;
using System.Collections.Generic;
using PlayFab.Internal;
using PlayFab.LocalizationModels;

namespace PlayFab;

public static class PlayFabLocalizationAPI
{
	static PlayFabLocalizationAPI()
	{
	}

	public static void ForgetAllCredentials()
	{
		PlayFabSettings.staticPlayer.ForgetAllCredentials();
	}

	public static void GetLanguageList(GetLanguageListRequest request, Action<GetLanguageListResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Locale/GetLanguageList", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}
}
