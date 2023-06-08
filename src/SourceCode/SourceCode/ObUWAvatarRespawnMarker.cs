using System.Collections;
using UnityEngine;

public class ObUWAvatarRespawnMarker : KAMonoBase
{
	public UWBreathZoneCSM _BreathZone;

	private void Start()
	{
		CoCommonLevel.WaitListCompleted += OnWaitListCompleted;
	}

	private void OnTriggerEnter(Collider c)
	{
		if (AvAvatar.IsCurrentPlayer(c.gameObject) && _BreathZone != null)
		{
			StartCoroutine(UpdateBreathZone());
		}
	}

	private IEnumerator UpdateBreathZone()
	{
		yield return new WaitForEndOfFrame();
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component.pUWSwimZone != null)
		{
			component.pUWSwimZone.pLastUsedBreathZone = _BreathZone;
		}
	}

	private void OnWaitListCompleted()
	{
		CoCommonLevel.WaitListCompleted -= OnWaitListCompleted;
		StartCoroutine(Disable(2));
	}

	private IEnumerator Disable(int frameDelay)
	{
		for (int i = 0; i < frameDelay; i++)
		{
			yield return null;
		}
		collider.enabled = false;
		base.enabled = false;
	}
}
