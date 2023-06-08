using System;
using System.Collections.Generic;

namespace PlayFab;

public interface IOneDSTransportPlugin : IPlayFabPlugin
{
	void DoPost(object request, Dictionary<string, string> extraHeaders, Action<object> callback);
}
