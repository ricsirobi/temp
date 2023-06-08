using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "TrackInfo", IsNullable = true, Namespace = "")]
public class TrackInfo
{
	[XmlElement(ElementName = "id", IsNullable = true)]
	public int? UserTrackID;

	[XmlElement(ElementName = "utcid")]
	public int UserTrackCategoryID;

	[XmlElement(ElementName = "uid")]
	public Guid UserID;

	[XmlElement(ElementName = "nm")]
	public string Name;

	[XmlElement(ElementName = "iu")]
	public string ImageURL;

	[XmlElement(ElementName = "is")]
	public bool IsShared;

	[XmlElement(ElementName = "sl", IsNullable = true)]
	public int? Slot;

	[XmlElement(ElementName = "cd", IsNullable = true)]
	public DateTime? CreatedDate;

	[XmlElement(ElementName = "md", IsNullable = true)]
	public DateTime? ModifiedDate;

	public const int TRACK_ELEMENT_PIECE = 1;

	public const int TRACK_ELEMENT_DECOR = 2;

	public const int TRACK_ELEMENT_RACE = 3;

	public static Dictionary<int, UserTrackArray> pTrackList = new Dictionary<int, UserTrackArray>();

	[XmlIgnore]
	private GetTrackEventHandler mGetTrackTextureCallback;

	[XmlIgnore]
	private object mGetTrackTextureUserData;

	[XmlIgnore]
	public Texture2D pTexture;

	[XmlIgnore]
	public TrackElement[] TrackElements;

	public string GetDebugString()
	{
		string[] obj = new string[7] { "TrackInfo : Slot = ", null, null, null, null, null, null };
		int? slot = Slot;
		obj[1] = slot.ToString();
		obj[2] = " Name = ";
		obj[3] = Name;
		obj[4] = " TrackID = ";
		slot = UserTrackID;
		obj[5] = slot.ToString();
		obj[6] = "\n";
		string text = string.Concat(obj);
		text = text + "ImgeURL = " + ImageURL + "\n";
		if (pTexture != null)
		{
			text = text + "Texture Name : " + pTexture.name + "  Width = " + pTexture.width + "\n";
		}
		string[] obj2 = new string[8]
		{
			text,
			"Shared = ",
			IsShared.ToString(),
			"  UserID = ",
			null,
			null,
			null,
			null
		};
		Guid userID = UserID;
		obj2[4] = userID.ToString();
		obj2[5] = "  Date = ";
		obj2[6] = CreatedDate.ToString();
		obj2[7] = "\n";
		text = string.Concat(obj2);
		if (TrackElements != null)
		{
			for (int i = 0; i < TrackElements.Length; i++)
			{
				text = text + "-- TrackElement[" + i + "] Type = " + TrackElements[i].ResourceID + "(" + TrackElements[i].PosX + "," + TrackElements[i].PosY + "," + TrackElements[i].PosZ + " ; " + TrackElements[i].RotX + "," + TrackElements[i].RotY + "," + TrackElements[i].RotZ + ")\n";
			}
		}
		return text;
	}

	public static TrackInfo[] GetTracks(int cID)
	{
		if (pTrackList.ContainsKey(cID))
		{
			return pTrackList[cID].mTracks;
		}
		return null;
	}

	public static void Init(int cID)
	{
		if (!pTrackList.ContainsKey(cID))
		{
			UserTrackArray userTrackArray = new UserTrackArray();
			userTrackArray.mUserID = UserInfo.pInstance.UserID;
			GetTrackArrayEventData getTrackArrayEventData = new GetTrackArrayEventData();
			getTrackArrayEventData.mCID = cID;
			getTrackArrayEventData.mCallback = null;
			getTrackArrayEventData.mUserData = null;
			getTrackArrayEventData.mTrackArray = userTrackArray;
			pTrackList.Add(cID, userTrackArray);
			WsWebService.GetTracksByUserID(cID, UserInfo.pInstance.UserID, WsEventHandlerStatic, getTrackArrayEventData);
		}
	}

	public static bool IsReady(int cID)
	{
		if (pTrackList.Count == 0)
		{
			return false;
		}
		if (!pTrackList.ContainsKey(cID))
		{
			return false;
		}
		return pTrackList[cID].pIsReady;
	}

	public static TrackInfo GetTrackInfoBySlotID(int cID, int sID)
	{
		if (pTrackList.Count == 0)
		{
			return null;
		}
		if (!pTrackList.ContainsKey(cID))
		{
			return null;
		}
		if (pTrackList[cID].mTracks == null)
		{
			return null;
		}
		TrackInfo[] mTracks = pTrackList[cID].mTracks;
		foreach (TrackInfo trackInfo in mTracks)
		{
			if (trackInfo.Slot == sID)
			{
				return trackInfo;
			}
		}
		return null;
	}

	public static void LoadTrackTextureBySlotID(int cID, int sID, GetTrackEventHandler callback, object userdata)
	{
		TrackInfo trackInfoBySlotID = GetTrackInfoBySlotID(cID, sID);
		if (trackInfoBySlotID != null)
		{
			trackInfoBySlotID.GetTrackTexture(callback, userdata);
		}
		else
		{
			callback(null, userdata);
		}
	}

	public static void GetTracksByUserID(string uid, int cID, GetTrackArrayEventHandler callback, object userdata)
	{
		GetTrackArrayEventData getTrackArrayEventData = new GetTrackArrayEventData();
		getTrackArrayEventData.mCID = cID;
		getTrackArrayEventData.mCallback = callback;
		getTrackArrayEventData.mUserData = userdata;
		WsWebService.GetTracksByUserID(cID, uid, WsEventHandlerStatic, getTrackArrayEventData);
	}

	public static void GetTracksByTrackIDs(int[] tid, int cID, GetTrackArrayEventHandler callback, object userdata)
	{
		GetTrackArrayEventData getTrackArrayEventData = new GetTrackArrayEventData();
		getTrackArrayEventData.mCID = cID;
		getTrackArrayEventData.mCallback = callback;
		getTrackArrayEventData.mUserData = userdata;
		WsWebService.GetTracksByTrackIDs(tid, WsEventHandlerStatic, getTrackArrayEventData);
	}

	public void WsEventHandlerLocal(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			switch (inType)
			{
			case WsServiceType.GET_TRACK_ELEMENTS:
			{
				TrackElements = (TrackElement[])inObject;
				GetTrackEventData getTrackEventData2 = (GetTrackEventData)inUserData;
				if (getTrackEventData2.mCallback != null)
				{
					getTrackEventData2.mCallback(this, getTrackEventData2.mUserData);
				}
				break;
			}
			case WsServiceType.SET_TRACK:
			{
				TrackInfo trackInfo = (TrackInfo)inObject;
				if (trackInfo != null)
				{
					UtDebug.Log("Set track successful");
					if (!UserTrackID.HasValue || UserTrackID == 0)
					{
						if (pTrackList[trackInfo.UserTrackCategoryID].mTracks == null)
						{
							pTrackList[trackInfo.UserTrackCategoryID].mTracks = new TrackInfo[1];
							pTrackList[trackInfo.UserTrackCategoryID].mTracks[0] = trackInfo;
						}
						else
						{
							int num = pTrackList[trackInfo.UserTrackCategoryID].mTracks.Length + 1;
							Array.Resize(ref pTrackList[trackInfo.UserTrackCategoryID].mTracks, num);
							pTrackList[trackInfo.UserTrackCategoryID].mTracks[num - 1] = trackInfo;
						}
						UtDebug.Log("New track info added " + trackInfo.GetDebugString());
					}
				}
				else
				{
					Debug.LogError("Set track failed");
				}
				SetTrackEventData setTrackEventData2 = (SetTrackEventData)inUserData;
				if (setTrackEventData2.mCallback != null)
				{
					setTrackEventData2.mCallback(trackInfo, setTrackEventData2.mUserData);
				}
				break;
			}
			}
			break;
		case WsServiceEvent.ERROR:
			switch (inType)
			{
			case WsServiceType.GET_TRACK_ELEMENTS:
			{
				GetTrackEventData getTrackEventData = (GetTrackEventData)inUserData;
				if (getTrackEventData.mCallback != null)
				{
					getTrackEventData.mCallback(this, getTrackEventData.mUserData);
				}
				break;
			}
			case WsServiceType.SET_TRACK:
			{
				Debug.LogError("Set track failed");
				SetTrackEventData setTrackEventData = (SetTrackEventData)inUserData;
				if (setTrackEventData.mCallback != null)
				{
					setTrackEventData.mCallback(null, setTrackEventData.mUserData);
				}
				break;
			}
			}
			break;
		}
	}

	public static void WsEventHandlerStatic(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			switch (inType)
			{
			case WsServiceType.REMOVE_USER_WO_TRACK:
			{
				string[] bIDs = (string[])inObject;
				RemoveUserWOTrackEventData removeUserWOTrackEventData = (RemoveUserWOTrackEventData)inUserData;
				if (removeUserWOTrackEventData.mCallback != null)
				{
					removeUserWOTrackEventData.mCallback(bIDs, removeUserWOTrackEventData.mUserData);
				}
				break;
			}
			case WsServiceType.DELETE_TRACK:
				UtDebug.Log("Delete track returns " + (bool)inObject);
				break;
			case WsServiceType.GET_TRACKS_BY_USER:
			{
				GetTrackArrayEventData getTrackArrayEventData4 = (GetTrackArrayEventData)inUserData;
				TrackInfo[] array = (TrackInfo[])inObject;
				if (getTrackArrayEventData4.mTrackArray != null)
				{
					getTrackArrayEventData4.mTrackArray.pIsReady = true;
					getTrackArrayEventData4.mTrackArray.mTracks = array;
				}
				if (getTrackArrayEventData4.mCallback != null)
				{
					getTrackArrayEventData4.mCallback(array, getTrackArrayEventData4.mUserData);
				}
				break;
			}
			case WsServiceType.GET_TRACKS_BY_IDS:
			{
				TrackInfo[] tdata = (TrackInfo[])inObject;
				GetTrackArrayEventData getTrackArrayEventData3 = (GetTrackArrayEventData)inUserData;
				if (getTrackArrayEventData3.mCallback != null)
				{
					getTrackArrayEventData3.mCallback(tdata, getTrackArrayEventData3.mUserData);
				}
				break;
			}
			}
			break;
		case WsServiceEvent.ERROR:
			switch (inType)
			{
			case WsServiceType.DELETE_TRACK:
				Debug.LogError("Delete track failed ");
				break;
			case WsServiceType.GET_TRACKS_BY_USER:
			{
				GetTrackArrayEventData getTrackArrayEventData2 = (GetTrackArrayEventData)inUserData;
				if (getTrackArrayEventData2.mTrackArray != null)
				{
					getTrackArrayEventData2.mTrackArray.pIsReady = true;
				}
				if (getTrackArrayEventData2.mCallback != null)
				{
					getTrackArrayEventData2.mCallback(null, getTrackArrayEventData2.mUserData);
				}
				break;
			}
			case WsServiceType.GET_TRACKS_BY_IDS:
			{
				GetTrackArrayEventData getTrackArrayEventData = (GetTrackArrayEventData)inUserData;
				if (getTrackArrayEventData.mCallback != null)
				{
					getTrackArrayEventData.mCallback(null, getTrackArrayEventData.mUserData);
				}
				break;
			}
			}
			break;
		}
	}

	public void SetName(string tname, bool save, SetTrackEventHandler callback, object userdata)
	{
		SetTrackEventData setTrackEventData = new SetTrackEventData();
		setTrackEventData.mCallback = callback;
		setTrackEventData.mUserData = userdata;
		Name = tname;
		TrackSetRequest trackSetRequest = new TrackSetRequest();
		trackSetRequest.UserTrackID = UserTrackID;
		trackSetRequest.Name = tname;
		if (save)
		{
			WsWebService.SetTrack(trackSetRequest, null, WsEventHandlerLocal, setTrackEventData);
		}
	}

	public void SetShared(bool shared, bool save, SetTrackEventHandler callback, object userdata)
	{
		Debug.LogError("Shared set to " + shared);
		SetTrackEventData setTrackEventData = new SetTrackEventData();
		setTrackEventData.mCallback = callback;
		setTrackEventData.mUserData = userdata;
		IsShared = shared;
		WsWebService.SetTrack(new TrackSetRequest
		{
			UserTrackID = UserTrackID,
			IsShared = IsShared
		}, null, WsEventHandlerLocal, setTrackEventData);
	}

	public void SaveData(SetTrackEventHandler callback, object userdata)
	{
		SetTrackEventData setTrackEventData = new SetTrackEventData();
		setTrackEventData.mCallback = callback;
		setTrackEventData.mUserData = userdata;
		WsWebService.SetTrack(new TrackSetRequest
		{
			UserTrackID = UserTrackID,
			UserTrackCategoryID = UserTrackCategoryID,
			Name = Name,
			UserID = UserID,
			IsShared = IsShared,
			Slot = Slot,
			TrackElements = TrackElements
		}, pTexture, WsEventHandlerLocal, setTrackEventData);
	}

	public void DeleteTrack()
	{
		if (pTrackList[UserTrackCategoryID].mTracks == null)
		{
			Debug.LogError("Track info list is empty");
			return;
		}
		int num = pTrackList[UserTrackCategoryID].mTracks.Length;
		if (num > 0)
		{
			num--;
			TrackInfo[] array = new TrackInfo[num];
			int num2 = 0;
			for (int i = 0; i < num + 1; i++)
			{
				if (this != pTrackList[UserTrackCategoryID].mTracks[i])
				{
					if (num2 == num)
					{
						Debug.LogError("TrackInfo to be deleted is not in the track info list");
						return;
					}
					array[num2] = pTrackList[UserTrackCategoryID].mTracks[i];
					num2++;
				}
			}
			UtDebug.Log("Track info removed from list");
			pTrackList[UserTrackCategoryID].mTracks = array;
			WsWebService.DeleteTrack(UserTrackID.Value, WsEventHandlerStatic, null);
		}
		else
		{
			Debug.LogError("Track info list is empty");
		}
	}

	public void OnTextureResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				pTexture = (Texture2D)inObject;
			}
			else
			{
				pTexture = null;
			}
			if (mGetTrackTextureCallback != null)
			{
				mGetTrackTextureCallback(this, mGetTrackTextureUserData);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			pTexture = null;
			if (mGetTrackTextureCallback != null)
			{
				mGetTrackTextureCallback(this, mGetTrackTextureUserData);
			}
			break;
		case RsResourceLoadEvent.PROGRESS:
			break;
		}
	}

	public void GetTrackTexture(GetTrackEventHandler callback, object userdata)
	{
		if (pTexture != null || string.IsNullOrEmpty(ImageURL))
		{
			callback(this, userdata);
			return;
		}
		mGetTrackTextureCallback = callback;
		mGetTrackTextureUserData = userdata;
		RsResourceManager.Load(ImageURL, OnTextureResLoadingEvent);
	}

	public TrackElement[] GetTrackElements(GetTrackEventHandler callback, object userdata)
	{
		if (TrackElements != null)
		{
			callback(this, userdata);
			return TrackElements;
		}
		GetTrackEventData getTrackEventData = new GetTrackEventData();
		getTrackEventData.mCallback = callback;
		getTrackEventData.mUserData = userdata;
		WsWebService.GetTrackElements(UserTrackID.Value, WsEventHandlerLocal, getTrackEventData);
		return null;
	}

	public static void RemoveUserWithoutTrack(string[] userIDs, RemoveUserWOTrackEventHandler callback, object userdata)
	{
		RemoveUserWOTrackEventData removeUserWOTrackEventData = new RemoveUserWOTrackEventData();
		removeUserWOTrackEventData.mCallback = callback;
		removeUserWOTrackEventData.mUserData = userdata;
		WsWebService.RemoveUserWithoutTrack(userIDs, WsEventHandlerStatic, removeUserWOTrackEventData);
	}
}
