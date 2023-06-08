using UnityEngine;

public class CTWoodenCrate : PhysicsObject
{
	public string _AssetName;

	public float _BreakThresholdVelocity;

	private bool mGrounded;

	private bool mActivated;

	private GameObject mStuffedAnimal;

	public override void OnCollisionEnter2D(Collision2D other)
	{
		if (!mGrounded && mActivated)
		{
			BreakOpen();
		}
	}

	private void LateUpdate()
	{
		if (mActivated)
		{
			if (base.gameObject.GetComponent<Rigidbody2D>().velocity.y >= _BreakThresholdVelocity)
			{
				mGrounded = true;
			}
			else
			{
				mGrounded = false;
			}
		}
	}

	public override void Enable()
	{
		mActivated = true;
		base.rigidbody2D.gravityScale = gravity;
		base.rigidbody2D.freezeRotation = false;
	}

	public override void Reset()
	{
		base.gameObject.SetActive(value: true);
		mGrounded = false;
		mActivated = false;
		base.rigidbody2D.gravityScale = 0f;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.freezeRotation = true;
		Object.Destroy(mStuffedAnimal);
		mStuffedAnimal = null;
	}

	private void BreakOpen()
	{
		RsResourceManager.LoadAssetFromBundle("RS_DATA/PfStuffedAnimal", _AssetName, OnLoaded, typeof(GameObject));
		base.gameObject.SetActive(value: false);
	}

	private void OnLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inObject != null && inEvent == RsResourceLoadEvent.COMPLETE)
		{
			mStuffedAnimal = Object.Instantiate((GameObject)inObject, base.transform.position, Quaternion.identity);
			mStuffedAnimal.transform.parent = base.transform.parent;
			mStuffedAnimal.GetComponent<PhysicsObject>().Enable();
		}
	}
}
