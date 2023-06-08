using UnityEngine;

public class ObTriggerSeating : ObTrigger
{
	public ObCouch[] _CouchList;

	public GameObject _AvatarCamera;

	public GameObject _SeatCamera;

	public Transform _ExitMarker;

	private ObCouch mCurCouch;

	private ObCouch FindSeat()
	{
		ObCouch[] couchList = _CouchList;
		foreach (ObCouch obCouch in couchList)
		{
			ObCouchAttributes[] couchAttributes = obCouch._CouchAttributes;
			foreach (ObCouchAttributes obCouchAttributes in couchAttributes)
			{
				if (obCouchAttributes.pOccupiedAvatar == null)
				{
					obCouch.OccupyCouch(obCouchAttributes, AvAvatar.pObject);
					return obCouch;
				}
			}
		}
		return null;
	}

	public override void DoTriggerAction(GameObject other)
	{
		mCurCouch = FindSeat();
		if (mCurCouch != null)
		{
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.NOT_ALLOWED);
			_AvatarCamera.SetActive(value: false);
			_SeatCamera.SetActive(value: true);
			AvAvatar.pToolbar.BroadcastMessage("SetVisibility", false);
		}
	}

	public override void Update()
	{
		base.Update();
		if (mCurCouch != null && ObCouch.pCurrentAvatarCouch == -1)
		{
			MainStreetMMOClient.pInstance.SetJoinAllowed(MMOJoinStatus.ALLOWED);
			mCurCouch = null;
			AvAvatar.pToolbar.BroadcastMessage("SetVisibility", true);
			AvAvatar.SetPosition(_ExitMarker);
			_AvatarCamera.SetActive(value: true);
			_SeatCamera.SetActive(value: false);
		}
	}
}
