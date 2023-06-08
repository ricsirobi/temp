using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaPricepointsManager : XsollaObjectsManager<XsollaPricepoint>, IParseble
{
	public struct FormParam
	{
		public string name { get; private set; }

		public object value { get; private set; }

		public FormParam(string newName, object newValue)
		{
			this = default(FormParam);
			name = newName;
			value = newValue;
		}
	}

	private Dictionary<string, object> formParams;

	private string projectCurrency;

	public string GetProjectCurrency()
	{
		return projectCurrency;
	}

	public IParseble Parse(JSONNode pricepointsNode)
	{
		IEnumerator<JSONNode> enumerator = pricepointsNode["list"].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			AddItem(new XsollaPricepoint().Parse(enumerator.Current) as XsollaPricepoint);
		}
		JSONNode jSONNode = pricepointsNode["formParams"];
		formParams = new Dictionary<string, object>(jSONNode.Count);
		IEnumerator<JSONNode> enumerator2 = jSONNode.Childs.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			JSONNode current = enumerator2.Current;
			formParams.Add(current["name"], current["value"]);
		}
		projectCurrency = pricepointsNode["projectCurrency"];
		return this;
	}
}
