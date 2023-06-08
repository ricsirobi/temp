using UnityEngine;

public class ItemPrefabResData : ItemResNameData
{
	public Transform _Prefab;

	public override void Init(string resName)
	{
		_Prefab = null;
		base.Init(resName);
	}

	public override Object LoadRes(AssetBundle bd)
	{
		GameObject gameObject = CoBundleLoader.LoadGameObject(bd, _ResName);
		if (gameObject != null)
		{
			_Prefab = gameObject.transform;
		}
		return _Prefab;
	}
}
