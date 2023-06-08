using System.Collections;
using UnityEngine;

public class CTRemoteRocket : PhysicsObject
{
	public float _MaxForce = 80f;

	public float _Acceleration = 0.25f;

	public ParticleSystem _ParticleSystem;

	private bool mActivated;

	private bool mOnCollision;

	private float mCurrForce;

	private ParticleSystem.EmissionModule mEmissionModule;

	public void Awake()
	{
		mEmissionModule = _ParticleSystem.emission;
		_ParticleSystem.Play();
		mEmissionModule.enabled = false;
	}

	public void FixedUpdate()
	{
		if (mActivated)
		{
			if (mCurrForce < _MaxForce)
			{
				mCurrForce += _Acceleration;
			}
			base.rigidbody2D.AddForce(base.transform.up * mCurrForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
			mEmissionModule.enabled = true;
		}
	}

	private void OnCollisionStay2D(Collision2D other)
	{
		mOnCollision = true;
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		mOnCollision = false;
		StartCoroutine("ReduceHorizontalSpeed", 0.5f);
	}

	public override void Enable()
	{
		base.rigidbody2D.gravityScale = gravity;
		canMove = true;
	}

	public override void Reset()
	{
		canMove = false;
		mActivated = false;
		mEmissionModule.enabled = false;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.gravityScale = 0f;
		base.rigidbody2D.freezeRotation = true;
		mCurrForce = 0f;
		StopAllCoroutines();
	}

	private IEnumerator ReduceHorizontalSpeed(float time)
	{
		float startValue = base.rigidbody2D.velocity.x;
		float rate = 1f / time;
		float t = 0f;
		_ = base.rigidbody2D.velocity;
		while (t < 1f && !mOnCollision)
		{
			t += Time.deltaTime * rate;
			Vector2 velocity = base.rigidbody2D.velocity;
			velocity.x = Mathf.Lerp(startValue, 0f, t);
			base.rigidbody2D.velocity = velocity;
			yield return new WaitForEndOfFrame();
		}
	}

	public void SetAcceloration()
	{
		mActivated = true;
		PlaySound(inForce: true);
	}
}
