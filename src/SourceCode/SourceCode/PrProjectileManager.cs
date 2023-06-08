using System.Collections.Generic;
using UnityEngine;

public class PrProjectileManager
{
	private static List<GameObject> mProjectiles = new List<GameObject>();

	public static GameObject FireAProjectile(GameObject iSource, Vector3 iStartPoint, GameObject iProjectile)
	{
		Vector3 iTargetPoint = iStartPoint + iSource.transform.forward;
		return FireAProjectile(iSource, iStartPoint, iProjectile, iTargetPoint);
	}

	public static GameObject FireAProjectile(GameObject iSource, Vector3 iStartPoint, GameObject iProjectile, Vector3 iTargetPoint)
	{
		if (!iSource || !iProjectile)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(iProjectile, iStartPoint, Quaternion.identity);
		gameObject.name = iProjectile.name;
		PrProjectile prProjectile = gameObject.GetComponent(typeof(PrProjectile)) as PrProjectile;
		if ((bool)prProjectile)
		{
			prProjectile.pTargetPosition = iTargetPoint;
			prProjectile.pProjectileSource = iSource;
		}
		Physics.IgnoreCollision(iSource.GetComponent<Collider>(), gameObject.GetComponent<Collider>());
		mProjectiles.Add(gameObject);
		return gameObject;
	}

	public static void DestroyProjectile(GameObject iObject)
	{
		if ((bool)iObject)
		{
			mProjectiles.Remove(iObject);
		}
	}

	public static void PauseProjectile(bool pause)
	{
		foreach (GameObject mProjectile in mProjectiles)
		{
			mProjectile.SendMessage("PauseProjectile", pause, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static void DestroyAll()
	{
		foreach (GameObject item in new List<GameObject>(mProjectiles))
		{
			item.SendMessage("DestroyMe", null, SendMessageOptions.DontRequireReceiver);
		}
	}
}
