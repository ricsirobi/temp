using System;
using System.Collections.Generic;
using PlayFab.DataModels;
using PlayFab.Internal;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabDataInstanceAPI : IPlayFabInstanceApi
{
	public PlayFabApiSettings apiSettings;

	private PlayFabAuthenticationContext authenticationContext;

	public PlayFabDataInstanceAPI()
	{
	}

	public PlayFabDataInstanceAPI(PlayFabApiSettings settings)
	{
		apiSettings = settings;
	}

	public PlayFabDataInstanceAPI(PlayFabAuthenticationContext context)
	{
		authenticationContext = context;
	}

	public PlayFabDataInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
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

	public void AbortFileUploads(AbortFileUploadsRequest request, Action<AbortFileUploadsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/File/AbortFileUploads", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void DeleteFiles(DeleteFilesRequest request, Action<DeleteFilesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/File/DeleteFiles", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void FinalizeFileUploads(FinalizeFileUploadsRequest request, Action<FinalizeFileUploadsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/File/FinalizeFileUploads", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void GetFiles(GetFilesRequest request, Action<GetFilesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/File/GetFiles", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void GetObjects(GetObjectsRequest request, Action<GetObjectsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/Object/GetObjects", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void InitiateFileUploads(InitiateFileUploadsRequest request, Action<InitiateFileUploadsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/File/InitiateFileUploads", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}

	public void SetObjects(SetObjectsRequest request, Action<SetObjectsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabHttp.MakeApiCall("/Object/SetObjects", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, apiSettings, this);
	}
}
