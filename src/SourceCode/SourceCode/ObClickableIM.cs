using System.Collections.Generic;
using UnityEngine;

public class ObClickableIM : ObClickableCreateInstance
{
	public List<string> _LevelsToLoad;

	public override void OnActivate()
	{
		if (string.IsNullOrEmpty(_AssetName))
		{
			_AssetName = GameConfig.GetKeyData("IncredibleMachineAsset");
		}
		if (_LevelsToLoad != null && _LevelsToLoad.Count > 0)
		{
			int index = Random.Range(0, _LevelsToLoad.Count - 1);
			CTLevelManager.pLevelToLoad = _LevelsToLoad[index];
		}
		base.OnActivate();
	}

	public override void OnCreateInstance(GameObject inObject)
	{
		inObject.name = inObject.name.Replace("(Clone)", "");
	}
}
