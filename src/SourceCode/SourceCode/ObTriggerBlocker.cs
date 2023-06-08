using UnityEngine;

public class ObTriggerBlocker : ObTrigger
{
	public GameObject _BlockEntryPrefab;

	public Transform _EntryMarker;

	public AudioClip _EntryVO;

	public override void OnTriggerEnter(Collider inCollider)
	{
		base.OnTriggerEnter(inCollider);
		if (!mInTrigger)
		{
			return;
		}
		if (_BlockEntryPrefab != null)
		{
			_BlockEntryPrefab.SetActive(value: true);
		}
		if ((!_ForMembersOnly || SubscriptionInfo.pIsMember) && (!_NoRides || !AvAvatar.IsPlayerOnRide()))
		{
			_BlockEntryPrefab.SetActive(value: false);
			if (_EntryVO != null)
			{
				SnChannel.Play(_EntryVO, "VO_Pool", inForce: true);
			}
		}
		else
		{
			_BlockEntryPrefab.SetActive(value: true);
		}
	}

	private void OnTriggerStay(Collider inCollider)
	{
		if (mInTrigger && AvAvatar.IsCurrentPlayer(inCollider.gameObject) && _NoRides && AvAvatar.IsPlayerOnRide())
		{
			AvAvatar.SetPosition(_EntryMarker);
			TeleportFx.PlayAt(_EntryMarker.position);
		}
	}
}
