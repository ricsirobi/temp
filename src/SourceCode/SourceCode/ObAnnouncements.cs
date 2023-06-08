using UnityEngine;

public class ObAnnouncements : MonoBehaviour
{
	public int _WorldObjectID = -1;

	protected AnnouncementList mAnnouncementList;

	public virtual void Start()
	{
		if (_WorldObjectID != -1)
		{
			WsWebService.GetAnnouncements(_WorldObjectID, ServiceEventHandler, null);
		}
	}

	public virtual void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType == WsServiceType.GET_ANNOUNCEMENTS)
		{
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mAnnouncementList = (AnnouncementList)inObject;
				break;
			case WsServiceEvent.ERROR:
				Debug.LogError("WEB SERVICE CALL GetAnnouncements FAILED!!!");
				mAnnouncementList = null;
				break;
			}
		}
	}
}
