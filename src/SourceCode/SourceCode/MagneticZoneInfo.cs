using UnityEngine;

public class MagneticZoneInfo : KAMonoBase
{
	public float _Radius;

	public float _MagneticPower = 10f;

	public string _DirectionMarker = "PfMarkerNorth";

	private MagneticZoneHandler mMagenticZoneHandlerRef;

	private void Start()
	{
		GameObject gameObject = GameObject.Find(_DirectionMarker);
		if (gameObject != null)
		{
			mMagenticZoneHandlerRef = gameObject.GetComponent<MagneticZoneHandler>();
		}
		if (mMagenticZoneHandlerRef != null)
		{
			mMagenticZoneHandlerRef.AddToZoneList(this);
		}
	}

	private void OnDestroy()
	{
		if (mMagenticZoneHandlerRef != null)
		{
			mMagenticZoneHandlerRef.RemoveFromZoneList(this);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, _Radius);
	}
}
