using System.Collections;
using UnityEngine;

public class Balloon : PhysicsObject
{
	public float upwardForce = 2f;

	public float maxTorque = 0.5f;

	public float maxRotation;

	public Sprite[] popAnimations;

	private float zRot;

	private float rotatingForce;

	private bool isPopped;

	private bool onCollision;

	public ParticleSystem explodeBalloon;

	public void FixedUpdate()
	{
		if (canMove)
		{
			base.rigidbody2D.AddForce(Vector2.up * upwardForce);
			zRot = base.transform.eulerAngles.z;
			if (zRot > 180f)
			{
				zRot = 0f - (360f - zRot);
			}
			rotatingForce = maxTorque * ((0f - zRot) / maxRotation);
			base.rigidbody2D.AddTorque(rotatingForce);
		}
	}

	public override void OnCollisionEnter2D(Collision2D other)
	{
		if ((other.collider.gameObject.name == "HeadCollider" || other.collider.gameObject.name == "FlameCollider") && !isPopped)
		{
			isPopped = true;
			StartCoroutine("Explode");
			GoalManager.pInstance.BalloonExloded(base.gameObject);
		}
	}

	private void OnCollisionStay2D(Collision2D other)
	{
		onCollision = true;
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		onCollision = false;
		StartCoroutine("ReduceHorizontalSpeed", 0.5f);
	}

	public override void Enable()
	{
		base.rigidbody2D.freezeRotation = false;
		canMove = true;
	}

	public override void Reset()
	{
		if (isPopped)
		{
			isPopped = false;
			GetComponent<PolygonCollider2D>().enabled = true;
			GetComponent<CircleCollider2D>().enabled = true;
			MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = true;
			}
		}
		canMove = false;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.freezeRotation = true;
		StopAllCoroutines();
	}

	private IEnumerator ReduceHorizontalSpeed(float time)
	{
		float startValue = base.rigidbody2D.velocity.x;
		float rate = 1f / time;
		float t = 0f;
		_ = base.rigidbody2D.velocity;
		while (t < 1f && !onCollision)
		{
			t += Time.deltaTime * rate;
			Vector2 velocity = base.rigidbody2D.velocity;
			velocity.x = Mathf.Lerp(startValue, 0f, t);
			base.rigidbody2D.velocity = velocity;
			yield return new WaitForEndOfFrame();
		}
	}

	private IEnumerator Explode()
	{
		explodeBalloon.Play();
		PlaySound(inForce: true);
		yield return new WaitForSeconds(0.05f);
		MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
		GetComponent<PolygonCollider2D>().enabled = false;
		GetComponent<CircleCollider2D>().enabled = false;
	}
}
