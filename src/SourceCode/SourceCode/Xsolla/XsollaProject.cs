using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaProject : IParseble
{
	public long id { get; private set; }

	public long merchantId { get; private set; }

	public int recurringPackageCount { get; private set; }

	public string name { get; private set; }

	public string nameEn { get; private set; }

	public string virtualCurrencyName { get; private set; }

	public string virtualCurrencyIconUrl { get; private set; }

	public string projectUrl { get; private set; }

	public string returnUrl { get; private set; }

	public string eula { get; private set; }

	public bool isDiscrete { get; private set; }

	public bool isKeepUsers { get; private set; }

	public bool canRepeatPayment { get; private set; }

	public Dictionary<string, XComponent> components { get; private set; }

	public XsollaProject()
	{
		components = new Dictionary<string, XComponent>();
	}

	public IParseble Parse(JSONNode projectNode)
	{
		id = projectNode["id"].AsInt;
		name = projectNode["name"];
		nameEn = projectNode["nameEn"];
		virtualCurrencyName = projectNode["virtualCurrencyName"];
		virtualCurrencyIconUrl = projectNode["virtualCurrencyImage"];
		merchantId = projectNode["merchantId"].AsInt;
		isDiscrete = projectNode["isDiscrete"].AsBool;
		projectUrl = projectNode["projectUrl"];
		returnUrl = projectNode["returnUrl"];
		isKeepUsers = projectNode["isKeepUsers"].AsBool;
		recurringPackageCount = projectNode["recurringPackageCount"].AsInt;
		eula = projectNode["eula"];
		canRepeatPayment = projectNode["canRepeatPayment"].AsBool;
		JSONClass asObject = projectNode["components"].AsObject;
		if (asObject == null)
		{
			return this;
		}
		IEnumerator enumerator = asObject.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, JSONNode> keyValuePair = (KeyValuePair<string, JSONNode>)enumerator.Current;
			string value = keyValuePair.Value["name"].Value;
			bool asBool = keyValuePair.Value["enabled"].AsBool;
			UtDebug.Log("elem.Key " + keyValuePair.Key + " name " + value + " isEnabled " + asBool);
			XComponent value2 = new XComponent(value, asBool);
			components.Add(keyValuePair.Key, value2);
		}
		return this;
	}

	public override string ToString()
	{
		return $"[XsollaProject: id={id}, name={name}, nameEn={nameEn}, virtualCurrencyName={virtualCurrencyName}, merchantId={merchantId}, isDiscrete={isDiscrete}, projectUrl={projectUrl}, returnUrl={returnUrl}, isKeepUsers={isKeepUsers}, recurringPackageCount={recurringPackageCount}, eula={eula}, canRepeatPayment={canRepeatPayment}]";
	}
}
