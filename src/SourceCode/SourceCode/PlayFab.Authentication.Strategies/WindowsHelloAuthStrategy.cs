using System;
using PlayFab.ClientModels;

namespace PlayFab.Authentication.Strategies;

internal sealed class WindowsHelloAuthStrategy : IAuthenticationStrategy
{
	public AuthTypes AuthType => AuthTypes.WindowsHello;

	public void Authenticate(PlayFabAuthService authService, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, AuthKeys authKeys = null)
	{
		if (authKeys == null || string.IsNullOrEmpty(authKeys.WindowsHelloPublicKeyHint) || string.IsNullOrEmpty(authKeys.WindowsHelloChallengeSignature))
		{
			authService.InvokeDisplayAuthentication();
			return;
		}
		PlayFabClientAPI.LoginWithWindowsHello(new LoginWithWindowsHelloRequest
		{
			ChallengeSignature = authKeys.WindowsHelloChallengeSignature,
			PublicKeyHint = authKeys.WindowsHelloPublicKeyHint,
			InfoRequestParameters = authService.InfoRequestParams
		}, resultCallback, errorCallback);
	}

	public void Link(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.LinkWindowsHello(new LinkWindowsHelloAccountRequest
		{
			PublicKey = authKeys.AuthTicket,
			AuthenticationContext = authService.AuthenticationContext,
			ForceLink = authService.ForceLink,
			UserName = authService.Username
		}, delegate
		{
			authService.InvokeLink(AuthTypes.WindowsHello);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeLink(AuthTypes.WindowsHello, errorCallback);
		});
	}

	public void Unlink(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.UnlinkWindowsHello(new UnlinkWindowsHelloAccountRequest
		{
			AuthenticationContext = authService.AuthenticationContext,
			PublicKeyHint = authKeys.AuthTicket
		}, delegate
		{
			authService.InvokeUnlink(AuthTypes.WindowsHello);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeUnlink(AuthTypes.WindowsHello, errorCallback);
		});
	}
}
