using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class LocaleString
{
	[TextArea]
	[XmlElement(ElementName = "Text")]
	public string _Text = "";

	[XmlElement(ElementName = "ID")]
	public int _ID;

	[Obsolete("For XML Serialization Only", true)]
	public LocaleString()
	{
	}

	public LocaleString(string text)
	{
		_Text = text;
	}

	public string GetLocalizedString()
	{
		return StringTable.GetStringData(_ID, _Text);
	}
}
