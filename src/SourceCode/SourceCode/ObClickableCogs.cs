using System.Collections.Generic;
using UnityEngine;

public class ObClickableCogs : ObClickableCreateInstance
{
	public List<string> _LevelsToLoad;

	public override void OnActivate()
	{
		if (string.IsNullOrEmpty(_AssetName))
		{
			_AssetName = GameConfig.GetKeyData("CogsAsset");
		}
		if (_LevelsToLoad != null && _LevelsToLoad.Count > 0)
		{
			int index = Random.Range(0, _LevelsToLoad.Count - 1);
			CogsLevelManager.pLevelToLoad = _LevelsToLoad[index];
		}
		base.OnActivate();
	}

	public override void OnCreateInstance(GameObject go)
	{
		if (go != null)
		{
			CogsLevelManager componentInChildren = go.GetComponentInChildren<CogsLevelManager>();
			if (componentInChildren != null)
			{
				componentInChildren._LoadLastLevel = false;
			}
		}
		else
		{
			UtDebug.LogError("Asset not found in the bundle");
		}
	}
}
