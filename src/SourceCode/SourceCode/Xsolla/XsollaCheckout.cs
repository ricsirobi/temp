using System.Collections.Generic;
using System.Text;
using SimpleJSON;

namespace Xsolla;

public class XsollaCheckout : IParseble
{
	private string action;

	private Dictionary<string, string> data;

	private string method;

	public string GetLink()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(action).Append("/?");
		foreach (KeyValuePair<string, string> datum in data)
		{
			stringBuilder.Append(datum.Key).Append("=").Append(datum.Value)
				.Append("&");
		}
		return stringBuilder.ToString();
	}

	public string GetMethod()
	{
		return method;
	}

	public IParseble Parse(JSONNode checkoutNode)
	{
		if (checkoutNode["action"] != null && !"null".Equals(checkoutNode["action"]))
		{
			action = checkoutNode["action"];
			data = new Dictionary<string, string>();
			IEnumerator<KeyValuePair<string, JSONNode>> enumerator = (checkoutNode["data"] as JSONClass).GetKeyValueDict().GetEnumerator();
			while (enumerator.MoveNext())
			{
				data.Add(enumerator.Current.Key, enumerator.Current.Value);
			}
			method = checkoutNode["method"];
			return this;
		}
		return null;
	}
}
