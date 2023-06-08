using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaCountries : XsollaObjectsManager<XsollaCountry>, IParseble
{
	private string iso;

	public string GetIso()
	{
		return iso;
	}

	public IParseble Parse(JSONNode countriesNode)
	{
		iso = countriesNode["iso"];
		IEnumerator<JSONNode> enumerator = countriesNode["countryList"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			XsollaCountry xsollaCountry = new XsollaCountry();
			AddItem(xsollaCountry.Parse(enumerator.Current) as XsollaCountry);
		}
		return this;
	}
}
