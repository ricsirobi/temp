using UnityEngine;

public class ElementNadderSpike : KAMonoBase
{
	public delegate void ActionDelegate();

	public float _DestroyAfter = 1.5f;

	public static event ActionDelegate OnActionComplete;

	private void Start()
	{
	}

	private void StartAnim()
	{
		Debug.Log("anim started");
	}

	private void DoActionComplete()
	{
		if (ElementNadderSpike.OnActionComplete != null)
		{
			ElementNadderSpike.OnActionComplete();
		}
		base.transform.parent.gameObject.SetActive(value: false);
		base.animation.Stop();
	}

	private void AnimCompleted()
	{
		base.transform.parent.gameObject.SetActive(value: false);
		base.animation.Stop();
	}
}
