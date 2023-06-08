using System;
using PlayFab.ClientModels;

namespace PlayFab.Authentication.Strategies;

internal sealed class SteamAuthStrategy : IAuthenticationStrategy
{
	public AuthTypes AuthType => AuthTypes.Steam;

	public void Authenticate(PlayFabAuthService authService, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, AuthKeys authKeys)
	{
		if (authKeys == null || string.IsNullOrEmpty(authKeys.AuthTicket))
		{
			authService.InvokeDisplayAuthentication();
			return;
		}
		PlayFabClientAPI.LoginWithSteam(new LoginWithSteamRequest
		{
			SteamTicket = authKeys.AuthTicket,
			InfoRequestParameters = authService.InfoRequestParams,
			CreateAccount = true
		}, resultCallback, errorCallback);
	}

	public void Link(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.LinkSteamAccount(new LinkSteamAccountRequest
		{
			SteamTicket = authKeys.AuthTicket,
			AuthenticationContext = authService.AuthenticationContext,
			ForceLink = authService.ForceLink
		}, delegate
		{
			authService.InvokeLink(AuthTypes.Steam);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeLink(AuthTypes.Steam, errorCallback);
		});
	}

	public void Unlink(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.UnlinkSteamAccount(new UnlinkSteamAccountRequest
		{
			AuthenticationContext = authService.AuthenticationContext
		}, delegate
		{
			authService.InvokeUnlink(AuthTypes.Steam);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeUnlink(AuthTypes.Steam, errorCallback);
		});
	}
}
