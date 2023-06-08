using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

public class TitleSavedSurveyEventData : PlayStreamEventBase
{
	public string Genre;

	public List<string> Monetization;

	public List<string> Platforms;

	public List<string> PlayerModes;

	public string TitleName;

	public string TitleWebsite;
}
