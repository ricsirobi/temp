using System;
using UnityEngine;

public class ObFloat : MonoBehaviour
{
	public Vector3 _Speed;

	public Vector3 _Distance;

	public bool _Active = true;

	public bool _NegateSwitch;

	public bool _Relative;

	private Vector3 mStartPosition;

	public void Start()
	{
		if (_Relative)
		{
			mStartPosition = base.transform.localPosition;
		}
		else
		{
			mStartPosition = base.transform.position;
		}
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
			Vector3 vector = mStartPosition;
			float num = Time.timeSinceLevelLoad * MathF.PI * 2f;
			if (_Speed.x != 0f && _Distance.x != 0f)
			{
				vector.x = mStartPosition.x + (0f - Mathf.Cos(num * _Speed.x)) * _Distance.x;
			}
			if (_Speed.y != 0f && _Distance.y != 0f)
			{
				vector.y = mStartPosition.y + (0f - Mathf.Cos(num * _Speed.y)) * _Distance.y;
			}
			if (_Speed.z != 0f && _Distance.z != 0f)
			{
				vector.z = mStartPosition.z + (0f - Mathf.Cos(num * _Speed.z)) * _Distance.z;
			}
			if (_Relative)
			{
				base.transform.localPosition = vector;
			}
			else
			{
				base.transform.position = vector;
			}
		}
	}
}
