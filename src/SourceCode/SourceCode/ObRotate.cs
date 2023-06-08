using UnityEngine;

public class ObRotate : MonoBehaviour
{
	public Vector3 _Speed;

	public bool _Active = true;

	public bool _NegateSwitch;

	private bool mIsVisible = true;

	private void OnBecameVisible()
	{
		mIsVisible = true;
	}

	private void OnBecameInVisible()
	{
		mIsVisible = false;
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
		if (mIsVisible && _Active)
		{
			float deltaTime = Time.deltaTime;
			base.transform.Rotate(_Speed.x * deltaTime, _Speed.y * deltaTime, _Speed.z * deltaTime);
		}
	}
}
