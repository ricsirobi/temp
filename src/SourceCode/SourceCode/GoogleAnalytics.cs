using System.Collections;
using UnityEngine;

public class GoogleAnalytics : MonoBehaviour
{
	public string propertyID;

	public static GoogleAnalytics instance;

	public string bundleID;

	public string appName;

	public string appVersion;

	private string screenRes;

	private string clientID;

	private void Awake()
	{
		if ((bool)instance)
		{
			Object.DestroyImmediate(base.gameObject);
			return;
		}
		Object.DontDestroyOnLoad(base.gameObject);
		instance = this;
	}

	private void Start()
	{
		screenRes = Screen.width + "x" + Screen.height;
		clientID = SystemInfo.deviceUniqueIdentifier;
	}

	public void LogScreen(string title)
	{
		title = WWW.EscapeURL(title);
		string url = "http://www.google-analytics.com/collect?v=1&ul=en-us&t=appview&sr=" + screenRes + "&an=" + WWW.EscapeURL(appName) + "&a=448166238&tid=" + propertyID + "&aid=" + bundleID + "&cid=" + WWW.EscapeURL(clientID) + "&_u=.sB&av=" + appVersion + "&_v=ma1b3&cd=" + title + "&qt=2500&z=185";
		StartCoroutine(Process(new WWW(url)));
	}

	public void LogEvent(string titleCat, string titleAction)
	{
		titleCat = WWW.EscapeURL(titleCat);
		titleAction = WWW.EscapeURL(titleAction);
		string url = "http://www.google-analytics.com/collect?v=1&ul=en-us&t=event&sr=" + screenRes + "&an=" + WWW.EscapeURL(appName) + "&a=448166238&tid=" + propertyID + "&aid=" + bundleID + "&cid=" + WWW.EscapeURL(clientID) + "&_u=.sB&av=" + appVersion + "&_v=ma1b3&ec=" + titleCat + "&ea=" + titleAction + "&qt=2500&z=185";
		StartCoroutine(Process(new WWW(url)));
	}

	private IEnumerator Process(WWW www)
	{
		yield return www;
		if (www.error == null)
		{
			if (www.responseHeaders.ContainsKey("STATUS"))
			{
				if (www.responseHeaders["STATUS"] == "HTTP/1.1 200 OK")
				{
					Debug.Log("GA Success");
				}
				else
				{
					Debug.LogWarning(www.responseHeaders["STATUS"]);
				}
			}
			else
			{
				Debug.LogWarning("Event failed to send to Google");
			}
		}
		else
		{
			Debug.LogWarning(www.error.ToString());
		}
		www.Dispose();
	}
}
