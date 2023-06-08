using UnityEngine;

public class AttractableObject : KAMonoBase
{
	public delegate void TargetGameObjectHit(GameObject sourceObject, GameObject targetObject);

	public float _AttractiveObjRadius = 0.5f;

	public float _AttractiveObjStrength = 5f;

	public GameObject _AttractiveGameObj;

	public TargetGameObjectHit _OnAttractiveObjHit;

	private Transform mAttractiveGameObjTransform;

	private bool mIsInAttractiveZone;

	private float mOrgDistance;

	private void OnEnable()
	{
		mAttractiveGameObjTransform = _AttractiveGameObj.transform;
	}

	private void Update()
	{
		if (Vector3.Distance(mAttractiveGameObjTransform.position, base.transform.position) < _AttractiveObjRadius)
		{
			if (!mIsInAttractiveZone)
			{
				InitEffect();
				mOrgDistance = Vector3.Distance(mAttractiveGameObjTransform.position, base.transform.position);
			}
		}
		else
		{
			mIsInAttractiveZone = false;
		}
	}

	private void InitEffect()
	{
		Rigidbody obj = base.rigidbody;
		obj.constraints = (RigidbodyConstraints)114;
		obj.useGravity = false;
		obj.isKinematic = false;
		mIsInAttractiveZone = true;
	}

	private void FixedUpdate()
	{
		if (mIsInAttractiveZone)
		{
			Vector3 vector = mAttractiveGameObjTransform.position - base.transform.position;
			float num = Vector3.Distance(mAttractiveGameObjTransform.position, base.transform.position);
			float num2 = mOrgDistance / num * _AttractiveObjStrength;
			Vector3 force = vector * num2;
			base.rigidbody.AddForce(force, ForceMode.Force);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (_OnAttractiveObjHit != null)
		{
			_OnAttractiveObjHit(base.gameObject, collision.gameObject);
		}
	}
}
