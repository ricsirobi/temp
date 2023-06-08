using UnityEngine;

public class CTBabyAnimalNet : PhysicsObject
{
	private Transform mLastBabyNetTransform;

	private GameObject mRopeHolder;

	private GameObject mNet;

	private void Start()
	{
		mLastBabyNetTransform = base.transform;
		base.rigidbody2D.isKinematic = true;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		mRopeHolder = base.transform.GetChild(0).gameObject;
		mNet = base.transform.GetChild(1).gameObject;
	}

	public override void Enable()
	{
		canMove = true;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		mRopeHolder.GetComponent<CTRopeHolder>().Enable();
		mNet.GetComponent<Rigidbody2D>().isKinematic = false;
		mNet.GetComponent<Rigidbody2D>().freezeRotation = false;
		mNet.GetComponent<CTWoodenCrate>().Enable();
	}

	public override void Reset()
	{
		canMove = false;
		base.transform.position = mLastBabyNetTransform.position;
		base.transform.eulerAngles = mLastBabyNetTransform.eulerAngles;
		base.rigidbody2D.isKinematic = true;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.gravityScale = 0f;
		base.rigidbody2D.freezeRotation = true;
		base.gameObject.SetActive(value: true);
		mRopeHolder.GetComponent<Rigidbody2D>().isKinematic = true;
		mRopeHolder.GetComponent<Rigidbody2D>().freezeRotation = true;
		mRopeHolder.GetComponent<CTRopeHolder>().Reset();
		mNet.GetComponent<Rigidbody2D>().isKinematic = true;
		mNet.GetComponent<Rigidbody2D>().freezeRotation = true;
		mNet.GetComponent<CTWoodenCrate>().Reset();
	}
}
