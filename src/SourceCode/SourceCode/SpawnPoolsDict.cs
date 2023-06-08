using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoolsDict : IDictionary<string, SpawnPool>, ICollection<KeyValuePair<string, SpawnPool>>, IEnumerable<KeyValuePair<string, SpawnPool>>, IEnumerable
{
	private Dictionary<string, SpawnPool> _pools = new Dictionary<string, SpawnPool>();

	public int Count => _pools.Count;

	public SpawnPool this[string key]
	{
		get
		{
			try
			{
				return _pools[key];
			}
			catch (KeyNotFoundException)
			{
				throw new KeyNotFoundException($"A Pool with the name '{key}' not found. \nPools={ToString()}");
			}
		}
		set
		{
			throw new NotImplementedException("Cannot set PoolManager.Pools[key] directly. SpawnPools add themselves to PoolManager.Pools when created, so there is no need to set them explicitly. Create pools using PoolManager.Pools.Create() or add a SpawnPool component to a GameObject.");
		}
	}

	public ICollection<string> Keys
	{
		get
		{
			throw new NotImplementedException("If you need this, please request it.");
		}
	}

	public ICollection<SpawnPool> Values
	{
		get
		{
			throw new NotImplementedException("If you need this, please request it.");
		}
	}

	private bool IsReadOnly => true;

	bool ICollection<KeyValuePair<string, SpawnPool>>.IsReadOnly => true;

	public SpawnPool Create(string poolName)
	{
		return new GameObject(poolName + "Pool").AddComponent<SpawnPool>();
	}

	public SpawnPool Create(string poolName, GameObject owner)
	{
		if (!assertValidPoolName(poolName))
		{
			return null;
		}
		string name = owner.gameObject.name;
		try
		{
			owner.gameObject.name = poolName;
			return owner.AddComponent<SpawnPool>();
		}
		finally
		{
			owner.gameObject.name = name;
		}
	}

	private bool assertValidPoolName(string poolName)
	{
		string text = poolName.Replace("Pool", "");
		if (text != poolName)
		{
			Debug.LogWarning($"'{poolName}' has the word 'Pool' in it. This word is reserved for GameObject defaul naming. The pool name has been changed to '{text}'");
			poolName = text;
		}
		if (ContainsKey(poolName))
		{
			Debug.Log($"A pool with the name '{poolName}' already exists");
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		string[] array = new string[_pools.Count];
		_pools.Keys.CopyTo(array, 0);
		return string.Format("[{0}]", string.Join(", ", array));
	}

	public bool Destroy(string poolName)
	{
		if (!_pools.TryGetValue(poolName, out var value))
		{
			Debug.LogError($"PoolManager: Unable to destroy '{poolName}'. Not in PoolManager");
			return false;
		}
		UnityEngine.Object.Destroy(value.gameObject);
		return true;
	}

	public void DestroyAll()
	{
		foreach (KeyValuePair<string, SpawnPool> pool in _pools)
		{
			UnityEngine.Object.Destroy(pool.Value);
		}
	}

	internal void Add(SpawnPool spawnPool)
	{
		if (ContainsKey(spawnPool.poolName))
		{
			Debug.LogError($"A pool with the name '{spawnPool.poolName}' already exists. This should only happen if a SpawnPool with this name is added to a scene twice.");
		}
		else
		{
			_pools.Add(spawnPool.poolName, spawnPool);
		}
	}

	public void Add(string key, SpawnPool value)
	{
		throw new NotImplementedException("SpawnPools add themselves to PoolManager.Pools when created, so there is no need to Add() them explicitly. Create pools using PoolManager.Pools.Create() or add a SpawnPool component to a GameObject.");
	}

	internal bool Remove(SpawnPool spawnPool)
	{
		if (!ContainsKey(spawnPool.poolName))
		{
			Debug.LogError($"PoolManager: Unable to remove '{spawnPool.poolName}'. Pool not in PoolManager");
			return false;
		}
		_pools.Remove(spawnPool.poolName);
		return true;
	}

	public bool Remove(string poolName)
	{
		throw new NotImplementedException("SpawnPools can only be destroyed, not removed and kept alive outside of PoolManager. There are only 2 legal ways to destroy a SpawnPool: Destroy the GameObject directly, if you have a reference, or use PoolManager.Destroy(string poolName).");
	}

	public bool ContainsKey(string poolName)
	{
		return _pools.ContainsKey(poolName);
	}

	public bool TryGetValue(string poolName, out SpawnPool spawnPool)
	{
		return _pools.TryGetValue(poolName, out spawnPool);
	}

	public bool Contains(KeyValuePair<string, SpawnPool> item)
	{
		throw new NotImplementedException("Use PoolManager.Pools.Contains(string poolName) instead.");
	}

	public void Add(KeyValuePair<string, SpawnPool> item)
	{
		throw new NotImplementedException("SpawnPools add themselves to PoolManager.Pools when created, so there is no need to Add() them explicitly. Create pools using PoolManager.Pools.Create() or add a SpawnPool component to a GameObject.");
	}

	public void Clear()
	{
		throw new NotImplementedException("Use PoolManager.Pools.DestroyAll() instead.");
	}

	private void CopyTo(KeyValuePair<string, SpawnPool>[] array, int arrayIndex)
	{
		throw new NotImplementedException("PoolManager.Pools cannot be copied");
	}

	void ICollection<KeyValuePair<string, SpawnPool>>.CopyTo(KeyValuePair<string, SpawnPool>[] array, int arrayIndex)
	{
		throw new NotImplementedException("PoolManager.Pools cannot be copied");
	}

	public bool Remove(KeyValuePair<string, SpawnPool> item)
	{
		throw new NotImplementedException("SpawnPools can only be destroyed, not removed and kept alive outside of PoolManager. There are only 2 legal ways to destroy a SpawnPool: Destroy the GameObject directly, if you have a reference, or use PoolManager.Destroy(string poolName).");
	}

	public IEnumerator<KeyValuePair<string, SpawnPool>> GetEnumerator()
	{
		return _pools.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _pools.GetEnumerator();
	}
}
