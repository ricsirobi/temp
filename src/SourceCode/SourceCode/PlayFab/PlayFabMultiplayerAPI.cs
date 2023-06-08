using System;
using System.Collections.Generic;
using PlayFab.Internal;
using PlayFab.MultiplayerModels;

namespace PlayFab;

public static class PlayFabMultiplayerAPI
{
	static PlayFabMultiplayerAPI()
	{
	}

	public static void ForgetAllCredentials()
	{
		PlayFabSettings.staticPlayer.ForgetAllCredentials();
	}

	public static void CancelAllMatchmakingTicketsForPlayer(CancelAllMatchmakingTicketsForPlayerRequest request, Action<CancelAllMatchmakingTicketsForPlayerResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/CancelAllMatchmakingTicketsForPlayer", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void CancelMatchmakingTicket(CancelMatchmakingTicketRequest request, Action<CancelMatchmakingTicketResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/CancelMatchmakingTicket", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void CreateBuildWithCustomContainer(CreateBuildWithCustomContainerRequest request, Action<CreateBuildWithCustomContainerResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/CreateBuildWithCustomContainer", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void CreateBuildWithManagedContainer(CreateBuildWithManagedContainerRequest request, Action<CreateBuildWithManagedContainerResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/CreateBuildWithManagedContainer", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void CreateMatchmakingTicket(CreateMatchmakingTicketRequest request, Action<CreateMatchmakingTicketResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/CreateMatchmakingTicket", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void CreateRemoteUser(CreateRemoteUserRequest request, Action<CreateRemoteUserResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/CreateRemoteUser", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void CreateServerMatchmakingTicket(CreateServerMatchmakingTicketRequest request, Action<CreateMatchmakingTicketResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/CreateServerMatchmakingTicket", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void DeleteAsset(DeleteAssetRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/DeleteAsset", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void DeleteBuild(DeleteBuildRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/DeleteBuild", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void DeleteCertificate(DeleteCertificateRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/DeleteCertificate", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void DeleteRemoteUser(DeleteRemoteUserRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/DeleteRemoteUser", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void EnableMultiplayerServersForTitle(EnableMultiplayerServersForTitleRequest request, Action<EnableMultiplayerServersForTitleResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/EnableMultiplayerServersForTitle", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetAssetUploadUrl(GetAssetUploadUrlRequest request, Action<GetAssetUploadUrlResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/GetAssetUploadUrl", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetBuild(GetBuildRequest request, Action<GetBuildResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/GetBuild", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetContainerRegistryCredentials(GetContainerRegistryCredentialsRequest request, Action<GetContainerRegistryCredentialsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/GetContainerRegistryCredentials", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetMatch(GetMatchRequest request, Action<GetMatchResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/GetMatch", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetMatchmakingQueue(GetMatchmakingQueueRequest request, Action<GetMatchmakingQueueResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/GetMatchmakingQueue", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetMatchmakingTicket(GetMatchmakingTicketRequest request, Action<GetMatchmakingTicketResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/GetMatchmakingTicket", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetMultiplayerServerDetails(GetMultiplayerServerDetailsRequest request, Action<GetMultiplayerServerDetailsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/GetMultiplayerServerDetails", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetQueueStatistics(GetQueueStatisticsRequest request, Action<GetQueueStatisticsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/GetQueueStatistics", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetRemoteLoginEndpoint(GetRemoteLoginEndpointRequest request, Action<GetRemoteLoginEndpointResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/GetRemoteLoginEndpoint", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void GetTitleEnabledForMultiplayerServersStatus(GetTitleEnabledForMultiplayerServersStatusRequest request, Action<GetTitleEnabledForMultiplayerServersStatusResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/GetTitleEnabledForMultiplayerServersStatus", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void JoinMatchmakingTicket(JoinMatchmakingTicketRequest request, Action<JoinMatchmakingTicketResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/JoinMatchmakingTicket", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListArchivedMultiplayerServers(ListMultiplayerServersRequest request, Action<ListMultiplayerServersResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ListArchivedMultiplayerServers", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListAssetSummaries(ListAssetSummariesRequest request, Action<ListAssetSummariesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ListAssetSummaries", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListBuildSummaries(ListBuildSummariesRequest request, Action<ListBuildSummariesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ListBuildSummaries", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListCertificateSummaries(ListCertificateSummariesRequest request, Action<ListCertificateSummariesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ListCertificateSummaries", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListContainerImages(ListContainerImagesRequest request, Action<ListContainerImagesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ListContainerImages", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListContainerImageTags(ListContainerImageTagsRequest request, Action<ListContainerImageTagsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ListContainerImageTags", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListMatchmakingQueues(ListMatchmakingQueuesRequest request, Action<ListMatchmakingQueuesResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/ListMatchmakingQueues", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListMatchmakingTicketsForPlayer(ListMatchmakingTicketsForPlayerRequest request, Action<ListMatchmakingTicketsForPlayerResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/ListMatchmakingTicketsForPlayer", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListMultiplayerServers(ListMultiplayerServersRequest request, Action<ListMultiplayerServersResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ListMultiplayerServers", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListQosServers(ListQosServersRequest request, Action<ListQosServersResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ListQosServers", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ListVirtualMachineSummaries(ListVirtualMachineSummariesRequest request, Action<ListVirtualMachineSummariesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ListVirtualMachineSummaries", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void RemoveMatchmakingQueue(RemoveMatchmakingQueueRequest request, Action<RemoveMatchmakingQueueResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/RemoveMatchmakingQueue", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void RequestMultiplayerServer(RequestMultiplayerServerRequest request, Action<RequestMultiplayerServerResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/RequestMultiplayerServer", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void RolloverContainerRegistryCredentials(RolloverContainerRegistryCredentialsRequest request, Action<RolloverContainerRegistryCredentialsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/RolloverContainerRegistryCredentials", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void SetMatchmakingQueue(SetMatchmakingQueueRequest request, Action<SetMatchmakingQueueResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/Match/SetMatchmakingQueue", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void ShutdownMultiplayerServer(ShutdownMultiplayerServerRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/ShutdownMultiplayerServer", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void UpdateBuildRegions(UpdateBuildRegionsRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/UpdateBuildRegions", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}

	public static void UploadCertificate(UploadCertificateRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext authenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabHttp.MakeApiCall("/MultiplayerServer/UploadCertificate", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, authenticationContext);
	}
}
