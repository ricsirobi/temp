using UnityEngine;

public class ObArrowFire : MonoBehaviour
{
	public GameObject _Projectile;

	public GameObject _Marker;

	public GameObject _ShootingPoint;

	public float _ShootInterval = 1f;

	public virtual void OnEnable()
	{
		Invoke("ShootArrow", _ShootInterval);
	}

	private void ShootArrow()
	{
		if (_Projectile != null && _Marker != null && _ShootingPoint != null)
		{
			PrProjectileManager.FireAProjectile(base.gameObject, _ShootingPoint.transform.position, _Projectile, _Marker.transform.position);
			Invoke("ShootArrow", _ShootInterval);
		}
	}
}
