using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeControl : KAMonoBase
{
	public float climbUpInterval = 0.05f;

	public float climbDownInterval = 0.04f;

	public float swingForce = 10f;

	public float delayBeforeSecondHang = 0.4f;

	private static Transform collidedChain;

	private static List<Transform> chains;

	private Transform playerTransform;

	private int chainIndex;

	private Collider2D[] colliders;

	private PlayerControl pControl;

	private Animator anim;

	private bool onRope;

	private float timer;

	private float direction;

	private void Start()
	{
		playerTransform = base.transform;
		colliders = GetComponentsInChildren<Collider2D>();
		pControl = GetComponent<PlayerControl>();
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		if (!onRope)
		{
			return;
		}
		playerTransform.position = collidedChain.position;
		playerTransform.localRotation = Quaternion.AngleAxis(direction * 90f, Vector3.forward);
		if (Input.GetAxisRaw("Vertical") > 0f && chainIndex > 1)
		{
			timer += Time.deltaTime;
			if (timer > climbUpInterval)
			{
				ClimbUp();
				timer = 0f;
			}
		}
		if (Input.GetAxisRaw("Vertical") < 0f)
		{
			if (chainIndex < chains.Count - 1)
			{
				timer += Time.deltaTime;
				if (timer > climbUpInterval)
				{
					ClimbDown();
					timer = 0f;
				}
			}
			else
			{
				StartCoroutine(JumpOff());
			}
		}
		if (Input.GetButtonDown("Jump"))
		{
			StartCoroutine(JumpOff());
			pControl.jump = true;
		}
		float axis = Input.GetAxis("Horizontal");
		if (axis > 0f && !pControl.facingRight)
		{
			pControl.Flip();
		}
		else if (axis < 0f && pControl.facingRight)
		{
			pControl.Flip();
		}
		collidedChain.GetComponent<Rigidbody2D>().AddForce(Vector2.right * axis * swingForce);
	}

	private void ClimbDown()
	{
		HingeJoint2D component = chains[chainIndex + 1].GetComponent<HingeJoint2D>();
		if ((bool)component && !component.enabled)
		{
			StartCoroutine(JumpOff());
			return;
		}
		collidedChain = chains[chainIndex + 1];
		playerTransform.parent = collidedChain;
		chainIndex++;
	}

	private void ClimbUp()
	{
		HingeJoint2D component = chains[chainIndex - 1].GetComponent<HingeJoint2D>();
		if (!component || component.enabled)
		{
			collidedChain = chains[chainIndex - 1];
			playerTransform.parent = collidedChain;
			chainIndex--;
		}
	}

	private IEnumerator JumpOff()
	{
		base.rigidbody2D.velocity = Vector2.zero;
		playerTransform.parent = null;
		onRope = false;
		pControl.enabled = true;
		yield return new WaitForSeconds(delayBeforeSecondHang);
		Collider2D[] array = colliders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = true;
		}
	}

	private IEnumerator OnCollisionEnter2D(Collision2D coll)
	{
		if (!pControl.grounded && coll.gameObject.tag == "rope2D")
		{
			HingeJoint2D component = coll.gameObject.GetComponent<HingeJoint2D>();
			if ((bool)component && component.enabled)
			{
				pControl.enabled = false;
				anim.SetFloat("Speed", 0f);
				Collider2D[] array = colliders;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = false;
				}
				Transform parent = coll.transform.parent;
				chains = new List<Transform>();
				foreach (Transform item in parent)
				{
					chains.Add(item);
				}
				collidedChain = coll.transform;
				chainIndex = chains.IndexOf(collidedChain);
				playerTransform.parent = collidedChain;
				onRope = true;
				direction = Mathf.Sign(Vector3.Dot(-collidedChain.right, Vector3.up));
			}
		}
		return null;
	}
}
