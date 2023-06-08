using System;
using PlayFab.ClientModels;
using UnityEngine;

namespace PlayFab.Authentication.Strategies;

internal sealed class EmailPasswordAuthStrategy : IAuthenticationStrategy
{
	public AuthTypes AuthType => AuthTypes.EmailPassword;

	public void Authenticate(PlayFabAuthService authService, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, AuthKeys authKeys)
	{
		if (!authService.RememberMe && string.IsNullOrEmpty(authService.Email) && string.IsNullOrEmpty(authService.Password))
		{
			authService.InvokeDisplayAuthentication();
			return;
		}
		PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest
		{
			Email = authService.Email,
			Password = authService.Password,
			InfoRequestParameters = authService.InfoRequestParams,
			TitleId = PlayFabSettings.TitleId
		}, resultCallback, delegate(PlayFabError error)
		{
			if (error.Error == PlayFabErrorCode.AccountNotFound)
			{
				Debug.LogWarning("Authentication failed. Try to register account.");
				PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest
				{
					Username = authService.Username,
					Password = authService.Password,
					Email = authService.Email,
					InfoRequestParameters = authService.InfoRequestParams,
					TitleId = PlayFabSettings.TitleId
				}, delegate(RegisterPlayFabUserResult registerSuccess)
				{
					resultCallback(new LoginResult
					{
						AuthenticationContext = registerSuccess.AuthenticationContext,
						EntityToken = registerSuccess.EntityToken,
						CustomData = registerSuccess.CustomData,
						NewlyCreated = true,
						PlayFabId = registerSuccess.PlayFabId,
						SessionTicket = registerSuccess.SessionTicket,
						SettingsForUser = registerSuccess.SettingsForUser,
						Request = registerSuccess.Request,
						LastLoginTime = DateTime.UtcNow
					});
				}, errorCallback);
			}
			else
			{
				errorCallback(error);
			}
		});
	}

	public void Link(PlayFabAuthService authService, AuthKeys authKeys)
	{
		throw new NotSupportedException();
	}

	public void Unlink(PlayFabAuthService authService, AuthKeys authKeys)
	{
		throw new NotSupportedException();
	}
}
