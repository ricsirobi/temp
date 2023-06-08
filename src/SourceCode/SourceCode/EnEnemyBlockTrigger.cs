using System.Collections.Generic;
using UnityEngine;

public class EnEnemyBlockTrigger : MonoBehaviour
{
	private List<Collider> mColliderList = new List<Collider>();

	private void OnTriggerEnter(Collider iCollider)
	{
		if (iCollider.gameObject.CompareTag("Projectile"))
		{
			mColliderList.Add(iCollider);
			base.gameObject.SendMessageUpwards("Block", iCollider.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnTriggerExit(Collider iCollider)
	{
		if (iCollider.gameObject.CompareTag("Projectile"))
		{
			mColliderList.Remove(iCollider);
			if (mColliderList.Count == 0)
			{
				base.gameObject.SendMessageUpwards("UnBlock", null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void Update()
	{
		if (mColliderList.Count == 0)
		{
			return;
		}
		int num = 0;
		while (num < mColliderList.Count)
		{
			if (mColliderList[num] == null)
			{
				mColliderList.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		if (mColliderList.Count == 0)
		{
			base.gameObject.SendMessageUpwards("UnBlock", null, SendMessageOptions.DontRequireReceiver);
		}
	}
}
