using UnityEngine;

[AddComponentMenu("Path-o-logical/PoolManager/Pre-Runtime Pool Item")]
public class PreRuntimePoolItem : MonoBehaviour
{
	public string poolName = "";

	public string prefabName = "";

	public bool despawnOnStart = true;

	public bool doNotReparent;

	private void Start()
	{
		if (!PoolManager.Pools.TryGetValue(poolName, out var spawnPool))
		{
			Debug.LogError($"PreRuntimePoolItem Error ('{base.name}'): No pool with the name '{poolName}' exists! Create one using the PoolManager Inspector interface or PoolManager.CreatePool().See the online docs for more information at http://docs.poolmanager.path-o-logical.com");
		}
		else
		{
			spawnPool.Add(base.transform, prefabName, despawnOnStart, !doNotReparent);
		}
	}
}
