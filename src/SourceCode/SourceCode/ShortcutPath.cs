using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShortcutPath : MonoBehaviour
{
	public GameObject _Path;

	public int _PathID;

	public int _StartNodeIndex;

	public int _ResumeNodeIndex;

	private static List<ShortcutPath> mShortPathList;

	public static int pShortcutCount
	{
		get
		{
			if (mShortPathList == null)
			{
				return 0;
			}
			return mShortPathList.Count;
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (mShortPathList != null)
		{
			mShortPathList.Clear();
		}
		else
		{
			mShortPathList = new List<ShortcutPath>();
		}
	}

	private void Awake()
	{
		if (mShortPathList == null)
		{
			mShortPathList = new List<ShortcutPath>();
		}
		mShortPathList.Add(this);
	}

	public static void GenerateShortCutNodeData()
	{
		if (mShortPathList == null)
		{
			return;
		}
		mShortPathList.Sort((ShortcutPath inObj1, ShortcutPath inObj2) => inObj1._PathID.CompareTo(inObj2._PathID));
		foreach (ShortcutPath mShortPath in mShortPathList)
		{
			PathManager.pInstance.PushNodeInList(mShortPath._Path);
		}
	}
}
