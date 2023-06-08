using System.Collections;
using UnityEngine;

public class CTSodaPop : PhysicsObject
{
	public float _UpwardForce = 10.5f;

	public float _MaxRotation;

	public float _SodaDuration = 3f;

	public ParticleSystem _ParticleSystem;

	public float _ThresholdVelocity = 1f;

	private float mCurrentTime;

	private float mDampeningForce;

	private bool mOnCollision;

	private bool mIsPopped;

	private ParticleSystem.EmissionModule mEmissionModule;

	private void Start()
	{
		mEmissionModule = _ParticleSystem.emission;
		UtDebug.Assert(_ParticleSystem != null, " _ParticleSystem  gameObject is null");
		if (_ParticleSystem != null)
		{
			mEmissionModule.enabled = false;
		}
	}

	public void FixedUpdate()
	{
		if (mIsPopped && mCurrentTime > 0f)
		{
			mDampeningForce = _UpwardForce;
			base.rigidbody2D.AddForce(base.transform.up * _UpwardForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
			mCurrentTime -= Time.deltaTime;
			if (_ParticleSystem != null)
			{
				mEmissionModule.enabled = true;
			}
		}
		else if (mIsPopped && mDampeningForce > 8f)
		{
			mDampeningForce -= Time.deltaTime;
			base.rigidbody2D.AddForce(base.transform.up * mDampeningForce * Time.fixedDeltaTime, ForceMode2D.Impulse);
			if (_ParticleSystem != null)
			{
				mEmissionModule.enabled = false;
			}
		}
	}

	public override void OnCollisionEnter2D(Collision2D other)
	{
		if (other.relativeVelocity.magnitude > _ThresholdVelocity)
		{
			mIsPopped = true;
			SetPop(pop: true);
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
		base.rigidbody2D.freezeRotation = false;
		base.rigidbody2D.gravityScale = gravity;
		_ParticleSystem.Play();
		if (_ParticleSystem != null)
		{
			mEmissionModule.enabled = false;
		}
		canMove = true;
		mCurrentTime = _SodaDuration;
	}

	public override void Reset()
	{
		canMove = false;
		mIsPopped = false;
		if (_ParticleSystem != null)
		{
			mEmissionModule.enabled = false;
		}
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.gravityScale = 0f;
		base.rigidbody2D.freezeRotation = true;
		mCurrentTime = _SodaDuration;
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

	public void SetPop(bool pop)
	{
		mIsPopped = pop;
		PlaySound(inForce: true);
	}
}
