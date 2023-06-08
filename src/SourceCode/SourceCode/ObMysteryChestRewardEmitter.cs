using System;
using UnityEngine;

public class ObMysteryChestRewardEmitter : ObBouncyCoinEmitter
{
	[NonSerialized]
	public GameObject _RewardItem;

	protected override GameObject InstantiateCoin()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_Coin);
		GameObject gameObject2 = UnityEngine.Object.Instantiate(_RewardItem);
		gameObject2.transform.parent = gameObject.transform;
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.transform.localRotation = Quaternion.identity;
		Collider[] components = gameObject2.GetComponents<Collider>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] is MeshCollider)
			{
				((MeshCollider)components[i]).convex = true;
			}
			components[i].isTrigger = true;
		}
		SetScale(gameObject2);
		return gameObject;
	}

	private void SetScale(GameObject inObj)
	{
		ObInfo component = inObj.GetComponent<ObInfo>();
		Collider component2 = inObj.GetComponent<Collider>();
		if (component != null)
		{
			inObj.transform.localScale = component._ViewInfo[0]._Scale;
		}
		else if (component2 != null)
		{
			Vector3 size = component2.bounds.size;
			float num = Mathf.Max(size.x, size.y, size.z);
			float num2 = size.x / num;
			float num3 = size.y / num;
			float num4 = size.z / num;
			Vector3 localScale = inObj.transform.localScale;
			localScale.x *= num2;
			localScale.y *= num3;
			localScale.z *= num4;
			inObj.transform.localScale = localScale;
		}
	}
}
