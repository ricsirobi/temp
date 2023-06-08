using System;
using PlayFab.ClientModels;

namespace PlayFab.Authentication.Strategies;

internal sealed class NintendoSwitchAuthStrategy : IAuthenticationStrategy
{
	public AuthTypes AuthType => AuthTypes.NintendoSwitch;

	public void Authenticate(PlayFabAuthService authService, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, AuthKeys authKeys)
	{
		PlayFabClientAPI.LoginWithNintendoSwitchDeviceId(new LoginWithNintendoSwitchDeviceIdRequest
		{
			NintendoSwitchDeviceId = PlayFabSettings.DeviceUniqueIdentifier,
			InfoRequestParameters = authService.InfoRequestParams,
			CreateAccount = true
		}, resultCallback, errorCallback);
	}

	public void Link(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.LinkNintendoSwitchDeviceId(new LinkNintendoSwitchDeviceIdRequest
		{
			NintendoSwitchDeviceId = PlayFabSettings.DeviceUniqueIdentifier,
			AuthenticationContext = authService.AuthenticationContext,
			ForceLink = authService.ForceLink
		}, delegate
		{
			authService.InvokeLink(AuthTypes.NintendoSwitch);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeLink(AuthTypes.NintendoSwitch, errorCallback);
		});
	}

	public void Unlink(PlayFabAuthService authService, AuthKeys authKeys)
	{
		PlayFabClientAPI.UnlinkNintendoSwitchDeviceId(new UnlinkNintendoSwitchDeviceIdRequest
		{
			AuthenticationContext = authService.AuthenticationContext,
			NintendoSwitchDeviceId = PlayFabSettings.DeviceUniqueIdentifier
		}, delegate
		{
			authService.InvokeUnlink(AuthTypes.NintendoSwitch);
		}, delegate(PlayFabError errorCallback)
		{
			authService.InvokeUnlink(AuthTypes.NintendoSwitch, errorCallback);
		});
	}
}
