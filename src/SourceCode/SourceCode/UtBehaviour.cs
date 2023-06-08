using System.Collections;
using UnityEngine;

public class UtBehaviour : MonoBehaviour
{
	private static UtBehaviour mInstance;

	public static UtBehaviour pInstance => mInstance;

	public static UtBehaviour Init()
	{
		if (mInstance == null)
		{
			GameObject obj = new GameObject("UtBehaviour");
			mInstance = obj.AddComponent<UtBehaviour>();
			Object.DontDestroyOnLoad(obj);
		}
		return mInstance;
	}

	public static void RunCoroutine(IEnumerator inCorutine)
	{
		Init();
		if (mInstance == null)
		{
			Debug.LogError("UtBehaviour not initialized.");
		}
		else
		{
			mInstance.StartCoroutine(inCorutine);
		}
	}
}
