using UnityEngine;

public class PlayerControl : KAMonoBase
{
	[HideInInspector]
	public bool facingRight = true;

	[HideInInspector]
	public bool jump;

	[HideInInspector]
	public bool grounded;

	public float moveForce = 365f;

	public float maxSpeed = 5f;

	public AudioClip[] jumpClips;

	public float jumpForce = 1000f;

	private Transform groundCheck;

	private Animator anim;

	private void Awake()
	{
		groundCheck = base.transform.Find("groundCheck");
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		grounded = Physics2D.Linecast(base.transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
		if (Input.GetButtonDown("Jump") && grounded)
		{
			jump = true;
		}
	}

	private void FixedUpdate()
	{
		float axis = Input.GetAxis("Horizontal");
		anim.SetFloat("Speed", Mathf.Abs(axis));
		if (axis * base.rigidbody2D.velocity.x < maxSpeed)
		{
			base.rigidbody2D.AddForce(Vector2.right * axis * moveForce);
		}
		if (Mathf.Abs(base.rigidbody2D.velocity.x) > maxSpeed)
		{
			base.rigidbody2D.velocity = new Vector2(Mathf.Sign(base.rigidbody2D.velocity.x) * maxSpeed, base.rigidbody2D.velocity.y);
		}
		if (axis > 0f && !facingRight)
		{
			Flip();
		}
		else if (axis < 0f && facingRight)
		{
			Flip();
		}
		if (jump)
		{
			anim.SetTrigger("Jump");
			int num = Random.Range(0, jumpClips.Length);
			AudioSource.PlayClipAtPoint(jumpClips[num], base.transform.position);
			base.rigidbody2D.AddForce(new Vector2(0f, jumpForce));
			jump = false;
		}
	}

	public void Flip()
	{
		facingRight = !facingRight;
		Vector3 localScale = base.transform.localScale;
		localScale.x *= -1f;
		base.transform.localScale = localScale;
	}
}
