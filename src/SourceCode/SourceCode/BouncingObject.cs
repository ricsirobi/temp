using System.Collections.Generic;
using UnityEngine;

public class BouncingObject : MonoBehaviour
{
	public bool _AllowYMovement = true;

	public float _ForceFactor = 1f;

	private bool mCanBounce;

	private AvAvatarController mFollowerAvatar;

	private float mMagnitudeThreshold = 3f;

	private Vector3 mVelocityChange = Vector3.zero;

	private bool mCanAddForce;

	private bool mCurrentPlayer;

	private static List<GameObject> mBouncingObjectList = new List<GameObject>();

	private Rigidbody mRigidbody;

	private void OnEnable()
	{
		mRigidbody = GetComponent<Rigidbody>();
		foreach (GameObject mBouncingObject in mBouncingObjectList)
		{
			Physics.IgnoreCollision(mBouncingObject.GetComponent<Collider>(), base.gameObject.GetComponent<Collider>());
		}
		mBouncingObjectList.Add(base.gameObject);
	}

	private void OnDisable()
	{
		mBouncingObjectList.Remove(base.gameObject);
	}

	private void Update()
	{
		if (mCanBounce && mFollowerAvatar != null)
		{
			mFollowerAvatar.transform.position = Vector3.Lerp(mFollowerAvatar.transform.position, mRigidbody.position, 0.5f);
		}
	}

	private void SetBounceStopThreshold(float inThreshold)
	{
		mMagnitudeThreshold = inThreshold;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject.tag != "Bounceable")
		{
			if (mCurrentPlayer)
			{
				StopBouncing();
			}
		}
		else if (Mathf.Abs(Vector3.Dot(mRigidbody.velocity, collision.contacts[0].normal)) < mMagnitudeThreshold)
		{
			if (mCurrentPlayer)
			{
				StopBouncing();
			}
		}
		else if (mFollowerAvatar != null)
		{
			bool inWithAnimation = Vector3.Dot(collision.contacts[0].normal, Vector3.up) > 0.5f;
			mFollowerAvatar.PlayBounceSound(inWithAnimation);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.relativeVelocity.magnitude < mMagnitudeThreshold && mCurrentPlayer)
		{
			StopBouncing();
		}
	}

	private void UpdateAvatarVelocity(Vector3 inVelocityChange)
	{
		if (mCanBounce)
		{
			mVelocityChange = new Vector3(inVelocityChange.x * _ForceFactor, inVelocityChange.y, inVelocityChange.z * _ForceFactor);
			mCanAddForce = true;
		}
	}

	private void FixedUpdate()
	{
		if (mCanBounce)
		{
			if (mCanAddForce)
			{
				mRigidbody.AddForce(mVelocityChange, ForceMode.VelocityChange);
				mCanAddForce = false;
			}
			if (!_AllowYMovement)
			{
				mRigidbody.velocity = new Vector3(mRigidbody.velocity.x, 0f, mRigidbody.velocity.z);
			}
			if (mFollowerAvatar != null)
			{
				mFollowerAvatar.pVelocity = mRigidbody.velocity;
			}
		}
	}

	private void StartBouncing(AvAvatarController inAvatarController)
	{
		mCanBounce = true;
		mFollowerAvatar = inAvatarController;
		mCurrentPlayer = AvAvatar.IsCurrentPlayer(inAvatarController.gameObject);
		Physics.IgnoreCollision(base.gameObject.GetComponent<Collider>(), inAvatarController.collider);
		mRigidbody.detectCollisions = true;
		mRigidbody.WakeUp();
	}

	private void StopBouncing()
	{
		mCanBounce = false;
		mRigidbody.detectCollisions = false;
		mRigidbody.Sleep();
		if (mFollowerAvatar != null)
		{
			mFollowerAvatar.StopBouncing();
		}
	}
}
