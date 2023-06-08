using UnityEngine;

public class ObDraggable : KAMonoBase
{
	public float _Range;

	public GameObject[] _AvatarMarker;

	public float _ForceDragFactor = 4f;

	public float _ForceAngularDragFactor = 3f;

	public AudioClip _DragSFX;

	public bool _DragSmall;

	public float _MinForceMagnitude = 0.02f;

	public float _MaxVelocity = 1f;

	public bool _CanRotate = true;

	private bool mInRange;

	private Rigidbody mRigidBody;

	private AvAvatarController mAvatarController;

	private void Start()
	{
		mRigidBody = GetComponent<Rigidbody>();
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
	}

	public Vector3 Move(Vector3 pushPos, Vector3 forceDir)
	{
		Vector3 result = forceDir;
		if (forceDir.magnitude > _MinForceMagnitude)
		{
			result = forceDir.normalized * forceDir.magnitude * _ForceDragFactor;
			if (mRigidBody.velocity.magnitude < _MaxVelocity)
			{
				mRigidBody.AddForce(result, ForceMode.VelocityChange);
			}
			float num = (UtPlatform.IsMobile() ? 0.04f : 0.1f);
			if (forceDir.magnitude > num)
			{
				SnChannel.Play(_DragSFX);
			}
			return result;
		}
		return result;
	}

	public float RotateAround(Vector3 originPos, Vector3 originAxis, float angle)
	{
		if (!_CanRotate)
		{
			return angle;
		}
		angle /= _ForceAngularDragFactor;
		Quaternion quaternion = Quaternion.AngleAxis(angle, originAxis);
		mRigidBody.MovePosition(quaternion * (base.transform.position - originPos) + originPos);
		mRigidBody.MoveRotation(base.transform.rotation * quaternion);
		return angle;
	}

	private void Update()
	{
		if (!(AvAvatar.pObject != null))
		{
			return;
		}
		if (Vector3.Distance(GetClosestMarker().position, AvAvatar.pObject.transform.position) <= _Range && mAvatarController.OnGround())
		{
			if (!mInRange)
			{
				mInRange = true;
				UiAvatarControls.pInstance.EnableDrag(base.gameObject, enable: true);
			}
		}
		else if (mInRange)
		{
			mInRange = false;
			UiAvatarControls.pInstance.EnableDrag(base.gameObject, enable: false);
		}
	}

	public Transform GetClosestMarker()
	{
		Transform result = null;
		float num = float.PositiveInfinity;
		GameObject[] avatarMarker = _AvatarMarker;
		foreach (GameObject gameObject in avatarMarker)
		{
			float num2 = Vector3.Distance(gameObject.transform.position, AvAvatar.pObject.transform.position);
			if (num2 < num)
			{
				result = gameObject.transform;
				num = num2;
			}
		}
		return result;
	}
}
