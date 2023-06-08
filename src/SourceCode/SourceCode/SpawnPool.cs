using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Path-o-logical/PoolManager/SpawnPool")]
public sealed class SpawnPool : KAMonoBase, IList<Transform>, ICollection<Transform>, IEnumerable<Transform>, IEnumerable
{
	public string poolName = "";

	public bool matchPoolScale;

	public bool matchPoolLayer;

	public bool dontDestroyOnLoad;

	public bool logMessages;

	public List<PrefabPool> _perPrefabPoolOptions = new List<PrefabPool>();

	public Dictionary<object, bool> prefabsFoldOutStates = new Dictionary<object, bool>();

	[HideInInspector]
	public float maxParticleDespawnTime = 60f;

	public PrefabsDict prefabs = new PrefabsDict();

	public Dictionary<object, bool> _editorListItemStates = new Dictionary<object, bool>();

	public List<PrefabPool> _prefabPools = new List<PrefabPool>();

	internal List<Transform> _spawned = new List<Transform>();

	public Transform group { get; private set; }

	public Dictionary<string, PrefabPool> prefabPools
	{
		get
		{
			Dictionary<string, PrefabPool> dictionary = new Dictionary<string, PrefabPool>();
			foreach (PrefabPool prefabPool in _prefabPools)
			{
				dictionary[prefabPool.prefabGO.name] = prefabPool;
			}
			return dictionary;
		}
	}

	public Transform this[int index]
	{
		get
		{
			return _spawned[index];
		}
		set
		{
			throw new NotImplementedException("Read-only.");
		}
	}

	public int Count => _spawned.Count;

	public bool IsReadOnly
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	private void Awake()
	{
		if (dontDestroyOnLoad)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		group = base.transform;
		if (poolName == "")
		{
			poolName = group.name.Replace("Pool", "");
			poolName = poolName.Replace("(Clone)", "");
		}
		if (logMessages)
		{
			UtDebug.Log("SpawnPool " + poolName + ": Initializing..");
		}
		foreach (PrefabPool perPrefabPoolOption in _perPrefabPoolOptions)
		{
			if (perPrefabPoolOption.prefab == null)
			{
				UtDebug.LogWarning("Initialization Warning: Pool '" + poolName + "' contains a PrefabPool with no prefab reference. Skipping.");
				continue;
			}
			perPrefabPoolOption.inspectorInstanceConstructor();
			CreatePrefabPool(perPrefabPoolOption);
		}
		PoolManager.Pools.Add(this);
	}

	private void OnDestroy()
	{
		if (logMessages)
		{
			UtDebug.Log("SpawnPool " + poolName + ": Destroying...");
		}
		if (PoolManager.Pools.TryGetValue(poolName, out var spawnPool))
		{
			PoolManager.Pools.Remove(spawnPool);
		}
		StopAllCoroutines();
		_spawned.Clear();
		foreach (PrefabPool prefabPool in _prefabPools)
		{
			prefabPool.SelfDestruct();
		}
		_prefabPools.Clear();
		prefabs._Clear();
	}

	public void CreatePrefabPool(PrefabPool prefabPool)
	{
		if (prefabs.ContainsKey(prefabPool.prefab.name))
		{
			UtDebug.LogWarning("@@@@@@@@@@@@@@@ Is already exist: " + prefabPool.prefab.name);
		}
		bool flag = ((!(GetPrefab(prefabPool.prefab) == null)) ? true : false);
		if (!prefabs.ContainsKey(prefabPool.prefab.name) && !flag)
		{
			prefabPool.spawnPool = this;
			_prefabPools.Add(prefabPool);
			prefabs._Add(prefabPool.prefab.name, prefabPool.prefab);
		}
		if (!prefabPool.preloaded)
		{
			if (logMessages)
			{
				Debug.Log($"SpawnPool {poolName}: Preloading {prefabPool.preloadAmount} {prefabPool.prefab.name}");
			}
			prefabPool.PreloadInstances();
		}
	}

	public void Add(Transform instance, string prefabName, bool despawn, bool parent)
	{
		foreach (PrefabPool prefabPool in _prefabPools)
		{
			if (prefabPool.prefabGO == null)
			{
				Debug.LogError("Unexpected Error: PrefabPool.prefabGO is null");
				return;
			}
			if (prefabPool.prefabGO.name == prefabName)
			{
				prefabPool.AddUnpooled(instance, despawn);
				if (logMessages)
				{
					Debug.Log($"SpawnPool {poolName}: Adding previously unpooled instance {instance.name}");
				}
				if (parent)
				{
					instance.parent = group;
				}
				if (!despawn)
				{
					_spawned.Add(instance);
				}
				return;
			}
		}
		Debug.LogError($"SpawnPool {poolName}: PrefabPool {prefabName} not found.");
	}

	public void Add(Transform item)
	{
		throw new NotImplementedException("Use SpawnPool.Spawn() to properly add items to the pool.");
	}

	public void Remove(Transform item)
	{
		throw new NotImplementedException("Use Despawn() to properly manage items that should remain in the pool but be deactivated.");
	}

	public PrefabPool GetPrefabPool(Transform prefab)
	{
		if (prefab == null)
		{
			return null;
		}
		foreach (PrefabPool prefabPool in _prefabPools)
		{
			if (prefabPool == null || prefabPool.prefabGO == null)
			{
				Debug.LogWarning("@@@@@@@ not good object in prefab pool");
			}
			else if (prefabPool.prefabGO.name == prefab.gameObject.name)
			{
				return prefabPool;
			}
		}
		return null;
	}

	public GameObject Spawn(GameObject go, Vector3 pos, Quaternion rot)
	{
		return Spawn(go.transform, pos, rot).gameObject;
	}

	public Transform Spawn(Transform prefab, Vector3 pos, Quaternion rot)
	{
		ObSelfDestructTimer component = prefab.GetComponent<ObSelfDestructTimer>();
		if (component != null)
		{
			component._Pool = this;
		}
		if (prefab == null)
		{
			return null;
		}
		Transform transform;
		foreach (PrefabPool prefabPool2 in _prefabPools)
		{
			if (prefabPool2.prefabGO == null)
			{
				Debug.LogError(" SpawnPool::Spawn PrefabPool.prefabGO is null");
			}
			else if (prefabPool2.prefabGO.name == prefab.name)
			{
				transform = prefabPool2.SpawnInstance(pos, rot);
				if (transform == null)
				{
					return null;
				}
				if (transform.parent != group)
				{
					transform.parent = group;
				}
				_spawned.Add(transform);
				return transform;
			}
		}
		PrefabPool prefabPool = new PrefabPool(prefab);
		CreatePrefabPool(prefabPool);
		transform = prefabPool.SpawnInstance(pos, rot);
		transform.parent = group;
		_spawned.Add(transform);
		return transform;
	}

	public Transform Spawn(Transform prefab)
	{
		return Spawn(prefab, Vector3.zero, Quaternion.identity);
	}

	public ParticleSystem Spawn(ParticleSystem prefab, Vector3 pos, Quaternion quat)
	{
		Transform transform = Spawn(prefab.transform, pos, quat);
		if (transform == null)
		{
			return null;
		}
		ParticleSystem component = transform.GetComponent<ParticleSystem>();
		StartCoroutine(ListenForEmitDespawn(component));
		return component;
	}

	public void Despawn(Transform xform)
	{
		bool flag = false;
		foreach (PrefabPool prefabPool in _prefabPools)
		{
			if (prefabPool._spawned.Contains(xform))
			{
				flag = prefabPool.DespawnInstance(xform);
				break;
			}
			if (prefabPool._despawned.Contains(xform))
			{
				Debug.LogError($"SpawnPool {poolName}: {xform.name} has already been despawned. You cannot despawn something more than once!");
				return;
			}
		}
		if (!flag)
		{
			UtDebug.LogWarning($"SpawnPool {poolName}: {xform.name} not found in SpawnPool");
		}
		else
		{
			_spawned.Remove(xform);
		}
	}

	public void Despawn(Transform instance, float seconds)
	{
		StartCoroutine(DoDespawnAfterSeconds(instance, seconds));
	}

	public void RemoveFromPool(Transform Obj)
	{
		int i = 0;
		for (int count = _prefabPools.Count; i < count; i++)
		{
			_prefabPools[i]._despawned.Remove(Obj);
		}
	}

	private IEnumerator DoDespawnAfterSeconds(Transform instance, float seconds)
	{
		yield return new WaitForSeconds(seconds);
		Despawn(instance);
	}

	public void DespawnAll()
	{
		foreach (Transform item in new List<Transform>(_spawned))
		{
			Despawn(item);
		}
	}

	public bool IsSpawned(Transform instance)
	{
		return _spawned.Contains(instance);
	}

	public Transform GetPrefab(Transform prefab)
	{
		if (prefab == null)
		{
			Debug.LogError(" SpawnPool::GetPrefab incoming prefab is null ");
			return null;
		}
		foreach (PrefabPool prefabPool in _prefabPools)
		{
			if (prefabPool.prefabGO == null)
			{
				Debug.LogError($"SpawnPool {poolName}: PrefabPool.prefabGO is null");
			}
			else if (prefabPool.prefabGO.name == prefab.name)
			{
				return prefabPool.prefab;
			}
		}
		return null;
	}

	public GameObject GetPrefab(GameObject prefab)
	{
		foreach (PrefabPool prefabPool in _prefabPools)
		{
			if (prefabPool.prefabGO == null)
			{
				Debug.LogError($"SpawnPool {poolName}: PrefabPool.prefabGO is null");
			}
			if (prefabPool.prefabGO.name == prefab.name)
			{
				return prefabPool.prefabGO;
			}
		}
		return null;
	}

	private IEnumerator ListenForEmitDespawn(ParticleSystem emitter)
	{
		yield return new WaitForSeconds(emitter.main.startDelay.constant + 0.25f);
		float safetimer = 0f;
		while (emitter.IsAlive(withChildren: true))
		{
			if (!PoolManagerUtils.activeInHierarchy(emitter.gameObject))
			{
				emitter.Clear(withChildren: true);
				yield break;
			}
			safetimer += Time.deltaTime;
			if (safetimer > maxParticleDespawnTime)
			{
				Debug.LogWarning($"SpawnPool {poolName}: Timed out while listening for all particles to die. Waited for {maxParticleDespawnTime}sec.");
			}
			yield return null;
		}
		Despawn(emitter.transform);
	}

	public override string ToString()
	{
		List<string> list = new List<string>();
		foreach (Transform item in _spawned)
		{
			list.Add(item.name);
		}
		return string.Join(", ", list.ToArray());
	}

	public bool Contains(Transform item)
	{
		throw new NotImplementedException("Use IsSpawned(Transform instance) instead.");
	}

	public void CopyTo(Transform[] array, int arrayIndex)
	{
		_spawned.CopyTo(array, arrayIndex);
	}

	public IEnumerator<Transform> GetEnumerator()
	{
		foreach (Transform item in _spawned)
		{
			yield return item;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		foreach (Transform item in _spawned)
		{
			yield return item;
		}
	}

	public int IndexOf(Transform item)
	{
		throw new NotImplementedException();
	}

	public void Insert(int index, Transform item)
	{
		throw new NotImplementedException();
	}

	public void RemoveAt(int index)
	{
		throw new NotImplementedException();
	}

	public void Clear()
	{
		throw new NotImplementedException();
	}

	bool ICollection<Transform>.Remove(Transform item)
	{
		throw new NotImplementedException();
	}
}
