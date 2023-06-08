using System;
using System.Collections.Generic;
using PlayFab.Internal;
using PlayFab.ProfilesModels;

namespace PlayFab;

public static class PlayFabProfilesAPI
{
	static PlayFabProfilesAPI()
	{
	}

	public static void ForgetAllCredentials()
	{
		PlayFabSettings.staticPlayer.ForgetAllCredentials();
	}

	public static void GetGlobalPolicy(GetGlobalPolicyRequest request, Action<GetGlobalPolicyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Profile/GetGlobalPolicy", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetProfile(GetEntityProfileRequest request, Action<GetEntityProfileResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Profile/GetProfile", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetProfiles(GetEntityProfilesRequest request, Action<GetEntityProfilesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Profile/GetProfiles", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void SetGlobalPolicy(SetGlobalPolicyRequest request, Action<SetGlobalPolicyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Profile/SetGlobalPolicy", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void SetProfileLanguage(SetProfileLanguageRequest request, Action<SetProfileLanguageResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Profile/SetProfileLanguage", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void SetProfilePolicy(SetEntityProfilePolicyRequest request, Action<SetEntityProfilePolicyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Profile/SetProfilePolicy", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}
}
