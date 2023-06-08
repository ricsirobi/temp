using System;
using System.Collections.Generic;
using PlayFab.Authentication.Strategies;
using PlayFab.ClientModels;
using PlayFab.Internal;
using UnityEngine;

namespace PlayFab;

public class PlayFabAuthService
{
	public delegate void DisplayAuthenticationEvent();

	public delegate void LoginSuccessEvent(LoginResult success);

	public delegate void PlayFabErrorEvent(PlayFabError error);

	public delegate void PlayFabLink(AuthTypes authType, PlayFabError error = null);

	public string Email;

	public string Username;

	public string Password;

	public GetPlayerCombinedInfoRequestParams InfoRequestParams;

	public bool ForceLink;

	private const string _LoginRememberKey = "PlayFabLoginRemember";

	private const string _PlayFabRememberMeIdKey = "PlayFabIdPassGuid";

	private const string _PlayFabAuthTypeKey = "PlayFabAuthType";

	private readonly Dictionary<AuthTypes, IAuthenticationStrategy> _authStrategies = new Dictionary<AuthTypes, IAuthenticationStrategy>();

	public PlayFabAuthenticationContext AuthenticationContext { get; private set; }

	public bool RememberMe
	{
		get
		{
			return PlayerPrefs.GetInt("PlayFabLoginRemember", 0) != 0;
		}
		set
		{
			PlayerPrefs.SetInt("PlayFabLoginRemember", value ? 1 : 0);
		}
	}

	public AuthTypes AuthType
	{
		get
		{
			return PlayFabUtil.TryEnumParse(PlayerPrefs.GetString("PlayFabAuthType"), AuthTypes.None);
		}
		set
		{
			PlayerPrefs.SetString("PlayFabAuthType", value.ToString());
		}
	}

	public string RememberMeId
	{
		get
		{
			return PlayerPrefs.GetString("PlayFabIdPassGuid", "");
		}
		set
		{
			string value2 = (string.IsNullOrEmpty(value) ? Guid.NewGuid().ToString() : value);
			PlayerPrefs.SetString("PlayFabIdPassGuid", value2);
		}
	}

	public event DisplayAuthenticationEvent OnDisplayAuthentication;

	public event LoginSuccessEvent OnLoginSuccess;

	public event PlayFabErrorEvent OnPlayFabError;

	public event PlayFabLink OnPlayFabLink;

	public event PlayFabLink OnPlayFabUnlink;

	public string GetOrCreateRememberMeId()
	{
		string text = PlayerPrefs.GetString("PlayFabIdPassGuid", "");
		if (string.IsNullOrEmpty(text))
		{
			text = Guid.NewGuid().ToString();
			PlayerPrefs.SetString("PlayFabIdPassGuid", text);
		}
		return text;
	}

	public PlayFabAuthService()
	{
		Type[] types = typeof(IAuthenticationStrategy).Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsInterface && typeof(IAuthenticationStrategy).IsAssignableFrom(type))
			{
				IAuthenticationStrategy authenticationStrategy = (IAuthenticationStrategy)Activator.CreateInstance(type);
				_authStrategies.Add(authenticationStrategy.AuthType, authenticationStrategy);
			}
		}
	}

	public bool IsClientLoggedIn()
	{
		if (AuthenticationContext != null)
		{
			return AuthenticationContext.IsClientLoggedIn();
		}
		return false;
	}

	public bool IsEntityLoggedIn()
	{
		if (AuthenticationContext != null)
		{
			return AuthenticationContext.IsEntityLoggedIn();
		}
		return false;
	}

	public void ClearRememberMe()
	{
		PlayerPrefs.DeleteKey("PlayFabLoginRemember");
		PlayerPrefs.DeleteKey("PlayFabIdPassGuid");
	}

	public void Authenticate(AuthTypes authType, AuthKeys authKeys = null)
	{
		AuthType = authType;
		Authenticate(authKeys);
	}

	public void Authenticate(AuthKeys authKeys = null)
	{
		if (AuthType == AuthTypes.None)
		{
			if (this.OnDisplayAuthentication != null)
			{
				this.OnDisplayAuthentication();
			}
			return;
		}
		if (RememberMe && !string.IsNullOrEmpty(RememberMeId))
		{
			PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
			{
				CustomId = RememberMeId,
				InfoRequestParameters = InfoRequestParams,
				CreateAccount = true
			}, InvokeLoginSuccess, InvokePlayFabError);
			return;
		}
		_authStrategies[AuthType].Authenticate(this, delegate(LoginResult resultCallback)
		{
			AuthenticationContext = resultCallback.AuthenticationContext;
			if (RememberMe && string.IsNullOrEmpty(RememberMeId))
			{
				PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest
				{
					CustomId = GetOrCreateRememberMeId(),
					ForceLink = ForceLink,
					AuthenticationContext = AuthenticationContext
				}, null, null);
			}
			InvokeLoginSuccess(resultCallback);
		}, InvokePlayFabError, authKeys);
	}

	internal void InvokeLink(AuthTypes linkType, PlayFabError error = null)
	{
		if (this.OnPlayFabLink != null)
		{
			this.OnPlayFabLink(linkType, error);
		}
	}

	internal void InvokeUnlink(AuthTypes unlinkType, PlayFabError error = null)
	{
		if (this.OnPlayFabUnlink != null)
		{
			this.OnPlayFabUnlink(unlinkType, error);
		}
	}

	internal void InvokeDisplayAuthentication()
	{
		if (this.OnDisplayAuthentication != null)
		{
			this.OnDisplayAuthentication();
		}
	}

	internal void InvokeLoginSuccess(LoginResult loginResult)
	{
		if (this.OnLoginSuccess != null)
		{
			this.OnLoginSuccess(loginResult);
		}
	}

	internal void InvokePlayFabError(PlayFabError playFabError)
	{
		if (this.OnPlayFabError != null)
		{
			this.OnPlayFabError(playFabError);
		}
	}

	public void Link(AuthKeys authKeys)
	{
		if (!IsClientLoggedIn())
		{
			InvokeLink(authKeys.AuthType, new PlayFabError
			{
				Error = PlayFabErrorCode.NotAuthorized,
				ErrorMessage = "You must log in before you can call L7ink."
			});
			return;
		}
		IAuthenticationStrategy authenticationStrategy = _authStrategies[authKeys.AuthType];
		if (authenticationStrategy == null)
		{
			Debug.LogError("Unhandled link type: " + authKeys.AuthType);
		}
		else
		{
			authenticationStrategy.Link(this, authKeys);
		}
	}

	public void Unlink(AuthKeys authKeys)
	{
		if (!IsClientLoggedIn())
		{
			InvokeUnlink(authKeys.AuthType, new PlayFabError
			{
				Error = PlayFabErrorCode.NotAuthorized,
				ErrorMessage = "You must log in before you can call Unlink."
			});
			return;
		}
		IAuthenticationStrategy authenticationStrategy = _authStrategies[authKeys.AuthType];
		if (authenticationStrategy == null)
		{
			Debug.LogError("Unhandled unlink type: " + authKeys.AuthType);
		}
		else
		{
			authenticationStrategy.Unlink(this, authKeys);
		}
	}
}
