using UnityEngine;

public class PhysicsObject : KAMonoBase
{
	public float gravity;

	public float springBounciness;

	public float inertia;

	public bool _FreezeRotation;

	[Tooltip("On collision if relativeVelocity.sqrMagnitude is greater than this value the sound will play.")]
	public float _MagnitudeForSound = 25f;

	protected bool canMove;

	public SnSound _Sound;

	private void Awake()
	{
		if ((bool)GetComponent<Rigidbody2D>())
		{
			GetComponent<Rigidbody2D>().inertia = inertia;
		}
	}

	protected void PlaySound(bool inForce)
	{
		if (_Sound != null && _Sound._AudioClip != null)
		{
			_Sound.Play(inForce);
		}
	}

	protected void PlaySound()
	{
		if (_Sound != null && _Sound._AudioClip != null)
		{
			_Sound.Play();
		}
	}

	public virtual void OnCollisionEnter2D(Collision2D other)
	{
		if (other == null || GoalManager.pInstance == null)
		{
			Debug.LogError("No collider or instance is null!!!!");
			return;
		}
		if ((bool)GetParent(other.transform))
		{
			GoalManager.pInstance.CollisionEvent(base.gameObject, GetParent(other.transform).gameObject);
		}
		if (other.collider.gameObject.name == "TrampolineTop")
		{
			base.rigidbody2D.AddForce(Vector2.up * base.rigidbody2D.velocity.y * springBounciness);
		}
		if (Mathf.Abs(base.rigidbody2D.velocity.y) < 1.25f)
		{
			base.rigidbody2D.velocity = new Vector2(base.rigidbody2D.velocity.x, 0f);
		}
		if (other.relativeVelocity.sqrMagnitude > 25f)
		{
			PlaySound(inForce: true);
		}
	}

	public virtual void OnTriggerEnter2D(Collider2D other)
	{
		if (other == null || GoalManager.pInstance == null)
		{
			Debug.LogError("No collider or instance is null!!!!");
		}
		else if ((bool)GetParent(other.transform))
		{
			GoalManager.pInstance.TriggerEvent(base.gameObject, GetParent(other.transform).gameObject);
		}
		else if (other.name == "EmptyTarget")
		{
			GoalManager.pInstance.TriggerEvent(base.gameObject, other.gameObject);
		}
	}

	public virtual void Enable()
	{
		canMove = true;
		base.rigidbody2D.gravityScale = gravity;
		base.rigidbody2D.freezeRotation = _FreezeRotation;
	}

	public virtual void Reset()
	{
		canMove = false;
		base.rigidbody2D.gravityScale = 0f;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.freezeRotation = true;
		SnChannel.StopPool("");
	}

	protected ObjectBase GetParent(Transform item)
	{
		while (item != null && item.GetComponent<ObjectBase>() == null)
		{
			item = item.parent;
		}
		if (item == null)
		{
			return null;
		}
		return item.GetComponent<ObjectBase>();
	}
}
