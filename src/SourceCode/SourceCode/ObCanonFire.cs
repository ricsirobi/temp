using UnityEngine;

public class ObCanonFire : MonoBehaviour
{
	public GameObject _Projectile;

	public GameObject _Marker;

	private void OnTriggerEnter(Collider other)
	{
		if (_Projectile != null && _Marker != null)
		{
			PrProjectileManager.FireAProjectile(base.gameObject, _Marker.transform.position, _Projectile);
		}
	}
}
