using System;
using System.Xml.Serialization;

[Serializable]
public class GraphicsProperty
{
	[XmlElement(ElementName = "Default")]
	public string Default;

	[XmlElement(ElementName = "Available")]
	public string Available;

	private string[] mAvailableSettings;

	public string[] pAvailableSettings
	{
		get
		{
			if (mAvailableSettings == null)
			{
				if (string.IsNullOrEmpty(Available))
				{
					return null;
				}
				mAvailableSettings = Available.Split(',');
			}
			return mAvailableSettings;
		}
	}
}
