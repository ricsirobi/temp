namespace PlayFab;

public sealed class PlayFabAuthenticationContext
{
	public string PlayFabId;

	public string ClientSessionTicket;

	public string EntityToken;

	public PlayFabAuthenticationContext()
	{
	}

	public PlayFabAuthenticationContext(string clientSessionTicket, string entityToken, string playFabId)
		: this()
	{
		ClientSessionTicket = clientSessionTicket;
		EntityToken = entityToken;
		PlayFabId = playFabId;
	}

	public bool IsClientLoggedIn()
	{
		return !string.IsNullOrEmpty(ClientSessionTicket);
	}

	public bool IsEntityLoggedIn()
	{
		return !string.IsNullOrEmpty(EntityToken);
	}

	public void ForgetAllCredentials()
	{
		ClientSessionTicket = null;
		EntityToken = null;
	}
}
