using System;
using System.Collections.Generic;

namespace CI.WSANative.Twitter;

public static class WSANativeTwitter
{
	public static bool IsLoggedIn => false;

	public static void Initialise(string consumerKey, string consumerSecret, string oauthCallback)
	{
	}

	public static void Login(Action<WSATwitterLoginResult> response)
	{
	}

	public static void Logout()
	{
	}

	public static void GetUserDetails(bool includeEmail, Action<WSATwitterResponse> response)
	{
	}

	public static void ApiRead(string url, IDictionary<string, string> parameters, Action<WSATwitterResponse> response)
	{
	}

	public static void ShowTweetDialog(IDictionary<string, string> parameters, Action closed)
	{
	}

	public static void ShowRetweetDialog(string tweetId, Action closed)
	{
	}

	public static void ShowLikeTweetDialog(string tweetId, Action closed)
	{
	}

	public static void ShowMiniProfileDialog(string userId, Action closed)
	{
	}

	public static void ShowFollowDialog(string userId, Action closed)
	{
	}
}
