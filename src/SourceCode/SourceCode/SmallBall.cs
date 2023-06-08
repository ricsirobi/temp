using UnityEngine;

public class SmallBall : PhysicsObject
{
	private float friction;

	public AudioClip _EnterPipeSound;

	[Tooltip("If the sqrMagnitude of the velocity is greather than or equal to this the audioSource will be played.")]
	public float _VelocityForSound;

	private int mInPipe;

	private void Start()
	{
		friction = base.collider2D.sharedMaterial.friction;
	}

	protected void Update()
	{
		if (!canMove)
		{
			return;
		}
		if (mInPipe > 0 && base.rigidbody2D.velocity.sqrMagnitude >= _VelocityForSound)
		{
			if (base.audio != null && !base.audio.isPlaying)
			{
				base.audio.loop = true;
				base.audio.Play();
			}
		}
		else if (base.audio != null && base.audio.isPlaying)
		{
			base.audio.Pause();
		}
	}

	public override void OnTriggerEnter2D(Collider2D other)
	{
		if (other.name == "ColliderPipe" && canMove)
		{
			if (_EnterPipeSound != null && mInPipe == 0)
			{
				SnChannel.Play(_EnterPipeSound, "", inForce: true);
			}
			mInPipe++;
			base.collider2D.sharedMaterial.friction = 0f;
		}
		base.OnTriggerEnter2D(other);
	}

	public void OnTriggerExit2D(Collider2D other)
	{
		if (other.name == "ColliderPipe" && canMove)
		{
			mInPipe--;
			base.collider2D.sharedMaterial.friction = friction;
		}
	}

	public override void Reset()
	{
		base.Reset();
		mInPipe = 0;
		if (base.audio != null)
		{
			base.audio.Stop();
		}
	}
}
