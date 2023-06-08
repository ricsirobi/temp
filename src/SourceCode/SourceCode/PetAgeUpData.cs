using System;

[Serializable]
public class PetAgeUpData
{
	public RaisedPetStage _FromPetStage;

	public RaisedPetStage _ToPetStage;

	public int _AgeUpItemID;

	public int _AgeUpTicketID;

	public LocaleString _AgeUpText;

	public LocaleString[] _UnlockPowers;

	public LocaleString[] _UnlockClasses;

	public KAWidget _TemplateWidget;
}
