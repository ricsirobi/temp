using System;
using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaStatusText
{
	public class StatusTextElement
	{
		private string key;

		private string pref;

		private string parameter;

		private string value;

		private string name;

		private StatusTextElement()
		{
		}

		public StatusTextElement(string key, string pref, string parameter, string value, string name)
		{
			this.key = key;
			this.pref = pref;
			this.parameter = parameter;
			this.value = value;
			this.name = name;
		}

		public string GetKey()
		{
			return key;
		}

		public string GetPref()
		{
			return pref;
		}

		public string GetParameter()
		{
			return parameter;
		}

		public string GetValue()
		{
			return value;
		}

		public string GetName()
		{
			return name;
		}

		public override string ToString()
		{
			return string.Format("[StatusTextElement]\n key= " + key + "\n pref= " + pref + "\n parameter= " + parameter + "\n value= " + value + "\n name= " + name);
		}
	}

	public static string ST_STATE = "state";

	public static string ST_BACKURL = "backurl";

	public static string ST_BACKURL_CAPTION = "backurl_caption";

	public static string ST_INFO = "info";

	public static string STE_KEY = "key";

	public static string STE_PREF = "pref";

	public static string STE_PARAMETER = "parameter";

	public static string STE_VALUE = "value";

	public static string STE_NAME = "name";

	private List<StatusTextElement> textElements;

	private Dictionary<string, StatusTextElement> textElementsMap;

	public string state { get; private set; }

	public string backUrl { get; private set; }

	public string backUrlCaption { get; private set; }

	public XsollaStatusText(JSONNode statusTextNode)
	{
		state = statusTextNode[ST_STATE];
		backUrl = statusTextNode[ST_BACKURL];
		backUrlCaption = statusTextNode[ST_BACKURL_CAPTION];
		textElements = new List<StatusTextElement>();
		textElementsMap = new Dictionary<string, StatusTextElement>();
		IEnumerator<JSONNode> enumerator = statusTextNode[ST_INFO].Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode current = enumerator.Current;
			string value = current[STE_KEY].Value;
			string value2 = current[STE_PREF].Value;
			string value3 = current[STE_PARAMETER].Value;
			string text = current[STE_VALUE].Value;
			string value4 = current[STE_NAME].Value;
			if ("time".Equals(value))
			{
				DateTime dateTime = DateTime.Parse(text);
				text = $"{dateTime:dd/MM/yyyy HH:mm}";
			}
			if ("recurrentDateNextCharge".Equals(value))
			{
				DateTime dateTime2 = DateTime.Parse(text);
				text = $"{dateTime2:dd/MM/yyyy}";
			}
			StatusTextElement textElement = new StatusTextElement(value, value2, value3, text, value4);
			AddStatusTextElement(textElement);
		}
	}

	public void AddStatusTextElement(StatusTextElement textElement)
	{
		textElements.Add(textElement);
		textElementsMap.Add(textElement.GetKey(), textElement);
	}

	public StatusTextElement Get(string key)
	{
		if (textElementsMap.ContainsKey(key))
		{
			return textElementsMap[key];
		}
		Logger.Log("We get null StatusTextElement by key - " + key);
		return new StatusTextElement("", "", "", "", "");
	}

	public string GetPurchsaeValue()
	{
		if (textElementsMap.ContainsKey("out"))
		{
			return textElementsMap["out"].GetValue();
		}
		if (textElementsMap.ContainsKey("digital_goods"))
		{
			return textElementsMap["digital_goods"].GetValue();
		}
		return "";
	}

	public List<StatusTextElement> GetStatusTextElements()
	{
		return textElements;
	}

	public string GetState()
	{
		return state;
	}

	public string GetProjectString()
	{
		return textElementsMap["project"].GetPref() + " - " + textElementsMap["project"].GetValue();
	}

	public override string ToString()
	{
		return string.Format("[XsollaStatusText]\nstate='" + state + "\n, backUrl='" + backUrl + "\n, textElements='" + textElements);
	}
}
