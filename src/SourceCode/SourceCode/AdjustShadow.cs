using System;
using UnityEngine;

public class AdjustShadow : MonoBehaviour
{
	[NonSerialized]
	public Vector3 initPosition;

	[NonSerialized]
	public Quaternion initRotation;

	[NonSerialized]
	public bool adjustRotation;

	[NonSerialized]
	public Vector3 normal;

	[NonSerialized]
	public Vector3 parentInitRotation;

	private bool visible = true;

	public float shadowScaleFactor;

	private Vector3 initScale;

	private void Start()
	{
		initScale = base.transform.localScale;
	}

	private void LateUpdate()
	{
		if (visible)
		{
			base.transform.position = initPosition;
			float num = 1f - Mathf.Clamp(Vector3.Distance(base.transform.parent.position, base.transform.position), 0f, 1f) * shadowScaleFactor;
			base.transform.localScale = new Vector3(initScale.x * num, initScale.y, initScale.z * num);
			if (adjustRotation)
			{
				base.transform.rotation = initRotation;
				base.transform.RotateAround(base.transform.position, normal, base.transform.parent.eulerAngles.y - parentInitRotation.y);
			}
			else
			{
				base.transform.rotation = initRotation;
			}
		}
	}

	private void OnBecameVisible()
	{
		visible = true;
	}

	private void OnBecameInvisible()
	{
		visible = false;
	}
}
