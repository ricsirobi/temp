using System.Collections.Generic;
using UnityEngine;

public class CogsPortalTrigger : ObTrigger
{
	public bool _LoadFromScene;

	public List<string> _LevelsToLoad;

	private Task mCurrentTask;

	public string _GameModuleName = "CogsDO";

	public override void DoTriggerAction(GameObject other)
	{
		string loadLevel = "";
		if (_LevelsToLoad != null && _LevelsToLoad.Count > 0)
		{
			int index = Random.Range(0, _LevelsToLoad.Count - 1);
			CogsLevelManager.pLevelToLoad = _LevelsToLoad[index];
		}
		if (string.IsNullOrEmpty(CogsLevelManager.pLevelToLoad) && MissionManager.pInstance != null)
		{
			List<Task> tasks = MissionManager.pInstance.GetTasks("Game", null, null);
			mCurrentTask = tasks.Find((Task task) => task.GetObjectiveValue<string>("Name").Contains(_GameModuleName));
			if (mCurrentTask != null && mCurrentTask.pData != null && mCurrentTask.pData.Objectives != null)
			{
				CogsLevelManager.pLevelToLoad = mCurrentTask.GetObjectiveValue<string>("Level");
				if (string.IsNullOrEmpty(CogsLevelManager.pLevelToLoad))
				{
					mCurrentTask = null;
					UtDebug.Log("Cogs : No level info to load from Mission, Loaded normal game");
				}
			}
		}
		if (!_LoadFromScene)
		{
			RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("CogsAsset"), OnBundleReady, typeof(GameObject));
			loadLevel = _LoadLevel;
			_LoadLevel = "";
		}
		base.DoTriggerAction(other);
		if (!_LoadFromScene)
		{
			_LoadLevel = loadLevel;
		}
	}

	private void OnBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				CogsLevelManager componentInChildren = Object.Instantiate((GameObject)inObject).GetComponentInChildren<CogsLevelManager>();
				if (componentInChildren != null)
				{
					componentInChildren._LoadLastLevel = false;
				}
			}
			else
			{
				UtDebug.LogError("Asset not found in the bundle");
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to download the asset bundle");
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}
}
