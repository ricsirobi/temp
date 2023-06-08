using System;
using PlayFab.ClientModels;

namespace PlayFab.Authentication.Strategies;

internal sealed class TwitchAuthStrategy : IAuthenticationStrategy
{
	public AuthTypes AuthType => AuthTypes.Twitch;

	public void Authenticate(PlayFabAuthService authService, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, AuthKeys authKeys)
	{
		if (authKeys == null || string.IsNullOrEmpty(authKeys.AuthTicket))
		{
			authService.InvokeDisplayAuthentication();
			return;
		}
		PlayFabClientAPI.LoginWithTwitch(new LoginWithTwitchRequest
		{
			AccessToken = authKeys.AuthTicket,
			InfoRequestParameters = authService.InfoRequestParams,
			CreateAccount = true
		}, resultCallback, errorCallback);
	}

	public void Link(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.LinkTwitch(new LinkTwitchAccountRequest
		{
			AccessToken = authKeys.AuthTicket,
			AuthenticationContext = authService.AuthenticationContext,
			ForceLink = authService.ForceLink
		}, delegate
		{
			authService.InvokeLink(AuthTypes.Twitch);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeLink(AuthTypes.Twitch, errorCallback);
		});
	}

	public void Unlink(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.UnlinkTwitch(new UnlinkTwitchAccountRequest
		{
			AuthenticationContext = authService.AuthenticationContext
		}, delegate
		{
			authService.InvokeUnlink(AuthTypes.Twitch);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeUnlink(AuthTypes.Twitch, errorCallback);
		});
	}
}
