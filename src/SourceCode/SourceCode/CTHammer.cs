using UnityEngine;

public class CTHammer : PhysicsObject
{
	public GameObject HammerBase;

	private Transform mLastHammerTransform;

	private Transform mLastHammerBaseTransform;

	private Vector3 mBasePosition;

	private Vector3 mBaseAngle;

	private void Start()
	{
		GetComponent<BoxCollider2D>().enabled = true;
		if (HammerBase != null)
		{
			HammerBase.GetComponent<Rigidbody2D>().isKinematic = true;
			mLastHammerTransform = base.transform;
			mBasePosition = HammerBase.transform.localPosition;
			mBaseAngle = HammerBase.transform.localEulerAngles;
			BoxCollider2D[] components = HammerBase.GetComponents<BoxCollider2D>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].enabled = false;
			}
		}
	}

	public override void Enable()
	{
		GetComponent<BoxCollider2D>().enabled = false;
		if (HammerBase != null)
		{
			HammerBase.GetComponent<Rigidbody2D>().isKinematic = false;
			HammerBase.GetComponent<Rigidbody2D>().gravityScale = gravity;
			BoxCollider2D[] components = HammerBase.GetComponents<BoxCollider2D>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].enabled = true;
			}
		}
	}

	public override void Reset()
	{
		if (HammerBase != null)
		{
			base.transform.position = mLastHammerTransform.position;
			base.transform.eulerAngles = mLastHammerTransform.eulerAngles;
			HammerBase.transform.localPosition = mBasePosition;
			HammerBase.transform.localEulerAngles = mBaseAngle;
			HammerBase.GetComponent<Rigidbody2D>().isKinematic = true;
			HammerBase.GetComponent<Rigidbody2D>().gravityScale = 0f;
			HammerBase.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			HammerBase.GetComponent<Rigidbody2D>().angularVelocity = 0f;
			GetComponent<BoxCollider2D>().enabled = true;
			BoxCollider2D[] components = HammerBase.GetComponents<BoxCollider2D>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].enabled = false;
			}
		}
		base.gameObject.SetActive(value: true);
	}
}
