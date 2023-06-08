using System.Collections.Generic;
using UnityEngine;

public class ObRespawnDragons : ObRespawn
{
	private List<GameObject> mClonedObjects = new List<GameObject>();

	protected override void Awake()
	{
		if (_Objects != null)
		{
			for (int i = 0; i < _Objects.Length; i++)
			{
				if (_Objects[i]._Object != null)
				{
					_Objects[i]._Object.SetActive(value: false);
				}
			}
		}
		Respawn();
	}

	private void Respawn()
	{
		for (int i = 0; i < _Objects.Length; i++)
		{
			GameObject @object = _Objects[i]._Object;
			if (@object != null)
			{
				GameObject gameObject = Object.Instantiate(@object, @object.transform.position, @object.transform.rotation);
				gameObject.transform.parent = @object.transform.parent;
				gameObject.SetActive(value: true);
				mClonedObjects.Add(gameObject);
			}
			else
			{
				Debug.LogError("Object can not be instantiated!!! Null reference.");
			}
		}
	}

	private void OnDisable()
	{
		if (mClonedObjects != null)
		{
			foreach (GameObject mClonedObject in mClonedObjects)
			{
				Object.Destroy(mClonedObject);
			}
		}
		mClonedObjects.Clear();
	}
}
