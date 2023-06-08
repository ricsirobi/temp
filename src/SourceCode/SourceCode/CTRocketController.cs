using UnityEngine;

public class CTRocketController : PhysicsObject
{
	public Transform _Body;

	public ParticleSystem _Particle;

	private bool mInPlayMode;

	private Vector2 mBodyStartPos;

	private CTRemoteRocket[] mRocketList;

	private RcCar[] mCarList;

	public float _carAcceleration = 50f;

	private ParticleSystem.EmissionModule mEmissionModule;

	private void Awake()
	{
		mBodyStartPos = _Body.transform.localPosition;
		mRocketList = Object.FindObjectsOfType<CTRemoteRocket>();
		mCarList = Object.FindObjectsOfType<RcCar>();
		mEmissionModule = _Particle.emission;
	}

	public void FixedUpdate()
	{
		_ = mInPlayMode;
	}

	public override void Enable()
	{
		mInPlayMode = true;
		_Body.GetComponent<Rigidbody2D>().gravityScale = gravity;
		base.rigidbody2D.gravityScale = gravity;
		mRocketList = Object.FindObjectsOfType<CTRemoteRocket>();
		mCarList = Object.FindObjectsOfType<RcCar>();
	}

	public override void Reset()
	{
		mInPlayMode = false;
		base.rigidbody2D.gravityScale = 0f;
		mEmissionModule.enabled = false;
		_Body.GetComponent<Rigidbody2D>().gravityScale = 0f;
		_Body.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		_Body.localPosition = mBodyStartPos;
	}

	public void SendSignal()
	{
		if (mInPlayMode)
		{
			mEmissionModule.enabled = true;
			for (int i = 0; i < mRocketList.Length; i++)
			{
				mRocketList[i].SetAcceloration();
			}
			for (int j = 0; j < mCarList.Length; j++)
			{
				mCarList[j].SetAcceloration(_carAcceleration);
			}
		}
	}
}
