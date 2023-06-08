using System.Collections;
using UnityEngine;

public class ObAvoid : MonoBehaviour
{
	private Animation anim;

	private bool isMoving;

	private float timeUntilIdleAnim = 1f;

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private float m_Speed;

	[SerializeField]
	private AnimationClip m_WalkAnim;

	[SerializeField]
	private AnimationClip m_IdleAnim;

	[SerializeField]
	private Transform rotationTransform;

	private void Start()
	{
		anim = GetComponent<Animation>();
		rb = GetComponent<Rigidbody>();
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (anim != null && !anim.IsPlaying(m_WalkAnim.name))
			{
				anim.Play(m_WalkAnim.name);
				isMoving = true;
			}
			MoveAway(other.gameObject);
			TurnAway(other.gameObject);
		}
	}

	private void MoveAway(GameObject other)
	{
		Vector3 vector = new Vector3(base.transform.position.x, 0f, base.transform.position.z) - new Vector3(other.transform.position.x, 0f, other.transform.position.z);
		rb.velocity = vector * m_Speed;
	}

	private void TurnAway(GameObject other)
	{
		Vector3 forward = base.transform.position - other.transform.position;
		rotationTransform.rotation = Quaternion.LookRotation(forward);
	}

	private void OnTriggerExit(Collider other)
	{
		isMoving = false;
		if (other.CompareTag("Player"))
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			anim.Play(m_IdleAnim.name);
			if (anim != null && !anim.IsPlaying(m_IdleAnim.name))
			{
				StartCoroutine(CountdownToIdle());
			}
		}
	}

	private IEnumerator CountdownToIdle()
	{
		while (!isMoving)
		{
			yield return new WaitForSeconds(timeUntilIdleAnim);
			anim.Play(m_IdleAnim.name);
		}
	}
}
