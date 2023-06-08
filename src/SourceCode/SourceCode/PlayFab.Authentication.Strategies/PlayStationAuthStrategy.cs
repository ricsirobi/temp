using System;
using PlayFab.ClientModels;

namespace PlayFab.Authentication.Strategies;

internal sealed class PlayStationAuthStrategy : IAuthenticationStrategy
{
	public AuthTypes AuthType => AuthTypes.PlayStation;

	public void Authenticate(PlayFabAuthService authService, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, AuthKeys authKeys)
	{
		if (authKeys == null || string.IsNullOrEmpty(authKeys.AuthTicket))
		{
			authService.InvokeDisplayAuthentication();
			return;
		}
		PlayFabClientAPI.LoginWithPSN(new LoginWithPSNRequest
		{
			AuthCode = authKeys.AuthTicket,
			InfoRequestParameters = authService.InfoRequestParams,
			CreateAccount = true
		}, resultCallback, errorCallback);
	}

	public void Link(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.LinkPSNAccount(new LinkPSNAccountRequest
		{
			AuthCode = authKeys.AuthTicket,
			AuthenticationContext = authService.AuthenticationContext,
			ForceLink = authService.ForceLink
		}, delegate
		{
			authService.InvokeLink(AuthTypes.PlayStation);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeLink(AuthTypes.PlayStation, errorCallback);
		});
	}

	public void Unlink(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.UnlinkPSNAccount(new UnlinkPSNAccountRequest
		{
			AuthenticationContext = authService.AuthenticationContext
		}, delegate
		{
			authService.InvokeUnlink(AuthTypes.PlayStation);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeUnlink(AuthTypes.PlayStation, errorCallback);
		});
	}
}
