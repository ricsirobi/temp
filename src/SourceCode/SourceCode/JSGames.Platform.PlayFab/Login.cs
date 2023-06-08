using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;

namespace JSGames.Platform.PlayFab;

public class Login
{
	public class ServiceTypes
	{
		public const string RegisterUser = "REGISTER_USER";

		public const string UpdateParentName = "UPDATE_PARENT_NAME";

		public const string LoginUser = "LOGIN_USER";

		public const string LoginGuest = "LOGIN_GUEST";

		public const string RegisterGuest = "REGISTER_GUEST";

		public const string RegisterChild = "REGISTER_CHILD";

		public const string GetChildList = "GET_CHILD_LIST";
	}

	public class PlayFabPlayerInfo
	{
		public string MasterPlayFabID;

		public string TitlePlayFabID;

		public string EntityType;

		public string EntityDisplayname;

		public string SessionToken;

		public List<CharacterResult> _Characters;

		public CharacterResult _CurrentCharacter;
	}

	public class JSPlayerInfo
	{
		public string Email;

		public string UserName;

		public string UserID;

		public string Password;

		public string ParentName;

		public string ParentToken;

		public string ChildName;

		public string ChildUserID;

		public string ChildType;

		public string ChildToken;
	}

	private bool mGuestLogin;

	private bool mLinkJSAccount;

	public JSPlayerInfo JSPlayer { get; private set; }

	public PlayFabPlayerInfo PlayFabPlayer { get; private set; }

	public Login()
	{
		if (JSPlayer == null)
		{
			JSPlayer = new JSPlayerInfo();
		}
		if (PlayFabPlayer == null)
		{
			PlayFabPlayer = new PlayFabPlayerInfo();
		}
	}

	public void RegisterUser(RegisterPlayFabUserRequest registrationRequest, bool linkJSAccount, string userId, string token, EventHandler callback, object userData)
	{
		if (registrationRequest != null)
		{
			JSPlayer.Email = registrationRequest.Email;
			JSPlayer.Password = registrationRequest.Password;
			JSPlayer.UserName = registrationRequest.Username;
			JSPlayer.ParentName = registrationRequest.DisplayName;
			JSPlayer.UserID = userId;
			JSPlayer.ParentToken = token;
			mLinkJSAccount = linkJSAccount;
			new ServiceRequest("REGISTER_USER", callback, userData);
			PlayFabClientAPI.RegisterPlayFabUser(registrationRequest, OnRegisterSuccess, OnRegistrationFail);
		}
	}

	private void OnRegisterSuccess(RegisterPlayFabUserResult result)
	{
		PlayFabPlayer.MasterPlayFabID = result.PlayFabId;
		PlayFabPlayer.TitlePlayFabID = result.EntityToken.Entity.Id;
		PlayFabPlayer.EntityType = result.EntityToken.Entity.Type;
		PlayFabPlayer.SessionToken = result.SessionTicket;
		ServiceRequest value = ServiceRequest.GetValue("REGISTER_USER");
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.COMPLETE, result, value._UserData);
		}
		UpdateTitleUserDisplayName(JSPlayer.ParentName, value?._EventDelegate);
		ServiceRequest.RemoveValue("REGISTER_USER");
		if (mLinkJSAccount)
		{
			PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest
			{
				CustomId = JSPlayer.UserID,
				ForceLink = true
			}, null, null);
		}
	}

	private void OnRegistrationFail(PlayFabError error)
	{
		ServiceRequest value = ServiceRequest.GetValue("REGISTER_USER");
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.ERROR, error, value._UserData);
			ServiceRequest.RemoveValue("REGISTER_USER");
		}
	}

	public void UpdateTitleUserDisplayName(string playerName, EventHandler callback)
	{
		PlayFabAuthService.OnUpdateUserTitleDisplayName += OnUpdateTitleUserDisplayName;
		PlayFabAuthService.OnPlayFabError += OnUpdateTitleUserDisplayNameFail;
		new ServiceRequest("UPDATE_PARENT_NAME", callback, null);
		PlayFabAuthService.Instance.UpdateTitleUserDisplayName(playerName);
	}

	private void OnUpdateTitleUserDisplayName(UpdateUserTitleDisplayNameResult result)
	{
		PlayFabPlayer.EntityDisplayname = result.DisplayName;
		ServiceRequest value = ServiceRequest.GetValue("UPDATE_PARENT_NAME");
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.COMPLETE, result, value._UserData);
			ServiceRequest.RemoveValue("UPDATE_PARENT_NAME");
		}
		PlayFabAuthService.OnUpdateUserTitleDisplayName -= OnUpdateTitleUserDisplayName;
		PlayFabAuthService.OnPlayFabError -= OnUpdateTitleUserDisplayNameFail;
	}

	private void OnUpdateTitleUserDisplayNameFail(PlayFabError error)
	{
		ServiceRequest value = ServiceRequest.GetValue("UPDATE_PARENT_NAME");
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.ERROR, error, value._UserData);
			ServiceRequest.RemoveValue("UPDATE_PARENT_NAME");
		}
		PlayFabAuthService.OnUpdateUserTitleDisplayName -= OnUpdateTitleUserDisplayName;
		PlayFabAuthService.OnPlayFabError -= OnUpdateTitleUserDisplayNameFail;
	}

	public void LoginUser(string UserID, bool guest, EventHandler callback, object userData)
	{
		mGuestLogin = guest;
		PlayFabAuthService.Instance.CustomId = UserID;
		PlayFabAuthService.Instance.RememberMe = false;
		JSPlayer.UserID = UserID;
		string type = "LOGIN_USER";
		if (mGuestLogin)
		{
			type = "LOGIN_GUEST";
		}
		new ServiceRequest(type, callback, userData);
		PlayFabAuthService.OnLoginSuccess += OnLoginSuccess;
		PlayFabAuthService.OnPlayFabError += OnLoginUserFail;
		PlayFabAuthService.Instance.Authenticate(Authtypes.Silent);
	}

	private void OnLoginSuccess(LoginResult result)
	{
		PlayFabPlayer.MasterPlayFabID = result.PlayFabId;
		PlayFabPlayer.TitlePlayFabID = result.EntityToken.Entity.Id;
		PlayFabPlayer.EntityType = result.EntityToken.Entity.Type;
		PlayFabPlayer.SessionToken = result.SessionTicket;
		string key = "LOGIN_USER";
		if (mGuestLogin)
		{
			key = "LOGIN_GUEST";
		}
		ServiceRequest value = ServiceRequest.GetValue(key);
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.COMPLETE, result, value._UserData);
			ServiceRequest.RemoveValue(key);
		}
		PlayFabAuthService.OnLoginSuccess -= OnLoginSuccess;
		PlayFabAuthService.OnPlayFabError -= OnLoginUserFail;
	}

	private void OnLoginUserFail(PlayFabError error)
	{
		string key = "LOGIN_USER";
		if (mGuestLogin)
		{
			key = "LOGIN_GUEST";
		}
		ServiceRequest value = ServiceRequest.GetValue(key);
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.COMPLETE, error, value._UserData);
			ServiceRequest.RemoveValue(key);
		}
		PlayFabAuthService.OnLoginSuccess -= OnLoginSuccess;
		PlayFabAuthService.OnPlayFabError -= OnLoginUserFail;
	}

	public void RegisterGuest(RegisterGuestRequest request, EventHandler callback, object userData)
	{
		JSPlayer.UserName = request.Username;
		JSPlayer.Email = request.Email;
		JSPlayer.ParentName = request.ParentName;
		JSPlayer.Password = request.Password;
		PlayFabAuthService.Instance.Username = JSPlayer.UserName;
		PlayFabAuthService.Instance.Email = JSPlayer.Email;
		PlayFabAuthService.Instance.Password = JSPlayer.Password;
		new ServiceRequest("REGISTER_GUEST", callback, userData);
		PlayFabAuthService.OnLoginSuccess += OnRegisterGuest;
		PlayFabAuthService.OnPlayFabError += OnRegisterGuestFail;
		PlayFabAuthService.Instance.Authenticate(Authtypes.RegisterPlayFabAccount);
	}

	private void OnRegisterGuest(LoginResult result)
	{
		PlayFabPlayer.MasterPlayFabID = result.PlayFabId;
		PlayFabPlayer.TitlePlayFabID = result.EntityToken.Entity.Id;
		PlayFabPlayer.EntityType = result.EntityToken.Entity.Type;
		PlayFabPlayer.SessionToken = result.SessionTicket;
		PlayFabAuthService.OnLoginSuccess -= OnRegisterGuest;
		PlayFabAuthService.OnPlayFabError -= OnRegisterGuestFail;
		ServiceRequest value = ServiceRequest.GetValue("REGISTER_GUEST");
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.COMPLETE, result, value._UserData);
		}
		UpdateTitleUserDisplayName(JSPlayer.ParentName, value?._EventDelegate);
		ServiceRequest.RemoveValue("REGISTER_GUEST");
	}

	private void OnRegisterGuestFail(PlayFabError error)
	{
		ServiceRequest value = ServiceRequest.GetValue("REGISTER_GUEST");
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.ERROR, error, value._UserData);
			ServiceRequest.RemoveValue("REGISTER_GUEST");
		}
		PlayFabAuthService.OnLoginSuccess -= OnRegisterGuest;
		PlayFabAuthService.OnPlayFabError -= OnRegisterGuestFail;
	}

	public void RegisterChild(RegisterChildRequest request, EventHandler callback, object userData)
	{
		JSPlayer.ChildName = request.Name;
		JSPlayer.ChildUserID = request.UserID;
		JSPlayer.ChildToken = request.Token;
		JSPlayer.ChildType = request.Type;
		new ServiceRequest("REGISTER_CHILD", callback, userData);
		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
		{
			FunctionName = "createCharacter",
			FunctionParameter = new
			{
				name = JSPlayer.ChildName,
				type = JSPlayer.ChildType,
				userId = JSPlayer.ChildUserID
			},
			GeneratePlayStreamEvent = true
		}, OnRegisterChild, OnRegisterChildFail);
	}

	private void OnRegisterChild(ExecuteCloudScriptResult result)
	{
		ServiceRequest value = ServiceRequest.GetValue("REGISTER_CHILD");
		if (value == null)
		{
			return;
		}
		if (result.Error != null && result.Logs.Count > 0)
		{
			string responseObj = string.Empty;
			foreach (LogStatement log in result.Logs)
			{
				if (log.Level == "Error")
				{
					responseObj = log.Message;
					break;
				}
			}
			if (value._EventDelegate != null)
			{
				value._EventDelegate(value._Type, EventType.ERROR, responseObj, value._UserData);
			}
		}
		else
		{
			((JsonObject)result.FunctionResult).TryGetValue("result", out var value2);
			if (value._EventDelegate != null)
			{
				value._EventDelegate(value._Type, EventType.COMPLETE, value2, value._UserData);
			}
		}
		ServiceRequest.RemoveValue("REGISTER_CHILD");
	}

	private void OnRegisterChildFail(PlayFabError error)
	{
		ServiceRequest value = ServiceRequest.GetValue("REGISTER_CHILD");
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.COMPLETE, error, value._UserData);
			ServiceRequest.RemoveValue("REGISTER_CHILD");
		}
	}

	public void GetChildList(EventHandler callback, object userData)
	{
		ListUsersCharactersRequest request = new ListUsersCharactersRequest
		{
			PlayFabId = PlayFabPlayer.MasterPlayFabID
		};
		new ServiceRequest("GET_CHILD_LIST", callback, userData);
		PlayFabClientAPI.GetAllUsersCharacters(request, OnGetChildList, OnGetChildListFail);
	}

	private void OnGetChildList(ListUsersCharactersResult result)
	{
		ServiceRequest value = ServiceRequest.GetValue("GET_CHILD_LIST");
		if (result != null && result.Characters.Count > 0)
		{
			PlayFabPlayer._Characters = new List<CharacterResult>(result.Characters);
		}
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.COMPLETE, result, value._UserData);
			ServiceRequest.RemoveValue("GET_CHILD_LIST");
		}
	}

	private void OnGetChildListFail(PlayFabError error)
	{
		ServiceRequest value = ServiceRequest.GetValue("GET_CHILD_LIST");
		if (value != null && value._EventDelegate != null)
		{
			value._EventDelegate(value._Type, EventType.COMPLETE, error, value._UserData);
			ServiceRequest.RemoveValue("GET_CHILD_LIST");
		}
	}
}
