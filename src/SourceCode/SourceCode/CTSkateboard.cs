using UnityEngine;

public class CTSkateboard : PhysicsObject
{
	public float _WheelInertia;

	public GameObject _Body;

	public HingeJoint2D[] _Wheels;

	[Tooltip("If the sqrMagnitude of the velocity of either wheel is greather than or equal to this the audioSource will be played.")]
	public float _VelocityForSound;

	private Rigidbody2D mMainRigidbody;

	private Rigidbody2D mFrontWheelRigidbody;

	private Rigidbody2D mBackWheelRigidbody;

	private Vector2 mFrontWheelPos;

	private Vector2 mBackWheelPos;

	private Transform mLastTransform;

	private Vector3 mLastBodyPos;

	private Vector3 mLastBodyRot;

	private void Start()
	{
		UtDebug.Assert(_Body != null, "_Body is not assigned");
		mMainRigidbody = _Body.GetComponent<Rigidbody2D>();
		UtDebug.Assert(mMainRigidbody != null, "mMainRigidbody is null ");
		base.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
		base.gameObject.GetComponent<BoxCollider2D>().enabled = true;
		mLastTransform = base.transform;
		mLastBodyPos = _Body.transform.localPosition;
		mLastBodyRot = _Body.transform.localEulerAngles;
		mFrontWheelPos = _Wheels[0].transform.localPosition;
		mBackWheelPos = _Wheels[1].transform.localPosition;
		mMainRigidbody.inertia = inertia;
		mFrontWheelRigidbody = _Wheels[0].GetComponent<Rigidbody2D>();
		mFrontWheelRigidbody.inertia = _WheelInertia;
		mBackWheelRigidbody = _Wheels[1].GetComponent<Rigidbody2D>();
		mBackWheelRigidbody.inertia = _WheelInertia;
	}

	protected void Update()
	{
		if (!canMove)
		{
			return;
		}
		if (mFrontWheelRigidbody.velocity.sqrMagnitude >= _VelocityForSound || mBackWheelRigidbody.velocity.sqrMagnitude >= _VelocityForSound)
		{
			if (base.audio != null && !base.audio.isPlaying)
			{
				base.audio.loop = true;
				base.audio.Play();
			}
		}
		else if (base.audio != null && base.audio.isPlaying)
		{
			base.audio.Pause();
		}
	}

	public void FixedUpdate()
	{
	}

	public override void Enable()
	{
		canMove = true;
		base.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
		base.gameObject.GetComponent<BoxCollider2D>().enabled = false;
		_Wheels[0].GetComponent<Rigidbody2D>().freezeRotation = false;
		_Wheels[1].GetComponent<Rigidbody2D>().freezeRotation = false;
		mMainRigidbody.gravityScale = gravity;
		mMainRigidbody.freezeRotation = false;
		mMainRigidbody.isKinematic = false;
	}

	public override void Reset()
	{
		canMove = false;
		base.transform.position = mLastTransform.position;
		base.transform.eulerAngles = mLastTransform.eulerAngles;
		base.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
		base.gameObject.GetComponent<BoxCollider2D>().enabled = true;
		_Body.transform.localPosition = mLastBodyPos;
		_Body.transform.localEulerAngles = mLastBodyRot;
		mMainRigidbody.freezeRotation = true;
		mMainRigidbody.gravityScale = 0f;
		mMainRigidbody.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		mMainRigidbody.isKinematic = true;
		_Wheels[0].transform.localPosition = mFrontWheelPos;
		_Wheels[1].transform.localPosition = mBackWheelPos;
		_Wheels[0].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		_Wheels[1].GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		_Wheels[0].GetComponent<Rigidbody2D>().freezeRotation = true;
		_Wheels[1].GetComponent<Rigidbody2D>().freezeRotation = true;
	}
}
