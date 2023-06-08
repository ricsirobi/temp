using System;
using PlayFab.ClientModels;

namespace PlayFab.Authentication.Strategies;

internal sealed class GameCenterAuthStrategy : IAuthenticationStrategy
{
	public AuthTypes AuthType => AuthTypes.GameCenter;

	public void Authenticate(PlayFabAuthService authService, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, AuthKeys authKeys)
	{
		if (authKeys == null || string.IsNullOrEmpty(authKeys.AuthTicket))
		{
			authService.InvokeDisplayAuthentication();
			return;
		}
		PlayFabClientAPI.LoginWithGameCenter(new LoginWithGameCenterRequest
		{
			Signature = authKeys.AuthTicket,
			InfoRequestParameters = authService.InfoRequestParams,
			CreateAccount = true
		}, resultCallback, errorCallback);
	}

	public void Link(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.LinkGameCenterAccount(new LinkGameCenterAccountRequest
		{
			GameCenterId = authKeys.AuthTicket,
			AuthenticationContext = authService.AuthenticationContext,
			ForceLink = authService.ForceLink
		}, delegate
		{
			authService.InvokeLink(AuthTypes.GameCenter);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeLink(AuthTypes.GameCenter, errorCallback);
		});
	}

	public void Unlink(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.UnlinkGameCenterAccount(new UnlinkGameCenterAccountRequest
		{
			AuthenticationContext = authService.AuthenticationContext
		}, delegate
		{
			authService.InvokeUnlink(AuthTypes.GameCenter);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeUnlink(AuthTypes.GameCenter, errorCallback);
		});
	}
}
