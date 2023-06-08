using System;
using System.Collections.Generic;
using PlayFab.AuthenticationModels;
using PlayFab.Internal;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabAuthenticationInstanceAPI : IPlayFabInstanceApi
{
	public PlayFabApiSettings apiSettings;

	private PlayFabAuthenticationContext authenticationContext;

	public PlayFabAuthenticationInstanceAPI()
	{
	}

	public PlayFabAuthenticationInstanceAPI(PlayFabApiSettings settings)
	{
		apiSettings = settings;
	}

	public PlayFabAuthenticationInstanceAPI(PlayFabAuthenticationContext context)
	{
		authenticationContext = context;
	}

	public PlayFabAuthenticationInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
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

	public bool IsEntityLoggedIn()
	{
		if (authenticationContext != null)
		{
			return authenticationContext.IsEntityLoggedIn();
		}
		return false;
	}

	public void ForgetAllCredentials()
	{
		if (authenticationContext != null)
		{
			authenticationContext.ForgetAllCredentials();
		}
	}

	public void GetEntityToken(GetEntityTokenRequest request, Action<GetEntityTokenResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		AuthType authType = AuthType.None;
		if (playFabAuthenticationContext.ClientSessionTicket != null)
		{
			authType = AuthType.LoginSession;
		}
		if (playFabAuthenticationContext.EntityToken != null)
		{
			authType = AuthType.EntityToken;
		}
		PlayFabHttp.MakeApiCall("/Authentication/GetEntityToken", request, authType, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}
}
