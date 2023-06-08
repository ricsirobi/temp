namespace PlayFab.PlayStreamModels;

public class TitleQueueConfigUpdatedEventData : PlayStreamEventBase
{
	public bool Deleted;

	public string DeveloperId;

	public string MatchQueueName;

	public string UserId;
}
