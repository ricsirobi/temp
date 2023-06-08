using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObPushAvatar : MonoBehaviour
{
	public Vector3 _SpeedVec = Vector3.forward;

	public bool _UseOnlyAvatar = true;

	public Space _Space;

	public bool _DisableAvatarInput = true;

	public float _Timer;

	private Transform mTarget;

	private bool mPrvInputState;

	private float mTimer;

	private void Update()
	{
		if (mTarget != null)
		{
			if (_Space == Space.Self)
			{
				mTarget.Translate(_SpeedVec * Time.deltaTime, _Space);
			}
			else
			{
				mTarget.Translate(base.transform.TransformDirection(_SpeedVec) * Time.deltaTime, _Space);
			}
		}
		if (mTimer > 0f)
		{
			mTimer -= Time.deltaTime;
			if (mTimer <= 0f)
			{
				mTarget = null;
			}
		}
	}

	private void OnTriggerEnter(Collider inCollider)
	{
		if (mTarget != null)
		{
			if (mTarget == inCollider.transform)
			{
				mTimer = _Timer;
			}
			return;
		}
		if (AvAvatar.pObject == null)
		{
			Debug.LogError(" @@ Avatar object is not created !!!");
		}
		if (inCollider.gameObject == AvAvatar.pObject)
		{
			mTarget = AvAvatar.mTransform;
			if (_DisableAvatarInput)
			{
				mPrvInputState = AvAvatar.pInputEnabled;
				AvAvatar.pInputEnabled = false;
			}
			float num = _SpeedVec.magnitude * 0.5f;
			mTarget.gameObject.SendMessage("SetPushSpeed", num, SendMessageOptions.DontRequireReceiver);
		}
		else if (!_UseOnlyAvatar)
		{
			mTarget = inCollider.gameObject.transform;
		}
		mTimer = _Timer;
	}

	private void OnTriggerExit(Collider inCollider)
	{
		if (!(mTarget != inCollider.transform))
		{
			if (mTarget != null && mTarget.gameObject == AvAvatar.pObject && _DisableAvatarInput)
			{
				AvAvatar.pInputEnabled = mPrvInputState;
			}
			if (mTimer <= 0f)
			{
				mTarget = null;
			}
		}
	}

	private void OnDisable()
	{
		mTarget = null;
	}
}
