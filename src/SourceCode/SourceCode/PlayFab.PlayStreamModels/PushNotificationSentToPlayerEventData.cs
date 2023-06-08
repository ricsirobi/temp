namespace PlayFab.PlayStreamModels;

public class PushNotificationSentToPlayerEventData : PlayStreamEventBase
{
	public string Body;

	public string ErrorMessage;

	public string ErrorName;

	public string Language;

	public string PushNotificationTemplateId;

	public string PushNotificationTemplateName;

	public string Subject;

	public bool Success;

	public string TitleId;
}
