using UnityEngine;

public class ObPingPongRotation : MonoBehaviour
{
	public float _Speed = 1f;

	public Vector3 _MinRotation;

	public Vector3 _MaxRotation;

	public float _MinPauseTime;

	public float _MaxPauseTime;

	private bool mUp = true;

	private float mTimer;

	private float mPauseTimer;

	private void Start()
	{
		mPauseTimer = _MinPauseTime;
	}

	private void Update()
	{
		if (mPauseTimer > 0f)
		{
			mPauseTimer -= Time.deltaTime;
		}
		if (mPauseTimer <= 0f)
		{
			if (mUp)
			{
				mTimer += Time.deltaTime * _Speed;
			}
			else
			{
				mTimer -= Time.deltaTime * _Speed;
			}
			base.transform.localEulerAngles = Vector3.Lerp(_MinRotation, _MaxRotation, mTimer);
			if (mTimer >= 1f)
			{
				mTimer = 1f;
				mPauseTimer = _MaxPauseTime;
				mUp = false;
			}
			else if (mTimer <= 0f)
			{
				mTimer = 0f;
				mPauseTimer = _MinPauseTime;
				mUp = true;
			}
		}
	}
}
