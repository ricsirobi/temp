using SimpleJSON;

namespace Xsolla;

public class XsollaInfo : IParseble
{
	private struct Attributes
	{
		public string key { get; private set; }

		public string pref { get; private set; }

		public string parameter { get; private set; }

		public Attributes(string newKey, string newPref, string newParameter)
		{
			key = newKey;
			pref = newPref;
			parameter = newParameter;
		}
	}

	private string name;

	private string value;

	private Attributes attributes;

	public string getName()
	{
		return name;
	}

	public string getValue()
	{
		return value;
	}

	public object getAttributes()
	{
		return attributes;
	}

	public IParseble Parse(JSONNode xsollaInfoNode)
	{
		name = xsollaInfoNode["name"].Value;
		value = xsollaInfoNode["value"].Value;
		string newKey = xsollaInfoNode["attributes"]["key"];
		string newPref = xsollaInfoNode["attributes"]["pref"];
		string newParameter = xsollaInfoNode["attributes"]["parameter"];
		attributes = new Attributes(newKey, newPref, newParameter);
		return this;
	}
}
