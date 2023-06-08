using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Announcement")]
public class Announcement
{
	[XmlElement(ElementName = "AnnouncementID")]
	public int AnnouncementID;

	[XmlElement(ElementName = "Description")]
	public string Description;

	[XmlElement(ElementName = "AnnouncementText")]
	public string AnnouncementText;

	[XmlElement(ElementName = "Type")]
	public AnnouncementType Type;

	[XmlElement(ElementName = "StartDate")]
	public DateTime StartDate;

	[XmlElement(ElementName = "EndDate", IsNullable = true)]
	public DateTime? EndDate;

	[XmlIgnore]
	public int _Frequency;

	private static Dictionary<int, AnnouncementListData> mAnnouncementLists = new Dictionary<int, AnnouncementListData>();

	public static void Init(int objectID, AnnouncementListEventHandler callback, object inUserData, string dataXML = null)
	{
		if (mAnnouncementLists.ContainsKey(objectID))
		{
			callback(mAnnouncementLists[objectID]._List, inUserData);
		}
		else
		{
			mAnnouncementLists[objectID] = new AnnouncementListData();
			if (!string.IsNullOrEmpty(dataXML))
			{
				RsResourceManager.Load(dataXML, XmlLoadEventHandler, RsResourceType.NONE, inDontDestroy: false, inDisableCache: false, inDownloadOnly: false, inIgnoreAssetVersion: false, objectID);
			}
			else
			{
				WsWebService.GetAnnouncements(objectID, AnnouncementListServiceEventHandler, objectID);
			}
		}
		mAnnouncementLists[objectID]._Callback = callback;
		mAnnouncementLists[objectID]._UserData = inUserData;
	}

	public static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				int key = (int)inUserData;
				mAnnouncementLists[key]._List = UtUtilities.DeserializeFromXml<AnnouncementList>((string)inObject);
				if (mAnnouncementLists[key]._Callback != null)
				{
					mAnnouncementLists[key]._Callback(mAnnouncementLists[key]._List, mAnnouncementLists[key]._UserData);
					mAnnouncementLists[key]._Callback = null;
				}
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("WEB SERVICE CALL GetAnnouncements FAILED!!!");
			break;
		}
	}

	public static void AnnouncementListServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inType != WsServiceType.GET_ANNOUNCEMENTS)
		{
			return;
		}
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			int key = (int)inUserData;
			if (inObject == null)
			{
				UtDebug.LogError("No AnnouncementList for mWorldObjectID " + key);
			}
			mAnnouncementLists[key]._List = (AnnouncementList)inObject;
			if (mAnnouncementLists[key]._Callback != null)
			{
				mAnnouncementLists[key]._Callback(mAnnouncementLists[key]._List, mAnnouncementLists[key]._UserData);
				mAnnouncementLists[key]._Callback = null;
			}
			break;
		}
		case WsServiceEvent.ERROR:
			UtDebug.LogError("WEB SERVICE CALL GetAnnouncements FAILED!!!");
			break;
		}
	}
}
