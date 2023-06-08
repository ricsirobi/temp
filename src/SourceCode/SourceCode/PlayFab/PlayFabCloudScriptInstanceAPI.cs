using System;
using System.Collections.Generic;
using PlayFab.CloudScriptModels;
using PlayFab.Internal;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabCloudScriptInstanceAPI : IPlayFabInstanceApi
{
	public PlayFabApiSettings apiSettings;

	private PlayFabAuthenticationContext authenticationContext;

	public PlayFabCloudScriptInstanceAPI()
	{
	}

	public PlayFabCloudScriptInstanceAPI(PlayFabApiSettings settings)
	{
		apiSettings = settings;
	}

	public PlayFabCloudScriptInstanceAPI(PlayFabAuthenticationContext context)
	{
		authenticationContext = context;
	}

	public PlayFabCloudScriptInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
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

	public void ExecuteEntityCloudScript(ExecuteEntityCloudScriptRequest request, Action<ExecuteCloudScriptResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/CloudScript/ExecuteEntityCloudScript", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}
}
