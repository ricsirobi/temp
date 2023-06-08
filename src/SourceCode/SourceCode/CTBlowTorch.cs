using UnityEngine;

public class CTBlowTorch : PhysicsObject
{
	public enum State
	{
		None,
		Ready,
		Animate,
		Blow,
		Done
	}

	public ParticleSystem _Particle;

	public BoxCollider2D _FlameCollider;

	public Animation _Animation;

	public float _BlowStartDelay = 1f;

	public string _BlowAnim;

	private string mDefaultAnim;

	private float mTimer;

	private State mCurrentState;

	private ParticleSystem.EmissionModule mEmissionModule;

	public bool pIsOn => mCurrentState == State.Blow;

	private void Start()
	{
		mEmissionModule = _Particle.emission;
		mEmissionModule.enabled = false;
		_Particle.Stop();
		_FlameCollider.enabled = false;
		if (_Animation != null)
		{
			mDefaultAnim = _Animation.clip.name;
		}
	}

	private void Update()
	{
		mTimer += Time.deltaTime;
		if (mCurrentState == State.Animate)
		{
			if (mTimer >= _BlowStartDelay && !mEmissionModule.enabled)
			{
				EnableParticle();
			}
		}
		else if (mCurrentState == State.Blow)
		{
			ParticleSystem.MainModule main = _Particle.main;
			if (!main.loop && mTimer >= main.duration)
			{
				FireStop();
			}
		}
	}

	private void FireStart()
	{
		mCurrentState = State.Animate;
		mTimer = 0f;
		if (!string.IsNullOrEmpty(_BlowAnim) && _Animation != null)
		{
			_Animation[_BlowAnim].wrapMode = (_Particle.main.loop ? WrapMode.Loop : WrapMode.ClampForever);
			_Animation.CrossFade(_BlowAnim);
		}
	}

	private void EnableParticle()
	{
		mTimer = 0f;
		mCurrentState = State.Blow;
		mEmissionModule.enabled = true;
		_Particle.Play();
		_FlameCollider.enabled = true;
		PlaySound(inForce: true);
	}

	private void FireStop()
	{
		mCurrentState = State.Done;
		if (!string.IsNullOrEmpty(mDefaultAnim) && _Animation != null)
		{
			_Animation[mDefaultAnim].wrapMode = WrapMode.Loop;
			_Animation.CrossFade(mDefaultAnim);
		}
		_Particle.Stop();
		mEmissionModule.enabled = false;
	}

	public override void OnCollisionEnter2D(Collision2D other)
	{
		if (mCurrentState == State.Ready && !other.gameObject.name.Contains("Pallet"))
		{
			FireStart();
		}
		if ((bool)GetParent(other.transform))
		{
			GoalManager.pInstance.CollisionEvent(base.gameObject, GetParent(other.transform).gameObject);
		}
		if (other.collider.gameObject.name == "TrampolineTop")
		{
			base.rigidbody2D.AddForce(Vector2.up * base.rigidbody2D.velocity.y * springBounciness);
		}
		if (Mathf.Abs(base.rigidbody2D.velocity.y) < 1.25f)
		{
			base.rigidbody2D.velocity = new Vector2(base.rigidbody2D.velocity.x, 0f);
		}
	}

	public override void Enable()
	{
		base.rigidbody2D.gravityScale = gravity;
		base.rigidbody2D.freezeRotation = false;
		mCurrentState = State.Ready;
	}

	public override void Reset()
	{
		FireStop();
		mCurrentState = State.None;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.freezeRotation = true;
		base.rigidbody2D.gravityScale = 0f;
		_FlameCollider.enabled = false;
	}
}
