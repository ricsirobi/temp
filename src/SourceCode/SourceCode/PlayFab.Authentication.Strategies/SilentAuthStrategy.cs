using System;
using PlayFab.ClientModels;

namespace PlayFab.Authentication.Strategies;

internal sealed class SilentAuthStrategy : IAuthenticationStrategy
{
	public AuthTypes AuthType => AuthTypes.Silent;

	public void Authenticate(PlayFabAuthService authService, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, AuthKeys authKeys)
	{
		PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
		{
			CustomId = authService.GetOrCreateRememberMeId(),
			CreateAccount = true,
			InfoRequestParameters = authService.InfoRequestParams
		}, resultCallback, errorCallback);
	}

	public void Link(PlayFabAuthService authService, AuthKeys authKeys)
	{
		Authenticate(authService, delegate
		{
			PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest
			{
				CustomId = authService.GetOrCreateRememberMeId(),
				AuthenticationContext = authService.AuthenticationContext,
				ForceLink = authService.ForceLink
			}, delegate
			{
				authService.InvokeLink(AuthTypes.Silent);
			}, delegate(PlayFabError errorCallback)
			{
				authService.InvokeLink(AuthTypes.Silent, errorCallback);
			});
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeLink(AuthTypes.Silent, errorCallback);
		}, authKeys);
	}

	public void Unlink(PlayFabAuthService authService, AuthKeys authKeys)
	{
		Authenticate(authService, delegate
		{
			PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest
			{
				CustomId = authService.GetOrCreateRememberMeId(),
				AuthenticationContext = authService.AuthenticationContext
			}, delegate
			{
				authService.InvokeUnlink(AuthTypes.Silent);
			}, delegate(PlayFabError errorCallback)
			{
				authService.InvokeUnlink(AuthTypes.Silent, errorCallback);
			});
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeUnlink(AuthTypes.Silent, errorCallback);
		}, authKeys);
	}
}
