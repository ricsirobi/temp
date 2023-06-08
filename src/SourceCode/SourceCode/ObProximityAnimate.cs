using UnityEngine;

public class ObProximityAnimate : ObAnimate
{
	public bool _Draw;

	public float _Range;

	public Vector3 _Offset;

	protected override bool IsTriggered()
	{
		return GetOffset().magnitude <= _Range;
	}

	public override void Update()
	{
		if (!(base.animation == null) && _Range != 0f && !(AvAvatar.pObject == null) && !(_Animation == null))
		{
			base.Update();
		}
	}

	private void OnDrawGizmos()
	{
		if (_Draw && _Range != 0f)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(base.transform.position + base.transform.TransformDirection(_Offset), _Range);
		}
	}

	protected Vector3 GetOffset()
	{
		return AvAvatar.position - (base.transform.position + base.transform.TransformDirection(_Offset));
	}
}
