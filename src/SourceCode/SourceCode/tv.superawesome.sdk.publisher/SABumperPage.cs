using UnityEngine;

namespace tv.superawesome.sdk.publisher;

public class SABumperPage : MonoBehaviour
{
	private static SABumperPage staticInstance;

	public static void createInstance()
	{
		if (staticInstance == null)
		{
			staticInstance = new GameObject().AddComponent<SABumperPage>();
			staticInstance.name = "SABumperPage";
			Object.DontDestroyOnLoad(staticInstance);
		}
	}

	public static void overrideName(string name)
	{
		createInstance();
		Debug.Log("Trying to set name to " + name);
	}
}
