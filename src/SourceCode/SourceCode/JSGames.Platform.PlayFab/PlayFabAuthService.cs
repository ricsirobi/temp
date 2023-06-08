using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace JSGames.Platform.PlayFab;

public class PlayFabAuthService
{
	public delegate void DisplayAuthenticationEvent();

	public delegate void LoginSuccessEvent(LoginResult success);

	public delegate void PlayFabErrorEvent(PlayFabError error);

	public delegate void UpdateUserTitleDisplayName(UpdateUserTitleDisplayNameResult success);

	public string Email;

	public string Username;

	public string Password;

	public string AuthTicket;

	public string CustomId;

	public GetPlayerCombinedInfoRequestParams InfoRequestParams;

	public bool ForceLink;

	private static string _playFabId;

	private static string _sessionTicket;

	private const string _LoginRememberKey = "PlayFabLoginRemember";

	private const string _PlayFabRememberMeIdKey = "PlayFabIdPassGuid";

	private const string _PlayFabAuthTypeKey = "PlayFabAuthType";

	private static PlayFabAuthService _instance;

	public static string PlayFabId => _playFabId;

	public static string SessionTicket => _sessionTicket;

	public static PlayFabAuthService Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PlayFabAuthService();
			}
			return _instance;
		}
	}

	public bool RememberMe
	{
		get
		{
			if (PlayerPrefs.GetInt("PlayFabLoginRemember", 0) != 0)
			{
				return true;
			}
			return false;
		}
		set
		{
			PlayerPrefs.SetInt("PlayFabLoginRemember", value ? 1 : 0);
		}
	}

	public Authtypes AuthType
	{
		get
		{
			return (Authtypes)PlayerPrefs.GetInt("PlayFabAuthType", 0);
		}
		set
		{
			PlayerPrefs.SetInt("PlayFabAuthType", (int)value);
		}
	}

	private string RememberMeId
	{
		get
		{
			return PlayerPrefs.GetString("PlayFabIdPassGuid", "");
		}
		set
		{
			string value2 = value ?? Guid.NewGuid().ToString();
			PlayerPrefs.SetString("PlayFabIdPassGuid", value2);
		}
	}

	public static event DisplayAuthenticationEvent OnDisplayAuthentication;

	public static event LoginSuccessEvent OnLoginSuccess;

	public static event PlayFabErrorEvent OnPlayFabError;

	public static event UpdateUserTitleDisplayName OnUpdateUserTitleDisplayName;

	public PlayFabAuthService()
	{
		_instance = this;
	}

	public void ClearRememberMe()
	{
		PlayerPrefs.DeleteKey("PlayFabLoginRemember");
		PlayerPrefs.DeleteKey("PlayFabIdPassGuid");
		PlayerPrefs.DeleteKey("PlayFabAuthType");
	}

	public void Authenticate(Authtypes authType)
	{
		AuthType = authType;
		Authenticate();
	}

	public void Authenticate()
	{
		switch (AuthType)
		{
		case Authtypes.None:
			if (PlayFabAuthService.OnDisplayAuthentication != null)
			{
				PlayFabAuthService.OnDisplayAuthentication();
			}
			break;
		case Authtypes.Silent:
			SilentlyAuthenticate();
			break;
		case Authtypes.EmailAndPassword:
			AuthenticateEmailPassword();
			break;
		case Authtypes.RegisterPlayFabAccount:
			AddAccountAndPassword();
			break;
		case Authtypes.UsernameAndPassword:
			break;
		}
	}

	private void AuthenticateEmailPassword()
	{
		if (RememberMe && !string.IsNullOrEmpty(RememberMeId))
		{
			PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
			{
				TitleId = PlayFabSettings.TitleId,
				CustomId = RememberMeId,
				CreateAccount = true,
				InfoRequestParameters = InfoRequestParams
			}, delegate(LoginResult result)
			{
				_playFabId = result.PlayFabId;
				_sessionTicket = result.SessionTicket;
				if (PlayFabAuthService.OnLoginSuccess != null)
				{
					PlayFabAuthService.OnLoginSuccess(result);
				}
			}, delegate(PlayFabError error)
			{
				if (PlayFabAuthService.OnPlayFabError != null)
				{
					PlayFabAuthService.OnPlayFabError(error);
				}
			});
			return;
		}
		if (string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Password))
		{
			PlayFabAuthService.OnDisplayAuthentication();
			return;
		}
		PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest
		{
			TitleId = PlayFabSettings.TitleId,
			Email = Email,
			Password = Password,
			InfoRequestParameters = InfoRequestParams
		}, delegate(LoginResult result)
		{
			_playFabId = result.PlayFabId;
			_sessionTicket = result.SessionTicket;
			if (RememberMe)
			{
				RememberMeId = Guid.NewGuid().ToString();
				AuthType = Authtypes.EmailAndPassword;
				PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest
				{
					CustomId = RememberMeId,
					ForceLink = ForceLink
				}, null, null);
			}
			if (PlayFabAuthService.OnLoginSuccess != null)
			{
				PlayFabAuthService.OnLoginSuccess(result);
			}
		}, delegate(PlayFabError error)
		{
			if (PlayFabAuthService.OnPlayFabError != null)
			{
				PlayFabAuthService.OnPlayFabError(error);
			}
		});
	}

	private void AddAccountAndPassword()
	{
		SilentlyAuthenticate(delegate(LoginResult result)
		{
			if (result == null)
			{
				PlayFabAuthService.OnPlayFabError(new PlayFabError
				{
					Error = PlayFabErrorCode.UnknownError,
					ErrorMessage = "Silent Authentication by Device failed"
				});
			}
			PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest
			{
				Username = (Username ?? result.PlayFabId),
				Email = Email,
				Password = Password
			}, delegate
			{
				if (PlayFabAuthService.OnLoginSuccess != null)
				{
					_playFabId = result.PlayFabId;
					_sessionTicket = result.SessionTicket;
					if (RememberMe)
					{
						RememberMeId = Guid.NewGuid().ToString();
						PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest
						{
							CustomId = RememberMeId,
							ForceLink = ForceLink
						}, null, null);
					}
					AuthType = Authtypes.EmailAndPassword;
					PlayFabAuthService.OnLoginSuccess(result);
				}
			}, delegate(PlayFabError error)
			{
				if (PlayFabAuthService.OnPlayFabError != null)
				{
					PlayFabAuthService.OnPlayFabError(error);
				}
			});
		});
	}

	private void SilentlyAuthenticate(Action<LoginResult> callback = null)
	{
		PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
		{
			TitleId = PlayFabSettings.TitleId,
			CustomId = (CustomId ?? SystemInfo.deviceUniqueIdentifier),
			CreateAccount = true,
			InfoRequestParameters = InfoRequestParams
		}, delegate(LoginResult result)
		{
			_playFabId = result.PlayFabId;
			_sessionTicket = result.SessionTicket;
			if (callback == null && PlayFabAuthService.OnLoginSuccess != null)
			{
				PlayFabAuthService.OnLoginSuccess(result);
			}
			else if (callback != null)
			{
				callback(result);
			}
		}, delegate(PlayFabError error)
		{
			if (callback == null && PlayFabAuthService.OnPlayFabError != null)
			{
				PlayFabAuthService.OnPlayFabError(error);
			}
			else
			{
				callback(null);
				Debug.LogError(error.GenerateErrorReport());
			}
		});
	}

	public void UnlinkSilentAuth()
	{
		SilentlyAuthenticate(delegate
		{
			PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest
			{
				CustomId = SystemInfo.deviceUniqueIdentifier
			}, null, null);
		});
	}

	public void UpdateTitleUserDisplayName(string displayName)
	{
		PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
		{
			DisplayName = displayName
		}, delegate(UpdateUserTitleDisplayNameResult result)
		{
			if (PlayFabAuthService.OnUpdateUserTitleDisplayName != null)
			{
				PlayFabAuthService.OnUpdateUserTitleDisplayName(result);
			}
		}, delegate(PlayFabError error)
		{
			if (PlayFabAuthService.OnPlayFabError != null)
			{
				PlayFabAuthService.OnPlayFabError(error);
			}
		});
	}
}
