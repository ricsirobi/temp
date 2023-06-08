using UnityEngine;

namespace JSGames;

public abstract class Singleton<T> : KAMonoBase where T : KAMonoBase
{
	private static T mInstance;

	public bool _PersistentOnSceneChange;

	public static T pInstance
	{
		get
		{
			if ((Object)mInstance == (Object)null)
			{
				mInstance = Object.FindObjectOfType<T>();
			}
			return mInstance;
		}
	}

	protected virtual void Awake()
	{
		if (_PersistentOnSceneChange)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	protected virtual void Start()
	{
		if (Object.FindObjectsOfType<T>().Length > 1)
		{
			Debug.Log(base.gameObject.name + " has been destroyed because another object already has the same component.");
			Object.Destroy(base.gameObject);
		}
	}

	protected virtual void OnDestroy()
	{
		if (this == mInstance)
		{
			mInstance = null;
		}
	}
}
