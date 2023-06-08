using System;
using PlayFab.ClientModels;

namespace PlayFab.Authentication.Strategies;

internal sealed class GooglePlayGamesAuthStrategy : IAuthenticationStrategy
{
	public AuthTypes AuthType => AuthTypes.GooglePlayGames;

	public void Authenticate(PlayFabAuthService authService, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, AuthKeys authKeys)
	{
		if (authKeys == null || string.IsNullOrEmpty(authKeys.AuthTicket))
		{
			authService.InvokeDisplayAuthentication();
			return;
		}
		PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest
		{
			ServerAuthCode = authKeys.AuthTicket,
			InfoRequestParameters = authService.InfoRequestParams,
			CreateAccount = true
		}, resultCallback, errorCallback);
	}

	public void Link(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.LinkGoogleAccount(new LinkGoogleAccountRequest
		{
			ServerAuthCode = authKeys.AuthTicket,
			AuthenticationContext = authService.AuthenticationContext,
			ForceLink = authService.ForceLink
		}, delegate
		{
			authService.InvokeLink(AuthTypes.GooglePlayGames);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeLink(AuthTypes.GooglePlayGames, errorCallback);
		});
	}

	public void Unlink(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.UnlinkGoogleAccount(new UnlinkGoogleAccountRequest
		{
			AuthenticationContext = authService.AuthenticationContext
		}, delegate
		{
			authService.InvokeUnlink(AuthTypes.GooglePlayGames);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeUnlink(AuthTypes.GooglePlayGames, errorCallback);
		});
	}
}
