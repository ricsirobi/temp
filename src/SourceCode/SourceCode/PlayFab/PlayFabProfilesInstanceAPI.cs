using System;
using System.Collections.Generic;
using PlayFab.Internal;
using PlayFab.ProfilesModels;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabProfilesInstanceAPI : IPlayFabInstanceApi
{
	public PlayFabApiSettings apiSettings;

	private PlayFabAuthenticationContext authenticationContext;

	public PlayFabProfilesInstanceAPI()
	{
	}

	public PlayFabProfilesInstanceAPI(PlayFabApiSettings settings)
	{
		apiSettings = settings;
	}

	public PlayFabProfilesInstanceAPI(PlayFabAuthenticationContext context)
	{
		authenticationContext = context;
	}

	public PlayFabProfilesInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
	{
		apiSettings = settings;
		authenticationContext = context;
	}

	public void SetAuthenticationContext(PlayFabAuthenticationContext context)
	{
		authenticationContext = context;
	}

	public PlayFabAuthenticationContext GetAuthenticationContext()
	{
		return authenticationContext;
	}

	public void ForgetAllCredentials()
	{
		if (authenticationContext != null)
		{
			authenticationContext.ForgetAllCredentials();
		}
	}

	public void GetGlobalPolicy(GetGlobalPolicyRequest request, Action<GetGlobalPolicyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/Profile/GetGlobalPolicy", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void GetProfile(GetEntityProfileRequest request, Action<GetEntityProfileResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/Profile/GetProfile", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void GetProfiles(GetEntityProfilesRequest request, Action<GetEntityProfilesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/Profile/GetProfiles", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void SetGlobalPolicy(SetGlobalPolicyRequest request, Action<SetGlobalPolicyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/Profile/SetGlobalPolicy", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void SetProfileLanguage(SetProfileLanguageRequest request, Action<SetProfileLanguageResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/Profile/SetProfileLanguage", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void SetProfilePolicy(SetEntityProfilePolicyRequest request, Action<SetEntityProfilePolicyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/Profile/SetProfilePolicy", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}
}
