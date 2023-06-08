using System;
using System.Collections.Generic;
using PlayFab.DataModels;
using PlayFab.Internal;

namespace PlayFab;

public static class PlayFabDataAPI
{
	static PlayFabDataAPI()
	{
	}

	public static void ForgetAllCredentials()
	{
		PlayFabSettings.staticPlayer.ForgetAllCredentials();
	}

	public static void AbortFileUploads(AbortFileUploadsRequest request, Action<AbortFileUploadsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/File/AbortFileUploads", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void DeleteFiles(DeleteFilesRequest request, Action<DeleteFilesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/File/DeleteFiles", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void FinalizeFileUploads(FinalizeFileUploadsRequest request, Action<FinalizeFileUploadsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/File/FinalizeFileUploads", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetFiles(GetFilesRequest request, Action<GetFilesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/File/GetFiles", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetObjects(GetObjectsRequest request, Action<GetObjectsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Object/GetObjects", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void InitiateFileUploads(InitiateFileUploadsRequest request, Action<InitiateFileUploadsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/File/InitiateFileUploads", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void SetObjects(SetObjectsRequest request, Action<SetObjectsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Object/SetObjects", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}
}
