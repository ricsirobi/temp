using UnityEngine;

public class FollowParent : MonoBehaviour
{
	private Transform tr;

	private Transform trParent;

	private Vector3 posDiference;

	private void Start()
	{
		if (!base.transform.parent)
		{
			Debug.LogWarning("Object " + base.gameObject.name + " has attached 'FollowParent' script, but it hasn't parent");
			return;
		}
		tr = base.transform;
		trParent = tr.parent;
		posDiference = trParent.position - tr.position;
	}

	private void Update()
	{
		if ((bool)trParent)
		{
			tr.position = trParent.position - posDiference;
		}
	}
}
