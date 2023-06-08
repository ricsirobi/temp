using System.Collections.Generic;
using UnityEngine.Purchasing;

public interface IAnalyticAgent
{
	bool pOptedOut { get; }

	void LogFTUEEvent(FTUEEvent inEventID, Dictionary<string, object> inParameter);

	void LogEvent(AnalyticEvent inEventID, Dictionary<string, object> inParameter);

	void LogEvent(string inEventName, Dictionary<string, object> inParameter);

	void LogEvent(string inEventName, Dictionary<string, string> inParameter);

	void PurchaseEvent(string inEventName, Product product);

	void OptOut();

	string GetAgentName();
}
