namespace PlayFab.PlayStreamModels;

public class PlayerLoggedInEventData : PlayStreamEventBase
{
	public EventLocation Location;

	public LoginIdentityProvider? Platform;

	public string PlatformUserId;

	public string PlatformUserName;

	public string TitleId;
}
