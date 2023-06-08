using System;

namespace ExpansionMissionBoard;

[Serializable]
public class ExpansionMissionData
{
	public int _MissionID;

	public LocaleString _MissionDescriptionText;

	public LocaleString _MissionPromptText;

	public string _NPC;

	public string _Scene;
}
