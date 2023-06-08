using UnityEngine;

namespace PlayFab.Internal;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
	private static T _instance;

	protected bool initialized;

	public static T instance
	{
		get
		{
			CreateInstance();
			return _instance;
		}
	}

	public static void CreateInstance()
	{
		if ((Object)_instance == (Object)null)
		{
			_instance = Object.FindObjectOfType<T>();
			if ((Object)_instance == (Object)null)
			{
				_instance = new GameObject(typeof(T).Name).AddComponent<T>();
			}
			if (!_instance.initialized)
			{
				_instance.Initialize();
				_instance.initialized = true;
			}
		}
	}

	public virtual void Awake()
	{
		if (Application.isPlaying)
		{
			Object.DontDestroyOnLoad(this);
		}
		if ((Object)_instance != (Object)null)
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}

	protected virtual void Initialize()
	{
	}
}
