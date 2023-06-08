using UnityEngine;

public class Table : MonoBehaviour
{
	public void PreSplit(Plane[] planes)
	{
		if (base.transform.childCount <= 0)
		{
			return;
		}
		foreach (Transform item in base.transform)
		{
			item.gameObject.AddComponent<Rigidbody>();
		}
		base.transform.DetachChildren();
	}
}
