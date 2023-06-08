using System;
using System.Collections.Generic;
using PlayFab.Internal;
using PlayFab.LocalizationModels;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabLocalizationInstanceAPI : IPlayFabInstanceApi
{
	public PlayFabApiSettings apiSettings;

	private PlayFabAuthenticationContext authenticationContext;

	public PlayFabLocalizationInstanceAPI()
	{
	}

	public PlayFabLocalizationInstanceAPI(PlayFabApiSettings settings)
	{
		apiSettings = settings;
	}

	public PlayFabLocalizationInstanceAPI(PlayFabAuthenticationContext context)
	{
		authenticationContext = context;
	}

	public PlayFabLocalizationInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
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

	public void GetLanguageList(GetLanguageListRequest request, Action<GetLanguageListResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/Locale/GetLanguageList", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}
}
