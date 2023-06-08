namespace PlayFab.PlayStreamModels;

public class CharacterStatisticDeletedEventData : PlayStreamEventBase
{
	public string PlayerId;

	public string StatisticName;

	public string TitleId;

	public uint Version;
}
