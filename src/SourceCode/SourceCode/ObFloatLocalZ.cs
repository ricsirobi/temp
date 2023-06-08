using System;
using UnityEngine;

public class ObFloatLocalZ : MonoBehaviour
{
	public float _FloatSpeed;

	public float _FloatDistance;

	public bool _Active = true;

	public bool _NegateSwitch;

	private Vector3 mStartPosition;

	private Vector3 mStartLocalZ;

	private void Start()
	{
		mStartPosition = base.transform.position;
		mStartLocalZ = base.transform.forward;
	}

	public void OnStateChange(bool switchOn)
	{
		if (_NegateSwitch)
		{
			_Active = !_Active;
		}
		else
		{
			_Active = switchOn;
		}
	}

	private void Update()
	{
		if (_Active)
		{
			float num = Time.timeSinceLevelLoad * MathF.PI * 2f;
			if (_FloatSpeed != 0f && _FloatDistance != 0f)
			{
				Vector3 vector = mStartLocalZ;
				Vector3 vector2 = (0f - Mathf.Cos(num * _FloatSpeed)) * _FloatDistance * vector;
				base.transform.position = mStartPosition + vector2;
			}
		}
	}
}
