using System;

[Serializable]
public class TagAndDefaultText
{
	public string _Tag;

	public LocaleString _DefaultText;

	public TagAndDefaultText(string tagName, LocaleString defaultText)
	{
		_Tag = tagName;
		_DefaultText = defaultText;
	}
}
