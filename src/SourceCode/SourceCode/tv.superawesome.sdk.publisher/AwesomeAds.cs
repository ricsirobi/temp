using System;
using UnityEngine;

namespace tv.superawesome.sdk.publisher;

public class AwesomeAds : MonoBehaviour
{
	private static Action<GetIsMinorModel> callback = delegate
	{
	};

	private static AwesomeAds staticInstance = null;

	private static void createInstance()
	{
		if (staticInstance == null)
		{
			staticInstance = new GameObject().AddComponent<AwesomeAds>();
			staticInstance.name = "AwesomeAds";
			UnityEngine.Object.DontDestroyOnLoad(staticInstance);
		}
	}

	public static void init(bool loggingEnabled)
	{
		createInstance();
		Debug.Log("Initialising SDK");
	}

	public static void triggerAgeCheck(string age, Action<GetIsMinorModel> value)
	{
		createInstance();
		callback = value;
		Debug.Log("triggerAgeCheck for " + age);
	}

	public void nativeCallback(string payload)
	{
		try
		{
			GetIsMinorModel obj = JsonUtility.FromJson<GetIsMinorModel>(payload);
			callback(obj);
		}
		catch
		{
			Debug.Log("Error parsing GetIsMinorModel");
		}
	}
}
