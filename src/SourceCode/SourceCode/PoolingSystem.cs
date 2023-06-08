using System.Collections.Generic;
using UnityEngine;

public class PoolingSystem
{
	private List<GameObject> pooledObjects = new List<GameObject>();

	private GameObject original;

	private string objectName = "ObjectName";

	private Transform objectParent;

	public PoolingSystem(GameObject obj, int initialSize, string name, Transform parent)
	{
		original = obj;
		objectName = name;
		objectParent = parent;
		GameObject gameObject = null;
		for (int i = 0; i < initialSize; i++)
		{
			gameObject = Object.Instantiate(original, Vector3.zero, Quaternion.identity);
			gameObject.name = name;
			gameObject.transform.parent = objectParent;
			gameObject.SetActive(value: false);
			pooledObjects.Add(gameObject);
		}
	}

	public void Remove(GameObject obj)
	{
		obj.transform.parent = objectParent;
		obj.name = objectName;
		obj.SetActive(value: false);
	}

	public void RemoveFromList(GameObject obj)
	{
		pooledObjects.Remove(obj);
	}

	public GameObject Create()
	{
		for (int i = 0; i < pooledObjects.Count; i++)
		{
			if (!pooledObjects[i].activeInHierarchy)
			{
				pooledObjects[i].SetActive(value: true);
				return pooledObjects[i];
			}
		}
		GameObject gameObject = Object.Instantiate(original, Vector3.zero, Quaternion.identity);
		pooledObjects.Add(gameObject);
		gameObject.name = objectName;
		gameObject.transform.parent = objectParent;
		return gameObject;
	}
}
