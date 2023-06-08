using System.Collections;
using DG.Tweening;
using SWS;
using UnityEngine;

public class RapidInputDemo : MonoBehaviour
{
	public TextMesh speedDisplay;

	public TextMesh timeDisplay;

	public float topSpeed = 15f;

	public float addSpeed = 2f;

	public float delay = 0.05f;

	public float slowTime = 0.5f;

	public float minPitch;

	public float maxPitch = 2f;

	private splineMove move;

	private float currentSpeed;

	private float timeCounter;

	private void Start()
	{
		move = GetComponent<splineMove>();
		if (!move)
		{
			Debug.LogWarning(base.gameObject.name + " missing movement script!");
			return;
		}
		move.speed = 0.01f;
		move.StartMove();
		move.Pause();
		move.speed = 0f;
	}

	private void Update()
	{
		if (move.tween == null || !move.tween.IsActive() || move.tween.IsComplete())
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (!move.tween.IsPlaying())
			{
				move.Resume();
			}
			float num = currentSpeed + addSpeed;
			if (num >= topSpeed)
			{
				num = topSpeed;
			}
			move.ChangeSpeed(num);
			StopAllCoroutines();
			StartCoroutine("SlowDown");
		}
		speedDisplay.text = "YOUR SPEED: " + Mathf.Round(move.speed * 100f) / 100f;
		timeCounter += Time.deltaTime;
		timeDisplay.text = "YOUR TIME: " + Mathf.Round(timeCounter * 100f) / 100f;
	}

	private IEnumerator SlowDown()
	{
		yield return new WaitForSeconds(delay);
		float t = 0f;
		float rate = 1f / slowTime;
		float speed = move.speed;
		while (t < 1f)
		{
			t += Time.deltaTime * rate;
			currentSpeed = Mathf.Lerp(speed, 0f, t);
			move.ChangeSpeed(currentSpeed);
			float num = maxPitch - minPitch;
			float to = minPitch + move.speed / topSpeed * num;
			if ((bool)GetComponent<AudioSource>())
			{
				GetComponent<AudioSource>().pitch = Mathf.SmoothStep(GetComponent<AudioSource>().pitch, to, 0.2f);
			}
			yield return null;
		}
	}
}
