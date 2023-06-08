using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class AppsFlyerAnalyticAgent : MonoBehaviour, IAnalyticAgent
{
	public string _BundleIdentifier = "com.KnowledgeAdventure.SchoolOfDragons";

	private const string mAgentName = "AppsFlyer";

	public bool pOptedOut => true;

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	public void LogEvent(string inEventName, Dictionary<string, object> inParameter)
	{
	}

	public void LogEvent(string inEventName, Dictionary<string, string> inParameter)
	{
	}

	public void PurchaseEvent(string inEventName, Product product)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (product.definition.storeSpecificId != null)
		{
			dictionary.Add("af_content_id", product.definition.storeSpecificId);
		}
		else
		{
			UtDebug.Log("AppsFlyer :storeSpecificId is null");
		}
		if (product.metadata.localizedPriceString != null)
		{
			dictionary.Add("af_price", product.metadata.localizedPriceString);
		}
		else
		{
			UtDebug.Log("AppsFlyer :PRICE String is null");
		}
		if (product.metadata.localizedDescription != null)
		{
			dictionary.Add("af_description", product.metadata.localizedDescription);
		}
		else
		{
			UtDebug.Log("AppsFlyer :DESCRIPTION is null");
		}
		if (product.transactionID != null)
		{
			dictionary.Add("af_receipt_id", product.transactionID);
		}
		else
		{
			UtDebug.Log("AppsFlyer :transactionID is null");
		}
		if (product.metadata.isoCurrencyCode != null)
		{
			dictionary.Add("af_currency", product.metadata.isoCurrencyCode);
		}
		else
		{
			UtDebug.Log("AppsFlyer :isoCurrencyCode is null");
		}
		dictionary.Add("af_revenue", product.metadata.localizedPrice.ToString());
		UtDebug.Log("AppsFlyer :localizedPrice is " + product.metadata.localizedPrice);
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			UtDebug.Log("AppsFlyer " + item.Key + " = " + item.Value);
		}
	}

	public string GetAgentName()
	{
		return "AppsFlyer";
	}

	public void LogFTUEEvent(FTUEEvent inEventID, Dictionary<string, object> inParameter)
	{
	}

	public void LogEvent(AnalyticEvent inEventID, Dictionary<string, object> inParameter)
	{
	}

	public void LogFTUEEvent(FTUEEvent inEventID, string ID, int stepIndex, Dictionary<string, object> inParameter)
	{
	}

	public void OptOut()
	{
	}
}
