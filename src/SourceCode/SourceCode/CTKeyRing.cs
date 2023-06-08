using UnityEngine;

public class CTKeyRing : PhysicsObject
{
	private float mFriction;

	private void Start()
	{
	}

	public override void OnTriggerEnter2D(Collider2D other)
	{
		base.OnTriggerEnter2D(other);
	}

	public override void Reset()
	{
		canMove = false;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.gravityScale = 0f;
		base.rigidbody2D.freezeRotation = true;
		StopAllCoroutines();
		base.gameObject.SetActive(value: true);
	}

	public void OnTriggerExit2D(Collider2D other)
	{
	}
}
