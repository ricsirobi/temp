using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTDynamite : PhysicsObject
{
	private bool mIsBlast;

	private bool mReady;

	private List<Rigidbody2D> mBlastObjects;

	public CircleCollider2D _BlastRadius;

	public ParticleSystem _ExplodeParticles;

	public float _ExplosionDelay;

	public float _ExplosionSize;

	public float _ExplosionForce;

	public float _ExplosionRadius;

	public void Start()
	{
		_BlastRadius.radius = _ExplosionRadius;
		_BlastRadius.enabled = false;
		mBlastObjects = new List<Rigidbody2D>();
	}

	public bool GetReady()
	{
		return mReady;
	}

	public void FixedUpdate()
	{
	}

	public override void OnTriggerEnter2D(Collider2D other)
	{
		if (other.GetComponent<Rigidbody2D>() != null && !other.GetComponent<Rigidbody2D>().isKinematic && other.gameObject.name != "PfOpenMOStar")
		{
			mBlastObjects.Add(other.GetComponent<Rigidbody2D>());
		}
	}

	public void OnTriggerExit2D(Collider2D other)
	{
		if (mBlastObjects.Contains(other.GetComponent<Rigidbody2D>()))
		{
			mBlastObjects.Remove(other.GetComponent<Rigidbody2D>());
		}
	}

	public override void Enable()
	{
		mBlastObjects = new List<Rigidbody2D>();
		_BlastRadius.enabled = true;
		base.rigidbody2D.gravityScale = gravity;
		base.rigidbody2D.freezeRotation = false;
		base.rigidbody2D.isKinematic = false;
		mReady = true;
	}

	public override void Reset()
	{
		if (mIsBlast)
		{
			mIsBlast = false;
			GetComponent<BoxCollider2D>().enabled = true;
			MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = true;
			}
		}
		mReady = false;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.freezeRotation = true;
		base.rigidbody2D.gravityScale = 0f;
		base.rigidbody2D.isKinematic = true;
		_BlastRadius.enabled = false;
		StopAllCoroutines();
		for (int i = 0; i < mBlastObjects.Count; i++)
		{
			mBlastObjects[i].velocity = Vector2.zero;
			mBlastObjects[i].angularVelocity = 0f;
			mBlastObjects[i].Sleep();
		}
	}

	private IEnumerator Explode()
	{
		yield return new WaitForSeconds(_ExplosionDelay);
		for (int i = 0; i < mBlastObjects.Count; i++)
		{
			if (!mBlastObjects[i].isKinematic)
			{
				AddExplosionForce(mBlastObjects[i], _ExplosionForce, base.transform.position, _ExplosionSize);
			}
		}
		_BlastRadius.enabled = false;
		_ExplodeParticles.Play();
		MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
		GetComponent<BoxCollider2D>().enabled = false;
		PlaySound(inForce: true);
	}

	public static void AddExplosionForce(Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius)
	{
		Vector3 vector = body.transform.position - explosionPosition;
		float num = 1f - vector.magnitude / explosionRadius;
		Vector2 force = vector.normalized * explosionForce * num;
		body.velocity = Vector2.zero;
		body.angularVelocity = 0f;
		body.AddForce(force, ForceMode2D.Impulse);
	}

	public void LightFuse()
	{
		if (!mIsBlast && mReady)
		{
			mIsBlast = true;
			StartCoroutine("Explode");
			GoalManager.pInstance.BalloonExloded(base.gameObject);
		}
	}
}
