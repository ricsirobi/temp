using UnityEngine;

public class ObProximitySwitch : ObSwitchBase
{
	public bool _Draw;

	public float _Range;

	public Vector3 _Offset;

	public void Update()
	{
		if (_Range == 0f || AvAvatar.pObject == null)
		{
			return;
		}
		if (GetOffset().magnitude <= _Range)
		{
			if (!mSwitchOn)
			{
				SwitchOn();
			}
		}
		else if (mSwitchOn)
		{
			SwitchOff();
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
