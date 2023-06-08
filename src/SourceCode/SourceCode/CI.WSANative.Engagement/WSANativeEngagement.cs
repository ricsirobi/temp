using System;
using System.Collections.Generic;

namespace CI.WSANative.Engagement;

public static class WSANativeEngagement
{
	public static Action<Dictionary<string, string>> _ShowFeedbackHub;

	public static Func<bool> _IsFeedbackHubSupported;

	public static bool IsFeedbackHubSupported()
	{
		return false;
	}

	public static void ShowFeedbackHub(Dictionary<string, string> feedbackProperties = null)
	{
	}
}
