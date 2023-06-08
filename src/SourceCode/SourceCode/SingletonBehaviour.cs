using UnityEngine;

public class SingletonBehaviour<TYPE> : MonoBehaviour where TYPE : Component
{
	private static TYPE mInstance;

	public static TYPE pInstance
	{
		get
		{
			if ((Object)mInstance == (Object)null)
			{
				Init();
			}
			return mInstance;
		}
	}

	public static void Init()
	{
		Init(typeof(TYPE).ToString());
	}

	public static void Init(string inObjectName)
	{
		if (!((Object)mInstance != (Object)null))
		{
			GameObject obj = new GameObject(inObjectName);
			mInstance = obj.AddComponent<TYPE>();
			Object.DontDestroyOnLoad(obj);
			obj.SetActive(value: true);
		}
	}
}
