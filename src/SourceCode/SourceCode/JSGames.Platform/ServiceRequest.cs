using System.Collections.Generic;

namespace JSGames.Platform;

public class ServiceRequest
{
	public string _Type;

	public EventHandler _EventDelegate;

	public object _UserData;

	private static Dictionary<string, ServiceRequest> _Params = new Dictionary<string, ServiceRequest>();

	public ServiceRequest(string type, EventHandler handler, object userData)
	{
		_Type = type;
		_EventDelegate = handler;
		_UserData = userData;
		AddParam(_Type, this);
	}

	private void AddParam(string key, ServiceRequest value)
	{
		if (!_Params.ContainsKey(key))
		{
			_Params.Add(key, value);
		}
	}

	public static ServiceRequest GetValue(string key)
	{
		return _Params[key];
	}

	public static bool RemoveValue(string key)
	{
		return _Params.Remove(key);
	}
}
