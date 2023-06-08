using DG.Tweening;
using SWS;
using UnityEngine;

public class PathInputDemo : MonoBehaviour
{
	public float speedMultiplier = 10f;

	public float progress;

	private splineMove move;

	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
		move = GetComponent<splineMove>();
		move.StartMove();
		move.Pause();
		progress = 0f;
	}

	private void Update()
	{
		float num = speedMultiplier / 100f;
		float num2 = move.tween.Duration();
		if (Input.GetKey("right"))
		{
			progress += Time.deltaTime * 10f * num;
			progress = Mathf.Clamp(progress, 0f, num2);
			move.tween.fullPosition = progress;
		}
		if (Input.GetKey("left"))
		{
			progress -= Time.deltaTime * 10f * num;
			progress = Mathf.Clamp(progress, 0f, num2);
			move.tween.fullPosition = progress;
		}
		if ((Input.GetKey("right") || Input.GetKey("left")) && progress != 0f && progress != num2)
		{
			animator.SetFloat("Speed", move.speed);
		}
		else
		{
			animator.SetFloat("Speed", 0f);
		}
	}

	private void LateUpdate()
	{
		if (Input.GetKey("left"))
		{
			Vector3 localEulerAngles = base.transform.localEulerAngles;
			localEulerAngles.y += 180f;
			base.transform.localEulerAngles = localEulerAngles;
		}
	}
}
