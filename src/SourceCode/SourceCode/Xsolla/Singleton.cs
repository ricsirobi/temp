using UnityEngine;

namespace Xsolla;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static object _lock = new object();

	private static bool applicationIsQuitting = false;

	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				Logger.Log("[Singleton] Instance '" + typeof(T)?.ToString() + "' already destroyed on application quit. Won't create again - returning null.");
				return null;
			}
			lock (_lock)
			{
				if ((Object)_instance == (Object)null)
				{
					_instance = (T)Object.FindObjectOfType(typeof(T));
					if (Object.FindObjectsOfType(typeof(T)).Length > 1)
					{
						Logger.Log("[Singleton] Something went really wrong  - there should never be more than 1 singleton! Reopening the scene might fix it.");
						return _instance;
					}
					if ((Object)_instance == (Object)null)
					{
						GameObject gameObject = new GameObject();
						_instance = gameObject.AddComponent<T>();
						gameObject.name = "(singleton) " + typeof(T).ToString();
						Object.DontDestroyOnLoad(gameObject);
						Logger.Log("[Singleton] An instance of " + typeof(T)?.ToString() + " is needed in the scene, so '" + gameObject?.ToString() + "' was created with DontDestroyOnLoad.");
					}
					else
					{
						Logger.Log("[Singleton] Using instance already created: " + _instance.gameObject.name);
					}
				}
				return _instance;
			}
		}
	}

	public void OnDestroy()
	{
		_instance = null;
	}
}
