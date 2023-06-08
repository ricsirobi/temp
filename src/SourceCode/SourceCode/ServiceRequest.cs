using System.Collections.Generic;

public class ServiceRequest
{
	public static bool pBlockRequest;

	public WsServiceType _Type;

	public WsServiceEventHandler _EventDelegate;

	public object _UserData;

	public string _URL;

	public Dictionary<string, object> _Params = new Dictionary<string, object>();

	public void AddParam(string inKey, object inVal)
	{
		_Params.Add(inKey, inVal);
	}

	public object GetValue(string inKey)
	{
		return _Params[inKey];
	}
}
